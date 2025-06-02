using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerQueueTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public MessageBrokerQueueTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 5 )]
    public async Task MessageNotification_ShouldPropagateMessagesToListenersCorrectly(int prefetchHint)
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 6 );
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
                .SetQueueLoggerFactory( _ => queueLogs.GetLogger() ) );

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
                            $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, RetryAttempt = 0, RedeliveryAttempt = 0, Length = 1",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 42)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 42)",
                            $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0",
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 1, RetryAttempt = 0, RedeliveryAttempt = 0, Length = 2",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 43)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 43)",
                            $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 1",
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 2, RetryAttempt = 0, RedeliveryAttempt = 0, Length = 3",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 44)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 44)",
                            $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 2",
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
                queueLogs.GetAll().Count( t => t.Logs.Any( e => e.Contains( "[Trace:EnqueueMessages]" ) ) ).TestInRange( 1, 3 ),
                queueLogs.GetAll().Count( t => t.Logs.Any( e => e.Contains( "[Trace:ProcessMessages]" ) ) ).TestInRange( 1, 3 ) )
            .Go();
    }

    [Fact]
    public async Task QueueProcessing_ShouldIgnoreMessagesTargetedToDisposedListeners()
    {
        var pushContinuation = new SafeTaskCompletionSource();
        var processContinuation = new SafeTaskCompletionSource( completionCount: 3 );
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage && e.Source.TraceId > 2 )
                                pushContinuation.Task.Wait();
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
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessages && e.Source.TraceId == 3 )
                                    pushContinuation.Complete();
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessages )
                                {
                                    processContinuation.Task.Wait();
                                    endSource.Complete();
                                }
                            },
                            messagesEnqueued: e =>
                            {
                                for ( var i = 0; i < e.MessageCount; ++i )
                                    processContinuation.Complete();
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
            .TestSequence(
            [
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:EnqueueMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 2 (start)",
                    "[StreamTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = 2, Correlation = (Stream = [1] 'c', TraceId = 2)",
                    "[EnqueueingMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 2, MessageCount = 1",
                    "[MessagesEnqueued] Client = [2] 'test2', Queue = [1] 'c', TraceId = 2, MessageCount = 1",
                    "[Trace:EnqueueMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 2 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:ProcessMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 3 (start)",
                    "[ProcessingMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 3, MessageCount = 1, SkippedMessageCount = 0",
                    "[MessageProcessed] Client = [2] 'test2', Queue = [1] 'c', TraceId = 3, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', MessageId = 0, Length = 1",
                    "[Trace:ProcessMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 3 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:EnqueueMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 4 (start)",
                    "[StreamTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = 4, Correlation = (Stream = [1] 'c', TraceId = 4)",
                    "[EnqueueingMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 4, MessageCount = 1",
                    "[MessagesEnqueued] Client = [2] 'test2', Queue = [1] 'c', TraceId = 4, MessageCount = 1",
                    "[Trace:EnqueueMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 4 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:UnbindListener] Client = [2] 'test2', Queue = [1] 'c', TraceId = 5 (start)",
                    "[ClientTrace] Client = [2] 'test2', Queue = [1] 'c', TraceId = 5, ClientTraceId = 4",
                    "[ListenerUnbound] Client = [2] 'test2', Queue = [1] 'c', TraceId = 5, Channel = [1] 'c'",
                    "[Trace:UnbindListener] Client = [2] 'test2', Queue = [1] 'c', TraceId = 5 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:ProcessMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 6 (start)",
                    "[ProcessingMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 6, MessageCount = 0, SkippedMessageCount = 1",
                    "[Trace:ProcessMessages] Client = [2] 'test2', Queue = [1] 'c', TraceId = 6 (end)"
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
                    "[ProcessingMessage] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, RetryAttempt = 0, RedeliveryAttempt = 0, Length = 2",
                    "[SendingSenderName] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 2, Type = SenderName",
                    "[SendingStreamName] Client = [1] 'test', TraceId = 2, Stream = [1] 'c'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 11)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 11)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 2, Type = StreamName",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 43)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 43)",
                    "[MessageProcessed] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0",
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 3 (start)",
                    "[ProcessingMessage] Client = [1] 'test', TraceId = 3, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 1, RetryAttempt = 0, RedeliveryAttempt = 0, Length = 3",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (MessageNotification, Length = 44)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (MessageNotification, Length = 44)",
                    "[MessageProcessed] Client = [1] 'test', TraceId = 3, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 1",
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
                    "[ProcessingMessage] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, RetryAttempt = 0, RedeliveryAttempt = 0, Length = 3",
                    "[SendingStreamName] Client = [1] 'test', TraceId = 4, Stream = [1] 'c'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (SystemNotification, Length = 11)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (SystemNotification, Length = 11)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 4, Type = StreamName",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 44)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 44)",
                    "[MessageProcessed] Client = [1] 'test', TraceId = 4, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0",
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
                                if ( ! disposalContinuation.Task.IsCompleted )
                                    disposalContinuation.Complete();
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
                    "[ProcessingMessage] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, RetryAttempt = 0, RedeliveryAttempt = 0, Length = 2",
                    "[SendingSenderName] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 2, Type = SenderName",
                    "[SendingStreamName] Client = [1] 'test', TraceId = 2, Stream = [1] 'c'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 11)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 11)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 2, Type = StreamName",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 43)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (MessageNotification, Length = 43)",
                    "[MessageProcessed] Client = [1] 'test', TraceId = 2, Sender = [2] 'test2', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0",
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 2 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:MessageNotification] Client = [1] 'test', TraceId = 5 (start)",
                    "[ProcessingMessage] Client = [1] 'test', TraceId = 5, Sender = [2] 'test3', Channel = [1] 'd', Stream = [1] 'd', Queue = [1] 'd', MessageId = 0, RetryAttempt = 0, RedeliveryAttempt = 0, Length = 3",
                    "[SendingSenderName] Client = [1] 'test', TraceId = 5, Sender = [2] 'test3'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (SystemNotification, Length = 15)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 5, Packet = (SystemNotification, Length = 15)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 5, Type = SenderName",
                    "[SendingStreamName] Client = [1] 'test', TraceId = 5, Stream = [1] 'd'",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (SystemNotification, Length = 11)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 5, Packet = (SystemNotification, Length = 11)",
                    "[SystemNotificationSent] Client = [1] 'test', TraceId = 5, Type = StreamName",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (MessageNotification, Length = 44)",
                    "[SendPacket:Sent] Client = [1] 'test', TraceId = 5, Packet = (MessageNotification, Length = 44)",
                    "[MessageProcessed] Client = [1] 'test', TraceId = 5, Sender = [2] 'test3', Channel = [1] 'd', Stream = [1] 'd', Queue = [1] 'd', MessageId = 0",
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
                            },
                            messageProcessed: _ => listenerDisposedSource.Task.Wait() ) ) ) );

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
    public async Task Disposal_ShouldDiscardEnqueuedMessages()
    {
        Exception? exception = null;
        var pushContinuation = new SafeTaskCompletionSource();
        var processContinuation = new SafeTaskCompletionSource( completionCount: 2 );
        var endSource = new SafeTaskCompletionSource();
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
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage && e.Source.TraceId > 2 )
                                pushContinuation.Task.Wait();
                        },
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Dispose && e.Source.Client.Id == 2 )
                                endSource.Complete();
                        } ) )
                .SetQueueLoggerFactory(
                    _ => queueLogs.GetLogger(
                        MessageBrokerQueueLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessages && e.Source.TraceId == 2 )
                                    pushContinuation.Complete();
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerQueueTraceEventType.ProcessMessages && e.Source.TraceId == 2 )
                                {
                                    processContinuation.Task.Wait();
                                    var __ = e.Source.Queue.Client.DisconnectAsync().AsTask();
                                }
                            },
                            messagesEnqueued: e =>
                            {
                                for ( var i = 0; i < e.MessageCount; ++i )
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

        await client2.GetTask( c => c.ReadMessageNotification( 1 ) );
        await endSource.Task;

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
