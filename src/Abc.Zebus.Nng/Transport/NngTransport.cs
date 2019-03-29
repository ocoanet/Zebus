using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Abc.Zebus.Directory;
using Abc.Zebus.Serialization.Protobuf;
using Abc.Zebus.Transport;
using Abc.Zebus.Util;
using log4net;

namespace Abc.Zebus.Nng.Transport
{
    public class NngTransport : ITransport
    {
        private readonly INngTransportConfiguration _configuration;
        private ILog _logger = LogManager.GetLogger(typeof(NngTransport));
        private ConcurrentDictionary<PeerId, NngOutboundSocket> _outboundSockets;
        private BlockingCollection<OutboundSocketAction> _outboundSocketActions;
        private BlockingCollection<PendingDisconnect> _pendingDisconnects;
        private Thread _inboundThread;
        private Thread _outboundThread;
        private Thread _disconnectThread;
        private volatile bool _isListening;
        private NngEndPoint _inboundEndPoint;
        private string _environment;
        private CountdownEvent _outboundSocketsToStop;
        private bool _isRunning;

        public NngTransport(INngTransportConfiguration configuration, NngSocketOptions socketOptions)
        {
            _configuration = configuration;
            SocketOptions = socketOptions;
        }

        public event Action<TransportMessage> MessageReceived;

        public bool IsListening => _isListening;

        public string InboundEndPoint => _inboundEndPoint != null ? _inboundEndPoint.ValueForConnect() : _configuration.InboundEndPoint;

        public int PendingSendCount => _outboundSocketActions?.Count ?? 0;

        public NngSocketOptions SocketOptions { get; }

        public int OutboundSocketCount => _outboundSockets.Count;

        public PeerId PeerId { get; private set; }

        internal void SetLogId(int logId)
        {
            _logger = LogManager.GetLogger(typeof(NngTransport).Assembly, typeof(NngTransport).FullName + "#" + logId);
        }

        public void Configure(PeerId peerId, string environment)
        {
            PeerId = peerId;
            _environment = environment;
        }

        public void OnPeerUpdated(PeerId peerId, PeerUpdateAction peerUpdateAction)
        {
            if (peerId == PeerId)
                return;

            if (peerUpdateAction == PeerUpdateAction.Decommissioned && !peerId.IsPersistence())
                Disconnect(peerId);

            // Forgetting a peer when it starts up make sure we don't have a stale socket for it, at the cost of losing the send buffer safety
            if (peerUpdateAction == PeerUpdateAction.Started)
                Disconnect(peerId, delayed: false);
        }

        public void OnRegistered()
        {
        }

        public void Start()
        {
            _logger.InfoFormat("Loaded nng {0}", NngNative.Version());

            _isListening = true;
            _outboundSockets = new ConcurrentDictionary<PeerId, NngOutboundSocket>();
            _outboundSocketActions = new BlockingCollection<OutboundSocketAction>();
            _pendingDisconnects = new BlockingCollection<PendingDisconnect>();

            var startSequenceState = new InboundProcStartSequenceState();

            _inboundThread = BackgroundThread.Start(InboundProc, startSequenceState);
            _outboundThread = BackgroundThread.Start(OutboundProc);
            _disconnectThread = BackgroundThread.Start(DisconnectProc);

            startSequenceState.Wait();
            _isRunning = true;
        }

        public void Stop()
        {
            Stop(false);
        }

        public void Stop(bool discardPendingMessages)
        {
            if (!_isRunning)
                return;

            _pendingDisconnects.CompleteAdding();

            if (discardPendingMessages)
                DiscardItems(_pendingDisconnects);

            if (!_disconnectThread.Join(30.Seconds()))
                _logger.Error("Unable to terminate disconnect thread");

            _outboundSocketActions.CompleteAdding();

            if (discardPendingMessages)
                DiscardItems(_outboundSocketActions);

            if (!_outboundThread.Join(30.Seconds()))
                _logger.Error("Unable to terminate outbound thread");

            _isListening = false;
            if (!_inboundThread.Join(30.Seconds()))
                _logger.Error("Unable to terminate inbound thread");

            _outboundSocketActions.Dispose();
            _outboundSocketActions = null;

            _logger.InfoFormat("{0} Stopped", PeerId);
        }

        private static void DiscardItems<T>(BlockingCollection<T> collection)
        {
            while (collection.TryTake(out _))
            {
            }
        }

        public void Send(TransportMessage message, IEnumerable<Peer> peers)
        {
            Send(message, peers, new SendContext());
        }

        public void Send(TransportMessage message, IEnumerable<Peer> peers, SendContext context)
        {
            _outboundSocketActions.Add(OutboundSocketAction.Send(message, peers, context));
        }

        private void Disconnect(PeerId peerId, bool delayed = true)
        {
            if (_outboundSockets.ContainsKey(peerId))
                _logger.InfoFormat("Queueing disconnect, PeerId: {0}, Delayed: {1}", peerId, delayed);

            if (delayed)
            {
                SafeAdd(_pendingDisconnects, new PendingDisconnect(peerId, SystemDateTime.UtcNow.Add(_configuration.WaitForEndOfStreamAckTimeout)));
            }
            else
            {
                SafeAdd(_outboundSocketActions, OutboundSocketAction.Disconnect(peerId));
            }
        }

        public void AckMessage(TransportMessage transportMessage)
        {
        }

        private void InboundProc(InboundProcStartSequenceState state)
        {
            Thread.CurrentThread.Name = "ZmqTransport.InboundProc";
            _logger.DebugFormat("Starting inbound proc...");

            var inboundSocket = CreateInboundSocket(state);
            if (inboundSocket == null)
                return;

            using (inboundSocket)
            {
                while (_isListening)
                {
                    var inputStream = inboundSocket.Receive();
                    if (inputStream == null)
                        continue;

                    DeserializeAndForwardTransportMessage(inputStream);
                }

                GracefullyDisconnectInboundSocket(inboundSocket);
            }

            _logger.InfoFormat("InboundProc terminated");
        }

        private NngInboundSocket CreateInboundSocket(InboundProcStartSequenceState state)
        {
            NngInboundSocket inboundSocket = null;
            try
            {
                inboundSocket = new NngInboundSocket(PeerId, SocketOptions, _environment);
                _inboundEndPoint = inboundSocket.Bind(_configuration.InboundEndPoint);
                return inboundSocket;
            }
            catch (Exception ex)
            {
                state.SetFailed(ex);
                inboundSocket?.Dispose();

                return null;
            }
            finally
            {
                state.Release();
            }
        }

        private void GracefullyDisconnectInboundSocket(NngInboundSocket inboundSocket)
        {
            inboundSocket.Disconnect();

            CodedInputStream inputStream;
            while ((inputStream = inboundSocket.Receive(100.Milliseconds())) != null)
                DeserializeAndForwardTransportMessage(inputStream);
        }

        private void DeserializeAndForwardTransportMessage(CodedInputStream inputStream)
        {
            try
            {
                var transportMessage = inputStream.ReadTransportMessage();

                if (!IsFromCurrentEnvironment(transportMessage))
                    return;

                if (transportMessage.MessageTypeId == MessageTypeId.EndOfStream)
                {
                    SendEndOfStreamAck(transportMessage);
                    return;
                }

                if (transportMessage.MessageTypeId == MessageTypeId.EndOfStreamAck)
                {
                    OnEndOfStreamAck(transportMessage);
                    return;
                }

                if (_isListening)
                    MessageReceived?.Invoke(transportMessage);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Failed to process inbound transport message: {0}", ex);
            }
        }

        private void OnEndOfStreamAck(TransportMessage transportMessage)
        {
            var senderId = transportMessage.Originator.SenderId;
            var senderEndPoint = transportMessage.Originator.SenderEndPoint;

            if (!_outboundSockets.ContainsKey(senderId))
            {
                _logger.ErrorFormat("Received EndOfStreamAck for an unknown socket ({0}) PeerId: {1} (Known peers: {2})", senderEndPoint, senderId, string.Join(", ", _outboundSockets.Keys));
                return;
            }

            _logger.InfoFormat("Received EndOfStreamAck for {0}, {1}", senderId, senderEndPoint);

            _outboundSocketsToStop.Signal();
        }

        private void SendEndOfStreamAck(TransportMessage transportMessage)
        {
            _logger.InfoFormat("Sending EndOfStreamAck to {0}", transportMessage.Originator.SenderEndPoint);

            var endOfStreamAck = new TransportMessage(MessageTypeId.EndOfStreamAck, new MemoryStream(), PeerId, InboundEndPoint);
            var closingPeer = new Peer(transportMessage.Originator.SenderId, transportMessage.Originator.SenderEndPoint);

            SafeAdd(_outboundSocketActions, OutboundSocketAction.Send(endOfStreamAck, new[] { closingPeer }, new SendContext()));
            SafeAdd(_pendingDisconnects, new PendingDisconnect(closingPeer.Id, SystemDateTime.UtcNow.Add(_configuration.WaitForEndOfStreamAckTimeout)));
        }

        private bool IsFromCurrentEnvironment(TransportMessage transportMessage)
        {
            if (transportMessage.Environment == null)
            {
                _logger.DebugFormat("Receiving message with null environment from  {0}", transportMessage.Originator.SenderId);
            }
            else if (transportMessage.Environment != _environment)
            {
                _logger.ErrorFormat("Receiving messages from wrong environment: {0} from {1}, discarding message type {2}", transportMessage.Environment, transportMessage.Originator.SenderEndPoint, transportMessage.MessageTypeId);
                return false;
            }

            return true;
        }

        private void OutboundProc()
        {
            Thread.CurrentThread.Name = "ZmqTransport.OutboundProc";
            _logger.DebugFormat("Starting outbound proc...");

            var outputStream = new CodedOutputStream();

            foreach (var socketAction in _outboundSocketActions.GetConsumingEnumerable())
            {
                if (socketAction.IsDisconnect)
                {
                    DisconnectPeers(socketAction.Targets.Select(x => x.Id));
                }
                else
                {
                    WriteTransportMessageAndSendToPeers(socketAction.Message, socketAction.Targets, socketAction.Context, outputStream);
                }
            }

            GracefullyDisconnectOutboundSockets(outputStream);

            _logger.InfoFormat("OutboundProc terminated");
        }

        private void WriteTransportMessageAndSendToPeers(TransportMessage transportMessage, List<Peer> peers, SendContext context, CodedOutputStream outputStream)
        {
            outputStream.Reset();
            outputStream.WriteTransportMessage(transportMessage, _environment);

            if (context.PersistencePeer == null && transportMessage.IsPersistTransportMessage)
            {
                outputStream.WritePersistentPeerIds(transportMessage, transportMessage.PersistentPeerIds);
            }

            foreach (var target in peers)
            {
                var isPersistent = context.WasPersisted(target.Id);
                outputStream.SetWasPersisted(isPersistent);

                SendToPeer(transportMessage, outputStream, target);
            }

            if (context.PersistencePeer != null)
            {
                outputStream.WritePersistentPeerIds(transportMessage, context.PersistentPeerIds);

                SendToPeer(transportMessage, outputStream, context.PersistencePeer);
            }
        }

        private void SendToPeer(TransportMessage transportMessage, CodedOutputStream outputStream, Peer target)
        {
            var outboundSocket = GetConnectedOutboundSocket(target, transportMessage);
            if (!outboundSocket.IsConnected)
                return;

            try
            {
                outboundSocket.Send(outputStream.Buffer, outputStream.Position, transportMessage);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send message, PeerId: {target.Id}, EndPoint: {target.EndPoint}, Exception: {ex}");
            }
        }

        private void DisconnectPeers(IEnumerable<PeerId> peerIds)
        {
            foreach (var peerId in peerIds)
            {
                if (!_outboundSockets.TryRemove(peerId, out var outboundSocket))
                    continue;

                outboundSocket.Disconnect();
            }
        }

        private NngOutboundSocket GetConnectedOutboundSocket(Peer peer, TransportMessage transportMessage)
        {
            if (!_outboundSockets.TryGetValue(peer.Id, out var outboundSocket))
            {
                outboundSocket = new NngOutboundSocket(peer.Id, peer.EndPoint, SocketOptions);
                outboundSocket.ConnectFor(transportMessage);

                _outboundSockets.TryAdd(peer.Id, outboundSocket);
            }
            else if (outboundSocket.EndPoint != peer.EndPoint)
            {
                outboundSocket.ReconnectFor(peer.EndPoint, transportMessage);
            }

            return outboundSocket;
        }

        private void GracefullyDisconnectOutboundSockets(CodedOutputStream outputStream)
        {
            var connectedOutboundSockets = _outboundSockets.Values.Where(x => x.IsConnected).ToList();

            _outboundSocketsToStop = new CountdownEvent(connectedOutboundSockets.Count);

            SendEndOfStreamMessages(connectedOutboundSockets, outputStream);

            _logger.InfoFormat("Waiting for {0} outbound socket end of stream acks", _outboundSocketsToStop.InitialCount);
            if (!_outboundSocketsToStop.Wait(_configuration.WaitForEndOfStreamAckTimeout))
                _logger.WarnFormat("{0} peers did not respond to end of stream", _outboundSocketsToStop.CurrentCount);

            DisconnectPeers(connectedOutboundSockets.Select(x => x.PeerId).ToList());
        }

        private void SendEndOfStreamMessages(List<NngOutboundSocket> connectedOutboundSockets, CodedOutputStream outputStream)
        {
            foreach (var outboundSocket in connectedOutboundSockets)
            {
                _logger.InfoFormat("Sending EndOfStream to {0}", outboundSocket.EndPoint);

                var endOfStreamMessage = new TransportMessage(MessageTypeId.EndOfStream, new MemoryStream(), PeerId, InboundEndPoint) { WasPersisted = false };
                outputStream.Reset();
                outputStream.WriteTransportMessage(endOfStreamMessage, _environment);
                outboundSocket.Send(outputStream.Buffer, outputStream.Position, endOfStreamMessage);
            }
        }

        private void DisconnectProc()
        {
            Thread.CurrentThread.Name = "ZmqTransport.DisconnectProc";

            foreach (var pendingDisconnect in _pendingDisconnects.GetConsumingEnumerable())
            {
                while (pendingDisconnect.DisconnectTimeUtc > SystemDateTime.UtcNow)
                {
                    if (_pendingDisconnects.IsAddingCompleted)
                        return;

                    Thread.Sleep(500);
                }

                SafeAdd(_outboundSocketActions, OutboundSocketAction.Disconnect(pendingDisconnect.PeerId));
            }
        }

        private void SafeAdd<T>(BlockingCollection<T> collection, T item)
        {
            try
            {
                collection.Add(item);
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Unable to enqueue item, Type: {0}, Exception: {1}", typeof(T).Name, ex);
            }
        }

        private readonly struct OutboundSocketAction
        {
            private static readonly TransportMessage _disconnectMessage = new TransportMessage(default, null, new PeerId(), null);

            private OutboundSocketAction(TransportMessage message, IEnumerable<Peer> targets, SendContext context)
            {
                Message = message;
                Targets = targets as List<Peer> ?? targets.ToList();
                Context = context;
            }

            public bool IsDisconnect => Message == _disconnectMessage;
            public TransportMessage Message { get; }
            public List<Peer> Targets { get; }
            public SendContext Context { get; }

            public static OutboundSocketAction Send(TransportMessage message, IEnumerable<Peer> peers, SendContext context)
                => new OutboundSocketAction(message, peers, context);

            public static OutboundSocketAction Disconnect(PeerId peerId)
                => new OutboundSocketAction(_disconnectMessage, new List<Peer> { new Peer(peerId, null) }, null);
        }

        private class PendingDisconnect
        {
            public readonly PeerId PeerId;
            public readonly DateTime DisconnectTimeUtc;

            public PendingDisconnect(PeerId peerId, DateTime disconnectTimeUtc)
            {
                PeerId = peerId;
                DisconnectTimeUtc = disconnectTimeUtc;
            }
        }

        private class InboundProcStartSequenceState
        {
            private Exception _inboundProcStartException;
            private readonly ManualResetEvent _inboundProcStartedSignal = new ManualResetEvent(false);

            public void Wait()
            {
                _inboundProcStartedSignal.WaitOne();
                if (_inboundProcStartException != null)
                    throw _inboundProcStartException;
            }

            public void SetFailed(Exception exception)
            {
                _inboundProcStartException = exception;
            }

            public void Release()
            {
                _inboundProcStartedSignal.Set();
            }
        }
    }
}
