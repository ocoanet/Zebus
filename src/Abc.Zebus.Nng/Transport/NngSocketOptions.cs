using System;
using Abc.Zebus.Util;

namespace Abc.Zebus.Nng.Transport
{
    public class NngSocketOptions
    {
        public NngSocketOptions()
        {
            SendBufferSize = 20_000;
            SendTimeout = 100.Milliseconds();
            SendRetriesBeforeSwitchingToClosedState = 2;

            ClosedStateDurationAfterSendFailure = 15.Seconds();
            ClosedStateDurationAfterConnectFailure = 2.Minutes();

            ReceiveBufferSize = 40_000;
            ReceiveTimeout = 300.Milliseconds();
        }

        public int SendBufferSize { get; set; }
        public TimeSpan SendTimeout { get; set; }
        public int SendRetriesBeforeSwitchingToClosedState { get; set; }

        public TimeSpan ClosedStateDurationAfterSendFailure { get; set; }
        public TimeSpan ClosedStateDurationAfterConnectFailure { get; set; }

        public int ReceiveBufferSize { get; set; }
        public TimeSpan ReceiveTimeout { get; set; }
    }
}
