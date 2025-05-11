using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests
{
    public class Listener : TestsBase
    {
        [Theory]
        [InlineData( true, true )]
        [InlineData( true, false )]
        [InlineData( false, true )]
        [InlineData( false, false )]
        public async Task BindAsync_ShouldCreateListenerCorrectly(bool channelCreated, bool queueCreated)
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();
            MessageBrokerListenerCallback callback = (_, _) => ValueTask.CompletedTask;

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var queueName = "bar";
            var bindRequest = new Protocol.BindListenerRequest(
                channelName,
                createChannelIfNotExists: true,
                queueName: queueName,
                prefetchHint: 1 );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendListenerBoundResponse( channelCreated, queueCreated, 1, 2 );
                } );

            var result = await client.Listeners.BindAsync( channelName, callback, queueName );
            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.AlreadyBound.TestFalse(),
                            r.ChannelCreated.TestEquals( channelCreated ),
                            r.QueueCreated.TestEquals( queueCreated ),
                            r.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                            r.ToString()
                                .TestEquals(
                                    channelCreated
                                        ? queueCreated
                                            ? $"[1] 'test' => [1] '{channelName}' listener (using [2] '{queueName}' queue) (Bound) (channel created) (queue created)"
                                            : $"[1] 'test' => [1] '{channelName}' listener (using [2] '{queueName}' queue) (Bound) (channel created)"
                                        : queueCreated
                                            ? $"[1] 'test' => [1] '{channelName}' listener (using [2] '{queueName}' queue) (Bound) (queue created)"
                                            : $"[1] 'test' => [1] '{channelName}' listener (using [2] '{queueName}' queue) (Bound)" ) ) ),
                    client.Listeners.Count.TestEquals( 1 ),
                    client.Listeners.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ) ] ),
                    client.Listeners.TryGetByChannelName( channelName ).TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                    client.Listeners.TryGetByChannelId( 1 )
                        .TestNotNull(
                            listener => Assertion.All(
                                "listener",
                                listener.Client.TestRefEquals( client ),
                                listener.ChannelId.TestEquals( 1 ),
                                listener.ChannelName.TestEquals( channelName ),
                                listener.QueueId.TestEquals( 2 ),
                                listener.QueueName.TestEquals( queueName ),
                                listener.PrefetchHint.TestEquals( 1 ),
                                listener.Callback.TestRefEquals( callback ),
                                listener.State.TestEquals( MessageBrokerListenerState.Bound ),
                                listener.ToString()
                                    .TestEquals(
                                        $"[1] 'test' => [1] '{channelName}' listener (using [2] '{queueName}' queue) (Bound)" ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 20] BindListenerRequest",
                            "['test'::1] [MessageSent] [PacketLength: 20] BindListenerRequest",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 14] ListenerBoundResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 14] Begin handling ListenerBoundResponse",
                            "['test'::1] [MessageAccepted] [PacketLength: 14] ListenerBoundResponse (ChannelId = 1, QueueId = 2)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldNotThrow_WhenListenerIsAlreadyLocallyBound()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var bindRequest = new Protocol.BindListenerRequest(
                channelName,
                createChannelIfNotExists: true,
                queueName: null,
                prefetchHint: 1 );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                } );

            await client.Listeners.BindAsync( channelName, (_, _) => ValueTask.CompletedTask );
            await serverTask;

            var result = await client.Listeners.BindAsync(
                channelName,
                (_, _) => ValueTask.CompletedTask,
                createChannelIfNotExists: false );

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.AlreadyBound.TestTrue(),
                            r.ChannelCreated.TestFalse(),
                            r.QueueCreated.TestFalse(),
                            r.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                            r.ToString()
                                .TestEquals(
                                    $"[1] 'test' => [1] '{channelName}' listener (using [1] '{channelName}' queue) (Bound) (already bound)" ) ) ),
                    client.Listeners.Count.TestEquals( 1 ) )
                .Go();
        }

        [Fact]
        public async Task ClientDispose_ShouldMarkAllListenersAsDisposed()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var bindRequest = new Protocol.BindListenerRequest(
                channelName,
                createChannelIfNotExists: true,
                queueName: null,
                prefetchHint: 1 );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                } );

            var result = await client.Listeners.BindAsync( channelName, (_, _) => ValueTask.CompletedTask );
            await serverTask;
            await client.DisposeAsync();

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull( r => r.Listener.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                    client.Listeners.Count.TestEquals( 0 ),
                    client.Listeners.GetAll().TestEmpty() )
                .Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenChannelNameIsEmpty()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( string.Empty, (_, _) => ValueTask.CompletedTask ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenQueueNameIsNotNullAndEmpty()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask, string.Empty ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenChannelNameIsTooLong()
        {
            var name = new string( 'x', 513 );
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( name, (_, _) => ValueTask.CompletedTask ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenQueueNameIsNotNullAndTooLong()
        {
            var name = new string( 'x', 513 );
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask, name ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenPrefetchHintIsLessThanOne()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask, prefetchHint: 0 ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowMessageBrokerClientStateException_WhenClientIsNotRunning()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask ) );
            action.Test(
                    exc => exc.TestType()
                        .Exact<MessageBrokerClientStateException>(
                            e => Assertion.All(
                                e.Client.TestRefEquals( client ),
                                e.Expected.TestEquals( MessageBrokerClientState.Running ),
                                e.Actual.TestEquals( MessageBrokerClientState.Created ) ) ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                _ = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            }
            catch ( Exception exc )
            {
                exception = exc;
            }

            exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerDoesNotRespondInTime()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.BindListenerRequest ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 17] BindListenerRequest",
                            "['test'::1] [MessageSent] [PacketLength: 17] BindListenerRequest",
                            """
                            ['test'::<ROOT>] [WaitingForMessage] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Message broker server failed to respond to 'test' client's BindListenerRequest in the specified amount of time (1000 milliseconds).
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnError_WhenClientIsDisposedBeforeServerResponds()
        {
            var endSource = new SafeTaskCompletionSource<Task>();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler(
                        e =>
                        {
                            if ( e.Type == MessageBrokerClientEventType.SendingMessage
                                && e.GetServerEndpoint() == MessageBrokerServerEndpoint.Ping )
                                endSource.Complete( e.Client.DisposeAsync().AsTask() );
                        } ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await endSource.Task.Unwrap();

            Assertion.All(
                    result.Value.TestNull(),
                    result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithListenerBoundResponseWithInvalidValues()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindListenerRequest(
                "foo",
                createChannelIfNotExists: true,
                queueName: null,
                prefetchHint: 1 );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendListenerBoundResponse( true, true, channelId: 0, queueId: -1 );
                } );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.ListenerBoundResponse ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 14] ListenerBoundResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 14] Begin handling ListenerBoundResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 14] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid ListenerBoundResponse from the server. Encountered 2 error(s):
                            1. Expected channel ID to be greater than 0 but found 0.
                            2. Expected queue ID to be greater than 0 but found -1.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithListenerBoundResponseWithInvalidPayload()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindListenerRequest(
                "foo",
                createChannelIfNotExists: true,
                queueName: null,
                prefetchHint: 1 );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendListenerBoundResponse( true, true, 1, 1, payload: 8 );
                } );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.ListenerBoundResponse ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 13] ListenerBoundResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 13] Begin handling ListenerBoundResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 13] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid ListenerBoundResponse from the server. Encountered 1 error(s):
                            1. Expected header payload to be 9 but found 8.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnError_WhenServerRespondsWithBindListenerFailureResponse()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindListenerRequest(
                "foo",
                createChannelIfNotExists: true,
                queueName: null,
                prefetchHint: 1 );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendBindListenerFailureResponse( true, true, true );
                } );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.BindListenerRequest ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] BindListenerFailureResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 6] Begin handling BindListenerFailureResponse",
                            """
                            ['test'::1] [MessageReceived] [PacketLength: 6] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Message broker server rejected an invalid BindListenerRequest sent by client 'test'. Encountered 3 error(s):
                            1. Channel 'foo' does not exist.
                            2. Client is already bound as a listener to channel 'foo'.
                            3. Binding client to channel 'foo' as a listener has been cancelled by the server.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithBindListenerFailureResponseWithInvalidPayload()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindListenerRequest(
                "foo",
                createChannelIfNotExists: true,
                queueName: null,
                prefetchHint: 1 );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendBindListenerFailureResponse( true, true, true, payload: 0 );
                } );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.BindListenerFailureResponse ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] BindListenerFailureResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 5] Begin handling BindListenerFailureResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid BindListenerFailureResponse from the server. Encountered 1 error(s):
                            1. Expected header payload to be 1 but found 0.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithInvalidEndpoint()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindListenerRequest(
                "foo",
                createChannelIfNotExists: true,
                queueName: null,
                prefetchHint: 1 );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] <unrecognized-endpoint-0>",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                            1. Received unexpected client endpoint.
                            """
                        ] ) )
                .Go();
        }

        [Theory]
        [InlineData( true, true )]
        [InlineData( false, true )]
        [InlineData( true, false )]
        [InlineData( false, false )]
        public async Task UnbindAsync_ShouldUnbindListenerCorrectly(bool channelRemoved, bool queueRemoved)
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            var channelId = 1;
            var channelName = "foo";
            await server.EstablishHandshake( client );
            var bindRequest = new Protocol.BindListenerRequest(
                channelName,
                createChannelIfNotExists: true,
                queueName: null,
                prefetchHint: 1 );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendListenerBoundResponse( true, true, channelId, 2 );
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( channelRemoved, queueRemoved );
                } );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            await client.Listeners.BindAsync( channelName, (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( channelId );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestFalse(),
                    result.Value.ChannelRemoved.TestEquals( channelRemoved ),
                    result.Value.QueueRemoved.TestEquals( queueRemoved ),
                    result.Value.ToString()
                        .TestEquals(
                            channelRemoved
                                ? queueRemoved ? "Success (channel removed) (queue removed)" : "Success (channel removed)"
                                : queueRemoved
                                    ? "Success (queue removed)"
                                    : "Success" ),
                    listener.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                    client.Listeners.Count.TestEquals( 0 ),
                    client.Listeners.GetAll().TestEmpty(),
                    client.Listeners.TryGetByChannelName( channelName ).TestNull(),
                    client.Listeners.TryGetByChannelId( channelId ).TestNull(),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::2] [SendingMessage] [PacketLength: 9] UnbindListenerRequest (ChannelId = 1, ChannelName = 'foo')",
                            "['test'::2] [MessageSent] [PacketLength: 9] UnbindListenerRequest",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] ListenerUnboundResponse",
                            "['test'::2] [MessageReceived] [PacketLength: 6] Begin handling ListenerUnboundResponse",
                            "['test'::2] [MessageAccepted] [PacketLength: 6] ListenerUnboundResponse"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldNotThrow_WhenListenerIsAlreadyLocallyDisposed()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var bindRequest = new Protocol.BindListenerRequest(
                channelName,
                createChannelIfNotExists: true,
                queueName: null,
                prefetchHint: 1 );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true );
                } );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            await client.Listeners.BindAsync( channelName, (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( 1 );
            if ( listener is not null )
            {
                await listener.UnbindAsync();
                result = await listener.UnbindAsync();
            }

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestTrue(),
                    result.Value.ChannelRemoved.TestFalse(),
                    result.Value.ToString().TestEquals( "Not bound" ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var listener = new MessageBrokerListener( client, 1, "foo", 1, "foo", 1, (_, _) => ValueTask.CompletedTask );
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                _ = await listener.UnbindAsync();
            }
            catch ( Exception exc )
            {
                exception = exc;
            }

            exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerDoesNotRespondInTime()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            var listener = client.Listeners.TryGetByChannelId( 1 );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.UnbindListenerRequest ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::2] [SendingMessage] [PacketLength: 9] UnbindListenerRequest (ChannelId = 1, ChannelName = 'foo')",
                            "['test'::2] [MessageSent] [PacketLength: 9] UnbindListenerRequest",
                            """
                            ['test'::<ROOT>] [WaitingForMessage] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Message broker server failed to respond to 'test' client's UnbindListenerRequest in the specified amount of time (1000 milliseconds).
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnError_WhenClientIsDisposedBeforeServerResponds()
        {
            var listenerBound = Ref.Create( false );
            var endSource = new SafeTaskCompletionSource<Task>();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler(
                        e =>
                        {
                            bool bound;
                            lock ( listenerBound )
                                bound = listenerBound.Value;

                            if ( bound
                                && e.Type == MessageBrokerClientEventType.SendingMessage
                                && e.GetServerEndpoint() == MessageBrokerServerEndpoint.Ping )
                                endSource.Complete( e.Client.DisposeAsync().AsTask() );
                        } ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;
            var listener = client.Listeners.TryGetByChannelId( 1 );
            lock ( listenerBound )
                listenerBound.Value = true;

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await endSource.Task.Unwrap();

            result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithListenerUnboundResponseWithInvalidPayload()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true, payload: 0 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.ListenerUnboundResponse ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] ListenerUnboundResponse",
                            "['test'::2] [MessageReceived] [PacketLength: 5] Begin handling ListenerUnboundResponse",
                            """
                            ['test'::2] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid ListenerUnboundResponse from the server. Encountered 1 error(s):
                            1. Expected header payload to be 1 but found 0.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnError_WhenServerRespondsWithUnbindListenerFailureResponse()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindListenerRequest();
                    s.SendUnbindListenerFailureResponse( true );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.UnbindListenerRequest ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] UnbindListenerFailureResponse",
                            "['test'::2] [MessageReceived] [PacketLength: 6] Begin handling UnbindListenerFailureResponse",
                            """
                            ['test'::2] [MessageReceived] [PacketLength: 6] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Message broker server rejected an invalid UnbindListenerRequest sent by client 'test'. Encountered 1 error(s):
                            1. Client is not bound as a listener to channel [1] 'foo'.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task
            UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithUnbindListenerFailureResponseWithInvalidPayload()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindListenerRequest();
                    s.SendUnbindListenerFailureResponse( true, payload: 0 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.UnbindListenerFailureResponse ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] UnbindListenerFailureResponse",
                            "['test'::2] [MessageReceived] [PacketLength: 5] Begin handling UnbindListenerFailureResponse",
                            """
                            ['test'::2] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid UnbindListenerFailureResponse from the server. Encountered 1 error(s):
                            1. Expected header payload to be 1 but found 0.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithInvalidEndpoint()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindListenerRequest();
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            var listener = client.Listeners.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
            if ( listener is not null )
                result = await listener.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] <unrecognized-endpoint-0>",
                            """
                            ['test'::2] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                            1. Received unexpected client endpoint.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_ShouldInvokeCallback()
        {
            var endSource = new SafeTaskCompletionSource<(MessageBrokerListenerCallbackArgs Args, byte[] Message)>();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            var enqueuedAt = client.GetTimestamp();
            var message = new byte[] { 1, 2, 3, 4 };

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 2, 1, 3, message, enqueuedAt, 4, 5 );
                } );

            await client.Listeners.BindAsync(
                "foo",
                (a, _) =>
                {
                    endSource.Complete( (a, a.Data.ToArray()) );
                    return ValueTask.CompletedTask;
                } );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            await serverTask;
            var (args, caughtMessage) = await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    args.Listener.TestRefEquals( listener ),
                    args.EnqueuedAt.TestEquals( enqueuedAt ),
                    args.ReceivedAt.TestGreaterThanOrEqualTo( args.EnqueuedAt ),
                    args.MessageId.TestEquals( 1UL ),
                    args.SenderId.TestEquals( 2 ),
                    args.StreamId.TestEquals( 3 ),
                    args.RetryAttempt.TestEquals( 4 ),
                    args.RedeliveryAttempt.TestEquals( 5 ),
                    caughtMessage.TestSequence( message ),
                    args.ToString()
                        .TestEquals(
                            $"Listener = ([1] 'test' => [1] 'foo' listener (using [1] 'foo' queue) (Bound)), Id = 1, Retry = 4, Redelivery = 5, Length = 4, EnqueuedAt = {args.EnqueuedAt}, ReceivedAt = {args.ReceivedAt}, Sender = 2, Stream = 3" ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 45] MessageNotification",
                            "['test'::2] [MessageReceived] [PacketLength: 45] Begin handling MessageNotification",
                            "['test'::2] [MessageAccepted] [PacketLength: 45] MessageNotification"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_ShouldWaitWithCallbackInvocationUntilPreviousInvocationFinishes()
        {
            var continuationSource = new SafeTaskCompletionSource( completionCount: 2 );
            var endSourceIndex = 0;
            var endSources = new[]
            {
                new SafeTaskCompletionSource<(MessageBrokerListenerCallbackArgs Args, byte[] Message)>(),
                new SafeTaskCompletionSource<(MessageBrokerListenerCallbackArgs Args, byte[] Message)>()
            };

            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.MessageAccepted
                                && e.GetClientEndpoint() == MessageBrokerClientEndpoint.MessageNotification )
                                continuationSource.Complete();
                        } ) );

            var message1 = new byte[] { 1, 2, 3, 4 };
            var message2 = new byte[] { 5, 6, 7, 8, 9 };

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 2, 1, 1, message1 );
                    s.SendMessageNotification( 2, 3, 1, 2, message2 );
                } );

            await client.Listeners.BindAsync(
                "foo",
                async (a, ct) =>
                {
                    var endSource = endSources[endSourceIndex++];
                    if ( endSourceIndex == 1 )
                    {
                        await continuationSource.Task;
                        await Task.Delay( 15, ct );
                    }

                    endSource.Complete( (a, a.Data.ToArray()) );
                } );

            var listener = client.Listeners.TryGetByChannelId( 1 );
            await serverTask;
            var (args1, caughtMessage1) = await endSources[0].Task;
            var (args2, caughtMessage2) = await endSources[1].Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    args1.Listener.TestRefEquals( listener ),
                    args1.MessageId.TestEquals( 1UL ),
                    args1.SenderId.TestEquals( 2 ),
                    args1.StreamId.TestEquals( 1 ),
                    caughtMessage1.TestSequence( message1 ),
                    args2.Listener.TestRefEquals( listener ),
                    args2.MessageId.TestEquals( 2UL ),
                    args2.SenderId.TestEquals( 3 ),
                    args2.StreamId.TestEquals( 2 ),
                    caughtMessage2.TestSequence( message2 ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 45] MessageNotification",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 46] MessageNotification"
                        ] ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::2] [MessageReceived] [PacketLength: 45] Begin handling MessageNotification",
                            "['test'::2] [MessageAccepted] [PacketLength: 45] MessageNotification",
                            "['test'::3] [MessageReceived] [PacketLength: 46] Begin handling MessageNotification",
                            "['test'::3] [MessageAccepted] [PacketLength: 46] MessageNotification"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_WhoseCallbackThrows_ShouldBeLogged()
        {
            var endSource = new SafeTaskCompletionSource();
            var exception = new Exception( "foo" );
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.Unexpected )
                                endSource.Complete();
                        } ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 2, 1, 1, [ ] );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => throw exception );
            await serverTask;
            var listener = client.Listeners.TryGetByChannelId( 1 );
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    listener.TestNotNull( l => l.State.TestEquals( MessageBrokerListenerState.Bound ) ),
                    logs.GetAll()
                        .TestAny(
                            (l, _) => l.TestStartsWith(
                                """
                                ['test'::<ROOT>] [Unexpected] Encountered an error:
                                System.Exception: foo
                                """ ) ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_WithInvalidPayload_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.Disposed )
                                endSource.Complete();
                        } ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 2, 1, 1, [ 1 ], payload: 35 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 40] MessageNotification",
                            "['test'::2] [MessageReceived] [PacketLength: 40] Begin handling MessageNotification",
                            """
                            ['test'::2] [MessageRejected] [PacketLength: 40] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid MessageNotification from the server. Encountered 1 error(s):
                            1. Expected header payload to be at least 36 but found 35.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_WithInvalidProperties_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.Disposed )
                                endSource.Complete();
                        } ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 0, -2, 0, -3, [ 1 ], retryAttempt: -4, redeliveryAttempt: -5 );
                } );

            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 42] MessageNotification",
                            "['test'::2] [MessageReceived] [PacketLength: 42] Begin handling MessageNotification",
                            """
                            ['test'::2] [MessageRejected] [PacketLength: 42] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid MessageNotification from the server. Encountered 5 error(s):
                            1. Expected sender ID to not be negative but found -2.
                            2. Expected channel ID to be greater than 0 but found 0.
                            3. Expected stream ID to not be negative but found -3.
                            4. Expected retry attempt to not be negative but found -4.
                            5. Expected redelivery attempt to not be negative but found -5.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageNotification_WithNonExistingListener_ShouldBeIgnored()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.MessageRejected
                                && e.GetClientEndpoint() == MessageBrokerClientEndpoint.MessageNotification )
                                endSource.Complete();
                        } ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask( s => { s.SendMessageNotification( 1, 1, 1, 1, [ 1, 2, 3, 4 ] ); } );

            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 45] MessageNotification",
                            "['test'::1] [MessageReceived] [PacketLength: 45] Begin handling MessageNotification",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 45] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid MessageNotification from the server. Encountered 1 error(s):
                            1. Listener for channel with ID 1 does not exist.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Disposal_ShouldDiscardPendingMessages()
        {
            var messageReceivedContinuation = new SafeTaskCompletionSource( completionCount: 3 );
            var callbackContinuation = new SafeTaskCompletionSource( completionCount: 3 );
            var unboundContinuation = new SafeTaskCompletionSource();

            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.MessageReceived )
                            {
                                if ( e.GetClientEndpoint() == MessageBrokerClientEndpoint.MessageNotification )
                                {
                                    if ( e.IsRootContext )
                                        messageReceivedContinuation.Complete();
                                    else
                                        messageReceivedContinuation.Task.Wait();
                                }
                            }
                            else if ( e.Type == MessageBrokerClientEventType.MessageAccepted )
                            {
                                if ( e.GetClientEndpoint() == MessageBrokerClientEndpoint.MessageNotification )
                                {
                                    if ( callbackContinuation.Complete() )
                                        unboundContinuation.Task.Wait();
                                }
                                else if ( e.GetClientEndpoint() == MessageBrokerClientEndpoint.ListenerUnboundResponse )
                                    unboundContinuation.Complete();
                            }
                        } ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 2, 1, 1, [ 1, 2 ] );
                    s.SendMessageNotification( 2, 2, 1, 1, [ 3, 4, 5 ] );
                    s.SendMessageNotification( 3, 2, 1, 1, [ 6, 7, 8, 9 ] );
                } );

            await client.Listeners.BindAsync(
                "foo",
                async (a, ct) =>
                {
                    await callbackContinuation.Task;
                    _ = a.Listener.UnbindAsync().AsTask();
                } );

            await serverTask;
            await server.GetTask(
                s =>
                {
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true );
                } );

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 43] MessageNotification",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 44] MessageNotification",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 45] MessageNotification"
                        ] ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::2] [MessageReceived] [PacketLength: 43] Begin handling MessageNotification",
                            "['test'::2] [MessageAccepted] [PacketLength: 43] MessageNotification",
                            "['test'::3] [MessageReceived] [PacketLength: 44] Begin handling MessageNotification",
                            "['test'::3] [MessageAccepted] [PacketLength: 44] MessageNotification",
                            "['test'::4] [MessageReceived] [PacketLength: 45] Begin handling MessageNotification",
                            "['test'::4] [MessageAccepted] [PacketLength: 45] MessageNotification"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Disposal_ShouldCancelCallbackCancellationToken()
        {
            var unbindContinuation = new SafeTaskCompletionSource<MessageBrokerListener>();
            var cancellationSource = new SafeTaskCompletionSource();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 2, 1, 1, [ 1, 2, 3, 4 ] );
                } );

            await client.Listeners.BindAsync(
                "foo",
                async (a, ct) =>
                {
                    ct.UnsafeRegister( _ => cancellationSource.Complete(), null );
                    unbindContinuation.Complete( a.Listener );
                    await cancellationSource.Task;
                    ct.ThrowIfCancellationRequested();
                } );

            await serverTask;
            var listener = await unbindContinuation.Task;
            serverTask = server.GetTask(
                s =>
                {
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true );
                } );

            await listener.UnbindAsync();
            await serverTask;
            await cancellationSource.Task;

            Assertion.All( client.State.TestEquals( MessageBrokerClientState.Running ) ).Go();
        }

        [Fact]
        public async Task Disposal_ShouldLogCallbackCancellationTokenRegistrationException()
        {
            var exception = new Exception( "foo" );
            var unbindContinuation = new SafeTaskCompletionSource<MessageBrokerListener>();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 2, 1, 1, [ 1, 2, 3, 4 ] );
                } );

            await client.Listeners.BindAsync(
                "foo",
                (a, ct) =>
                {
                    ct.UnsafeRegister( _ => throw exception, null );
                    unbindContinuation.Complete( a.Listener );
                    return ValueTask.CompletedTask;
                } );

            await serverTask;
            var listener = await unbindContinuation.Task;
            serverTask = server.GetTask(
                s =>
                {
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true );
                } );

            await listener.UnbindAsync();
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    listener.TestNotNull( l => l.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                    logs.GetAll()
                        .TestAny(
                            (l, _) => l.TestStartsWith(
                                """
                                ['test'::<ROOT>] [Unexpected] Encountered an error:
                                System.AggregateException: One or more errors occurred. (foo)
                                 ---> System.Exception: foo
                                """ ) ) )
                .Go();
        }

        [Fact]
        public async Task Disposal_ShouldLogInvocationTimeoutAndStopWaiting()
        {
            var unbindContinuation = new SafeTaskCompletionSource<MessageBrokerListener>();
            var invocationContinuation = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetListenerDisposalTimeout( Duration.FromMilliseconds( 1 ) )
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.Unexpected )
                                invocationContinuation.Complete();
                        } ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindListenerRequest(
                        "foo",
                        createChannelIfNotExists: true,
                        queueName: null,
                        prefetchHint: 1 );

                    s.Read( request );
                    s.SendListenerBoundResponse( true, true, 1, 1 );
                    s.SendMessageNotification( 1, 2, 1, 1, [ 1, 2, 3, 4 ] );
                } );

            await client.Listeners.BindAsync(
                "foo",
                async (a, _) =>
                {
                    unbindContinuation.Complete( a.Listener );
                    await invocationContinuation.Task;
                } );

            await serverTask;
            var listener = await unbindContinuation.Task;
            serverTask = server.GetTask(
                s =>
                {
                    s.ReadUnbindListenerRequest();
                    s.SendListenerUnboundResponse( true, true );
                } );

            await listener.UnbindAsync();
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    listener.TestNotNull( l => l.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                    logs.GetAll()
                        .TestAny(
                            (l, _) => l.TestStartsWith(
                                """
                                ['test'::<ROOT>] [Unexpected] Encountered an error:
                                System.TimeoutException
                                """ ) ) )
                .Go();
        }
    }
}
