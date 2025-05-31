using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerChannelListenerBindingTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public MessageBrokerChannelListenerBindingTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task Creation_ShouldCreateBindingCorrectly()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 15)",
                            "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 15)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 1, Channel = [1] 'c' (created), Queue = [1] 'c' (created)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 15)"
                    ] ),
                channelLogs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Channel = [1] 'c', TraceId = 0 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 0, Correlation = (Client = [1] 'test', TraceId = 1)",
                            "[Created] Channel = [1] 'c', TraceId = 0",
                            "[ListenerBound] Channel = [1] 'c', TraceId = 0, Client = [1] 'test', Queue = [1] 'c' (created), PrefetchHint = 1",
                            "[Trace:BindListener] Channel = [1] 'c', TraceId = 0 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 0 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 0, ClientTraceId = 1",
                            "[Created] Client = [1] 'test', Queue = [1] 'c', TraceId = 0",
                            "[ListenerBound] Client = [1] 'test', Queue = [1] 'c', TraceId = 0, Channel = [1] 'c' (created), PrefetchHint = 1",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 0 (end)"
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
        var queueLogs = new QueueEventLogger();
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 15)",
                            "[BindingListener] Client = [1] 'test', TraceId = 2, ChannelName = 'c', PrefetchHint = 10, CreateChannelIfNotExists = False",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 15)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 2, Channel = [1] 'c', Queue = [1] 'c' (created)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 15)"
                    ] ),
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[ListenerBound] Channel = [1] 'c', TraceId = 1, Client = [1] 'test', Queue = [1] 'c' (created), PrefetchHint = 10",
                            "[Trace:BindListener] Channel = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 0 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 0, ClientTraceId = 2",
                            "[Created] Client = [1] 'test', Queue = [1] 'c', TraceId = 0",
                            "[ListenerBound] Client = [1] 'test', Queue = [1] 'c', TraceId = 0, Channel = [1] 'c', PrefetchHint = 10",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 0 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldCreateBindingForExistingQueueCorrectly()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelLoggerFactory( c => c.Id == 2 ? channelLogs.GetLogger() : null )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 16)",
                            "[BindingListener] Client = [1] 'test', TraceId = 2, ChannelName = 'd', QueueName = 'c', PrefetchHint = 1, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 16)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 2, Channel = [2] 'd' (created), Queue = [1] 'c'",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 15)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 16)"
                    ] ),
                channelLogs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Channel = [2] 'd', TraceId = 0 (start)",
                            "[ClientTrace] Channel = [2] 'd', TraceId = 0, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[Created] Channel = [2] 'd', TraceId = 0",
                            "[ListenerBound] Channel = [2] 'd', TraceId = 0, Client = [1] 'test', Queue = [1] 'c', PrefetchHint = 1",
                            "[Trace:BindListener] Channel = [2] 'd', TraceId = 0 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[ListenerBound] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, Channel = [2] 'd' (created), PrefetchHint = 1",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (end)"
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
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindListener
                                    or MessageBrokerRemoteClientTraceEventType.Unexpected )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelLoggerFactory( _ => throw exception ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 15)",
                            "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, CreateChannelIfNotExists = True",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 15)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenQueueEventHandlerFactoryThrowsForCreatedQueue()
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
                                if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindListener
                                    or MessageBrokerRemoteClientTraceEventType.Unexpected )
                                    endSource.Complete();
                            } ) ) )
                .SetQueueLoggerFactory( _ => throw exception ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 15)",
                            "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, CreateChannelIfNotExists = True",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 15)"
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true, payload: 8 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 13)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected header payload to be at least 9 but found 8.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 13)"
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    endSource.Complete();
                            } ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 15)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected binary channel name length to be in [0, 1] range but found -1.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 15)"
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindListenerRequest( "foo", createChannelIfNotExists: true, channelNameLength: 4 ) );
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 17)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected binary channel name length to be in [0, 3] range but found 4.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 17)"
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    endSource.Complete();
                            } ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 14)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel name length to be in [1, 512] range but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 14)"
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    endSource.Complete();
                            } ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 527)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel name length to be in [1, 512] range but found 513.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 527)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsTooLongQueueName()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    endSource.Complete();
                            } ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 528)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected queue name length to be in [1, 512] range but found 513.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 528)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsInvalidPrefetchHint()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    endSource.Complete();
                            } ) ) ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 15)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected prefetch hint to be greater than 0 but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 15)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindListenerRequest_ShouldBeRejected_WhenClientIsAlreadyBoundAsListenerToChannel()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 15)",
                            "[BindingListener] Client = [1] 'test', TraceId = 2, ChannelName = 'c', PrefetchHint = 1, CreateChannelIfNotExists = False",
                            """
                            [Error] Client = [1] 'test', TraceId = 2
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelListenerBindingException: Client [1] 'test' could not be bound as a listener to channel [1] 'c' because it is already bound as a listener to it.
                            """,
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (BindListenerFailureResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (BindListenerFailureResponse, Length = 6)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 15)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 15)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindListenerRequest_ShouldBeRejected_WhenChannelDoesNotExistAndIsNotCreatedDuringListenerBinding()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 15)",
                            "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, CreateChannelIfNotExists = False",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelListenerBindingException: Client [1] 'test' could not be bound as a listener to channel 'c' because channel does not exist.
                            """,
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerFailureResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerFailureResponse, Length = 6)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 15)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task ServerDispose_ShouldDisposeBinding()
    {
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
        var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger() )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                queueLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[Disposing] Client = [1] 'test', Queue = [1] 'c', TraceId = 1",
                            "[Disposed] Client = [1] 'test', Queue = [1] 'c', TraceId = 1",
                            "[Trace:Dispose] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (end)"
                        ] )
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
    public async Task ClientDisconnect_ShouldDisposeChannelAndBinding_WhenChannelIsOnlyBoundAsPublisherAndListenerToDisconnectedClient()
    {
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger() )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                channelLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 2, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[PublisherUnbound] Channel = [1] 'c', TraceId = 2, Client = [1] 'test', Stream = [1] 'c'",
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 2 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 3 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[ListenerUnbound] Channel = [1] 'c', TraceId = 3, Client = [1] 'test', Queue = [1] 'c'",
                            "[Disposing] Channel = [1] 'c', TraceId = 3",
                            "[Disposed] Channel = [1] 'c', TraceId = 3",
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, ClientTraceId = 3",
                            "[Disposing] Client = [1] 'test', Queue = [1] 'c', TraceId = 1",
                            "[Disposed] Client = [1] 'test', Queue = [1] 'c', TraceId = 1",
                            "[Trace:Dispose] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .Skip( 3 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 3 (start)",
                            "[Disposing] Client = [1] 'test', TraceId = 3",
                            "[Disposed] Client = [1] 'test', TraceId = 3",
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task
        ClientDisconnect_ShouldDisposeBinding_WhenChannelIsBoundAsPublisherToAnotherClientAndBoundAsListenerToDisconnectedClient()
    {
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 2 ? clientLogs.GetLogger() : null )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                channelLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 2, Correlation = (Client = [2] 'test2', TraceId = 2)",
                            "[ListenerUnbound] Channel = [1] 'c', TraceId = 2, Client = [2] 'test2', Queue = [1] 'c'",
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[Disposing] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1",
                            "[Disposed] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1",
                            "[Trace:Dispose] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [2] 'test2', TraceId = 2 (start)",
                            "[Disposing] Client = [2] 'test2', TraceId = 2",
                            "[Disposed] Client = [2] 'test2', TraceId = 2",
                            "[Trace:Dispose] Client = [2] 'test2', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task
        ClientDisconnect_ShouldDisposeChannelAndBinding_WhenChannelIsNotBoundAsPublisherToAnyClientAndBoundAsListenerOnlyToDisconnectedClient()
    {
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger() )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[ListenerUnbound] Channel = [1] 'c', TraceId = 1, Client = [1] 'test', Queue = [1] 'c'",
                            "[Disposing] Channel = [1] 'c', TraceId = 1",
                            "[Disposed] Channel = [1] 'c', TraceId = 1",
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[Disposing] Client = [1] 'test', Queue = [1] 'c', TraceId = 1",
                            "[Disposed] Client = [1] 'test', Queue = [1] 'c', TraceId = 1",
                            "[Trace:Dispose] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (end)"
                        ] )
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
    public async Task ClientDisconnect_ShouldDisposeBinding_WhenChannelIsNotBoundAsPublisherToAnyClientAndBoundAsListenerToAnotherClient()
    {
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 2 ? clientLogs.GetLogger() : null )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( q => q.Client.Id == 2 ? queueLogs.GetLogger() : null ) );

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
                channelLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 2, Correlation = (Client = [2] 'test2', TraceId = 2)",
                            "[ListenerUnbound] Channel = [1] 'c', TraceId = 2, Client = [2] 'test2', Queue = [1] 'c'",
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[Disposing] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1",
                            "[Disposed] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1",
                            "[Trace:Dispose] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [2] 'test2', TraceId = 2 (start)",
                            "[Disposing] Client = [2] 'test2', TraceId = 2",
                            "[Disposed] Client = [2] 'test2', TraceId = 2",
                            "[Trace:Dispose] Client = [2] 'test2', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindLastClientFromChannelAndQueueAndRemoveThem()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                            "[UnbindingListener] Client = [1] 'test', TraceId = 2, ChannelId = 1",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                            "[ListenerUnbound] Client = [1] 'test', TraceId = 2, Channel = [1] 'c' (removed), Queue = [1] 'c' (removed)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (ListenerUnboundResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (ListenerUnboundResponse, Length = 6)",
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerRequest, Length = 9)"
                    ] ),
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[ListenerUnbound] Channel = [1] 'c', TraceId = 1, Client = [1] 'test', Queue = [1] 'c' (removed)",
                            "[Disposing] Channel = [1] 'c', TraceId = 1",
                            "[Disposed] Channel = [1] 'c', TraceId = 1",
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, Channel = [1] 'c' (removed)",
                            "[Disposing] Client = [1] 'test', Queue = [1] 'c', TraceId = 1",
                            "[Disposed] Client = [1] 'test', Queue = [1] 'c', TraceId = 1",
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindNonLastClientFromChannelWithoutRemovingIt()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                        endSource.Complete();
                                } ) )
                        : null )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( q => q.Client.Id == 2 ? queueLogs.GetLogger() : null ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [2] 'test2', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [2] 'test2', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                            "[UnbindingListener] Client = [2] 'test2', TraceId = 2, ChannelId = 1",
                            "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                            "[ListenerUnbound] Client = [2] 'test2', TraceId = 2, Channel = [1] 'c', Queue = [1] 'c' (removed)",
                            "[SendPacket:Sending] Client = [2] 'test2', TraceId = 2, Packet = (ListenerUnboundResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [2] 'test2', TraceId = 2, Packet = (ListenerUnboundResponse, Length = 6)",
                            "[Trace:UnbindListener] Client = [2] 'test2', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [2] 'test2'",
                        "[AwaitPacket] Client = [2] 'test2', Packet = (UnbindListenerRequest, Length = 9)"
                    ] ),
                channelLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 2, Correlation = (Client = [2] 'test2', TraceId = 2)",
                            "[ListenerUnbound] Channel = [1] 'c', TraceId = 2, Client = [2] 'test2', Queue = [1] 'c' (removed)",
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[ListenerUnbound] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, Channel = [1] 'c'",
                            "[Disposing] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1",
                            "[Disposed] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1",
                            "[Trace:UnbindListener] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindNonLastBindingFromQueueAndNotRemoveIt()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelLoggerFactory( c => c.Id == 1 ? channelLogs.GetLogger() : null )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                clientLogs.GetAll()
                    .Skip( 3 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (UnbindListenerRequest, Length = 9)",
                            "[UnbindingListener] Client = [1] 'test', TraceId = 3, ChannelId = 1",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (UnbindListenerRequest, Length = 9)",
                            "[ListenerUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'c' (removed), Queue = [1] 'c'",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerUnboundResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerUnboundResponse, Length = 6)",
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerRequest, Length = 9)"
                    ] ),
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[ListenerUnbound] Channel = [1] 'c', TraceId = 1, Client = [1] 'test', Queue = [1] 'c'",
                            "[Disposing] Channel = [1] 'c', TraceId = 1",
                            "[Disposed] Channel = [1] 'c', TraceId = 1",
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, ClientTraceId = 3",
                            "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Channel = [1] 'c' (removed)",
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldUnbindLastClientFromChannelWithPublisherBindingAndNotRemoveIt()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var channelLogs = new ChannelEventLogger();
        var queueLogs = new QueueEventLogger();
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                clientLogs.GetAll()
                    .Skip( 3 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (UnbindListenerRequest, Length = 9)",
                            "[UnbindingListener] Client = [1] 'test', TraceId = 3, ChannelId = 1",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (UnbindListenerRequest, Length = 9)",
                            "[ListenerUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'c', Queue = [1] 'c' (removed)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerUnboundResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerUnboundResponse, Length = 6)",
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerRequest, Length = 9)"
                    ] ),
                channelLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 2, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[ListenerUnbound] Channel = [1] 'c', TraceId = 2, Client = [1] 'test', Queue = [1] 'c' (removed)",
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, ClientTraceId = 3",
                            "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, Channel = [1] 'c'",
                            "[Disposing] Client = [1] 'test', Queue = [1] 'c', TraceId = 1",
                            "[Disposed] Client = [1] 'test', Queue = [1] 'c', TraceId = 1",
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (end)"
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
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() ) );

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
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 8)",
                            """
                            [Error] Client = [1] 'test', TraceId = 2
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid UnbindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected header payload to be 4 but found 3.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 2",
                            "[Disposed] Client = [1] 'test', TraceId = 2",
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerRequest, Length = 8)"
                    ] ),
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[ListenerUnbound] Channel = [1] 'c', TraceId = 1, Client = [1] 'test', Queue = [1] 'c'",
                            "[Disposing] Channel = [1] 'c', TraceId = 1",
                            "[Disposed] Channel = [1] 'c', TraceId = 1",
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 1 (end)"
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
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
                c.SendUnbindListenerRequest( 0 );
            } );

        await endSource.Task;

        Assertion.All(
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
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerRequest, Length = 9)",
                            """
                            [Error] Client = [1] 'test', TraceId = 2
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid UnbindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel ID to be greater than 0 but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 2",
                            "[Disposed] Client = [1] 'test', TraceId = 2",
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerRequest, Length = 9)"
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (UnbindListenerRequest, Length = 9)",
                            "[UnbindingListener] Client = [1] 'test', TraceId = 1, ChannelId = 1",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelListenerBindingException: Client [1] 'test' could not be unbound as a listener from non-existing channel with ID 1.
                            """,
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (UnbindListenerFailureResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (UnbindListenerFailureResponse, Length = 6)",
                            "[Trace:UnbindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerRequest, Length = 9)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Unbind_ShouldBeRejected_WhenClientIsNotBoundAsListenerToChannel()
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
                    c => c.Id == 2
                        ? clientLogs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) )
                        : null ) );

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
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [2] 'test2', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [2] 'test2', TraceId = 1, Packet = (UnbindListenerRequest, Length = 9)",
                            "[UnbindingListener] Client = [2] 'test2', TraceId = 1, ChannelId = 1",
                            """
                            [Error] Client = [2] 'test2', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelListenerBindingException: Client [2] 'test2' could not be unbound as a listener from channel [1] 'c' because it is not bound as a listener to it.
                            """,
                            "[SendPacket:Sending] Client = [2] 'test2', TraceId = 1, Packet = (UnbindListenerFailureResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [2] 'test2', TraceId = 1, Packet = (UnbindListenerFailureResponse, Length = 6)",
                            "[Trace:UnbindListener] Client = [2] 'test2', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [2] 'test2'",
                        "[AwaitPacket] Client = [2] 'test2', Packet = (UnbindListenerRequest, Length = 9)"
                    ] ) )
            .Go();
    }
}
