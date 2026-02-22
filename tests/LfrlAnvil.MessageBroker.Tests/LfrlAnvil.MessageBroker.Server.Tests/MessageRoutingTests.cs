using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageRoutingTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public MessageRoutingTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task Routing_ShouldBeAccepted_WhenAllTargetsAreValid()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 1
                    ? clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                    endSource.Complete();
                            } ) )
                    : null ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client1.GetTask( c => c.SendPushMessageRouting( [ Routing.FromId( 1 ), Routing.FromName( "test2" ) ] ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "remoteClient",
                    c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                    c.MessageRouting.IsActive.TestTrue(),
                    MessageRouting.Contains( c.MessageRouting.Data.Span, 1 ).TestTrue(),
                    MessageRouting.Contains( c.MessageRouting.Data.Span, 2 ).TestTrue() ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 19)",
                            "[EnqueueingRouting] Client = [1] 'test', TraceId = 1, TargetCount = 2",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 19)",
                            "[RoutingEnqueued] Client = [1] 'test', TraceId = 1, TargetCount = 2, ValidTargetCount = 2",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldBeAccepted_WhenValidTargetCountIsEqualToZero()
    {
        var exceptions = new List<Exception>();
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        },
                        error: e =>
                        {
                            lock ( exceptions )
                                exceptions.Add( e.Exception );
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessageRouting( [ Routing.FromId( 2 ), Routing.FromName( "foo" ) ] ) );
        await endSource.Task;

        Assertion.All(
                exceptions.TestCount( count => count.TestEquals( 2 ) )
                    .Then( exc => exc.TestAll( (x, _) => x.TestType()
                        .Exact<MessageBrokerRemoteClientException>( e => e.Client.TestRefEquals( remoteClient ) ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "remoteClient",
                    c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                    c.MessageRouting.IsActive.TestTrue() ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 17)",
                            "[EnqueueingRouting] Client = [1] 'test', TraceId = 1, TargetCount = 2",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Target client with ID 2 at index 0 could not be found.
                            """,
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Target client with name 'foo' at index 1 could not be found.
                            """,
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 17)",
                            "[RoutingEnqueued] Client = [1] 'test', TraceId = 1, TargetCount = 2, ValidTargetCount = 0",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldDisposeClient_WhenClientSendsInvalidPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessageRouting( [ ], payload: 1 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 6)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessageRouting from client [1] 'test'. Encountered 1 error(s):
                            1. Expected header payload to be at least 2 but found 1.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldDisposeClient_WhenClientSendsTargetCountLessThanOne()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessageRouting( [ ] ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 7)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessageRouting from client [1] 'test'. Encountered 1 error(s):
                            1. Expected target count to be greater than 0 but found 0.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldDisposeClient_WhenClientAlreadyHasRoutingEnqueued()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c =>
        {
            c.SendPushMessageRouting( [ Routing.FromId( 1 ) ] );
            c.SendPushMessageRouting( [ Routing.FromName( "foo" ) ] );
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (PushMessageRouting, Length = 12)",
                            """
                            [Error] Client = [1] 'test', TraceId = 2
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessageRouting from client [1] 'test'. Encountered 1 error(s):
                            1. Message routing is already enqueued.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldDisposeClient_WhenPayloadIsTooShortForId()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessageRouting( [ Routing.FromId( 1 ) ], payload: 6 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 11)",
                            "[EnqueueingRouting] Client = [1] 'test', TraceId = 1, TargetCount = 1",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessageRouting from client [1] 'test'. Encountered 1 error(s):
                            1. Expected packet element length at index 0 to be at least 5 but found 4.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldDisposeClient_WhenPayloadIsTooShortForName()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessageRouting( [ Routing.FromName( "foo" ) ], payload: 6 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 11)",
                            "[EnqueueingRouting] Client = [1] 'test', TraceId = 1, TargetCount = 1",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessageRouting from client [1] 'test'. Encountered 1 error(s):
                            1. Expected packet element length at index 0 to be at least 5 but found 4.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldDisposeClient_WhenAnyIdIsNotPositive()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessageRouting( [ Routing.FromId( 1 ), Routing.FromId( 0 ) ] ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 17)",
                            "[EnqueueingRouting] Client = [1] 'test', TraceId = 1, TargetCount = 2",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessageRouting from client [1] 'test'. Encountered 1 error(s):
                            1. Expected target ID at index 1 to be greater than 0 but found 0.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldDisposeClient_WhenAnyNameIsEmpty()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessageRouting( [ Routing.FromName( "test" ), Routing.FromName( string.Empty ) ] ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 15)",
                            "[EnqueueingRouting] Client = [1] 'test', TraceId = 1, TargetCount = 2",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessageRouting from client [1] 'test'. Encountered 1 error(s):
                            1. Expected target name length at index 1 to be in [1, 512] range but found 0.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldDisposeClient_WhenAnyNameIsTooLong()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessageRouting( [ Routing.FromName( "test" ), Routing.FromName( new string( 'x', 513 ) ) ] ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 528)",
                            "[EnqueueingRouting] Client = [1] 'test', TraceId = 1, TargetCount = 2",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessageRouting from client [1] 'test'. Encountered 1 error(s):
                            1. Expected target name length at index 1 to be in [1, 512] range but found 513.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldDisposeClient_WhenTargetCountIsGreaterThanRouteCount()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessageRouting( [ Routing.FromName( "test" ), Routing.FromId( 1 ) ], targetCount: 3 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 18)",
                            "[EnqueueingRouting] Client = [1] 'test', TraceId = 1, TargetCount = 3",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Target client [1] 'test' at index 1 is a duplicate.
                            """,
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessageRouting from client [1] 'test'. Encountered 1 error(s):
                            1. Target count 3 is larger than actual element count 2.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldDisposeClient_WhenThereIsRemainingDataAfterReadingTotalCountElements()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessageRouting )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessageRouting( [ Routing.FromName( "test" ), Routing.FromId( 1 ) ], targetCount: 1 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessageRouting, Length = 18)",
                            "[EnqueueingRouting] Client = [1] 'test', TraceId = 1, TargetCount = 1",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessageRouting from client [1] 'test'. Encountered 1 error(s):
                            1. Header payload is too large by 5.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Routing_ShouldBeConsumedByNextPushedMessage()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 3 );
        var clientLogs = new ClientEventLogger();
        var streamLogs = new StreamEventLogger();
        var queueLogs = new QueueEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 1
                    ? clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            } ) )
                    : MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                endSource.Complete();
                        } ) )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger(
                    MessageBrokerStreamLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerStreamTraceEventType.ProcessMessage )
                                endSource.Complete();
                        } ) ) )
                .SetQueueLoggerFactory( q => q.Client.Id == 1 ? queueLogs.GetLogger() : null ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client2.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", true );
            c.ReadListenerBoundResponse();
        } );

        await client1.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
            c.SendBindListenerRequest( "c", true );
            c.ReadListenerBoundResponse();
            c.SendPushMessageRouting( [ Routing.FromId( 2 ) ] );
            c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
        } );

        await client2.GetTask( c => c.ReadMessageNotification( 3 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "remoteClient",
                    c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                    c.MessageRouting.IsActive.TestFalse() ) ),
                clientLogs.GetAll()
                    .TakeLast( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (PushMessageRouting, Length = 12)",
                            "[EnqueueingRouting] Client = [1] 'test', TraceId = 3, TargetCount = 1",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (PushMessageRouting, Length = 12)",
                            "[RoutingEnqueued] Client = [1] 'test', TraceId = 3, TargetCount = 1, ValidTargetCount = 1",
                            "[Trace:PushMessageRouting] Client = [1] 'test', TraceId = 3 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (PushMessage, Length = 13)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 4, Length = 3, ChannelId = 1, Confirm = False",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (PushMessage, Length = 13)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 4, Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, RoutingTraceId = 3",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Stream = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 4)",
                            "[MessagePushed] Stream = [1] 'c', TraceId = 1, Client = [1] 'test', Channel = [1] 'c', MessageId = 0, StoreKey = 0, Length = 3",
                            "[Trace:PushMessage] Stream = [1] 'c', TraceId = 1 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:ProcessMessage] Stream = [1] 'c', TraceId = 2 (start)",
                            "[ProcessingMessage] Stream = [1] 'c', TraceId = 2, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, Length = 3, HasRouting = True, ListenerCount = 2",
                            "[MessageProcessed] Stream = [1] 'c', TraceId = 2, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, Failures = 0, Filtered = 1",
                            "[Trace:ProcessMessage] Stream = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll().Where( t => t.Logs.Any( l => l.Contains( "[Trace:EnqueueMessage]" ) ) ).TestEmpty() )
            .Go();
    }
}
