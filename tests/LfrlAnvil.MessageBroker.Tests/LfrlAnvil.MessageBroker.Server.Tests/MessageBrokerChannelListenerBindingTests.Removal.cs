using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerChannelListenerBindingTests
{
    public class Removal : TestsBase, IClassFixture<SharedResourceFixture>
    {
        private readonly ValueTaskDelaySource _sharedDelaySource;

        public Removal(SharedResourceFixture fixture)
        {
            _sharedDelaySource = fixture.DelaySource;
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
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
            await client.GetTask( c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

            var remoteClient = server.Clients.TryGetById( 1 );
            var channel = server.Channels.TryGetByName( "c" );
            var binding = channel?.Listeners.TryGetByClientId( 1 );
            var queue = binding?.Queue;
            await client.GetTask( c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    channel.TestNotNull( c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                    queue.TestNotNull( q =>
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
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
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
                    .SetClientLoggerFactory( c => c.Id == 2
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
            var channel = server.Channels.TryGetByName( "c" );
            var binding1 = channel?.Listeners.TryGetByClientId( 1 );
            var binding2 = channel?.Listeners.TryGetByClientId( 2 );
            var queue1 = binding1?.Queue;
            var queue2 = binding2?.Queue;

            await client2.GetTask( c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    channel.TestNotNull( c => Assertion.All(
                        "channel",
                        c.Server.TestRefEquals( server ),
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "c" ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ),
                        c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                        c.Listeners.TryGetByClientId( 2 ).TestNull() ) ),
                    remoteClient1.TestNotNull( c => Assertion.All(
                        "client1",
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding1 ) ] ),
                        c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding1 ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue1 ) ] ) ) ),
                    remoteClient2.TestNotNull( c => Assertion.All(
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
                                "[Deactivating] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [2] 'test2', Queue = [1] 'c', TraceId = 1, IsAlive = False",
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
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
            await client.GetTask( c =>
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
            await client.GetTask( c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    queue1.TestRefEquals( queue2 ),
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 1 ),
                        c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding2 ) ] ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.GetAll().TestSequence( [ (q, _) => q.TestRefEquals( queue1 ) ] ) ) ),
                    queue1.TestNotNull( q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.Listeners.Count.TestEquals( 1 ),
                        q.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( binding2?.QueueBindings.Primary ) ] ) ) ),
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
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
            await client.GetTask( c =>
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
            await client.GetTask( c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    channel.TestNotNull( c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( publisherBinding ) ] ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Publishers.Count.TestEquals( 1 ),
                        c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( publisherBinding ) ] ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                    queue.TestNotNull( q => Assertion.All(
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
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
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
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
            await client.GetTask( c =>
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
                                "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
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
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
            await client.GetTask( c =>
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
                                "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
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
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
            await client.GetTask( c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadUnbindListenerFailureResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    exception.TestType()
                        .Exact<MessageBrokerChannelListenerBindingException>( exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
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
                    .SetClientLoggerFactory( c => c.Id == 2
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
            await client1.GetTask( c =>
            {
                c.SendBindListenerRequest( "c", createChannelIfNotExists: true );
                c.ReadListenerBoundResponse();
            } );

            using var client2 = new ClientMock();
            await client2.EstablishHandshake( server, "test2" );
            await client2.GetTask( c =>
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
                        .Exact<MessageBrokerChannelListenerBindingException>( exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient2 ),
                            exc.Listener.TestNull() ) ),
                    remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                    remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                    channel.TestNotNull( c => Assertion.All(
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

        [Fact]
        public async Task Unbind_ShouldUnbindNonEphemeralListener()
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                endSource.Complete();
                        } ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, isEphemeral: false );
            await client.GetTask( c =>
            {
                c.SendBindListenerRequest( "c", true, isEphemeral: false );
                c.ReadListenerBoundResponse();
            } );

            var remoteClient = server.Clients.TryGetById( 1 );
            var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );
            await client.GetTask( c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                    binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                    storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                    storage.FileExists( StorageScope.GetQueueMetadataSubpath( clientId: 1, queueId: 1 ) ).TestFalse(),
                    storage.FileExists( StorageScope.GetChannelMetadataSubpath( channelId: 1 ) ).TestFalse() )
                .Go();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public async Task Unbind_ShouldUnbindInactiveListener(bool isClientEphemeral)
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
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                endSource.Complete();
                        } ) ) );

            await server.StartAsync();

            var remoteClient = server.Clients.TryGetById( 1 );
            var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

            using var client = new ClientMock();
            await client.EstablishHandshake( server, isEphemeral: isClientEphemeral );
            await client.GetTask( c =>
            {
                c.SendUnbindListenerRequest( channelId: 1 );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ) ) ),
                    binding.TestNotNull( p => Assertion.All(
                        "listener",
                        p.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ),
                        p.IsEphemeral.TestEquals( isClientEphemeral ) ) ),
                    storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse() )
                .Go();
        }

        [Fact]
        public async Task Unbind_ShouldDisposeListenerWithSecondaryBindings()
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
                    StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] )
                ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "test" );
            storage.WriteListenerMetadata(
                clientId: 1,
                channelId: 1,
                queueId: 1,
                maxRedeliveries: 1,
                minAckTimeout: Duration.FromMinutes( 1 ),
                deadLetterCapacityHint: 5,
                minDeadLetterRetention: Duration.FromHours( 1 ) );

            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 2,
                messages:
                [
                    StorageScope.PrepareQueueDeadLetterMessage(
                        streamId: 1,
                        storeKey: 0,
                        retry: 0,
                        redelivery: 0,
                        expiresAt: TimestampProvider.Shared.GetNow() + Duration.FromHours( 1 ) )
                ] );

            storage.WriteQueueMetadata( clientId: 1, queueId: 3, queueName: "b" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 3, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 3, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 3, messages: [ ] );
            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 3,
                messages:
                [
                    StorageScope.PrepareQueueDeadLetterMessage(
                        streamId: 1,
                        storeKey: 1,
                        retry: 0,
                        redelivery: 0,
                        expiresAt: TimestampProvider.Shared.GetNow() + Duration.FromHours( 1 ) )
                ] );

            var endSource = new SafeTaskCompletionSource( completionCount: 5 );
            var queueLogs = new[] { new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger() };
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetRootStoragePath( storage.Path )
                    .SetQueueLoggerFactory( q => queueLogs[q.Id - 1]
                        .GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerQueueTraceEventType.ProcessMessage
                                        or MessageBrokerQueueTraceEventType.Deactivate )
                                        endSource.Complete();
                                } ) ) )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                endSource.Complete();
                        } ) ) );

            await server.StartAsync();

            var remoteClient = server.Clients.TryGetById( 1 );
            var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

            using var client = new ClientMock();
            await client.EstablishHandshake( server, isEphemeral: false );
            await client.GetTask( c =>
            {
                c.SendUnbindListenerRequest( channelId: 1 );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Queues.Count.TestEquals( 0 ) ) ),
                    binding.TestNotNull( p => Assertion.All(
                        "listener",
                        p.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ),
                        p.QueueBindings.Count.TestEquals( 1 ),
                        p.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Disposed ) ) ),
                    storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                    queueLogs[0]
                        .GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 3",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo' (removed)",
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                            ] )
                        ] ),
                    queueLogs[1]
                        .GetAll()
                        .TakeLast( 3 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, ClientTraceId = 3",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, Channel = [1] 'foo' (removed)",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [2] 'a', TraceId = 3 (start)",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [2] 'a', TraceId = 3, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Reason = DisposedDeadLetter, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = True, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [2] 'a', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [2] 'a', TraceId = 4 (start)",
                                "[Deactivating] Client = [1] 'test', Queue = [2] 'a', TraceId = 4, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [2] 'a', TraceId = 4, IsAlive = False",
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [2] 'a', TraceId = 4 (end)"
                            ] )
                        ] ),
                    queueLogs[2]
                        .GetAll()
                        .TakeLast( 3 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, ClientTraceId = 3",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, Channel = [1] 'foo' (removed)",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [3] 'b', TraceId = 3 (start)",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [3] 'b', TraceId = 3, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Reason = DisposedDeadLetter, StoreKey = 1, Retry = 0, Redelivery = 0, MessageRemoved = True, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [3] 'b', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [3] 'b', TraceId = 4 (start)",
                                "[Deactivating] Client = [1] 'test', Queue = [3] 'b', TraceId = 4, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [3] 'b', TraceId = 4, IsAlive = False",
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [3] 'b', TraceId = 4 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindByName_ShouldUnbindLastClientFromChannelAndQueueAndRemoveThem()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) )
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
            var channel = server.Channels.TryGetByName( "c" );
            var binding = channel?.Listeners.TryGetByClientId( 1 );
            var queue = binding?.Queue;
            await client.GetTask( c =>
            {
                c.SendUnbindListenerByNameRequest( "c" );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    channel.TestNotNull( c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty() ) ),
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Listeners.GetAll().TestEmpty(),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Queues.GetAll().TestEmpty() ) ),
                    queue.TestNotNull( q =>
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
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerByNameRequest, Length = 6)",
                                "[UnbindingListener] Client = [1] 'test', TraceId = 2, ChannelName = 'c'",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (UnbindListenerByNameRequest, Length = 6)",
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
                            "[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerByNameRequest, Length = 6)"
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
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'c', TraceId = 1, IsAlive = False",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 1 (end)"
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            var channelName = new string( 'x', nameLength );
            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask( c => c.SendUnbindListenerByNameRequest( channelName ) );
            await endSource.Task;

            var requestLength = channelName.Length + 5;

            Assertion.All(
                    remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                    server.Clients.Count.TestEquals( 0 ),
                    server.Clients.GetAll().TestEmpty(),
                    server.Channels.Count.TestEquals( 0 ),
                    server.Channels.GetAll().TestEmpty(),
                    clientLogs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 1 (start)",
                                $"[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (UnbindListenerByNameRequest, Length = {requestLength})",
                                $"""
                                 [Error] Client = [1] 'test', TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid UnbindListenerByNameRequest from client [1] 'test'. Encountered 1 error(s):
                                 1. Expected channel name length to be in [1, 512] range but found {nameLength}.
                                 """,
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            $"[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerByNameRequest, Length = {requestLength})"
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );

            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask( c =>
            {
                c.SendUnbindListenerByNameRequest( "foo" );
                c.ReadUnbindListenerFailureResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    exception.TestType()
                        .Exact<MessageBrokerChannelListenerBindingException>( exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
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
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (UnbindListenerByNameRequest, Length = 8)",
                                "[UnbindingListener] Client = [1] 'test', TraceId = 1, ChannelName = 'foo'",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelListenerBindingException: Client [1] 'test' could not be unbound as a listener from non-existing channel 'foo'.
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
                            "[AwaitPacket] Client = [1] 'test', Packet = (UnbindListenerByNameRequest, Length = 8)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindByName_ShouldBeRejected_WhenClientIsNotBoundAsListenerToChannel()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) )
                        : null ) );

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
                c.SendUnbindListenerByNameRequest( "c" );
                c.ReadUnbindListenerFailureResponse();
            } );

            await endSource.Task;

            var channel = server.Channels.TryGetByName( "c" );
            var remoteClient1 = server.Clients.TryGetById( 1 );
            var remoteClient2 = server.Clients.TryGetById( 2 );

            Assertion.All(
                    exception.TestType()
                        .Exact<MessageBrokerChannelListenerBindingException>( exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient2 ),
                            exc.Listener.TestNull() ) ),
                    remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                    remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                    channel.TestNotNull( c => Assertion.All(
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
                                "[ReadPacket:Received] Client = [2] 'test2', TraceId = 1, Packet = (UnbindListenerByNameRequest, Length = 6)",
                                "[UnbindingListener] Client = [2] 'test2', TraceId = 1, ChannelName = 'c'",
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
                            "[AwaitPacket] Client = [2] 'test2', Packet = (UnbindListenerByNameRequest, Length = 6)"
                        ] ) )
                .Go();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public async Task UnbindByName_ShouldUnbindInactiveListener(bool isClientEphemeral)
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
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetRootStoragePath( storage.Path )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                endSource.Complete();
                        } ) ) );

            await server.StartAsync();

            var remoteClient = server.Clients.TryGetById( 1 );
            var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

            using var client = new ClientMock();
            await client.EstablishHandshake( server, isEphemeral: isClientEphemeral );
            await client.GetTask( c =>
            {
                c.SendUnbindListenerByNameRequest( "foo" );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ) ) ),
                    binding.TestNotNull( p => Assertion.All(
                        "listener",
                        p.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ),
                        p.IsEphemeral.TestEquals( isClientEphemeral ) ) ),
                    storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse() )
                .Go();
        }

        [Fact]
        public async Task UnbindByName_ShouldDisposeListenerWithSecondaryBindings()
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
                    StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] )
                ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "test" );
            storage.WriteListenerMetadata(
                clientId: 1,
                channelId: 1,
                queueId: 1,
                maxRedeliveries: 1,
                minAckTimeout: Duration.FromMinutes( 1 ),
                deadLetterCapacityHint: 5,
                minDeadLetterRetention: Duration.FromHours( 1 ) );

            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 2,
                messages:
                [
                    StorageScope.PrepareQueueDeadLetterMessage(
                        streamId: 1,
                        storeKey: 0,
                        retry: 0,
                        redelivery: 0,
                        expiresAt: TimestampProvider.Shared.GetNow() + Duration.FromHours( 1 ) )
                ] );

            storage.WriteQueueMetadata( clientId: 1, queueId: 3, queueName: "b" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 3, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 3, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 3, messages: [ ] );
            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 3,
                messages:
                [
                    StorageScope.PrepareQueueDeadLetterMessage(
                        streamId: 1,
                        storeKey: 1,
                        retry: 0,
                        redelivery: 0,
                        expiresAt: TimestampProvider.Shared.GetNow() + Duration.FromHours( 1 ) )
                ] );

            var endSource = new SafeTaskCompletionSource( completionCount: 5 );
            var queueLogs = new[] { new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger() };
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetRootStoragePath( storage.Path )
                    .SetQueueLoggerFactory( q => queueLogs[q.Id - 1]
                        .GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerQueueTraceEventType.ProcessMessage
                                        or MessageBrokerQueueTraceEventType.Deactivate )
                                        endSource.Complete();
                                } ) ) )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                endSource.Complete();
                        } ) ) );

            await server.StartAsync();

            var remoteClient = server.Clients.TryGetById( 1 );
            var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

            using var client = new ClientMock();
            await client.EstablishHandshake( server, isEphemeral: false );
            await client.GetTask( c =>
            {
                c.SendUnbindListenerByNameRequest( channelName: "foo" );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Queues.Count.TestEquals( 0 ) ) ),
                    binding.TestNotNull( p => Assertion.All(
                        "listener",
                        p.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ),
                        p.QueueBindings.Count.TestEquals( 1 ),
                        p.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Disposed ) ) ),
                    storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                    queueLogs[0]
                        .GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 3",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo' (removed)",
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                            ] )
                        ] ),
                    queueLogs[1]
                        .GetAll()
                        .TakeLast( 3 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, ClientTraceId = 3",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, Channel = [1] 'foo' (removed)",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [2] 'a', TraceId = 3 (start)",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [2] 'a', TraceId = 3, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Reason = DisposedDeadLetter, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = True, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [2] 'a', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [2] 'a', TraceId = 4 (start)",
                                "[Deactivating] Client = [1] 'test', Queue = [2] 'a', TraceId = 4, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [2] 'a', TraceId = 4, IsAlive = False",
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [2] 'a', TraceId = 4 (end)"
                            ] )
                        ] ),
                    queueLogs[2]
                        .GetAll()
                        .TakeLast( 3 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, ClientTraceId = 3",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, Channel = [1] 'foo' (removed)",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [3] 'b', TraceId = 3 (start)",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [3] 'b', TraceId = 3, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Reason = DisposedDeadLetter, StoreKey = 1, Retry = 0, Redelivery = 0, MessageRemoved = True, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [3] 'b', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [3] 'b', TraceId = 4 (start)",
                                "[Deactivating] Client = [1] 'test', Queue = [3] 'b', TraceId = 4, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [3] 'b', TraceId = 4, IsAlive = False",
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [3] 'b', TraceId = 4 (end)"
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
        public async Task DeleteAsync_ShouldDeleteNonEphemeralListenerAndSendSystemNotificationToClient()
        {
            using var storage = StorageScope.Create();
            var bindEndSource = new SafeTaskCompletionSource();
            var endSource = new SafeTaskCompletionSource();
            var clientLogs = new ClientEventLogger();
            var channelLogs = new ChannelEventLogger();
            var queueLogs = new QueueEventLogger();

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetRootStoragePath( storage.Path )
                    .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger() )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                    bindEndSource.Complete();
                                else if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, isEphemeral: false );
            await client.GetTask( c =>
            {
                c.SendBindListenerRequest( "foo", true, isEphemeral: false );
                c.ReadListenerBoundResponse();
            } );

            await bindEndSource.Task;

            var remoteClient = server.Clients.TryGetById( 1 );
            var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );
            var deleteTask = binding?.DeleteAsync().AsTask() ?? Task.CompletedTask;

            await client.GetTask( c => c.ReadListenerDeletedSystemNotification( "foo" ) );
            await deleteTask;
            await endSource.Task;

            Assertion.All(
                    server.Channels.Count.TestEquals( 0 ),
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Queues.Count.TestEquals( 0 ) ) ),
                    binding.TestNotNull( p => Assertion.All(
                        "listener",
                        p.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ),
                        p.Channel.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        p.Queue.State.TestEquals( MessageBrokerQueueState.Disposed ) ) ),
                    storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                    channelLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Channel = [1] 'foo', TraceId = 1 (start)",
                                "[ClientTrace] Channel = [1] 'foo', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                                "[ListenerUnbound] Channel = [1] 'foo', TraceId = 1, Client = [1] 'test', Queue = [1] 'foo' (removed)",
                                "[Disposing] Channel = [1] 'foo', TraceId = 1",
                                "[Disposed] Channel = [1] 'foo', TraceId = 1",
                                "[Trace:UnbindListener] Channel = [1] 'foo', TraceId = 1 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 1 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 1, ClientTraceId = 2",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 1, Channel = [1] 'foo' (removed)",
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 1, IsAlive = False",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 1 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                                "[ListenerUnbound] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo' (removed), Queue = [1] 'foo' (removed)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 9)",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteInactiveListenerAndSendSystemNotificationToClient()
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
            var channelLogs = new ChannelEventLogger();
            var queueLogs = new QueueEventLogger();

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetRootStoragePath( storage.Path )
                    .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger() )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, isEphemeral: false );

            var remoteClient = server.Clients.TryGetById( 1 );
            var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );
            var deleteTask = binding?.DeleteAsync().AsTask() ?? Task.CompletedTask;

            await client.GetTask( c => c.ReadListenerDeletedSystemNotification( "foo" ) );
            await deleteTask;
            await endSource.Task;

            Assertion.All(
                    server.Channels.Count.TestEquals( 0 ),
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Queues.Count.TestEquals( 0 ) ) ),
                    binding.TestNotNull( p => Assertion.All(
                        "listener",
                        p.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ),
                        p.Channel.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        p.Queue.State.TestEquals( MessageBrokerQueueState.Disposed ) ) ),
                    storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                    channelLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Channel = [1] 'foo', TraceId = 3 (start)",
                                "[ClientTrace] Channel = [1] 'foo', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 3)",
                                "[ListenerUnbound] Channel = [1] 'foo', TraceId = 3, Client = [1] 'test', Queue = [1] 'foo' (removed)",
                                "[Disposing] Channel = [1] 'foo', TraceId = 3",
                                "[Disposed] Channel = [1] 'foo', TraceId = 3",
                                "[Trace:UnbindListener] Channel = [1] 'foo', TraceId = 3 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 3",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo' (removed)",
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (start)",
                                "[ListenerUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo' (removed), Queue = [1] 'foo' (removed)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (SystemNotification, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (SystemNotification, Length = 9)",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteInactiveListenerAndNotSendSystemNotificationToInactiveClient()
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
            var channelLogs = new ChannelEventLogger();
            var queueLogs = new QueueEventLogger();

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetRootStoragePath( storage.Path )
                    .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger() )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            var remoteClient = server.Clients.TryGetById( 1 );
            var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );
            await (binding?.DeleteAsync().AsTask() ?? Task.CompletedTask);
            await endSource.Task;

            Assertion.All(
                    server.Channels.Count.TestEquals( 0 ),
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Queues.Count.TestEquals( 0 ) ) ),
                    binding.TestNotNull( p => Assertion.All(
                        "listener",
                        p.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ),
                        p.Channel.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        p.Queue.State.TestEquals( MessageBrokerQueueState.Disposed ) ) ),
                    storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                    channelLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Channel = [1] 'foo', TraceId = 3 (start)",
                                "[ClientTrace] Channel = [1] 'foo', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 2)",
                                "[ListenerUnbound] Channel = [1] 'foo', TraceId = 3, Client = [1] 'test', Queue = [1] 'foo' (removed)",
                                "[Disposing] Channel = [1] 'foo', TraceId = 3",
                                "[Disposed] Channel = [1] 'foo', TraceId = 3",
                                "[Trace:UnbindListener] Channel = [1] 'foo', TraceId = 3 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 2",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo' (removed)",
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (start)",
                                "[ListenerUnbound] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo' (removed), Queue = [1] 'foo' (removed)",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteListenerAndWakeUpQueueProcessorWhenMessageForDeletedListenerIsBlockingIt()
        {
            using var storage = StorageScope.Create();
            storage.WriteServerMetadata();
            storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
            storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
            storage.WriteStreamMessages(
                streamId: 1,
                messages: [ StorageScope.PrepareStreamMessage( id: 0, storeKey: 0, senderId: 1, channelId: 1, data: [ 1 ] ) ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "test" );
            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages(
                clientId: 1,
                queueId: 1,
                messages: [ StorageScope.PrepareQueuePendingMessage( streamId: 1, storeKey: 0 ) ] );

            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteListenerMetadata( clientId: 1, channelId: 1, queueId: 1 );

            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var clientLogs = new ClientEventLogger();
            var channelLogs = new ChannelEventLogger();
            var queueLogs = new QueueEventLogger();

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetRootStoragePath( storage.Path )
                    .SetChannelLoggerFactory( _ => channelLogs.GetLogger() )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.Deactivate )
                                    endSource.Complete();
                            } ) ) )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, isEphemeral: false );
            await Task.Delay( 15 );

            var remoteClient = server.Clients.TryGetById( 1 );
            var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );
            await (binding?.DeleteAsync().AsTask() ?? Task.CompletedTask);
            await endSource.Task;

            Assertion.All(
                    server.Channels.Count.TestEquals( 0 ),
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Queues.Count.TestEquals( 0 ) ) ),
                    binding.TestNotNull( p => Assertion.All(
                        "listener",
                        p.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ),
                        p.Channel.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        p.Queue.State.TestEquals( MessageBrokerQueueState.Disposed ) ) ),
                    storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                    channelLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Channel = [1] 'foo', TraceId = 3 (start)",
                                "[ClientTrace] Channel = [1] 'foo', TraceId = 3, Correlation = (Client = [1] 'test', TraceId = 3)",
                                "[ListenerUnbound] Channel = [1] 'foo', TraceId = 3, Client = [1] 'test', Queue = [1] 'foo'",
                                "[Disposing] Channel = [1] 'foo', TraceId = 3",
                                "[Disposed] Channel = [1] 'foo', TraceId = 3",
                                "[Trace:UnbindListener] Channel = [1] 'foo', TraceId = 3 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .TakeLast( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'foo', TraceId = 4 (start)",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [1] 'foo', TraceId = 4, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Reason = DisposedPending, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = True, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'foo', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [1] 'foo', TraceId = 5 (start)",
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 5, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 5, IsAlive = False",
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [1] 'foo', TraceId = 5 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (start)",
                                "[ListenerUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo' (removed), Queue = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (SystemNotification, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (SystemNotification, Length = 9)",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 3 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task DeleteAsync_ShouldDisposeListenerWithSecondaryBindings()
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
                    StorageScope.PrepareStreamMessage( id: 1, storeKey: 1, senderId: 1, channelId: 1, data: [ 2, 3 ] )
                ] );

            storage.WriteClientMetadata( clientId: 1, clientName: "test" );
            storage.WriteListenerMetadata(
                clientId: 1,
                channelId: 1,
                queueId: 1,
                maxRedeliveries: 1,
                minAckTimeout: Duration.FromMinutes( 1 ),
                deadLetterCapacityHint: 5,
                minDeadLetterRetention: Duration.FromHours( 1 ) );

            storage.WriteQueueMetadata( clientId: 1, queueId: 1, queueName: "foo" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 1, messages: [ ] );
            storage.WriteQueueDeadLetterMessages( clientId: 1, queueId: 1, messages: [ ] );

            storage.WriteQueueMetadata( clientId: 1, queueId: 2, queueName: "a" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 2, messages: [ ] );
            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 2,
                messages:
                [
                    StorageScope.PrepareQueueDeadLetterMessage(
                        streamId: 1,
                        storeKey: 0,
                        retry: 0,
                        redelivery: 0,
                        expiresAt: TimestampProvider.Shared.GetNow() + Duration.FromHours( 1 ) )
                ] );

            storage.WriteQueueMetadata( clientId: 1, queueId: 3, queueName: "b" );
            storage.WriteQueuePendingMessages( clientId: 1, queueId: 3, messages: [ ] );
            storage.WriteQueueUnackedMessages( clientId: 1, queueId: 3, messages: [ ] );
            storage.WriteQueueRetryMessages( clientId: 1, queueId: 3, messages: [ ] );
            storage.WriteQueueDeadLetterMessages(
                clientId: 1,
                queueId: 3,
                messages:
                [
                    StorageScope.PrepareQueueDeadLetterMessage(
                        streamId: 1,
                        storeKey: 1,
                        retry: 0,
                        redelivery: 0,
                        expiresAt: TimestampProvider.Shared.GetNow() + Duration.FromHours( 1 ) )
                ] );

            var endSource = new SafeTaskCompletionSource( completionCount: 4 );
            var queueLogs = new[] { new QueueEventLogger(), new QueueEventLogger(), new QueueEventLogger() };
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetRootStoragePath( storage.Path )
                    .SetQueueLoggerFactory( q => queueLogs[q.Id - 1]
                        .GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerQueueTraceEventType.ProcessMessage
                                        or MessageBrokerQueueTraceEventType.Deactivate )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, isEphemeral: false );
            await Task.Delay( 15 );

            var remoteClient = server.Clients.TryGetById( 1 );
            var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );
            if ( binding is not null )
                await binding.DeleteAsync();

            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => Assertion.All(
                        "client",
                        c.Listeners.Count.TestEquals( 0 ),
                        c.Queues.Count.TestEquals( 0 ) ) ),
                    binding.TestNotNull( p => Assertion.All(
                        "listener",
                        p.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ),
                        p.QueueBindings.Count.TestEquals( 1 ),
                        p.QueueBindings.Primary.State.TestEquals( MessageBrokerQueueListenerBindingState.Disposed ) ) ),
                    storage.FileExists( StorageScope.GetListenerMetadataSubpath( clientId: 1, channelId: 1 ) ).TestFalse(),
                    queueLogs[0]
                        .GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, ClientTraceId = 3",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, Channel = [1] 'foo' (removed)",
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3, IsAlive = False",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'foo', TraceId = 3 (end)"
                            ] )
                        ] ),
                    queueLogs[1]
                        .GetAll()
                        .TakeLast( 3 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, ClientTraceId = 3",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [2] 'a', TraceId = 2, Channel = [1] 'foo' (removed)",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [2] 'a', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [2] 'a', TraceId = 3 (start)",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [2] 'a', TraceId = 3, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Reason = DisposedDeadLetter, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = True, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [2] 'a', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [2] 'a', TraceId = 4 (start)",
                                "[Deactivating] Client = [1] 'test', Queue = [2] 'a', TraceId = 4, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [2] 'a', TraceId = 4, IsAlive = False",
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [2] 'a', TraceId = 4 (end)"
                            ] )
                        ] ),
                    queueLogs[2]
                        .GetAll()
                        .TakeLast( 3 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, ClientTraceId = 3",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [3] 'b', TraceId = 2, Channel = [1] 'foo' (removed)",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [3] 'b', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [3] 'b', TraceId = 3 (start)",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [3] 'b', TraceId = 3, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Reason = DisposedDeadLetter, StoreKey = 1, Retry = 0, Redelivery = 0, MessageRemoved = True, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [3] 'b', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [3] 'b', TraceId = 4 (start)",
                                "[Deactivating] Client = [1] 'test', Queue = [3] 'b', TraceId = 4, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [3] 'b', TraceId = 4, IsAlive = False",
                                "[Trace:Deactivate] Client = [1] 'test', Queue = [3] 'b', TraceId = 4 (end)"
                            ] )
                        ] ) )
                .Go();
        }
    }
}
