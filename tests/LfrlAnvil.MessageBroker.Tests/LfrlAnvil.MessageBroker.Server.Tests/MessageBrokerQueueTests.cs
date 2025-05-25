using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
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
    public async Task MessageRequest_ShouldPropagateMessagesToListenersCorrectly(int prefetchHint)
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 6 );
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
                                if ( e.Type is MessageBrokerRemoteClientTraceEventType.MessageNotification
                                    or MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            } ) ) )
                .SetQueueEventHandlerFactory( _ => logs.Add ) );

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
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'test'::'c'::2] [Created] by listener to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [MessageEnqueued] MessageId = 0 due to listener to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [MessageDequeued] MessageId = 0 due to listener to [1::'c']",
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'test'::'c'::<ROOT>] [MessageEnqueued] MessageId = 1 due to listener to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [MessageDequeued] MessageId = 1 due to listener to [1::'c']",
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'test'::'c'::<ROOT>] [MessageEnqueued] MessageId = 2 due to listener to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [MessageDequeued] MessageId = 2 due to listener to [1::'c']",
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task QueueProcessing_ShouldIgnoreMessagesTargetedToDisposedListeners()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.UnbindListener )
                                endSource.Complete();
                        } ) )
                .SetQueueEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerQueueEventType.MessageDequeued )
                            endSource.Task.Wait();
                    } ) );

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
                c.SendPushMessage( 1, [ 1, 2, 3 ] );
                c.ReadMessageAcceptedResponse();
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
        await endSource.Task;

        Assertion.All(
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::'c'::1] [Created] by listener to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [MessageEnqueued] MessageId = 0 due to listener to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [MessageDequeued] MessageId = 0 due to listener to [1::'c']"
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::'c'::<ROOT>] [MessageEnqueued] MessageId = 1 due to listener to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [MessageEnqueued] MessageId = 2 due to listener to [1::'c']"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task QueueProcessing_ShouldDisposeQueueAutomatically_WhenQueueIsEmptyAndNoListenersAreRelated()
    {
        var endSource = new SafeTaskCompletionSource();
        var listenerDisposedSource = new SafeTaskCompletionSource<MessageBrokerQueueState>();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => MessageBrokerRemoteClientLogger.Create(
                        listenerUnbound: e => listenerDisposedSource.Complete( e.Listener.Queue.State ) ) )
                .SetQueueEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerQueueEventType.MessageDequeued )
                            listenerDisposedSource.Task.Wait();
                        else if ( e.Type == MessageBrokerQueueEventType.Disposed )
                            endSource.Complete();
                    } ) );

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
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::'c'::1] [Created] by listener to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [MessageEnqueued] MessageId = 0 due to listener to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [MessageDequeued] MessageId = 0 due to listener to [1::'c']",
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::'c'::<ROOT>] [MessageEnqueued] MessageId = 1 due to listener to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [Disposing]",
                        "[2::'test2'::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }
}
