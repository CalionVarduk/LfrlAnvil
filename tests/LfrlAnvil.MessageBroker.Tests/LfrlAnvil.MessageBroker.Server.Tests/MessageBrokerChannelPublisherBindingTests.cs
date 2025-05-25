using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerChannelPublisherBindingTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public MessageBrokerChannelPublisherBindingTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task Creation_ShouldCreateBindingCorrectly()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'c'",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                            "[PublisherBound] Client = [1] 'test', TraceId = 1, Channel = [1] 'c' (created), Stream = [1] 'c' (created)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 11)"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateBindingForExistingChannelCorrectly()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    c => c.Id == 2
                        ? clientLogs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                        endSource.Complete();
                                } ) )
                        : null )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [2] 'test2', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [2] 'test2', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                            "[BindingPublisher] Client = [2] 'test2', TraceId = 1, ChannelName = 'c'",
                            "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                            "[PublisherBound] Client = [2] 'test2', TraceId = 1, Channel = [1] 'c', Stream = [1] 'c'",
                            "[SendPacket:Sending] Client = [2] 'test2', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [2] 'test2', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                            "[Trace:BindPublisher] Client = [2] 'test2', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [2] 'test2'",
                        "[AwaitPacket] Client = [2] 'test2', Packet = (BindPublisherRequest, Length = 11)"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateBindingForExistingStreamCorrectly()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindPublisherRequest, Length = 12)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 2, ChannelName = 'd', StreamName = 'c'",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (BindPublisherRequest, Length = 12)",
                            "[PublisherBound] Client = [1] 'test', TraceId = 2, Channel = [2] 'd' (created), Stream = [1] 'c'",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PublisherBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PublisherBoundResponse, Length = 14)",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 12)"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[2::'d'::2] [Created] by client [1::'test']"
                    ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenChannelEventHandlerFactoryThrowsForCreatedChannel()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var exception = new Exception( "foo" );
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                    or MessageBrokerRemoteClientTraceEventType.Unexpected )
                                    endSource.Complete();
                            } ) ) )
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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'c'",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            (e, _) => e.TestEquals( "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (start)" ),
                            (e, _) => e.TestStartsWith(
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                System.Exception: foo
                                """ ),
                            (e, _) => e.TestEquals( "[Disposing] Client = [1] 'test', TraceId = 2" ),
                            (e, _) => e.TestEquals( "[Disposed] Client = [1] 'test', TraceId = 2" ),
                            (e, _) => e.TestEquals( "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (end)" )
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 11)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenStreamEventHandlerFactoryThrowsForCreatedStream()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var exception = new Exception( "foo" );
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                    or MessageBrokerRemoteClientTraceEventType.Unexpected )
                                    endSource.Complete();
                            } ) ) )
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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'c'",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            (e, _) => e.TestEquals( "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (start)" ),
                            (e, _) => e.TestStartsWith(
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                System.Exception: foo
                                """ ),
                            (e, _) => e.TestEquals( "[Disposing] Client = [1] 'test', TraceId = 2" ),
                            (e, _) => e.TestEquals( "[Disposed] Client = [1] 'test', TraceId = 2" ),
                            (e, _) => e.TestEquals( "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (end)" )
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 11)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsInvalidPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindPublisherRequest( "c", payload: 4 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 9)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected header payload to be at least 5 but found 4.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 9)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsNegativeChannelNameLength()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            } ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected binary channel name length to be in [0, 1] range but found -1.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 11)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsTooLongChannelNameLength()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            } ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected binary channel name length to be in [0, 3] range but found 4.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsEmptyChannelName()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            } ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 10)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel name length to be in [1, 512] range but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 10)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsTooLongChannelName()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            } ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 523)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel name length to be in [1, 512] range but found 513.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 523)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsTooLongStreamName()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            } ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 524)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected stream name length to be in [1, 512] range but found 513.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 524)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindPublisherRequest_ShouldBeRejected_WhenClientIsAlreadyBoundAsPublisherToChannel()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindPublisherRequest, Length = 12)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 2, ChannelName = 'c', StreamName = 'd'",
                            """
                            [Error] Client = [1] 'test', TraceId = 2
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Client [1] 'test' could not be bound as a publisher to channel [1] 'c' because it is already bound as a publisher to it.
                            """,
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (BindPublisherFailureResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (BindPublisherFailureResponse, Length = 6)",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 11)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 12)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldNotThrow_WhenChannelOrStreamEventHandlerThrows()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelEventHandlerFactory( _ => _ => throw new Exception( "foo" ) )
                .SetStreamEventHandlerFactory( _ => _ => throw new Exception( "bar" ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'c'",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                            "[PublisherBound] Client = [1] 'test', TraceId = 1, Channel = [1] 'c' (created), Stream = [1] 'c' (created)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 11)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ServerDispose_ShouldDisposeBinding()
    {
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger() )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 2 (start)",
                            $"[ServerTrace] Client = [1] 'test', TraceId = 2, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                            "[Disposing] Client = [1] 'test', TraceId = 2",
                            "[Disposed] Client = [1] 'test', TraceId = 2",
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeBinding_WhenChannelIsOnlyBoundAsPublisherToDisconnectedClient()
    {
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger() )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 2 (start)",
                            "[Disposing] Client = [1] 'test', TraceId = 2",
                            "[Disposed] Client = [1] 'test', TraceId = 2",
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldRemoveBinding_WhenChannelIsAlsoBoundAsPublisherToOtherClient()
    {
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 1 ? clientLogs.GetLogger() : null )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 2 (start)",
                            "[Disposing] Client = [1] 'test', TraceId = 2",
                            "[Disposed] Client = [1] 'test', TraceId = 2",
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindLastClientFromChannelAndStreamAndRemoveThem()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                            "[UnbindingPublisher] Client = [1] 'test', TraceId = 2, ChannelId = 1",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                            "[PublisherUnbound] Client = [1] 'test', TraceId = 2, Channel = [1] 'c' (removed), Stream = [1] 'c' (removed)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PublisherUnboundResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PublisherUnboundResponse, Length = 6)",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherRequest, Length = 9)"
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
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindNonLastClientFromChannelAndStreamWithoutRemovingThem()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    c => c.Id == 2
                        ? clientLogs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                        endSource.Complete();
                                } ) )
                        : null )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [2] 'test2', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [2] 'test2', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                            "[UnbindingPublisher] Client = [2] 'test2', TraceId = 2, ChannelId = 1",
                            "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                            "[PublisherUnbound] Client = [2] 'test2', TraceId = 2, Channel = [1] 'c', Stream = [1] 'c'",
                            "[SendPacket:Sending] Client = [2] 'test2', TraceId = 2, Packet = (PublisherUnboundResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [2] 'test2', TraceId = 2, Packet = (PublisherUnboundResponse, Length = 6)",
                            "[Trace:UnbindPublisher] Client = [2] 'test2', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [2] 'test2'",
                        "[AwaitPacket] Client = [2] 'test2', Packet = (UnbindPublisherRequest, Length = 9)"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindLastClientFromChannelWithListenerBindingAndNotRemoveIt()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

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
                clientLogs.GetAll()
                    .Skip( 3 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (UnbindPublisherRequest, Length = 9)",
                            "[UnbindingPublisher] Client = [1] 'test', TraceId = 3, ChannelId = 1",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (UnbindPublisherRequest, Length = 9)",
                            "[PublisherUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'c', Stream = [1] 'c' (removed)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (PublisherUnboundResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (PublisherUnboundResponse, Length = 6)",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherRequest, Length = 9)"
                    ] ),
                logs.GetAllChannel().TestSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldDisposeClient_WhenClientSendsInvalidPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelEventHandlerFactory( _ => logs.Add ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 8)",
                            """
                            [Error] Client = [1] 'test', TraceId = 2
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid UnbindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected header payload to be 4 but found 3.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 2",
                            "[Disposed] Client = [1] 'test', TraceId = 2",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherRequest, Length = 8)"
                    ] ),
                logs.GetAllChannel()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by client [1::'test']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldDisposeClient_WhenClientSendsNonPositiveChannelId()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendUnbindPublisherRequest( 0 );
            } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Channels.Count.TestEquals( 0 ),
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                            """
                            [Error] Client = [1] 'test', TraceId = 2
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid UnbindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel ID to be greater than 0 but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 2",
                            "[Disposed] Client = [1] 'test', TraceId = 2",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherRequest, Length = 9)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldBeRejected_WhenChannelDoesNotExist()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (UnbindPublisherRequest, Length = 9)",
                            "[UnbindingPublisher] Client = [1] 'test', TraceId = 1, ChannelId = 1",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Client [1] 'test' could not be unbound as a publisher from non-existing channel with ID 1.
                            """,
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (UnbindPublisherFailureResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (UnbindPublisherFailureResponse, Length = 6)",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherRequest, Length = 9)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldBeRejected_WhenClientIsNotBoundAsPublisherToChannel()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    c => c.Id == 2
                        ? clientLogs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) )
                        : null )
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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [2] 'test2', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [2] 'test2', TraceId = 1, Packet = (UnbindPublisherRequest, Length = 9)",
                            "[UnbindingPublisher] Client = [2] 'test2', TraceId = 1, ChannelId = 1",
                            """
                            [Error] Client = [2] 'test2', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Client [2] 'test2' could not be unbound as a publisher from channel [1] 'c' because it is not bound as a publisher to it.
                            """,
                            "[SendPacket:Sending] Client = [2] 'test2', TraceId = 1, Packet = (UnbindPublisherFailureResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [2] 'test2', TraceId = 1, Packet = (UnbindPublisherFailureResponse, Length = 6)",
                            "[Trace:UnbindPublisher] Client = [2] 'test2', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [2] 'test2'",
                        "[AwaitPacket] Client = [2] 'test2', Packet = (UnbindPublisherRequest, Length = 9)"
                    ] ),
                logs.GetAllChannel().TestContainsSequence( [ "[1::'c'::1] [Created] by client [1::'test']" ] ) )
            .Go();
    }
}
