using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerServerTests
{
    public class Reconnecting : TestsBase
    {
        [Fact]
        public async Task Start_ShouldReconnect_WhenInactiveNonEphemeralClientsReconnects()
        {
            using var storage = StorageScope.Create();

            var firstConnectEndSource = new SafeTaskCompletionSource( completionCount: 2 );
            var secondConnectEndSource = new SafeTaskCompletionSource( completionCount: 2 );
            var disposalEndSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            var connectCount = Atomic.Create( 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                {
                                    ++connectCount.Value;
                                }
                                else if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                    or MessageBrokerRemoteClientTraceEventType.BindListener )
                                {
                                    if ( connectCount.Value == 1 )
                                        firstConnectEndSource.Complete();
                                    else
                                        secondConnectEndSource.Complete();
                                }
                                else if ( e.Type == MessageBrokerRemoteClientTraceEventType.Deactivate )
                                {
                                    if ( connectCount.Value == 1 )
                                        disposalEndSource.Complete();
                                }
                            } ) ) ) );

            await server.StartAsync();

            MessageBrokerRemoteClient? remoteClient;
            using ( var client = new ClientMock() )
            {
                await client.EstablishHandshake( server, "test", isEphemeral: false );
                await client.GetTask( c =>
                {
                    c.SendBindPublisherRequest( "foo", isEphemeral: false );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest( "foo", createChannelIfNotExists: true, isEphemeral: false );
                    c.ReadListenerBoundResponse();
                } );

                await firstConnectEndSource.Task;
                remoteClient = server.Clients.TryGetById( 1 );
                if ( remoteClient is not null )
                    await remoteClient.DisconnectAsync();
            }

            await disposalEndSource.Task;

            using var reconnectedClient = new ClientMock();
            await reconnectedClient.EstablishHandshake(
                server,
                "test",
                isEphemeral: false,
                messageTimeout: Duration.FromSeconds( 2 ),
                pingInterval: Duration.FromSeconds( 15 ),
                maxBatchPacketCount: 5,
                synchronizeExternalObjectNames: true );

            await reconnectedClient.GetTask( c =>
            {
                c.SendBindPublisherRequest( "bar", isEphemeral: false );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest( "bar", createChannelIfNotExists: true, isEphemeral: false );
                c.ReadListenerBoundResponse();
            } );

            await secondConnectEndSource.Task;

            Assertion.All(
                    server.Clients.Count.TestEquals( 1 ),
                    remoteClient.TestNotNull( c => Assertion.All(
                        "remoteClient",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.IsEphemeral.TestFalse(),
                        c.Listeners.TryGetByChannelId( 1 )
                            .TestNotNull( l => l.State.TestEquals( MessageBrokerChannelListenerBindingState.Inactive ) ),
                        c.Listeners.TryGetByChannelId( 2 )
                            .TestNotNull( l => l.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ) ),
                        c.Publishers.TryGetByChannelId( 1 )
                            .TestNotNull( p => p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Inactive ) ),
                        c.Publishers.TryGetByChannelId( 2 )
                            .TestNotNull( p => p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ) ),
                        c.Queues.TryGetById( 1 ).TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ),
                        c.Queues.TryGetById( 2 ).TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ) ) ),
                    logs.GetAll()
                        .Skip( 4 )
                        .Take( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Start] Client = [1] 'test', TraceId = 4 (start)",
                                $"[ServerTrace] Client = [1] 'test', TraceId = 4, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                "[HandshakeEstablished] Client = [1] 'test', TraceId = 4, MessageTimeout = 2 second(s), PingInterval = 15 second(s), BatchPacket = (MaxPacketCount = 5, MaxLength = 16384 B)",
                                "[Trace:Start] Client = [1] 'test', TraceId = 4 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldReconnect_WhenInactiveNonEphemeralClientsReconnects_AfterServerRestart()
        {
            using var storage = StorageScope.Create();
            await using var delaySource = ValueTaskDelaySource.Start();

            var firstConnectEndSource = new SafeTaskCompletionSource( completionCount: 2 );
            var disposalEndSource = new SafeTaskCompletionSource();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using ( var disposedServer = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetDelaySourceFactory( _ => delaySource )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                or MessageBrokerRemoteClientTraceEventType.BindListener )
                            {
                                firstConnectEndSource.Complete();
                            }
                            else if ( e.Type == MessageBrokerRemoteClientTraceEventType.Deactivate )
                            {
                                disposalEndSource.Complete();
                            }
                        } ) ) ) )
            {
                await disposedServer.StartAsync();

                using ( var client = new ClientMock() )
                {
                    await client.EstablishHandshake( disposedServer, "test", isEphemeral: false );
                    await client.GetTask( c =>
                    {
                        c.SendBindPublisherRequest( "foo", isEphemeral: false );
                        c.SendBindListenerRequest( "foo", createChannelIfNotExists: true, isEphemeral: false );
                        c.ReadPublisherBoundResponse();
                        c.ReadListenerBoundResponse();
                    } );

                    await firstConnectEndSource.Task;
                    var remoteClient = disposedServer.Clients.TryGetById( 1 );
                    if ( remoteClient is not null )
                        await remoteClient.DisconnectAsync();
                }

                await disposalEndSource.Task;
            }

            var secondConnectEndSource = new SafeTaskCompletionSource( completionCount: 2 );
            var logs = new ClientEventLogger();

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetDelaySourceFactory( _ => delaySource )
                    .SetClientLoggerFactory( _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                    or MessageBrokerRemoteClientTraceEventType.BindListener )
                                {
                                    secondConnectEndSource.Complete();
                                }
                            } ) ) ) );

            await server.StartAsync();

            using var reconnectedClient = new ClientMock();
            await reconnectedClient.EstablishHandshake( server, "test", isEphemeral: false );
            await reconnectedClient.GetTask( c =>
            {
                c.SendBindPublisherRequest( "bar", isEphemeral: false );
                c.SendBindListenerRequest( "bar", createChannelIfNotExists: true, isEphemeral: false );
                c.ReadPublisherBoundResponse();
                c.ReadListenerBoundResponse();
            } );

            await secondConnectEndSource.Task;

            Assertion.All(
                    server.Clients.Count.TestEquals( 1 ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c => Assertion.All(
                            "remoteClient",
                            c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                            c.IsEphemeral.TestFalse(),
                            c.Listeners.TryGetByChannelId( 1 )
                                .TestNotNull( l => l.State.TestEquals( MessageBrokerChannelListenerBindingState.Inactive ) ),
                            c.Listeners.TryGetByChannelId( 2 )
                                .TestNotNull( l => l.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ) ),
                            c.Publishers.TryGetByChannelId( 1 )
                                .TestNotNull( p => p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Inactive ) ),
                            c.Publishers.TryGetByChannelId( 2 )
                                .TestNotNull( p => p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ) ),
                            c.Queues.TryGetById( 1 ).TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ),
                            c.Queues.TryGetById( 2 ).TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .Take( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Start] Client = [1] 'test', TraceId = 6 (start)",
                                $"[ServerTrace] Client = [1] 'test', TraceId = 6, Correlation = (Server = {server.LocalEndPoint}, TraceId = 4)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 6, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 6, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                "[HandshakeEstablished] Client = [1] 'test', TraceId = 6, MessageTimeout = 1 second(s), PingInterval = 10 second(s), BatchPacket = <disabled>",
                                "[Trace:Start] Client = [1] 'test', TraceId = 6 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefullyAndPreserveStorage_WhenNonEphemeralClientReconnectFails()
        {
            using var storage = StorageScope.Create();

            var firstConnectEndSource = new SafeTaskCompletionSource( completionCount: 2 );
            var secondConnectEndSource = new SafeTaskCompletionSource();
            var disposalEndSource = new SafeTaskCompletionSource();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            var logs = new ClientEventLogger();
            var connectCount = Atomic.Create( 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                {
                                    if ( ++connectCount.Value > 1 )
                                        secondConnectEndSource.Complete();
                                }
                                else if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                    or MessageBrokerRemoteClientTraceEventType.BindListener )
                                {
                                    firstConnectEndSource.Complete();
                                }
                                else if ( e.Type == MessageBrokerRemoteClientTraceEventType.Deactivate )
                                {
                                    disposalEndSource.Complete();
                                }
                            } ) ) ) );

            await server.StartAsync();

            using ( var client = new ClientMock() )
            {
                await client.EstablishHandshake( server, "test", isEphemeral: false );
                await client.GetTask( c =>
                {
                    c.SendBindPublisherRequest( "foo", isEphemeral: false );
                    c.SendBindListenerRequest( "foo", createChannelIfNotExists: true, isEphemeral: false );
                    c.ReadPublisherBoundResponse();
                    c.ReadListenerBoundResponse();
                } );

                await firstConnectEndSource.Task;
            }

            await disposalEndSource.Task;

            using var reconnectedClient = new ClientMock();
            reconnectedClient.Connect( server.LocalEndPoint );
            await reconnectedClient.GetTask( c =>
            {
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), isEphemeral: false );
                c.ReadHandshakeAcceptedResponse();
                c.Send( [ 0, 0, 0, 0, 0 ] );
            } );

            await secondConnectEndSource.Task;

            Assertion.All(
                    server.Clients.Count.TestEquals( 1 ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c => Assertion.All(
                            "remoteClient",
                            c.State.TestEquals( MessageBrokerRemoteClientState.Inactive ),
                            c.IsEphemeral.TestFalse(),
                            c.Listeners.TryGetByChannelId( 1 )
                                .TestNotNull( l => l.State.TestEquals( MessageBrokerChannelListenerBindingState.Inactive ) ),
                            c.Publishers.TryGetByChannelId( 1 )
                                .TestNotNull( p => p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Inactive ) ),
                            c.Queues.TryGetById( 1 ).TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Inactive ) ) ) ),
                    logs.GetAll()
                        .Skip( 4 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Start] Client = [1] 'test', TraceId = 4 (start)",
                                $"[ServerTrace] Client = [1] 'test', TraceId = 4, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (<unrecognized-endpoint-0>, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 4
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid <unrecognized-endpoint-0> from client [1] 'test'. Encountered 1 error(s):
                                1. Received unexpected server endpoint.
                                """,
                                "[Deactivating] Client = [1] 'test', TraceId = 4, IsAlive = True",
                                "[Deactivated] Client = [1] 'test', TraceId = 4, IsAlive = True",
                                "[Trace:Start] Client = [1] 'test', TraceId = 4 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefullyAndPreserveStorage_WhenNonEphemeralClientReconnectFails_AfterServerDisposal()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 1, messages: [ ] );
            storage.WriteClientMetadata( clientId: 1, clientName: "test", traceId: 4 );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1 );
            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            var connectEndSource = new SafeTaskCompletionSource();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            var logs = new ClientEventLogger();

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    connectEndSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            client.Connect( server.LocalEndPoint );
            await client.GetTask( c =>
            {
                c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), isEphemeral: false );
                c.ReadHandshakeAcceptedResponse();
                c.Send( [ 0, 0, 0, 0, 0 ] );
            } );

            await connectEndSource.Task;

            Assertion.All(
                    server.Clients.Count.TestEquals( 1 ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c => Assertion.All(
                            "remoteClient",
                            c.State.TestEquals( MessageBrokerRemoteClientState.Inactive ),
                            c.IsEphemeral.TestFalse(),
                            c.Listeners.TryGetByChannelId( 1 )
                                .TestNotNull( l => l.State.TestEquals( MessageBrokerChannelListenerBindingState.Inactive ) ),
                            c.Publishers.TryGetByChannelId( 1 )
                                .TestNotNull( p => p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Inactive ) ),
                            c.Queues.TryGetById( 1 ).TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Inactive ) ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Start] Client = [1] 'test', TraceId = 6 (start)",
                                $"[ServerTrace] Client = [1] 'test', TraceId = 6, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 6, Packet = (<unrecognized-endpoint-0>, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 6
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid <unrecognized-endpoint-0> from client [1] 'test'. Encountered 1 error(s):
                                1. Received unexpected server endpoint.
                                """,
                                "[Deactivating] Client = [1] 'test', TraceId = 6, IsAlive = True",
                                "[Deactivated] Client = [1] 'test', TraceId = 6, IsAlive = True",
                                "[Trace:Start] Client = [1] 'test', TraceId = 6 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldReconnect_WhenNonEphemeralClientReconnectsAsEphemeral()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 1, messages: [ ] );
            storage.WriteClientMetadata( clientId: 1, clientName: "test", traceId: 4 );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1 );
            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            var connectEndSource = new SafeTaskCompletionSource();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            var logs = new ClientEventLogger();

            await using ( var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    connectEndSource.Complete();
                            } ) ) ) ) )
            {
                await server.StartAsync();

                using var client = new ClientMock();
                await client.EstablishHandshake( server, "test", isEphemeral: true );
                await connectEndSource.Task;

                Assertion.All(
                        storage.DirectoryExists( StorageScope.GetClientMetadataSubpath( clientId: 1 ) ).TestFalse(),
                        server.Clients.Count.TestEquals( 1 ),
                        server.Clients.TryGetById( 1 )
                            .TestNotNull( c => Assertion.All(
                                "remoteClient",
                                c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                                c.IsEphemeral.TestTrue(),
                                c.Listeners.TryGetByChannelId( 1 )
                                    .TestNotNull( l => Assertion.All(
                                        "listener",
                                        l.State.TestEquals( MessageBrokerChannelListenerBindingState.Inactive ),
                                        l.IsEphemeral.TestTrue() ) ),
                                c.Publishers.TryGetByChannelId( 1 )
                                    .TestNotNull( p => Assertion.All(
                                        "publisher",
                                        p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Inactive ),
                                        p.IsEphemeral.TestTrue() ) ),
                                c.Queues.TryGetById( 1 ).TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ) ) ),
                        logs.GetAll()
                            .Skip( 1 )
                            .TestSequence(
                            [
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:Start] Client = [1] 'test', TraceId = 6 (start)",
                                    $"[ServerTrace] Client = [1] 'test', TraceId = 6, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                    "[ReadPacket:Received] Client = [1] 'test', TraceId = 6, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                    "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 6, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                    "[HandshakeEstablished] Client = [1] 'test', TraceId = 6, MessageTimeout = 1 second(s), PingInterval = 10 second(s), BatchPacket = <disabled>",
                                    "[Trace:Start] Client = [1] 'test', TraceId = 6 (end)"
                                ] )
                            ] ) )
                    .Go();
            }

            storage.DirectoryExists( StorageScope.GetClientMetadataSubpath( clientId: 1 ) ).TestFalse().Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenNonEphemeralClientReconnectAsEphemeralFails()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 1, messages: [ ] );
            storage.WriteClientMetadata( clientId: 1, clientName: "test", traceId: 4 );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1 );
            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            var connectEndSource = new SafeTaskCompletionSource();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            var logs = new ClientEventLogger();

            await using ( var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    connectEndSource.Complete();
                            } ) ) ) ) )
            {
                await server.StartAsync();

                using var client = new ClientMock();
                client.Connect( server.LocalEndPoint );
                await client.GetTask( c =>
                {
                    c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), isEphemeral: true );
                    c.ReadHandshakeAcceptedResponse();
                    c.Send( [ 0, 0, 0, 0, 0 ] );
                } );

                await connectEndSource.Task;

                Assertion.All(
                        storage.DirectoryExists( StorageScope.GetClientMetadataSubpath( clientId: 1 ) ).TestFalse(),
                        server.Clients.Count.TestEquals( 0 ),
                        logs.GetAll()
                            .Skip( 1 )
                            .TestSequence(
                            [
                                (t, _) => t.Logs.TestSequence(
                                [
                                    "[Trace:Start] Client = [1] 'test', TraceId = 6 (start)",
                                    $"[ServerTrace] Client = [1] 'test', TraceId = 6, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                    "[ReadPacket:Received] Client = [1] 'test', TraceId = 6, Packet = (<unrecognized-endpoint-0>, Length = 5)",
                                    """
                                    [Error] Client = [1] 'test', TraceId = 6
                                    LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid <unrecognized-endpoint-0> from client [1] 'test'. Encountered 1 error(s):
                                    1. Received unexpected server endpoint.
                                    """,
                                    "[Deactivating] Client = [1] 'test', TraceId = 6, IsAlive = False",
                                    "[Deactivated] Client = [1] 'test', TraceId = 6, IsAlive = False",
                                    "[Trace:Start] Client = [1] 'test', TraceId = 6 (end)"
                                ] )
                            ] ) )
                    .Go();
            }

            storage.DirectoryExists( StorageScope.GetClientMetadataSubpath( clientId: 1 ) ).TestFalse().Go();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public async Task Start_ShouldDisposeUnreferencedQueues_WhenClientReconnects(bool isEphemeral)
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages( streamId: 1, messages: [ ] );
            storage.WriteClientMetadata( clientId: 1, clientName: "test", traceId: 4 );
            storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            var connectEndSource = new SafeTaskCompletionSource( completionCount: 2 );
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            var logs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    connectEndSource.Complete();
                            } ) ) )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.Deactivate )
                                    connectEndSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, "test", isEphemeral: isEphemeral );
            await connectEndSource.Task;

            Assertion.All(
                    server.Clients.Count.TestEquals( 1 ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c => Assertion.All(
                            "remoteClient",
                            c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                            c.IsEphemeral.TestEquals( isEphemeral ),
                            c.Publishers.TryGetByChannelId( 1 )
                                .TestNotNull( p => p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Inactive ) ),
                            c.Queues.Count.TestEquals( 0 ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Start] Client = [1] 'test', TraceId = 6 (start)",
                                $"[ServerTrace] Client = [1] 'test', TraceId = 6, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 6, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 6, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                "[HandshakeEstablished] Client = [1] 'test', TraceId = 6, MessageTimeout = 1 second(s), PingInterval = 10 second(s), BatchPacket = <disabled>",
                                "[Trace:Start] Client = [1] 'test', TraceId = 6 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [1] 'foo', TraceId = 2 (start)",
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 2, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 2, IsAlive = False",
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [1] 'foo', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldReconnectAndNotProcessAnyMessages_WhenClientWithPendingMessagesReconnects()
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

            storage.WriteClientMetadata( clientId: 1, clientName: "test", traceId: 4 );
            storage.WriteListenerMetadata(
                clientId: 1,
                channelId: 1,
                queueId: 1,
                maxRedeliveries: 5,
                minAckTimeout: Duration.FromMilliseconds( 1 ),
                maxRetries: 5,
                retryDelay: Duration.FromMilliseconds( 1 ),
                deadLetterCapacityHint: 5,
                minDeadLetterRetention: Duration.FromMilliseconds( 1 ) );

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
                messages:
                [
                    StorageScope.PrepareQueueDeadLetterMessage(
                        streamId: 1,
                        storeKey: 0,
                        retry: 0,
                        redelivery: 0,
                        expiresAt: TimestampProvider.Shared.GetNow() + Duration.FromMinutes( 1 ) )
                ] );

            var connectEndSource = new SafeTaskCompletionSource();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            var logs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    connectEndSource.Complete();
                            } ) ) )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, "test", isEphemeral: false );
            await connectEndSource.Task;
            await Task.Delay( 15 );

            Assertion.All(
                    server.Clients.Count.TestEquals( 1 ),
                    server.Clients.TryGetById( 1 )
                        .TestNotNull( c => Assertion.All(
                            "remoteClient",
                            c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                            c.IsEphemeral.TestFalse(),
                            c.Listeners.TryGetByChannelId( 1 )
                                .TestNotNull( p => p.State.TestEquals( MessageBrokerChannelListenerBindingState.Inactive ) ),
                            c.Queues.TryGetById( 1 )
                                .TestNotNull( q => Assertion.All(
                                    "queue",
                                    q.State.TestEquals( MessageBrokerQueueState.Running ),
                                    q.Messages.Pending.Count.TestEquals( 1 ),
                                    q.Messages.Pending.TryPeekAt( 0 ).TestNotNull( m => m.StoreKey.TestEquals( 3 ) ),
                                    q.Messages.Unacked.Count.TestEquals( 1 ),
                                    q.Messages.Unacked.TryGetFirst().TestNotNull( m => m.StoreKey.TestEquals( 2 ) ),
                                    q.Messages.Retries.Count.TestEquals( 1 ),
                                    q.Messages.Retries.TryGetNext().TestNotNull( m => m.StoreKey.TestEquals( 1 ) ),
                                    q.Messages.DeadLetter.Count.TestEquals( 1 ),
                                    q.Messages.DeadLetter.TryPeekAt( 0 ).TestNotNull( m => m.StoreKey.TestEquals( 0 ) ) ) ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Start] Client = [1] 'test', TraceId = 6 (start)",
                                $"[ServerTrace] Client = [1] 'test', TraceId = 6, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (HandshakeAcceptedResponse, Length = 32)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 6, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 6, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                "[HandshakeEstablished] Client = [1] 'test', TraceId = 6, MessageTimeout = 1 second(s), PingInterval = 10 second(s), BatchPacket = <disabled>",
                                "[Trace:Start] Client = [1] 'test', TraceId = 6 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll().Skip( 2 ).TestEmpty() )
                .Go();
        }
    }
}
