using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerSubscriptionTests : TestsBase
{
    [Fact]
    public async Task Creation_ShouldCreateSubscriptionCorrectly()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.SubscribedResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
            } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var queue = remoteClient?.Queues.TryGetByName( "c" );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Client.TestRefEquals( remoteClient ),
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "c" ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                        q.Subscriptions.Count.TestEquals( 1 ),
                        q.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ),
                        q.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ),
                        c.Queues.TryGetById( 1 ).TestRefEquals( queue ) ) ),
                subscription.TestNotNull(
                    s => Assertion.All(
                        "subscription",
                        s.Channel.TestRefEquals( channel ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.PrefetchHint.TestEquals( 1 ),
                        s.State.TestEquals( MessageBrokerSubscriptionState.Running ),
                        s.ToString().TestEquals( "[1] 'test' => [1] 'c' subscription (using [1] 'c' queue) (Running)" ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] SubscribedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] SubscribedResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'test'::'c'::1] [Created] by subscription to [1::'c']" ] ),
                logs.GetAllSubscription().TestSequence( [ "[1::'test'=>1::'c'::1] [Created]" ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateSubscriptionForExistingChannelCorrectly()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.SubscribedResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
                c.SendSubscribeRequest( "c", createChannelIfNotExists: false, prefetchHint: 10 );
                c.ReadSubscribedResponse();
            } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var queue = remoteClient?.Queues.TryGetByName( "c" );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Client.TestRefEquals( remoteClient ),
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "c" ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                        q.Subscriptions.Count.TestEquals( 1 ),
                        q.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ),
                        q.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ),
                        c.Queues.TryGetById( 1 ).TestRefEquals( queue ) ) ),
                subscription.TestNotNull(
                    s => Assertion.All(
                        "subscription",
                        s.Channel.TestRefEquals( channel ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.PrefetchHint.TestEquals( 10 ),
                        s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 14] SubscribedResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 14] SubscribedResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'test'::'c'::2] [Created] by subscription to [1::'c']" ] ),
                logs.GetAllSubscription().TestSequence( [ "[1::'test'=>1::'c'::2] [Created]" ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateSubscriptionForExistingQueueCorrectly()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.SubscribedResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", true );
                c.ReadSubscribedResponse();
            } );

        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "d", true, null, "c" );
                c.ReadSubscribedResponse();
            } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel1 = server.Channels.TryGetByName( "c" );
        var channel2 = server.Channels.TryGetByName( "d" );
        var queue = remoteClient?.Queues.TryGetByName( "c" );
        var subscription1 = channel1?.Subscriptions.TryGetByClientId( 1 );
        var subscription2 = channel2?.Subscriptions.TryGetByClientId( 1 );

        Assertion.All(
                channel1.TestNotNull(
                    c => Assertion.All(
                        "channel1",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.ToString().TestEquals( "[1] 'c' channel (Running)" ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription1 ) ] ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription1 ) ) ),
                channel2.TestNotNull(
                    c => Assertion.All(
                        "channel2",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 2 ),
                        c.Name.TestEquals( "d" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.ToString().TestEquals( "[2] 'd' channel (Running)" ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription2 ) ] ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription2 ) ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Client.TestRefEquals( remoteClient ),
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "c" ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                        q.Subscriptions.Count.TestEquals( 2 ),
                        q.Subscriptions.GetAll().TestSetEqual( [ subscription1, subscription2 ] ),
                        q.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription1 ),
                        q.Subscriptions.TryGetByChannelId( 2 ).TestRefEquals( subscription2 ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Subscriptions.Count.TestEquals( 2 ),
                        c.Subscriptions.GetAll().TestSetEqual( [ subscription1, subscription2 ] ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription1 ),
                        c.Subscriptions.TryGetByChannelId( 2 ).TestRefEquals( subscription2 ) ) ),
                subscription1.TestNotNull(
                    s => Assertion.All(
                        "subscription1",
                        s.Channel.TestRefEquals( channel1 ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.State.TestEquals( MessageBrokerSubscriptionState.Running ),
                        s.ToString().TestEquals( "[1] 'test' => [1] 'c' subscription (using [1] 'c' queue) (Running)" ) ) ),
                subscription2.TestNotNull(
                    s => Assertion.All(
                        "subscription2",
                        s.Channel.TestRefEquals( channel2 ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.State.TestEquals( MessageBrokerSubscriptionState.Running ),
                        s.ToString().TestEquals( "[1] 'test' => [2] 'd' subscription (using [1] 'c' queue) (Running)" ) ) ),
                server.Channels.Count.TestEquals( 2 ),
                server.Channels.GetAll().TestSetEqual( [ channel1, channel2 ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel1 ),
                server.Channels.TryGetById( 2 ).TestRefEquals( channel2 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 16] SubscribeRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 16] Begin handling SubscribeRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 16] SubscribeRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 14] SubscribedResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 14] SubscribedResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[2::'d'::2] [Created] by client [1::'test']"
                    ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'test'::'c'::1] [Created] by subscription to [1::'c']" ] ),
                logs.GetAllSubscription()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>2::'d'::2] [Created]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenSubscriptionEventHandlerFactoryThrows()
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
                .SetSubscriptionEventHandlerFactory( _ => throw exception ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendSubscribeRequest( "c", createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest" ),
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
        await client.GetTask( c => c.SendSubscribeRequest( "c", createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest" ),
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
        await client.GetTask( c => c.SendSubscribeRequest( "c", createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest" ),
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
        await client.GetTask( c => c.SendSubscribeRequest( "c", createChannelIfNotExists: true, payload: 8 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 13] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 13] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 13] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid SubscribeRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendSubscribeRequest( "c", createChannelIfNotExists: true, channelNameLength: -1 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 15] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid SubscribeRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendSubscribeRequest( "foo", createChannelIfNotExists: true, channelNameLength: 4 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 17] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 17] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 17] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid SubscribeRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendSubscribeRequest( string.Empty, createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 14] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 14] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 14] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid SubscribeRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendSubscribeRequest( new string( 'x', 513 ), createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 527] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 527] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 527] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid SubscribeRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendSubscribeRequest( "c", createChannelIfNotExists: true, queueName: new string( 'x', 513 ) ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 528] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 528] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 528] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid SubscribeRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendSubscribeRequest( "c", createChannelIfNotExists: true, prefetchHint: 0 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 15] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid SubscribeRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected prefetch hint to be greater than 0 but found 0.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task SubscribeRequest_ShouldBeRejected_WhenClientIsAlreadySubscribedToChannel()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.SubscribeFailureResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
                c.SendSubscribeRequest( "c", createChannelIfNotExists: false );
                c.ReadSubscribeFailureResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerSubscriptionException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestRefEquals( channel ),
                            exc.Subscription.TestRefEquals( subscription ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] SubscribedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] SubscribedResponse",
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 15] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerSubscriptionException: Message broker client [1] 'test' failed to create a subscription to channel [1] 'c' because it is already subscribed to it.
                        """,
                        "[1::'test'::2] [SendingMessage] [PacketLength: 6] SubscribeFailureResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 6] SubscribeFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task SubscribeRequest_ShouldBeRejected_WhenChannelDoesNotExistAndIsNotCreatedDuringSubscription()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.SubscribeFailureResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: false );
                c.ReadSubscribeFailureResponse();
            } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerSubscriptionException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestNull(),
                            exc.Subscription.TestNull() ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 15] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerSubscriptionException: Message broker client [1] 'test' failed to create a subscription to channel 'c' because channel does not exist.
                        """,
                        "[1::'test'::1] [SendingMessage] [PacketLength: 6] SubscribeFailureResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 6] SubscribeFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldNotThrow_WhenChannelOrQueueOrSubscriptionEventHandlerThrows()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.SubscribedResponse )
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
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
        var queue = subscription?.Queue;

        Assertion.All(
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (cl, _) => cl.TestRefEquals( subscription ) ] ) ) ),
                subscription.TestNotNull(
                    s => Assertion.All(
                        "subscription",
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.Channel.TestRefEquals( channel ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] SubscribedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] SubscribedResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ServerDispose_ShouldDisposeSubscription()
    {
        var logs = new EventLogger();
        var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
        var queue = subscription?.Queue;
        await server.DisposeAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Subscriptions.Count.TestEquals( 0 ),
                        q.Subscriptions.GetAll().TestEmpty() ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
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
                        "[1::'test'::'c'::1] [Created] by subscription to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [Disposing]",
                        "[1::'test'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllSubscription()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeChannelAndSubscription_WhenChannelIsOnlyBoundToAndSubscribedByDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

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
        var channel = server.Channels.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
        var queue = subscription?.Queue;
        var binding = channel?.Bindings.TryGetByClientId( 1 );
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Subscriptions.Count.TestEquals( 0 ),
                        q.Subscriptions.GetAll().TestEmpty() ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
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
                        "[1::'test'::'c'::2] [Created] by subscription to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [Disposing]",
                        "[1::'test'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllSubscription()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::2] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeSubscription_WhenChannelIsBoundToAnotherClientAndSubscribedByDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

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
                c.SendSubscribeRequest( "c", createChannelIfNotExists: false );
                c.ReadSubscribedResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 2 );
        var queue = subscription?.Queue;
        if ( remoteClient2 is not null )
            await remoteClient2.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 1 ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Subscriptions.Count.TestEquals( 0 ),
                        q.Subscriptions.GetAll().TestEmpty() ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[2::'test2'::'c'::1] [Created] by subscription to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [Disposing]",
                        "[2::'test2'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllSubscription()
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
        ClientDisconnect_ShouldDisposeChannelAndSubscription_WhenChannelIsNotBoundToAnyClientAndSubscribedOnlyByDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
        var queue = subscription?.Queue;
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Subscriptions.Count.TestEquals( 0 ),
                        q.Subscriptions.GetAll().TestEmpty() ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
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
                        "[1::'test'::'c'::1] [Created] by subscription to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [Disposing]",
                        "[1::'test'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllSubscription()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeSubscription_WhenChannelIsNotBoundToAnyClientAndSubscribedByAnotherClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: false );
                c.ReadSubscribedResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var subscription1 = channel?.Subscriptions.TryGetByClientId( 1 );
        var subscription2 = channel?.Subscriptions.TryGetByClientId( 2 );
        var queue1 = subscription1?.Queue;
        var queue2 = subscription2?.Queue;
        if ( remoteClient2 is not null )
            await remoteClient2.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 1 ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription1 ) ] ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( queue1 ) ] ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription1 ) ] ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription1 ),
                        c.Subscriptions.TryGetByClientId( 2 ).TestNull() ) ),
                queue1.TestNotNull(
                    q => Assertion.All(
                        "queue1",
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.Subscriptions.Count.TestEquals( 1 ),
                        q.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription1 ) ] ) ) ),
                queue2.TestNotNull(
                    q => Assertion.All(
                        "queue2",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Subscriptions.Count.TestEquals( 0 ),
                        q.Subscriptions.GetAll().TestEmpty() ) ),
                subscription1.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ),
                subscription2.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'test'::'c'::1] [Created] by subscription to [1::'c']",
                        "[2::'test2'::'c'::1] [Created] by subscription to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [Disposing]",
                        "[2::'test2'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllSubscription()
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
    public async Task Unsubscribe_ShouldUnsubscribeLastClientFromChannelAndQueueAndRemoveThem()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnsubscribedResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
        var queue = subscription?.Queue;
        await client.GetTask(
            c =>
            {
                c.SendUnsubscribeRequest( 1 );
                c.ReadUnsubscribedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q =>
                        Assertion.All(
                            "queue",
                            q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                            q.Subscriptions.Count.TestEquals( 0 ),
                            q.Subscriptions.GetAll().TestEmpty() ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                server.Channels.Count.TestEquals( 0 ),
                server.Channels.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnsubscribeRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 9] Begin handling UnsubscribeRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 9] UnsubscribeRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 6] UnsubscribedResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 6] UnsubscribedResponse"
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
                        "[1::'test'::'c'::1] [Created] by subscription to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [Disposing]",
                        "[1::'test'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllSubscription()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::2] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unsubscribe_ShouldUnsubscribeNonLastClientFromChannelWithoutRemovingIt()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnsubscribedResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: false );
                c.ReadSubscribedResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetByName( "c" );
        var subscription1 = channel?.Subscriptions.TryGetByClientId( 1 );
        var subscription2 = channel?.Subscriptions.TryGetByClientId( 2 );
        var queue1 = subscription1?.Queue;
        var queue2 = subscription2?.Queue;

        await client2.GetTask(
            c =>
            {
                c.SendUnsubscribeRequest( 1 );
                c.ReadUnsubscribedResponse();
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
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription1 ) ] ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription1 ),
                        c.Subscriptions.TryGetByClientId( 2 ).TestNull() ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription1 ) ] ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription1 ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue1 ) ] ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                queue1.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ),
                queue2.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Disposed ) ),
                subscription1.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ),
                subscription2.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 15] SubscribeRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] SubscribedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] SubscribedResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 15] SubscribeRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 15] Begin handling SubscribeRequest",
                        "[2::'test2'::1] [MessageAccepted] [PacketLength: 15] SubscribeRequest",
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 14] SubscribedResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 14] SubscribedResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnsubscribeRequest",
                        "[2::'test2'::2] [MessageReceived] [PacketLength: 9] Begin handling UnsubscribeRequest",
                        "[2::'test2'::2] [MessageAccepted] [PacketLength: 9] UnsubscribeRequest",
                        "[2::'test2'::2] [SendingMessage] [PacketLength: 6] UnsubscribedResponse",
                        "[2::'test2'::2] [MessageSent] [PacketLength: 6] UnsubscribedResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'test'::'c'::1] [Created] by subscription to [1::'c']",
                        "[2::'test2'::'c'::1] [Created] by subscription to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [Disposing]",
                        "[2::'test2'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllSubscription()
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
    public async Task Unsubscribe_ShouldUnregisterNonLastClientFromQueueAndNotRemoveIt()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnsubscribedResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
                c.SendSubscribeRequest( "d", createChannelIfNotExists: true, queueName: "c" );
                c.ReadSubscribedResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var subscription1 = remoteClient?.Subscriptions.TryGetByChannelId( 1 );
        var subscription2 = remoteClient?.Subscriptions.TryGetByChannelId( 2 );
        var queue1 = subscription1?.Queue;
        var queue2 = subscription2?.Queue;
        await client.GetTask(
            c =>
            {
                c.SendUnsubscribeRequest( 1 );
                c.ReadUnsubscribedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                queue1.TestRefEquals( queue2 ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription2 ) ] ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue1 ) ] ) ) ),
                queue1.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.Subscriptions.Count.TestEquals( 1 ),
                        q.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription2 ) ] ) ) ),
                subscription1.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                subscription2.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ),
                server.Channels.Count.TestEquals( 1 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnsubscribeRequest",
                        "[1::'test'::3] [MessageReceived] [PacketLength: 9] Begin handling UnsubscribeRequest",
                        "[1::'test'::3] [MessageAccepted] [PacketLength: 9] UnsubscribeRequest",
                        "[1::'test'::3] [SendingMessage] [PacketLength: 6] UnsubscribedResponse",
                        "[1::'test'::3] [MessageSent] [PacketLength: 6] UnsubscribedResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[2::'d'::2] [Created] by client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'test'::'c'::1] [Created] by subscription to [1::'c']" ] ),
                logs.GetAllSubscription()
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
    public async Task Unsubscribe_ShouldUnsubscribeLastClientFromChannelWithBindingAndNotRemoveIt()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnsubscribedResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add )
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

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
        var binding = channel?.Bindings.TryGetByClientId( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
        var queue = subscription?.Queue;
        await client.GetTask(
            c =>
            {
                c.SendUnsubscribeRequest( 1 );
                c.ReadUnsubscribedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Subscriptions.Count.TestEquals( 0 ),
                        q.Subscriptions.GetAll().TestEmpty() ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Running ) ),
                server.Channels.Count.TestEquals( 1 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnsubscribeRequest",
                        "[1::'test'::3] [MessageReceived] [PacketLength: 9] Begin handling UnsubscribeRequest",
                        "[1::'test'::3] [MessageAccepted] [PacketLength: 9] UnsubscribeRequest",
                        "[1::'test'::3] [SendingMessage] [PacketLength: 6] UnsubscribedResponse",
                        "[1::'test'::3] [MessageSent] [PacketLength: 6] UnsubscribedResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'test'::'c'::2] [Created] by subscription to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [Disposing]",
                        "[1::'test'::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllSubscription()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::2] [Created]",
                        "[1::'test'=>1::'c'::3] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unsubscribe_ShouldDisposeClient_WhenClientSendsInvalidPayload()
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
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
        await client.GetTask( c => c.SendUnsubscribeRequest( 1, payload: 3 ) );
        await endSource.Task;

        Assertion.All(
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Clients.GetAll().TestEmpty(),
                server.Channels.Count.TestEquals( 0 ),
                server.Channels.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 8] UnsubscribeRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 8] Begin handling UnsubscribeRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 8] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid UnsubscribeRequest from client [1] 'test'. Encountered 1 error(s):
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
                logs.GetAllSubscription()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unsubscribe_ShouldBeRejected_WhenChannelDoesNotExist()
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
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.UnsubscribeRequest )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnsubscribeFailureResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );

        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendUnsubscribeRequest( 1 );
                c.ReadUnsubscribeFailureResponse();
            } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerSubscriptionException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestNull(),
                            exc.Subscription.TestNull() ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( remoteClient ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnsubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 9] Begin handling UnsubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerSubscriptionException: Message broker client [1] 'test' could not be unsubscribed from non-existing channel with ID 1.
                        """,
                        "[1::'test'::1] [SendingMessage] [PacketLength: 6] UnsubscribeFailureResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 6] UnsubscribeFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unsubscribe_ShouldBeRejected_WhenClientIsNotSubscribedToChannel()
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
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.UnsubscribeRequest )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnsubscribeFailureResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadSubscribedResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendUnsubscribeRequest( 1 );
                c.ReadUnsubscribeFailureResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetByName( "c" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerSubscriptionException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient2 ),
                            exc.Channel.TestRefEquals( channel ),
                            exc.Subscription.TestNull() ) ),
                remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Subscriptions.Count.TestEquals( 1 ) ) ),
                server.Clients.Count.TestEquals( 2 ),
                server.Clients.GetAll().TestSetEqual( [ remoteClient1, remoteClient2 ] ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( channel ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnsubscribeRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 9] Begin handling UnsubscribeRequest",
                        """
                        [2::'test2'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerSubscriptionException: Message broker client [2] 'test2' could not be unsubscribed from channel [1] 'c' because it is not subscribed to it.
                        """,
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 6] UnsubscribeFailureResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 6] UnsubscribeFailureResponse"
                    ] ),
                logs.GetAllChannel().TestContainsSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ) )
            .Go();
    }
}
