using System;
using System.Diagnostics;
using Abc.Zebus.Transport;
using log4net;

namespace Abc.Zebus.Nng.Transport
{
    internal unsafe class NngOutboundSocket
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NngOutboundSocket));

        private readonly Stopwatch _closedStateStopwatch = new Stopwatch();
        private readonly NngSocketOptions _options;
        private NngSocket _socket;
        private int _failedSendCount;
        private bool _isInClosedState;
        private TimeSpan _closedStateDuration;

        public NngOutboundSocket(PeerId peerId, string endPoint, NngSocketOptions options)
        {
            _options = options;
            PeerId = peerId;
            EndPoint = endPoint;
        }

        public PeerId PeerId { get; }
        public bool IsConnected { get; private set; }
        public string EndPoint { get; private set; }

        public void ConnectFor(TransportMessage message)
        {
            if (!CanSendOrConnect(message))
                return;

            try
            {
                _socket = CreateSocket();
                NngNative.nng_dial(_socket, EndPoint, null, 0);

                IsConnected = true;

                _logger.InfoFormat("Socket connected, Peer: {0}, EndPoint: {1}", PeerId, EndPoint);
            }
            catch (Exception ex)
            {
                NngNative.nng_close(_socket);
                _socket = default;
                IsConnected = false;

                _logger.ErrorFormat("Unable to connect socket, Peer: {0}, EndPoint: {1}, Exception: {2}", PeerId, EndPoint, ex);

                SwitchToClosedState(_options.ClosedStateDurationAfterConnectFailure);
            }
        }

        private NngSocket CreateSocket()
        {
            NngSocket socket;
            NngNative.nng_push0_open(&socket);
            NngNative.nng_setopt_int(socket, NngOptions.NNG_OPT_SENDBUF, _options.SendBufferSize);
            NngNative.nng_setopt_ms(socket, NngOptions.NNG_OPT_SENDTIMEO, (int)_options.SendTimeout.TotalMilliseconds);

            return socket;
        }

        public void ReconnectFor(string endPoint, TransportMessage message)
        {
            Disconnect();
            EndPoint = endPoint;
            ConnectFor(message);
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;

            try
            {
                //_socket.SetOption(ZmqSocketOption.LINGER, 0);
                NngNative.nng_close(_socket);

                _logger.InfoFormat("Socket disconnected, Peer: {0}", PeerId);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Unable to disconnect socket, Peer: {0}, Exception: {1}", PeerId, ex);
            }

            IsConnected = false;
        }

        public void Send(byte[] buffer, int length, TransportMessage message)
        {
            if (!CanSendOrConnect(message))
                return;

            var error = _socket.Send(buffer, length);

            if (error == NngError.None)
            {
                _failedSendCount = 0;
                return;
            }

            var hasReachedHighWaterMark = error == NngError.Timedout;
            var errorMessage = hasReachedHighWaterMark ? "High water mark reached" : "Unknown error TODO";

            _logger.ErrorFormat("Unable to send message, destination peer: {0}, MessageTypeId: {1}, MessageId: {2}, Error: {3}", PeerId, message.MessageTypeId, message.Id, errorMessage);

            if (_failedSendCount >= _options.SendRetriesBeforeSwitchingToClosedState)
                SwitchToClosedState(_options.ClosedStateDurationAfterSendFailure);

            ++_failedSendCount;
        }

        private bool CanSendOrConnect(TransportMessage message)
        {
            if (_isInClosedState)
            {
                if (_closedStateStopwatch.Elapsed < _closedStateDuration)
                {
                    _logger.WarnFormat("Send or connect ignored in closed state, Peer: {0}, MessageTypeId: {1}, MessageId: {2}", PeerId, message.MessageTypeId, message.Id);
                    return false;
                }

                SwitchToOpenState();
            }

            return true;
        }

        private void SwitchToClosedState(TimeSpan duration)
        {
            _logger.ErrorFormat("Switching to closed state, Peer: {0}, Duration: {1}", PeerId, duration);

            _closedStateStopwatch.Start();
            _closedStateDuration = duration;
            _isInClosedState = true;
        }

        private void SwitchToOpenState()
        {
            _logger.InfoFormat("Switching back to open state, Peer: {0}", PeerId);

            _isInClosedState = false;
            _closedStateStopwatch.Reset();
        }
    }
}
