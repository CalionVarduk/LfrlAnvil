using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerChannelTests : TestsBase
{
    [Fact]
    public async Task Creation_ShouldCreateChannelAndClientLinkCorrectly()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory( _ => logs.Add )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.LinkedClients.Count.TestEquals( 1 ),
                        c.LinkedClients.GetAll().TestSequence( [ (cl, _) => cl.TestRefEquals( remoteClient ) ] ),
                        c.LinkedClients.TryGetById( 1 ).TestRefEquals( remoteClient ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                        c.LinkedChannels.TryGetById( 1 ).TestRefEquals( channel ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling LinkChannelRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 10] ChannelLinkedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 10] ChannelLinkedResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::1] [Linked] to client [1::'test']"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateClientLinkForExistingChannelCorrectly()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.ChannelLinkedResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetByName( "c" );
        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.LinkedClients.Count.TestEquals( 2 ),
                        c.LinkedClients.GetAll().TestSetEqual( [ remoteClient1, remoteClient2 ] ),
                        c.LinkedClients.TryGetById( 1 ).TestRefEquals( remoteClient1 ),
                        c.LinkedClients.TryGetById( 2 ).TestRefEquals( remoteClient2 ) ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                        c.LinkedChannels.TryGetById( 1 ).TestRefEquals( channel ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                        c.LinkedChannels.TryGetById( 1 ).TestRefEquals( channel ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling LinkChannelRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 10] ChannelLinkedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 10] ChannelLinkedResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 7] LinkChannelRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 7] Begin handling LinkChannelRequest",
                        "[2::'test2'::1] [MessageAccepted] [PacketLength: 7] LinkChannelRequest",
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 10] ChannelLinkedResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 10] ChannelLinkedResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::1] [Linked] to client [1::'test']",
                        "[1::'c'::1] [Linked] to client [2::'test2']"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenChannelEventHandlerFactoryThrows()
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
        await client.GetTask( c => c.SendLinkChannelRequest( "c" ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] LinkChannelRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling LinkChannelRequest" ),
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
        await client.GetTask( c => c.SendLinkChannelRequest( "c", payload: 0 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 5] LinkChannelRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 5] Begin handling LinkChannelRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid LinkChannelRequest with payload 0 from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendLinkChannelRequest( string.Empty ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 6] LinkChannelRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 6] Begin handling LinkChannelRequest",
                        """
                        [1::'test'::<ROOT>] [MessageRejected] [PacketLength: 6] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid LinkChannelRequest with payload 1 from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendLinkChannelRequest( new string( 'x', 513 ) ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 519] LinkChannelRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 519] Begin handling LinkChannelRequest",
                        """
                        [1::'test'::<ROOT>] [MessageRejected] [PacketLength: 519] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid LinkChannelRequest with payload 514 from client [1] 'test'. Encountered 1 error(s):
                        1. Expected name length to be in [1, 512] range but found 513.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task LinkRequest_ShouldBeRejected_WhenClientIsAlreadyLinkedToChannel()
    {
        Exception? exception = null;
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
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.LinkChannelFailureResponse.Payload );
            } );

        var channel = server.Channels.TryGetById( 1 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerRemoteClientChannelLinkException>(
                        exc => Assertion.All( exc.Client.TestRefEquals( remoteClient ), exc.Channel.TestRefEquals( channel ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.LinkedClients.Count.TestEquals( 1 ),
                        c.LinkedClients.GetAll().TestSequence( [ (cl, _) => cl.TestRefEquals( remoteClient ) ] ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling LinkChannelRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 10] ChannelLinkedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 10] ChannelLinkedResponse",
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 7] Begin handling LinkChannelRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 7] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientChannelLinkException: Message broker client [1] 'test' could not be linked to channel [1] 'c' because it is already linked to the channel.
                        """,
                        "[1::'test'::2] [SendingMessage] [PacketLength: 6] LinkChannelFailureResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 6] LinkChannelFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldNotThrow_WhenChannelEventHandlerThrows()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory( _ => logs.Add )
                .SetChannelEventHandlerFactory( _ => _ => throw new Exception( "foo" ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        var channel = server.Channels.TryGetById( 1 );

        Assertion.All(
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.LinkedClients.Count.TestEquals( 1 ),
                        c.LinkedClients.GetAll().TestSequence( [ (cl, _) => cl.TestRefEquals( remoteClient ) ] ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling LinkChannelRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 10] ChannelLinkedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 10] ChannelLinkedResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ServerDispose_ShouldDisposeChannel()
    {
        var logs = new EventLogger();
        var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        await server.DisposeAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All( "client", c.LinkedChannels.Count.TestEquals( 0 ), c.LinkedChannels.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.LinkedClients.Count.TestEquals( 0 ),
                        c.LinkedClients.GetAll().TestEmpty() ) ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::1] [Linked] to client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeChannel_WhenChannelIsOnlyLinkedToDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All( "client", c.LinkedChannels.Count.TestEquals( 0 ), c.LinkedChannels.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.LinkedClients.Count.TestEquals( 0 ),
                        c.LinkedClients.GetAll().TestEmpty() ) ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::1] [Linked] to client [1::'test']",
                        "[1::'c'::<ROOT>] [Unlinked] from client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldRemoveChannelLink_WhenChannelIsAlsoLinkedToOtherClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        if ( remoteClient1 is not null )
            await remoteClient1.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 1 ),
                remoteClient1.TestNotNull(
                    c => Assertion.All( "client1", c.LinkedChannels.Count.TestEquals( 0 ), c.LinkedChannels.GetAll().TestEmpty() ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.LinkedClients.Count.TestEquals( 1 ),
                        c.LinkedClients.GetAll().TestSequence( [ (cl, _) => cl.TestRefEquals( remoteClient2 ) ] ) ) ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::1] [Linked] to client [1::'test']",
                        "[1::'c'::1] [Linked] to client [2::'test2']",
                        "[1::'c'::<ROOT>] [Unlinked] from client [1::'test']"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unlink_ShouldUnlinkLastClientFromChannelAndRemoveIt()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory( _ => logs.Add )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        await client.GetTask(
            c =>
            {
                c.SendUnlinkChannelRequest( 1 );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelUnlinkedResponse.Payload );
            } );

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.LinkedClients.Count.TestEquals( 0 ),
                        c.LinkedClients.GetAll().TestEmpty() ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.LinkedChannels.Count.TestEquals( 0 ),
                        c.LinkedChannels.GetAll().TestEmpty() ) ),
                server.Channels.Count.TestEquals( 0 ),
                server.Channels.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnlinkChannelRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 9] Begin handling UnlinkChannelRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 9] UnlinkChannelRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 6] ChannelUnlinkedResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 6] ChannelUnlinkedResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::1] [Linked] to client [1::'test']",
                        "[1::'c'::2] [Unlinked] from client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unlink_ShouldUnlinkNonLastClientFromChannelWithoutRemovingIt()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.ChannelUnlinkedResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetByName( "c" );

        await client2.GetTask(
            c =>
            {
                c.SendUnlinkChannelRequest( 1 );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelUnlinkedResponse.Payload );
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
                        c.LinkedClients.Count.TestEquals( 1 ),
                        c.LinkedClients.GetAll().TestSequence( [ (cl, _) => cl.TestRefEquals( remoteClient1 ) ] ),
                        c.LinkedClients.TryGetById( 1 ).TestRefEquals( remoteClient1 ),
                        c.LinkedClients.TryGetById( 2 ).TestNull() ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                        c.LinkedChannels.TryGetById( 1 ).TestRefEquals( channel ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.LinkedChannels.Count.TestEquals( 0 ),
                        c.LinkedChannels.GetAll().TestEmpty() ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 7] Begin handling LinkChannelRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 7] LinkChannelRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 10] ChannelLinkedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 10] ChannelLinkedResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 7] LinkChannelRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 7] Begin handling LinkChannelRequest",
                        "[2::'test2'::1] [MessageAccepted] [PacketLength: 7] LinkChannelRequest",
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 10] ChannelLinkedResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 10] ChannelLinkedResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnlinkChannelRequest",
                        "[2::'test2'::2] [MessageReceived] [PacketLength: 9] Begin handling UnlinkChannelRequest",
                        "[2::'test2'::2] [MessageAccepted] [PacketLength: 9] UnlinkChannelRequest",
                        "[2::'test2'::2] [SendingMessage] [PacketLength: 6] ChannelUnlinkedResponse",
                        "[2::'test2'::2] [MessageSent] [PacketLength: 6] ChannelUnlinkedResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::1] [Linked] to client [1::'test']",
                        "[1::'c'::1] [Linked] to client [2::'test2']",
                        "[1::'c'::2] [Unlinked] from client [2::'test2']"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unlink_ShouldDisposeClient_WhenClientSendsInvalidPayload()
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
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.ChannelLinkedResponse.Payload );
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        await client.GetTask( c => c.SendUnlinkChannelRequest( 1, payload: 3 ) );
        await endSource.Task;

        Assertion.All(
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Clients.GetAll().TestEmpty(),
                server.Channels.Count.TestEquals( 0 ),
                server.Channels.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 8] UnlinkChannelRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 8] Begin handling UnlinkChannelRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 8] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid UnlinkChannelRequest with payload 3 from client [1] 'test'. Encountered 1 error(s):
                        1. Expected header payload to be 4.
                        """
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::1] [Linked] to client [1::'test']",
                        "[1::'c'::<ROOT>] [Unlinked] from client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unlink_ShouldBeRejected_WhenChannelDoesNotExist()
    {
        Exception? exception = null;
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
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.UnlinkChannelRequest )
                            exception = e.Exception;
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );

        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendUnlinkChannelRequest( 1 );
                c.Read( Protocol.PacketHeader.Length + Protocol.UnlinkChannelFailureResponse.Payload );
            } );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerRemoteClientChannelLinkException>(
                        exc => Assertion.All( exc.Client.TestRefEquals( remoteClient ), exc.Channel.TestNull() ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( remoteClient ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnlinkChannelRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 9] Begin handling UnlinkChannelRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientChannelLinkException: Message broker client [1] 'test' could not be unlinked from non-existing channel with ID 1.
                        """,
                        "[1::'test'::1] [SendingMessage] [PacketLength: 6] UnlinkChannelFailureResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 6] UnlinkChannelFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unlink_ShouldBeRejected_WhenClientIsNotLinkedToChannel()
    {
        Exception? exception = null;
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
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.UnlinkChannelRequest )
                            exception = e.Exception;
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendLinkChannelRequest( "c" );
                c.Read( Protocol.PacketHeader.Length + Protocol.UnlinkChannelFailureResponse.Payload );
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendUnlinkChannelRequest( 1 );
                c.Read( Protocol.PacketHeader.Length + Protocol.UnlinkChannelFailureResponse.Payload );
            } );

        var channel = server.Channels.TryGetByName( "c" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerRemoteClientChannelLinkException>(
                        exc => Assertion.All( exc.Client.TestRefEquals( remoteClient2 ), exc.Channel.TestRefEquals( channel ) ) ),
                remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.LinkedClients.Count.TestEquals( 1 ) ) ),
                server.Clients.Count.TestEquals( 2 ),
                server.Clients.GetAll().TestSetEqual( [ remoteClient1, remoteClient2 ] ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( channel ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnlinkChannelRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 9] Begin handling UnlinkChannelRequest",
                        """
                        [2::'test2'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientChannelLinkException: Message broker client [2] 'test2' could not be unlinked from channel [1] 'c' because they are not linked to begin with.
                        """,
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 6] UnlinkChannelFailureResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 6] UnlinkChannelFailureResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestContainsSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::1] [Linked] to client [1::'test']"
                    ] ) )
            .Go();
    }
}
