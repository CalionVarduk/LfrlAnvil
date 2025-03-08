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
        [InlineData( true )]
        [InlineData( false )]
        public async Task SubscribeAsync_ShouldCreateListenerCorrectly(bool channelCreated)
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            var handshakeRequest = await server.EstablishHandshake( client );

            var channelName = "foo";
            var expectedResultType = channelCreated
                ? MessageBrokerSubscriptionResult.ResultType.SubscribedAndChannelCreated
                : MessageBrokerSubscriptionResult.ResultType.Subscribed;

            var linkRequest = new Protocol.SubscribeRequest( channelName, createChannelIfNotExists: true );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendSubscribedResponse( channelCreated, 1 );
                } );

            var result = await client.Listeners.SubscribeAsync( channelName );
            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.Type.TestEquals( expectedResultType ),
                            r.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ) ) ),
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
                                listener.State.TestEquals( MessageBrokerListenerState.Listening ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 9] SubscribeRequest (ChannelName = 'foo')",
                            "['test'::1] [MessageSent] [PacketLength: 9] SubscribeRequest",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 10] SubscribedResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 10] Begin handling SubscribedResponse",
                            "['test'::1] [MessageAccepted] [PacketLength: 10] SubscribedResponse (ChannelId = 1)"
                        ] ),
                    AssertServerData(
                        server.GetAllReceived(),
                        (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse),
                        (linkRequest.Length, MessageBrokerServerEndpoint.SubscribeRequest) ) )
                .Go();
        }

        [Fact]
        public async Task SubscribeAsync_ShouldNotThrow_WhenListenerAlreadyLocallyExists()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var linkRequest = new Protocol.SubscribeRequest( channelName, createChannelIfNotExists: true );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendSubscribedResponse( true, 1 );
                } );

            await client.Listeners.SubscribeAsync( channelName );
            await serverTask;

            var result = await client.Listeners.SubscribeAsync( channelName, createChannelIfNotExists: false );

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.Type.TestEquals( MessageBrokerSubscriptionResult.ResultType.AlreadySubscribed ),
                            r.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ) ) ),
                    client.Listeners.Count.TestEquals( 1 ) )
                .Go();
        }

        [Fact]
        public async Task ClientDispose_ShouldMarkAllListenersAsDisposed()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            var client = new MessageBrokerClient(
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var linkRequest = new Protocol.SubscribeRequest( channelName, createChannelIfNotExists: true );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendSubscribedResponse( true, 1 );
                } );

            var result = await client.Listeners.SubscribeAsync( channelName );
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
        public void SubscribeAsync_ShouldThrowArgumentOutOfRangeException_WhenChannelNameIsEmpty()
        {
            using var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.SubscribeAsync( string.Empty ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void SubscribeAsync_ShouldThrowArgumentOutOfRangeException_WhenChannelNameIsTooLong()
        {
            var name = new string( 'x', 513 );
            using var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.SubscribeAsync( name ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void SubscribeAsync_ShouldThrowMessageBrokerClientStateException_WhenClientIsNotRunning()
        {
            using var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Listeners.SubscribeAsync( "foo" ) );
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
        public async Task SubscribeAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                _ = await client.Listeners.SubscribeAsync( "foo" );
            }
            catch ( Exception exc )
            {
                exception = exc;
            }

            exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task SubscribeAsync_ShouldReturnErrorAndDisposeClient_WhenServerDoesNotRespondInTime()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var result = await client.Listeners.SubscribeAsync( "foo" );

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.SubscribeRequest ),
                                exc.ResponseEndpoint.TestEquals( MessageBrokerClientEndpoint.SubscribedResponse ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 9] SubscribeRequest (ChannelName = 'foo')",
                            "['test'::1] [MessageSent] [PacketLength: 9] SubscribeRequest",
                            """
                            ['test'::<ROOT>] [WaitingForMessage] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Message broker server failed to respond with SubscribedResponse packet to 'test' client's SubscribeRequest request in the specified amount of time (1000 milliseconds).
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SubscribeAsync_ShouldReturnError_WhenClientIsDisposedBeforeServerResponds()
        {
            var endSource = new SafeTaskCompletionSource<Task>();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler(
                        e =>
                        {
                            if ( e.Type == MessageBrokerClientEventType.SendingMessage
                                && e.GetServerEndpoint() == MessageBrokerServerEndpoint.PingRequest )
                                endSource.Complete( e.Client.DisposeAsync().AsTask() );
                        } ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );

            var result = await client.Listeners.SubscribeAsync( "foo" );
            await endSource.Task.Unwrap();

            Assertion.All(
                    result.Value.TestNull(),
                    result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ) )
                .Go();
        }

        [Fact]
        public async Task SubscribeAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithSubscribedResponseWithInvalidChannelId()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var linkRequest = new Protocol.SubscribeRequest( "foo", createChannelIfNotExists: true );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendSubscribedResponse( true, channelId: 0 );
                } );

            var result = await client.Listeners.SubscribeAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.SubscribedResponse ),
                                exc.Payload.TestEquals( ( uint )Protocol.SubscribedResponse.Length ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 10] SubscribedResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 10] Begin handling SubscribedResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 10] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid SubscribedResponse with payload 5 from the server. Encountered 1 error(s):
                            1. Expected channel ID to be greater than 0 but found 0.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SubscribeAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithSubscribedResponseWithInvalidPayload()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var linkRequest = new Protocol.SubscribeRequest( "foo", createChannelIfNotExists: true );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendSubscribedResponse( true, 1, payload: 4 );
                } );

            var result = await client.Listeners.SubscribeAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.SubscribedResponse ),
                                exc.Payload.TestEquals( 4U ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 9] SubscribedResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 9] Begin handling SubscribedResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid SubscribedResponse with payload 4 from the server. Encountered 1 error(s):
                            1. Expected header payload to be 5.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SubscribeAsync_ShouldReturnError_WhenServerRespondsWithSubscribeFailureResponse()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var linkRequest = new Protocol.SubscribeRequest( "foo", createChannelIfNotExists: true );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendSubscribeFailureResponse( true, true, true );
                } );

            var result = await client.Listeners.SubscribeAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.SubscribeRequest ),
                                exc.Payload.TestEquals( linkRequest.Header.Payload ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] SubscribeFailureResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 6] Begin handling SubscribeFailureResponse",
                            """
                            ['test'::1] [MessageReceived] [PacketLength: 6] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Message broker server rejected an invalid SubscribeRequest with payload 4 sent by client 'test'. Encountered 3 error(s):
                            1. Channel 'foo' does not exist.
                            2. Client is already subscribed to channel 'foo'.
                            3. Subscribing client to channel 'foo' has been cancelled by the server.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task
            SubscribeAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithSubscribeFailureResponseWithInvalidPayload()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var linkRequest = new Protocol.SubscribeRequest( "foo", createChannelIfNotExists: true );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendSubscribeFailureResponse( true, true, true, payload: 0 );
                } );

            var result = await client.Listeners.SubscribeAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.SubscribeFailureResponse ),
                                exc.Payload.TestEquals( 0U ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] SubscribeFailureResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 5] Begin handling SubscribeFailureResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid SubscribeFailureResponse with payload 0 from the server. Encountered 1 error(s):
                            1. Expected header payload to be 1.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SubscribeAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithInvalidEndpoint()
        {
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var linkRequest = new Protocol.SubscribeRequest( "foo", createChannelIfNotExists: true );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            var result = await client.Listeners.SubscribeAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ),
                                exc.Payload.TestEquals( 0U ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] 0",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid 0 with payload 0 from the server. Encountered 1 error(s):
                            1. Received unexpected client endpoint.
                            """
                        ] ) )
                .Go();
        }

        // TODO: Unsubscribe
    }
}
