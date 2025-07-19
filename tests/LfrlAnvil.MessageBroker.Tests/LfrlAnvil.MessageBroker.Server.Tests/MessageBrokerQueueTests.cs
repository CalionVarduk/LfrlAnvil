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
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerQueueTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public MessageBrokerQueueTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 5 )]
    public async Task MessageNotification_ShouldPropagateMessagesToListenersCorrectly(short prefetchHint)
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 9 );
        var storeKeyByMessageId = new ConcurrentDictionary<ulong, int>();
        var dataRemovedByMessageId = new ConcurrentDictionary<ulong, bool>();
        var streamTraceIdsByQueueTraceId = new ConcurrentDictionary<ulong, ulong>();
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
                                if ( e.Type is MessageBrokerRemoteClientTraceEventType.MessageNotification
                                    or MessageBrokerRemoteClientTraceEventType.PushMessage )
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
                            streamTrace: e => streamTraceIdsByQueueTraceId[e.Source.TraceId] = e.Correlation.TraceId,
                            messageProcessed: e => dataRemovedByMessageId[e.MessageId] = e.MessageRemoved ) ) )
                .SetStreamLoggerFactory(
                    _ => MessageBrokerStreamLogger.Create( messagePushed: e => storeKeyByMessageId[e.MessageId] = e.StoreKey ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest( "c", true, prefetchHint: prefetchHint );
                c.ReadListenerBoundResponse();
                c.SendPushMessage( 1, [ 1 ] );
                c.SendPushMessage( 1, [ 2, 3 ] );
                c.SendPushMessage( 1, [ 4, 5, 6 ] );

                var nextNotificationLength = 1;
                for ( var i = 0; i < 6; ++i )
                {
                    var index = c.ReadAny(
                            (MessageBrokerClientEndpoint.MessageAcceptedResponse, Protocol.MessageAcceptedResponse.Payload),
                            (MessageBrokerClientEndpoint.MessageNotification,
                                Protocol.MessageNotificationHeader.Payload + nextNotificationLength) )
                        .Index;

                    if ( index != 0 )
                        ++nextNotificationLength;
                }
            } );

        await endSource.Task;

        Assertion.All(
                clientLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.Contains( "[Trace:PushMessage]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Client = [1] 'test', TraceId = {t.Id} (start)",
                            $"[ReadPacket:Received] Client = [1] 'test', TraceId = {t.Id}, Packet = (PushMessage, Length = 11)",
                            $"[PushingMessage] Client = [1] 'test', TraceId = {t.Id}, Length = 1, ChannelId = 1, Confirm = True",
                            $"[ReadPacket:Accepted] Client = [1] 'test', TraceId = {t.Id}, Packet = (PushMessage, Length = 11)",
                            $"[MessagePushed] Client = [1] 'test', TraceId = {t.Id}, Channel = [1] 'c', Stream = [1] 'c', MessageId = 0",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageAcceptedResponse, Length = 13)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageAcceptedResponse, Length = 13)",
                            $"[Trace:PushMessage] Client = [1] 'test', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Client = [1] 'test', TraceId = {t.Id} (start)",
                            $"[ReadPacket:Received] Client = [1] 'test', TraceId = {t.Id}, Packet = (PushMessage, Length = 12)",
                            $"[PushingMessage] Client = [1] 'test', TraceId = {t.Id}, Length = 2, ChannelId = 1, Confirm = True",
                            $"[ReadPacket:Accepted] Client = [1] 'test', TraceId = {t.Id}, Packet = (PushMessage, Length = 12)",
                            $"[MessagePushed] Client = [1] 'test', TraceId = {t.Id}, Channel = [1] 'c', Stream = [1] 'c', MessageId = 1",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageAcceptedResponse, Length = 13)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageAcceptedResponse, Length = 13)",
                            $"[Trace:PushMessage] Client = [1] 'test', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Client = [1] 'test', TraceId = {t.Id} (start)",
                            $"[ReadPacket:Received] Client = [1] 'test', TraceId = {t.Id}, Packet = (PushMessage, Length = 13)",
                            $"[PushingMessage] Client = [1] 'test', TraceId = {t.Id}, Length = 3, ChannelId = 1, Confirm = True",
                            $"[ReadPacket:Accepted] Client = [1] 'test', TraceId = {t.Id}, Packet = (PushMessage, Length = 13)",
                            $"[MessagePushed] Client = [1] 'test', TraceId = {t.Id}, Channel = [1] 'c', Stream = [1] 'c', MessageId = 2",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageAcceptedResponse, Length = 13)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageAcceptedResponse, Length = 13)",
                            $"[Trace:PushMessage] Client = [1] 'test', TraceId = {t.Id} (end)"
                        ] )
                    ] ),
                clientLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.Contains( "[Trace:MessageNotification]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, Retry = 0, Redelivery = 0, Length = 1",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 46)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 46)",
                            $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, Retry = 0, Redelivery = 0",
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 1, Retry = 0, Redelivery = 0, Length = 2",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 47)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 47)",
                            $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 1, Retry = 0, Redelivery = 0",
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 2, Retry = 0, Redelivery = 0, Length = 3",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 48)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 48)",
                            $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 2, Retry = 0, Redelivery = 0",
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (end)"
                        ] )
                    ] ),
                clientLogs.GetAll().Where( t => t.Logs.Any( e => e.Contains( "[Trace:SystemNotification]" ) ) ).TestEmpty(),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 11)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 12)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 13)"
                    ] ),
                queueLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.Contains( "[Trace:EnqueueMessage]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:EnqueueMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (start)",
                            $"[StreamTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Correlation = (Stream = [1] 'c', TraceId = {streamTraceIdsByQueueTraceId.GetValueOrDefault( t.Id )})",
                            $"[EnqueueingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 0UL )}, Length = 1",
                            $"[MessageEnqueued] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0",
                            $"[Trace:EnqueueMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:EnqueueMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (start)",
                            $"[StreamTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Correlation = (Stream = [1] 'c', TraceId = {streamTraceIdsByQueueTraceId.GetValueOrDefault( t.Id )})",
                            $"[EnqueueingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 1, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 1UL )}, Length = 2",
                            $"[MessageEnqueued] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 1",
                            $"[Trace:EnqueueMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:EnqueueMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (start)",
                            $"[StreamTrace] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Correlation = (Stream = [1] 'c', TraceId = {streamTraceIdsByQueueTraceId.GetValueOrDefault( t.Id )})",
                            $"[EnqueueingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 2, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 2UL )}, Length = 3",
                            $"[MessageEnqueued] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 2",
                            $"[Trace:EnqueueMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (end)"
                        ] )
                    ] ),
                queueLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.Contains( "[Trace:ProcessMessage]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = {storeKeyByMessageId.GetValueOrDefault( 0UL )}, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                            $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, MessageRemoved = {dataRemovedByMessageId.GetValueOrDefault( 0UL )}",
                            $"[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = {storeKeyByMessageId.GetValueOrDefault( 1UL )}, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                            $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 1, MessageRemoved = {dataRemovedByMessageId.GetValueOrDefault( 1UL )}",
                            $"[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = {storeKeyByMessageId.GetValueOrDefault( 2UL )}, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                            $"[MessageProcessed] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 2, MessageRemoved = {dataRemovedByMessageId.GetValueOrDefault( 2UL )}",
                            $"[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = {t.Id} (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task QueueProcessing_ShouldIgnoreMessagesTargetedToDisposedListeners()
    {
        var pushContinuation = new SafeTaskCompletionSource();
        var unbindListenerContinuation = new SafeTaskCompletionSource( completionCount: 2 );
        var processContinuation = new SafeTaskCompletionSource( completionCount: 3 );
        var storeKeyByMessageId = new ConcurrentDictionary<ulong, int>();
        var dataRemovedByMessageId = new ConcurrentDictionary<ulong, bool>();
        var dataRemovedByStoreKey = new ConcurrentDictionary<int, bool>();
        var firstPushStarted = Atomic.Create( false );
        var firstProcessStarted = Atomic.Create( false );
        var endSource = new SafeTaskCompletionSource( completionCount: 3 );
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                            {
                                if ( ! firstPushStarted.Value )
                                    firstPushStarted.Value = true;
                                else
                                    pushContinuation.Task.Wait();
                            }
                            else if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                unbindListenerContinuation.Task.Wait();
                        },
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                            {
                                processContinuation.Complete();
                                endSource.Complete();
                            }
                        } ) )
                .SetQueueLoggerFactory(
                    _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage && ! firstProcessStarted.Value )
                                {
                                    firstProcessStarted.Value = true;
                                    pushContinuation.Complete();
                                }
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                {
                                    processContinuation.Task.Wait();
                                    endSource.Complete();
                                }
                                else if ( e.Type == MessageBrokerQueueTraceEventType.EnqueueMessage )
                                {
                                    processContinuation.Complete();
                                    unbindListenerContinuation.Complete();
                                }
                            },
                            messageProcessed: e => dataRemovedByMessageId[e.MessageId] = e.MessageRemoved,
                            messageDiscarded: e => dataRemovedByStoreKey[e.StoreKey] = e.MessageRemoved ) ) )
                .SetStreamLoggerFactory(
                    _ => MessageBrokerStreamLogger.Create( messagePushed: e => storeKeyByMessageId[e.MessageId] = e.StoreKey ) ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        using var client2 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client2.EstablishHandshake( server, "test2" );

        await client1.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        await client2.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
                c.SendBindListenerRequest( "d", true, queueName: "c" );
                c.ReadListenerBoundResponse();
            } );

        await client1.GetTask(
            c =>
            {
                c.SendPushMessage( 1, [ 1 ] );
                c.SendPushMessage( 1, [ 1, 2 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
            } );

        await client2.GetTask(
            c =>
            {
                c.ReadMessageNotification( 1 );
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

        await endSource.Task;

        queueLogs.GetAll()
            .Skip( 2 )
            .Where( t => ! t.Logs.Any( l => l.Contains( "[Trace:UnbindListener]" ) ) )
            .TestSequence(
            [
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:EnqueueMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = 2 (start)",
                    "[StreamTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = 2, Correlation = (Stream = [1] 'c', TraceId = 2)",
                    $"[EnqueueingMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = 2, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 0UL )}, Length = 1",
                    "[MessageEnqueued] Client = [2] 'test2', Queue = [1] 'c', TraceId = 2, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0",
                    "[Trace:EnqueueMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = 2 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:ProcessMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = 3 (start)",
                    $"[ProcessingMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = 3, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = {storeKeyByMessageId.GetValueOrDefault( 0UL )}, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                    $"[MessageProcessed] Client = [2] 'test2', Queue = [1] 'c', TraceId = 3, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, MessageRemoved = {dataRemovedByMessageId.GetValueOrDefault( 0UL )}",
                    "[Trace:ProcessMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = 3 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    $"[Trace:EnqueueMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id} (start)",
                    $"[StreamTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id}, Correlation = (Stream = [1] 'c', TraceId = 4)",
                    $"[EnqueueingMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 1, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 1UL )}, Length = 2",
                    $"[MessageEnqueued] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 1",
                    $"[Trace:EnqueueMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id} (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    $"[Trace:ProcessMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id} (start)",
                    $"[MessageDiscarded] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Reason = DisposedPending, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 1UL )}, Retry = 0, Redelivery = 0, MessageRemoved = {dataRemovedByStoreKey.GetValueOrDefault( storeKeyByMessageId.GetValueOrDefault( 1UL ) )}, MovedToDeadLetter = False",
                    $"[Trace:ProcessMessage] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id} (end)"
                ] )
            ] )
            .Go();
    }

    [Fact]
    public async Task MessageNotification_ShouldBePrecededBySenderAndStreamNames_WhenTheyWereNotSent()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    c => c.Id == 1
                        ? clientLogs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                        endSource.Complete();
                                } ) )
                        : null ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server, synchronizeExternalObjectNames: true );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );

        await client2.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        await client1.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
            } );

        await client2.GetTask(
            c =>
            {
                c.SendPushMessage( 1, [ 1, 2 ], confirm: false );
                c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
            } );

        await client1.GetTask(
            c =>
            {
                c.ReadObjectNameSystemNotification(
                    new Protocol.ObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "test2" ) );

                c.ReadObjectNameSystemNotification(
                    new Protocol.ObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "c" ) );

                c.ReadMessageNotification( 2 );
                c.ReadMessageNotification( 3 );
            } );

        await endSource.Task;

        clientLogs.GetAll()
            .Skip( 2 )
            .TestSequence(
            [
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)",
                    "[ProcessingMessage] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, Retry = 0, Redelivery = 0, Length = 2",
                    "[SendingSenderName] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 2, Type = SenderName",
                    "[SendingStreamName] Client = [1] 'test', TraceId = 2, Stream = [1] 'c'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 11)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 11)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 2, Type = StreamName",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 47)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 47)",
                    "[MessageProcessed] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, Retry = 0, Redelivery = 0",
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 3 (start)",
                    "[ProcessingMessage] Client = [1] 'test', TraceId = 3, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 1, Retry = 0, Redelivery = 0, Length = 3",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (MessageNotification, Length = 48)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (MessageNotification, Length = 48)",
                    "[MessageProcessed] Client = [1] 'test', TraceId = 3, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 1, Retry = 0, Redelivery = 0",
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 3 (end)"
                ] )
            ] )
            .Go();
    }

    [Fact]
    public async Task MessageNotification_ShouldOnlyBePrecededByStreamName_WhenClientSendsMessageToSelf()
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
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server, synchronizeExternalObjectNames: true );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
                c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
                c.ReadObjectNameSystemNotification(
                    new Protocol.ObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "c" ) );

                c.ReadMessageNotification( 3 );
            } );

        await endSource.Task;

        clientLogs.GetAll()
            .Skip( 4 )
            .TestSequence(
            [
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                    "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                    "[SendingStreamName] Client = [1] 'test', TraceId = 4, Stream = [1] 'c'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (SystemNotification, Length = 11)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (SystemNotification, Length = 11)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 4, Type = StreamName",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                    "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, Retry = 0, Redelivery = 0",
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                ] )
            ] )
            .Go();
    }

    [Fact]
    public async Task MessageNotification_ShouldBePrecededBySenderAndStreamNames_WhenTheyChange()
    {
        var disposalContinuation = new SafeTaskCompletionSource();
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var clientLogs = new ClientEventLogger();
        var isDisposed = Atomic.Create( false );
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    c =>
                    {
                        if ( c.Id == 1 )
                            return clientLogs.GetLogger(
                                MessageBrokerRemoteClientLogger.Create(
                                    traceEnd: e =>
                                    {
                                        if ( e.Type == MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                            endSource.Complete();
                                    } ) );

                        return MessageBrokerRemoteClientLogger.Create(
                            disposed: _ =>
                            {
                                if ( ! isDisposed.Value )
                                {
                                    isDisposed.Value = true;
                                    disposalContinuation.Complete();
                                }
                            } );
                    } ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server, synchronizeExternalObjectNames: true );
        await client1.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
            } );

        using ( var client2 = new ClientMock() )
        {
            await client2.EstablishHandshake( server, "test2" );
            await client2.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "c" );
                    c.ReadPublisherBoundResponse();
                } );

            await client2.GetTask( c => c.SendPushMessage( 1, [ 1, 2 ], confirm: false ) );

            await client1.GetTask(
                c =>
                {
                    c.ReadObjectNameSystemNotification(
                        new Protocol.ObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "test2" ) );

                    c.ReadObjectNameSystemNotification(
                        new Protocol.ObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "c" ) );

                    c.ReadMessageNotification( 2 );
                    c.SendUnbindListenerRequest( 1 );
                    c.ReadListenerUnboundResponse();
                } );
        }

        await disposalContinuation.Task;
        await client1.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "d", true );
                c.ReadListenerBoundResponse();
            } );

        using var client3 = new ClientMock();
        await client3.EstablishHandshake( server, "test3" );
        await client3.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "d" );
                c.ReadPublisherBoundResponse();
                c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
            } );

        await client1.GetTask(
            c =>
            {
                c.ReadObjectNameSystemNotification(
                    new Protocol.ObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "test3" ) );

                c.ReadObjectNameSystemNotification(
                    new Protocol.ObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "d" ) );

                c.ReadMessageNotification( 3 );
            } );

        await endSource.Task;

        clientLogs.GetAll()
            .Where( t => t.Logs.Any( e => e.Contains( "[Trace:MessageNotification]" ) ) )
            .TestSequence(
            [
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (start)",
                    "[ProcessingMessage] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, Retry = 0, Redelivery = 0, Length = 2",
                    "[SendingSenderName] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 2, Type = SenderName",
                    "[SendingStreamName] Client = [1] 'test', TraceId = 2, Stream = [1] 'c'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 11)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 11)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 2, Type = StreamName",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 47)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 47)",
                    "[MessageProcessed] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, Retry = 0, Redelivery = 0",
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 5 (start)",
                    "[ProcessingMessage] Client = [1] 'test', TraceId = 5, Sender = [2] 'test3', Channel = [1] 'd', Stream = [1] 'd', Queue = [1] 'd', MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                    "[SendingSenderName] Client = [1] 'test', TraceId = 5, Sender = [2] 'test3'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (SystemNotification, Length = 15)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 5, Packet = (SystemNotification, Length = 15)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 5, Type = SenderName",
                    "[SendingStreamName] Client = [1] 'test', TraceId = 5, Stream = [1] 'd'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (SystemNotification, Length = 11)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 5, Packet = (SystemNotification, Length = 11)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 5, Type = StreamName",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (MessageNotification, Length = 48)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 5, Packet = (MessageNotification, Length = 48)",
                    "[MessageProcessed] Client = [1] 'test', TraceId = 5, Sender = [2] 'test3', Channel = [1] 'd', Stream = [1] 'd', Queue = [1] 'd', MessageId = 0, Retry = 0, Redelivery = 0",
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 5 (end)"
                ] )
            ] )
            .Go();
    }

    [Fact]
    public async Task QueueProcessing_ShouldDisposeQueueAutomatically_WhenQueueIsEmptyAndNoListenersAreRelated()
    {
        var endSource = new SafeTaskCompletionSource();
        var listenerDisposedSource = new SafeTaskCompletionSource<MessageBrokerQueueState>();
        var queueLogs = new QueueEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => MessageBrokerRemoteClientLogger.Create(
                        listenerUnbound: e => listenerDisposedSource.Complete( e.Listener.Queue.State ) ) )
                .SetQueueLoggerFactory(
                    _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.Dispose )
                                    endSource.Complete();
                                else if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                    listenerDisposedSource.Task.Wait();
                            } ) ) ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        using var client2 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client2.EstablishHandshake( server, "test2" );

        await client1.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        await client2.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
            } );

        await client1.GetTask(
            c =>
            {
                c.SendPushMessage( 1, [ 1 ] );
                c.SendPushMessage( 1, [ 1, 2 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
            } );

        await client2.GetTask(
            c =>
            {
                c.ReadMessageNotification( 1 );
                c.SendUnbindListenerRequest( 1 );
                c.ReadListenerUnboundResponse();
            } );

        var queueStateOnListenerDisposed = await listenerDisposedSource.Task;
        await endSource.Task;

        Assertion.All(
                queueStateOnListenerDisposed.TestEquals( MessageBrokerQueueState.Running ),
                queueLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:Dispose] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id} (start)",
                            $"[Disposing] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id}",
                            $"[Disposed] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id}",
                            $"[Trace:Dispose] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id} (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task QueueProcessing_ShouldLogLackOfMessageDataInStream()
    {
        Exception? exception = null;
        MessageBrokerStream? stream = null;
        var endSource = new SafeTaskCompletionSource();
        var streamEndSource = new SafeTaskCompletionSource();
        var readMessageSource = new SafeTaskCompletionSource();
        var continuation = new SafeTaskCompletionSource();
        var queueLogs = new QueueEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetQueueLoggerFactory(
                    _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                    streamEndSource.Task.Wait();
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage )
                                    endSource.Complete();
                            },
                            processingMessage: e =>
                            {
                                readMessageSource.Complete();
                                continuation.Task.Wait();
                                if ( stream is not null )
                                {
                                    using ( stream.AcquireLock() )
                                        stream.MessageStore.DecrementRefCount( e.StoreKey, out var _ );
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
                c.SendBindListenerRequest( "c", false );
                c.ReadListenerBoundResponse();
            } );

        var minPushedAt = TimestampProvider.Shared.GetNow();
        var remoteClient = server.Clients.TryGetById( 1 );
        var queue = remoteClient?.Queues.TryGetById( 1 );
        var publisher = remoteClient?.Publishers.TryGetByChannelId( 1 );
        var listener = remoteClient?.Listeners.TryGetByChannelId( 1 );
        stream = server.Streams.TryGetById( 1 );

        await client.GetTask( c => c.SendPushMessage( 1, [ 1 ], confirm: false ) );
        await readMessageSource.Task;

        var pendingCount = queue?.Messages.Pending.Count ?? 0;
        var messageCount = stream?.Messages.Count ?? 0;
        var pendingMessage = queue?.Messages.Pending.TryPeekAt( 0 );
        var message = pendingMessage?.TryGetMessage();

        continuation.Complete();
        await endSource.Task;

        Assertion.All(
                exception.TestType().Exact<MessageBrokerQueueException>( e => e.Queue.TestRefEquals( queue ) ),
                stream.TestNotNull( s => s.Messages.Count.TestEquals( 0 ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Running ),
                        q.Messages.Pending.Count.TestEquals( 0 ) ) ),
                pendingCount.TestEquals( 1 ),
                messageCount.TestEquals( 1 ),
                message.TestNotNull(
                    m => Assertion.All(
                        "message",
                        m.Publisher.TestRefEquals( publisher ),
                        m.Length.TestEquals( MemorySize.FromBytes( 1 ) ),
                        m.Id.TestEquals( 0UL ),
                        m.PushedAt.TestGreaterThanOrEqualTo( minPushedAt ),
                        m.StoreKey.TestEquals( 0 ),
                        m.RefCount.TestInRange( 1, 2 ),
                        m.ToString()
                            .TestEquals(
                                $"Publisher = ([1] 'test' => [1] 'c' publisher binding (using [1] 'c' stream) (Running)), Length = 1 B, Id = 0, PushedAt = {m.PushedAt}, StoreKey = 0, RefCount = {m.RefCount}" ) ) ),
                pendingMessage.TestNotNull(
                    m => Assertion.All(
                        "pendingMessage",
                        m.Publisher.TestRefEquals( publisher ),
                        m.Listener.TestRefEquals( listener ),
                        m.StoreKey.TestEquals( 0 ),
                        m.ToString()
                            .TestEquals(
                                "Publisher = ([1] 'test' => [1] 'c' publisher binding (using [1] 'c' stream) (Running)), Listener = ([1] 'test' => [1] 'c' listener binding (using [1] 'c' queue) (Running)), StoreKey = 0" ) ) ),
                queueLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (start)",
                            "[ProcessingMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', StoreKey = 0, Retry = 0, Redelivery = 0, IsFromDeadLetter = False",
                            """
                            [Error] Client = [1] 'test', Queue = [1] 'c', TraceId = 2
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerQueueException: Stream [1] 'c' does not have a message related to store key 0.
                            """,
                            "[Trace:ProcessMessage] Client = [1] 'test', Queue = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Disposal_ShouldDiscardEnqueuedMessages()
    {
        Exception? exception = null;
        var pushContinuation = new SafeTaskCompletionSource();
        var processContinuation = new SafeTaskCompletionSource( completionCount: 2 );
        var endSource = new SafeTaskCompletionSource<Task>();
        var queueLogs = new QueueEventLogger();
        var firstPushStarted = Atomic.Create( false );
        var firstProcessStarted = Atomic.Create( false );
        var firstProcessEnded = Atomic.Create( false );
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => MessageBrokerRemoteClientLogger.Create(
                        traceStart: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                            {
                                if ( ! firstPushStarted.Value )
                                    firstPushStarted.Value = true;
                                else
                                    pushContinuation.Task.Wait();
                            }
                        } ) )
                .SetQueueLoggerFactory(
                    _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage && ! firstProcessStarted.Value )
                                {
                                    firstProcessStarted.Value = true;
                                    pushContinuation.Complete();
                                }
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessage && ! firstProcessEnded.Value )
                                {
                                    firstProcessEnded.Value = true;
                                    processContinuation.Task.Wait();
                                    endSource.Complete( e.Source.Queue.Client.DisconnectAsync().AsTask() );
                                }
                                else if ( e.Type == MessageBrokerQueueTraceEventType.EnqueueMessage )
                                    processContinuation.Complete();
                            },
                            error: e => exception = e.Exception ) ) ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        using var client2 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client2.EstablishHandshake( server, "test2" );

        await client1.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        await client2.GetTask(
            c =>
            {
                c.SendBindListenerRequest( "c", true );
                c.ReadListenerBoundResponse();
            } );

        var queue = server.Clients.TryGetById( 2 )?.Queues.TryGetById( 1 );

        await client1.GetTask(
            c =>
            {
                c.SendPushMessage( 1, [ 1 ], confirm: false );
                c.SendPushMessage( 1, [ 1, 2 ], confirm: false );
            } );

        await endSource.Task.Unwrap();

        Assertion.All(
                exception.TestType().Exact<MessageBrokerQueueException>( exc => exc.Queue.TestRefEquals( queue ) ),
                queueLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:Dispose] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ClientTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id}, ClientTraceId = 3",
                            $"[Disposing] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id}",
                            $"""
                             [Error] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id}
                             LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerQueueException: 1 enqueued message(s) have been discarded due to client disposal.
                             """,
                            $"[Disposed] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id}",
                            $"[Trace:Dispose] Client = [2] 'test2', Queue = [1] 'c', TraceId = {t.Id} (end)"
                        ] )
                    ] ) )
            .Go();
    }
}
