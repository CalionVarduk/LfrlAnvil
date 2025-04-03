using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerChannelBindingTests : TestsBase
{
    [Fact]
    public async Task Creation_ShouldCreateBindingCorrectly()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.BoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var queue = server.Queues.TryGetByName( "c" );
        var binding = channel?.Bindings.TryGetByClientId( 1 );

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.ToString().TestEquals( "[1] 'c' channel (Running)" ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                        c.Bindings.TryGetByClientId( 1 ).TestRefEquals( binding ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Server.TestRefEquals( server ),
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "c" ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                        q.Bindings.Count.TestEquals( 1 ),
                        q.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                        q.Bindings.TryGetByKey( 1, 1 ).TestRefEquals( binding ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                        c.Bindings.TryGetByChannelId( 1 ).TestRefEquals( binding ) ) ),
                binding.TestNotNull(
                    b => Assertion.All(
                        "binding",
                        b.Channel.TestRefEquals( channel ),
                        b.Queue.TestRefEquals( queue ),
                        b.Client.TestRefEquals( remoteClient ),
                        b.State.TestEquals( MessageBrokerChannelBindingState.Running ),
                        b.ToString().TestEquals( "[1] 'test' => [1] 'c' binding (using [1] 'c' queue) (Running)" ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                server.Queues.Count.TestEquals( 1 ),
                server.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ),
                server.Queues.TryGetById( 1 ).TestRefEquals( queue ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] BoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] BoundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']" ] ),
                logs.GetAllBinding().TestSequence( [ "[1::'test'=>1::'c'::1] [Created]" ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateBindingForExistingChannelCorrectly()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Client.Id == 2
                            && e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.BoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetByName( "c" );
        var queue = server.Queues.TryGetByName( "c" );
        var binding1 = channel?.Bindings.TryGetByClientId( 1 );
        var binding2 = channel?.Bindings.TryGetByClientId( 2 );
        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 2 ),
                        c.Bindings.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                        c.Bindings.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                        c.Bindings.TryGetByClientId( 2 ).TestRefEquals( binding2 ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Server.TestRefEquals( server ),
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "c" ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                        q.Bindings.Count.TestEquals( 2 ),
                        q.Bindings.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                        q.Bindings.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ),
                        q.Bindings.TryGetByKey( 2, 1 ).TestRefEquals( binding2 ) ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                        c.Bindings.TryGetByChannelId( 1 ).TestRefEquals( binding1 ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ),
                        c.Bindings.TryGetByChannelId( 1 ).TestRefEquals( binding2 ) ) ),
                binding1.TestNotNull(
                    b => Assertion.All(
                        "binding1",
                        b.Channel.TestRefEquals( channel ),
                        b.Queue.TestRefEquals( queue ),
                        b.Client.TestRefEquals( remoteClient1 ),
                        b.State.TestEquals( MessageBrokerChannelBindingState.Running ) ) ),
                binding2.TestNotNull(
                    b => Assertion.All(
                        "binding2",
                        b.Channel.TestRefEquals( channel ),
                        b.Queue.TestRefEquals( queue ),
                        b.Client.TestRefEquals( remoteClient2 ),
                        b.State.TestEquals( MessageBrokerChannelBindingState.Running ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                server.Queues.Count.TestEquals( 1 ),
                server.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ),
                server.Queues.TryGetById( 1 ).TestRefEquals( queue ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] BoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] BoundResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest",
                        "[2::'test2'::1] [MessageAccepted] [PacketLength: 11] BindRequest",
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 14] BoundResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 14] BoundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']" ] ),
                logs.GetAllBinding()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[2::'test2'=>1::'c'::1] [Created]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateBindingForExistingQueueCorrectly()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.BoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "d", "c" );
                c.ReadBoundResponse();
            } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel1 = server.Channels.TryGetByName( "c" );
        var channel2 = server.Channels.TryGetByName( "d" );
        var queue = server.Queues.TryGetByName( "c" );
        var binding1 = channel1?.Bindings.TryGetByClientId( 1 );
        var binding2 = channel2?.Bindings.TryGetByClientId( 1 );

        Assertion.All(
                channel1.TestNotNull(
                    c => Assertion.All(
                        "channel1",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.ToString().TestEquals( "[1] 'c' channel (Running)" ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                        c.Bindings.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                channel2.TestNotNull(
                    c => Assertion.All(
                        "channel2",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 2 ),
                        c.Name.TestEquals( "d" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.ToString().TestEquals( "[2] 'd' channel (Running)" ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ),
                        c.Bindings.TryGetByClientId( 1 ).TestRefEquals( binding2 ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Server.TestRefEquals( server ),
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "c" ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                        q.Bindings.Count.TestEquals( 2 ),
                        q.Bindings.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                        q.Bindings.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ),
                        q.Bindings.TryGetByKey( 1, 2 ).TestRefEquals( binding2 ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Bindings.Count.TestEquals( 2 ),
                        c.Bindings.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                        c.Bindings.TryGetByChannelId( 1 ).TestRefEquals( binding1 ),
                        c.Bindings.TryGetByChannelId( 2 ).TestRefEquals( binding2 ) ) ),
                binding1.TestNotNull(
                    b => Assertion.All(
                        "binding1",
                        b.Channel.TestRefEquals( channel1 ),
                        b.Queue.TestRefEquals( queue ),
                        b.Client.TestRefEquals( remoteClient ),
                        b.State.TestEquals( MessageBrokerChannelBindingState.Running ),
                        b.ToString().TestEquals( "[1] 'test' => [1] 'c' binding (using [1] 'c' queue) (Running)" ) ) ),
                binding2.TestNotNull(
                    b => Assertion.All(
                        "binding2",
                        b.Channel.TestRefEquals( channel2 ),
                        b.Queue.TestRefEquals( queue ),
                        b.Client.TestRefEquals( remoteClient ),
                        b.State.TestEquals( MessageBrokerChannelBindingState.Running ),
                        b.ToString().TestEquals( "[1] 'test' => [2] 'd' binding (using [1] 'c' queue) (Running)" ) ) ),
                server.Channels.Count.TestEquals( 2 ),
                server.Channels.GetAll().TestSetEqual( [ channel1, channel2 ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel1 ),
                server.Channels.TryGetById( 2 ).TestRefEquals( channel2 ),
                server.Queues.Count.TestEquals( 1 ),
                server.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ),
                server.Queues.TryGetById( 1 ).TestRefEquals( queue ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] BoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] BoundResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[2::'d'::2] [Created] by client [1::'test']"
                    ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']" ] ),
                logs.GetAllBinding()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>2::'d'::2] [Created]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenChannelBindingEventHandlerFactoryThrows()
    {
        var endSource = new SafeTaskCompletionSource();
        var exception = new Exception( "foo" );
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } )
                .SetChannelBindingEventHandlerFactory( _ => throw exception ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindRequest( "c" ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                server.Queues.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest" ),
                        (m, _) => m.TestStartsWith(
                            """
                            [1::'test'::<ROOT>] [Unexpected] Encountered an error:
                            System.Exception: foo
                            """ ),
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [Disposing]" ),
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [Disposed]" )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenChannelEventHandlerFactoryThrowsForCreatedChannel()
    {
        var endSource = new SafeTaskCompletionSource();
        var exception = new Exception( "foo" );
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => throw exception ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindRequest( "c" ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                server.Queues.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest" ),
                        (m, _) => m.TestStartsWith(
                            """
                            [1::'test'::<ROOT>] [Unexpected] Encountered an error:
                            System.Exception: foo
                            """ ),
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [Disposing]" ),
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [Disposed]" )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenQueueEventHandlerFactoryThrowsForCreatedQueue()
    {
        var endSource = new SafeTaskCompletionSource();
        var exception = new Exception( "foo" );
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } )
                .SetQueueEventHandlerFactory( _ => throw exception ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindRequest( "c" ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                server.Queues.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest" ),
                        (m, _) => m.TestStartsWith(
                            """
                            [1::'test'::<ROOT>] [Unexpected] Encountered an error:
                            System.Exception: foo
                            """ ),
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [Disposing]" ),
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [Disposed]" )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsInvalidPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindRequest( "c", payload: 0 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 5] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 5] Begin handling BindRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected header payload to be at least 5 but found 0.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsNegativeChannelNameLength()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindRequest( "c", channelNameLength: -1 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 11] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected binary channel name length to be in [0, 1] range but found -1.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsTooLongChannelNameLength()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindRequest( "foo", channelNameLength: 4 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 13] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 13] Begin handling BindRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 13] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected binary channel name length to be in [0, 3] range but found 4.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsEmptyChannelName()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindRequest( string.Empty ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                server.Queues.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 10] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 10] Begin handling BindRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 10] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected channel name length to be in [1, 512] range but found 0.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsTooLongChannelName()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindRequest( new string( 'x', 513 ) ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 523] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 523] Begin handling BindRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 523] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected channel name length to be in [1, 512] range but found 513.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsTooLongQueueName()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindRequest( "c", new string( 'x', 513 ) ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 524] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 524] Begin handling BindRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 524] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected queue name length to be in [1, 512] range but found 513.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindRequest_ShouldBeRejected_WhenClientIsAlreadyBoundToChannel()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageRejected )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.BindFailureResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
                c.SendBindRequest( "c", "d" );
                c.ReadBindFailureResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestRefEquals( channel ),
                            exc.Binding.TestRefEquals( binding ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                server.Queues.Count.TestEquals( 1 ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] BoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] BoundResponse",
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 12] BindRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 12] Begin handling BindRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 12] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelBindingException: Message broker client [1] 'test' could not be bound to channel [1] 'c' because it is already bound to it.
                        """,
                        "[1::'test'::2] [SendingMessage] [PacketLength: 6] BindFailureResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 6] BindFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldNotThrow_WhenChannelOrQueueOrChannelBindingEventHandlerThrows()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.BoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => _ => throw new Exception( "foo" ) )
                .SetQueueEventHandlerFactory( _ => _ => throw new Exception( "bar" ) )
                .SetChannelBindingEventHandlerFactory( _ => _ => throw new Exception( "qux" ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetById( 1 );
        var queue = server.Queues.TryGetById( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );

        Assertion.All(
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                server.Queues.Count.TestEquals( 1 ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                binding.TestNotNull(
                    s => Assertion.All(
                        "binding",
                        s.Client.TestRefEquals( remoteClient ),
                        s.Channel.TestRefEquals( channel ),
                        s.Queue.TestRefEquals( queue ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] BoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] BoundResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ServerDispose_ShouldDisposeChannelBinding()
    {
        var logs = new EventLogger();
        var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var queue = server.Queues.TryGetById( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );
        await server.DisposeAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                server.Queues.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All( "client", c.Bindings.Count.TestEquals( 0 ), c.Bindings.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Bindings.Count.TestEquals( 0 ),
                        q.Bindings.GetAll().TestEmpty() ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllBinding()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeChannelBinding_WhenChannelIsOnlyBoundToDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var queue = server.Queues.TryGetById( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                server.Queues.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All( "client", c.Bindings.Count.TestEquals( 0 ), c.Bindings.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Bindings.Count.TestEquals( 0 ),
                        q.Bindings.GetAll().TestEmpty() ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllBinding()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldRemoveChannelBinding_WhenChannelIsAlsoBoundToOtherClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var queue = server.Queues.TryGetById( 1 );
        var binding1 = channel?.Bindings.TryGetByClientId( 1 );
        var binding2 = channel?.Bindings.TryGetByClientId( 2 );
        if ( remoteClient1 is not null )
            await remoteClient1.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 1 ),
                server.Queues.Count.TestEquals( 1 ),
                remoteClient1.TestNotNull(
                    c => Assertion.All( "client1", c.Bindings.Count.TestEquals( 0 ), c.Bindings.GetAll().TestEmpty() ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ) ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.Bindings.Count.TestEquals( 1 ),
                        q.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ) ) ),
                binding1.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']" ] ),
                logs.GetAllBinding()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[2::'test2'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindLastClientFromChannelAndQueueAndRemoveThem()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnboundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var queue = server.Queues.TryGetByName( "c" );
        var binding = channel?.Bindings.TryGetByClientId( 1 );
        await client.GetTask(
            c =>
            {
                c.SendUnbindRequest( 1 );
                c.ReadUnboundResponse();
            } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Bindings.Count.TestEquals( 0 ),
                        q.Bindings.GetAll().TestEmpty() ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty() ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
                server.Channels.Count.TestEquals( 0 ),
                server.Channels.GetAll().TestEmpty(),
                server.Queues.Count.TestEquals( 0 ),
                server.Queues.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 9] Begin handling UnbindRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 9] UnbindRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 6] UnboundResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 6] UnboundResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllBinding()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::2] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindNonLastClientFromChannelAndQueueWithoutRemovingThem()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Client.Id == 2
                            && e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnboundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetByName( "c" );
        var queue = server.Queues.TryGetByName( "c" );
        var binding1 = channel?.Bindings.TryGetByClientId( 1 );
        var binding2 = channel?.Bindings.TryGetByClientId( 2 );

        await client2.GetTask(
            c =>
            {
                c.SendUnbindRequest( 1 );
                c.ReadUnboundResponse();
            } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                        c.Bindings.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                        c.Bindings.TryGetByClientId( 2 ).TestNull() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Server.TestRefEquals( server ),
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "c" ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.Bindings.Count.TestEquals( 1 ),
                        q.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                        q.Bindings.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ),
                        q.Bindings.TryGetByKey( 2, 1 ).TestNull() ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                        c.Bindings.TryGetByChannelId( 1 ).TestRefEquals( binding1 ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty() ) ),
                binding1.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Running ) ),
                binding2.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                server.Queues.Count.TestEquals( 1 ),
                server.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ),
                server.Queues.TryGetById( 1 ).TestRefEquals( queue ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] BoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] BoundResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 11] BindRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 11] Begin handling BindRequest",
                        "[2::'test2'::1] [MessageAccepted] [PacketLength: 11] BindRequest",
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 14] BoundResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 14] BoundResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindRequest",
                        "[2::'test2'::2] [MessageReceived] [PacketLength: 9] Begin handling UnbindRequest",
                        "[2::'test2'::2] [MessageAccepted] [PacketLength: 9] UnbindRequest",
                        "[2::'test2'::2] [SendingMessage] [PacketLength: 6] UnboundResponse",
                        "[2::'test2'::2] [MessageSent] [PacketLength: 6] UnboundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']" ] ),
                logs.GetAllBinding()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[2::'test2'=>1::'c'::1] [Created]",
                        "[2::'test2'=>1::'c'::2] [Disposing]",
                        "[2::'test2'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindLastClientFromChannelWithSubscriptionAndNotRemoveIt()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnboundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
                c.SendSubscribeRequest( "c", createChannelIfNotExists: false );
                c.ReadSubscribedResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );
        await client.GetTask(
            c =>
            {
                c.SendUnbindRequest( 1 );
                c.ReadUnboundResponse();
            } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ),
                server.Channels.Count.TestEquals( 1 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindRequest",
                        "[1::'test'::3] [MessageReceived] [PacketLength: 9] Begin handling UnbindRequest",
                        "[1::'test'::3] [MessageAccepted] [PacketLength: 9] UnbindRequest",
                        "[1::'test'::3] [SendingMessage] [PacketLength: 6] UnboundResponse",
                        "[1::'test'::3] [MessageSent] [PacketLength: 6] UnboundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllBinding()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::3] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldDisposeClient_WhenClientSendsInvalidPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var binding = channel?.Bindings.TryGetByClientId( 1 );
        await client.GetTask( c => c.SendUnbindRequest( 1, payload: 3 ) );
        await endSource.Task;

        Assertion.All(
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Clients.GetAll().TestEmpty(),
                server.Channels.Count.TestEquals( 0 ),
                server.Channels.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 8] UnbindRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 8] Begin handling UnbindRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 8] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid UnbindRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected header payload to be 4 but found 3.
                        """
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllBinding()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldBeRejected_WhenChannelDoesNotExist()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageRejected
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.UnbindRequest )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnbindFailureResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );

        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendUnbindRequest( 1 );
                c.ReadUnbindFailureResponse();
            } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelBindingException>(
                        exc => Assertion.All( exc.Client.TestRefEquals( remoteClient ), exc.Channel.TestNull(), exc.Binding.TestNull() ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( remoteClient ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 9] Begin handling UnbindRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelBindingException: Message broker client [1] 'test' could not be unbound from non-existing channel with ID 1.
                        """,
                        "[1::'test'::1] [SendingMessage] [PacketLength: 6] UnbindFailureResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 6] UnbindFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldBeRejected_WhenClientIsNotBoundToChannel()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageRejected
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.UnbindRequest )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnbindFailureResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendUnbindRequest( 1 );
                c.ReadUnbindFailureResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetByName( "c" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient2 ),
                            exc.Channel.TestRefEquals( channel ),
                            exc.Binding.TestNull() ) ),
                remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 1 ) ) ),
                server.Clients.Count.TestEquals( 2 ),
                server.Clients.GetAll().TestSetEqual( [ remoteClient1, remoteClient2 ] ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( channel ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 9] Begin handling UnbindRequest",
                        """
                        [2::'test2'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelBindingException: Message broker client [2] 'test2' could not be unbound from channel [1] 'c' because it is not bound to it.
                        """,
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 6] UnbindFailureResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 6] UnbindFailureResponse"
                    ] ),
                logs.GetAllChannel().TestContainsSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ) )
            .Go();
    }
}
