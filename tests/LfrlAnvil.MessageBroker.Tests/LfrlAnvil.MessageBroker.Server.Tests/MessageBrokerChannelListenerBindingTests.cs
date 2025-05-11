using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerChannelListenerBindingTests : TestsBase
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.ListenerBoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var queue = remoteClient?.Queues.TryGetByName( "c" );
        var binding = channel?.Listeners.TryGetByClientId( 1 );

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty(),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ) ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Client.TestRefEquals( remoteClient ),
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "c" ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                        q.Listeners.Count.TestEquals( 1 ),
                        q.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ),
                        q.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty(),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ),
                        c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ),
                        c.Queues.TryGetById( 1 ).TestRefEquals( queue ) ) ),
                binding.TestNotNull(
                    s => Assertion.All(
                        "binding",
                        s.Channel.TestRefEquals( channel ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.PrefetchHint.TestEquals( 1 ),
                        s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                        s.ToString().TestEquals( "[1] 'test' => [1] 'c' listener binding (using [1] 'c' queue) (Running)" ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] ListenerBoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] ListenerBoundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'test'::'c'::1] [Created] by listener to [1::'c']" ] ),
                logs.GetAllListener().TestSequence( [ "[1::'test'=>1::'c'::1] [Created]" ] ) )
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
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.ListenerBoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest( "c", createChannelIfNotExists: false, prefetchHint: 10 );
                c.ReadListenerBoundResponse();
            } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var queue = remoteClient?.Queues.TryGetByName( "c" );
        var binding = channel?.Listeners.TryGetByClientId( 1 );

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ) ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Client.TestRefEquals( remoteClient ),
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "c" ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                        q.Listeners.Count.TestEquals( 1 ),
                        q.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ),
                        q.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ),
                        c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ),
                        c.Queues.TryGetById( 1 ).TestRefEquals( queue ) ) ),
                binding.TestNotNull(
                    s => Assertion.All(
                        "binding",
                        s.Channel.TestRefEquals( channel ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.PrefetchHint.TestEquals( 10 ),
                        s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 14] ListenerBoundResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 14] ListenerBoundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'test'::'c'::2] [Created] by listener to [1::'c']" ] ),
                logs.GetAllListener().TestSequence( [ "[1::'test'=>1::'c'::2] [Created]" ] ) )
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.ListenerBoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
            } );

        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "d", true, null, "c" );
                c.ReadListenerBoundResponse();
            } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel1 = server.Channels.TryGetByName( "c" );
        var channel2 = server.Channels.TryGetByName( "d" );
        var queue = remoteClient?.Queues.TryGetByName( "c" );
        var binding1 = channel1?.Listeners.TryGetByClientId( 1 );
        var binding2 = channel2?.Listeners.TryGetByClientId( 1 );

        Assertion.All(
                channel1.TestNotNull(
                    c => Assertion.All(
                        "channel1",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.ToString().TestEquals( "[1] 'c' channel (Running)" ),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ),
                        c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding1 ) ) ),
                channel2.TestNotNull(
                    c => Assertion.All(
                        "channel2",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 2 ),
                        c.Name.TestEquals( "d" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.ToString().TestEquals( "[2] 'd' channel (Running)" ),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding2 ) ] ),
                        c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding2 ) ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Client.TestRefEquals( remoteClient ),
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "c" ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                        q.Listeners.Count.TestEquals( 2 ),
                        q.Listeners.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                        q.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding1 ),
                        q.Listeners.TryGetByChannelId( 2 ).TestRefEquals( binding2 ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 2 ),
                        c.Listeners.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                        c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding1 ),
                        c.Listeners.TryGetByChannelId( 2 ).TestRefEquals( binding2 ) ) ),
                binding1.TestNotNull(
                    s => Assertion.All(
                        "binding1",
                        s.Channel.TestRefEquals( channel1 ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                        s.ToString().TestEquals( "[1] 'test' => [1] 'c' listener binding (using [1] 'c' queue) (Running)" ) ) ),
                binding2.TestNotNull(
                    s => Assertion.All(
                        "binding2",
                        s.Channel.TestRefEquals( channel2 ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                        s.ToString().TestEquals( "[1] 'test' => [2] 'd' listener binding (using [1] 'c' queue) (Running)" ) ) ),
                server.Channels.Count.TestEquals( 2 ),
                server.Channels.GetAll().TestSetEqual( [ channel1, channel2 ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel1 ),
                server.Channels.TryGetById( 2 ).TestRefEquals( channel2 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 16] BindListenerRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 16] Begin handling BindListenerRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 16] BindListenerRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 14] ListenerBoundResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 14] ListenerBoundResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[2::'d'::2] [Created] by client [1::'test']"
                    ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'test'::'c'::1] [Created] by listener to [1::'c']" ] ),
                logs.GetAllListener()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>2::'d'::2] [Created]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenListenerEventHandlerFactoryThrows()
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
                .SetListenerEventHandlerFactory( _ => throw exception ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest" ),
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
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest" ),
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
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest" ),
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
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true, payload: 8 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 13] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 13] Begin handling BindListenerRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 13] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected header payload to be at least 9 but found 8.
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
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true, channelNameLength: -1 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 15] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendBindListenerRequest( "foo", createChannelIfNotExists: true, channelNameLength: 4 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 17] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 17] Begin handling BindListenerRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 17] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendBindListenerRequest( string.Empty, createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 14] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 14] Begin handling BindListenerRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 14] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendBindListenerRequest( new string( 'x', 513 ), createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 527] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 527] Begin handling BindListenerRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 527] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true, queueName: new string( 'x', 513 ) ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 528] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 528] Begin handling BindListenerRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 528] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected queue name length to be in [1, 512] range but found 513.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsInvalidPrefetchHint()
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
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true, prefetchHint: 0 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 15] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected prefetch hint to be greater than 0 but found 0.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindListenerRequest_ShouldBeRejected_WhenClientIsAlreadyBoundAsListenerToChannel()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.BindListenerFailureResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
                c.SendBindListenerRequest( "c", createChannelIfNotExists: false );
                c.ReadBindListenerFailureResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Listeners.TryGetByClientId( 1 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelListenerBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestRefEquals( channel ),
                            exc.Listener.TestRefEquals( binding ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] ListenerBoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] ListenerBoundResponse",
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 15] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelListenerBindingException: Message broker client [1] 'test' could not be bound as a listener to channel [1] 'c' because it is already bound as a listener to it.
                        """,
                        "[1::'test'::2] [SendingMessage] [PacketLength: 6] BindListenerFailureResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 6] BindListenerFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindListenerRequest_ShouldBeRejected_WhenChannelDoesNotExistAndIsNotCreatedDuringListenerBinding()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.BindListenerFailureResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: false );
                c.ReadBindListenerFailureResponse();
            } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelListenerBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestNull(),
                            exc.Listener.TestNull() ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 15] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelListenerBindingException: Message broker client [1] 'test' could not be bound as a listener to channel 'c' because channel does not exist.
                        """,
                        "[1::'test'::1] [SendingMessage] [PacketLength: 6] BindListenerFailureResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 6] BindListenerFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldNotThrow_WhenChannelOrQueueOrListenerEventHandlerThrows()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.ListenerBoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => _ => throw new Exception( "foo" ) )
                .SetQueueEventHandlerFactory( _ => _ => throw new Exception( "bar" ) )
                .SetPublisherEventHandlerFactory( _ => _ => throw new Exception( "qux" ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Listeners.TryGetByClientId( 1 );
        var queue = binding?.Queue;

        Assertion.All(
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (cl, _) => cl.TestRefEquals( binding ) ] ) ) ),
                binding.TestNotNull(
                    s => Assertion.All(
                        "binding",
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.Channel.TestRefEquals( channel ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] ListenerBoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] ListenerBoundResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ServerDispose_ShouldDisposeBinding()
    {
        var logs = new EventLogger();
        var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Listeners.TryGetByClientId( 1 );
        var queue = binding?.Queue;
        await server.DisposeAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Listeners.Count.TestEquals( 0 ),
                        q.Listeners.GetAll().TestEmpty() ) ),
                binding.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
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
                        "[1::'test'::'c'::1] [Created] by listener to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [Disposing]",
                        "[1::'test'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllListener()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeChannelAndBinding_WhenChannelIsOnlyBoundAsPublisherAndListenerToDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest( "c", createChannelIfNotExists: false );
                c.ReadListenerBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var listenerBinding = channel?.Listeners.TryGetByClientId( 1 );
        var queue = listenerBinding?.Queue;
        var publisherBinding = channel?.Publishers.TryGetByClientId( 1 );
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty(),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty(),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Listeners.Count.TestEquals( 0 ),
                        q.Listeners.GetAll().TestEmpty() ) ),
                listenerBinding.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                publisherBinding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
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
                        "[1::'test'::'c'::2] [Created] by listener to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [Disposing]",
                        "[1::'test'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllListener()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::2] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task
        ClientDisconnect_ShouldDisposeBinding_WhenChannelIsBoundAsPublisherToAnotherClientAndBoundAsListenerToDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: false );
                c.ReadListenerBoundResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var publisherBinding = channel?.Publishers.TryGetByClientId( 1 );
        var listenerBinding = channel?.Listeners.TryGetByClientId( 2 );
        var queue = listenerBinding?.Queue;
        if ( remoteClient2 is not null )
            await remoteClient2.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 1 ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( publisherBinding ) ] ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( publisherBinding ) ] ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Listeners.Count.TestEquals( 0 ),
                        q.Listeners.GetAll().TestEmpty() ) ),
                listenerBinding.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[2::'test2'::'c'::1] [Created] by listener to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [Disposing]",
                        "[2::'test2'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllListener()
                    .TestSequence(
                    [
                        "[2::'test2'=>1::'c'::1] [Created]",
                        "[2::'test2'=>1::'c'::<ROOT>] [Disposing]",
                        "[2::'test2'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task
        ClientDisconnect_ShouldDisposeChannelAndBinding_WhenChannelIsNotBoundAsPublisherToAnyClientAndBoundAsListenerOnlyToDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Listeners.TryGetByClientId( 1 );
        var queue = binding?.Queue;
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty(),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty(),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Listeners.Count.TestEquals( 0 ),
                        q.Listeners.GetAll().TestEmpty() ) ),
                binding.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
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
                        "[1::'test'::'c'::1] [Created] by listener to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [Disposing]",
                        "[1::'test'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllListener()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeBinding_WhenChannelIsNotBoundAsPublisherToAnyClientAndBoundAsListenerToAnotherClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: false );
                c.ReadListenerBoundResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var binding1 = channel?.Listeners.TryGetByClientId( 1 );
        var binding2 = channel?.Listeners.TryGetByClientId( 2 );
        var queue1 = binding1?.Queue;
        var queue2 = binding2?.Queue;
        if ( remoteClient2 is not null )
            await remoteClient2.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 1 ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( queue1 ) ] ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ),
                        c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                        c.Listeners.TryGetByClientId( 2 ).TestNull() ) ),
                queue1.TestNotNull(
                    q => Assertion.All(
                        "queue1",
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.Listeners.Count.TestEquals( 1 ),
                        q.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ) ) ),
                queue2.TestNotNull(
                    q => Assertion.All(
                        "queue2",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Listeners.Count.TestEquals( 0 ),
                        q.Listeners.GetAll().TestEmpty() ) ),
                binding1.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ) ),
                binding2.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'test'::'c'::1] [Created] by listener to [1::'c']",
                        "[2::'test2'::'c'::1] [Created] by listener to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [Disposing]",
                        "[2::'test2'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllListener()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[2::'test2'=>1::'c'::1] [Created]",
                        "[2::'test2'=>1::'c'::<ROOT>] [Disposing]",
                        "[2::'test2'=>1::'c'::<ROOT>] [Disposed]"
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.ListenerUnboundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var binding = channel?.Listeners.TryGetByClientId( 1 );
        var queue = binding?.Queue;
        await client.GetTask(
            c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q =>
                        Assertion.All(
                            "queue",
                            q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                            q.Listeners.Count.TestEquals( 0 ),
                            q.Listeners.GetAll().TestEmpty() ) ),
                binding.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                server.Channels.Count.TestEquals( 0 ),
                server.Channels.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindListenerRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 9] Begin handling UnbindListenerRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 9] UnbindListenerRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 6] ListenerUnboundResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 6] ListenerUnboundResponse"
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
                        "[1::'test'::'c'::1] [Created] by listener to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [Disposing]",
                        "[1::'test'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllListener()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::2] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindNonLastClientFromChannelWithoutRemovingIt()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.ListenerUnboundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: false );
                c.ReadListenerBoundResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetByName( "c" );
        var binding1 = channel?.Listeners.TryGetByClientId( 1 );
        var binding2 = channel?.Listeners.TryGetByClientId( 2 );
        var queue1 = binding1?.Queue;
        var queue2 = binding2?.Queue;

        await client2.GetTask(
            c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
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
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ),
                        c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                        c.Listeners.TryGetByClientId( 2 ).TestNull() ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ),
                        c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding1 ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue1 ) ] ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                queue1.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ),
                queue2.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Disposed ) ),
                binding1.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ) ),
                binding2.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 15] BindListenerRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] ListenerBoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] ListenerBoundResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 15] BindListenerRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 15] Begin handling BindListenerRequest",
                        "[2::'test2'::1] [MessageAccepted] [PacketLength: 15] BindListenerRequest",
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 14] ListenerBoundResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 14] ListenerBoundResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindListenerRequest",
                        "[2::'test2'::2] [MessageReceived] [PacketLength: 9] Begin handling UnbindListenerRequest",
                        "[2::'test2'::2] [MessageAccepted] [PacketLength: 9] UnbindListenerRequest",
                        "[2::'test2'::2] [SendingMessage] [PacketLength: 6] ListenerUnboundResponse",
                        "[2::'test2'::2] [MessageSent] [PacketLength: 6] ListenerUnboundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'test'::'c'::1] [Created] by listener to [1::'c']",
                        "[2::'test2'::'c'::1] [Created] by listener to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [Disposing]",
                        "[2::'test2'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllListener()
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
    public async Task Unbind_ShouldUnregisterNonLastBindingFromQueueAndNotRemoveIt()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.ListenerUnboundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
                c.SendBindListenerRequest( "d", createChannelIfNotExists: true, queueName: "c" );
                c.ReadListenerBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding1 = remoteClient?.Listeners.TryGetByChannelId( 1 );
        var binding2 = remoteClient?.Listeners.TryGetByChannelId( 2 );
        var queue1 = binding1?.Queue;
        var queue2 = binding2?.Queue;
        await client.GetTask(
            c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

        await endSource.Task;

        Assertion.All(
                queue1.TestRefEquals( queue2 ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding2 ) ] ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue1 ) ] ) ) ),
                queue1.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.Listeners.Count.TestEquals( 1 ),
                        q.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding2 ) ] ) ) ),
                binding1.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                binding2.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ) ),
                server.Channels.Count.TestEquals( 1 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindListenerRequest",
                        "[1::'test'::3] [MessageReceived] [PacketLength: 9] Begin handling UnbindListenerRequest",
                        "[1::'test'::3] [MessageAccepted] [PacketLength: 9] UnbindListenerRequest",
                        "[1::'test'::3] [SendingMessage] [PacketLength: 6] ListenerUnboundResponse",
                        "[1::'test'::3] [MessageSent] [PacketLength: 6] ListenerUnboundResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[2::'d'::2] [Created] by client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'test'::'c'::1] [Created] by listener to [1::'c']" ] ),
                logs.GetAllListener()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>2::'d'::2] [Created]",
                        "[1::'test'=>1::'c'::3] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindLastClientFromChannelWithPublisherBindingAndNotRemoveIt()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.ListenerUnboundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest( "c", createChannelIfNotExists: false );
                c.ReadListenerBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var publisherBinding = channel?.Publishers.TryGetByClientId( 1 );
        var listenerBinding = channel?.Listeners.TryGetByClientId( 1 );
        var queue = listenerBinding?.Queue;
        await client.GetTask(
            c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( publisherBinding ) ] ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( publisherBinding ) ] ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Listeners.Count.TestEquals( 0 ),
                        q.Listeners.GetAll().TestEmpty() ) ),
                listenerBinding.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                publisherBinding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ) ),
                server.Channels.Count.TestEquals( 1 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindListenerRequest",
                        "[1::'test'::3] [MessageReceived] [PacketLength: 9] Begin handling UnbindListenerRequest",
                        "[1::'test'::3] [MessageAccepted] [PacketLength: 9] UnbindListenerRequest",
                        "[1::'test'::3] [SendingMessage] [PacketLength: 6] ListenerUnboundResponse",
                        "[1::'test'::3] [MessageSent] [PacketLength: 6] ListenerUnboundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'test'::'c'::2] [Created] by listener to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [Disposing]",
                        "[1::'test'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllListener()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::2] [Created]",
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
                .SetListenerEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var subscription = channel?.Listeners.TryGetByClientId( 1 );
        await client.GetTask( c => c.SendUnbindListenerRequest( 1, payload: 3 ) );
        await endSource.Task;

        Assertion.All(
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Clients.GetAll().TestEmpty(),
                server.Channels.Count.TestEquals( 0 ),
                server.Channels.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 8] UnbindListenerRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 8] Begin handling UnbindListenerRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 8] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid UnbindListenerRequest from client [1] 'test'. Encountered 1 error(s):
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
                logs.GetAllListener()
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
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.UnbindListenerRequest )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnbindListenerFailureResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );

        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadUnbindListenerFailureResponse();
            } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelListenerBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestNull(),
                            exc.Listener.TestNull() ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( remoteClient ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindListenerRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 9] Begin handling UnbindListenerRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelListenerBindingException: Message broker client [1] 'test' could not be unbound as a listener from non-existing channel with ID 1.
                        """,
                        "[1::'test'::1] [SendingMessage] [PacketLength: 6] UnbindListenerFailureResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 6] UnbindListenerFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldBeRejected_WhenClientIsNotBoundAsListenerToChannel()
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
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.UnbindListenerRequest )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnbindListenerFailureResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadUnbindListenerFailureResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetByName( "c" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelListenerBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient2 ),
                            exc.Channel.TestRefEquals( channel ),
                            exc.Listener.TestNull() ) ),
                remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Listeners.Count.TestEquals( 1 ) ) ),
                server.Clients.Count.TestEquals( 2 ),
                server.Clients.GetAll().TestSetEqual( [ remoteClient1, remoteClient2 ] ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( channel ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindListenerRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 9] Begin handling UnbindListenerRequest",
                        """
                        [2::'test2'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelListenerBindingException: Message broker client [2] 'test2' could not be unbound as a listener from channel [1] 'c' because it is not bound as a listener to it.
                        """,
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 6] UnbindListenerFailureResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 6] UnbindListenerFailureResponse"
                    ] ),
                logs.GetAllChannel().TestContainsSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ) )
            .Go();
    }
}
