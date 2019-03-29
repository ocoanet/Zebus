using System;
using System.Threading;
using Abc.Zebus.Nng.Transport;
using Abc.Zebus.Testing;
using Abc.Zebus.Testing.Measurements;
using Abc.Zebus.Tests.Messages;
using Abc.Zebus.Transport;
using Abc.Zebus.Util;
using NUnit.Framework;

namespace Abc.Zebus.Nng.Tests.Transport
{
    [TestFixture]
    [Explicit]
    [Category("ManualOnly")]
    public class NngTransportPerformanceTests
    {
        [Test]
        public void CreateIdleTransport()
        {
            var transport = CreateAndStartNngTransport("Abc.Testing.Sender");

            Thread.Sleep(30000);

            transport.Stop();
        }

        [Test]
        public void MeasureThroughput()
        {
            //const int sendMessageCount = 2_000_000;
            const int sendMessageCount = 200_000;

            var senderTransport = CreateAndStartNngTransport("Abc.Testing.Sender");

            var receivedMessageCount = 0;
            var receiverTransport = CreateAndStartNngTransport("Abc.Testing.Receiver", _ => ++receivedMessageCount);
            var receivers = new[] { new Peer(receiverTransport.PeerId, receiverTransport.InboundEndPoint) };

            var transportMessage = new FakeCommand(42).ToTransportMessage();
            senderTransport.Send(transportMessage, receivers);

            var spinWait = new SpinWait();
            while (receivedMessageCount != 1)
                spinWait.SpinOnce();

            using (Measure.Throughput(sendMessageCount))
            {
                for (var i = 0; i < sendMessageCount; ++i)
                {
                    senderTransport.Send(transportMessage, receivers);
                }

                while (receivedMessageCount != sendMessageCount + 1)
                    spinWait.SpinOnce();
            }

            senderTransport.Stop();
            receiverTransport.Stop();
        }

        private static NngTransport CreateAndStartNngTransport(string peerId, Action<TransportMessage> onMessageReceived = null)
        {
            var configuration = new NngTransportConfiguration();
            var transport = new NngTransport(configuration, new NngSocketOptions());
            transport.Configure(new PeerId(peerId), "Test");
            transport.SocketOptions.SendTimeout = 5.Seconds();
            transport.SocketOptions.SendBufferSize = 100_000;
            transport.SocketOptions.ReceiveBufferSize = 100_000;

            if (onMessageReceived != null)
                transport.MessageReceived += onMessageReceived;

            transport.Start();
            return transport;
        }
    }
}
