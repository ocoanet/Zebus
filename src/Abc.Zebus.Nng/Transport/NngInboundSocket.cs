using System;
using System.Text;
using Abc.Zebus.Serialization.Protobuf;
using log4net;

namespace Abc.Zebus.Nng.Transport
{
    internal unsafe class NngInboundSocket : IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NngInboundSocket));

        private readonly PeerId _peerId;
        private readonly NngSocketOptions _options;
        private readonly string _environment;
        private byte[] _readBuffer = new byte[0];
        private NngSocket _socket;
        private NngEndPoint _listenerEndPoint;
        //private TimeSpan _lastReceiveTimeout;
        private NngListener? _listener;

        public NngInboundSocket(PeerId peerId, NngSocketOptions options, string environment)
        {
            _peerId = peerId;
            _options = options;
            _environment = environment;
        }

        public NngEndPoint Bind(string endpoint)
        {
            _socket = CreateSocket();

            var nngEndPoint = new NngEndPoint(endpoint);

            var listener = new NngListener();
            _socket.Listen(nngEndPoint.ValueForListen(), &listener);

            _listener = listener;

            _listenerEndPoint = new NngEndPoint(listener.GetOptionString(NngOptions.NNG_OPT_URL));
            _logger.InfoFormat("Socket bound, Inbound EndPoint: {0}", _listenerEndPoint.ValueForConnect());

            return _listenerEndPoint;
        }

        public void Dispose()
        {
            NngNative.nng_close(_socket);
        }

        public CodedInputStream Receive(TimeSpan? timeout = null)
        {
            var (error, size) = _socket.ReceiveMsg(ref _readBuffer);

            if (error == NngError.None)
                return new CodedInputStream(_readBuffer, 0, size);

            if (error == NngError.Timedout || size == 0)
                return null;

            //throw ZmqUtil.ThrowLastError("ZMQ Receive error");
            throw new Exception("TODO");
        }

        private NngSocket CreateSocket()
        {
            var socket = NngSocket.Pull();
            socket.SetOptionInt32(NngOptions.NNG_OPT_RECVBUF, _options.ReceiveBufferSize);
            socket.SetOptionTimeSpan(NngOptions.NNG_OPT_RECVTIMEO, _options.ReceiveTimeout);

            //_lastReceiveTimeout = _options.ReceiveTimeout;

            return socket;
        }

        public void Disconnect()
        {
            if (_listener == null)
                return;

            _logger.InfoFormat("Closing listener socket, Inbound Endpoint: {0}", _listenerEndPoint);

            _listener.Value.Close();
            _listener = null;
        }
    }
}
