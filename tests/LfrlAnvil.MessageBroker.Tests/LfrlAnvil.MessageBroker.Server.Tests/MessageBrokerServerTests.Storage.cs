using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.Computable.Expressions.Extensions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerServerTests
{
    public class Storage : TestsBase
    {
        [Fact]
        public async Task StateShouldBePersistedCorrectlyOnServerDisposal()
        {
            using var storage = StorageScope.Create();
            ClientMock client1;
            ClientMock client2;
            ClientMock client3;

            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            await using ( var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path ) ) )
            {
                await server.StartAsync();

                client1 = new ClientMock();
                await client1.EstablishHandshake( server, "c1", isEphemeral: false );

                client2 = new ClientMock();
                await client2.EstablishHandshake( server, "c2", isEphemeral: false );

                client3 = new ClientMock();
                await client3.EstablishHandshake( server, "c3", isEphemeral: true );

                await client1.GetTask( c =>
                {
                    c.SendBindListenerRequest(
                        channelName: "ch1",
                        queueName: "q1",
                        createChannelIfNotExists: true,
                        isEphemeral: false,
                        maxRetries: 5,
                        retryDelay: Duration.FromHours( 1 ),
                        maxRedeliveries: 5,
                        minAckTimeout: Duration.FromMinutes( 1 ),
                        deadLetterCapacityHint: 5,
                        minDeadLetterRetention: Duration.FromHours( 1 ) );

                    c.ReadListenerBoundResponse();

                    c.SendBindListenerRequest(
                        channelName: "ch2",
                        queueName: "q1",
                        createChannelIfNotExists: true,
                        isEphemeral: false,
                        maxRedeliveries: 5,
                        minAckTimeout: Duration.FromMinutes( 1 ) );

                    c.ReadListenerBoundResponse();

                    c.SendBindPublisherRequest(
                        channelName: "ch1",
                        streamName: "st1",
                        isEphemeral: false );

                    c.ReadPublisherBoundResponse();

                    c.SendBindPublisherRequest(
                        channelName: "ch2",
                        streamName: "st1",
                        isEphemeral: false );

                    c.ReadPublisherBoundResponse();

                    c.SendBindPublisherRequest(
                        channelName: "ch3",
                        streamName: "st2",
                        isEphemeral: true );

                    c.ReadPublisherBoundResponse();
                } );

                await client2.GetTask( c =>
                {
                    c.SendBindListenerRequest(
                        channelName: "ch1",
                        queueName: "q1",
                        createChannelIfNotExists: true,
                        isEphemeral: true,
                        maxRetries: 5,
                        retryDelay: Duration.FromHours( 1 ),
                        maxRedeliveries: 5,
                        minAckTimeout: Duration.FromMinutes( 1 ),
                        deadLetterCapacityHint: 5,
                        minDeadLetterRetention: Duration.FromHours( 1 ) );

                    c.ReadListenerBoundResponse();

                    c.SendBindListenerRequest(
                        channelName: "ch2",
                        queueName: "q2",
                        createChannelIfNotExists: true,
                        isEphemeral: false,
                        maxRedeliveries: 5,
                        minAckTimeout: Duration.FromMinutes( 1 ) );

                    c.ReadListenerBoundResponse();

                    c.SendBindPublisherRequest(
                        channelName: "ch1",
                        streamName: "st1",
                        isEphemeral: true );

                    c.ReadPublisherBoundResponse();

                    c.SendBindPublisherRequest(
                        channelName: "ch2",
                        streamName: "st1",
                        isEphemeral: false );

                    c.ReadPublisherBoundResponse();
                } );

                await client3.GetTask( c =>
                {
                    c.SendBindListenerRequest(
                        channelName: "ch3",
                        queueName: "q1",
                        createChannelIfNotExists: true );

                    c.ReadListenerBoundResponse();

                    c.SendBindPublisherRequest(
                        channelName: "ch1",
                        streamName: "st2" );

                    c.ReadPublisherBoundResponse();
                } );

                await client2.GetTask( c =>
                {
                    c.SendPushMessage( channelId: 1, data: [ 0 ], confirm: false );
                    c.ReadMessageNotification( length: 1 );
                    c.SendMessageNotificationNegativeAck( queueId: 1, ackId: 1, streamId: 1, messageId: 0 );
                } );

                await client1.GetTask( c =>
                {
                    c.ReadMessageNotification( length: 1 );
                    c.SendMessageNotificationNegativeAck( queueId: 1, ackId: 1, streamId: 1, messageId: 0 );
                } );

                await client1.GetTask( c =>
                {
                    c.SendPushMessage( channelId: 1, data: [ 1, 2 ], confirm: false );
                    c.ReadMessageNotification( length: 2 );
                    c.SendMessageNotificationNegativeAck( queueId: 1, ackId: 1, streamId: 1, messageId: 1 );
                } );

                await client2.GetTask( c =>
                {
                    c.ReadMessageNotification( length: 2 );
                    c.SendMessageNotificationNegativeAck( queueId: 1, ackId: 1, streamId: 1, messageId: 1 );
                } );

                await client2.GetTask( c =>
                {
                    c.SendPushMessage( channelId: 1, data: [ 3, 4, 5 ], confirm: false );
                    c.ReadMessageNotification( length: 3 );
                    c.SendMessageNotificationNegativeAck( queueId: 1, ackId: 1, streamId: 1, messageId: 2, noRetry: true );
                } );

                await client1.GetTask( c =>
                {
                    c.ReadMessageNotification( length: 3 );
                    c.SendMessageNotificationNegativeAck( queueId: 1, ackId: 1, streamId: 1, messageId: 2, noRetry: true );
                } );

                await client1.GetTask( c =>
                {
                    c.SendPushMessage( channelId: 1, data: [ 6, 7, 8, 9 ], confirm: false );
                    c.ReadMessageNotification( length: 4 );
                    c.SendMessageNotificationNegativeAck( queueId: 1, ackId: 1, streamId: 1, messageId: 3, noRetry: true );
                } );

                await client2.GetTask( c =>
                {
                    c.ReadMessageNotification( length: 4 );
                    c.SendMessageNotificationNegativeAck( queueId: 1, ackId: 1, streamId: 1, messageId: 3, noRetry: true );
                } );

                await client2.GetTask( c =>
                {
                    c.SendPushMessage( channelId: 1, data: [ 10, 11, 12, 13, 14 ], confirm: false );
                    c.ReadMessageNotification( length: 5 );
                } );

                await client1.GetTask( c => c.ReadMessageNotification( length: 5 ) );

                await client2.GetTask( c =>
                {
                    c.SendPushMessage( channelId: 1, data: [ 15, 16, 17, 18, 19, 20 ] );
                    c.ReadMessageAcceptedResponse();
                } );

                await client1.GetTask( c =>
                {
                    c.SendPushMessage( channelId: 1, data: [ 21, 22, 23, 24, 25, 26, 27 ] );
                    c.ReadMessageAcceptedResponse();
                } );
            }

            client1.TryDispose();
            client2.TryDispose();
            client3.TryDispose();

            var serverLogger = new ServerEventLogger();
            var channelLoggers = new[] { new ChannelEventLogger(), new ChannelEventLogger() };
            var streamLogger = new StreamEventLogger();
            var clientLoggers = new[] { new ClientEventLogger(), new ClientEventLogger() };
            var queueLoggers = new[] { new QueueEventLogger(), new QueueEventLogger() };

            var streamOrder = new List<(string Client, string Channel)>();
            var ch2ClientOrder = new List<(string Client, string Queue)>();
            var c1PublisherOrder = new List<string>();
            var c1ListenerOrder = new List<string>();
            var q1Order = new List<string>();

            await using ( var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetLogger( serverLogger.GetLogger() )
                    .SetChannelLoggerFactory( c => channelLoggers[c.Id - 1]
                        .GetLogger(
                            MessageBrokerChannelLogger.Create(
                                listenerBound: e =>
                                {
                                    if ( e.Listener.Channel.Name == "ch2" )
                                        ch2ClientOrder.Add(
                                            ($"[{e.Listener.Client.Id}] '{e.Listener.Client.Name}'",
                                                $"[{e.Listener.QueueBindings.Primary.Queue.Id}] '{e.Listener.QueueBindings.Primary.Queue.Name}'") );
                                } ) ) )
                    .SetStreamLoggerFactory( _ => streamLogger.GetLogger(
                        MessageBrokerStreamLogger.Create(
                            publisherBound: e =>
                            {
                                streamOrder.Add(
                                    (
                                        $"[{e.Publisher.Client.Id}] '{e.Publisher.Client.Name}'",
                                        $"[{e.Publisher.Channel.Id}] '{e.Publisher.Channel.Name}'") );
                            } ) ) )
                    .SetClientLoggerFactory( c => clientLoggers[c.Id - 1]
                        .GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                publisherBound: e =>
                                {
                                    if ( e.Publisher.Client.Name == "c1" )
                                        c1PublisherOrder.Add( $"[{e.Publisher.Channel.Id}] '{e.Publisher.Channel.Name}'" );
                                },
                                listenerBound: e =>
                                {
                                    if ( e.Listener.Client.Name == "c1" )
                                        c1ListenerOrder.Add( $"[{e.Listener.Channel.Id}] '{e.Listener.Channel.Name}'" );
                                } ) ) )
                    .SetQueueLoggerFactory( q => queueLoggers[q.Client.Id - 1]
                        .GetLogger(
                            MessageBrokerQueueLogger.Create(
                                listenerBound: e =>
                                {
                                    if ( e.Listener.Queue.Name == "q1" )
                                        q1Order.Add( $"[{e.Listener.Owner.Channel.Id}] '{e.Listener.Owner.Channel.Name}'" );
                                } ) ) ) ) )
            {
                await server.StartAsync();

                var directory = server.RootStorageDirectoryPath;
                var channel1 = server.Channels.TryGetByName( "ch1" );
                var channel2 = server.Channels.TryGetByName( "ch2" );
                var stream = server.Streams.TryGetByName( "st1" );
                var remoteClient1 = server.Clients.TryGetByName( "c1" );
                var queue1 = remoteClient1?.Queues.TryGetByName( "q1" );
                var remoteClient2 = server.Clients.TryGetByName( "c2" );
                var queue2 = remoteClient2?.Queues.TryGetByName( "q2" );
                var now = TimestampProvider.Shared.GetNow();

                Assertion.All(
                        server.State.TestEquals( MessageBrokerServerState.Running ),
                        server.Channels.GetAll()
                            .Select( c => (c.Id, c.Name, c.State) )
                            .TestSetEqual(
                            [
                                (Id: 1, Name: "ch1", State: MessageBrokerChannelState.Running),
                                (Id: 2, Name: "ch2", State: MessageBrokerChannelState.Running)
                            ] ),
                        server.Streams.GetAll()
                            .Select( s => (s.Id, s.Name, s.State) )
                            .TestSetEqual( [ (Id: 1, Name: "st1", State: MessageBrokerStreamState.Running) ] ),
                        server.Clients.GetAll()
                            .Select( c => (c.Id, c.Name, c.State) )
                            .TestSetEqual(
                            [
                                (Id: 1, Name: "c1", State: MessageBrokerRemoteClientState.Inactive),
                                (Id: 2, Name: "c2", State: MessageBrokerRemoteClientState.Inactive)
                            ] ),
                        channel1.TestNotNull( c => Assertion.All(
                            "channel1",
                            c.Listeners.GetAll().Select( l => l.Client.Id ).TestSetEqual( [ 1 ] ),
                            c.Publishers.GetAll().Select( p => p.Client.Id ).TestSetEqual( [ 1 ] ) ) ),
                        channel2.TestNotNull( c => Assertion.All(
                            "channel2",
                            c.Listeners.GetAll().Select( l => l.Client.Id ).TestSetEqual( [ 1, 2 ] ),
                            c.Publishers.GetAll().Select( p => p.Client.Id ).TestSetEqual( [ 1, 2 ] ) ) ),
                        stream.TestNotNull( s => Assertion.All(
                            "stream",
                            s.Publishers.GetAll()
                                .Select( p => (ClientId: p.Client.Id, ChannelId: p.Channel.Id) )
                                .TestSetEqual(
                                [
                                    (ClientId: 1, ChannelId: 1),
                                    (ClientId: 1, ChannelId: 2),
                                    (ClientId: 2, ChannelId: 2)
                                ] ),
                            s.Messages.Count.TestEquals( 7 ),
                            s.Messages.TryGetByKey( 0, includeData: true )
                                .TestNotNull( m => Assertion.All(
                                    "message0",
                                    m.Id.TestEquals( 0UL ),
                                    m.Data.TestSequence<byte>( [ 0 ] ) ) ),
                            s.Messages.TryGetByKey( 1, includeData: true )
                                .TestNotNull( m => Assertion.All(
                                    "message1",
                                    m.Id.TestEquals( 1UL ),
                                    m.Data.TestSequence<byte>( [ 1, 2 ] ) ) ),
                            s.Messages.TryGetByKey( 2, includeData: true )
                                .TestNotNull( m => Assertion.All(
                                    "message2",
                                    m.Id.TestEquals( 2UL ),
                                    m.Data.TestSequence<byte>( [ 3, 4, 5 ] ) ) ),
                            s.Messages.TryGetByKey( 3, includeData: true )
                                .TestNotNull( m => Assertion.All(
                                    "message3",
                                    m.Id.TestEquals( 3UL ),
                                    m.Data.TestSequence<byte>( [ 6, 7, 8, 9 ] ) ) ),
                            s.Messages.TryGetByKey( 4, includeData: true )
                                .TestNotNull( m => Assertion.All(
                                    "message4",
                                    m.Id.TestEquals( 4UL ),
                                    m.Data.TestSequence<byte>( [ 10, 11, 12, 13, 14 ] ) ) ),
                            s.Messages.TryGetByKey( 5, includeData: true )
                                .TestNotNull( m => Assertion.All(
                                    "message5",
                                    m.Id.TestEquals( 5UL ),
                                    m.Data.TestSequence<byte>( [ 15, 16, 17, 18, 19, 20 ] ) ) ),
                            s.Messages.TryGetByKey( 6, includeData: true )
                                .TestNotNull( m => Assertion.All(
                                    "message6",
                                    m.Id.TestEquals( 6UL ),
                                    m.Data.TestSequence<byte>( [ 21, 22, 23, 24, 25, 26, 27 ] ) ) ) ) ),
                        remoteClient1.TestNotNull( c => Assertion.All(
                            "remoteClient1",
                            c.Queues.GetAll()
                                .Select( q => (q.Id, q.Name, q.State) )
                                .TestSetEqual( [ (Id: 1, Name: "q1", State: MessageBrokerQueueState.Inactive) ] ),
                            c.Listeners.GetAll()
                                .Select( l => (ChannelId: l.Channel.Id, QueueId: l.QueueBindings.Primary.Queue.Id, l.State) )
                                .TestSetEqual(
                                [
                                    (ChannelId: 1, QueueId: 1, State: MessageBrokerChannelListenerBindingState.Inactive),
                                    (ChannelId: 2, QueueId: 1, State: MessageBrokerChannelListenerBindingState.Inactive)
                                ] ),
                            c.Publishers.GetAll()
                                .Select( l => (ChannelId: l.Channel.Id, StreamId: l.Stream.Id, l.State) )
                                .TestSetEqual(
                                [
                                    (ChannelId: 1, StreamId: 1, State: MessageBrokerChannelPublisherBindingState.Inactive),
                                    (ChannelId: 2, StreamId: 1, State: MessageBrokerChannelPublisherBindingState.Inactive)
                                ] ) ) ),
                        queue1.TestNotNull( q => Assertion.All(
                            "queue1",
                            q.Listeners.GetAll().Select( l => l.Owner.Channel.Id ).TestSetEqual( [ 1, 2 ] ),
                            q.Messages.DeadLetter.Count.TestEquals( 2 ),
                            q.Messages.DeadLetter.TryPeekAt( 0 )
                                .TestNotNull( m => Assertion.All(
                                    "deadLetter0",
                                    m.StoreKey.TestEquals( 2 ),
                                    m.Retry.TestEquals( 0 ),
                                    m.Redelivery.TestEquals( 0 ),
                                    m.ExpiresAt.TestGreaterThan( now ),
                                    m.Listener.Owner.Channel.Id.TestEquals( 1 ),
                                    m.Publisher.ClientId.TestEquals( 2 ) ) ),
                            q.Messages.DeadLetter.TryPeekAt( 1 )
                                .TestNotNull( m => Assertion.All(
                                    "deadLetter1",
                                    m.StoreKey.TestEquals( 3 ),
                                    m.Retry.TestEquals( 0 ),
                                    m.Redelivery.TestEquals( 0 ),
                                    m.ExpiresAt.TestGreaterThan( now ),
                                    m.Listener.Owner.Channel.Id.TestEquals( 1 ),
                                    m.Publisher.ClientId.TestEquals( 1 ) ) ),
                            q.Messages.Retries.Count.TestEquals( 2 ),
                            q.Messages.Retries.TryGetNext()
                                .TestNotNull( m => Assertion.All(
                                    "retries0",
                                    m.StoreKey.TestEquals( 0 ),
                                    m.Retry.TestEquals( 1 ),
                                    m.Redelivery.TestEquals( 0 ),
                                    m.SendAt.TestGreaterThan( now ),
                                    m.Listener.Owner.Channel.Id.TestEquals( 1 ),
                                    m.Publisher.ClientId.TestEquals( 2 ) ) ),
                            q.Messages.Unacked.Count.TestEquals( 1 ),
                            q.Messages.Unacked.TryGetByAckId( 1 )
                                .TestNotNull( m => Assertion.All(
                                    "unacked1",
                                    m.StoreKey.TestEquals( 4 ),
                                    m.Retry.TestEquals( 0 ),
                                    m.Redelivery.TestEquals( 0 ),
                                    m.MessageId.TestEquals( 4UL ),
                                    m.ExpiresAt.TestLessThanOrEqualTo( now ),
                                    m.Listener.Owner.Channel.Id.TestEquals( 1 ),
                                    m.Publisher.ClientId.TestEquals( 2 ) ) ),
                            q.Messages.Pending.Count.TestEquals( 2 ),
                            q.Messages.Pending.TryPeekAt( 0 )
                                .TestNotNull( m => Assertion.All(
                                    "pending0",
                                    m.StoreKey.TestEquals( 5 ),
                                    m.Listener.Owner.Channel.Id.TestEquals( 1 ),
                                    m.Publisher.ClientId.TestEquals( 2 ) ) ),
                            q.Messages.Pending.TryPeekAt( 1 )
                                .TestNotNull( m => Assertion.All(
                                    "pending1",
                                    m.StoreKey.TestEquals( 6 ),
                                    m.Listener.Owner.Channel.Id.TestEquals( 1 ),
                                    m.Publisher.ClientId.TestEquals( 1 ) ) ) ) ),
                        queue2.TestNotNull( q => Assertion.All(
                            "queue2",
                            q.Listeners.GetAll().Select( l => l.Owner.Channel.Id ).TestSetEqual( [ 2 ] ),
                            q.Messages.DeadLetter.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Pending.Count.TestEquals( 0 ) ) ),
                        remoteClient2.TestNotNull( c => Assertion.All(
                            "remoteClient2",
                            c.Queues.GetAll()
                                .Select( q => (q.Id, q.Name, q.State) )
                                .TestSetEqual( [ (Id: 2, Name: "q2", State: MessageBrokerQueueState.Inactive) ] ),
                            c.Listeners.GetAll()
                                .Select( l => (ChannelId: l.Channel.Id, QueueId: l.QueueBindings.Primary.Queue.Id, l.State) )
                                .TestSetEqual( [ (ChannelId: 2, QueueId: 2, State: MessageBrokerChannelListenerBindingState.Inactive) ] ),
                            c.Publishers.GetAll()
                                .Select( l => (ChannelId: l.Channel.Id, StreamId: l.Stream.Id, l.State) )
                                .TestSetEqual(
                                    [ (ChannelId: 2, StreamId: 1, State: MessageBrokerChannelPublisherBindingState.Inactive) ] ) ) ),
                        serverLogger.GetAll()
                            .TestSequence(
                            [
                                (t, _) => t.Logs.TestSequence(
                                [
                                    $"[Trace:Start] Server = {originalEndPoint}, TraceId = 5 (start)",
                                    $"[StorageLoading] Server = {originalEndPoint}, TraceId = 5, Directory = '{directory}'",
                                    $"[StorageLoaded] Server = {originalEndPoint}, TraceId = 5, Directory = '{directory}', ChannelCount = 2, StreamCount = 1, ClientCount = 2, QueueCount = 2, PublisherCount = 3, ListenerCount = 3",
                                    $"[ListenerStarting] Server = {originalEndPoint}, TraceId = 5, HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = [0.001 second(s), 2147483.647 second(s)], AcceptablePingInterval = [0.001 second(s), 86400 second(s)]",
                                    $"[ListenerStarted] Server = {server.LocalEndPoint}, TraceId = 5",
                                    $"[Trace:Start] Server = {server.LocalEndPoint}, TraceId = 5 (end)"
                                ] )
                            ] ),
                        channelLoggers[0]
                            .GetAll()
                            .TestSequence(
                            [
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:Recreated] Channel = [1] 'ch1', TraceId = 6 (start)",
                                    $"[ServerTrace] Channel = [1] 'ch1', TraceId = 6, Correlation = (Server = {originalEndPoint}, TraceId = 5)",
                                    "[Trace:Recreated] Channel = [1] 'ch1', TraceId = 6 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindPublisher] Channel = [1] 'ch1', TraceId = 7 (start)",
                                    "[ClientTrace] Channel = [1] 'ch1', TraceId = 7, Correlation = (Client = [1] 'c1', TraceId = 19)",
                                    "[PublisherBound] Channel = [1] 'ch1', TraceId = 7, Client = [1] 'c1', Stream = [1] 'st1'",
                                    "[Trace:BindPublisher] Channel = [1] 'ch1', TraceId = 7 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindListener] Channel = [1] 'ch1', TraceId = 8 (start)",
                                    "[ClientTrace] Channel = [1] 'ch1', TraceId = 8, Correlation = (Client = [1] 'c1', TraceId = 19)",
                                    "[ListenerBound] Channel = [1] 'ch1', TraceId = 8, Client = [1] 'c1', Queue = [1] 'q1'",
                                    "[Trace:BindListener] Channel = [1] 'ch1', TraceId = 8 (end)"
                                ] )
                            ] ),
                        channelLoggers[1]
                            .GetAll()
                            .TestSequence(
                            [
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:Recreated] Channel = [2] 'ch2', TraceId = 5 (start)",
                                    $"[ServerTrace] Channel = [2] 'ch2', TraceId = 5, Correlation = (Server = {originalEndPoint}, TraceId = 5)",
                                    "[Trace:Recreated] Channel = [2] 'ch2', TraceId = 5 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindPublisher] Channel = [2] 'ch2', TraceId = 6 (start)",
                                    $"[ClientTrace] Channel = [2] 'ch2', TraceId = 6, Correlation = (Client = {ch2ClientOrder[0].Client}, TraceId = 19)",
                                    $"[PublisherBound] Channel = [2] 'ch2', TraceId = 6, Client = {ch2ClientOrder[0].Client}, Stream = [1] 'st1'",
                                    "[Trace:BindPublisher] Channel = [2] 'ch2', TraceId = 6 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindListener] Channel = [2] 'ch2', TraceId = 7 (start)",
                                    $"[ClientTrace] Channel = [2] 'ch2', TraceId = 7, Correlation = (Client = {ch2ClientOrder[0].Client}, TraceId = 19)",
                                    $"[ListenerBound] Channel = [2] 'ch2', TraceId = 7, Client = {ch2ClientOrder[0].Client}, Queue = {ch2ClientOrder[0].Queue}",
                                    "[Trace:BindListener] Channel = [2] 'ch2', TraceId = 7 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindPublisher] Channel = [2] 'ch2', TraceId = 8 (start)",
                                    $"[ClientTrace] Channel = [2] 'ch2', TraceId = 8, Correlation = (Client = {ch2ClientOrder[1].Client}, TraceId = 19)",
                                    $"[PublisherBound] Channel = [2] 'ch2', TraceId = 8, Client = {ch2ClientOrder[1].Client}, Stream = [1] 'st1'",
                                    "[Trace:BindPublisher] Channel = [2] 'ch2', TraceId = 8 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindListener] Channel = [2] 'ch2', TraceId = 9 (start)",
                                    $"[ClientTrace] Channel = [2] 'ch2', TraceId = 9, Correlation = (Client = {ch2ClientOrder[1].Client}, TraceId = 19)",
                                    $"[ListenerBound] Channel = [2] 'ch2', TraceId = 9, Client = {ch2ClientOrder[1].Client}, Queue = {ch2ClientOrder[1].Queue}",
                                    "[Trace:BindListener] Channel = [2] 'ch2', TraceId = 9 (end)"
                                ] )
                            ] ),
                        streamLogger
                            .GetAll()
                            .TestSequence(
                            [
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:Recreated] Stream = [1] 'st1', TraceId = 19 (start)",
                                    $"[ServerTrace] Stream = [1] 'st1', TraceId = 19, Correlation = (Server = {originalEndPoint}, TraceId = 5)",
                                    "[Trace:Recreated] Stream = [1] 'st1', TraceId = 19 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindPublisher] Stream = [1] 'st1', TraceId = 20 (start)",
                                    $"[ClientTrace] Stream = [1] 'st1', TraceId = 20, Correlation = (Client = {streamOrder[0].Client}, TraceId = 19)",
                                    $"[PublisherBound] Stream = [1] 'st1', TraceId = 20, Client = {streamOrder[0].Client}, Channel = {streamOrder[0].Channel}",
                                    "[Trace:BindPublisher] Stream = [1] 'st1', TraceId = 20 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindPublisher] Stream = [1] 'st1', TraceId = 21 (start)",
                                    $"[ClientTrace] Stream = [1] 'st1', TraceId = 21, Correlation = (Client = {streamOrder[1].Client}, TraceId = 19)",
                                    $"[PublisherBound] Stream = [1] 'st1', TraceId = 21, Client = {streamOrder[1].Client}, Channel = {streamOrder[1].Channel}",
                                    "[Trace:BindPublisher] Stream = [1] 'st1', TraceId = 21 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindPublisher] Stream = [1] 'st1', TraceId = 22 (start)",
                                    $"[ClientTrace] Stream = [1] 'st1', TraceId = 22, Correlation = (Client = {streamOrder[2].Client}, TraceId = 19)",
                                    $"[PublisherBound] Stream = [1] 'st1', TraceId = 22, Client = {streamOrder[2].Client}, Channel = {streamOrder[2].Channel}",
                                    "[Trace:BindPublisher] Stream = [1] 'st1', TraceId = 22 (end)"
                                ] )
                            ] ),
                        clientLoggers[0]
                            .GetAll()
                            .TestSequence(
                            [
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:Recreated] Client = [1] 'c1', TraceId = 19 (start)",
                                    $"[ServerTrace] Client = [1] 'c1', TraceId = 19, Correlation = (Server = {originalEndPoint}, TraceId = 5)",
                                    $"[PublisherBound] Client = [1] 'c1', TraceId = 19, Channel = {c1PublisherOrder[0]}, Stream = [1] 'st1'",
                                    $"[PublisherBound] Client = [1] 'c1', TraceId = 19, Channel = {c1PublisherOrder[1]}, Stream = [1] 'st1'",
                                    $"[ListenerBound] Client = [1] 'c1', TraceId = 19, Channel = {c1ListenerOrder[0]}, Queue = [1] 'q1'",
                                    $"[ListenerBound] Client = [1] 'c1', TraceId = 19, Channel = {c1ListenerOrder[1]}, Queue = [1] 'q1'",
                                    "[Trace:Recreated] Client = [1] 'c1', TraceId = 19 (end)"
                                ] )
                            ] ),
                        clientLoggers[1]
                            .GetAll()
                            .TestSequence(
                            [
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:Recreated] Client = [2] 'c2', TraceId = 19 (start)",
                                    $"[ServerTrace] Client = [2] 'c2', TraceId = 19, Correlation = (Server = {originalEndPoint}, TraceId = 5)",
                                    "[PublisherBound] Client = [2] 'c2', TraceId = 19, Channel = [2] 'ch2', Stream = [1] 'st1'",
                                    "[ListenerBound] Client = [2] 'c2', TraceId = 19, Channel = [2] 'ch2', Queue = [2] 'q2'",
                                    "[Trace:Recreated] Client = [2] 'c2', TraceId = 19 (end)"
                                ] )
                            ] ),
                        queueLoggers[0]
                            .GetAll()
                            .TestSequence(
                            [
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:Recreated] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 19 (start)",
                                    "[ClientTrace] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 19, ClientTraceId = 19",
                                    "[Trace:Recreated] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 19 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindListener] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 20 (start)",
                                    "[ClientTrace] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 20, ClientTraceId = 19",
                                    $"[ListenerBound] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 20, Channel = {q1Order[0]}",
                                    "[Trace:BindListener] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 20 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindListener] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 21 (start)",
                                    "[ClientTrace] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 21, ClientTraceId = 19",
                                    $"[ListenerBound] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 21, Channel = {q1Order[1]}",
                                    "[Trace:BindListener] Client = [1] 'c1', Queue = [1] 'q1', TraceId = 21 (end)"
                                ] )
                            ] ),
                        queueLoggers[1]
                            .GetAll()
                            .TestSequence(
                            [
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:Recreated] Client = [2] 'c2', Queue = [2] 'q2', TraceId = 2 (start)",
                                    "[ClientTrace] Client = [2] 'c2', Queue = [2] 'q2', TraceId = 2, ClientTraceId = 19",
                                    "[Trace:Recreated] Client = [2] 'c2', Queue = [2] 'q2', TraceId = 2 (end)"
                                ] ),
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:BindListener] Client = [2] 'c2', Queue = [2] 'q2', TraceId = 3 (start)",
                                    "[ClientTrace] Client = [2] 'c2', Queue = [2] 'q2', TraceId = 3, ClientTraceId = 19",
                                    "[ListenerBound] Client = [2] 'c2', Queue = [2] 'q2', TraceId = 3, Channel = [2] 'ch2'",
                                    "[Trace:BindListener] Client = [2] 'c2', Queue = [2] 'q2', TraceId = 3 (end)"
                                ] )
                            ] ) )
                    .Go();
            }
        }

        [Fact]
        public async Task StatePersistenceShouldTakeIntoAccountEphemeralPublishersAndMessageRoutingAndLackOfListeners()
        {
            using var storage = StorageScope.Create();

            var endSource = new SafeTaskCompletionSource( completionCount: 5 );
            var disposalSource = new SafeTaskCompletionSource();
            ClientMock client1;
            ClientMock client2;

            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            await using ( var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( c => c.Id == 1
                        ? MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                    endSource.Complete();
                            } )
                        : null )
                    .SetQueueLoggerFactory( q => q.Client.Id == 1
                        ? MessageBrokerQueueLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                    endSource.Complete();
                            } )
                        : null )
                    .SetStreamLoggerFactory( _ => MessageBrokerStreamLogger.Create(
                        traceStart: e =>
                        {
                            if ( e.Type == MessageBrokerStreamTraceEventType.Dispose )
                                disposalSource.Complete();
                            else if ( e.Type == MessageBrokerStreamTraceEventType.PushMessage )
                                endSource.Complete();
                        },
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerStreamTraceEventType.ProcessMessage )
                                disposalSource.Task.Wait();
                        } ) ) ) )
            {
                await server.StartAsync();

                client1 = new ClientMock();
                await client1.EstablishHandshake( server, "c1", isEphemeral: false );

                client2 = new ClientMock();
                await client2.EstablishHandshake( server, new string( 'e', 200 ), isEphemeral: true );

                await client1.GetTask( c =>
                {
                    c.SendBindPublisherRequest( "foo", streamName: "str" );
                    c.ReadPublisherBoundResponse();

                    c.SendBindListenerRequest(
                        "foo",
                        true,
                        maxRedeliveries: 5,
                        minAckTimeout: Duration.FromHours( 1 ),
                        isEphemeral: false );

                    c.ReadListenerBoundResponse();

                    c.SendPushMessage( 1, [ 1 ] );
                    c.ReadAny(
                        (MessageBrokerClientEndpoint.MessageAcceptedResponse, Protocol.MessageAcceptedResponse.Payload),
                        (MessageBrokerClientEndpoint.MessageNotification, Protocol.MessageNotificationHeader.Payload + 1) );

                    c.ReadAny(
                        (MessageBrokerClientEndpoint.MessageAcceptedResponse, Protocol.MessageAcceptedResponse.Payload),
                        (MessageBrokerClientEndpoint.MessageNotification, Protocol.MessageNotificationHeader.Payload + 1) );
                } );

                await client2.GetTask( c =>
                {
                    c.SendBindPublisherRequest( "foo", streamName: "str" );
                    c.SendBindPublisherRequest( "bar", streamName: "str" );
                    c.ReadPublisherBoundResponse();
                    c.ReadPublisherBoundResponse();

                    c.SendPushMessageRouting( [ Routing.FromId( 1 ) ] );
                    c.SendPushMessage( 1, [ 1, 2, 3 ] );
                    c.ReadMessageAcceptedResponse();

                    c.SendPushMessageRouting( [ Routing.FromId( 1 ) ] );
                    c.SendPushMessage( 2, [ 4, 5, 6, 7 ] );
                    c.ReadMessageAcceptedResponse();
                } );

                await endSource.Task;
            }

            client1.TryDispose();
            client2.TryDispose();

            await using ( var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path ) ) )
            {
                await server.StartAsync();

                var stream = server.Streams.TryGetById( 1 );
                var channel = server.Channels.TryGetById( 1 );
                var remoteClient1 = server.Clients.TryGetById( 1 );
                var remoteClient2 = server.Clients.TryGetById( 2 );

                Assertion.All(
                        remoteClient1.TestNotNull(),
                        remoteClient2.TestNull(),
                        stream.TestNotNull( s => Assertion.All(
                            "stream",
                            s.Name.TestEquals( "str" ),
                            s.Messages.Count.TestEquals( 2 ),
                            s.Messages.TryGetByKey( 0, includeData: true )
                                .TestNotNull( m => Assertion.All(
                                    "message0",
                                    m.Id.TestEquals( 0UL ),
                                    m.Publisher.Client.TestRefEquals( remoteClient1 ),
                                    m.Publisher.TestType()
                                        .Exact<MessageBrokerChannelPublisherBinding>( b =>
                                            b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                                    m.Data.TestSequence<byte>( [ 1 ] ) ) ),
                            s.Messages.TryGetByKey( 1, includeData: true )
                                .TestNotNull( m => Assertion.All(
                                    "message1",
                                    m.Id.TestEquals( 1UL ),
                                    m.Publisher.Client.TestNull(),
                                    m.Publisher.Channel.TestRefEquals( channel ),
                                    m.Publisher.Stream.TestRefEquals( stream ),
                                    m.Publisher.ClientId.TestEquals( 2 ),
                                    m.Publisher.ClientName.TestEquals( new string( 'e', 200 ) ),
                                    m.Publisher.IsClientEphemeral.TestTrue(),
                                    m.Publisher.ToString()
                                        .TestEquals(
                                            $"[2] '{new string( 'e', 200 )}' => [1] 'foo' ephemeral publisher binding (using [1] 'str' stream) (Disposed)" ),
                                    m.Data.TestSequence<byte>( [ 1, 2, 3 ] ) ) ) ) ) )
                    .Go();
            }
        }

        [Fact]
        public async Task StatePersistenceShouldPersistDisconnectedClient()
        {
            using var storage = StorageScope.Create();

            var endSource = new SafeTaskCompletionSource( completionCount: 3 );
            var disposalSource = new SafeTaskCompletionSource();
            var deactivated = Atomic.Create( false );

            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            await using ( var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.PushMessage
                                or MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                endSource.Complete();
                            else if ( e.Type == MessageBrokerRemoteClientTraceEventType.Deactivate && ! deactivated.Value )
                            {
                                deactivated.Value = true;
                                disposalSource.Complete();
                            }
                        } ) )
                    .SetQueueLoggerFactory( _ => MessageBrokerQueueLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                endSource.Complete();
                        } ) ) ) )
            {
                await server.StartAsync();

                using ( var client = new ClientMock() )
                {
                    await client.EstablishHandshake( server, "c1", isEphemeral: false );

                    await client.GetTask( c =>
                    {
                        c.SendBindPublisherRequest( "bar", streamName: "str", isEphemeral: true );
                        c.SendBindPublisherRequest( "foo", streamName: "str", isEphemeral: false );
                        c.ReadPublisherBoundResponse();
                        c.ReadPublisherBoundResponse();

                        c.SendBindListenerRequest( "bar", true, queueName: "q", isEphemeral: true );
                        c.SendBindListenerRequest( "qux", true, isEphemeral: true );
                        c.SendBindListenerRequest(
                            "foo",
                            true,
                            queueName: "q",
                            maxRedeliveries: 5,
                            minAckTimeout: Duration.FromHours( 1 ),
                            isEphemeral: false );

                        c.ReadListenerBoundResponse();
                        c.ReadListenerBoundResponse();
                        c.ReadListenerBoundResponse();

                        c.SendPushMessage( 2, [ 1 ], confirm: false );
                        c.ReadMessageNotification( 1 );
                    } );

                    await endSource.Task;
                }

                await disposalSource.Task;

                var remoteClient = server.Clients.TryGetById( 1 );
                var listener = remoteClient?.Listeners.TryGetByChannelId( 2 );
                var publisher = remoteClient?.Publishers.TryGetByChannelId( 2 );
                var channel = server.Channels.TryGetById( 2 );
                var stream = server.Streams.TryGetById( 1 );

                Assertion.All(
                        server.Channels.Count.TestEquals( 1 ),
                        server.Streams.Count.TestEquals( 1 ),
                        channel.TestNotNull( c => Assertion.All(
                            "channel",
                            c.State.TestEquals( MessageBrokerChannelState.Running ),
                            c.Listeners.Count.TestEquals( 1 ),
                            c.Publishers.Count.TestEquals( 1 ),
                            c.Listeners.TryGetByClientId( 1 ).TestRefEquals( listener ),
                            c.Publishers.TryGetByClientId( 1 ).TestRefEquals( publisher ) ) ),
                        stream.TestNotNull( s => Assertion.All(
                            "stream",
                            s.State.TestEquals( MessageBrokerStreamState.Running ),
                            s.Publishers.Count.TestEquals( 1 ),
                            s.Publishers.TryGetByKey( 1, 2 ).TestRefEquals( publisher ) ) ),
                        remoteClient.TestNotNull( c => Assertion.All(
                            "client",
                            c.State.TestEquals( MessageBrokerRemoteClientState.Inactive ),
                            c.Listeners.Count.TestEquals( 1 ),
                            c.Publishers.Count.TestEquals( 1 ),
                            c.Queues.Count.TestEquals( 1 ),
                            c.Queues.TryGetById( 1 )
                                .TestNotNull( q => Assertion.All(
                                    "queue",
                                    q.State.TestEquals( MessageBrokerQueueState.Inactive ),
                                    q.Listeners.Count.TestEquals( 1 ),
                                    q.Listeners.TryGetByChannelId( 2 ).TestRefEquals( listener?.QueueBindings.Primary ),
                                    q.Messages.Unacked.Count.TestEquals( 1 ) ) ) ) ) )
                    .Go();
            }

            await using ( var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path ) ) )
            {
                await server.StartAsync();

                var stream = server.Streams.TryGetById( 1 );
                var channel = server.Channels.TryGetById( 2 );
                var remoteClient = server.Clients.TryGetById( 1 );

                Assertion.All(
                        remoteClient.TestNotNull( c => Assertion.All(
                            "client",
                            c.Name.TestEquals( "c1" ),
                            c.Publishers.Count.TestEquals( 1 ),
                            c.Listeners.Count.TestEquals( 1 ),
                            c.Queues.TryGetById( 1 ).TestNotNull( q => q.Messages.Unacked.Count.TestEquals( 1 ) ) ) ),
                        stream.TestNotNull( s => s.Name.TestEquals( "str" ) ),
                        channel.TestNotNull( c => c.Name.TestEquals( "foo" ) ) )
                    .Go();
            }
        }

        [Fact]
        public async Task StatePersistenceShouldRecreateDisposedPublisherWhenOneExistsButForDifferentStream()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                nextPendingNodeId: NullableIndex.Create( 0 ),
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ) ] );

            storage.WriteStreamMetadata( streamId: 2, streamName: "bar" );
            storage.WriteStreamMessages( streamId: 2, messages: [ ] );
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 2 );
            storage.WriteListenerMetadata(
                clientId: 1,
                channelId: 1,
                queueId: 1,
                maxRedeliveries: 5,
                minAckTimeout: Duration.FromHours( 1 ) );

            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetRootStoragePath( storage.Path ) );

            await server.StartAsync();

            var stream1 = server.Streams.TryGetById( 1 );
            var stream2 = server.Streams.TryGetById( 2 );
            var publisher = server.Clients.TryGetById( 1 )?.Publishers.TryGetByChannelId( 1 );

            Assertion.All(
                    stream2.TestNotNull( s => s.Publishers.GetAll().TestSequence( [ (e, _) => e.TestRefEquals( publisher ) ] ) ),
                    stream1.TestNotNull( s =>
                        s.Messages.TryGetByKey( 0 )
                            .TestNotNull( m => Assertion.All(
                                "message0",
                                m.Publisher.Client.TestRefEquals( publisher?.Client ),
                                m.Publisher.TestNotRefEquals( publisher ) ) ) ) )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldNotDeleteUnreferencedObjects_WhenServerIsDisposedBeforeStorageLoadingIsFinished()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );

            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1, header: [ 1, 2, 3, 4 ] );
            storage.WriteListenerMetadata(
                clientId: 1,
                channelId: 1,
                queueId: 1,
                maxRedeliveries: 5,
                minAckTimeout: Duration.FromHours( 1 ),
                maxRetries: 5,
                retryDelay: Duration.FromHours( 1 ),
                deadLetterCapacityHint: 5,
                minDeadLetterRetention: Duration.FromHours( 1 ) );

            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 3 ) ] );

            storage.WriteQueueUnackedMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueUnackedMessage( streamId: 1, storeKey: 2, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueRetryMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueRetryMessage( streamId: 1, storeKey: 1, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueDeadLetterMessage( streamId: 1, storeKey: 0, retry: 0, redelivery: 0 ) ] );

            var originalEndPoint = new IPEndPoint( IPAddress.Any, 0 );
            await using ( var failedServer = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) ) )
                await failedServer.StartAsync();

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );

            await using ( var failedServer = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) ) )
                await failedServer.StartAsync();

            storage.WriteStreamMessages(
                streamId: 1,
                messages:
                [
                    StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ),
                    StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] ),
                    StorageScope.PrepareStreamMessage( id: 2, storeKey: 2, senderId: 1, channelId: 1, data: [ 4, 5, 6 ] ),
                    StorageScope.PrepareStreamMessage( id: 3, storeKey: 3, senderId: 1, channelId: 1, data: [ 7, 8, 9, 10 ] )
                ] );

            await using ( var failedServer = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) ) )
                await failedServer.StartAsync();

            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            await server.StartAsync();

            Assertion.All(
                    server.Channels.Count.TestEquals( 1 ),
                    server.Channels.TryGetById( 1 )
                        .TestNotNull( c => Assertion.All(
                            "channel",
                            c.Publishers.Count.TestEquals( 1 ),
                            c.Listeners.Count.TestEquals( 1 ) ) ),
                    server.Streams.Count.TestEquals( 1 ),
                    server.Streams.TryGetById( 1 )
                        .TestNotNull( s => Assertion.All(
                            "stream",
                            s.Publishers.Count.TestEquals( 1 ),
                            s.Messages.Count.TestEquals( 4 ),
                            s.Messages.TryGetByKey( 0, includeData: true ).TestNotNull( m => m.Data.TestSequence<byte>( [ 1 ] ) ),
                            s.Messages.TryGetByKey( 1, includeData: true ).TestNotNull( m => m.Data.TestSequence<byte>( [ 2, 3 ] ) ),
                            s.Messages.TryGetByKey( 2, includeData: true ).TestNotNull( m => m.Data.TestSequence<byte>( [ 4, 5, 6 ] ) ),
                            s.Messages.TryGetByKey( 3, includeData: true )
                                .TestNotNull( m => m.Data.TestSequence<byte>( [ 7, 8, 9, 10 ] ) ) ) ),
                    server.Clients.Count.TestEquals( 1 ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c => Assertion.All(
                            "client",
                            c.Publishers.Count.TestEquals( 1 ),
                            c.Listeners.Count.TestEquals( 1 ),
                            c.Queues.Count.TestEquals( 1 ),
                            c.Queues.TryGetById( 1 )
                                .TestNotNull( q => Assertion.All(
                                    "queue",
                                    q.Messages.Pending.Count.TestEquals( 1 ),
                                    q.Messages.Unacked.Count.TestEquals( 1 ),
                                    q.Messages.Retries.Count.TestEquals( 1 ),
                                    q.Messages.DeadLetter.Count.TestEquals( 1 ),
                                    q.Messages.Pending.TryPeekAt( 0 ).TestNotNull( m => m.StoreKey.TestEquals( 3 ) ),
                                    q.Messages.Unacked.TryGetFirst().TestNotNull( m => m.StoreKey.TestEquals( 2 ) ),
                                    q.Messages.Retries.TryGetNext().TestNotNull( m => m.StoreKey.TestEquals( 1 ) ),
                                    q.Messages.DeadLetter.TryPeekAt( 0 ).TestNotNull( m => m.StoreKey.TestEquals( 0 ) ) ) ) ) ) )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldNotDeleteUnreferencedObjects_WhenServerIsManuallyDisposedBeforeStorageLoadingIsFinished()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages:
                [
                    StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ),
                    StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] ),
                    StorageScope.PrepareStreamMessage( id: 2, storeKey: 2, senderId: 1, channelId: 1, data: [ 4, 5, 6 ] ),
                    StorageScope.PrepareStreamMessage( id: 3, storeKey: 3, senderId: 1, channelId: 1, data: [ 7, 8, 9, 10 ] )
                ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );
            storage.WriteListenerMetadata(
                clientId: 1,
                channelId: 1,
                queueId: 1,
                maxRedeliveries: 5,
                minAckTimeout: Duration.FromHours( 1 ),
                maxRetries: 5,
                retryDelay: Duration.FromHours( 1 ),
                deadLetterCapacityHint: 5,
                minDeadLetterRetention: Duration.FromHours( 1 ) );

            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 3 ) ] );

            storage.WriteQueueUnackedMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueUnackedMessage( streamId: 1, storeKey: 2, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueRetryMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueRetryMessage( streamId: 1, storeKey: 1, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueDeadLetterMessage( streamId: 1, storeKey: 0, retry: 0, redelivery: 0 ) ] );

            var originalEndPoint = new IPEndPoint( IPAddress.Any, 0 );
            await using ( var disposedServer = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Recreated )
                                e.Source.Client.Server.Dispose();
                        } ) ) ) )
                await disposedServer.StartAsync();

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            await server.StartAsync();

            Assertion.All(
                    server.Channels.Count.TestEquals( 1 ),
                    server.Channels.TryGetById( 1 )
                        .TestNotNull( c => Assertion.All(
                            "channel",
                            c.Publishers.Count.TestEquals( 1 ),
                            c.Listeners.Count.TestEquals( 1 ) ) ),
                    server.Streams.Count.TestEquals( 1 ),
                    server.Streams.TryGetById( 1 )
                        .TestNotNull( s => Assertion.All(
                            "stream",
                            s.Publishers.Count.TestEquals( 1 ),
                            s.Messages.Count.TestEquals( 4 ),
                            s.Messages.TryGetByKey( 0, includeData: true ).TestNotNull( m => m.Data.TestSequence<byte>( [ 1 ] ) ),
                            s.Messages.TryGetByKey( 1, includeData: true ).TestNotNull( m => m.Data.TestSequence<byte>( [ 2, 3 ] ) ),
                            s.Messages.TryGetByKey( 2, includeData: true ).TestNotNull( m => m.Data.TestSequence<byte>( [ 4, 5, 6 ] ) ),
                            s.Messages.TryGetByKey( 3, includeData: true )
                                .TestNotNull( m => m.Data.TestSequence<byte>( [ 7, 8, 9, 10 ] ) ) ) ),
                    server.Clients.Count.TestEquals( 1 ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c => Assertion.All(
                            "client",
                            c.Publishers.Count.TestEquals( 1 ),
                            c.Listeners.Count.TestEquals( 1 ),
                            c.Queues.Count.TestEquals( 1 ),
                            c.Queues.TryGetById( 1 )
                                .TestNotNull( q => Assertion.All(
                                    "queue",
                                    q.Messages.Pending.Count.TestEquals( 1 ),
                                    q.Messages.Unacked.Count.TestEquals( 1 ),
                                    q.Messages.Retries.Count.TestEquals( 1 ),
                                    q.Messages.DeadLetter.Count.TestEquals( 1 ),
                                    q.Messages.Pending.TryPeekAt( 0 ).TestNotNull( m => m.StoreKey.TestEquals( 3 ) ),
                                    q.Messages.Unacked.TryGetFirst().TestNotNull( m => m.StoreKey.TestEquals( 2 ) ),
                                    q.Messages.Retries.TryGetNext().TestNotNull( m => m.StoreKey.TestEquals( 1 ) ),
                                    q.Messages.DeadLetter.TryPeekAt( 0 ).TestNotNull( m => m.StoreKey.TestEquals( 0 ) ) ) ) ) ) )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldDisposeUnreferencedChannel()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );

            var logger = new ChannelEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetRootStoragePath( storage.Path )
                    .SetChannelLoggerFactory( _ => logger.GetLogger() ) );

            await server.StartAsync();

            Assertion.All(
                    server.Channels.TryGetById( 1 ).TestNull(),
                    logger.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Dispose] Channel = [1] 'foo', TraceId = 2 (start)",
                                $"[ServerTrace] Channel = [1] 'foo', TraceId = 2, Correlation = (Server = {originalEndPoint}, TraceId = 1)",
                                "[Disposing] Channel = [1] 'foo', TraceId = 2",
                                "[Disposed] Channel = [1] 'foo', TraceId = 2",
                                "[Trace:Dispose] Channel = [1] 'foo', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldBeginDisposingUnreferencedStream()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 1, messages: [ ] );

            var endSource = new SafeTaskCompletionSource();
            var logger = new StreamEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetRootStoragePath( storage.Path )
                    .SetStreamLoggerFactory( _ => logger.GetLogger(
                        MessageBrokerStreamLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerStreamTraceEventType.Dispose )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();
            await endSource.Task;

            Assertion.All(
                    server.Streams.TryGetById( 1 ).TestNull(),
                    logger.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Dispose] Stream = [1] 'foo', TraceId = 2 (start)",
                                "[Disposing] Stream = [1] 'foo', TraceId = 2",
                                "[Disposed] Stream = [1] 'foo', TraceId = 2",
                                "[Trace:Dispose] Stream = [1] 'foo', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenServerMetadataFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteToFile( StorageScope.GetServerMetadataSubpath(), [ 0, 1, 2 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Created ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenServerMetadataFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata( header: [ 1, 2, 3, 4 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Created ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenChannelMetadataFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteToFile( StorageScope.GetChannelMetadataSubpath( 1 ), [ 0, 1, 2 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenChannelMetadataFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo", header: [ 1, 2, 3, 4 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenChannelMetadataFileHasInvalidChannelName()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: new string( 'x', 513 ) );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenChannelIdCannotBeParsedFromMetadataFile()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo", fileName: "metax.mbch" );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerException_WhenChannelIsDuplicated()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteChannelMetadata( channelId: 2, channelName: "foo" );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMetadataFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteToFile( StorageScope.GetStreamMetadataSubpath( 1 ), [ 0, 1, 2 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMetadataFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo", header: [ 1, 2, 3, 4 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMetadataFileHasInvalidStreamName()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: new string( 'x', 513 ) );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamIdCannotBeParsedFromDirectory()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo", directoryName: "_x" );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMetadataFileDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.CreateDirectory( StorageScope.GetStreamMetadataSubpath( 1 ) );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerException_WhenStreamIsDuplicated()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 1, messages: [ ] );
            storage.WriteStreamMetadata( streamId: 2, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 2, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteToFile( StorageScope.GetStreamMessagesSubpath( 1 ), [ 0, 1, 2 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 1, messages: [ ], header: [ 1, 2, 3, 4 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasNegativeMessageOrRoutingCount()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 1, messages: [ ], messageCount: -1, routingCount: -2 );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasInvalidMessageHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages:
                [ StorageScope.PrepareStreamMessage( id: 0, storeKey: -1, senderId: 1, channelId: 1, data: [ ], dataLength: -2 ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasNonEmptyDiscardedMessage()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareDiscardedStreamMessage( id: 0, storeKey: -1, senderId: 1, dataLength: 1 ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task
            StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasMessageForNonExistingChannelOrClient()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasRoutingWithInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ ],
                routings:
                [
                    StorageScope.PrepareStreamMessageRouting( messageId: 0, targetClientIds: new HashSet<int> { 1 }, dataLength: -1 )
                ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasRoutingForNonExistentMessage()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ ],
                routings: [ StorageScope.PrepareStreamMessageRouting( messageId: 0, targetClientIds: new HashSet<int> { 1 } ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasDuplicatedMessages()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages:
                [
                    StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ),
                    StorageScope.PrepareStreamMessage( id: 1, storeKey: 0, senderId: 1, channelId: 1, data: [ 2, 3 ] )
                ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasNonExistentNextPendingMessageKey()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 1, messages: [ ], nextPendingNodeId: NullableIndex.Create( 0 ) );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasInvalidEphemeralPublisher()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ ],
                ephemeralClients: [ StorageScope.PrepareEphemeralClient( senderId: 0, virtualId: 0, name: string.Empty ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasInvalidEphemeralPublisherName()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ ],
                ephemeralClients: [ StorageScope.PrepareEphemeralClient( senderId: 1, virtualId: 1, name: new string( 'x', 513 ) ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task
            StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesHasMessageWithNonExistentEphemeralPublisher()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: -1, channelId: 1, data: [ 1 ] ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenStreamMessagesFileDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenClientMetadataFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteToFile( StorageScope.GetClientMetadataSubpath( 1 ), [ 0, 1, 2 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenClientMetadataFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo", header: [ 1, 2, 3, 4 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenClientMetadataFileHasInvalidClientName()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: new string( 'x', 513 ) );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenClientIdCannotBeParsedFromDirectory()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo", directoryName: "_x" );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenClientMetadataFileDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.CreateDirectory( StorageScope.GetClientMetadataSubpath( 1 ) );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerException_WhenClientIsDuplicated()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteClientMetadata( clientId: 2, clientName: "foo" );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueMetadataFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteToFile( StorageScope.GetQueueMetadataSubpath( 1, 1 ), [ 0, 1, 2 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueMetadataFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo", header: [ 1, 2, 3, 4 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueMetadataFileHasInvalidQueueName()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: new string( 'x', 513 ) );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueIdCannotBeParsedFromDirectory()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo", directoryName: "_x" );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueMetadataFileDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.CreateDirectory( StorageScope.GetQueueMetadataSubpath( 1, 1 ) );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerRemoteClientException_WhenQueueIsDuplicated()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 2, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerRemoteClientException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueuePendingMessagesFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteToFile( StorageScope.GetQueuePendingMessagesSubpath( 1, 1 ), [ 0, 1, 2 ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueuePendingMessagesFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ], header: [ 1, 2, 3, 4 ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueuePendingMessagesFileDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task
            StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueuePendingMessagesHasMessageFromNonExistingStream()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ) ] );

            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueuePendingMessagesHasNonExistingMessage()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 1 ) ] );

            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueuePendingMessagesHasPendingStreamMessage()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ],
                nextPendingNodeId: NullableIndex.Create( 0 ) );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ) ] );

            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueUnackedMessagesFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteToFile( StorageScope.GetQueueUnackedMessagesSubpath( 1, 1 ), [ 0, 1, 2 ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueUnackedMessagesFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ], header: [ 1, 2, 3, 4 ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueUnackedMessagesFileDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task
            StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueUnackedMessagesHasMessageFromNonExistingStream()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueUnackedMessage( streamId: 1, storeKey: 0, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueUnackedMessagesHasNonExistingMessage()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueUnackedMessage( streamId: 1, storeKey: 1, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueUnackedMessagesHasPendingStreamMessage()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ],
                nextPendingNodeId: NullableIndex.Create( 0 ) );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueUnackedMessage( streamId: 1, storeKey: 0, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueUnackedMessagesHasInvalidRetryOrRedelivery()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueUnackedMessage( streamId: 1, storeKey: 0, retry: -1, redelivery: -2 ) ] );

            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueRetryMessagesFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteToFile( StorageScope.GetQueueRetryMessagesSubpath( 1, 1 ), [ 0, 1, 2 ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueRetryMessagesFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ], header: [ 1, 2, 3, 4 ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueRetryMessagesFileDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task
            StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueRetryMessagesHasMessageFromNonExistingStream()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueRetryMessage( streamId: 1, storeKey: 0, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueRetryMessagesHasNonExistingMessage()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueRetryMessage( streamId: 1, storeKey: 1, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueRetryMessagesHasPendingStreamMessage()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ],
                nextPendingNodeId: NullableIndex.Create( 0 ) );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueRetryMessage( streamId: 1, storeKey: 0, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueRetryMessagesHasInvalidRetryOrRedelivery()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueRetryMessage( streamId: 1, storeKey: 0, retry: -1, redelivery: -2 ) ] );

            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueDeadLetterMessagesFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteToFile( StorageScope.GetQueueDeadLetterMessagesSubpath( 1, 1 ), [ 0, 1, 2 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueDeadLetterMessagesFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ], header: [ 1, 2, 3, 4 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueDeadLetterMessagesFileDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task
            StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueDeadLetterMessagesHasMessageFromNonExistingStream()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueDeadLetterMessage( streamId: 1, storeKey: 0, retry: 0, redelivery: 0 ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueDeadLetterMessagesHasNonExistingMessage()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueDeadLetterMessage( streamId: 1, storeKey: 1, retry: 0, redelivery: 0 ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueDeadLetterMessagesHasPendingStreamMessage()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ],
                nextPendingNodeId: NullableIndex.Create( 0 ) );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueDeadLetterMessage( streamId: 1, storeKey: 0, retry: 0, redelivery: 0 ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task
            StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenQueueDeadLetterMessagesHasInvalidRetryOrRedelivery()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ) ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueDeadLetterMessage( streamId: 1, storeKey: 0, retry: -1, redelivery: -2 ) ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldNotThrow_WhenQueueMessagesHaveMessagesForNonExistingListener()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages:
                [
                    StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 0 ] ),
                    StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 1 ] ),
                    StorageScope.PrepareStreamMessage( id: 2, storeKey: 2, senderId: 1, channelId: 1, data: [ 2 ] ),
                    StorageScope.PrepareStreamMessage( id: 3, storeKey: 3, senderId: 1, channelId: 1, data: [ 3 ] )
                ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ) ] );

            storage.WriteQueueUnackedMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueUnackedMessage( streamId: 1, storeKey: 1, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueRetryMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueRetryMessage( streamId: 1, storeKey: 2, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueueDeadLetterMessage( streamId: 1, storeKey: 3, retry: 0, redelivery: 0 ) ] );

            storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "bar" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 2, messages: [ ] );

            var logger = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetLogger( logger.GetLogger() )
                    .SetRootStoragePath( storage.Path ) );

            await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Running ),
                    logger.GetAll()
                        .TestAny( (t, _) => t.Logs.TestContainsSequence(
                        [
                            $"""
                             [Error] Server = {originalEndPoint}, TraceId = 1
                             LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerStorageException: Server storage file '{Path.Combine( server.RootStorageDirectoryPath!, "clients", "_1", "queues", "_1", "pending.mbpm" )}' contains invalid data. Encountered 1 error(s):
                             1. Failed to enqueue a message from stream [1] 'foo' with store key 0 in queue [1] 'foo' because listener for channel [1] 'foo' does not exist.
                             """,
                            $"""
                             [Error] Server = {originalEndPoint}, TraceId = 1
                             LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerStorageException: Server storage file '{Path.Combine( server.RootStorageDirectoryPath!, "clients", "_1", "queues", "_1", "unacked.mbue" )}' contains invalid data. Encountered 1 error(s):
                             1. Failed to enqueue a message from stream [1] 'foo' with store key 1 in queue [1] 'foo' because listener for channel [1] 'foo' does not exist.
                             """,
                            $"""
                             [Error] Server = {originalEndPoint}, TraceId = 1
                             LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerStorageException: Server storage file '{Path.Combine( server.RootStorageDirectoryPath!, "clients", "_1", "queues", "_1", "retries.mbre" )}' contains invalid data. Encountered 1 error(s):
                             1. Failed to enqueue a message from stream [1] 'foo' with store key 2 in queue [1] 'foo' because listener for channel [1] 'foo' does not exist.
                             """,
                            $"""
                             [Error] Server = {originalEndPoint}, TraceId = 1
                             LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerStorageException: Server storage file '{Path.Combine( server.RootStorageDirectoryPath!, "clients", "_1", "queues", "_1", "deadletter.mbdl" )}' contains invalid data. Encountered 1 error(s):
                             1. Failed to enqueue a message from stream [1] 'foo' with store key 3 in queue [1] 'foo' because listener for channel [1] 'foo' does not exist.
                             """
                        ] ) ) )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenPublisherMetadataFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteToFile( StorageScope.GetPublisherMetadataSubpath( 1, 1 ), [ 0, 1, 2 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenPublisherMetadataFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1, header: [ 1, 2, 3, 4 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenChannelIdCannotBeParsedFromPublisherMetadataFile()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1, fileName: "metax.mbpb" );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerRemoteClientException_WhenPublisherChannelDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 1, messages: [ ] );
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerRemoteClientException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerRemoteClientException_WhenPublisherStreamDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerRemoteClientException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenListenerMetadataFileHasInvalidLength()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteToFile( StorageScope.GetListenerMetadataSubpath( 1, 1 ), [ 0, 1, 2 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenListenerMetadataFileHasInvalidHeader()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1, header: [ 1, 2, 3, 4 ] );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerServerStorageException_WhenChannelIdCannotBeParsedFromListenerMetadataFile()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1, fileName: "metax.mbls" );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerServerStorageException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerRemoteClientException_WhenListenerChannelDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1 );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerRemoteClientException>() )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldReturnMessageBrokerRemoteClientException_WhenListenerQueueDoesNotExist()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1 );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default.SetRootStoragePath( storage.Path ) );

            var result = await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Disposed ),
                    result.Exception.TestType().Exact<MessageBrokerRemoteClientException>() )
                .Go();
        }

        [Theory]
        [InlineData(
            0,
            -1,
            -1,
            -1,
            -1,
            -1,
            -1,
            """
            Encountered 6 issue(s):
            1. Expected prefetch hint to be greater than 0 but found 0.
            2. Expected max retries to not be negative but found -1.
            3. Expected retry delay to not be negative but found -0.001 second(s).
            4. Expected max redeliveries to not be negative but found -1.
            5. Expected min ACK timeout to not be negative but found -0.001 second(s).
            6. Expected dead letter capacity hint to not be negative but found -1.
            """ )]
        [InlineData(
            1,
            0,
            2,
            1,
            0,
            0,
            5,
            """
            Encountered 3 issue(s):
            1. Expected disabled retry delay to be equal to 0 but found 0.002 second(s).
            2. Expected enabled min ACK timeout to be greater than 0 but found 0 second(s).
            3. Expected disabled min dead letter retention to be equal to 0 but found 0.005 second(s).
            """ )]
        [InlineData(
            1,
            0,
            0,
            0,
            0,
            1,
            0,
            """
            Encountered 2 issue(s):
            1. Expected enabled min ACK timeout to be greater than 0 but found 0 second(s).
            2. Expected enabled min dead letter retention to be greater than 0 but found 0 second(s).
            """ )]
        public async Task StartAsync_ShouldSanitizeInvalidListenerProperties(
            short prefetchHint,
            int maxRetries,
            int retryDelayMs,
            int maxRedeliveries,
            int minAckTimeoutMs,
            int deadLetterCapacity,
            long deadLetterRetentionMs,
            string expectedWarning)
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteListenerMetadata(
                clientId: 1,
                channelId: 1,
                queueId: 1,
                prefetchHint: prefetchHint,
                maxRetries: maxRetries,
                retryDelay: Duration.FromMilliseconds( retryDelayMs ),
                maxRedeliveries: maxRedeliveries,
                minAckTimeout: Duration.FromMilliseconds( minAckTimeoutMs ),
                deadLetterCapacityHint: deadLetterCapacity,
                minDeadLetterRetention: Duration.FromMilliseconds( deadLetterRetentionMs ) );

            var clientLogs = new ClientEventLogger();

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger() ) );

            await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Running ),
                    server.Clients.TryGetById( 1 ).TestNotNull( c => c.Listeners.TryGetByChannelId( 1 ).TestNotNull() ),
                    clientLogs.GetAll()
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestContainsSequence(
                            [
                                $"""
                                 [Error] Client = [1] 'foo', TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Listener for client [1] 'foo' and channel with ID 1 has invalid metadata. {expectedWarning}
                                 """
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldSanitizeUnexpectedFilterExpression()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1, filter: "expression" );

            var clientLogs = new ClientEventLogger();

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger() ) );

            await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Running ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c =>
                            c.Listeners.TryGetByChannelId( 1 ).TestNotNull( l => l.QueueBindings.Primary.FilterExpression.TestNull() ) ),
                    clientLogs.GetAll()
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestContainsSequence(
                            [
                                """
                                [Error] Client = [1] 'foo', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Listener for client [1] 'foo' and channel with ID 1 has invalid metadata. Encountered 1 issue(s):
                                1. Filter expressions are not enabled but found:
                                expression
                                """
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldSanitizeFilterExpressionThatCannotBeParsed()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1, filter: "expression" );

            var clientLogs = new ClientEventLogger();

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetRootStoragePath( storage.Path )
                    .SetExpressionFactory(
                        new ParsedExpressionFactoryBuilder()
                            .AddGenericArithmeticOperators()
                            .AddGenericBitwiseOperators()
                            .AddGenericLogicalOperators()
                            .Build() )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger() ) );

            await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Running ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c =>
                            c.Listeners.TryGetByChannelId( 1 ).TestNotNull( l => l.QueueBindings.Primary.FilterExpression.TestNull() ) ),
                    clientLogs.GetAll()
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestContainsSequence(
                            [
                                (e, _) => e.TestStartsWith(
                                    """
                                    [Error] Client = [1] 'foo', TraceId = 1
                                    LfrlAnvil.Computable.Expressions.Exceptions.ParsedExpressionCreationException: Failed to create an expression:
                                    expression

                                    Encountered 1 error(s):
                                    1. OutputTypeConverterHasThrownException, construct of type LfrlAnvil.Computable.Expressions.Constructs.ParsedExpressionTypeConverter, an exception has been thrown:
                                    System.InvalidOperationException
                                    """ )
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldSanitizeFilterExpressionWithTooManyArguments()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1, filter: "a.Data.Length + b.Data.Length < 10i" );

            var clientLogs = new ClientEventLogger();

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetRootStoragePath( storage.Path )
                    .SetExpressionFactory(
                        new ParsedExpressionFactoryBuilder()
                            .AddGenericArithmeticOperators()
                            .AddGenericBitwiseOperators()
                            .AddGenericLogicalOperators()
                            .AddInt32TypeDefinition()
                            .Build() )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger() ) );

            await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Running ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c =>
                            c.Listeners.TryGetByChannelId( 1 ).TestNotNull( l => l.QueueBindings.Primary.FilterExpression.TestNull() ) ),
                    clientLogs.GetAll()
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestContainsSequence(
                            [
                                """
                                [Error] Client = [1] 'foo', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Listener for client [1] 'foo' and channel with ID 1 has invalid metadata. Encountered 1 issue(s):
                                1. Expected at most one filter expression context argument but found 2 ('a', 'b') in the following expression:
                                a.Data.Length + b.Data.Length < 10i
                                """
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task StartAsync_ShouldRecreateListenerWithValidFilterExpression()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteClientMetadata( clientId: 1, clientName: "foo" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1, filter: "a.Data.Length < 10i" );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetRootStoragePath( storage.Path )
                    .SetExpressionFactory(
                        new ParsedExpressionFactoryBuilder()
                            .AddGenericArithmeticOperators()
                            .AddGenericBitwiseOperators()
                            .AddGenericLogicalOperators()
                            .AddInt32TypeDefinition()
                            .Build() ) );

            await server.StartAsync();

            Assertion.All(
                    server.State.TestEquals( MessageBrokerServerState.Running ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c =>
                            c.Listeners.TryGetByChannelId( 1 )
                                .TestNotNull( l => l.QueueBindings.Primary.FilterExpression.TestEquals( "a.Data.Length < 10i" ) ) ) )
                .Go();
        }
    }
}
