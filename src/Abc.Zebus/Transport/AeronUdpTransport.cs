using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Abc.Zebus.Directory;
using Abc.Zebus.Serialization.Protobuf;
using Abc.Zebus.Util;
using Adaptive.Aeron;
using Adaptive.Aeron.LogBuffer;
using Adaptive.Agrona.Concurrent;
using log4net;

namespace Abc.Zebus.Transport
{
    public interface IAeronUdpTransportConfiguration
    {
        string AeronMediaDriverDirectory { get; }
    }

    public class AeronUdpTransport : ITransport
    {
        private ILog _log = LogManager.GetLogger(typeof(AeronUdpTransport));

        // Maximum number of message fragments to receive during a single 'poll' operation
        const int fragmentLimitCount = 1000;
        const int _streamId = 2;

        private readonly IAeronUdpTransportConfiguration _configuration;
        private BlockingCollection<OutboundSocketAction> _outboundSocketActions;
        private ConcurrentDictionary<PeerId, Publication> _publications;

        private string _environment;
        private bool _isRunning;
        private Aeron _aeronClient;
        private Thread _inboundThread;
        private Thread _outboundThread;

        public AeronUdpTransport(IAeronUdpTransportConfiguration configuration)
        {
            _configuration = configuration;
        }

        public PeerId PeerId { get; private set; }

        public string InboundEndPoint { get; private set; }

        public int PendingSendCount { get; }

        public bool IsListening { get; private set; }

        public event Action<TransportMessage> MessageReceived;

        public void OnRegistered()
        {
        }

        public void OnPeerUpdated(PeerId peerId, PeerUpdateAction peerUpdateAction)
        {
            throw new NotImplementedException();
        }

        public void Configure(PeerId peerId, string environment)
        {
            PeerId = peerId;
            _environment = environment;
        }

        public void Start()
        {
            IsListening = true;

            Config.Params[Aeron.Context.AERON_DIR_PROP_NAME] = _configuration.AeronMediaDriverDirectory;
            _aeronClient = Aeron.Connect();
            _publications = new ConcurrentDictionary<PeerId, Publication>();
            _outboundSocketActions = new BlockingCollection<OutboundSocketAction>();

            var waiter = new ManualResetEvent(false);
            _inboundThread = BackgroundThread.Start(InboundProc, waiter, null);
            _outboundThread = BackgroundThread.Start(OutboundProc);

            waiter.WaitOne();
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

            //_pendingDisconnects.CompleteAdding();

            //if (discardPendingMessages)
            //    DiscardItems(_pendingDisconnects);

            //if (!_disconnectThread.Join(30.Seconds()))
            //    _logger.Error("Unable to terminate disconnect thread");

            _outboundSocketActions.CompleteAdding();

            //if (discardPendingMessages)
            //    DiscardItems(_outboundSocketActions);

            if (!_outboundThread.Join(30.Seconds()))
                _log.Error("Unable to terminate outbound thread");

            IsListening = false;
            if (!_inboundThread.Join(30.Seconds()))
                _log.Error("Unable to terminate inbound thread");

            _outboundSocketActions.Dispose();
            _outboundSocketActions = null;

            _aeronClient.Dispose();
            _log.Info($"{PeerId} Stopped");
        }

        public void Send(TransportMessage message, IEnumerable<Peer> peers)
        {
            Send(message, peers, new SendContext());
        }

        public void Send(TransportMessage message, IEnumerable<Peer> peers, SendContext context)
        {
            _outboundSocketActions.Add(OutboundSocketAction.Send(message, peers, context));
        }

        public void AckMessage(TransportMessage transportMessage)
        {
        }

        private void InboundProc(ManualResetEvent state)
        {
            Thread.CurrentThread.Name = "AeronUdpTransport.InboundProc";
            _log.Debug("Starting inbound proc...");

            InboundEndPoint = $"aeron:udp?endpoint={AeronUtil.GetLocalEndpoint()}";
            var assembler = new FragmentAssembler(FragmentHandler);

            using (var subscription = _aeronClient.AddSubscription(InboundEndPoint, _streamId))
            {
                state.Set();
                var idleStrategy = new BusySpinIdleStrategy();

                while (IsListening)
                {
                    // poll delivers messages to the dataHandler as they arrive
                    // and returns number of fragments read, or 0
                    // if no data is available.
                    var fragmentsRead = subscription.Poll(assembler.OnFragment, fragmentLimitCount);
                    // Give the IdleStrategy a chance to spin/yield/sleep to reduce CPU
                    // use if no messages were received.
                    idleStrategy.Idle(fragmentsRead);
                }
            }

            _log.InfoFormat("InboundProc terminated");
        }

        private void FragmentHandler(UnsafeBuffer buffer, int offset, int length, Header header)
        {
            var bytes = new byte[length];
            buffer.GetBytes(offset, bytes);

            var inputStream = new CodedInputStream(bytes, 0, length);
            DeserializeAndForwardTransportMessage(inputStream);
        }

        private void DeserializeAndForwardTransportMessage(CodedInputStream inputStream)
        {
            try
            {
                var transportMessage = inputStream.ReadTransportMessage();

                if (!IsFromCurrentEnvironment(transportMessage))
                    return;

                //if (transportMessage.MessageTypeId == MessageTypeId.EndOfStream)
                //{
                //    SendEndOfStreamAck(transportMessage);
                //    return;
                //}

                //if (transportMessage.MessageTypeId == MessageTypeId.EndOfStreamAck)
                //{
                //    OnEndOfStreamAck(transportMessage);
                //    return;
                //}

                if (IsListening)
                    MessageReceived?.Invoke(transportMessage);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Failed to process inbound transport message: {0}", ex);
            }
        }

        private bool IsFromCurrentEnvironment(TransportMessage transportMessage)
        {
            if (transportMessage.Environment == null)
            {
                _log.DebugFormat("Receiving message with null environment from  {0}", transportMessage.Originator.SenderId);
            }
            else if (transportMessage.Environment != _environment)
            {
                _log.ErrorFormat("Receiving messages from wrong environment: {0} from {1}, discarding message type {2}", transportMessage.Environment, transportMessage.Originator.SenderEndPoint, transportMessage.MessageTypeId);
                return false;
            }
            return true;
        }

        private void OutboundProc()
        {
            Thread.CurrentThread.Name = "AeronUpdTransport.OutboundProc";
            _log.DebugFormat("Starting outbound proc...");

            var outputStream = new CodedOutputStream();
            foreach (var socketAction in _outboundSocketActions.GetConsumingEnumerable())
            {
                if (socketAction.IsDisconnect)
                {
                    //DisconnectPeers(socketAction.Targets.Select(x => x.Id));
                }
                else
                {
                    WriteTransportMessageAndSendToPeers(socketAction.Message, socketAction.Targets, socketAction.Context, outputStream);
                }
            }

            //GracefullyDisconnectOutboundSockets(outputStream);

            _log.InfoFormat("OutboundProc terminated");

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

                SendToPeer(outputStream, target);
            }

            if (context.PersistencePeer != null)
            {
                outputStream.WritePersistentPeerIds(transportMessage, context.PersistentPeerIds);

                SendToPeer(outputStream, context.PersistencePeer);
            }
        }

        private void SendToPeer(CodedOutputStream outputStream, Peer target)
        {
            var publication = GetOrCreatePublicationForPeer(target);

            try
            {
                var result = publication.Offer(new UnsafeBuffer(outputStream.ToArray()));
                if (result >= 0L) return;

                switch (result)
                {
                    case Publication.BACK_PRESSURED:
                        _log.Error(" Offer failed due to back pressure");
                        break;
                    case Publication.NOT_CONNECTED:
                        _log.Error(" Offer failed because publisher is not connected to subscriber");
                        break;
                    case Publication.ADMIN_ACTION:
                        _log.Error("Offer failed because of an administration action in the system");
                        break;
                    case Publication.CLOSED:
                        _log.Error("Offer failed publication is closed");
                        break;
                    default:
                        _log.Error(" Offer failed due to unknown reason");
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to send message, PeerId: {target.Id}, EndPoint: {target.EndPoint}, Exception: {ex}");
            }
        }

        private Publication GetOrCreatePublicationForPeer(Peer target)
        {
            if (!_publications.TryGetValue(target.Id, out var publication))
            {
                publication = _aeronClient.AddPublication(target.EndPoint, _streamId);
                _publications.TryAdd(target.Id, publication);
            }
            else if (publication.Channel != target.EndPoint)
            {
                publication.Dispose();
                publication = _aeronClient.AddPublication(target.EndPoint, _streamId);
                _publications.AddOrUpdate(target.Id, publication, (peerId, existingPublication) => publication);
            }
            else if (publication.IsClosed || !publication.IsConnected)
            {
                publication.Dispose();
                publication = _aeronClient.AddPublication(target.EndPoint, _streamId);
                _publications.AddOrUpdate(target.Id, publication, (peerId, existingPublication) => publication);
            }

            var timeout = DateTime.UtcNow.Add(500.Milliseconds());
            if (publication.IsConnected)
                return publication;

            var spin = new SpinWait();
            while (!publication.IsConnected)
            {
                spin.SpinOnce();
                if(DateTime.UtcNow >= timeout)
                    break;
            }

            return publication;
        }

        private struct OutboundSocketAction
        {
            private static readonly TransportMessage _disconnectMessage = new TransportMessage(default(MessageTypeId), null, new PeerId(), null);

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
            {
                return new OutboundSocketAction(message, peers, context);
            }

            public static OutboundSocketAction Disconnect(PeerId peerId)
            {
                return new OutboundSocketAction(_disconnectMessage, new List<Peer> { new Peer(peerId, null) }, null);
            }
        }
    }

    public static class AeronUtil
    {
        public static string GetLocalEndpoint()
        {
            const int minPort = 40_000;
            const int maxPort = 41_000;
            var random = new Random((int)DateTime.UtcNow.Ticks);
            var port = 0;
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            while (port == 0)
            {
                var alreadyInUse = new HashSet<int>(ipGlobalProperties.GetActiveUdpListeners().Select(x => x.Port));

                var randomPort = random.Next(minPort, maxPort);
                if (!alreadyInUse.Contains(randomPort))
                    port = randomPort;
            }

            return $"{ipGlobalProperties.HostName}:{port}";
        }
    }
}