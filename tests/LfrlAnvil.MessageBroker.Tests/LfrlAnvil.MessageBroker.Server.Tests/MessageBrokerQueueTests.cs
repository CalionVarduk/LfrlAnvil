using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerQueueTests : TestsBase
{
    [Fact]
    public async Task MessageRequest_ShouldEnqueueMessagesCorrectly()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 6 );
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
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.MessageAcceptedResponse )
                            endSource.Complete();
                    } )
                .SetQueueEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerQueueEventType.MessageDequeued )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
                c.SendMessageRequest( 1, [ 1 ] );
                c.SendMessageRequest( 1, [ 2, 3 ] );
                c.SendMessageRequest( 1, [ 4, 5, 6 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 10] MessageRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 10] Begin handling MessageRequest",
                        "[1::'test'::2] [MessageAccepted] [PacketLength: 10] MessageRequest",
                        "[1::'test'::2] [SendingMessage] [PacketLength: 13] MessageAcceptedResponse",
                        "[1::'test'::2] [MessageSent] [PacketLength: 13] MessageAcceptedResponse"
                    ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] MessageRequest",
                        "[1::'test'::3] [MessageReceived] [PacketLength: 11] Begin handling MessageRequest",
                        "[1::'test'::3] [MessageAccepted] [PacketLength: 11] MessageRequest",
                        "[1::'test'::3] [SendingMessage] [PacketLength: 13] MessageAcceptedResponse",
                        "[1::'test'::3] [MessageSent] [PacketLength: 13] MessageAcceptedResponse"
                    ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 12] MessageRequest",
                        "[1::'test'::4] [MessageReceived] [PacketLength: 12] Begin handling MessageRequest",
                        "[1::'test'::4] [MessageAccepted] [PacketLength: 12] MessageRequest",
                        "[1::'test'::4] [SendingMessage] [PacketLength: 13] MessageAcceptedResponse",
                        "[1::'test'::4] [MessageSent] [PacketLength: 13] MessageAcceptedResponse"
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']",
                        "[1::'c'::2] [MessageEnqueued] MessageId = 0 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 0 by binding [1::'test'] => [1::'c']",
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'c'::3] [MessageEnqueued] MessageId = 1 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 1 by binding [1::'test'] => [1::'c']",
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'c'::4] [MessageEnqueued] MessageId = 2 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 2 by binding [1::'test'] => [1::'c']"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task MessageRequest_ShouldDisposeClient_WhenClientSendsInvalidPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
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
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Bindings.TryGetByChannelId( 1 );
        await client.GetTask( c => c.SendMessageRequest( 1, [ ], payload: 3 ) );

        await endSource.Task;

        Assertion.All(
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Clients.GetAll().TestEmpty(),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 8] MessageRequest",
                        "[1::'test'::2] [MessageReceived] [PacketLength: 8] Begin handling MessageRequest",
                        """
                        [1::'test'::2] [MessageRejected] [PacketLength: 8] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid MessageRequest from client [1] 'test'. Encountered 1 error(s):
                        1. Expected header payload to be at least 4 but found 3.
                        """
                    ] ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task MessageRequest_ShouldBeRejected_WhenChannelDoesNotExist()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageRejected
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.MessageRequest )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.MessageRejectedResponse )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );

        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendMessageRequest( 1, [ ] );
                c.ReadMessageRejectedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelBindingException>(
                        exc => Assertion.All( exc.Client.TestRefEquals( remoteClient ), exc.Channel.TestNull(), exc.Binding.TestNull() ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( remoteClient ) ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 9] MessageRequest",
                        "[1::'test'::1] [MessageReceived] [PacketLength: 9] Begin handling MessageRequest",
                        """
                        [1::'test'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelBindingException: Message broker client [1] 'test' could not add message to channel with ID 1 because it is not bound to it.
                        """,
                        "[1::'test'::1] [SendingMessage] [PacketLength: 6] MessageRejectedResponse",
                        "[1::'test'::1] [MessageSent] [PacketLength: 6] MessageRejectedResponse"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task MessageRequest_ShouldBeRejected_WhenClientIsNotBoundToChannel()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetClientEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageRejected
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.MessageRequest )
                            exception = e.Exception;

                        if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.MessageRejectedResponse )
                            endSource.Complete();
                    } )
                .SetQueueEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendMessageRequest( 1, [ ] );
                c.ReadMessageRejectedResponse();
            } );

        await endSource.Task;

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelBindingException>(
                        exc => Assertion.All( exc.Client.TestRefEquals( remoteClient2 ), exc.Channel.TestNull(), exc.Binding.TestNull() ) ),
                remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 2 ),
                server.Clients.GetAll().TestSetEqual( [ remoteClient1, remoteClient2 ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[2::'test2'::<ROOT>] [MessageReceived] [PacketLength: 9] MessageRequest",
                        "[2::'test2'::1] [MessageReceived] [PacketLength: 9] Begin handling MessageRequest",
                        """
                        [2::'test2'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                        LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelBindingException: Message broker client [2] 'test2' could not add message to channel with ID 1 because it is not bound to it.
                        """,
                        "[2::'test2'::1] [SendingMessage] [PacketLength: 6] MessageRejectedResponse",
                        "[2::'test2'::1] [MessageSent] [PacketLength: 6] MessageRejectedResponse"
                    ] ),
                logs.GetAllQueue().TestSequence( [ "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']" ] ) )
            .Go();
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveQueueAndRelatedBindingsCorrectly()
    {
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelEventHandlerFactory( _ => logs.Add )
                .SetChannelBindingEventHandlerFactory( _ => logs.Add )
                .SetQueueEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );
        var queue = server.Queues.TryGetById( 1 );
        if ( queue is not null )
            await queue.RemoveAsync();

        Assertion.All(
                server.Clients.Count.TestEquals( 1 ),
                server.Channels.Count.TestEquals( 0 ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                        q.Bindings.Count.TestEquals( 0 ),
                        q.Bindings.GetAll().TestEmpty() ) ),
                channel.TestNotNull(
                    ch => Assertion.All(
                        "channel",
                        ch.State.TestEquals( MessageBrokerChannelState.Disposed ),
                        ch.Bindings.Count.TestEquals( 0 ),
                        ch.Bindings.GetAll().TestEmpty() ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
                logs.GetAllQueue()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task RemoveAsync_ShouldDoNothing_WhenQueueIsAlreadyDisposed()
    {
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
            } );

        var action = Lambda.Of( () => ValueTask.FromException( new Exception() ) );
        var queue = server.Queues.TryGetById( 1 );
        if ( queue is not null )
        {
            await queue.RemoveAsync();
            action = Lambda.Of( () => queue.RemoveAsync() );
        }

        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveQueueAndRelatedBindingsCorrectly_AlongWithAnyQueuedMessages()
    {
        var dequeueCount = Ref.Create( new InterlockedInt32( 0 ) );
        var endSource = new SafeTaskCompletionSource<Task>();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetQueueEventHandlerFactory(
                    _ => e =>
                    {
                        if ( e.Type == MessageBrokerQueueEventType.MessageDequeued )
                        {
                            if ( dequeueCount.Value.Increment() == 1 )
                            {
                                logs.Add( e );
                                endSource.Complete( e.Queue.RemoveAsync().AsTask() );
                            }
                            else
                                endSource.Task.Wait();
                        }
                        else
                            logs.Add( e );
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
                c.SendMessageRequest( 1, [ 1 ] );
                c.SendMessageRequest( 1, [ 1, 2 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
            } );

        await endSource.Task.Unwrap();

        Assertion.All(
                server.Queues.Count.TestEquals( 0 ),
                server.Queues.GetAll().TestEmpty(),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']",
                        "[1::'c'::2] [MessageEnqueued] MessageId = 0 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 0 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ),
                logs.GetAllQueue()
                    .TestContainsSequence( [ "[1::'c'::3] [MessageEnqueued] MessageId = 1 by binding [1::'test'] => [1::'c']" ] ) )
            .Go();
    }

    [Fact]
    public async Task QueueProcessing_ShouldDisposeQueueAutomatically_WhenQueueIsEmptyAndNoBindingsAreRelated()
    {
        var endSource = new SafeTaskCompletionSource();
        var bindingDisposedSource = new SafeTaskCompletionSource<MessageBrokerQueueState>();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelBindingEventHandlerFactory(
                    _ => e =>
                    {
                        if ( e.Type == MessageBrokerChannelBindingEventType.Disposed )
                            bindingDisposedSource.Complete( e.Binding.Queue.State );
                    } )
                .SetQueueEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerQueueEventType.MessageDequeued )
                            bindingDisposedSource.Task.Wait();
                        else if ( e.Type == MessageBrokerQueueEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
                c.SendMessageRequest( 1, [ 1 ] );
                c.SendMessageRequest( 1, [ 1, 2 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
                c.SendUnbindRequest( 1 );
                c.ReadUnboundResponse();
            } );

        var queueStateOnBindingDisposed = await bindingDisposedSource.Task;
        await endSource.Task;

        Assertion.All(
                queueStateOnBindingDisposed.TestEquals( MessageBrokerQueueState.Running ),
                server.Queues.Count.TestEquals( 0 ),
                server.Queues.GetAll().TestEmpty(),
                logs.GetAllQueue()
                    .TestContainsSequence(
                    [
                        "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']",
                        "[1::'c'::2] [MessageEnqueued] MessageId = 0 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 0 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }
}
