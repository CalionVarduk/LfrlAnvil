using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Extensions;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerChannelListenerBindingTests : TestsBase, IClassFixture<SharedResourceFixture>
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", createChannelIfNotExists: true, minAckTimeout: Duration.FromMinutes( 10 ) );
            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var queue = remoteClient?.Queues.TryGetByName( "c" );
        var binding = channel?.Listeners.TryGetByClientId( 1 );

        Assertion.All(
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Server.TestRefEquals( server ),
                    c.Id.TestEquals( 1 ),
                    c.Name.TestEquals( "c" ),
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty(),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ) ) ),
                queue.TestNotNull( q => Assertion.All(
                    "queue",
                    q.Client.TestRefEquals( remoteClient ),
                    q.Id.TestEquals( 1 ),
                    q.Name.TestEquals( "c" ),
                    q.State.TestEquals( MessageBrokerQueueState.Running ),
                    q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                    q.Listeners.Count.TestEquals( 1 ),
                    q.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding?.QueueBindings.Primary ) ] ),
                    q.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding?.QueueBindings.Primary ),
                    q.Messages.Pending.Count.TestEquals( 0 ),
                    q.Messages.Pending.TryPeekAt( 0 ).TestNull(),
                    q.Messages.Retries.Count.TestEquals( 0 ),
                    q.Messages.Retries.TryGetNext().TestNull(),
                    q.Messages.Unacked.Count.TestEquals( 0 ),
                    q.Messages.Unacked.TryGetFirst().TestNull(),
                    q.Messages.Unacked.TryGetLast().TestNull(),
                    q.Messages.Unacked.TryGetByAckId( 0 ).TestNull(),
                    q.Messages.DeadLetter.Count.TestEquals( 0 ),
                    q.Messages.DeadLetter.TryPeekAt( 0 ).TestNull() ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty(),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ),
                    c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding ),
                    c.Queues.Count.TestEquals( 1 ),
                    c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ),
                    c.Queues.TryGetById( 1 ).TestRefEquals( queue ) ) ),
                binding.TestNotNull( s => Assertion.All(
                    "binding",
                    s.Channel.TestRefEquals( channel ),
                    s.Client.TestRefEquals( remoteClient ),
                    s.QueueBindings.Primary.Queue.TestRefEquals( queue ),
                    s.QueueBindings.Primary.PrefetchHint.TestEquals( 1 ),
                    s.QueueBindings.Primary.MaxRetries.TestEquals( 0 ),
                    s.QueueBindings.Primary.RetryDelay.TestEquals( Duration.Zero ),
                    s.QueueBindings.Primary.MaxRedeliveries.TestEquals( 0 ),
                    s.QueueBindings.Primary.MinAckTimeout.TestEquals( Duration.FromMinutes( 10 ) ),
                    s.QueueBindings.Primary.DeadLetterCapacityHint.TestEquals( 0 ),
                    s.QueueBindings.Primary.MinDeadLetterRetention.TestEquals( Duration.Zero ),
                    s.QueueBindings.Primary.FilterExpression.TestNull(),
                    s.IsEphemeral.TestTrue(),
                    s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    s.ToString().TestEquals( "[1] 'test' => [1] 'c' listener binding (using [1] 'c' queue) (Running)" ),
                    s.QueueBindings.Count.TestEquals( 1 ),
                    s.QueueBindings.GetAll().TestSequence( [ s.QueueBindings.Primary ] ),
                    s.QueueBindings.TryGetByQueueId( 1 ).TestRefEquals( s.QueueBindings.Primary ),
                    s.QueueBindings.Primary.Client.TestRefEquals( remoteClient ),
                    s.QueueBindings.Primary.Owner.TestRefEquals( s ),
                    s.QueueBindings.Primary.Queue.TestRefEquals( queue ),
                    s.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                    s.QueueBindings.Primary.ToString()
                        .TestEquals( "[1] 'test' => [1] 'c' queue listener binding (using [1] 'c' queue) (Primary:Running)" ) ) ),
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 41)",
                            "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = 600 second(s), DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 41)",
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 41)"
                    ] ),
                channelLogs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Channel = [1] 'c', TraceId = 0 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 0, Correlation = (Client = [1] 'test', TraceId = 1)",
                            "[Created] Channel = [1] 'c', TraceId = 0",
                            "[ListenerBound] Channel = [1] 'c', TraceId = 0, Client = [1] 'test', Queue = [1] 'c' (created)",
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
                            "[ListenerBound] Client = [1] 'test', Queue = [1] 'c', TraceId = 0, Channel = [1] 'c' (created)",
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) )
                .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() )
                .SetExpressionFactory(
                    new ParsedExpressionFactoryBuilder()
                        .AddGenericArithmeticOperators()
                        .AddGenericBitwiseOperators()
                        .AddGenericLogicalOperators()
                        .AddInt32TypeDefinition()
                        .Build() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
            c.SendBindListenerRequest(
                "c",
                createChannelIfNotExists: false,
                filterExpression: "c.Data.Length > 10i",
                prefetchHint: 10,
                maxRetries: 2,
                maxRedeliveries: 3,
                retryDelay: Duration.FromSeconds( 10 ),
                minAckTimeout: Duration.FromMinutes( 10 ),
                deadLetterCapacityHint: 100,
                minDeadLetterRetention: Duration.FromHours( 1 ) );

            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetByName( "c" );
        var queue = remoteClient?.Queues.TryGetByName( "c" );
        var binding = channel?.Listeners.TryGetByClientId( 1 );

        Assertion.All(
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Server.TestRefEquals( server ),
                    c.Id.TestEquals( 1 ),
                    c.Name.TestEquals( "c" ),
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ) ) ),
                queue.TestNotNull( q => Assertion.All(
                    "queue",
                    q.Client.TestRefEquals( remoteClient ),
                    q.Id.TestEquals( 1 ),
                    q.Name.TestEquals( "c" ),
                    q.State.TestEquals( MessageBrokerQueueState.Running ),
                    q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                    q.Listeners.Count.TestEquals( 1 ),
                    q.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding?.QueueBindings.Primary ) ] ),
                    q.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding?.QueueBindings.Primary ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ),
                    c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding ),
                    c.Queues.Count.TestEquals( 1 ),
                    c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue ) ] ),
                    c.Queues.TryGetById( 1 ).TestRefEquals( queue ) ) ),
                binding.TestNotNull( s => Assertion.All(
                    "binding",
                    s.Channel.TestRefEquals( channel ),
                    s.Client.TestRefEquals( remoteClient ),
                    s.QueueBindings.Primary.Queue.TestRefEquals( queue ),
                    s.QueueBindings.Primary.PrefetchHint.TestEquals( 10 ),
                    s.QueueBindings.Primary.MaxRetries.TestEquals( 2 ),
                    s.QueueBindings.Primary.RetryDelay.TestEquals( Duration.FromSeconds( 10 ) ),
                    s.QueueBindings.Primary.MaxRedeliveries.TestEquals( 3 ),
                    s.QueueBindings.Primary.MinAckTimeout.TestEquals( Duration.FromMinutes( 10 ) ),
                    s.QueueBindings.Primary.DeadLetterCapacityHint.TestEquals( 100 ),
                    s.QueueBindings.Primary.MinDeadLetterRetention.TestEquals( Duration.FromHours( 1 ) ),
                    s.QueueBindings.Primary.FilterExpression.TestEquals( "c.Data.Length > 10i" ),
                    s.IsEphemeral.TestTrue(),
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 60)",
                            """
                            [BindingListener] Client = [1] 'test', TraceId = 2, ChannelName = 'c', PrefetchHint = 10, MaxRetries = 2, RetryDelay = 10 second(s), MaxRedeliveries = 3, MinAckTimeout = 600 second(s), DeadLetter = (CapacityHint = 100, MinRetention = 3600 second(s)), IsEphemeral = True, CreateChannelIfNotExists = False, FilterExpression:
                            c.Data.Length > 10i
                            """,
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 60)",
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 60)"
                    ] ),
                channelLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Channel = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[ListenerBound] Channel = [1] 'c', TraceId = 1, Client = [1] 'test', Queue = [1] 'c' (created)",
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
                            "[ListenerBound] Client = [1] 'test', Queue = [1] 'c', TraceId = 0, Channel = [1] 'c'",
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", true, minAckTimeout: Duration.FromMinutes( 10 ) );
            c.ReadListenerBoundResponse();
        } );

        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "d", true, queueName: "c", minAckTimeout: Duration.FromMinutes( 10 ) );
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
                channel1.TestNotNull( c => Assertion.All(
                    "channel1",
                    c.Server.TestRefEquals( server ),
                    c.Id.TestEquals( 1 ),
                    c.Name.TestEquals( "c" ),
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.ToString().TestEquals( "[1] 'c' channel (Running)" ),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ),
                    c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding1 ) ) ),
                channel2.TestNotNull( c => Assertion.All(
                    "channel2",
                    c.Server.TestRefEquals( server ),
                    c.Id.TestEquals( 2 ),
                    c.Name.TestEquals( "d" ),
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.ToString().TestEquals( "[2] 'd' channel (Running)" ),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding2 ) ] ),
                    c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding2 ) ) ),
                queue.TestNotNull( q => Assertion.All(
                    "queue",
                    q.Client.TestRefEquals( remoteClient ),
                    q.Id.TestEquals( 1 ),
                    q.Name.TestEquals( "c" ),
                    q.State.TestEquals( MessageBrokerQueueState.Running ),
                    q.ToString().TestEquals( "[1] 'c' queue (Running)" ),
                    q.Listeners.Count.TestEquals( 2 ),
                    q.Listeners.GetAll().TestSetEqual( [ binding1?.QueueBindings.Primary, binding2?.QueueBindings.Primary ] ),
                    q.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding1?.QueueBindings.Primary ),
                    q.Listeners.TryGetByChannelId( 2 ).TestRefEquals( binding2?.QueueBindings.Primary ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 2 ),
                    c.Listeners.GetAll().TestSetEqual( [ binding1, binding2 ] ),
                    c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding1 ),
                    c.Listeners.TryGetByChannelId( 2 ).TestRefEquals( binding2 ) ) ),
                binding1.TestNotNull( s => Assertion.All(
                    "binding1",
                    s.Channel.TestRefEquals( channel1 ),
                    s.Client.TestRefEquals( remoteClient ),
                    s.QueueBindings.Primary.Queue.TestRefEquals( queue ),
                    s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    s.ToString().TestEquals( "[1] 'test' => [1] 'c' listener binding (using [1] 'c' queue) (Running)" ) ) ),
                binding2.TestNotNull( s => Assertion.All(
                    "binding2",
                    s.Channel.TestRefEquals( channel2 ),
                    s.Client.TestRefEquals( remoteClient ),
                    s.QueueBindings.Primary.Queue.TestRefEquals( queue ),
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 42)",
                            "[BindingListener] Client = [1] 'test', TraceId = 2, ChannelName = 'd', QueueName = 'c', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = 600 second(s), DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 42)",
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 41)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 42)"
                    ] ),
                channelLogs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Channel = [2] 'd', TraceId = 0 (start)",
                            "[ClientTrace] Channel = [2] 'd', TraceId = 0, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[Created] Channel = [2] 'd', TraceId = 0",
                            "[ListenerBound] Channel = [2] 'd', TraceId = 0, Client = [1] 'test', Queue = [1] 'c'",
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
                            "[ListenerBound] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, Channel = [2] 'd' (created)",
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
        await client.GetTask( c => c.SendBindListenerRequest(
            "c",
            createChannelIfNotExists: true,
            minAckTimeout: Duration.FromMinutes( 10 ) ) );

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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 41)",
                            "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = 600 second(s), DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = True",
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
                            (e, _) => e.TestEquals( "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False" ),
                            (e, _) => e.TestEquals( "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False" ),
                            (e, _) => e.TestEquals( "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (end)" )
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 41)"
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
        await client.GetTask( c => c.SendBindListenerRequest(
            "c",
            createChannelIfNotExists: true,
            minAckTimeout: Duration.FromMinutes( 10 ) ) );

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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 41)",
                            "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = 600 second(s), DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = True",
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
                            (e, _) => e.TestEquals( "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False" ),
                            (e, _) => e.TestEquals( "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False" ),
                            (e, _) => e.TestEquals( "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (end)" )
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 41)"
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindListenerRequest( "c", createChannelIfNotExists: true, payload: 34 ) );
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 39)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected header payload to be at least 35 but found 34.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 39)"
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 43)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 2 error(s):
                            1. Expected binary channel name length to be in [0, 3] range but found 4.
                            2. Expected binary queue name length to be in [0, -1] range but found 0.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 43)"
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 40)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel name length to be in [1, 512] range but found 0.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 40)"
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 553)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel name length to be in [1, 512] range but found 513.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 553)"
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 554)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. Encountered 1 error(s):
                            1. Expected queue name length to be in [1, 512] range but found 513.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 554)"
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData(
        0,
        -1,
        -1,
        -1,
        -1,
        -1,
        -1,
        """
        Encountered 6 error(s):
        1. Expected prefetch hint to be greater than 0 but found 0.
        2. Expected max retries to not be negative but found -1.
        3. Expected retry delay to not be negative but found -0.001 second(s).
        4. Expected max redeliveries to not be negative but found -1.
        5. Expected min ACK timeout to not be negative but found -0.001 second(s).
        6. Expected dead letter capacity hint to not be negative but found -1.
        """ )]
    [InlineData(
        1,
        0,
        2,
        1,
        0,
        0,
        5,
        """
        Encountered 3 error(s):
        1. Expected disabled retry delay to be equal to 0 but found 0.002 second(s).
        2. Expected enabled min ACK timeout to be greater than 0 but found 0 second(s).
        3. Expected disabled min dead letter retention to be equal to 0 but found 0.005 second(s).
        """ )]
    [InlineData(
        1,
        0,
        0,
        0,
        0,
        1,
        0,
        """
        Encountered 2 error(s):
        1. Expected enabled min ACK timeout to be greater than 0 but found 0 second(s).
        2. Expected enabled min dead letter retention to be greater than 0 but found 0 second(s).
        """ )]
    public async Task Creation_ShouldDisposeClient_WhenClientSendsInvalidValues(
        short prefetchHint,
        int maxRetries,
        int retryDelayMs,
        int maxRedeliveries,
        int minAckTimeoutMs,
        int deadLetterCapacity,
        long deadLetterRetentionMs,
        string expectedError)
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendBindListenerRequest(
            "c",
            createChannelIfNotExists: true,
            prefetchHint: prefetchHint,
            maxRetries: maxRetries,
            retryDelay: Duration.FromMilliseconds( retryDelayMs ),
            maxRedeliveries: maxRedeliveries,
            minAckTimeout: Duration.FromMilliseconds( minAckTimeoutMs ),
            deadLetterCapacityHint: deadLetterCapacity,
            minDeadLetterRetention: Duration.FromMilliseconds( deadLetterRetentionMs ) ) );

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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 41)",
                            $"""
                             [Error] Client = [1] 'test', TraceId = 1
                             LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid BindListenerRequest from client [1] 'test'. {expectedError}
                             """,
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 41)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindListenerRequest_ShouldBeRejected_WhenClientIsAlreadyBoundAsListenerToChannel()
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        },
                        error: e => exception = e.Exception ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", createChannelIfNotExists: true, minAckTimeout: Duration.FromMinutes( 10 ) );
            c.ReadListenerBoundResponse();
            c.SendBindListenerRequest( "c", createChannelIfNotExists: false, minAckTimeout: Duration.FromMinutes( 10 ) );
            c.ReadBindListenerFailureResponse();
        } );

        await endSource.Task;

        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Listeners.TryGetByClientId( 1 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelListenerBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient ),
                        exc.Listener.TestRefEquals( binding ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding ) ] ) ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 1 ),
                channel.TestNotNull( c => Assertion.All(
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 41)",
                            "[BindingListener] Client = [1] 'test', TraceId = 2, ChannelName = 'c', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = 600 second(s), DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = False",
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 41)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 41)"
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", createChannelIfNotExists: false, minAckTimeout: Duration.FromMinutes( 10 ) );
            c.ReadBindListenerFailureResponse();
        } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelListenerBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient ),
                        exc.Listener.TestNull() ) ),
                remoteClient.TestNotNull( c => Assertion.All(
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 41)",
                            "[BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = 600 second(s), DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = False",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelListenerBindingException: Client [1] 'test' could not be bound as a listener to a non-existing channel 'c'.
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 41)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindListenerRequest_ShouldBeRejected_WhenFilterExpressionsAreNotEnabledAndRequestContainsOne()
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        },
                        error: e => exception = e.Exception ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", false, filterExpression: "expression" );
            c.ReadBindListenerFailureResponse();
        } );

        await endSource.Task;

        Assertion.All(
                exception.TestType().Exact<MessageBrokerRemoteClientException>( exc => exc.Client.TestRefEquals( remoteClient ) ),
                remoteClient.TestNotNull( c => Assertion.All(
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 51)",
                            """
                            [BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = False, FilterExpression:
                            expression
                            """,
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Filter expressions are not enabled but found:
                            expression
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 51)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindListenerRequest_ShouldBeRejected_WhenFilterExpressionCouldNotBeParsed()
    {
        var exception1 = Atomic.Create( ( Exception? )null );
        var exception2 = Atomic.Create( ( Exception? )null );
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        },
                        error: e =>
                        {
                            var target = exception1.Value is null ? exception1 : exception2;
                            target.Value = e.Exception;
                        } ) ) )
                .SetExpressionFactory(
                    new ParsedExpressionFactoryBuilder()
                        .AddGenericArithmeticOperators()
                        .AddGenericBitwiseOperators()
                        .AddGenericLogicalOperators()
                        .Build() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", false, filterExpression: "expression" );
            c.ReadBindListenerFailureResponse();
        } );

        await endSource.Task;

        Assertion.All(
                exception1.Value.TestType().Exact<ParsedExpressionCreationException>(),
                exception2.Value.TestType().Exact<MessageBrokerRemoteClientException>( exc => exc.Client.TestRefEquals( remoteClient ) ),
                remoteClient.TestNotNull( c => Assertion.All(
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
                            (e, _) => e.TestEquals( "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (start)" ),
                            (e, _) => e.TestEquals(
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 51)" ),
                            (e, _) => e.TestEquals(
                                """
                                [BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = False, FilterExpression:
                                expression
                                """ ),
                            (e, _) => e.TestStartsWith(
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.Computable.Expressions.Exceptions.ParsedExpressionCreationException: Failed to create an expression:
                                expression

                                Encountered 1 error(s):
                                1. OutputTypeConverterHasThrownException, construct of type LfrlAnvil.Computable.Expressions.Constructs.ParsedExpressionTypeConverter, an exception has been thrown:
                                System.InvalidOperationException
                                """ ),
                            (e, _) => e.TestEquals(
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Failed to parse the following filter expression:
                                expression
                                """ ),
                            (e, _) => e.TestEquals(
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindListenerFailureResponse, Length = 6)" ),
                            (e, _) => e.TestEquals(
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindListenerFailureResponse, Length = 6)" ),
                            (e, _) => e.TestEquals( "[Trace:BindListener] Client = [1] 'test', TraceId = 1 (end)" )
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 51)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task BindListenerRequest_ShouldBeRejected_WhenFilterExpressionContainsTooManyArguments()
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        },
                        error: e => exception = e.Exception ) ) )
                .SetExpressionFactory(
                    new ParsedExpressionFactoryBuilder()
                        .AddGenericArithmeticOperators()
                        .AddGenericBitwiseOperators()
                        .AddGenericLogicalOperators()
                        .AddInt32TypeDefinition()
                        .Build() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", false, filterExpression: "a.Data.Length + b.Data.Length < 10i" );
            c.ReadBindListenerFailureResponse();
        } );

        await endSource.Task;

        Assertion.All(
                exception.TestType().Exact<MessageBrokerRemoteClientException>( exc => exc.Client.TestRefEquals( remoteClient ) ),
                remoteClient.TestNotNull( c => Assertion.All(
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindListenerRequest, Length = 76)",
                            """
                            [BindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'c', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = False, FilterExpression:
                            a.Data.Length + b.Data.Length < 10i
                            """,
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Expected at most one filter expression context argument but found 2 ('a', 'b') in the following expression:
                            a.Data.Length + b.Data.Length < 10i
                            """,
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Failed to parse the following filter expression:
                            a.Data.Length + b.Data.Length < 10i
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
                        "[AwaitPacket] Client = [1] 'test', Packet = (BindListenerRequest, Length = 76)"
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
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
            c.ReadListenerBoundResponse();
        } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Listeners.TryGetByClientId( 1 );
        var queue = binding?.QueueBindings.Primary.Queue;
        await server.DisposeAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Listeners.GetAll().TestEmpty(),
                    c.Queues.Count.TestEquals( 0 ),
                    c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Listeners.GetAll().TestEmpty() ) ),
                queue.TestNotNull( q => Assertion.All(
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
                            "[Trace:Deactivate] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[Deactivating] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                            "[Trace:Deactivate] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (end)"
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
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
            c.SendBindListenerRequest( "c", createChannelIfNotExists: false );
            c.ReadListenerBoundResponse();
        } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var listenerBinding = channel?.Listeners.TryGetByClientId( 1 );
        var queue = listenerBinding?.QueueBindings.Primary.Queue;
        var publisherBinding = channel?.Publishers.TryGetByClientId( 1 );
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty(),
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Listeners.GetAll().TestEmpty(),
                    c.Queues.Count.TestEquals( 0 ),
                    c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty(),
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Listeners.GetAll().TestEmpty() ) ),
                queue.TestNotNull( q => Assertion.All(
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
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 2 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 2, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[ListenerUnbound] Channel = [1] 'c', TraceId = 2, Client = [1] 'test', Queue = [1] 'c'",
                            "[Trace:UnbindListener] Channel = [1] 'c', TraceId = 2 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 3 (start)",
                            "[ClientTrace] Channel = [1] 'c', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 3)",
                            "[PublisherUnbound] Channel = [1] 'c', TraceId = 3, Client = [1] 'test', Stream = [1] 'c'",
                            "[Disposing] Channel = [1] 'c', TraceId = 3",
                            "[Disposed] Channel = [1] 'c', TraceId = 3",
                            "[Trace:UnbindPublisher] Channel = [1] 'c', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Deactivate] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, ClientTraceId = 3",
                            "[Deactivating] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                            "[Trace:Deactivate] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .Skip( 3 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 3 (start)",
                            "[Deactivating] Client = [1] 'test', TraceId = 3, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 3, IsAlive = False",
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 3 (end)"
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
        await client1.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", createChannelIfNotExists: false );
            c.ReadListenerBoundResponse();
        } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var publisherBinding = channel?.Publishers.TryGetByClientId( 1 );
        var listenerBinding = channel?.Listeners.TryGetByClientId( 2 );
        var queue = listenerBinding?.QueueBindings.Primary.Queue;
        if ( remoteClient2 is not null )
            await remoteClient2.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 1 ),
                remoteClient1.TestNotNull( c => Assertion.All(
                    "client1",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( publisherBinding ) ] ) ) ),
                remoteClient2.TestNotNull( c => Assertion.All(
                    "client2",
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Listeners.GetAll().TestEmpty(),
                    c.Queues.Count.TestEquals( 0 ),
                    c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( publisherBinding ) ] ),
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Listeners.GetAll().TestEmpty() ) ),
                queue.TestNotNull( q => Assertion.All(
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
                            "[Trace:Deactivate] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[Deactivating] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                            "[Trace:Deactivate] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Deactivate] Client = [2] 'test2', TraceId = 2 (start)",
                            "[Deactivating] Client = [2] 'test2', TraceId = 2, IsAlive = False",
                            "[Deactivated] Client = [2] 'test2', TraceId = 2, IsAlive = False",
                            "[Trace:Deactivate] Client = [2] 'test2', TraceId = 2 (end)"
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
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
            c.ReadListenerBoundResponse();
        } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Listeners.TryGetByClientId( 1 );
        var queue = binding?.QueueBindings.Primary.Queue;
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 0 ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty(),
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Listeners.GetAll().TestEmpty(),
                    c.Queues.Count.TestEquals( 0 ),
                    c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Publishers.GetAll().TestEmpty(),
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Listeners.GetAll().TestEmpty() ) ),
                queue.TestNotNull( q => Assertion.All(
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
                            "[Trace:Deactivate] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[Deactivating] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                            "[Trace:Deactivate] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (end)"
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
        await client1.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
            c.ReadListenerBoundResponse();
        } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask( c =>
        {
            c.SendBindListenerRequest( "c", createChannelIfNotExists: false );
            c.ReadListenerBoundResponse();
        } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var binding1 = channel?.Listeners.TryGetByClientId( 1 );
        var binding2 = channel?.Listeners.TryGetByClientId( 2 );
        var queue1 = binding1?.QueueBindings.Primary.Queue;
        var queue2 = binding2?.QueueBindings.Primary.Queue;
        if ( remoteClient2 is not null )
            await remoteClient2.DisconnectAsync();

        Assertion.All(
                server.Channels.Count.TestEquals( 1 ),
                remoteClient1.TestNotNull( c => Assertion.All(
                    "client1",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ),
                    c.Queues.Count.TestEquals( 1 ),
                    c.Queues.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( queue1 ) ] ) ) ),
                remoteClient2.TestNotNull( c => Assertion.All(
                    "client2",
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Listeners.GetAll().TestEmpty(),
                    c.Queues.Count.TestEquals( 0 ),
                    c.Queues.GetAll().TestEmpty() ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ),
                    c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                    c.Listeners.TryGetByClientId( 2 ).TestNull() ) ),
                queue1.TestNotNull( q => Assertion.All(
                    "queue1",
                    q.State.TestEquals( MessageBrokerQueueState.Running ),
                    q.Listeners.Count.TestEquals( 1 ),
                    q.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1?.QueueBindings.Primary ) ] ) ) ),
                queue2.TestNotNull( q => Assertion.All(
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
                            "[Trace:Deactivate] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, ClientTraceId = 2",
                            "[Deactivating] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                            "[Trace:Deactivate] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Deactivate] Client = [2] 'test2', TraceId = 2 (start)",
                            "[Deactivating] Client = [2] 'test2', TraceId = 2, IsAlive = False",
                            "[Deactivated] Client = [2] 'test2', TraceId = 2, IsAlive = False",
                            "[Trace:Deactivate] Client = [2] 'test2', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveNonEphemeralListener()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteListenerMetadata(
            clientId: 1,
            channelId: 1,
            queueId: 1,
            prefetchHint: 3,
            maxRetries: 1,
            retryDelay: Duration.FromHours( 1 ),
            maxRedeliveries: 2,
            minAckTimeout: Duration.FromHours( 2 ),
            deadLetterCapacityHint: 4,
            minDeadLetterRetention: Duration.FromHours( 3 ) );

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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "foo", true, maxRedeliveries: 20, minAckTimeout: Duration.FromHours( 5 ), isEphemeral: false );
            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 1 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    p.IsEphemeral.TestFalse(),
                    p.QueueBindings.Primary.PrefetchHint.TestEquals( 1 ),
                    p.QueueBindings.Primary.MaxRetries.TestEquals( 0 ),
                    p.QueueBindings.Primary.RetryDelay.TestEquals( Duration.Zero ),
                    p.QueueBindings.Primary.MaxRedeliveries.TestEquals( 20 ),
                    p.QueueBindings.Primary.MinAckTimeout.TestEquals( Duration.FromHours( 5 ) ),
                    p.QueueBindings.Primary.DeadLetterCapacityHint.TestEquals( 0 ),
                    p.QueueBindings.Primary.MinDeadLetterRetention.TestEquals( Duration.Zero ) ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 43)",
                            "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 20, MinAckTimeout = 18000 second(s), DeadLetter = <disabled>, IsEphemeral = False, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 43)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveNonEphemeralListener_AsEphemeral()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1 );

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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "foo", true, isEphemeral: true );
            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 1 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    p.IsEphemeral.TestTrue() ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 43)",
                            "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 43)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveEphemeralListener()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1 );

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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: true );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest( "foo", true, isEphemeral: false );
            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 1 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    p.IsEphemeral.TestTrue() ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 43)",
                            "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 43)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveListener_ToDifferentNewQueue()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages(
            streamId: 1,
            messages:
            [
                StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ),
                StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] ),
                StorageScope.PrepareStreamMessage( id: 2, storeKey: 2, senderId: 1, channelId: 1, data: [ 4, 5, 6 ] ),
                StorageScope.PrepareStreamMessage( id: 3, storeKey: 3, senderId: 1, channelId: 1, data: [ 7, 8, 9, 10 ] ),
                StorageScope.PrepareStreamMessage( id: 4, storeKey: 4, senderId: 1, channelId: 1, data: [ 11, 12, 13, 14, 15 ] ),
                StorageScope.PrepareStreamMessage( id: 5, storeKey: 5, senderId: 1, channelId: 1, data: [ 16, 17, 18, 19, 20, 21 ] )
            ] );

        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteListenerMetadata(
            clientId: 1,
            channelId: 1,
            queueId: 1,
            maxRedeliveries: 5,
            minAckTimeout: Duration.FromMinutes( 1 ) );

        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 1,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 4 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 5 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 2,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 1 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 2, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 3, queueName: "b" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 3,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 2 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 3 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 3, messages: [ ] );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var queueLogs = new[] { new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger() };

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetQueueLoggerFactory( q => queueLogs[q.Id - 1].GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest(
                "foo",
                true,
                queueName: "bar",
                isEphemeral: false,
                maxRedeliveries: 5,
                minAckTimeout: Duration.FromMinutes( 1 ) );

            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Queues.Count.TestEquals( 4 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    p.IsEphemeral.TestFalse(),
                    p.QueueBindings.Count.TestEquals( 4 ),
                    p.QueueBindings.Primary.Queue.Id.TestEquals( 4 ),
                    p.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                    p.QueueBindings.Primary.IsPrimary.TestTrue(),
                    p.QueueBindings.Primary.Queue.Listeners.Count.TestEquals( 1 ),
                    p.QueueBindings.TryGetByQueueId( 1 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding1",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ),
                    p.QueueBindings.TryGetByQueueId( 2 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding2",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ),
                    p.QueueBindings.TryGetByQueueId( 3 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding3",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ) ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                clientLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 46)",
                            "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', QueueName = 'bar', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 5, MinAckTimeout = 60 second(s), DeadLetter = <disabled>, IsEphemeral = False, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 46)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [4] 'bar' (created) (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[0]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[1]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs[2]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs[3]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [4] 'bar', TraceId = 0 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [4] 'bar', TraceId = 0, ClientTraceId = 3",
                            "[Created] Client = [1] 'test', Queue = [4] 'bar', TraceId = 0",
                            "[ListenerBound] Client = [1] 'test', Queue = [4] 'bar', TraceId = 0, Channel = [1] 'foo'",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [4] 'bar', TraceId = 0 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveListener_ToDifferentNewQueue_WithDisposedOldPrimaryBinding()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages(
            streamId: 1,
            messages:
            [
                StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ),
                StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] ),
                StorageScope.PrepareStreamMessage( id: 2, storeKey: 2, senderId: 1, channelId: 1, data: [ 4, 5, 6 ] ),
                StorageScope.PrepareStreamMessage( id: 3, storeKey: 3, senderId: 1, channelId: 1, data: [ 7, 8, 9, 10 ] )
            ] );

        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteListenerMetadata(
            clientId: 1,
            channelId: 1,
            queueId: 1,
            maxRedeliveries: 5,
            minAckTimeout: Duration.FromMinutes( 1 ) );

        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 2,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 1 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 2, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 3, queueName: "b" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 3,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 2 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 3 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 3, messages: [ ] );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var queueLogs = new[] { new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger() };

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetQueueLoggerFactory( q => queueLogs[q.Id - 1].GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest(
                "foo",
                true,
                queueName: "bar",
                isEphemeral: false,
                maxRedeliveries: 5,
                minAckTimeout: Duration.FromMinutes( 1 ) );

            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Queues.Count.TestEquals( 3 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    p.IsEphemeral.TestFalse(),
                    p.QueueBindings.Count.TestEquals( 3 ),
                    p.QueueBindings.Primary.Queue.Id.TestEquals( 4 ),
                    p.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                    p.QueueBindings.Primary.IsPrimary.TestTrue(),
                    p.QueueBindings.Primary.Queue.Listeners.Count.TestEquals( 1 ),
                    p.QueueBindings.TryGetByQueueId( 1 ).TestNull(),
                    p.QueueBindings.TryGetByQueueId( 2 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding2",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ),
                    p.QueueBindings.TryGetByQueueId( 3 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding3",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ) ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                clientLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 46)",
                            "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', QueueName = 'bar', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 5, MinAckTimeout = 60 second(s), DeadLetter = <disabled>, IsEphemeral = False, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 46)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [4] 'bar' (created) (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[0]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 3",
                            "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo'",
                            "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[1]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs[2]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs[3]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [4] 'bar', TraceId = 0 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [4] 'bar', TraceId = 0, ClientTraceId = 3",
                            "[Created] Client = [1] 'test', Queue = [4] 'bar', TraceId = 0",
                            "[ListenerBound] Client = [1] 'test', Queue = [4] 'bar', TraceId = 0, Channel = [1] 'foo'",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [4] 'bar', TraceId = 0 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveListener_ToDifferentExistingQueue()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteChannelMetadata( channelId: 2, channelName: "bar" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages(
            streamId: 1,
            messages:
            [
                StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ),
                StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] ),
                StorageScope.PrepareStreamMessage( id: 2, storeKey: 2, senderId: 1, channelId: 1, data: [ 4, 5, 6 ] ),
                StorageScope.PrepareStreamMessage( id: 3, storeKey: 3, senderId: 1, channelId: 1, data: [ 7, 8, 9, 10 ] ),
                StorageScope.PrepareStreamMessage( id: 4, storeKey: 4, senderId: 1, channelId: 1, data: [ 11, 12, 13, 14, 15 ] ),
                StorageScope.PrepareStreamMessage( id: 5, storeKey: 5, senderId: 1, channelId: 1, data: [ 16, 17, 18, 19, 20, 21 ] )
            ] );

        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteListenerMetadata( clientId: 1, channelId: 2, queueId: 4 );
        storage.WriteListenerMetadata(
            clientId: 1,
            channelId: 1,
            queueId: 1,
            maxRedeliveries: 5,
            minAckTimeout: Duration.FromMinutes( 1 ) );

        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 1,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 4 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 5 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 2,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 1 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 2, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 3, queueName: "b" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 3,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 2 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 3 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 3, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 4, queueName: "bar" );
        storage.WriteQueuePendingMessages( clientId: 1, queueId: 4, messages: [ ] );
        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 4, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 4, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 4, messages: [ ] );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var queueLogs = new[] { new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger() };

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetQueueLoggerFactory( q => queueLogs[q.Id - 1].GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest(
                "foo",
                true,
                queueName: "bar",
                isEphemeral: false,
                maxRedeliveries: 5,
                minAckTimeout: Duration.FromMinutes( 1 ) );

            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 2 ),
                    c.Queues.Count.TestEquals( 4 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    p.IsEphemeral.TestFalse(),
                    p.QueueBindings.Count.TestEquals( 4 ),
                    p.QueueBindings.Primary.Queue.Id.TestEquals( 4 ),
                    p.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                    p.QueueBindings.Primary.IsPrimary.TestTrue(),
                    p.QueueBindings.Primary.Queue.Listeners.Count.TestEquals( 2 ),
                    p.QueueBindings.TryGetByQueueId( 1 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding1",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ),
                    p.QueueBindings.TryGetByQueueId( 2 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding2",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ),
                    p.QueueBindings.TryGetByQueueId( 3 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding3",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ) ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                clientLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 46)",
                            "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', QueueName = 'bar', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 5, MinAckTimeout = 60 second(s), DeadLetter = <disabled>, IsEphemeral = False, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 46)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [4] 'bar' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[0]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[1]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs[2]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs[3]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [4] 'bar', TraceId = 3 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [4] 'bar', TraceId = 3, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [4] 'bar', TraceId = 3, Channel = [1] 'foo'",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [4] 'bar', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveListener_ToDifferentExistingQueue_WithDisposedOldPrimaryBinding()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteChannelMetadata( channelId: 2, channelName: "bar" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages(
            streamId: 1,
            messages:
            [
                StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ),
                StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] ),
                StorageScope.PrepareStreamMessage( id: 2, storeKey: 2, senderId: 1, channelId: 1, data: [ 4, 5, 6 ] ),
                StorageScope.PrepareStreamMessage( id: 3, storeKey: 3, senderId: 1, channelId: 1, data: [ 7, 8, 9, 10 ] )
            ] );

        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteListenerMetadata( clientId: 1, channelId: 2, queueId: 4 );
        storage.WriteListenerMetadata(
            clientId: 1,
            channelId: 1,
            queueId: 1,
            maxRedeliveries: 5,
            minAckTimeout: Duration.FromMinutes( 1 ) );

        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 2,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 1 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 2, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 3, queueName: "b" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 3,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 2 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 3 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 3, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 4, queueName: "bar" );
        storage.WriteQueuePendingMessages( clientId: 1, queueId: 4, messages: [ ] );
        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 4, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 4, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 4, messages: [ ] );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var queueLogs = new[] { new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger() };

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetQueueLoggerFactory( q => queueLogs[q.Id - 1].GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest(
                "foo",
                true,
                queueName: "bar",
                isEphemeral: false,
                maxRedeliveries: 5,
                minAckTimeout: Duration.FromMinutes( 1 ) );

            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 2 ),
                    c.Queues.Count.TestEquals( 3 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    p.IsEphemeral.TestFalse(),
                    p.QueueBindings.Count.TestEquals( 3 ),
                    p.QueueBindings.Primary.Queue.Id.TestEquals( 4 ),
                    p.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                    p.QueueBindings.Primary.IsPrimary.TestTrue(),
                    p.QueueBindings.Primary.Queue.Listeners.Count.TestEquals( 2 ),
                    p.QueueBindings.TryGetByQueueId( 1 ).TestNull(),
                    p.QueueBindings.TryGetByQueueId( 2 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding2",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ),
                    p.QueueBindings.TryGetByQueueId( 3 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding3",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ) ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                clientLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 46)",
                            "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', QueueName = 'bar', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 5, MinAckTimeout = 60 second(s), DeadLetter = <disabled>, IsEphemeral = False, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 46)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [4] 'bar' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[0]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 3",
                            "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo'",
                            "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[1]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs[2]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs[3]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [4] 'bar', TraceId = 3 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [4] 'bar', TraceId = 3, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [4] 'bar', TraceId = 3, Channel = [1] 'foo'",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [4] 'bar', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveListener_ToSecondaryQueue()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages(
            streamId: 1,
            messages:
            [
                StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ),
                StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] ),
                StorageScope.PrepareStreamMessage( id: 2, storeKey: 2, senderId: 1, channelId: 1, data: [ 4, 5, 6 ] ),
                StorageScope.PrepareStreamMessage( id: 3, storeKey: 3, senderId: 1, channelId: 1, data: [ 7, 8, 9, 10 ] ),
                StorageScope.PrepareStreamMessage( id: 4, storeKey: 4, senderId: 1, channelId: 1, data: [ 11, 12, 13, 14, 15 ] )
            ] );

        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteListenerMetadata(
            clientId: 1,
            channelId: 1,
            queueId: 1,
            maxRedeliveries: 5,
            minAckTimeout: Duration.FromMinutes( 1 ) );

        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 1,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 1 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 2,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 2 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 3 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 2, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 3, queueName: "b" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 3,
            messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 4 ) ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 3, messages: [ ] );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var queueLogs = new[] { new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger() };

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetQueueLoggerFactory( q => queueLogs[q.Id - 1].GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest(
                "foo",
                true,
                queueName: "b",
                isEphemeral: false,
                maxRedeliveries: 5,
                minAckTimeout: Duration.FromMinutes( 1 ) );

            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Queues.Count.TestEquals( 3 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    p.IsEphemeral.TestFalse(),
                    p.QueueBindings.Count.TestEquals( 3 ),
                    p.QueueBindings.Primary.Queue.Id.TestEquals( 3 ),
                    p.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                    p.QueueBindings.Primary.IsPrimary.TestTrue(),
                    p.QueueBindings.Primary.Queue.Listeners.Count.TestEquals( 1 ),
                    p.QueueBindings.TryGetByQueueId( 1 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding1",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ),
                    p.QueueBindings.TryGetByQueueId( 2 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding2",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ) ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                clientLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 44)",
                            "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', QueueName = 'b', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 5, MinAckTimeout = 60 second(s), DeadLetter = <disabled>, IsEphemeral = False, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 44)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [3] 'b' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[0]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[1]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs[2]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, Channel = [1] 'foo'",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldReactivateInactiveListener_ToSecondaryQueue_WithDisposedOldPrimaryBinding()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages(
            streamId: 1,
            messages:
            [
                StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ),
                StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] ),
                StorageScope.PrepareStreamMessage( id: 2, storeKey: 2, senderId: 1, channelId: 1, data: [ 4, 5, 6 ] )
            ] );

        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteListenerMetadata(
            clientId: 1,
            channelId: 1,
            queueId: 1,
            maxRedeliveries: 5,
            minAckTimeout: Duration.FromMinutes( 1 ) );

        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 2,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 1 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 2, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 3, queueName: "b" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 3,
            messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 2 ) ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 3, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 3, messages: [ ] );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var queueLogs = new[] { new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger() };

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetQueueLoggerFactory( q => queueLogs[q.Id - 1].GetLogger() )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindListenerRequest(
                "foo",
                true,
                queueName: "b",
                isEphemeral: false,
                maxRedeliveries: 5,
                minAckTimeout: Duration.FromMinutes( 1 ) );

            c.ReadListenerBoundResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Queues.Count.TestEquals( 2 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    p.IsEphemeral.TestFalse(),
                    p.QueueBindings.Count.TestEquals( 2 ),
                    p.QueueBindings.Primary.Queue.Id.TestEquals( 3 ),
                    p.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                    p.QueueBindings.Primary.IsPrimary.TestTrue(),
                    p.QueueBindings.Primary.Queue.Listeners.Count.TestEquals( 1 ),
                    p.QueueBindings.TryGetByQueueId( 1 ).TestNull(),
                    p.QueueBindings.TryGetByQueueId( 2 )
                        .TestNotNull( b => Assertion.All(
                            "queueBinding2",
                            b.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                            b.IsPrimary.TestFalse(),
                            b.Queue.Listeners.Count.TestEquals( 1 ) ) ) ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                clientLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 44)",
                            "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', QueueName = 'b', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 5, MinAckTimeout = 60 second(s), DeadLetter = <disabled>, IsEphemeral = False, CreateChannelIfNotExists = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 44)",
                            "[ListenerBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [3] 'b' (reactivated)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (ListenerBoundResponse, Length = 14)",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[0]
                    .GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 3",
                            "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo'",
                            "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                            "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                        ] )
                    ] ),
                queueLogs[1]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, Channel = [1] 'foo' (reactivated)",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (end)"
                        ] )
                    ] ),
                queueLogs[2]
                    .GetAll()
                    .Where( t => t.Logs.Any( e => e.StartsWith( "[Trace:BindListener]" ) ) )
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (start)",
                            "[ClientTrace] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, ClientTraceId = 3",
                            "[ListenerBound] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, Channel = [1] 'foo'",
                            "[Trace:BindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Rebind_ShouldFailAndDeactivateClient_WhenNewQueueLoggerFactoryThrows()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages(
            streamId: 1,
            messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ) ] );

        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteListenerMetadata(
            clientId: 1,
            channelId: 1,
            queueId: 1,
            maxRedeliveries: 5,
            minAckTimeout: Duration.FromMinutes( 1 ) );

        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 2,
            messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ) ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 2, messages: [ ] );

        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetQueueLoggerFactory( q => q.Id > 2 ? throw new Exception( "foo" ) : null )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindListener
                                or MessageBrokerRemoteClientTraceEventType.Unexpected )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var channel = server.Channels.TryGetById( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c => c.SendBindListenerRequest( "foo", true, queueName: "bar", isEphemeral: true ) );
        await endSource.Task;

        Assertion.All(
                server.Streams.Count.TestEquals( 1 ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestContainsSequence( [ binding ] ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ binding ] ),
                    c.State.TestEquals( MessageBrokerRemoteClientState.Inactive ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Inactive ),
                    p.QueueBindings.Primary.Queue.Listeners.Count.TestEquals( 1 ),
                    p.QueueBindings.Primary.Queue.State.TestEquals( MessageBrokerQueueState.Inactive ) ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestTrue(),
                clientLogs.GetAll()
                    .TakeLast( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 46)",
                            "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', QueueName = 'bar', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = True",
                            "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
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
    public async Task Rebind_ShouldReactivateInactiveListener_WithSettingsChanges()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages(
            streamId: 1,
            messages:
            [
                StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ),
                StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] ),
                StorageScope.PrepareStreamMessage( id: 2, storeKey: 2, senderId: 1, channelId: 1, data: [ 4, 5, 6 ] )
            ] );

        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WriteListenerMetadata(
            clientId: 1,
            channelId: 1,
            queueId: 1,
            prefetchHint: 5,
            maxRetries: 5,
            retryDelay: Duration.FromMinutes( 1 ),
            maxRedeliveries: 5,
            minAckTimeout: Duration.FromMinutes( 10 ),
            deadLetterCapacityHint: 5,
            minDeadLetterRetention: Duration.FromHours( 1 ) );

        storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 1,
            messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ) ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

        storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
        storage.WriteQueuePendingMessages(
            clientId: 1,
            queueId: 2,
            messages:
            [
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 1 ),
                StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 2 )
            ] );

        storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
        storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 2, messages: [ ] );

        var endSource = new SafeTaskCompletionSource( completionCount: 6 );
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetQueueLoggerFactory( _ => MessageBrokerQueueLogger.Create(
                    traceEnd: e =>
                    {
                        if ( e.Type == MessageBrokerQueueTraceEventType.Deactivate )
                            endSource.Complete();
                    } ) )
                .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                    traceEnd: e =>
                    {
                        if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindListener
                            or MessageBrokerRemoteClientTraceEventType.MessageNotification )
                            endSource.Complete();
                    } ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "foo" );
            c.ReadPublisherBoundResponse();
            c.SendBindListenerRequest( "foo", true, isEphemeral: true );
            c.ReadListenerBoundResponse();
            c.Read( (Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload) * 3 + 6 );
            c.SendPushMessage( 1, [ 7, 8, 9, 10 ], confirm: false );
            c.ReadMessageNotification( 4 );
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Queues.Count.TestEquals( 1 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "listener",
                    p.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ),
                    p.IsEphemeral.TestTrue(),
                    p.QueueBindings.Count.TestEquals( 1 ),
                    p.QueueBindings.Primary.Queue.Id.TestEquals( 1 ),
                    p.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Running ),
                    p.QueueBindings.Primary.IsPrimary.TestTrue(),
                    p.QueueBindings.Primary.Queue.Listeners.Count.TestEquals( 1 ),
                    p.QueueBindings.Primary.PrefetchHint.TestEquals( 1 ),
                    p.QueueBindings.Primary.MaxRetries.TestEquals( 0 ),
                    p.QueueBindings.Primary.RetryDelay.TestEquals( Duration.Zero ),
                    p.QueueBindings.Primary.MaxRedeliveries.TestEquals( 0 ),
                    p.QueueBindings.Primary.MinAckTimeout.TestEquals( Duration.Zero ),
                    p.QueueBindings.Primary.DeadLetterCapacityHint.TestEquals( 0 ),
                    p.QueueBindings.Primary.MinDeadLetterRetention.TestEquals( Duration.Zero ),
                    p.QueueBindings.Primary.FilterExpression.TestNull(),
                    p.QueueBindings.Primary.AreAcksEnabled.TestFalse(),
                    p.QueueBindings.TryGetByQueueId( 2 ).TestNull() ) ),
                storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse() )
            .Go();
    }
}
