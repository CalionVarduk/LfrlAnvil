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
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                endSource.Complete();
                        } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
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
                channel.TestNotNull( c => Assertion.All(
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
                stream.TestNotNull( s => Assertion.All(
                    "stream",
                    s.Server.TestRefEquals( server ),
                    s.Id.TestEquals( 1 ),
                    s.Name.TestEquals( "c" ),
                    s.State.TestEquals( MessageBrokerStreamState.Running ),
                    s.ToString().TestEquals( "[1] 'c' stream (Running)" ),
                    s.Publishers.Count.TestEquals( 1 ),
                    s.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                    s.Publishers.TryGetByKey( 1, 1 ).TestRefEquals( binding ),
                    s.Messages.Count.TestEquals( 0 ),
                    s.Messages.TryGetByKey( 0 ).TestNull() ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ),
                    c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding ) ) ),
                binding.TestNotNull( b => Assertion.All(
                    "binding",
                    b.Channel.TestRefEquals( channel ),
                    b.Stream.TestRefEquals( stream ),
                    b.Client.TestRefEquals( remoteClient ),
                    b.IsEphemeral.TestTrue(),
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 9)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'c', IsEphemeral = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 9)",
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 9)"
                    ] ),
                channelLogs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Channel = [1] 'c', TraceId = 0 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 0, Correlation = (Client = [1] 'test', TraceId = 1)",
                            "[Created] Channel = [1] 'c', TraceId = 0",
                            "[PublisherBound] Channel = [1] 'c', TraceId = 0, Client = [1] 'test', Stream = [1] 'c' (created)",
                            "[Trace:BindPublisher] Channel = [1] 'c', TraceId = 0 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Stream = [1] 'c', TraceId = 0 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 0, Correlation = (Client = [1] 'test', TraceId = 1)",
                            "[Created] Stream = [1] 'c', TraceId = 0",
                            "[PublisherBound] Stream = [1] 'c', TraceId = 0, Client = [1] 'test', Channel = [1] 'c' (created)",
                            "[Trace:BindPublisher] Stream = [1] 'c', TraceId = 0 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateBindingForExistingChannelCorrectly()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 2
                    ? clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                    endSource.Complete();
                            } ) )
                    : null )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask( c =>
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
                channel.TestNotNull( c => Assertion.All(
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
                stream.TestNotNull( s => Assertion.All(
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
                remoteClient1.TestNotNull( c => Assertion.All(
                    "client1",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                    c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding1 ) ) ),
                remoteClient2.TestNotNull( c => Assertion.All(
                    "client2",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ),
                    c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding2 ) ) ),
                binding1.TestNotNull( b => Assertion.All(
                    "binding1",
                    b.Channel.TestRefEquals( channel ),
                    b.Stream.TestRefEquals( stream ),
                    b.Client.TestRefEquals( remoteClient1 ),
                    b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ) ) ),
                binding2.TestNotNull( b => Assertion.All(
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
                            "[ReadPacket:Received] Client = [2] 'test2', TraceId = 1, Packet = (BindPublisherRequest, Length = 9)",
                            "[BindingPublisher] Client = [2] 'test2', TraceId = 1, ChannelName = 'c', IsEphemeral = True",
                            "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 1, Packet = (BindPublisherRequest, Length = 9)",
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
                        "[AwaitPacket] Client = [2] 'test2', Packet = (BindPublisherRequest, Length = 9)"
                    ] ),
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [2] 'test2', TraceId = 1)",
                            "[PublisherBound] Channel = [1] 'c', TraceId = 1, Client = [2] 'test2', Stream = [1] 'c'",
                            "[Trace:BindPublisher] Channel = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Stream = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Client = [2] 'test2', TraceId = 1)",
                            "[PublisherBound] Stream = [1] 'c', TraceId = 1, Client = [2] 'test2', Channel = [1] 'c'",
                            "[Trace:BindPublisher] Stream = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateBindingForExistingStreamCorrectly()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                endSource.Complete();
                        } ) ) )
                .SetChannelLoggerFactory( c => c.Id == 2 ? channelLogs.GetLogger() : null )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "d", true, "c" );
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
                channel1.TestNotNull( c => Assertion.All(
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
                channel2.TestNotNull( c => Assertion.All(
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
                stream.TestNotNull( s => Assertion.All(
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
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 2 ),
                    c.Publishers.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                    c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding1 ),
                    c.Publishers.TryGetByChannelId( 2 ).TestRefEquals( binding2 ) ) ),
                binding1.TestNotNull( b => Assertion.All(
                    "binding1",
                    b.Channel.TestRefEquals( channel1 ),
                    b.Stream.TestRefEquals( stream ),
                    b.Client.TestRefEquals( remoteClient ),
                    b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ),
                    b.ToString().TestEquals( "[1] 'test' => [1] 'c' publisher binding (using [1] 'c' stream) (Running)" ) ) ),
                binding2.TestNotNull( b => Assertion.All(
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindPublisherRequest, Length = 10)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 2, ChannelName = 'd', StreamName = 'c', IsEphemeral = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (BindPublisherRequest, Length = 10)",
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 10)"
                    ] ),
                channelLogs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Channel = [2] 'd', TraceId = 0 (start)",
                            "[ClientTrace] Channel = [2] 'd', TraceId = 0, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[Created] Channel = [2] 'd', TraceId = 0",
                            "[PublisherBound] Channel = [2] 'd', TraceId = 0, Client = [1] 'test', Stream = [1] 'c'",
                            "[Trace:BindPublisher] Channel = [2] 'd', TraceId = 0 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Stream = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherBound] Stream = [1] 'c', TraceId = 1, Client = [1] 'test', Channel = [2] 'd' (created)",
                            "[Trace:BindPublisher] Stream = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ) )
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                or MessageBrokerRemoteClientTraceEventType.Unexpected )
                                endSource.Complete();
                        } ) ) )
                .SetChannelLoggerFactory( _ => throw exception ) );

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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 9)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'c', IsEphemeral = True",
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
                            (e, _) => e.TestEquals( "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False" ),
                            (e, _) => e.TestEquals( "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False" ),
                            (e, _) => e.TestEquals( "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (end)" )
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                or MessageBrokerRemoteClientTraceEventType.Unexpected )
                                endSource.Complete();
                        } ) ) )
                .SetStreamLoggerFactory( _ => throw exception ) );

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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 9)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'c', IsEphemeral = True",
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
                            (e, _) => e.TestEquals( "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False" ),
                            (e, _) => e.TestEquals( "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False" ),
                            (e, _) => e.TestEquals( "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (end)" )
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
    public async Task Creation_ShouldDisposeClient_WhenClientSendsInvalidPayload()
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindPublisherRequest( "c", payload: 2 ) );
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 7)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected header payload to be at least 3 but found 2.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 7)"
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected binary channel name length to be in [0, 3] range but found 4.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
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
    public async Task Creation_ShouldDisposeClient_WhenClientSendsEmptyChannelName()
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 8)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel name length to be in [1, 512] range but found 0.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 8)"
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 521)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel name length to be in [1, 512] range but found 513.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 521)"
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
        await client.GetTask( c => c.SendBindPublisherRequest( "c", true, new string( 'x', 513 ) ) );
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 522)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindPublisherRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected stream name length to be in [1, 512] range but found 513.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 522)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindPublisherRequest_ShouldBeRejected_WhenClientIsAlreadyBoundAsPublisherToChannel()
    {
        Exception? exception = null;
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                endSource.Complete();
                        },
                        error: e => exception = e.Exception ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
            c.SendBindPublisherRequest( "c", true, "d" );
            c.ReadBindPublisherFailureResponse();
        } );

        await endSource.Task;

        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Publishers.TryGetByClientId( 1 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient ),
                        exc.Publisher.TestRefEquals( binding ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                server.Streams.Count.TestEquals( 1 ),
                channel.TestNotNull( c => Assertion.All(
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindPublisherRequest, Length = 10)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 2, ChannelName = 'c', StreamName = 'd', IsEphemeral = True",
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 9)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 10)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ServerDispose_ShouldDisposeBinding()
    {
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();
        var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger() )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
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
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty() ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty() ) ),
                stream.TestNotNull( s => Assertion.All(
                    "stream",
                    s.State.TestEquals( MessageBrokerStreamState.Disposed ),
                    s.Publishers.Count.TestEquals( 0 ),
                    s.Publishers.GetAll().TestEmpty() ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Channel = [1] 'c', TraceId = 1 (start)",
                            $"[ServerTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                            "[Disposing] Channel = [1] 'c', TraceId = 1",
                            "[Disposed] Channel = [1] 'c', TraceId = 1",
                            "[Trace:Dispose] Channel = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Stream = [1] 'c', TraceId = 1 (start)",
                            $"[ServerTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                            "[Disposing] Stream = [1] 'c', TraceId = 1",
                            "[Disposed] Stream = [1] 'c', TraceId = 1",
                            "[Trace:Dispose] Stream = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 2 (start)",
                            $"[ServerTrace] Client = [1] 'test', TraceId = 2, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                            "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldDisposeBinding_WhenChannelIsOnlyBoundAsPublisherToDisconnectedClient()
    {
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger() )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
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
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty() ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty() ) ),
                stream.TestNotNull( s => Assertion.All(
                    "stream",
                    s.State.TestEquals( MessageBrokerStreamState.Disposed ),
                    s.Publishers.Count.TestEquals( 0 ),
                    s.Publishers.GetAll().TestEmpty() ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Channel = [1] 'c', TraceId = 1, Client = [1] 'test', Stream = [1] 'c'",
                            "[Disposing] Channel = [1] 'c', TraceId = 1",
                            "[Disposed] Channel = [1] 'c', TraceId = 1",
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Stream = [1] 'c', TraceId = 1, Client = [1] 'test', Channel = [1] 'c'",
                            "[Disposing] Stream = [1] 'c', TraceId = 1",
                            "[Disposed] Stream = [1] 'c', TraceId = 1",
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 2 (start)",
                            "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ClientDisconnect_ShouldRemoveBinding_WhenChannelIsAlsoBoundAsPublisherToOtherClient()
    {
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 1 ? clientLogs.GetLogger() : null )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask( c =>
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
                remoteClient1.TestNotNull( c => Assertion.All(
                    "client1",
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty() ) ),
                remoteClient2.TestNotNull( c => Assertion.All(
                    "client2",
                    c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ) ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ) ) ),
                stream.TestNotNull( s => Assertion.All(
                    "stream",
                    s.State.TestEquals( MessageBrokerStreamState.Running ),
                    s.Publishers.Count.TestEquals( 1 ),
                    s.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding2 ) ] ) ) ),
                binding1.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                channelLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 2, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Channel = [1] 'c', TraceId = 2, Client = [1] 'test', Stream = [1] 'c'",
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 2, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Stream = [1] 'c', TraceId = 2, Client = [1] 'test', Channel = [1] 'c'",
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 2 (start)",
                            "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindLastClientFromChannelAndStreamAndRemoveThem()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                endSource.Complete();
                        } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var stream = server.Streams.TryGetByName( "c" );
        var binding = channel?.Publishers.TryGetByClientId( 1 );
        await client.GetTask( c =>
        {
            c.SendUnbindPublisherRequest( 1 );
            c.ReadPublisherUnboundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty() ) ),
                stream.TestNotNull( s => Assertion.All(
                    "stream",
                    s.State.TestEquals( MessageBrokerStreamState.Disposed ),
                    s.Publishers.Count.TestEquals( 0 ),
                    s.Publishers.GetAll().TestEmpty() ) ),
                remoteClient.TestNotNull( c => Assertion.All(
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
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Channel = [1] 'c', TraceId = 1, Client = [1] 'test', Stream = [1] 'c' (removed)",
                            "[Disposing] Channel = [1] 'c', TraceId = 1",
                            "[Disposed] Channel = [1] 'c', TraceId = 1",
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Stream = [1] 'c', TraceId = 1, Client = [1] 'test', Channel = [1] 'c' (removed)",
                            "[Disposing] Stream = [1] 'c', TraceId = 1",
                            "[Disposed] Stream = [1] 'c', TraceId = 1",
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindNonLastClientFromChannelAndStreamWithoutRemovingThem()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 2
                    ? clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                    endSource.Complete();
                            } ) )
                    : null )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask( c =>
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

        await client2.GetTask( c =>
        {
            c.SendUnbindPublisherRequest( 1 );
            c.ReadPublisherUnboundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Server.TestRefEquals( server ),
                    c.Id.TestEquals( 1 ),
                    c.Name.TestEquals( "c" ),
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                    c.Publishers.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                    c.Publishers.TryGetByClientId( 2 ).TestNull() ) ),
                stream.TestNotNull( s => Assertion.All(
                    "stream",
                    s.Server.TestRefEquals( server ),
                    s.Id.TestEquals( 1 ),
                    s.Name.TestEquals( "c" ),
                    s.State.TestEquals( MessageBrokerStreamState.Running ),
                    s.Publishers.Count.TestEquals( 1 ),
                    s.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                    s.Publishers.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ),
                    s.Publishers.TryGetByKey( 2, 1 ).TestNull() ) ),
                remoteClient1.TestNotNull( c => Assertion.All(
                    "client1",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding1 ) ] ),
                    c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding1 ) ) ),
                remoteClient2.TestNotNull( c => Assertion.All(
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
                channelLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 2, Correlation = (Client = [2] 'test2', TraceId = 2)",
                            "[PublisherUnbound] Channel = [1] 'c', TraceId = 2, Client = [2] 'test2', Stream = [1] 'c'",
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 2, Correlation = (Client = [2] 'test2', TraceId = 2)",
                            "[PublisherUnbound] Stream = [1] 'c', TraceId = 2, Client = [2] 'test2', Channel = [1] 'c'",
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindLastClientFromChannelWithListenerBindingAndNotRemoveIt()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                endSource.Complete();
                        } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
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
        await client.GetTask( c =>
        {
            c.SendUnbindPublisherRequest( 1 );
            c.ReadPublisherUnboundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty(),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
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
                channelLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 2, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[PublisherUnbound] Channel = [1] 'c', TraceId = 2, Client = [1] 'test', Stream = [1] 'c' (removed)",
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldDisposeClient_WhenClientSendsInvalidPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                endSource.Complete();
                        } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
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
                            "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherRequest, Length = 8)"
                    ] ),
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Channel = [1] 'c', TraceId = 1, Client = [1] 'test', Stream = [1] 'c'",
                            "[Disposing] Channel = [1] 'c', TraceId = 1",
                            "[Disposed] Channel = [1] 'c', TraceId = 1",
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 1 (end)"
                        ] )
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
        await client.GetTask( c =>
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
                            "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
        await client.GetTask( c =>
        {
            c.SendUnbindPublisherRequest( 1 );
            c.ReadUnbindPublisherFailureResponse();
        } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient ),
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
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 2
                    ? clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) )
                    : null ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask( c =>
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
                    .Exact<MessageBrokerChannelPublisherBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient2 ),
                        exc.Publisher.TestNull() ) ),
                remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                channel.TestNotNull( c => Assertion.All(
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
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindNonEphemeralPublisher()
    {
        using var storage = StorageScope.Create();

        var endSource = new SafeTaskCompletionSource();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                    traceEnd: e =>
                    {
                        if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                            endSource.Complete();
                    } ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c", isEphemeral: false );
            c.ReadPublisherBoundResponse();
        } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );
        await client.GetTask( c =>
        {
            c.SendUnbindPublisherRequest( 1 );
            c.ReadPublisherUnboundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty() ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                storage.FileExists( StorageScope.GetStreamMetadataSubpath( streamId: 1 ) ).TestFalse(),
                storage.FileExists( StorageScope.GetChannelMetadataSubpath( channelId: 1 ) ).TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public async Task Unbind_ShouldUnbindInactivePublisher(bool isClientEphemeral)
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

        var endSource = new SafeTaskCompletionSource();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                    traceEnd: e =>
                    {
                        if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                            endSource.Complete();
                    } ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: isClientEphemeral );
        await client.GetTask( c =>
        {
            c.SendUnbindPublisherRequest( channelId: 1 );
            c.ReadPublisherUnboundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ),
                    p.IsEphemeral.TestEquals( isClientEphemeral ) ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse() )
            .Go();
    }

    [Fact]
    public async Task UnbindByName_ShouldUnbindLastClientFromChannelAndStreamAndRemoveThem()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                endSource.Complete();
                        } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var stream = server.Streams.TryGetByName( "c" );
        var binding = channel?.Publishers.TryGetByClientId( 1 );
        await client.GetTask( c =>
        {
            c.SendUnbindPublisherByNameRequest( "c" );
            c.ReadPublisherUnboundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty() ) ),
                stream.TestNotNull( s => Assertion.All(
                    "stream",
                    s.State.TestEquals( MessageBrokerStreamState.Disposed ),
                    s.Publishers.Count.TestEquals( 0 ),
                    s.Publishers.GetAll().TestEmpty() ) ),
                remoteClient.TestNotNull( c => Assertion.All(
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherByNameRequest, Length = 6)",
                            "[UnbindingPublisher] Client = [1] 'test', TraceId = 2, ChannelName = 'c'",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherByNameRequest, Length = 6)",
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherByNameRequest, Length = 6)"
                    ] ),
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Channel = [1] 'c', TraceId = 1, Client = [1] 'test', Stream = [1] 'c' (removed)",
                            "[Disposing] Channel = [1] 'c', TraceId = 1",
                            "[Disposed] Channel = [1] 'c', TraceId = 1",
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Stream = [1] 'c', TraceId = 1, Client = [1] 'test', Channel = [1] 'c' (removed)",
                            "[Disposing] Stream = [1] 'c', TraceId = 1",
                            "[Disposed] Stream = [1] 'c', TraceId = 1",
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 513 )]
    public async Task UnbindByName_ShouldDisposeClient_WhenClientSendsInvalidChannelName(int nameLength)
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var channelName = new string( 'x', nameLength );
        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendUnbindPublisherByNameRequest( channelName ) );
        await endSource.Task;

        var requestLength = channelName.Length + 5;

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
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                            $"[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (UnbindPublisherByNameRequest, Length = {requestLength})",
                            $"""
                             [Error] Client = [1] 'test', TraceId = 1
                             LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid UnbindPublisherByNameRequest from client [1] 'test'. Encountered 1 error(s):
                             1. Expected channel name length to be in [1, 512] range but found {nameLength}.
                             """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        $"[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherByNameRequest, Length = {requestLength})"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task UnbindByName_ShouldBeRejected_WhenChannelDoesNotExist()
    {
        Exception? exception = null;
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                endSource.Complete();
                        },
                        error: e => exception = e.Exception ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );

        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c =>
        {
            c.SendUnbindPublisherByNameRequest( "foo" );
            c.ReadUnbindPublisherFailureResponse();
        } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient ),
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (UnbindPublisherByNameRequest, Length = 8)",
                            "[UnbindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo'",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Client [1] 'test' could not be unbound as a publisher from non-existing channel 'foo'.
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherByNameRequest, Length = 8)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task UnbindByName_ShouldBeRejected_WhenClientIsNotBoundAsPublisherToChannel()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 2
                    ? clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) )
                    : null ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask( c =>
        {
            c.SendUnbindPublisherByNameRequest( "c" );
            c.ReadUnbindPublisherFailureResponse();
        } );

        await endSource.Task;

        var channel = server.Channels.TryGetByName( "c" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient2 ),
                        exc.Publisher.TestNull() ) ),
                remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                channel.TestNotNull( c => Assertion.All(
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
                            "[ReadPacket:Received] Client = [2] 'test2', TraceId = 1, Packet = (UnbindPublisherByNameRequest, Length = 6)",
                            "[UnbindingPublisher] Client = [2] 'test2', TraceId = 1, ChannelName = 'c'",
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
                        "[AwaitPacket] Client = [2] 'test2', Packet = (UnbindPublisherByNameRequest, Length = 6)"
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public async Task UnbindByName_ShouldUnbindInactivePublisher(bool isClientEphemeral)
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

        var endSource = new SafeTaskCompletionSource();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                    traceEnd: e =>
                    {
                        if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                            endSource.Complete();
                    } ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: isClientEphemeral );
        await client.GetTask( c =>
        {
            c.SendUnbindPublisherByNameRequest( "foo" );
            c.ReadPublisherUnboundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ),
                    p.IsEphemeral.TestEquals( isClientEphemeral ) ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse() )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveNonEphemeralPublisher()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "foo", isEphemeral: false );
            c.ReadPublisherBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 1 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ),
                    p.IsEphemeral.TestFalse() ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 11)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', IsEphemeral = False",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 11)",
                            "[PublisherBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [1] 'foo' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (PublisherBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (PublisherBoundResponse, Length = 14)",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveNonEphemeralPublisher_AsEphemeral()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "foo", isEphemeral: true );
            c.ReadPublisherBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 1 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ),
                    p.IsEphemeral.TestTrue() ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 11)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', IsEphemeral = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 11)",
                            "[PublisherBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [1] 'foo' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (PublisherBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (PublisherBoundResponse, Length = 14)",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveEphemeralPublisher()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: true );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "foo", isEphemeral: false );
            c.ReadPublisherBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 1 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ),
                    p.IsEphemeral.TestTrue() ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 11)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', IsEphemeral = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 11)",
                            "[PublisherBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [1] 'foo' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (PublisherBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (PublisherBoundResponse, Length = 14)",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactivePublisher_ToDifferentNewStream()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

        var endSource = new SafeTaskCompletionSource();
        var channelLogs = new ChannelEventLogger();
        var clientLogs = new ClientEventLogger();
        var streamLogs = new[] { new StreamEventLogger(), new StreamEventLogger() };

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( s => streamLogs[s.Id - 1].GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var channel = server.Channels.TryGetById( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "foo", streamName: "bar", isEphemeral: true );
            c.ReadPublisherBoundResponse();
        } );

        await endSource.Task;
        var newBinding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        Assertion.All(
                server.Streams.Count.TestEquals( 1 ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ newBinding ] ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ newBinding ] ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ),
                    p.Stream.Publishers.Count.TestEquals( 0 ),
                    p.Stream.State.TestEquals( MessageBrokerStreamState.Disposed ) ) ),
                newBinding.TestNotNull( p => Assertion.All(
                    "newPublisher",
                    p.TestNotRefEquals( binding ),
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ),
                    p.IsEphemeral.TestTrue(),
                    p.Stream.Publishers.GetAll().TestSequence( [ p ] ),
                    p.Stream.State.TestEquals( MessageBrokerStreamState.Running ),
                    p.Stream.Id.TestEquals( 2 ),
                    p.Stream.Name.TestEquals( "bar" ) ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                channelLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Channel = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Channel = [1] 'foo', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[PublisherUnbound] Channel = [1] 'foo', TraceId = 3, Client = [1] 'test', Stream = [1] 'foo' (removed)",
                            "[PublisherBound] Channel = [1] 'foo', TraceId = 3, Client = [1] 'test', Stream = [2] 'bar' (created) (reactivated)",
                            "[Trace:BindPublisher] Channel = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                streamLogs[0]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Stream = [1] 'foo', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[PublisherUnbound] Stream = [1] 'foo', TraceId = 3, Client = [1] 'test', Channel = [1] 'foo'",
                            "[Disposing] Stream = [1] 'foo', TraceId = 3",
                            "[Disposed] Stream = [1] 'foo', TraceId = 3",
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                streamLogs[1]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Stream = [2] 'bar', TraceId = 0 (start)",
                            "[ClientTrace] Stream = [2] 'bar', TraceId = 0, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[Created] Stream = [2] 'bar', TraceId = 0",
                            "[PublisherBound] Stream = [2] 'bar', TraceId = 0, Client = [1] 'test', Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindPublisher] Stream = [2] 'bar', TraceId = 0 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 14)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', StreamName = 'bar', IsEphemeral = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 14)",
                            "[PublisherUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [1] 'foo' (removed)",
                            "[PublisherBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [2] 'bar' (created) (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (PublisherBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (PublisherBoundResponse, Length = 14)",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactivePublisher_ToDifferentExistingStream()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteChannelMetadata( channelId: 2, channelName: "qux" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteStreamMetadata( streamId: 2, streamName: "bar" );
        storage.WriteStreamMessages( streamId: 2, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );
        storage.WriteClientMetadata( clientId: 2, clientName: "other" );
        storage.WritePublisherMetadata( clientId: 2, channelId: 1, streamId: 2 );
        storage.WritePublisherMetadata( clientId: 2, channelId: 2, streamId: 1 );

        var endSource = new SafeTaskCompletionSource();
        var channelLogs = new ChannelEventLogger();
        var clientLogs = new ClientEventLogger();
        var streamLogs = new[] { new StreamEventLogger(), new StreamEventLogger() };

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( s => streamLogs[s.Id - 1].GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var channel = server.Channels.TryGetById( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "foo", streamName: "bar", isEphemeral: false );
            c.ReadPublisherBoundResponse();
        } );

        await endSource.Task;
        var newBinding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        Assertion.All(
                server.Streams.Count.TestEquals( 2 ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Publishers.Count.TestEquals( 2 ),
                    c.Publishers.GetAll().TestContainsSequence( [ newBinding ] ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ newBinding ] ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ),
                    p.Stream.Publishers.Count.TestEquals( 1 ),
                    p.Stream.State.TestEquals( MessageBrokerStreamState.Running ) ) ),
                newBinding.TestNotNull( p => Assertion.All(
                    "newPublisher",
                    p.TestNotRefEquals( binding ),
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ),
                    p.IsEphemeral.TestFalse(),
                    p.Stream.Publishers.GetAll().TestContainsSequence( [ p ] ),
                    p.Stream.State.TestEquals( MessageBrokerStreamState.Running ),
                    p.Stream.Id.TestEquals( 2 ),
                    p.Stream.Name.TestEquals( "bar" ) ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                channelLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Channel = [1] 'foo', TraceId = 4 (start)",
                            "[ClientTrace] Channel = [1] 'foo', TraceId = 4, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[PublisherUnbound] Channel = [1] 'foo', TraceId = 4, Client = [1] 'test', Stream = [1] 'foo'",
                            "[PublisherBound] Channel = [1] 'foo', TraceId = 4, Client = [1] 'test', Stream = [2] 'bar' (reactivated)",
                            "[Trace:BindPublisher] Channel = [1] 'foo', TraceId = 4 (end)"
                        ] )
                    ] ),
                streamLogs[0]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 4 (start)",
                            "[ClientTrace] Stream = [1] 'foo', TraceId = 4, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[PublisherUnbound] Stream = [1] 'foo', TraceId = 4, Client = [1] 'test', Channel = [1] 'foo'",
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 4 (end)"
                        ] )
                    ] ),
                streamLogs[1]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Stream = [2] 'bar', TraceId = 3 (start)",
                            "[ClientTrace] Stream = [2] 'bar', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[PublisherBound] Stream = [2] 'bar', TraceId = 3, Client = [1] 'test', Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindPublisher] Stream = [2] 'bar', TraceId = 3 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 14)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', StreamName = 'bar', IsEphemeral = False",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 14)",
                            "[PublisherUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [1] 'foo'",
                            "[PublisherBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [2] 'bar' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (PublisherBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (PublisherBoundResponse, Length = 14)",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldFailAndDeactivateClient_WhenNewStreamLoggerFactoryThrows()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetStreamLoggerFactory( s => s.Id == 1 ? null : throw new Exception( "foo" ) )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                or MessageBrokerRemoteClientTraceEventType.Unexpected )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var channel = server.Channels.TryGetById( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c => c.SendBindPublisherRequest( "foo", streamName: "bar", isEphemeral: true ) );

        await endSource.Task;

        Assertion.All(
                server.Streams.Count.TestEquals( 1 ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestContainsSequence( [ binding ] ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ binding ] ),
                    c.State.TestEquals( MessageBrokerRemoteClientState.Inactive ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Inactive ),
                    p.Stream.Publishers.Count.TestEquals( 1 ),
                    p.Stream.State.TestEquals( MessageBrokerStreamState.Running ) ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                clientLogs.GetAll()
                    .TakeLast( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindPublisherRequest, Length = 14)",
                            "[BindingPublisher] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', StreamName = 'bar', IsEphemeral = True",
                            "[Trace:BindPublisher] Client = [1] 'test', TraceId = 3 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            (e, _) => e.TestEquals( "[Trace:Unexpected] Client = [1] 'test', TraceId = 4 (start)" ),
                            (e, _) => e.TestStartsWith(
                                """
                                [Error] Client = [1] 'test', TraceId = 4
                                System.Exception: foo
                                """ ),
                            (e, _) => e.TestEquals( "[Deactivating] Client = [1] 'test', TraceId = 4, IsAlive = True" ),
                            (e, _) => e.TestEquals( "[Deactivated] Client = [1] 'test', TraceId = 4, IsAlive = True" ),
                            (e, _) => e.TestEquals( "[Trace:Unexpected] Client = [1] 'test', TraceId = 4 (end)" )
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeletePublisherAndSendSystemNotificationToClient()
    {
        var bindEndSource = new SafeTaskCompletionSource();
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                bindEndSource.Complete();
                            else if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "foo" );
            c.ReadPublisherBoundResponse();
        } );

        await bindEndSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );
        var deleteTask = binding?.DeleteAsync().AsTask() ?? Task.CompletedTask;

        await client.GetTask( c => c.ReadPublisherDeletedSystemNotification( "foo" ) );
        await deleteTask;
        await endSource.Task;

        var action = () => binding?.DeleteAsync().AsTask() ?? Task.CompletedTask;

        Assertion.All(
                action.Test( exc => exc.TestNull() ),
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ),
                    p.Channel.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    p.Stream.State.TestEquals( MessageBrokerStreamState.Disposed ) ) ),
                channelLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'foo', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'foo', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Channel = [1] 'foo', TraceId = 1, Client = [1] 'test', Stream = [1] 'foo' (removed)",
                            "[Disposing] Channel = [1] 'foo', TraceId = 1",
                            "[Disposed] Channel = [1] 'foo', TraceId = 1",
                            "[Trace:UnbindPublisher] Channel = [1] 'foo', TraceId = 1 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'foo', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Stream = [1] 'foo', TraceId = 1, Client = [1] 'test', Channel = [1] 'foo' (removed)",
                            "[Disposing] Stream = [1] 'foo', TraceId = 1",
                            "[Disposed] Stream = [1] 'foo', TraceId = 1",
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                            "[PublisherUnbound] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo' (removed), Stream = [1] 'foo' (removed)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 9)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 9)",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteNonEphemeralPublisherAndSendSystemNotificationToClient()
    {
        using var storage = StorageScope.Create();
        var bindEndSource = new SafeTaskCompletionSource();
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                bindEndSource.Complete();
                            else if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "foo", isEphemeral: false );
            c.ReadPublisherBoundResponse();
        } );

        await bindEndSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );
        var deleteTask = binding?.DeleteAsync().AsTask() ?? Task.CompletedTask;

        await client.GetTask( c => c.ReadPublisherDeletedSystemNotification( "foo" ) );
        await deleteTask;
        await endSource.Task;

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ),
                    p.Channel.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    p.Stream.State.TestEquals( MessageBrokerStreamState.Disposed ) ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                channelLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'foo', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'foo', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Channel = [1] 'foo', TraceId = 1, Client = [1] 'test', Stream = [1] 'foo' (removed)",
                            "[Disposing] Channel = [1] 'foo', TraceId = 1",
                            "[Disposed] Channel = [1] 'foo', TraceId = 1",
                            "[Trace:UnbindPublisher] Channel = [1] 'foo', TraceId = 1 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'foo', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Stream = [1] 'foo', TraceId = 1, Client = [1] 'test', Channel = [1] 'foo' (removed)",
                            "[Disposing] Stream = [1] 'foo', TraceId = 1",
                            "[Disposed] Stream = [1] 'foo', TraceId = 1",
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                            "[PublisherUnbound] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo' (removed), Stream = [1] 'foo' (removed)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 9)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 9)",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteInactivePublisherAndSendSystemNotificationToClient()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );
        var deleteTask = binding?.DeleteAsync().AsTask() ?? Task.CompletedTask;

        await client.GetTask( c => c.ReadPublisherDeletedSystemNotification( "foo" ) );
        await deleteTask;
        await endSource.Task;

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ),
                    p.Channel.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    p.Stream.State.TestEquals( MessageBrokerStreamState.Disposed ) ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                channelLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Channel = [1] 'foo', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[PublisherUnbound] Channel = [1] 'foo', TraceId = 3, Client = [1] 'test', Stream = [1] 'foo' (removed)",
                            "[Disposing] Channel = [1] 'foo', TraceId = 3",
                            "[Disposed] Channel = [1] 'foo', TraceId = 3",
                            "[Trace:UnbindPublisher] Channel = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Stream = [1] 'foo', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[PublisherUnbound] Stream = [1] 'foo', TraceId = 3, Client = [1] 'test', Channel = [1] 'foo' (removed)",
                            "[Disposing] Stream = [1] 'foo', TraceId = 3",
                            "[Disposed] Stream = [1] 'foo', TraceId = 3",
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 3 (start)",
                            "[PublisherUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo' (removed), Stream = [1] 'foo' (removed)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (SystemNotification, Length = 9)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (SystemNotification, Length = 9)",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteInactivePublisherAndNotSendSystemNotificationToInactiveClient()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var streamLogs = new StreamEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );
        await (binding?.DeleteAsync().AsTask() ?? Task.CompletedTask);
        await endSource.Task;

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                server.Streams.Count.TestEquals( 0 ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ),
                    p.Channel.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    p.Stream.State.TestEquals( MessageBrokerStreamState.Disposed ) ) ),
                storage.FileExists( StorageScope.GetPublisherMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                channelLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Channel = [1] 'foo', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Channel = [1] 'foo', TraceId = 3, Client = [1] 'test', Stream = [1] 'foo' (removed)",
                            "[Disposing] Channel = [1] 'foo', TraceId = 3",
                            "[Disposed] Channel = [1] 'foo', TraceId = 3",
                            "[Trace:UnbindPublisher] Channel = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Stream = [1] 'foo', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Stream = [1] 'foo', TraceId = 3, Client = [1] 'test', Channel = [1] 'foo' (removed)",
                            "[Disposing] Stream = [1] 'foo', TraceId = 3",
                            "[Disposed] Stream = [1] 'foo', TraceId = 3",
                            "[Trace:UnbindPublisher] Stream = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                            "[PublisherUnbound] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo' (removed), Stream = [1] 'foo' (removed)",
                            "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }
}
