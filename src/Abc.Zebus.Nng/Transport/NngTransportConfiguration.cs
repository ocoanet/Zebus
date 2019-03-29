using System;

namespace Abc.Zebus.Nng.Transport
{
    public class NngTransportConfiguration : INngTransportConfiguration
    {
        public string InboundEndPoint { get; set; }
        public TimeSpan WaitForEndOfStreamAckTimeout { get; set; }
    }
}
