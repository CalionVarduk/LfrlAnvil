using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerStreamTests : TestsBase
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
                .SetStreamEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerStreamEventType.MessageDequeued )
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
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']",
                        "[1::'c'::2] [MessageEnqueued] MessageId = 0 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 0 by binding [1::'test'] => [1::'c']",
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::3] [MessageEnqueued] MessageId = 1 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 1 by binding [1::'test'] => [1::'c']",
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::4] [MessageEnqueued] MessageId = 2 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 2 by binding [1::'test'] => [1::'c']"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task MessageRequest_ShouldEnqueueMessagesCorrectly_WithDifferentChannels()
    {
        var enqueueEndSource = new SafeTaskCompletionSource( completionCount: 5 );
        var endSource = new SafeTaskCompletionSource( completionCount: 10 );
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
                .SetStreamEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerStreamEventType.MessageEnqueued )
                            enqueueEndSource.Complete();
                        else if ( e.Type == MessageBrokerStreamEventType.MessageDequeued )
                        {
                            enqueueEndSource.Task.Wait();
                            endSource.Complete();
                        }
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindRequest( "c" );
                c.ReadBoundResponse();
                c.SendBindRequest( "d", "c" );
                c.ReadBoundResponse();
                c.SendBindRequest( "e", "c" );
                c.ReadBoundResponse();
                c.SendMessageRequest( 1, [ 1 ] );
                c.SendMessageRequest( 2, [ 2, 3 ] );
                c.SendMessageRequest( 3, [ 4, 5, 6 ] );
                c.SendMessageRequest( 3, [ 7, 8, 9, 10 ] );
                c.SendMessageRequest( 1, [ 11, 12, 13, 14, 15 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
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
                        "[1::'test'::4] [MessageReceived] [PacketLength: 10] Begin handling MessageRequest",
                        "[1::'test'::4] [MessageAccepted] [PacketLength: 10] MessageRequest",
                        "[1::'test'::4] [SendingMessage] [PacketLength: 13] MessageAcceptedResponse",
                        "[1::'test'::4] [MessageSent] [PacketLength: 13] MessageAcceptedResponse"
                    ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 11] MessageRequest",
                        "[1::'test'::5] [MessageReceived] [PacketLength: 11] Begin handling MessageRequest",
                        "[1::'test'::5] [MessageAccepted] [PacketLength: 11] MessageRequest",
                        "[1::'test'::5] [SendingMessage] [PacketLength: 13] MessageAcceptedResponse",
                        "[1::'test'::5] [MessageSent] [PacketLength: 13] MessageAcceptedResponse"
                    ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 12] MessageRequest",
                        "[1::'test'::6] [MessageReceived] [PacketLength: 12] Begin handling MessageRequest",
                        "[1::'test'::6] [MessageAccepted] [PacketLength: 12] MessageRequest",
                        "[1::'test'::6] [SendingMessage] [PacketLength: 13] MessageAcceptedResponse",
                        "[1::'test'::6] [MessageSent] [PacketLength: 13] MessageAcceptedResponse"
                    ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 13] MessageRequest",
                        "[1::'test'::7] [MessageReceived] [PacketLength: 13] Begin handling MessageRequest",
                        "[1::'test'::7] [MessageAccepted] [PacketLength: 13] MessageRequest",
                        "[1::'test'::7] [SendingMessage] [PacketLength: 13] MessageAcceptedResponse",
                        "[1::'test'::7] [MessageSent] [PacketLength: 13] MessageAcceptedResponse"
                    ] ),
                logs.GetAllClient()
                    .TestContainsSequence(
                    [
                        "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 14] MessageRequest",
                        "[1::'test'::8] [MessageReceived] [PacketLength: 14] Begin handling MessageRequest",
                        "[1::'test'::8] [MessageAccepted] [PacketLength: 14] MessageRequest",
                        "[1::'test'::8] [SendingMessage] [PacketLength: 13] MessageAcceptedResponse",
                        "[1::'test'::8] [MessageSent] [PacketLength: 13] MessageAcceptedResponse"
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']",
                        "[1::'c'::4] [MessageEnqueued] MessageId = 0 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 0 by binding [1::'test'] => [1::'c']",
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::5] [MessageEnqueued] MessageId = 1 by binding [1::'test'] => [2::'d']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 1 by binding [1::'test'] => [2::'d']",
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::6] [MessageEnqueued] MessageId = 2 by binding [1::'test'] => [3::'e']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 2 by binding [1::'test'] => [3::'e']"
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::7] [MessageEnqueued] MessageId = 3 by binding [1::'test'] => [3::'e']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 3 by binding [1::'test'] => [3::'e']"
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::8] [MessageEnqueued] MessageId = 4 by binding [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 4 by binding [1::'test'] => [1::'c']"
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
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

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
                logs.GetAllStream()
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
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

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
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by binding [1::'test'] => [1::'c']" ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamProcessing_ShouldDisposeStreamAutomatically_WhenStreamIsEmptyAndNoBindingsAreRelated()
    {
        var endSource = new SafeTaskCompletionSource();
        var bindingDisposedSource = new SafeTaskCompletionSource<MessageBrokerStreamState>();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetChannelBindingEventHandlerFactory(
                    _ => e =>
                    {
                        if ( e.Type == MessageBrokerChannelBindingEventType.Disposed )
                            bindingDisposedSource.Complete( e.Binding.Stream.State );
                    } )
                .SetStreamEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerStreamEventType.MessageDequeued )
                            bindingDisposedSource.Task.Wait();
                        else if ( e.Type == MessageBrokerStreamEventType.Disposed )
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

        var streamStateOnBindingDisposed = await bindingDisposedSource.Task;
        await endSource.Task;

        Assertion.All(
                streamStateOnBindingDisposed.TestEquals( MessageBrokerStreamState.Running ),
                server.Streams.Count.TestEquals( 0 ),
                server.Streams.GetAll().TestEmpty(),
                logs.GetAllStream()
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
