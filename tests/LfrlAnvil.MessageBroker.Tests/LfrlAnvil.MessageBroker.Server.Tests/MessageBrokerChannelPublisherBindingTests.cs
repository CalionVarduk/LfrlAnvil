using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerChannelPublisherBindingTests : TestsBase
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.PublisherBoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add )
                .SetPublisherEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var stream = server.Streams.TryGetByName( "c" );
        var binding = channel?.Publishers.TryGetByClientId( 1 );

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.ToString().TestEquals( "[1] 'c' channel (Running)" ),
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                        c.Publishers.TryGetByClientId( 1 ).TestRefEquals( binding ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                stream.TestNotNull(
                    s => Assertion.All(
                        "stream",
                        s.Server.TestRefEquals( server ),
                        s.Id.TestEquals( 1 ),
                        s.Name.TestEquals( "c" ),
                        s.State.TestEquals( MessageBrokerStreamState.Running ),
                        s.ToString().TestEquals( "[1] 'c' stream (Running)" ),
                        s.Publishers.Count.TestEquals( 1 ),
                        s.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                        s.Publishers.TryGetByKey( 1, 1 ).TestRefEquals( binding ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                        c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding ) ) ),
                binding.TestNotNull(
                    b => Assertion.All(
                        "binding",
                        b.Channel.TestRefEquals( channel ),
                        b.Stream.TestRefEquals( stream ),
                        b.Client.TestRefEquals( remoteClient ),
                        b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ),
                        b.ToString().TestEquals( "[1] 'test' => [1] 'c' publisher binding (using [1] 'c' stream) (Running)" ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                server.Streams.Count.TestEquals( 1 ),
                server.Streams.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( stream ) ] ),
                server.Streams.TryGetById( 1 ).TestRefEquals( stream ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] PublisherBoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] PublisherBoundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ),
                logs.GetAllPublisher().TestSequence( [ "[1::'test'=>1::'c'::1] [Created]" ] ) )
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.PublisherBoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add )
                .SetPublisherEventHandlerFactory( _ => logs.Add ) );

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
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetByName( "c" );
        var stream = server.Streams.TryGetByName( "c" );
        var binding1 = channel?.Publishers.TryGetByClientId( 1 );
        var binding2 = channel?.Publishers.TryGetByClientId( 2 );
        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Publishers.Count.TestEquals( 2 ),
                        c.Publishers.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                        c.Publishers.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                        c.Publishers.TryGetByClientId( 2 ).TestRefEquals( binding2 ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                stream.TestNotNull(
                    s => Assertion.All(
                        "stream",
                        s.Server.TestRefEquals( server ),
                        s.Id.TestEquals( 1 ),
                        s.Name.TestEquals( "c" ),
                        s.State.TestEquals( MessageBrokerStreamState.Running ),
                        s.ToString().TestEquals( "[1] 'c' stream (Running)" ),
                        s.Publishers.Count.TestEquals( 2 ),
                        s.Publishers.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                        s.Publishers.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ),
                        s.Publishers.TryGetByKey( 2, 1 ).TestRefEquals( binding2 ) ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                        c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding1 ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ),
                        c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding2 ) ) ),
                binding1.TestNotNull(
                    b => Assertion.All(
                        "binding1",
                        b.Channel.TestRefEquals( channel ),
                        b.Stream.TestRefEquals( stream ),
                        b.Client.TestRefEquals( remoteClient1 ),
                        b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ) ) ),
                binding2.TestNotNull(
                    b => Assertion.All(
                        "binding2",
                        b.Channel.TestRefEquals( channel ),
                        b.Stream.TestRefEquals( stream ),
                        b.Client.TestRefEquals( remoteClient2 ),
                        b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ) ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                server.Streams.Count.TestEquals( 1 ),
                server.Streams.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( stream ) ] ),
                server.Streams.TryGetById( 1 ).TestRefEquals( stream ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] PublisherBoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] PublisherBoundResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest",
                        "[2::'test2'::1] [MessageAccepted] [PacketLength: 11] BindPublisherRequest",
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 14] PublisherBoundResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 14] PublisherBoundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ),
                logs.GetAllPublisher()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[2::'test2'=>1::'c'::1] [Created]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateBindingForExistingStreamCorrectly()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.PublisherBoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add )
                .SetPublisherEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "d", "c" );
                c.ReadPublisherBoundResponse();
            } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel1 = server.Channels.TryGetByName( "c" );
        var channel2 = server.Channels.TryGetByName( "d" );
        var stream = server.Streams.TryGetByName( "c" );
        var binding1 = channel1?.Publishers.TryGetByClientId( 1 );
        var binding2 = channel2?.Publishers.TryGetByClientId( 1 );

        Assertion.All(
                channel1.TestNotNull(
                    c => Assertion.All(
                        "channel1",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.ToString().TestEquals( "[1] 'c' channel (Running)" ),
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                        c.Publishers.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                channel2.TestNotNull(
                    c => Assertion.All(
                        "channel2",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 2 ),
                        c.Name.TestEquals( "d" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.ToString().TestEquals( "[2] 'd' channel (Running)" ),
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ),
                        c.Publishers.TryGetByClientId( 1 ).TestRefEquals( binding2 ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                stream.TestNotNull(
                    s => Assertion.All(
                        "stream",
                        s.Server.TestRefEquals( server ),
                        s.Id.TestEquals( 1 ),
                        s.Name.TestEquals( "c" ),
                        s.State.TestEquals( MessageBrokerStreamState.Running ),
                        s.ToString().TestEquals( "[1] 'c' stream (Running)" ),
                        s.Publishers.Count.TestEquals( 2 ),
                        s.Publishers.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                        s.Publishers.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ),
                        s.Publishers.TryGetByKey( 1, 2 ).TestRefEquals( binding2 ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Publishers.Count.TestEquals( 2 ),
                        c.Publishers.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                        c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding1 ),
                        c.Publishers.TryGetByChannelId( 2 ).TestRefEquals( binding2 ) ) ),
                binding1.TestNotNull(
                    b => Assertion.All(
                        "binding1",
                        b.Channel.TestRefEquals( channel1 ),
                        b.Stream.TestRefEquals( stream ),
                        b.Client.TestRefEquals( remoteClient ),
                        b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ),
                        b.ToString().TestEquals( "[1] 'test' => [1] 'c' publisher binding (using [1] 'c' stream) (Running)" ) ) ),
                binding2.TestNotNull(
                    b => Assertion.All(
                        "binding2",
                        b.Channel.TestRefEquals( channel2 ),
                        b.Stream.TestRefEquals( stream ),
                        b.Client.TestRefEquals( remoteClient ),
                        b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ),
                        b.ToString().TestEquals( "[1] 'test' => [2] 'd' publisher binding (using [1] 'c' stream) (Running)" ) ) ),
                server.Channels.Count.TestEquals( 2 ),
                server.Channels.GetAll().TestSetEqual( [ channel1, channel2 ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel1 ),
                server.Channels.TryGetById( 2 ).TestRefEquals( channel2 ),
                server.Streams.Count.TestEquals( 1 ),
                server.Streams.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( stream ) ] ),
                server.Streams.TryGetById( 1 ).TestRefEquals( stream ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] PublisherBoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] PublisherBoundResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[2::'d'::2] [Created] by client [1::'test']"
                    ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ),
                logs.GetAllPublisher()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>2::'d'::2] [Created]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenPublisherEventHandlerFactoryThrows()
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
                .SetPublisherEventHandlerFactory( _ => throw exception ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindPublisherRequest( "c" ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest" ),
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
        await client.GetTask( c => c.SendBindPublisherRequest( "c" ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest" ),
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
    public async Task Creation_ShouldDisposeClient_WhenStreamEventHandlerFactoryThrowsForCreatedStream()
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
                .SetStreamEventHandlerFactory( _ => throw exception ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindPublisherRequest( "c" ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        (m, _) => m.TestEquals( "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest" ),
                        (m, _) => m.TestEquals( "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest" ),
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
        await client.GetTask( c => c.SendBindPublisherRequest( "c", payload: 0 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 5] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 5] Begin handling BindPublisherRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendBindPublisherRequest( "c", channelNameLength: -1 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 11] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendBindPublisherRequest( "foo", channelNameLength: 4 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 13] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 13] Begin handling BindPublisherRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 13] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendBindPublisherRequest( string.Empty ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 10] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 10] Begin handling BindPublisherRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 10] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
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
        await client.GetTask( c => c.SendBindPublisherRequest( new string( 'x', 513 ) ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 523] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 523] Begin handling BindPublisherRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 523] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected channel name length to be in [1, 512] range but found 513.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsTooLongStreamName()
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
        await client.GetTask( c => c.SendBindPublisherRequest( "c", new string( 'x', 513 ) ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 524] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 524] Begin handling BindPublisherRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 524] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected stream name length to be in [1, 512] range but found 513.
                        """,
                        "[1::'test'::<ROOT>] [Disposing]",
                        "[1::'test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindPublisherRequest_ShouldBeRejected_WhenClientIsAlreadyBoundAsPublisherToChannel()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.BindPublisherFailureResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindPublisherRequest( "c", "d" );
                c.ReadBindPublisherFailureResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Publishers.TryGetByClientId( 1 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestRefEquals( channel ),
                            exc.Publisher.TestRefEquals( binding ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                server.Streams.Count.TestEquals( 1 ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] PublisherBoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] PublisherBoundResponse",
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 12] BindPublisherRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 12] Begin handling BindPublisherRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 12] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Message broker client [1] 'test' could not be bound as a publisher to channel [1] 'c' because it is already bound as a publisher to it.
                        """,
                        "[1::'test'::2] [SendingMessage] [PacketLength: 6] BindPublisherFailureResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 6] BindPublisherFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldNotThrow_WhenChannelOrStreamOrPublisherEventHandlerThrows()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.PublisherBoundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => _ => throw new Exception( "foo" ) )
                .SetStreamEventHandlerFactory( _ => _ => throw new Exception( "bar" ) )
                .SetPublisherEventHandlerFactory( _ => _ => throw new Exception( "qux" ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding = channel?.Publishers.TryGetByClientId( 1 );

        Assertion.All(
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                server.Streams.Count.TestEquals( 1 ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                binding.TestNotNull(
                    b => Assertion.All(
                        "binding",
                        b.Client.TestRefEquals( remoteClient ),
                        b.Channel.TestRefEquals( channel ),
                        b.Stream.TestRefEquals( stream ) ) ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] PublisherBoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] PublisherBoundResponse"
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
                .SetStreamEventHandlerFactory( _ => logs.Add )
                .SetPublisherEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding = channel?.Publishers.TryGetByClientId( 1 );
        await server.DisposeAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All( "client", c.Publishers.Count.TestEquals( 0 ), c.Publishers.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty() ) ),
                stream.TestNotNull(
                    s => Assertion.All(
                        "stream",
                        s.State.TestEquals( MessageBrokerStreamState.Disposed ),
                        s.Publishers.Count.TestEquals( 0 ),
                        s.Publishers.GetAll().TestEmpty() ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllStream()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllPublisher()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeBinding_WhenChannelIsOnlyBoundAsPublisherToDisconnectedClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add )
                .SetPublisherEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding = channel?.Publishers.TryGetByClientId( 1 );
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                remoteClient.TestNotNull(
                    c => Assertion.All( "client", c.Publishers.Count.TestEquals( 0 ), c.Publishers.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty() ) ),
                stream.TestNotNull(
                    s => Assertion.All(
                        "stream",
                        s.State.TestEquals( MessageBrokerStreamState.Disposed ),
                        s.Publishers.Count.TestEquals( 0 ),
                        s.Publishers.GetAll().TestEmpty() ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllStream()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllPublisher()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldRemoveBinding_WhenChannelIsAlsoBoundAsPublisherToOtherClient()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add )
                .SetPublisherEventHandlerFactory( _ => logs.Add ) );

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
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding1 = channel?.Publishers.TryGetByClientId( 1 );
        var binding2 = channel?.Publishers.TryGetByClientId( 2 );
        if ( remoteClient1 is not null )
            await remoteClient1.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 1 ),
                server.Streams.Count.TestEquals( 1 ),
                remoteClient1.TestNotNull(
                    c => Assertion.All( "client1", c.Publishers.Count.TestEquals( 0 ), c.Publishers.GetAll().TestEmpty() ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ) ) ),
                stream.TestNotNull(
                    s => Assertion.All(
                        "stream",
                        s.State.TestEquals( MessageBrokerStreamState.Running ),
                        s.Publishers.Count.TestEquals( 1 ),
                        s.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ) ) ),
                binding1.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ),
                logs.GetAllPublisher()
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
    public async Task Unbind_ShouldUnbindLastClientFromChannelAndStreamAndRemoveThem()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.PublisherUnboundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add )
                .SetPublisherEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var stream = server.Streams.TryGetByName( "c" );
        var binding = channel?.Publishers.TryGetByClientId( 1 );
        await client.GetTask(
            c =>
            {
                c.SendUnbindPublisherRequest( 1 );
                c.ReadPublisherUnboundResponse();
            } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty() ) ),
                stream.TestNotNull(
                    s => Assertion.All(
                        "stream",
                        s.State.TestEquals( MessageBrokerStreamState.Disposed ),
                        s.Publishers.Count.TestEquals( 0 ),
                        s.Publishers.GetAll().TestEmpty() ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty() ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                server.Channels.Count.TestEquals( 0 ),
                server.Channels.GetAll().TestEmpty(),
                server.Streams.Count.TestEquals( 0 ),
                server.Streams.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindPublisherRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 9] Begin handling UnbindPublisherRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 9] UnbindPublisherRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 6] PublisherUnboundResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 6] PublisherUnboundResponse"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllStream()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllPublisher()
                    .TestSequence(
                    [
                        "[1::'test'=>1::'c'::1] [Created]",
                        "[1::'test'=>1::'c'::2] [Disposing]",
                        "[1::'test'=>1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindNonLastClientFromChannelAndStreamWithoutRemovingThem()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.PublisherUnboundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add )
                .SetPublisherEventHandlerFactory( _ => logs.Add ) );

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
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetByName( "c" );
        var stream = server.Streams.TryGetByName( "c" );
        var binding1 = channel?.Publishers.TryGetByClientId( 1 );
        var binding2 = channel?.Publishers.TryGetByClientId( 2 );

        await client2.GetTask(
            c =>
            {
                c.SendUnbindPublisherRequest( 1 );
                c.ReadPublisherUnboundResponse();
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
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                        c.Publishers.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                        c.Publishers.TryGetByClientId( 2 ).TestNull() ) ),
                stream.TestNotNull(
                    s => Assertion.All(
                        "stream",
                        s.Server.TestRefEquals( server ),
                        s.Id.TestEquals( 1 ),
                        s.Name.TestEquals( "c" ),
                        s.State.TestEquals( MessageBrokerStreamState.Running ),
                        s.Publishers.Count.TestEquals( 1 ),
                        s.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                        s.Publishers.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ),
                        s.Publishers.TryGetByKey( 2, 1 ).TestNull() ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "client1",
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                        c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding1 ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "client2",
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty() ) ),
                binding1.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ) ),
                binding2.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ),
                server.Channels.TryGetById( 1 ).TestRefEquals( channel ),
                server.Streams.Count.TestEquals( 1 ),
                server.Streams.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( stream ) ] ),
                server.Streams.TryGetById( 1 ).TestRefEquals( stream ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest",
                        "[1::'test'::1] [MessageAccepted] [PacketLength: 11] BindPublisherRequest",
                        "[1::'test'::1] [SendingMessage] [PacketLength: 14] PublisherBoundResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 14] PublisherBoundResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 11] BindPublisherRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 11] Begin handling BindPublisherRequest",
                        "[2::'test2'::1] [MessageAccepted] [PacketLength: 11] BindPublisherRequest",
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 14] PublisherBoundResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 14] PublisherBoundResponse",
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindPublisherRequest",
                        "[2::'test2'::2] [MessageReceived] [PacketLength: 9] Begin handling UnbindPublisherRequest",
                        "[2::'test2'::2] [MessageAccepted] [PacketLength: 9] UnbindPublisherRequest",
                        "[2::'test2'::2] [SendingMessage] [PacketLength: 6] PublisherUnboundResponse",
                        "[2::'test2'::2] [MessageSent] [PacketLength: 6] PublisherUnboundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ),
                logs.GetAllPublisher()
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
    public async Task Unbind_ShouldUnbindLastClientFromChannelWithListenerBindingAndNotRemoveIt()
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.PublisherUnboundResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetPublisherEventHandlerFactory( _ => logs.Add ) );

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
        var subscription = channel?.Listeners.TryGetByClientId( 1 );
        var binding = channel?.Publishers.TryGetByClientId( 1 );
        await client.GetTask(
            c =>
            {
                c.SendUnbindPublisherRequest( 1 );
                c.ReadPublisherUnboundResponse();
            } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty(),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "client",
                        c.Publishers.Count.TestEquals( 0 ),
                        c.Publishers.GetAll().TestEmpty(),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ) ),
                server.Channels.Count.TestEquals( 1 ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindPublisherRequest",
                        "[1::'test'::3] [MessageReceived] [PacketLength: 9] Begin handling UnbindPublisherRequest",
                        "[1::'test'::3] [MessageAccepted] [PacketLength: 9] UnbindPublisherRequest",
                        "[1::'test'::3] [SendingMessage] [PacketLength: 6] PublisherUnboundResponse",
                        "[1::'test'::3] [MessageSent] [PacketLength: 6] PublisherUnboundResponse"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllPublisher()
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
                .SetPublisherEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var binding = channel?.Publishers.TryGetByClientId( 1 );
        await client.GetTask( c => c.SendUnbindPublisherRequest( 1, payload: 3 ) );
        await endSource.Task;

        Assertion.All(
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Clients.GetAll().TestEmpty(),
                server.Channels.Count.TestEquals( 0 ),
                server.Channels.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 8] UnbindPublisherRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 8] Begin handling UnbindPublisherRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 8] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid UnbindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
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
                logs.GetAllPublisher()
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
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.UnbindPublisherRequest )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnbindPublisherFailureResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );

        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendUnbindPublisherRequest( 1 );
                c.ReadUnbindPublisherFailureResponse();
            } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestNull(),
                            exc.Publisher.TestNull() ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( remoteClient ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindPublisherRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 9] Begin handling UnbindPublisherRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Message broker client [1] 'test' could not be unbound as a publisher from non-existing channel with ID 1.
                        """,
                        "[1::'test'::1] [SendingMessage] [PacketLength: 6] UnbindPublisherFailureResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 6] UnbindPublisherFailureResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldBeRejected_WhenClientIsNotBoundAsPublisherToChannel()
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
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.UnbindPublisherRequest )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.UnbindPublisherFailureResponse )
                            endSource.Complete();
                    } )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

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
                c.SendUnbindPublisherRequest( 1 );
                c.ReadUnbindPublisherFailureResponse();
            } );

        await endSource.Task;

        var channel = server.Channels.TryGetByName( "c" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient2 ),
                            exc.Channel.TestRefEquals( channel ),
                            exc.Publisher.TestNull() ) ),
                remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Publishers.Count.TestEquals( 1 ) ) ),
                server.Clients.Count.TestEquals( 2 ),
                server.Clients.GetAll().TestSetEqual( [ remoteClient1, remoteClient2 ] ),
                server.Channels.Count.TestEquals( 1 ),
                server.Channels.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( channel ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] UnbindPublisherRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 9] Begin handling UnbindPublisherRequest",
                        """
                        [2::'test2'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Message broker client [2] 'test2' could not be unbound as a publisher from channel [1] 'c' because it is not bound as a publisher to it.
                        """,
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 6] UnbindPublisherFailureResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 6] UnbindPublisherFailureResponse"
                    ] ),
                logs.GetAllChannel().TestContainsSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ) )
            .Go();
    }
}
