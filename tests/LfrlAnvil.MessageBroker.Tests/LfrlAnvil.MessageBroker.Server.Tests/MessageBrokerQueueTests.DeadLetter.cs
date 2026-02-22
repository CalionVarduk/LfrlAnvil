using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerQueueTests
{
    public class DeadLetter : TestsBase, IClassFixture<SharedResourceFixture>
    {
        private readonly ValueTaskDelaySource _sharedDelaySource;

        public DeadLetter(SharedResourceFixture fixture)
        {
            _sharedDelaySource = fixture.DelaySource;
        }

        [Fact]
        public async Task Expiration_ShouldRemoveMessageFromDeadLetter()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 4 );
            var queueLogs = new QueueEventLogger();
            var messageRemoved = Atomic.Create( false );
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.MessageNotification
                                or MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                endSource.Complete();
                        } ) )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                    endSource.Complete();
                            },
                            messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest(
                    "c",
                    false,
                    deadLetterCapacityHint: 1,
                    minDeadLetterRetention: Duration.FromMilliseconds( 15 ),
                    minAckTimeout: Duration.FromMinutes( 10 ) );

                c.ReadListenerBoundResponse();
                c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                c.ReadMessageNotification( 3 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, noRetry: true );
            } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await endSource.Task;

            Assertion.All(
                    queue.TestNotNull( q => Assertion.All(
                        "queue",
                        q.Messages.Pending.Count.TestEquals( 0 ),
                        q.Messages.Unacked.Count.TestEquals( 0 ),
                        q.Messages.Retries.Count.TestEquals( 0 ),
                        q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:ProcessMessage]" ) ) )
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (start)",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = DeadLetterExpiration, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Expiration_ShouldRemoveMessageFromDeadLetter_ForDisposedListener()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 4 );
            var unbindContinuation = new SafeTaskCompletionSource();
            var queueLogs = new QueueEventLogger();
            var messageRemoved = Atomic.Create( false );
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.MessageNotification
                                or MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                endSource.Complete();
                            else if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                unbindContinuation.Complete();
                        } ) )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                {
                                    endSource.Complete();
                                    unbindContinuation.Task.Wait();
                                }
                            },
                            messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest(
                    "c",
                    false,
                    deadLetterCapacityHint: 1,
                    minDeadLetterRetention: Duration.FromMilliseconds( 15 ),
                    minAckTimeout: Duration.FromMinutes( 10 ) );

                c.ReadListenerBoundResponse();
                c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                c.ReadMessageNotification( 3 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, noRetry: true );
            } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await client.GetTask( c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    queue.TestNotNull( q => Assertion.All(
                        "queue",
                        q.Messages.Pending.Count.TestEquals( 0 ),
                        q.Messages.Unacked.Count.TestEquals( 0 ),
                        q.Messages.Retries.Count.TestEquals( 0 ),
                        q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:ProcessMessage]" ) ) )
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (start)",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = DisposedDeadLetter, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task CapacityExceeded_ShouldRemoveMessageFromDeadLetter()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 7 );
            var queueLogs = new QueueEventLogger();
            var messageRemoved = Atomic.Create( false );
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.MessageNotification
                                or MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                endSource.Complete();
                        } ) )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                    endSource.Complete();
                            },
                            messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest(
                    "c",
                    false,
                    prefetchHint: 2,
                    deadLetterCapacityHint: 1,
                    minDeadLetterRetention: Duration.FromSeconds( 10 ),
                    minAckTimeout: Duration.FromMinutes( 10 ) );

                c.ReadListenerBoundResponse();
                c.SendPushMessage( 1, [ 1, 2 ], confirm: false );
                c.ReadMessageNotification( 2 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, noRetry: true );
                c.SendPushMessage( 1, [ 3, 4, 5 ], confirm: false );
                c.ReadMessageNotification( 3 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 1, noRetry: true );
            } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await endSource.Task;

            Assertion.All(
                    queue.TestNotNull( q => Assertion.All(
                        "queue",
                        q.Messages.Pending.Count.TestEquals( 0 ),
                        q.Messages.Unacked.Count.TestEquals( 0 ),
                        q.Messages.Retries.Count.TestEquals( 0 ),
                        q.Messages.DeadLetter.Count.TestEquals( 1 ) ) ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:ProcessMessage]" ) ) )
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 7 (start)",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 7, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = DeadLetterCapacityExceeded, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 7 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task DisposedListener_ShouldRemoveMessageFromDeadLetter()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 4 );
            var unbindContinuation = new SafeTaskCompletionSource();
            var unbindCompletion = new SafeTaskCompletionSource();
            var queueLogs = new QueueEventLogger();
            var completedProcessMessages = Atomic.Create( 0 );
            var messageRemoved = Atomic.Create( false );
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                endSource.Complete();
                            else if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                unbindCompletion.Complete();
                        } ) )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                {
                                    if ( completedProcessMessages.Value == 1 )
                                    {
                                        unbindContinuation.Complete();
                                        unbindCompletion.Task.Wait();
                                    }

                                    ++completedProcessMessages.Value;
                                    endSource.Complete();
                                }
                            },
                            messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest(
                    "c",
                    false,
                    deadLetterCapacityHint: 1,
                    minDeadLetterRetention: Duration.FromSeconds( 10 ),
                    minAckTimeout: Duration.FromMilliseconds( 15 ) );

                c.ReadListenerBoundResponse();
                c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                c.ReadMessageNotification( 3 );
            } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await unbindContinuation.Task;
            await client.GetTask( c =>
            {
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    queue.TestNotNull( q => Assertion.All(
                        "queue",
                        q.Messages.Pending.Count.TestEquals( 0 ),
                        q.Messages.Unacked.Count.TestEquals( 0 ),
                        q.Messages.Retries.Count.TestEquals( 0 ),
                        q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:ProcessMessage]" ) ) )
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (start)",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = DisposedDeadLetter, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindListener_ShouldStartDiscardingDeadLetterMessagesForThatListenerIfPossible()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 6 );
            var queueLogs = new QueueEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.UnbindListener
                                or MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                endSource.Complete();
                        } ) )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest(
                    "c",
                    false,
                    deadLetterCapacityHint: 2,
                    minDeadLetterRetention: Duration.FromHours( 1 ),
                    minAckTimeout: Duration.FromMinutes( 1 ) );

                c.ReadListenerBoundResponse();
                c.SendPushMessage( 1, [ 1 ], confirm: false );
                c.SendPushMessage( 1, [ 1, 2 ], confirm: false );
                c.ReadMessageNotification( 1 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 0 );
                c.ReadMessageNotification( 2 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 1 );
            } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await client.GetTask( c => c.SendUnbindListenerRequest( 1 ) );
            await endSource.Task;

            Assertion.All(
                    queue.TestNotNull( q => q.Messages.DeadLetter.Count.TestEquals( 0 ) ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:ProcessMessage]" ) ) )
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 8 (start)",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 8, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = DisposedDeadLetter, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = True, MovedToDeadLetter = False",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 8, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = DisposedDeadLetter, StoreKey = 1, Retry = 0, Redelivery = 0, MessageRemoved = True, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 8 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Query_ShouldGetExistingDeadLetterCount_WhenRequestedReadCountIsEqualToZero()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.DeadLetterQuery )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c =>
            {
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
                c.SendDeadLetterQuery( 1, 0 );
                c.ReadDeadLetterQueryResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 2, QueueId = 1, ReadCount = 0",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                "[DeadLetterQueried] Client = [1] 'test', TraceId = 2, Queue = [1] 'c', TotalCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQuery, Length = 13)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Query_ShouldNotAllowToExceedActualDeadLetterCount()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.DeadLetterQuery )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c =>
            {
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
                c.SendDeadLetterQuery( 1, 5 );
                c.ReadDeadLetterQueryResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 2, QueueId = 1, ReadCount = 5",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                "[DeadLetterQueried] Client = [1] 'test', TraceId = 2, Queue = [1] 'c', TotalCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQuery, Length = 13)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Query_ShouldStartConsumingDeadLetterMessages_WhenReadCountIsGreaterThanZero()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 17 );
            var deadLetterConsumptionContinuation = new SafeTaskCompletionSource();
            var deadLetterQueryStarted = Atomic.Create( false );
            var nextExpirationAt = Atomic.Create( Timestamp.Zero );
            var clientLogs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.DeadLetterQuery )
                                    deadLetterQueryStarted.Value = true;
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                {
                                    if ( deadLetterQueryStarted.Value )
                                        Thread.Sleep( 15 );

                                    endSource.Complete();
                                }
                                else if ( e.Type == MessageBrokerRemoteClientTraceEventType.DeadLetterQuery )
                                {
                                    deadLetterConsumptionContinuation.Complete();
                                    endSource.Complete();
                                }
                            },
                            deadLetterQueried: e => nextExpirationAt.Value = e.NextExpirationAt ) ) )
                    .SetQueueLoggerFactory( _ => MessageBrokerQueueLogger.Create(
                        traceStart: e =>
                        {
                            if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage && deadLetterQueryStarted.Value )
                                deadLetterConsumptionContinuation.Task.Wait();
                        },
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                endSource.Complete();
                        } ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest(
                    "c",
                    false,
                    deadLetterCapacityHint: 10,
                    minDeadLetterRetention: Duration.FromHours( 1 ),
                    minAckTimeout: Duration.FromMinutes( 1 ) );

                c.ReadListenerBoundResponse();
            } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await client.GetTask( c =>
            {
                c.SendPushMessage( 1, [ 1 ], confirm: false );
                c.SendPushMessage( 1, [ 2, 3 ], confirm: false );
                c.SendPushMessage( 1, [ 4, 5, 6 ], confirm: false );
                c.SendPushMessage( 1, [ 7, 8, 9, 10 ], confirm: false );
                c.SendPushMessage( 1, [ 11, 12, 13, 14, 15 ], confirm: false );
                c.ReadMessageNotification( 1 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 0 );
                c.ReadMessageNotification( 2 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 1 );
                c.ReadMessageNotification( 3 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 2 );
                c.ReadMessageNotification( 4 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 3 );
                c.ReadMessageNotification( 5 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 4 );
                c.SendDeadLetterQuery( 1, 3 );
                c.ReadDeadLetterQueryResponse();
                c.ReadMessageNotification( 1 );
                c.ReadMessageNotification( 2 );
                c.ReadMessageNotification( 3 );
            } );

            await endSource.Task;

            Assertion.All(
                    queue.TestNotNull( q => q.Messages.DeadLetter.Count.TestEquals( 2 ) ),
                    clientLogs.GetAll()
                        .Skip( 18 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 18 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 18, Packet = (DeadLetterQuery, Length = 13)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 18, QueueId = 1, ReadCount = 3",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 18, Packet = (DeadLetterQuery, Length = 13)",
                                $"[DeadLetterQueried] Client = [1] 'test', TraceId = 18, Queue = [1] 'c', TotalCount = 5, MaxReadCount = 3, NextExpirationAt = {nextExpirationAt.Value}",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 18, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 18, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 18 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 19 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 19, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = <dead-letter>, MessageId = 0, Retry = 0, Redelivery = 0, Length = 1",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 19, Packet = (MessageNotification, Length = 46)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 19, Packet = (MessageNotification, Length = 46)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 19, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = <dead-letter>, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 19 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 20 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 20, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = <dead-letter>, MessageId = 1, Retry = 0, Redelivery = 0, Length = 2",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 20, Packet = (MessageNotification, Length = 47)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 20, Packet = (MessageNotification, Length = 47)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 20, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = <dead-letter>, MessageId = 1, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 20 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 21 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 21, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = <dead-letter>, MessageId = 2, Retry = 0, Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 21, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 21, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 21, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = <dead-letter>, MessageId = 2, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 21 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQuery, Length = 13)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Query_ShouldStartDiscardingDeadLetterMessages_WhenListenerIsDisposed()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 10 );
            var deadLetterConsumptionContinuation = new SafeTaskCompletionSource();
            var deadLetterQueryStarted = Atomic.Create( false );
            var nextExpirationAt = Atomic.Create( Timestamp.Zero );
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.DeadLetterQuery )
                                    deadLetterQueryStarted.Value = true;
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                    endSource.Complete();
                                else if ( e.Type == MessageBrokerRemoteClientTraceEventType.DeadLetterQuery )
                                {
                                    deadLetterConsumptionContinuation.Complete();
                                    endSource.Complete();
                                }
                            },
                            deadLetterQueried: e => nextExpirationAt.Value = e.NextExpirationAt ) ) )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage && deadLetterQueryStarted.Value )
                                    deadLetterConsumptionContinuation.Task.Wait();
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindPublisherRequest( "d", streamName: "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest(
                    "c",
                    false,
                    prefetchHint: 2,
                    deadLetterCapacityHint: 10,
                    minDeadLetterRetention: Duration.FromHours( 1 ),
                    minAckTimeout: Duration.FromMinutes( 1 ) );

                c.ReadListenerBoundResponse();
                c.SendBindListenerRequest(
                    "d",
                    false,
                    queueName: "c",
                    deadLetterCapacityHint: 10,
                    minDeadLetterRetention: Duration.FromHours( 1 ),
                    minAckTimeout: Duration.FromMinutes( 1 ) );

                c.ReadListenerBoundResponse();
            } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await client.GetTask( c =>
            {
                c.SendPushMessage( 1, [ 1 ], confirm: false );
                c.SendPushMessage( 2, [ 2, 3 ], confirm: false );
                c.SendPushMessage( 1, [ 4, 5, 6 ], confirm: false );
                c.ReadMessageNotification( 1 );
                c.ReadMessageNotification( 2 );
                c.ReadMessageNotification( 3 );
                c.SendMessageNotificationNegativeAck( 1, 1, 1, 0 );
                c.SendMessageNotificationNegativeAck( 1, 2, 1, 1 );
                c.SendMessageNotificationNegativeAck( 1, 3, 1, 2 );
                c.SendUnbindListenerRequest( 2 );
                c.ReadListenerUnboundResponse();
                c.SendDeadLetterQuery( 1, 2 );
                c.ReadDeadLetterQueryResponse();
                c.ReadMessageNotification( 1 );
            } );

            await endSource.Task;

            Assertion.All(
                    queue.TestNotNull( q => q.Messages.DeadLetter.Count.TestEquals( 1 ) ),
                    clientLogs.GetAll()
                        .Skip( 15 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 15 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 15, Packet = (DeadLetterQuery, Length = 13)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 15, QueueId = 1, ReadCount = 2",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 15, Packet = (DeadLetterQuery, Length = 13)",
                                $"[DeadLetterQueried] Client = [1] 'test', TraceId = 15, Queue = [1] 'c', TotalCount = 3, MaxReadCount = 2, NextExpirationAt = {nextExpirationAt.Value}",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 15, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 15, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 15 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 16 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 16, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = <dead-letter>, MessageId = 0, Retry = 0, Redelivery = 0, Length = 1",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 16, Packet = (MessageNotification, Length = 46)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 16, Packet = (MessageNotification, Length = 46)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 16, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = <dead-letter>, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 16 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQuery, Length = 13)"
                        ] ),
                    queueLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 13 (start)",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 13, Sender = [1] 'test', Channel = [2] 'd', Stream = [1] 'c', Reason = DisposedDeadLetter, StoreKey = 1, Retry = 0, Redelivery = 0, MessageRemoved = True, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 13 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Query_ShouldDisposeClient_WhenClientSendsInvalidPayload()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.DeadLetterQuery )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask( c =>
            {
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
                c.SendDeadLetterQuery( 1, 0, payload: 7 );
            } );

            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                    server.Clients.Count.TestEquals( 0 ),
                    server.Clients.GetAll().TestEmpty(),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQuery, Length = 12)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid DeadLetterQuery from client [1] 'test'. Encountered 1 error(s):
                                1. Expected header payload to be 8 but found 7.
                                """,
                                "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQuery, Length = 12)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Query_ShouldDisposeClient_WhenClientSendsNonInvalidValues()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.DeadLetterQuery )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask( c =>
            {
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
                c.SendDeadLetterQuery( 0, -1 );
            } );

            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                    server.Clients.Count.TestEquals( 0 ),
                    server.Clients.GetAll().TestEmpty(),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid DeadLetterQuery from client [1] 'test'. Encountered 2 error(s):
                                1. Expected queue ID to be greater than 0 but found 0.
                                2. Expected read count to not be negative but found -1.
                                """,
                                "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQuery, Length = 13)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Query_ShouldBeRejected_WhenQueueDoesNotExist()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.DeadLetterQuery )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );

            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask( c => c.SendDeadLetterQuery( 1, 1 ) );
            await endSource.Task;

            Assertion.All(
                    exception.TestType()
                        .Exact<MessageBrokerRemoteClientException>( exc => Assertion.All( exc.Client.TestRefEquals( remoteClient ) ) ),
                    remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                    server.Clients.Count.TestEquals( 1 ),
                    server.Clients.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( remoteClient ) ] ),
                    clientLogs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 1, QueueId = 1, ReadCount = 1",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Client [1] 'test' could not process a dead letter query for non-existing queue with ID 1.
                                """,
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    clientLogs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQuery, Length = 13)"
                        ] ) )
                .Go();
        }
    }
}
