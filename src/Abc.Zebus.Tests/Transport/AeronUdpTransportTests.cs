using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Abc.Zebus.Testing;
using Abc.Zebus.Testing.Extensions;
using Abc.Zebus.Tests.Messages;
using Abc.Zebus.Transport;
using Abc.Zebus.Util;
using Moq;
using NUnit.Framework;

namespace Abc.Zebus.Tests.Transport
{
    public class AeronUdpTransportTests
    {
        private const string _environment = "Test";
        private List<AeronUdpTransport> _transports;

        [SetUp]
        public void Setup()
        {
            _transports = new List<AeronUdpTransport>();
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var transport in _transports)
            {
                try
                {
                    if (transport.IsListening)
                        transport.Stop(true);
                }
                catch (Exception)
                {
                }
            }
        }

        [Test]
        public void Should_publish_and_receive_a_message()
        {
            var transport1ReceivedMessages = new ConcurrentBag<TransportMessage>();
            var transport1 = CreateAndStartAeronTransport(onMessageReceived: transport1ReceivedMessages.Add);
            var transport1Peer = new Peer(transport1.PeerId, transport1.InboundEndPoint);

            var transport2ReceivedMessages = new ConcurrentBag<TransportMessage>();
            var transport2 = CreateAndStartAeronTransport(onMessageReceived: transport2ReceivedMessages.Add);
            var transport2Peer = new Peer(transport2.PeerId, transport2.InboundEndPoint);

            var message1 = new FakeCommand(1).ToTransportMessage();
            transport1.Send(message1, new[] { transport2Peer });

            Wait.Until(() => transport2ReceivedMessages.Count == 1, 500.Milliseconds());
            var transport2ReceivedMessage = transport2ReceivedMessages.ExpectedSingle();
            transport2ReceivedMessage.ShouldHaveSamePropertiesAs(message1, "Environment", "WasPersisted");
            transport2ReceivedMessage.Environment.ShouldEqual("Test");
            transport2ReceivedMessage.WasPersisted.ShouldEqual(false);

            var message2 = new FakeCommand(2).ToTransportMessage();
            transport2.Send(message2, new[] { transport1Peer });

            Wait.Until(() => transport1ReceivedMessages.Count == 1, 500.Milliseconds());
            var transport1ReceivedMessage = transport1ReceivedMessages.ExpectedSingle();
            transport1ReceivedMessage.ShouldHaveSamePropertiesAs(message2, "Environment", "WasPersisted");
            transport1ReceivedMessage.Environment.ShouldEqual("Test");
            transport1ReceivedMessage.WasPersisted.ShouldEqual(false);
        }

        [Test]
        public void should_receive_many_messages()
        {
            var senderTransport = CreateAndStartAeronTransport();

            var receviedMessages = new List<TransportMessage>();
            var receiverTransport = CreateAndStartAeronTransport(onMessageReceived: receviedMessages.Add);
            var receiver = new Peer(receiverTransport.PeerId, receiverTransport.InboundEndPoint);

            for (var i = 0; i < 10; ++i)
            {
                var message = new FakeCommand(i).ToTransportMessage();
                senderTransport.Send(message, new[] { receiver });
            }

            Wait.Until(() => receviedMessages.Count == 10, 1.Second());

            for (var i = 0; i < 10; ++i)
            {
                var message = (FakeCommand)receviedMessages[i].ToMessage();
                message.FakeId.ShouldEqual(i);
            }
        }

        private AeronUdpTransport CreateAndStartAeronTransport(Action<TransportMessage> onMessageReceived = null, string peerId = null,
            string environment = _environment)
        {
            var configurationMock = new Mock<IAeronUdpTransportConfiguration>();
            configurationMock.SetupGet(x => x.AeronMediaDriverDirectory).Returns(@"C:\Users\Antoine\AppData\Local\Temp\aeron-Antoine");

            if (peerId == null)
                peerId = "Abc.Testing." + _transports.Count;

            var transport = new AeronUdpTransport(configurationMock.Object);
            _transports.Add(transport);

            transport.Configure(new PeerId(peerId), environment);

            if (onMessageReceived != null)
                transport.MessageReceived += onMessageReceived;

            transport.Start();
            return transport;
        }
    }
}