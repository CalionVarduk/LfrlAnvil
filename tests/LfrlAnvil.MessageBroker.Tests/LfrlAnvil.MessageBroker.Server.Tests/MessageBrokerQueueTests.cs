using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerQueueTests : TestsBase
{
    [Theory]
    [InlineData( 1 )]
    [InlineData( 5 )]
    public async Task MessageRequest_ShouldPropagateMessagesToSubscribersCorrectly(int prefetchHint)
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 3 );
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.MessageNotification )
                            endSource.Complete();
                    } )
                .SetQueueEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
                c.SendSubscribeRequest( "c", true, prefetchHint: prefetchHint );
                c.ReadSubscribedResponse();
                c.SendMessageRequest( 1, [ 1 ] );
                c.SendMessageRequest( 1, [ 2, 3 ] );
                c.SendMessageRequest( 1, [ 4, 5, 6 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
                c.ReadMessageNotification( 1 );
                c.ReadMessageNotification( 2 );
                c.ReadMessageNotification( 3 );
            } );

        await endSource.Task;

        Assertion.All(
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::6] [SendingMessage] [PacketLength: 42] MessageNotification",
                        "[1::'test'::6] [MessageSent] [PacketLength: 42] MessageNotification",
                        "[1::'test'::7] [SendingMessage] [PacketLength: 43] MessageNotification",
                        "[1::'test'::7] [MessageSent] [PacketLength: 43] MessageNotification",
                        "[1::'test'::8] [SendingMessage] [PacketLength: 44] MessageNotification",
                        "[1::'test'::8] [MessageSent] [PacketLength: 44] MessageNotification"
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'test'::'c'::2] [Created] by subscription to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [MessageEnqueued] MessageId = 0 due to subscription to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [MessageDequeued] MessageId = 0 due to subscription to [1::'c']",
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'test'::'c'::<ROOT>] [MessageEnqueued] MessageId = 1 due to subscription to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [MessageDequeued] MessageId = 1 due to subscription to [1::'c']",
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'test'::'c'::<ROOT>] [MessageEnqueued] MessageId = 2 due to subscription to [1::'c']",
                        "[1::'test'::'c'::<ROOT>] [MessageDequeued] MessageId = 2 due to subscription to [1::'c']",
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task QueueProcessing_ShouldIgnoreMessagesTargetedToDisposedSubscriptions()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetSubscriptionEventHandlerFactory(
                    _ => e =>
                    {
                        if ( e.Type == MessageBrokerSubscriptionEventType.Disposed )
                            endSource.Complete();
                    } )
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
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        await client2.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", true );
                c.ReadSubscribedResponse();
                c.SendSubscribeRequest( "d", true, queueName: "c" );
                c.ReadSubscribedResponse();
            } );

        await client1.GetTask(
            c =>
            {
                c.SendMessageRequest( 1, [ 1 ] );
                c.SendMessageRequest( 1, [ 1, 2 ] );
                c.SendMessageRequest( 1, [ 1, 2, 3 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
            } );

        await client2.GetTask(
            c =>
            {
                c.ReadMessageNotification( 1 );
                c.SendUnsubscribeRequest( 1 );
                c.ReadUnsubscribedResponse();
            } );

        await endSource.Task;
        await endSource.Task;

        logs.GetAllQueue()
            .TestContainsSequence(
            [
                "[2::'test2'::'c'::1] [Created] by subscription to [1::'c']",
                "[2::'test2'::'c'::<ROOT>] [MessageEnqueued] MessageId = 0 due to subscription to [1::'c']",
                "[2::'test2'::'c'::<ROOT>] [MessageEnqueued] MessageId = 1 due to subscription to [1::'c']",
                "[2::'test2'::'c'::<ROOT>] [MessageDequeued] MessageId = 0 due to subscription to [1::'c']"
            ] )
            .Go();
    }

    [Fact]
    public async Task QueueProcessing_ShouldDisposeQueueAutomatically_WhenQueueIsEmptyAndNoSubscriptionsAreRelated()
    {
        var endSource = new SafeTaskCompletionSource();
        var subscriptionDisposedSource = new SafeTaskCompletionSource<MessageBrokerQueueState>();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetSubscriptionEventHandlerFactory(
                    _ => e =>
                    {
                        if ( e.Type == MessageBrokerSubscriptionEventType.Disposed )
                            subscriptionDisposedSource.Complete( e.Subscription.Queue.State );
                    } )
                .SetQueueEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerQueueEventType.MessageDequeued )
                            subscriptionDisposedSource.Task.Wait();
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
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        await client2.GetTask(
            c =>
            {
                c.SendSubscribeRequest( "c", true );
                c.ReadSubscribedResponse();
            } );

        await client1.GetTask(
            c =>
            {
                c.SendMessageRequest( 1, [ 1 ] );
                c.SendMessageRequest( 1, [ 1, 2 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
            } );

        await client2.GetTask(
            c =>
            {
                c.ReadMessageNotification( 1 );
                c.SendUnsubscribeRequest( 1 );
                c.ReadUnsubscribedResponse();
            } );

        var queueStateOnBindingDisposed = await subscriptionDisposedSource.Task;
        await endSource.Task;

        Assertion.All(
                queueStateOnBindingDisposed.TestEquals( MessageBrokerQueueState.Running ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::'c'::1] [Created] by subscription to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [MessageEnqueued] MessageId = 0 due to subscription to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [MessageEnqueued] MessageId = 1 due to subscription to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [MessageDequeued] MessageId = 0 due to subscription to [1::'c']",
                        "[2::'test2'::'c'::<ROOT>] [Disposing]",
                        "[2::'test2'::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }
}
