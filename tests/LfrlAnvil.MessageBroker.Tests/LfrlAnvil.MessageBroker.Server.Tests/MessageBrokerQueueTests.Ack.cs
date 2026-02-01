using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerQueueTests
{
    public class Ack : TestsBase, IClassFixture<SharedResourceFixture>
    {
        private readonly ValueTaskDelaySource _sharedDelaySource;

        public Ack(SharedResourceFixture fixture)
        {
            _sharedDelaySource = fixture.DelaySource;
        }

        [Fact]
        public async Task Ack_ShouldRemoveMessageFromUnackedCollection()
        {
            var endSource = new SafeTaskCompletionSource();
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var messageRemoved = Atomic.Create( false );
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create( ackProcessed: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            var minPushedAt = TimestampProvider.Shared.GetNow();
            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest( "c", false, minAckTimeout: Duration.FromMinutes( 10 ) );
                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                } );

            var remoteClient = server.Clients.TryGetById( 1 );
            var stream = server.Streams.TryGetById( 1 );
            var queue = remoteClient?.Queues.TryGetById( 1 );
            var publisher = remoteClient?.Publishers.TryGetByChannelId( 1 );
            var listener = remoteClient?.Listeners.TryGetByChannelId( 1 );
            var messageCount = stream?.Messages.Count ?? 0;
            var pendingCount = queue?.Messages.Pending.Count ?? 0;
            var unackedCount = queue?.Messages.Unacked.Count ?? 0;
            var unackedMessage = queue?.Messages.Unacked.TryGetByAckId( 1 );
            var message = unackedMessage?.TryGetMessage();

            await client.GetTask( c => c.SendMessageNotificationAck( 1, 1, 1, 0 ) );
            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 0 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    messageCount.TestEquals( 1 ),
                    pendingCount.TestEquals( 0 ),
                    unackedCount.TestEquals( 1 ),
                    message.TestNotNull(
                        m => Assertion.All(
                            "message",
                            m.Publisher.TestRefEquals( publisher ),
                            m.Length.TestEquals( MemorySize.FromBytes( 3 ) ),
                            m.Id.TestEquals( 0UL ),
                            m.PushedAt.TestGreaterThanOrEqualTo( minPushedAt ),
                            m.StoreKey.TestEquals( 0 ),
                            m.RefCount.TestInRange( 1, 2 ),
                            m.ToString()
                                .TestEquals(
                                    $"Publisher = ([1] 'test' => [1] 'c' publisher binding (using [1] 'c' stream) (Running)), Length = 3 B, Id = 0, PushedAt = {m.PushedAt}, StoreKey = 0, RefCount = {m.RefCount}" ) ) ),
                    unackedMessage.TestNotNull(
                        m =>
                            Assertion.All(
                                "unackedMessage",
                                m.Publisher.TestRefEquals( publisher ),
                                m.Listener.TestRefEquals( listener ),
                                m.StoreKey.TestEquals( 0 ),
                                m.MessageId.TestEquals( 0UL ),
                                m.Retry.TestEquals( 0 ),
                                m.Redelivery.TestEquals( 0 ),
                                m.ExpiresAt.TestGreaterThanOrEqualTo( minPushedAt + listener?.MinAckTimeout ?? Timestamp.Zero ),
                                m.ToString()
                                    .TestEquals(
                                        $"Publisher = ([1] 'test' => [1] 'c' publisher binding (using [1] 'c' stream) (Running)), Listener = ([1] 'test' => [1] 'c' listener binding (using [1] 'c' queue) (Running)), StoreKey = 0, MessageId = 0, Retry = 0, Redelivery = 0, ExpiresAt = {m.ExpiresAt}" ) ) ),
                    clientLogs.GetAll()
                        .Skip( 5 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', TraceId = 5 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 5, Packet = (MessageNotificationAck, Length = 33)",
                                "[ProcessingAck] Client = [1] 'test', TraceId = 5, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 5, Packet = (MessageNotificationAck, Length = 33)",
                                "[AckProcessed] Client = [1] 'test', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, IsNack = False",
                                "[Trace:Ack] Client = [1] 'test', TraceId = 5 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Skip( 3 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, ClientTraceId = 5",
                                $"[AckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, AckId = 1, MessageRemoved = {messageRemoved.Value}",
                                "[Trace:Ack] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Ack_ShouldDecrementListenerPrefetchCounterAndCauseNextMessageToBeSent()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var firstAckTraceStarted = Atomic.Create( false );
            var clientTraceIdsByQueueTraceId = new ConcurrentDictionary<ulong, ulong>();
            var messageRemovedByQueueTraceId = new ConcurrentDictionary<ulong, bool>();
            var clientLogs = new ClientEventLogger();
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceStart: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.Ack )
                                        firstAckTraceStarted.Value = true;
                                },
                                clientTrace: e =>
                                {
                                    if ( firstAckTraceStarted.Value )
                                        clientTraceIdsByQueueTraceId[e.Source.TraceId] = e.ClientTraceId;
                                },
                                ackProcessed: e => messageRemovedByQueueTraceId[e.Source.TraceId] = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest( "c", false, minAckTimeout: Duration.FromMinutes( 10 ) );
                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2 ], confirm: false );
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 2 );
                    c.SendMessageNotificationAck( 1, 1, 1, 0 );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationAck( 1, 1, 1, 1 );
                } );

            await endSource.Task;

            Assertion.All(
                    clientLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:Ack]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:Ack] Client = [1] 'test', TraceId = {t.Id} (start)",
                                $"[ReadPacket:Received] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotificationAck, Length = 33)",
                                $"[ProcessingAck] Client = [1] 'test', TraceId = {t.Id}, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                $"[ReadPacket:Accepted] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotificationAck, Length = 33)",
                                $"[AckProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, IsNack = False",
                                $"[Trace:Ack] Client = [1] 'test', TraceId = {t.Id} (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', TraceId = 8 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 8, Packet = (MessageNotificationAck, Length = 33)",
                                "[ProcessingAck] Client = [1] 'test', TraceId = 8, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 8, Packet = (MessageNotificationAck, Length = 33)",
                                "[AckProcessed] Client = [1] 'test', TraceId = 8, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 1, Retry = 0, Redelivery = 0, IsNack = False",
                                "[Trace:Ack] Client = [1] 'test', TraceId = 8 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:Ack]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:Ack] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (start)",
                                $"[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, ClientTraceId = {clientTraceIdsByQueueTraceId.GetValueOrDefault( t.Id )}",
                                $"[AckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, AckId = 1, MessageRemoved = {messageRemovedByQueueTraceId.GetValueOrDefault( t.Id )}",
                                $"[Trace:Ack] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', Queue = [1] 'c', TraceId = 6 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 6, ClientTraceId = 8",
                                $"[AckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 6, AckId = 1, MessageRemoved = {messageRemovedByQueueTraceId.GetValueOrDefault( 6UL )}",
                                "[Trace:Ack] Client = [1] 'test', Queue = [1] 'c', TraceId = 6 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Ack_ShouldAutomaticallyDisposeQueueAndStream_WhenNoLongerReferencedAndWithoutMessages()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var ackContinuation = new SafeTaskCompletionSource( completionCount: 2 );
            var streamLogs = new StreamEventLogger();
            var queueLogs = new QueueEventLogger();
            var messageRemoved = Atomic.Create( false );
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => MessageBrokerRemoteClientLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                    ackContinuation.Task.Wait();
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                    endSource.Complete();
                                else if ( e.Type is MessageBrokerRemoteClientTraceEventType.UnbindListener
                                    or MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                    ackContinuation.Complete();
                            } ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create( ackProcessed: e => messageRemoved.Value = e.MessageRemoved ) ) )
                    .SetStreamLoggerFactory(
                        _ => streamLogs.GetLogger(
                            MessageBrokerStreamLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerStreamTraceEventType.Dispose )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest( "c", false, minAckTimeout: Duration.FromMinutes( 10 ) );
                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ ], confirm: false );
                    c.ReadMessageNotification( 0 );
                    c.SendUnbindPublisherRequest( 1 );
                    c.ReadPublisherUnboundResponse();
                    c.SendUnbindListenerRequest( 1 );
                    c.ReadListenerUnboundResponse();
                    c.SendMessageNotificationAck( 1, 1, 1, 0 );
                } );

            await endSource.Task;

            Assertion.All(
                    streamLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Dispose] Stream = [1] 'c', TraceId = 4 (start)",
                                "[Disposing] Stream = [1] 'c', TraceId = 4",
                                "[Disposed] Stream = [1] 'c', TraceId = 4",
                                "[Trace:Dispose] Stream = [1] 'c', TraceId = 4 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, ClientTraceId = 7",
                                $"[AckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, AckId = 1, MessageRemoved = {messageRemoved.Value}",
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, IsAlive = False",
                                "[Trace:Ack] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Ack_ShouldLogLackOfMessageInStreamStore()
        {
            Exception? exception = null;
            MessageBrokerStream? stream = null;
            var endSource = new SafeTaskCompletionSource();
            var streamEndSource = new SafeTaskCompletionSource();
            var queueLogs = new QueueEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => MessageBrokerRemoteClientLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                    streamEndSource.Task.Wait();
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                    endSource.Complete();
                            } ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceStart: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.Ack && stream is not null )
                                    {
                                        using ( stream.AcquireLock() )
                                            stream.MessageStore.DecrementRefCount( 0, out var _ );
                                    }
                                },
                                error: e => exception = e.Exception ) ) )
                    .SetStreamLoggerFactory(
                        _ => MessageBrokerStreamLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerStreamTraceEventType.ProcessMessage )
                                    streamEndSource.Complete();
                            } ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest( "c", false, minAckTimeout: Duration.FromMinutes( 10 ) );
                    c.ReadListenerBoundResponse();
                } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            stream = server.Streams.TryGetById( 1 );

            await client.GetTask(
                c =>
                {
                    c.SendPushMessage( 1, [ ], confirm: false );
                    c.ReadMessageNotification( 0 );
                    c.SendMessageNotificationAck( 1, 1, 1, 0 );
                } );

            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<MessageBrokerQueueException>( e => e.Queue.TestRefEquals( queue ) ),
                    queueLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, ClientTraceId = 5",
                                """
                                [Error] Client = [1] 'test', Queue = [1] 'c', TraceId = 3
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerQueueException: Stream [1] 'c' does not have a message related to store key 0.
                                """,
                                "[AckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, AckId = 1, MessageRemoved = False",
                                "[Trace:Ack] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Ack_ShouldBeRejected_WhenQueueDoesNotExist()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask( c => c.SendMessageNotificationAck( 1, 2, 3, 4, 5, 6 ) );
            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<MessageBrokerRemoteClientException>( e => e.Client.TestRefEquals( remoteClient ) ),
                    remoteClient.TestNotNull( q => q.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (MessageNotificationAck, Length = 33)",
                                "[ProcessingAck] Client = [1] 'test', TraceId = 1, QueueId = 1, AckId = 2, StreamId = 3, MessageId = 4, Retry = 5, Redelivery = 6",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Client [1] 'test' could not process a message ACK for non-existing queue with ID 1.
                                """,
                                "[Trace:Ack] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Ack_ShouldBeRejected_WhenMessageDoesNotExistInQueue()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindListenerRequest( "c", true, minAckTimeout: Duration.FromMinutes( 10 ) );
                    c.ReadListenerBoundResponse();
                } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );

            await client.GetTask( c => c.SendMessageNotificationAck( 1, 2, 3, 4, 5, 6 ) );
            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<MessageBrokerQueueException>( e => e.Queue.TestRefEquals( queue ) ),
                    queue.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotificationAck, Length = 33)",
                                "[ProcessingAck] Client = [1] 'test', TraceId = 2, QueueId = 1, AckId = 2, StreamId = 3, MessageId = 4, Retry = 5, Redelivery = 6",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerQueueException: Queue [1] 'c' for client [1] 'test' could not process a (ack ID: 2, stream ID: 3, message ID: 4) message ACK because the message does not exist.
                                """,
                                "[Trace:Ack] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Ack_ShouldBeRejected_WhenMessageVersionDoesNotExistInQueue()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest( "c", false, minAckTimeout: Duration.FromMinutes( 10 ) );
                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ ], confirm: false );
                    c.ReadMessageNotification( 0 );
                } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );

            await client.GetTask( c => c.SendMessageNotificationAck( 1, 1, 1, 0, 2, 3 ) );
            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<MessageBrokerQueueException>( e => e.Queue.TestRefEquals( queue ) ),
                    queue.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', TraceId = 5 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 5, Packet = (MessageNotificationAck, Length = 33)",
                                "[ProcessingAck] Client = [1] 'test', TraceId = 5, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 0, Retry = 2, Redelivery = 3",
                                """
                                [Error] Client = [1] 'test', TraceId = 5
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerQueueException: Queue [1] 'c' for client [1] 'test' could not process a (stream ID: 1, message ID: 0) message ACK because its (retry: 2, redelivery: 3) version does not exist.
                                """,
                                "[Trace:Ack] Client = [1] 'test', TraceId = 5 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Ack_ShouldDisposeClient_WhenClientSendsInvalidPayload()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask( c => c.SendMessageNotificationAck( 1, 1, 1, 0, payload: 27 ) );
            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (MessageNotificationAck, Length = 32)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid MessageNotificationAck from client [1] 'test'. Encountered 1 error(s):
                                1. Expected header payload to be 28 but found 27.
                                """,
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:Ack] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Ack_ShouldDisposeClient_WhenClientSendsInvalidRequestData()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ack )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask( c => c.SendMessageNotificationAck( 0, -1, -2, 0, -3, -4 ) );
            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (MessageNotificationAck, Length = 33)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid MessageNotificationAck from client [1] 'test'. Encountered 5 error(s):
                                1. Expected queue ID to be greater than 0 but found 0.
                                2. Expected ACK ID to be greater than 0 but found -1.
                                3. Expected stream ID to be greater than 0 but found -2.
                                4. Expected retry to not be negative but found -3.
                                5. Expected redelivery to not be negative but found -4.
                                """,
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:Ack] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldMoveUnackedMessageToRetries()
        {
            var endSource = new SafeTaskCompletionSource();
            var clientLogs = new ClientEventLogger();
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

            await server.StartAsync();

            var minPushedAt = TimestampProvider.Shared.GetNow();
            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRetries: 1,
                        retryDelay: Duration.FromSeconds( 10 ),
                        minAckTimeout: Duration.FromMinutes( 10 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                } );

            var remoteClient = server.Clients.TryGetById( 1 );
            var stream = server.Streams.TryGetById( 1 );
            var queue = remoteClient?.Queues.TryGetById( 1 );
            var publisher = remoteClient?.Publishers.TryGetByChannelId( 1 );
            var listener = remoteClient?.Listeners.TryGetByChannelId( 1 );

            await client.GetTask( c => c.SendMessageNotificationNegativeAck( 1, 1, 1, 0 ) );
            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 1 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 1 ),
                            q.Messages.Retries.TryGetNext()
                                .TestNotNull(
                                    r => Assertion.All(
                                        "retryMessage",
                                        r.Publisher.TestRefEquals( publisher ),
                                        r.Listener.TestRefEquals( listener ),
                                        r.StoreKey.TestEquals( 0 ),
                                        r.Retry.TestEquals( 1 ),
                                        r.Redelivery.TestEquals( 0 ),
                                        r.SendAt.TestGreaterThanOrEqualTo( minPushedAt + listener?.RetryDelay ?? Timestamp.Zero ),
                                        r.ToString()
                                            .TestEquals(
                                                $"Publisher = ([1] 'test' => [1] 'c' publisher binding (using [1] 'c' stream) (Running)), Listener = ([1] 'test' => [1] 'c' listener binding (using [1] 'c' queue) (Running)), StoreKey = 0, Retry = 1, Redelivery = 0, SendAt = {r.SendAt}" ),
                                        r.TryGetMessage().TestNotNull( m => m.StoreKey.TestEquals( r.StoreKey ) ) ) ) ) ),
                    clientLogs.GetAll()
                        .Skip( 5 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 5 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 5, Packet = (MessageNotificationNack, Length = 38)",
                                "[ProcessingNegativeAck] Client = [1] 'test', TraceId = 5, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0, NoRetry = False, NoDeadLetter = False",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 5, Packet = (MessageNotificationNack, Length = 38)",
                                "[AckProcessed] Client = [1] 'test', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, IsNack = True",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 5 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Skip( 3 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, ClientTraceId = 5",
                                "[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, AckId = 1, Delay = 10 second(s)",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldDecrementListenerPrefetchCounterAndCauseNextMessageToBeSent()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var firstNackTraceStarted = Atomic.Create( false );
            var clientTraceIdsByQueueTraceId = new ConcurrentDictionary<ulong, ulong>();
            var clientLogs = new ClientEventLogger();
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceStart: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.NegativeAck )
                                        firstNackTraceStarted.Value = true;
                                },
                                clientTrace: e =>
                                {
                                    if ( firstNackTraceStarted.Value )
                                        clientTraceIdsByQueueTraceId[e.Source.TraceId] = e.ClientTraceId;
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRetries: 1,
                        retryDelay: Duration.FromSeconds( 10 ),
                        minAckTimeout: Duration.FromMinutes( 10 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2 ], confirm: false );
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 2 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0 );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 1 );
                } );

            await endSource.Task;

            Assertion.All(
                    clientLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:NegativeAck]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:NegativeAck] Client = [1] 'test', TraceId = {t.Id} (start)",
                                $"[ReadPacket:Received] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotificationNack, Length = 38)",
                                $"[ProcessingNegativeAck] Client = [1] 'test', TraceId = {t.Id}, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0, NoRetry = False, NoDeadLetter = False",
                                $"[ReadPacket:Accepted] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotificationNack, Length = 38)",
                                $"[AckProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, IsNack = True",
                                $"[Trace:NegativeAck] Client = [1] 'test', TraceId = {t.Id} (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 8 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 8, Packet = (MessageNotificationNack, Length = 38)",
                                "[ProcessingNegativeAck] Client = [1] 'test', TraceId = 8, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, NoRetry = False, NoDeadLetter = False",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 8, Packet = (MessageNotificationNack, Length = 38)",
                                "[AckProcessed] Client = [1] 'test', TraceId = 8, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 1, Retry = 0, Redelivery = 0, IsNack = True",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 8 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:NegativeAck]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (start)",
                                $"[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, ClientTraceId = {clientTraceIdsByQueueTraceId.GetValueOrDefault( t.Id )}",
                                $"[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, AckId = 1, Delay = 10 second(s)",
                                $"[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 6 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 6, ClientTraceId = 8",
                                "[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 6, AckId = 1, Delay = 10 second(s)",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 6 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldDecrementListenerPrefetchCounterAndCauseNextMessageToBeSent_WithNoRetry()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var firstNackTraceStarted = Atomic.Create( false );
            var clientTraceIdsByQueueTraceId = new ConcurrentDictionary<ulong, ulong>();
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var messageRemoved = Atomic.Create( false );
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceStart: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.NegativeAck )
                                        firstNackTraceStarted.Value = true;
                                },
                                clientTrace: e =>
                                {
                                    if ( firstNackTraceStarted.Value )
                                        clientTraceIdsByQueueTraceId[e.Source.TraceId] = e.ClientTraceId;
                                },
                                messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRetries: 1,
                        retryDelay: Duration.FromSeconds( 10 ),
                        minAckTimeout: Duration.FromMinutes( 10 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2 ], confirm: false );
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 2 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, noRetry: true );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 1 );
                } );

            await endSource.Task;

            Assertion.All(
                    clientLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:NegativeAck]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:NegativeAck] Client = [1] 'test', TraceId = {t.Id} (start)",
                                $"[ReadPacket:Received] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotificationNack, Length = 38)",
                                $"[ProcessingNegativeAck] Client = [1] 'test', TraceId = {t.Id}, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0, NoRetry = True, NoDeadLetter = False",
                                $"[ReadPacket:Accepted] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotificationNack, Length = 38)",
                                $"[AckProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, IsNack = True",
                                $"[Trace:NegativeAck] Client = [1] 'test', TraceId = {t.Id} (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 8 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 8, Packet = (MessageNotificationNack, Length = 38)",
                                "[ProcessingNegativeAck] Client = [1] 'test', TraceId = 8, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, NoRetry = False, NoDeadLetter = False",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 8, Packet = (MessageNotificationNack, Length = 38)",
                                "[AckProcessed] Client = [1] 'test', TraceId = 8, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 1, Retry = 0, Redelivery = 0, IsNack = True",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 8 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:NegativeAck]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (start)",
                                $"[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, ClientTraceId = {clientTraceIdsByQueueTraceId.GetValueOrDefault( t.Id )}",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = ExplicitNoRetry, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                $"[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, AckId = 1, MessageRemoved = {messageRemoved.Value}",
                                $"[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 6 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 6, ClientTraceId = 8",
                                "[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 6, AckId = 1, Delay = 10 second(s)",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 6 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldNotAddMessageToDeadLetterOnLastAttempt_WhenExplicitlySpecified()
        {
            var endSource = new SafeTaskCompletionSource();
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var messageRemoved = Atomic.Create( false );
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create( messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        deadLetterCapacityHint: 1,
                        minDeadLetterRetention: Duration.FromHours( 1 ),
                        minAckTimeout: Duration.FromMinutes( 10 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1 ], confirm: false );
                    c.ReadMessageNotification( 1 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, noDeadLetter: true );
                } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await endSource.Task;

            Assertion.All(
                    queue.TestNotNull( q => q.Messages.DeadLetter.Count.TestEquals( 0 ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 5 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 5, Packet = (MessageNotificationNack, Length = 38)",
                                "[ProcessingNegativeAck] Client = [1] 'test', TraceId = 5, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0, NoRetry = False, NoDeadLetter = True",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 5, Packet = (MessageNotificationNack, Length = 38)",
                                "[AckProcessed] Client = [1] 'test', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, IsNack = True",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 5 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, ClientTraceId = 5",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = MaxRetriesReached, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                $"[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, AckId = 1, MessageRemoved = {messageRemoved.Value}",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldAutomaticallyDisposeQueueAndStream_WhenNoLongerReferencedAndWithoutMessages()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var ackContinuation = new SafeTaskCompletionSource( completionCount: 2 );
            var streamLogs = new StreamEventLogger();
            var queueLogs = new QueueEventLogger();
            var messageRemoved = Atomic.Create( false );
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => MessageBrokerRemoteClientLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                    ackContinuation.Task.Wait();
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                    endSource.Complete();
                                else if ( e.Type is MessageBrokerRemoteClientTraceEventType.UnbindListener
                                    or MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                    ackContinuation.Complete();
                            } ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create( messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) )
                    .SetStreamLoggerFactory(
                        _ => streamLogs.GetLogger(
                            MessageBrokerStreamLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerStreamTraceEventType.Dispose )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest( "c", false, minAckTimeout: Duration.FromMinutes( 10 ) );
                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ ], confirm: false );
                    c.ReadMessageNotification( 0 );
                    c.SendUnbindPublisherRequest( 1 );
                    c.ReadPublisherUnboundResponse();
                    c.SendUnbindListenerRequest( 1 );
                    c.ReadListenerUnboundResponse();
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0 );
                } );

            await endSource.Task;

            Assertion.All(
                    streamLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Dispose] Stream = [1] 'c', TraceId = 4 (start)",
                                "[Disposing] Stream = [1] 'c', TraceId = 4",
                                "[Disposed] Stream = [1] 'c', TraceId = 4",
                                "[Trace:Dispose] Stream = [1] 'c', TraceId = 4 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, ClientTraceId = 7",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = MaxRetriesReached, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                $"[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, AckId = 1, MessageRemoved = {messageRemoved.Value}",
                                "[Deactivating] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, IsAlive = False",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldBeRejected_WhenQueueDoesNotExist()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask( c => c.SendMessageNotificationNegativeAck( 1, 2, 3, 4, 5, 6 ) );
            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<MessageBrokerRemoteClientException>( e => e.Client.TestRefEquals( remoteClient ) ),
                    remoteClient.TestNotNull( q => q.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (MessageNotificationNack, Length = 38)",
                                "[ProcessingNegativeAck] Client = [1] 'test', TraceId = 1, QueueId = 1, AckId = 2, StreamId = 3, MessageId = 4, Retry = 5, Redelivery = 6, NoRetry = False, NoDeadLetter = False",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: Client [1] 'test' could not process a message ACK for non-existing queue with ID 1.
                                """,
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldBeRejected_WhenMessageDoesNotExistInQueue()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindListenerRequest( "c", true, minAckTimeout: Duration.FromMinutes( 10 ) );
                    c.ReadListenerBoundResponse();
                } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );

            await client.GetTask( c => c.SendMessageNotificationNegativeAck( 1, 2, 3, 4, 5, 6 ) );
            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<MessageBrokerQueueException>( e => e.Queue.TestRefEquals( queue ) ),
                    queue.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageNotificationNack, Length = 38)",
                                "[ProcessingNegativeAck] Client = [1] 'test', TraceId = 2, QueueId = 1, AckId = 2, StreamId = 3, MessageId = 4, Retry = 5, Redelivery = 6, NoRetry = False, NoDeadLetter = False",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerQueueException: Queue [1] 'c' for client [1] 'test' could not process a (ack ID: 2, stream ID: 3, message ID: 4) message ACK because the message does not exist.
                                """,
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldBeRejected_WhenMessageVersionDoesNotExistInQueue()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest( "c", false, minAckTimeout: Duration.FromMinutes( 10 ) );
                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ ], confirm: false );
                    c.ReadMessageNotification( 0 );
                } );

            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );

            await client.GetTask( c => c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, 2, 3 ) );
            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<MessageBrokerQueueException>( e => e.Queue.TestRefEquals( queue ) ),
                    queue.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 5 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 5, Packet = (MessageNotificationNack, Length = 38)",
                                "[ProcessingNegativeAck] Client = [1] 'test', TraceId = 5, QueueId = 1, AckId = 1, StreamId = 1, MessageId = 0, Retry = 2, Redelivery = 3, NoRetry = False, NoDeadLetter = False",
                                """
                                [Error] Client = [1] 'test', TraceId = 5
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerQueueException: Queue [1] 'c' for client [1] 'test' could not process a (stream ID: 1, message ID: 0) message ACK because its (retry: 2, redelivery: 3) version does not exist.
                                """,
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 5 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldDisposeClient_WhenClientSendsInvalidPayload()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask( c => c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, payload: 32 ) );
            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (MessageNotificationNack, Length = 37)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid MessageNotificationNack from client [1] 'test'. Encountered 1 error(s):
                                1. Expected header payload to be 33 but found 32.
                                """,
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldDisposeClient_WhenClientSendsInvalidRequestData()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask(
                c => c.SendMessageNotificationNegativeAck(
                    0,
                    -1,
                    -2,
                    0,
                    -3,
                    -4,
                    hasExplicitDelay: true,
                    explicitDelay: Duration.FromMilliseconds( -1 ) ) );

            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (MessageNotificationNack, Length = 38)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid MessageNotificationNack from client [1] 'test'. Encountered 6 error(s):
                                1. Expected queue ID to be greater than 0 but found 0.
                                2. Expected ACK ID to be greater than 0 but found -1.
                                3. Expected stream ID to be greater than 0 but found -2.
                                4. Expected retry to not be negative but found -3.
                                5. Expected redelivery to not be negative but found -4.
                                6. Expected explicit delay to not be negative but found -0.001 second(s).
                                """,
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task NegativeAck_ShouldDisposeClient_WhenClientSendsInvalidPositiveExplicitDelay()
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var remoteClient = server.Clients.TryGetById( 1 );
            await client.GetTask(
                c => c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, noRetry: true, explicitDelay: Duration.FromMilliseconds( 1 ) ) );

            await endSource.Task;

            Assertion.All(
                    remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                    clientLogs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (MessageNotificationNack, Length = 38)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid MessageNotificationNack from client [1] 'test'. Encountered 1 error(s):
                                1. Expected disabled explicit delay to be equal to 0 found 0.001 second(s).
                                """,
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task AckTimeout_ShouldSendMessageRedeliveryUntilListenerLimit()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 7 );
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var ackExpireDatesByQueueTraceId = new ConcurrentDictionary<ulong, Timestamp>();
            var messageRemoved = Atomic.Create( false );
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                        endSource.Complete();
                                },
                                messageProcessed: e => ackExpireDatesByQueueTraceId[e.Source.TraceId] = e.AckExpiresAt,
                                messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRedeliveries: 2,
                        minAckTimeout: Duration.FromMilliseconds( 15 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                    c.ReadMessageNotification( 3 );
                    c.ReadMessageNotification( 3 );
                } );

            var stream = server.Streams.TryGetById( 1 );
            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 0 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    clientLogs.GetAll()
                        .Skip( 4 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 5 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 2, MessageId = 0, Retry = 0, Redelivery = 1 (active), Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 5, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 2, MessageId = 0, Retry = 0, Redelivery = 1 (active)",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 5 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 6 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 2 (active), Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 2 (active)",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 6 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 2UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 1 (active), IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 2, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 3UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 2 (active), IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 4UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (start)",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = MaxRedeliveriesReached, StoreKey = 0, Retry = 0, Redelivery = 2, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task AckTimeout_ShouldSendMessageRedeliveryUntilListenerLimit_WithDeadLetter()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 5 );
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var ackExpireDatesByQueueTraceId = new ConcurrentDictionary<ulong, Timestamp>();
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
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                        endSource.Complete();
                                },
                                messageProcessed: e => ackExpireDatesByQueueTraceId[e.Source.TraceId] = e.AckExpiresAt ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            var minPushedAt = TimestampProvider.Shared.GetNow();
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRedeliveries: 1,
                        deadLetterCapacityHint: 1,
                        minDeadLetterRetention: Duration.FromMinutes( 1 ),
                        minAckTimeout: Duration.FromMilliseconds( 15 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                    c.ReadMessageNotification( 3 );
                } );

            var stream = server.Streams.TryGetById( 1 );
            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            var publisher = stream?.Publishers.TryGetByKey( 1, 1 );
            var listener = queue?.Listeners.TryGetByChannelId( 1 );
            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 1 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 1 ),
                            q.Messages.DeadLetter.TryPeekAt( 0 )
                                .TestNotNull(
                                    d => Assertion.All(
                                        "deadLetterMessage",
                                        d.Listener.TestRefEquals( listener ),
                                        d.Publisher.TestRefEquals( publisher ),
                                        d.StoreKey.TestEquals( 0 ),
                                        d.Retry.TestEquals( 0 ),
                                        d.Redelivery.TestEquals( 1 ),
                                        d.ExpiresAt.TestGreaterThanOrEqualTo(
                                            minPushedAt + listener?.MinDeadLetterRetention ?? Timestamp.Zero ),
                                        d.ToString()
                                            .TestEquals(
                                                $"Publisher = ([1] 'test' => [1] 'c' publisher binding (using [1] 'c' stream) (Running)), Listener = ([1] 'test' => [1] 'c' listener binding (using [1] 'c' queue) (Running)), StoreKey = 0, Retry = 0, Redelivery = 1, ExpiresAt = {d.ExpiresAt}" ),
                                        d.TryGetMessage().TestNotNull( m => m.StoreKey.TestEquals( d.StoreKey ) ) ) ) ) ),
                    clientLogs.GetAll()
                        .Skip( 4 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 5 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 2, MessageId = 0, Retry = 0, Redelivery = 1 (active), Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 5, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 2, MessageId = 0, Retry = 0, Redelivery = 1 (active)",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 5 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 2UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 1 (active), IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 2, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 3UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (start)",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = MaxRedeliveriesReached, StoreKey = 0, Retry = 0, Redelivery = 1, MessageRemoved = False, MovedToDeadLetter = True",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task AckTimeout_ShouldIgnoreMessagesTargetedToDisposedListeners()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 5 );
            var listenerUnbound = new SafeTaskCompletionSource();
            var firstProcess = new SafeTaskCompletionSource();
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var ackExpireDatesByQueueTraceId = new ConcurrentDictionary<ulong, Timestamp>();
            var firstProcessComplete = Atomic.Create( false );
            var messageRemoved = Atomic.Create( false );
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
                                    {
                                        listenerUnbound.Complete();
                                        endSource.Complete();
                                    }
                                    else if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                    {
                                        if ( ! firstProcessComplete.Value )
                                        {
                                            firstProcessComplete.Value = true;
                                            firstProcess.Complete();
                                        }

                                        endSource.Complete();
                                        listenerUnbound.Task.Wait();
                                    }
                                    else if ( e.Type == MessageBrokerQueueTraceEventType.UnbindListener )
                                        endSource.Complete();
                                },
                                messageProcessed: e => ackExpireDatesByQueueTraceId[e.Source.TraceId] = e.AckExpiresAt,
                                messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRedeliveries: 2,
                        minAckTimeout: Duration.FromMilliseconds( 15 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                } );

            await firstProcess.Task;
            var stream = server.Streams.TryGetById( 1 );
            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await client.GetTask(
                c =>
                {
                    c.SendUnbindListenerRequest( 1 );
                    c.ReadListenerUnboundResponse();
                } );

            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 0 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    clientLogs.GetAll()
                        .Skip( 4 )
                        .Take( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Skip( 2 )
                        .SkipLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 2UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, ClientTraceId = 5",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, Channel = [1] 'c'",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (start)",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = DisposedUnacked, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Retry_ShouldScheduleMessageRetryUntilListenerLimit()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 9 );
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var ackExpireDatesByQueueTraceId = new ConcurrentDictionary<ulong, Timestamp>();
            var messageRemoved = Atomic.Create( false );
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
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.NegativeAck
                                        or MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                        endSource.Complete();
                                },
                                messageProcessed: e => ackExpireDatesByQueueTraceId[e.Source.TraceId] = e.AckExpiresAt,
                                messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRetries: 2,
                        retryDelay: Duration.FromMilliseconds( 15 ),
                        minAckTimeout: Duration.FromMinutes( 10 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0 );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, 1 );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, 2 );
                } );

            var stream = server.Streams.TryGetById( 1 );
            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 0 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    clientLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:MessageNotification]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 6 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 1 (active), Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 1 (active), Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 6 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 8 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 8, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 2 (active), Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 8, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 8, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 8, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 2 (active), Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 8 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:ProcessMessage]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 2UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 1 (active), Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 4UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 6 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 2 (active), Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 6UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 6 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:NegativeAck]" ) ) )
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 7 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 7, ClientTraceId = 9",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 7, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = MaxRetriesReached, StoreKey = 0, Retry = 2, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                $"[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 7, AckId = 1, MessageRemoved = {messageRemoved.Value}",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 7 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Retry_ShouldScheduleMessageRetryUntilListenerLimit_WithExplicitDelay()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 9 );
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var ackExpireDatesByQueueTraceId = new ConcurrentDictionary<ulong, Timestamp>();
            var messageRemoved = Atomic.Create( false );
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
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.NegativeAck
                                        or MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                        endSource.Complete();
                                },
                                messageProcessed: e => ackExpireDatesByQueueTraceId[e.Source.TraceId] = e.AckExpiresAt,
                                messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRetries: 2,
                        retryDelay: Duration.FromSeconds( 10 ),
                        minAckTimeout: Duration.FromMinutes( 10 ) );

                    var delay = Duration.Zero;
                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, hasExplicitDelay: true, explicitDelay: delay );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, 1, hasExplicitDelay: true, explicitDelay: delay );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, 2, hasExplicitDelay: true, explicitDelay: delay );
                } );

            var stream = server.Streams.TryGetById( 1 );
            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 0 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    clientLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:MessageNotification]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 6 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 1 (active), Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 1 (active), Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 6 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 8 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 8, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 2 (active), Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 8, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 8, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 8, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 2 (active), Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 8 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:ProcessMessage]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 2UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 1 (active), Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 4UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 6 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 2 (active), Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 6UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 6 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:NegativeAck]" ) ) )
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 7 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 7, ClientTraceId = 9",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 7, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = MaxRetriesReached, StoreKey = 0, Retry = 2, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                $"[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 7, AckId = 1, MessageRemoved = {messageRemoved.Value}",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 7 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Retry_ShouldScheduleMessageRetryUntilListenerLimit_WithDeadLetter()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 6 );
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var ackExpireDatesByQueueTraceId = new ConcurrentDictionary<ulong, Timestamp>();
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
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.NegativeAck
                                        or MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                        endSource.Complete();
                                },
                                messageProcessed: e => ackExpireDatesByQueueTraceId[e.Source.TraceId] = e.AckExpiresAt ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRetries: 1,
                        retryDelay: Duration.FromMilliseconds( 15 ),
                        deadLetterCapacityHint: 1,
                        minDeadLetterRetention: Duration.FromMinutes( 1 ),
                        minAckTimeout: Duration.FromMinutes( 10 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0 );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, 1 );
                } );

            var stream = server.Streams.TryGetById( 1 );
            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 1 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 1 ) ) ),
                    clientLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:MessageNotification]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 6 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 1 (active), Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 6, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 1 (active), Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 6 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:ProcessMessage]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 2UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 1 (active), Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 4UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:NegativeAck]" ) ) )
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 5, ClientTraceId = 7",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = MaxRetriesReached, StoreKey = 0, Retry = 1, Redelivery = 0, MessageRemoved = False, MovedToDeadLetter = True",
                                "[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 5, AckId = 1, MessageRemoved = False",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Retry_ShouldNotBeScheduled_WhenNegativeAckContainsNoRetryFlag()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 3 );
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var ackExpireDatesByQueueTraceId = new ConcurrentDictionary<ulong, Timestamp>();
            var messageRemoved = Atomic.Create( false );
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
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.NegativeAck
                                        or MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                        endSource.Complete();
                                },
                                messageProcessed: e => ackExpireDatesByQueueTraceId[e.Source.TraceId] = e.AckExpiresAt,
                                messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRetries: 2,
                        retryDelay: Duration.FromMilliseconds( 15 ),
                        minAckTimeout: Duration.FromMinutes( 10 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, noRetry: true );
                } );

            var stream = server.Streams.TryGetById( 1 );
            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestInRange( 0, 1 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    clientLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:MessageNotification]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 2UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, ClientTraceId = 5",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = ExplicitNoRetry, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                $"[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, AckId = 1, MessageRemoved = {messageRemoved.Value}",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Retry_ShouldNotBeScheduled_WhenNegativeAckContainsNoRetryFlag_WithDeadLetter()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 3 );
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var ackExpireDatesByQueueTraceId = new ConcurrentDictionary<ulong, Timestamp>();
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
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.NegativeAck
                                        or MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                        endSource.Complete();
                                },
                                messageProcessed: e => ackExpireDatesByQueueTraceId[e.Source.TraceId] = e.AckExpiresAt ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRetries: 2,
                        retryDelay: Duration.FromMilliseconds( 15 ),
                        deadLetterCapacityHint: 1,
                        minDeadLetterRetention: Duration.FromMinutes( 1 ),
                        minAckTimeout: Duration.FromMinutes( 10 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, noRetry: true );
                } );

            var stream = server.Streams.TryGetById( 1 );
            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 1 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 1 ) ) ),
                    clientLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:MessageNotification]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 2UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, ClientTraceId = 5",
                                "[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = ExplicitNoRetry, StoreKey = 0, Retry = 0, Redelivery = 0, MessageRemoved = False, MovedToDeadLetter = True",
                                "[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, AckId = 1, MessageRemoved = False",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Retry_ShouldNotBeInvokedImmediately_WhenListenerPrefetchCounterCannotBeIncremented()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 8 );
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var clientTraceIdsByQueueTraceId = new ConcurrentDictionary<ulong, ulong>();
            var ackExpireDatesByQueueTraceId = new ConcurrentDictionary<ulong, Timestamp>();
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
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.Ack
                                        or MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                        endSource.Complete();
                                },
                                messageProcessed: e => ackExpireDatesByQueueTraceId[e.Source.TraceId] = e.AckExpiresAt,
                                clientTrace: e => clientTraceIdsByQueueTraceId[e.Source.TraceId] = e.ClientTraceId ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRetries: 2,
                        retryDelay: Duration.FromMilliseconds( 50 ),
                        minAckTimeout: Duration.FromMinutes( 10 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1 ], confirm: false );
                    c.ReadMessageNotification( 1 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0 );
                    c.SendPushMessage( 1, [ 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 2 );
                } );

            var stream = server.Streams.TryGetById( 1 );
            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );

            await Task.Delay( 100 );
            await client.GetTask(
                c =>
                {
                    c.SendMessageNotificationAck( 1, 1, 1, 1 );
                    c.ReadMessageNotification( 1 );
                    c.SendMessageNotificationAck( 1, 1, 1, 0, 1 );
                } );

            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 0 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    clientLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:MessageNotification]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (start)",
                                $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 1",
                                $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 46)",
                                $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 46)",
                                $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (start)",
                                $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 1, Retry = 0, Redelivery = 0, Length = 2",
                                $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 47)",
                                $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 47)",
                                $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 1, Retry = 0, Redelivery = 0",
                                $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (start)",
                                $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 1 (active), Redelivery = 0, Length = 1",
                                $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 46)",
                                $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 46)",
                                $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 1 (active), Redelivery = 0",
                                $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Where( t => t.Logs.Any( l => l.Contains( "[Trace:ProcessMessage]" ) || l.Contains( "[Trace:NegativeAck]" ) ) )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 2UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                $"[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, ClientTraceId = {clientTraceIdsByQueueTraceId.GetValueOrDefault( 3UL )}",
                                "[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, AckId = 1, Delay = 0.05 second(s)",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 1, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 1, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 5UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 7 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 7, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 1 (active), Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 7, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 7UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 7 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Retry_ShouldIgnoreMessagesTargetedToDisposedListeners()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 4 );
            var listenerUnbound = new SafeTaskCompletionSource();
            var firstProcess = new SafeTaskCompletionSource();
            var clientLogs = new ClientEventLogger();
            var queueLogs = new QueueEventLogger();
            var ackExpireDatesByQueueTraceId = new ConcurrentDictionary<ulong, Timestamp>();
            var messageRemoved = Atomic.Create( false );
            var firstProcessComplete = Atomic.Create( false );
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
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                    {
                                        listenerUnbound.Complete();
                                        endSource.Complete();
                                    }
                                    else if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) ) )
                    .SetQueueLoggerFactory(
                        _ => queueLogs.GetLogger(
                            MessageBrokerQueueLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                    {
                                        if ( ! firstProcessComplete.Value )
                                        {
                                            firstProcessComplete.Value = true;
                                            firstProcess.Complete();
                                        }

                                        endSource.Complete();
                                        listenerUnbound.Task.Wait();
                                    }
                                },
                                messageProcessed: e => ackExpireDatesByQueueTraceId[e.Source.TraceId] = e.AckExpiresAt,
                                messageDiscarded: e => messageRemoved.Value = e.MessageRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                    c.SendBindListenerRequest(
                        "c",
                        false,
                        maxRetries: 2,
                        retryDelay: Duration.FromMilliseconds( 15 ),
                        minAckTimeout: Duration.FromSeconds( 10 ) );

                    c.ReadListenerBoundResponse();
                    c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                    c.ReadMessageNotification( 3 );
                    c.SendMessageNotificationNegativeAck( 1, 1, 1, 0 );
                } );

            await firstProcess.Task;
            var stream = server.Streams.TryGetById( 1 );
            var queue = server.Clients.TryGetById( 1 )?.Queues.TryGetById( 1 );
            await client.GetTask(
                c =>
                {
                    c.SendUnbindListenerRequest( 1 );
                    c.ReadListenerUnboundResponse();
                } );

            await endSource.Task;

            Assertion.All(
                    stream.TestNotNull( s => s.Messages.Count.TestEquals( 0 ) ),
                    queue.TestNotNull(
                        q => Assertion.All(
                            "queue",
                            q.Messages.Pending.Count.TestEquals( 0 ),
                            q.Messages.Unacked.Count.TestEquals( 0 ),
                            q.Messages.Retries.Count.TestEquals( 0 ),
                            q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                    clientLogs.GetAll()
                        .Skip( 4 )
                        .Take( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] )
                        ] ),
                    queueLogs.GetAll()
                        .Skip( 2 )
                        .SkipLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                                "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                                $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Ack = (Id = 1, ExpiresAt = {ackExpireDatesByQueueTraceId.GetValueOrDefault( 2UL )})",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, ClientTraceId = 5",
                                "[NegativeAckProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = 3, AckId = 1, Delay = 0.015 second(s)",
                                "[Trace:NegativeAck] Client = [1] 'test', Queue = [1] 'c', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (start)",
                                "[ClientTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, ClientTraceId = 6",
                                "[ListenerUnbound] Client = [1] 'test', Queue = [1] 'c', TraceId = 4, Channel = [1] 'c'",
                                "[Trace:UnbindListener] Client = [1] 'test', Queue = [1] 'c', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (start)",
                                $"[MessageDiscarded] Client = [1] 'test', Queue = [1] 'c', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = DisposedRetry, StoreKey = 0, Retry = 1, Redelivery = 0, MessageRemoved = {messageRemoved.Value}, MovedToDeadLetter = False",
                                "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 5 (end)"
                            ] )
                        ] ) )
                .Go();
        }
    }
}
