﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abc.Zebus.Directory;
using Abc.Zebus.Nng.Transport;
using Abc.Zebus.Testing;
using Abc.Zebus.Testing.Extensions;
using Abc.Zebus.Tests;
using Abc.Zebus.Tests.Messages;
using Abc.Zebus.Transport;
using Abc.Zebus.Util;
using Moq;
using NUnit.Framework;

namespace Abc.Zebus.Nng.Tests.Transport
{
    [TestFixture]
    public class NngTransportTests
    {
        private const string _environment = "Test";
        private List<NngTransport> _transports;

        [SetUp]
        public void Setup()
        {
            _transports = new List<NngTransport>();
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var transport in _transports)
            {
                try
                {
                    transport.Stop(true);
                }
                catch (Exception)
                {
                }
            }
        }

        [Test]
        public void should_not_crash_when_stopping_if_it_was_not_started()
        {
            var configurationMock = new Mock<INngTransportConfiguration>();
            configurationMock.SetupGet(x => x.WaitForEndOfStreamAckTimeout).Returns(100.Milliseconds());
            var transport = new NngTransport(configurationMock.Object, new NngSocketOptions());

            Assert.That(transport.Stop, Throws.Nothing);
        }

        [Test]
        public void should_not_filter_received_messages_when_environment_is_not_specified()
        {
            var transport1 = CreateAndStartNngTransport(environment: null);

            var transport2ReceivedMessages = new ConcurrentBag<TransportMessage>();
            var transport2 = CreateAndStartNngTransport(onMessageReceived: transport2ReceivedMessages.Add, environment: "NotTest");
            var transport2Peer = new Peer(transport2.PeerId, transport2.InboundEndPoint);

            var message = new FakeCommand(1).ToTransportMessage();
            transport1.Send(message, new[] { transport2Peer });

            Wait.Until(() => transport2ReceivedMessages.Count >= 1, 2.Seconds());
            transport2ReceivedMessages.Single().Id.ShouldEqual(message.Id);
        }

        [Test]
        public void should_not_let_the_outbound_thread_die_if_a_peer_cannot_be_resolved()
        {
            var senderTransport = CreateAndStartNngTransport(environment: null);

            var receivedMessages = new ConcurrentBag<TransportMessage>();
            var destinationTransport = CreateAndStartNngTransport(onMessageReceived: receivedMessages.Add, environment: "NotTest");
            var destinationPeer = new Peer(destinationTransport.PeerId, destinationTransport.InboundEndPoint);
            var nonExistingPeer = new Peer(new PeerId("Abc.NonExistingPeer.2"), "tcp://non_existing_peer:1234");

            var message = new FakeCommand(1).ToTransportMessage();
            senderTransport.Send(message, new[] { nonExistingPeer });
            senderTransport.Send(message, new[] { destinationPeer });

            Wait.Until(() => receivedMessages.Count >= 1, 2.Seconds(), "The outbound thread was killed and couldn't connect to the next peer");
        }

        [Test]
        public void should_not_dispatch_messages_received_from_wrong_environment()
        {
            var transport1ReceivedMessages = new ConcurrentBag<TransportMessage>();
            var transport1 = CreateAndStartNngTransport(onMessageReceived: transport1ReceivedMessages.Add);

            var transport2ReceivedMessages = new ConcurrentBag<TransportMessage>();
            var transport2 = CreateAndStartNngTransport(onMessageReceived: transport2ReceivedMessages.Add, environment: "NotTest");
            var transport2Peer = new Peer(transport2.PeerId, transport2.InboundEndPoint);

            var message1 = new FakeCommand(1).ToTransportMessage();
            var message2 = new FakeCommand(2).ToTransportMessage();
            transport1.Send(message1, new[] { transport2Peer }); // should not arrive

            Thread.Sleep(500); //:(
            transport2.Configure(transport2Peer.Id, _environment);
            transport1.Send(message2, new[] { transport2Peer }); //should arrive

            Wait.Until(() => transport2ReceivedMessages.Count >= 1, 2.Seconds());
            transport2ReceivedMessages.Single().Id.ShouldEqual(message2.Id);
        }

        [Test]
        public void should_send_messages()
        {
            var transport1ReceivedMessages = new ConcurrentBag<TransportMessage>();
            var transport1 = CreateAndStartNngTransport(onMessageReceived: transport1ReceivedMessages.Add);
            var transport1Peer = new Peer(transport1.PeerId, transport1.InboundEndPoint);

            var transport2ReceivedMessages = new ConcurrentBag<TransportMessage>();
            var transport2 = CreateAndStartNngTransport(onMessageReceived: transport2ReceivedMessages.Add);
            var transport2Peer = new Peer(transport2.PeerId, transport2.InboundEndPoint);

            var message1 = new FakeCommand(1).ToTransportMessage();
            transport1.Send(message1, new[] { transport2Peer });

            Wait.Until(() => transport2ReceivedMessages.Count == 1, 2.Seconds());
            var transport2ReceivedMessage = transport2ReceivedMessages.ExpectedSingle();
            transport2ReceivedMessage.ShouldHaveSamePropertiesAs(message1, "Environment", "WasPersisted");
            transport2ReceivedMessage.Environment.ShouldEqual("Test");
            transport2ReceivedMessage.WasPersisted.ShouldEqual(false);

            var message2 = new FakeCommand(2).ToTransportMessage();
            transport2.Send(message2, new[] { transport1Peer });

            Wait.Until(() => transport1ReceivedMessages.Count == 1, 2.Seconds());
            var transport1ReceivedMessage = transport1ReceivedMessages.ExpectedSingle();
            transport1ReceivedMessage.ShouldHaveSamePropertiesAs(message2, "Environment", "WasPersisted");
            transport1ReceivedMessage.Environment.ShouldEqual("Test");
            transport1ReceivedMessage.WasPersisted.ShouldEqual(false);
        }

        [Test]
        public void should_send_message_to_peer_and_persistence()
        {
            // standard case: the message is forwarded to the persistence through SendContext.PersistencePeer
            // the target peer is up

            var senderTransport = CreateAndStartNngTransport();

            var receiverMessages = new ConcurrentBag<TransportMessage>();
            var receiverTransport = CreateAndStartNngTransport(onMessageReceived: receiverMessages.Add);
            var receiverPeer = new Peer(receiverTransport.PeerId, receiverTransport.InboundEndPoint);

            var persistenceMessages = new ConcurrentBag<TransportMessage>();
            var persistenceTransport = CreateAndStartNngTransport(onMessageReceived: persistenceMessages.Add);
            var persistencePeer = new Peer(persistenceTransport.PeerId, persistenceTransport.InboundEndPoint);

            var message = new FakeCommand(999).ToTransportMessage();
            senderTransport.Send(message, new[] { receiverPeer }, new SendContext { PersistentPeerIds = { receiverPeer.Id }, PersistencePeer = persistencePeer });

            Wait.Until(() => receiverMessages.Count == 1, 2.Seconds());
            var messageFromReceiver = receiverMessages.ExpectedSingle();
            messageFromReceiver.ShouldHaveSamePropertiesAs(message, "Environment", "WasPersisted");
            messageFromReceiver.Environment.ShouldEqual("Test");
            messageFromReceiver.WasPersisted.ShouldEqual(true);

            Wait.Until(() => persistenceMessages.Count == 1, 2.Seconds());
            var messageFromPersistence = persistenceMessages.ExpectedSingle();
            messageFromPersistence.ShouldHaveSamePropertiesAs(message, "Environment", "WasPersisted", "PersistentPeerIds", "IsPersistTransportMessage");
            messageFromPersistence.Environment.ShouldEqual("Test");
            messageFromPersistence.PersistentPeerIds.ShouldBeEquivalentTo(new[] { receiverPeer.Id });
        }

        [Test]
        public void should_send_message_to_persistence()
        {
            // standard case: the message is forwarded to the persistence through SendContext.PersistencePeer
            // the target peer is down

            var senderTransport = CreateAndStartNngTransport();

            var receiverPeerId = new PeerId("Abc.R.0");

            var persistenceMessages = new ConcurrentBag<TransportMessage>();
            var persistenceTransport = CreateAndStartNngTransport(onMessageReceived: persistenceMessages.Add);
            var persistencePeer = new Peer(persistenceTransport.PeerId, persistenceTransport.InboundEndPoint);

            var message = new FakeCommand(999).ToTransportMessage();
            senderTransport.Send(message, Enumerable.Empty<Peer>(), new SendContext { PersistentPeerIds = { receiverPeerId }, PersistencePeer = persistencePeer });

            Wait.Until(() => persistenceMessages.Count == 1, 2.Seconds());
            var messageFromPersistence = persistenceMessages.ExpectedSingle();
            messageFromPersistence.ShouldHaveSamePropertiesAs(message, "Environment", "WasPersisted", "PersistentPeerIds", "IsPersistTransportMessage");
            messageFromPersistence.Environment.ShouldEqual("Test");
            messageFromPersistence.PersistentPeerIds.ShouldBeEquivalentTo(new[] { receiverPeerId });
        }

        [Test]
        public void should_send_persist_transport_message_to_persistence()
        {
            // edge case: the message is directly forwarded to the persistence

            var senderTransport = CreateAndStartNngTransport();

            var receiverPeerId = new PeerId("Abc.Receiver.123");

            var persistenceMessages = new ConcurrentBag<TransportMessage>();
            var persistenceTransport = CreateAndStartNngTransport(onMessageReceived: persistenceMessages.Add);
            var persistencePeer = new Peer(persistenceTransport.PeerId, persistenceTransport.InboundEndPoint);

            var message = new FakeCommand(999).ToTransportMessage().ToPersistTransportMessage(receiverPeerId);
            senderTransport.Send(message, new[] { persistencePeer });

            Wait.Until(() => persistenceMessages.Count == 1, 2.Seconds());
            var messageFromPersistence = persistenceMessages.ExpectedSingle();
            messageFromPersistence.ShouldHaveSamePropertiesAs(message, "Environment", "WasPersisted");
            messageFromPersistence.Environment.ShouldEqual("Test");
            messageFromPersistence.PersistentPeerIds.ShouldBeEquivalentTo(new[] { receiverPeerId });
        }

        [Test]
        public void should_write_WasPersisted_when_requested()
        {
            var sender = CreateAndStartNngTransport();

            var receivedMessages = new ConcurrentBag<TransportMessage>();
            var receiver = CreateAndStartNngTransport(onMessageReceived: receivedMessages.Add);
            var receivingPeer = new Peer(receiver.PeerId, receiver.InboundEndPoint);
            var message = new FakeCommand(1).ToTransportMessage();
            var otherMessage = new FakeCommand(2).ToTransportMessage();

            sender.Send(message, new[] { receivingPeer }, new SendContext { PersistentPeerIds = { receivingPeer.Id } });
            sender.Send(otherMessage, new[] { receivingPeer }, new SendContext());

            Wait.Until(() => receivedMessages.Count >= 2, 2.Seconds());
            receivedMessages.Single(x => x.Id == message.Id).WasPersisted.ShouldEqual(true);
            receivedMessages.Single(x => x.Id == otherMessage.Id).WasPersisted.ShouldEqual(false);
        }

        [Test]
        public void should_send_message_to_both_persisted_and_non_persisted_peers()
        {
            var sender = CreateAndStartNngTransport();
            var receivedMessages = new ConcurrentBag<TransportMessage>();

            var receiver1 = CreateAndStartNngTransport(onMessageReceived: receivedMessages.Add);
            var receivingPeer1 = new Peer(receiver1.PeerId, receiver1.InboundEndPoint);

            var receiver2 = CreateAndStartNngTransport(onMessageReceived: receivedMessages.Add);
            var receivingPeer2 = new Peer(receiver2.PeerId, receiver2.InboundEndPoint);

            var message = new FakeCommand(1).ToTransportMessage();

            sender.Send(message, new[] { receivingPeer1, receivingPeer2 }, new SendContext { PersistentPeerIds = { receivingPeer1.Id } });

            Wait.Until(() => receivedMessages.Count >= 2, 2.Seconds());
            receivedMessages.ShouldContain(x => x.Id == message.Id && x.WasPersisted == true);
            receivedMessages.ShouldContain(x => x.Id == message.Id && x.WasPersisted == false);
        }

        [Test]
        public void should_support_peer_endpoint_modifications()
        {
            var senderTransport = CreateAndStartNngTransport();

            var receivedMessages = new ConcurrentBag<TransportMessage>();
            var receiverTransport = CreateAndStartNngTransport(onMessageReceived: receivedMessages.Add);
            var receiver = new Peer(receiverTransport.PeerId, receiverTransport.InboundEndPoint);

            senderTransport.Send(new FakeCommand(0).ToTransportMessage(), new[] { receiver });
            Wait.Until(() => receivedMessages.Count == 1, 2.Seconds());

            var newEndPoint = "tcp://127.0.0.1:" + TcpUtil.GetRandomUnusedPort();
            receiverTransport.Stop();
            receiverTransport = CreateAndStartNngTransport(newEndPoint, receivedMessages.Add);
            receiver.EndPoint = receiverTransport.InboundEndPoint;

            senderTransport.Send(new FakeCommand(0).ToTransportMessage(), new[] { receiver });
            Wait.Until(() => receivedMessages.Count == 2, 2.Seconds(), "unable to receive message");
        }

        [Test, Repeat(5)] 
        public void should_terminate_Nng_connection_of_a_forgotten_peer_after_some_time()
        {
            var senderTransport = CreateAndStartNngTransport();
            var receiverTransport = CreateAndStartNngTransport();
            var receiverPeer = new Peer(receiverTransport.PeerId, receiverTransport.InboundEndPoint);

            var message = new FakeCommand(1).ToTransportMessage();
            senderTransport.Send(message, new[] { receiverPeer });
            Wait.Until(() => senderTransport.OutboundSocketCount == 1, 2.Seconds());

            senderTransport.OnPeerUpdated(receiverPeer.Id, PeerUpdateAction.Decommissioned);

            Thread.Sleep(100);

            senderTransport.OutboundSocketCount.ShouldEqual(1);

            using (SystemDateTime.Set(utcNow: SystemDateTime.UtcNow.Add(30.Seconds())))
            {
                Wait.Until(() => senderTransport.OutboundSocketCount == 0, 1.Seconds(), "Socket should be disconnected");
            }
        }

        [Test, Repeat(5)]
        public void should_terminate_Nng_connection_of_a_started_peer_with_no_delay()
        {
            var senderTransport = CreateAndStartNngTransport();
            var receiverTransport = CreateAndStartNngTransport();
            var receiverPeer = new Peer(receiverTransport.PeerId, receiverTransport.InboundEndPoint);

            var message = new FakeCommand(1).ToTransportMessage();
            senderTransport.Send(message, new[] { receiverPeer });
            Wait.Until(() => senderTransport.OutboundSocketCount == 1, 2.Seconds());

            senderTransport.OnPeerUpdated(receiverPeer.Id, PeerUpdateAction.Started);

            Wait.Until(() => senderTransport.OutboundSocketCount == 0, 2.Seconds(), "Socket should be disconnected");
        }

        [Test]
        public void should_receive_many_messages()
        {
            var senderTransport = CreateAndStartNngTransport();

            var receviedMessages = new List<TransportMessage>();
            var receiverTransport = CreateAndStartNngTransport(onMessageReceived: receviedMessages.Add);
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

        [Test]
        public void should_not_block_when_hitting_high_water_mark()
        {
            var senderTransport = CreateAndStartNngTransport();
            senderTransport.SocketOptions.SendBufferSize = 3;
            senderTransport.SocketOptions.SendTimeout = 50.Milliseconds();
            senderTransport.SocketOptions.SendRetriesBeforeSwitchingToClosedState = 2;

            var receviedMessages = new List<TransportMessage>();
            var upReceiverTransport = CreateAndStartNngTransport(onMessageReceived: receviedMessages.Add);
            var upReceiver = new Peer(upReceiverTransport.PeerId, upReceiverTransport.InboundEndPoint);

            var downReceiverTransport = CreateAndStartNngTransport();
            var downReceiver = new Peer(downReceiverTransport.PeerId, downReceiverTransport.InboundEndPoint);

            downReceiverTransport.Stop();

            for (var i = 1; i <= 10; ++i)
            {
                var message = new FakeCommand(i).ToTransportMessage();
                senderTransport.Send(message, new[] { upReceiver, downReceiver });

                var expectedMessageCount = i;
                Wait.Until(() => receviedMessages.Count == expectedMessageCount, 2.Seconds(), "Failed to send message after " + i + " successful sent");
            }
        }

        [Test]
        public void should_not_wait_blocked_peers_on_every_send()
        {
            var senderTransport = CreateAndStartNngTransport();
            senderTransport.SocketOptions.SendBufferSize = 1;
            senderTransport.SocketOptions.SendTimeout = 20.Milliseconds();
            senderTransport.SocketOptions.SendRetriesBeforeSwitchingToClosedState = 0;

            var receivedMessages = new List<TransportMessage>();
            var upReceiverTransport = CreateAndStartNngTransport( onMessageReceived: receivedMessages.Add);
            var upReceiver = new Peer(upReceiverTransport.PeerId, upReceiverTransport.InboundEndPoint);

            var downReceiverTransport = CreateAndStartNngTransport();
            var downReceiver = new Peer(downReceiverTransport.PeerId, downReceiverTransport.InboundEndPoint);

            Console.WriteLine("Stopping receiver");

            downReceiverTransport.Stop();

            Console.WriteLine("Receiver stopped");

            for (var i = 1; i <= 10; ++i)
            {
                var senderStopwatch = Stopwatch.StartNew();
                var message = new FakeCommand(i).ToTransportMessage();
                senderTransport.Send(message, new[] { upReceiver, downReceiver });
                Console.WriteLine("Send a message to two peers in " + senderStopwatch.Elapsed);
            }

            var receiverStopwatch = Stopwatch.StartNew();
            Wait.Until(() => receivedMessages.Count == 10, 2.Seconds(), "Timed out while waiting for messages");
            receiverStopwatch.Stop();
            Console.WriteLine("Elapsed time to get messages: " + receiverStopwatch.Elapsed);
            receiverStopwatch.ElapsedMilliseconds.ShouldBeLessOrEqualThan(200, "Throughput is too low");
        }

        [Test]
        public void should_not_wait_for_unknown_peer_on_every_send()
        {
            var receivedMessageCount = 0;
            var senderTransport = CreateAndStartNngTransport();
            var receiverTransport = CreateAndStartNngTransport(onMessageReceived: _ => receivedMessageCount++);
            var receiver = new Peer(receiverTransport.PeerId, receiverTransport.InboundEndPoint);
            var invalidPeer = new Peer(new PeerId("Abc.Testing.Invalid"), "tcp://unknown-bastard:123456");

            for (var i = 0; i < 1000; i++)
            {
                var message = new FakeCommand(i).ToTransportMessage();
                senderTransport.Send(message, new[] { invalidPeer, receiver });
            }

            Wait.Until(() => receivedMessageCount == 1000, 5.Seconds());
        }

        [Test]
        public void should_send_various_sized_messages()
        {
            var senderTransport = CreateAndStartNngTransport();
            senderTransport.SocketOptions.SendBufferSize = 3;

            var receviedMessages = new List<TransportMessage>();
            var receiverTransport = CreateAndStartNngTransport(onMessageReceived: receviedMessages.Add);
            var receiver = new Peer(receiverTransport.PeerId, receiverTransport.InboundEndPoint);

            var messageBytes = new byte[5000];
            new Random().NextBytes(messageBytes);

            var bigMessage = new TransportMessage(new MessageTypeId(typeof(FakeCommand)), TestDataBuilder.CreateStream(messageBytes), new PeerId("X"), senderTransport.InboundEndPoint);
            senderTransport.Send(bigMessage, new[] { receiver });

            Wait.Until(() => receviedMessages.Count == 1, 2.Seconds());

            receviedMessages[0].ShouldHaveSamePropertiesAs(bigMessage, "Environment", "WasPersisted");

            var smallMessage = new TransportMessage(new MessageTypeId(typeof(FakeCommand)), TestDataBuilder.CreateStream(new byte[1]), new PeerId("X"), senderTransport.InboundEndPoint);
            senderTransport.Send(smallMessage, new[] { receiver });

            Wait.Until(() => receviedMessages.Count == 2, 2.Seconds());

            receviedMessages[1].ShouldHaveSamePropertiesAs(smallMessage, "Environment", "WasPersisted");
        }

        [Test]
        public void should_send_message_to_self()
        {
            var receviedMessages = new List<TransportMessage>();
            var transport = CreateAndStartNngTransport(onMessageReceived: receviedMessages.Add);
            var self = new Peer(transport.PeerId, transport.InboundEndPoint);

            transport.Send(new FakeCommand(1).ToTransportMessage(), new[] { self });

            Wait.Until(() => receviedMessages.Count == 1, 2.Seconds());
        }

        [Test]
        public void should_not_forward_messages_to_upper_layer_when_stopping()
        {
            var receivedMessages = new List<TransportMessage>();

            var receivingTransport = CreateAndStartNngTransport(onMessageReceived: receivedMessages.Add);

            var receivingPeer = new Peer(receivingTransport.PeerId, receivingTransport.InboundEndPoint);
            bool receivedWhileNotListening = false;
            receivingTransport.MessageReceived += message => receivedWhileNotListening |= !receivingTransport.IsListening;

            var sendingTransport = CreateAndStartNngTransport();
            var shouldSendMessages = true;

            Task.Factory.StartNew(() =>
            {
                var sendCount = 0;
                var spinWait = new SpinWait();
                // ReSharper disable once AccessToModifiedClosure
                while (shouldSendMessages)
                {
                    sendingTransport.Send(new FakeCommand(0).ToTransportMessage(), new[] { receivingPeer });
                    sendCount++;
                    spinWait.SpinOnce();
                }
                Console.WriteLine($"{sendCount} messages sent");
            });

            Wait.Until(() => receivedMessages.Count > 1, 10.Seconds());
            Console.WriteLine("Message received");

            receivingTransport.Stop();
            Console.WriteLine("Receiving transport stopped");

            receivedWhileNotListening.ShouldBeFalse();
            shouldSendMessages = false;

            sendingTransport.Stop();
            Console.WriteLine("Sending transport stopped");
        }

        [Test]
        public void should_process_all_messages_in_buffer_on_stop()
        {
            var state = new should_process_all_messages_in_buffer_on_stop_state { ShouldSend = true };

            var receivingTransport = CreateAndStartNngTransport(onMessageReceived: x => state.ReceivedMessageCount++);
            var sendingTransport = CreateAndStartNngTransport();
            var receivingPeer = new Peer(sendingTransport.PeerId, receivingTransport.InboundEndPoint);

            var senderTask = new Thread(() =>
            {
                Log($"Send loop started");

                while (state.ShouldSend)
                {
                    sendingTransport.Send(new FakeCommand(state.SentMessageCount++).ToTransportMessage(), new[] { receivingPeer });
                }

                Log($"Send loop terminated, Count: {state.SentMessageCount}");

                sendingTransport.Stop();

                Log($"Sender stopped");
            });

            senderTask.Start();
            Wait.Until(() => state.ReceivedMessageCount != 0, 2.Seconds());

            Log($"Stopping the sender");
            state.ShouldSend = false;
            senderTask.Join();

            Log($"Stopping the receiver");
            receivingTransport.Stop();
            Log($"Receiver stopped");

            Thread.MemoryBarrier();
            if (state.ReceivedMessageCount != state.SentMessageCount)
                Thread.Sleep(1.Second());

            state.ReceivedMessageCount.ShouldEqual(state.SentMessageCount);

            void Log(string text) => Console.WriteLine(DateTime.Now.TimeOfDay + " " + text + Environment.NewLine + Environment.NewLine);
        }

        private class should_process_all_messages_in_buffer_on_stop_state
        {
            public volatile int ReceivedMessageCount;
            public volatile int SentMessageCount;
            public volatile bool ShouldSend;
        }

        [Test]
        public void should_disconnect_peer_socket_of_a_stopped_peer_after_some_time()
        {
            var transport1 = CreateAndStartNngTransport();
            var peer1 = new Peer(transport1.PeerId, transport1.InboundEndPoint);

            var transport2 = CreateAndStartNngTransport();
            var peer2 = new Peer(transport2.PeerId, transport2.InboundEndPoint);

            transport1.Send(new FakeCommand(0).ToTransportMessage(), new[] { peer2 });
            transport2.Send(new FakeCommand(0).ToTransportMessage(), new[] { peer1 });
            Wait.Until(() => transport1.OutboundSocketCount == 1, 10.Seconds());
            Wait.Until(() => transport2.OutboundSocketCount == 1, 10.Seconds());
            
            transport2.Stop();

            Wait.Until(() => transport1.OutboundSocketCount == 0, 10.Seconds());
        }

        private NngTransport CreateAndStartNngTransport(string endPoint = null, Action<TransportMessage> onMessageReceived = null, string peerId = null, string environment = _environment)
        {
            var configurationMock = new Mock<INngTransportConfiguration>();
            configurationMock.SetupGet(x => x.InboundEndPoint).Returns(endPoint);
            configurationMock.SetupGet(x => x.WaitForEndOfStreamAckTimeout).Returns(1.Second());

            if (peerId == null)
                peerId = $"Abc.Testing.{Guid.NewGuid():N}";

            var transport = new NngTransport(configurationMock.Object, new NngSocketOptions());
            transport.SetLogId(_transports.Count);

            transport.SocketOptions.SendTimeout = 500.Milliseconds();
            _transports.Add(transport);

            transport.Configure(new PeerId(peerId), environment);

            if (onMessageReceived != null)
                transport.MessageReceived += onMessageReceived;

            transport.Start();
            return transport;
        }
    }
}