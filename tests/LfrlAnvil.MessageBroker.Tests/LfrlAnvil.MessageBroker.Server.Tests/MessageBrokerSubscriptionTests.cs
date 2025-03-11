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
            () => new TimestampProvider(),
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
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription ) ) ),
                subscription.TestNotNull(
                    s => Assertion.All(
                        "subscription",
                        s.Channel.TestRefEquals( channel ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.State.TestEquals( MessageBrokerSubscriptionState.Running ),
                        s.ToString().TestEquals( "[1] 'test' => [1] 'c' subscription (Running)" ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling SubscribeRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 10] SubscribedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 10] SubscribedResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllSubscription().TestSequence( [ "[1::'test'=>1::'c'::1] [Created]" ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateSubscriptionForExistingChannelCorrectly()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
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

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
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
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription ) ) ),
                subscription.TestNotNull(
                    s => Assertion.All(
                        "subscription",
                        s.Channel.TestRefEquals( channel ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 7] Begin handling SubscribeRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 10] SubscribedResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 10] SubscribedResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllSubscription().TestSequence( [ "[1::'test'=>1::'c'::2] [Created]" ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenSubscriptionEventHandlerFactoryThrows()
    {
        var endSource = new SafeTaskCompletionSource();
        var exception = new Exception( "foo" );
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
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
                server.Channels.Count.TestEquals( 1 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] SubscribeRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling SubscribeRequest" ),
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
            () => new TimestampProvider(),
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
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] SubscribeRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling SubscribeRequest" ),
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
            () => new TimestampProvider(),
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
        await client.GetTask( c => c.SendSubscribeRequest( "c", createChannelIfNotExists: true, payload: 0 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 5] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 5] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid SubscribeRequest with payload 0 from client [1] 'test'. Encountered 1 error(s):
                        1. Packet length is invalid.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsEmptyName()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
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
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 6] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 6] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 6] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid SubscribeRequest with payload 1 from client [1] 'test'. Encountered 1 error(s):
                        1. Expected name length to be in [1, 512] range but found 0.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsTooLongName()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
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
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 519] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 519] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 519] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid SubscribeRequest with payload 514 from client [1] 'test'. Encountered 1 error(s):
                        1. Expected name length to be in [1, 512] range but found 513.
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
            () => new TimestampProvider(),
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
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling SubscribeRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 10] SubscribedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 10] SubscribedResponse",
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 7] Begin handling SubscribeRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 7] Encountered an error:
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
            () => new TimestampProvider(),
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
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling SubscribeRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 7] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerSubscriptionException: Message broker client [1] 'test' failed to create a subscription to channel 'c' because channel does not exist.
                        """,
                        "[1::'test'::1] [SendingMessage] [PacketLength: 6] SubscribeFailureResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 6] SubscribeFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldNotThrow_WhenSubscriptionEventHandlerThrows()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
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
                .SetSubscriptionEventHandlerFactory( _ => _ => throw new Exception( "foo" ) ) );

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

        Assertion.All(
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
                        c.Subscriptions.GetAll().TestSequence( [ (cl, _) => cl.TestRefEquals( subscription ) ] ) ) ),
                subscription.TestNotNull(
                    s => Assertion.All(
                        "subscription",
                        s.Client.TestRefEquals( remoteClient ),
                        s.Channel.TestRefEquals( channel ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling SubscribeRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 10] SubscribedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 10] SubscribedResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ServerDispose_ShouldDisposeSubscription()
    {
        var logs = new EventLogger();
        var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
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
        await server.DisposeAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All( "client", c.Subscriptions.Count.TestEquals( 0 ), c.Subscriptions.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
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
    public async Task ClientDisconnect_ShouldDisposeChannelAndSubscription_WhenChannelIsOnlyLinkedToAndSubscribedByDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
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
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
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
                        "[1::'test'=>1::'c'::2] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeSubscription_WhenChannelIsLinkedToAnotherClientAndSubscribedByDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
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
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
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
        ClientDisconnect_ShouldDisposeChannelAndSubscription_WhenChannelIsNotLinkedToAnyClientAndSubscribedOnlyByDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
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
        var channel = server.Channels.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
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
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Bindings.GetAll().TestEmpty(),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
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
    public async Task ClientDisconnect_ShouldDisposeSubscription_WhenChannelIsNotLinkedToAnyClientAndSubscribedByAnotherClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
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
        if ( remoteClient2 is not null )
            await remoteClient2.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 1 ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription1 ) ] ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription1 ) ] ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription1 ),
                        c.Subscriptions.TryGetByClientId( 2 ).TestNull() ) ),
                subscription1.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ),
                subscription2.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
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
    public async Task Unsubscribe_ShouldUnsubscribeLastClientFromChannelAndRemoveIt()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
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
                .SetSubscriptionEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", createChannelIfNotExists: true );
                c.ReadBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
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
                        c.Subscriptions.GetAll().TestEmpty() ) ),
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
            () => new TimestampProvider(),
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
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription1 ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Subscriptions.GetAll().TestEmpty() ) ),
                subscription1.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ),
                subscription2.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling SubscribeRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 7] SubscribeRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 10] SubscribedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 10] SubscribedResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 7] SubscribeRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 7] Begin handling SubscribeRequest",
                        "[2::'test2'::1] [MessageAccepted] [PacketLength: 7] SubscribeRequest",
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 10] SubscribedResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 10] SubscribedResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnsubscribeRequest",
                        "[2::'test2'::2] [MessageReceived] [PacketLength: 9] Begin handling UnsubscribeRequest",
                        "[2::'test2'::2] [MessageAccepted] [PacketLength: 9] UnsubscribeRequest",
                        "[2::'test2'::2] [SendingMessage] [PacketLength: 6] UnsubscribedResponse",
                        "[2::'test2'::2] [MessageSent] [PacketLength: 6] UnsubscribedResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
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
    public async Task Unsubscribe_ShouldUnsubscribeLastClientFromChannelWithLinkAndNotRemoveIt()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
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
                        c.Subscriptions.GetAll().TestEmpty() ) ),
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
            () => new TimestampProvider(),
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
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid UnsubscribeRequest with payload 3 from client [1] 'test'. Encountered 1 error(s):
                        1. Expected header payload to be 4.
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
            () => new TimestampProvider(),
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
            () => new TimestampProvider(),
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
