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
    public class Publisher : TestsBase
    {
        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public async Task BindAsync_ShouldBindChannelCorrectly(bool channelCreated)
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
            var bindRequest = new Protocol.BindRequest( channelName );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest.Length );
                    s.SendBoundResponse( channelCreated, 1 );
                } );

            var result = await client.Publishers.BindAsync( channelName );
            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.AlreadyBound.TestFalse(),
                            r.ChannelCreated.TestEquals( channelCreated ),
                            r.Publisher.TestRefEquals( client.Publishers.TryGetByChannelId( 1 ) ),
                            r.ToString()
                                .TestEquals(
                                    channelCreated
                                        ? $"[1] '{channelName}' publisher (Bound) (channel created)"
                                        : $"[1] '{channelName}' publisher (Bound)" ) ) ),
                    client.Publishers.Count.TestEquals( 1 ),
                    client.Publishers.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( client.Publishers.TryGetByChannelId( 1 ) ) ] ),
                    client.Publishers.TryGetByChannelName( channelName ).TestRefEquals( client.Publishers.TryGetByChannelId( 1 ) ),
                    client.Publishers.TryGetByChannelId( 1 )
                        .TestNotNull(
                            publisher => Assertion.All(
                                "publisher",
                                publisher.Client.TestRefEquals( client ),
                                publisher.ChannelId.TestEquals( 1 ),
                                publisher.ChannelName.TestEquals( channelName ),
                                publisher.State.TestEquals( MessageBrokerPublisherState.Bound ),
                                publisher.ToString().TestEquals( $"[1] '{channelName}' publisher (Bound)" ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 9] BindRequest (ChannelName = 'foo')",
                            "['test'::1] [MessageSent] [PacketLength: 9] BindRequest",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 10] BoundResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 10] Begin handling BoundResponse",
                            "['test'::1] [MessageAccepted] [PacketLength: 10] BoundResponse (Id = 1)"
                        ] ),
                    AssertServerData(
                        server.GetAllReceived(),
                        (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse),
                        (bindRequest.Length, MessageBrokerServerEndpoint.BindRequest) ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldNotThrow_WhenChannelIsAlreadyLocallyBound()
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
            var bindRequest = new Protocol.BindRequest( channelName );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest.Length );
                    s.SendBoundResponse( true, 1 );
                } );

            await client.Publishers.BindAsync( channelName );
            await serverTask;

            var result = await client.Publishers.BindAsync( channelName );

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.AlreadyBound.TestTrue(),
                            r.ChannelCreated.TestFalse(),
                            r.Publisher.TestRefEquals( client.Publishers.TryGetByChannelId( 1 ) ),
                            r.ToString().TestEquals( $"[1] '{channelName}' publisher (Bound) (already bound)" ) ) ),
                    client.Publishers.Count.TestEquals( 1 ) )
                .Go();
        }

        [Fact]
        public async Task ClientDispose_ShouldMarkAllChannelsAsUnbound()
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
            var bindRequest = new Protocol.BindRequest( channelName );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest.Length );
                    s.SendBoundResponse( true, 1 );
                } );

            var result = await client.Publishers.BindAsync( channelName );
            await serverTask;
            await client.DisposeAsync();

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull( r => r.Publisher.State.TestEquals( MessageBrokerPublisherState.Disposed ) ),
                    client.Publishers.Count.TestEquals( 0 ),
                    client.Publishers.GetAll().TestEmpty() )
                .Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenNameIsEmpty()
        {
            using var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Publishers.BindAsync( string.Empty ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenNameIsTooLong()
        {
            var name = new string( 'x', 513 );
            using var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Publishers.BindAsync( name ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowMessageBrokerClientStateException_WhenClientIsNotRunning()
        {
            using var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Publishers.BindAsync( "foo" ) );
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
            var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                _ = await client.Publishers.BindAsync( "foo" );
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
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var result = await client.Publishers.BindAsync( "foo" );

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.BindRequest ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 9] BindRequest (ChannelName = 'foo')",
                            "['test'::1] [MessageSent] [PacketLength: 9] BindRequest",
                            """
                            ['test'::<ROOT>] [WaitingForMessage] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Message broker server failed to respond to 'test' client's BindRequest request in the specified amount of time (1000 milliseconds).
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

            var result = await client.Publishers.BindAsync( "foo" );
            await endSource.Task.Unwrap();

            Assertion.All(
                    result.Value.TestNull(),
                    result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithBoundResponseWithInvalidId()
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

            var bindRequest = new Protocol.BindRequest( "foo" );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest.Length );
                    s.SendBoundResponse( true, channelId: 0 );
                } );

            var result = await client.Publishers.BindAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.BoundResponse ),
                                exc.Payload.TestEquals( ( uint )Protocol.BoundResponse.Length ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 10] BoundResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 10] Begin handling BoundResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 10] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid BoundResponse with payload 5 from the server. Encountered 1 error(s):
                            1. Expected channel ID to be greater than 0 but found 0.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithBoundResponseWithInvalidPayload()
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

            var bindRequest = new Protocol.BindRequest( "foo" );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest.Length );
                    s.SendBoundResponse( true, 1, payload: 4 );
                } );

            var result = await client.Publishers.BindAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.BoundResponse ),
                                exc.Payload.TestEquals( 4U ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 9] BoundResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 9] Begin handling BoundResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid BoundResponse with payload 4 from the server. Encountered 1 error(s):
                            1. Expected header payload to be 5.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnError_WhenServerRespondsWithBindFailureResponse()
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

            var bindRequest = new Protocol.BindRequest( "foo" );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest.Length );
                    s.SendBindFailureResponse( true, true );
                } );

            var result = await client.Publishers.BindAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.BindRequest ),
                                exc.Payload.TestEquals( bindRequest.Header.Payload ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] BindFailureResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 6] Begin handling BindFailureResponse",
                            """
                            ['test'::1] [MessageReceived] [PacketLength: 6] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Message broker server rejected an invalid BindRequest with payload 4 sent by client 'test'. Encountered 2 error(s):
                            1. Client is already bound to channel 'foo'.
                            2. Binding client to channel 'foo' has been cancelled by the server.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithBindFailureResponseWithInvalidPayload()
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

            var bindRequest = new Protocol.BindRequest( "foo" );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest.Length );
                    s.SendBindFailureResponse( true, true, payload: 0 );
                } );

            var result = await client.Publishers.BindAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.BindFailureResponse ),
                                exc.Payload.TestEquals( 0U ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] BindFailureResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 5] Begin handling BindFailureResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid BindFailureResponse with payload 0 from the server. Encountered 1 error(s):
                            1. Expected header payload to be 1.
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
                new TimestampProvider(),
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetEventHandler( logs.Add ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindRequest( "foo" );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest.Length );
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            var result = await client.Publishers.BindAsync( "foo" );
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

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public async Task UnbindAsync_ShouldUnbindChannelCorrectly(bool channelRemoved)
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

            var channelId = 1;
            var channelName = "foo";
            var handshakeRequest = await server.EstablishHandshake( client );
            var bindRequest = new Protocol.BindRequest( channelName );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest.Length );
                    s.SendBoundResponse( true, channelId );
                    s.ReadUnbindRequest();
                    s.SendUnboundResponse( channelRemoved );
                } );

            var result = Result.Create( default( MessageBrokerUnbindResult ) );
            await client.Publishers.BindAsync( channelName );
            var publisher = client.Publishers.TryGetByChannelId( channelId );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestFalse(),
                    result.Value.ChannelRemoved.TestEquals( channelRemoved ),
                    result.Value.ToString().TestEquals( channelRemoved ? "Success (channel removed)" : "Success" ),
                    publisher.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Disposed ) ),
                    client.Publishers.Count.TestEquals( 0 ),
                    client.Publishers.GetAll().TestEmpty(),
                    client.Publishers.TryGetByChannelName( channelName ).TestNull(),
                    client.Publishers.TryGetByChannelId( channelId ).TestNull(),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::2] [SendingMessage] [PacketLength: 9] UnbindRequest (ChannelId = 1, ChannelName = 'foo')",
                            "['test'::2] [MessageSent] [PacketLength: 9] UnbindRequest",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] UnboundResponse",
                            "['test'::2] [MessageReceived] [PacketLength: 6] Begin handling UnboundResponse",
                            "['test'::2] [MessageAccepted] [PacketLength: 6] UnboundResponse"
                        ] ),
                    AssertServerData(
                        server.GetAllReceived(),
                        (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse),
                        (bindRequest.Length, MessageBrokerServerEndpoint.BindRequest),
                        (Protocol.UnbindRequest.Length, MessageBrokerServerEndpoint.UnbindRequest) ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldNotThrow_WhenChannelIsAlreadyLocallyUnbound()
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
            var bindRequest = new Protocol.BindRequest( channelName );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest.Length );
                    s.SendBoundResponse( true, 1 );
                    s.ReadUnbindRequest();
                    s.SendUnboundResponse( true );
                } );

            var result = Result.Create( default( MessageBrokerUnbindResult ) );
            await client.Publishers.BindAsync( channelName );
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            if ( publisher is not null )
            {
                await publisher.UnbindAsync();
                result = await publisher.UnbindAsync();
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
            var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var publisher = new MessageBrokerPublisher( client, 1, "foo" );
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                _ = await publisher.UnbindAsync();
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
                new TimestampProvider(),
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
                    var request = new Protocol.BindRequest( "foo" );
                    s.Read( request.Length );
                    s.SendBoundResponse( true, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;

            var result = Result.Create( default( MessageBrokerUnbindResult ) );
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.UnbindRequest ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::2] [SendingMessage] [PacketLength: 9] UnbindRequest (ChannelId = 1, ChannelName = 'foo')",
                            "['test'::2] [MessageSent] [PacketLength: 9] UnbindRequest",
                            """
                            ['test'::<ROOT>] [WaitingForMessage] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Message broker server failed to respond to 'test' client's UnbindRequest request in the specified amount of time (1000 milliseconds).
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnError_WhenClientIsDisposedBeforeServerResponds()
        {
            var channelBound = Ref.Create( false );
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
                            bool linked;
                            lock ( channelBound )
                                linked = channelBound.Value;

                            if ( linked
                                && e.Type == MessageBrokerClientEventType.SendingMessage
                                && e.GetServerEndpoint() == MessageBrokerServerEndpoint.PingRequest )
                                endSource.Complete( e.Client.DisposeAsync().AsTask() );
                        } ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindRequest( "foo" );
                    s.Read( request.Length );
                    s.SendBoundResponse( true, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            lock ( channelBound )
                channelBound.Value = true;

            var result = Result.Create( default( MessageBrokerUnbindResult ) );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await endSource.Task.Unwrap();

            result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithUnboundResponseWithInvalidPayload()
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
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindRequest( "foo" );
                    s.Read( request.Length );
                    s.SendBoundResponse( true, 1 );
                    s.ReadUnbindRequest();
                    s.SendUnboundResponse( true, payload: 0 );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindResult ) );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.UnboundResponse ),
                                exc.Payload.TestEquals( 0U ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] UnboundResponse",
                            "['test'::2] [MessageReceived] [PacketLength: 5] Begin handling UnboundResponse",
                            """
                            ['test'::2] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid UnboundResponse with payload 0 from the server. Encountered 1 error(s):
                            1. Expected header payload to be 1.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnError_WhenServerRespondsWithUnbindFailureResponse()
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
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindRequest( "foo" );
                    s.Read( request.Length );
                    s.SendBoundResponse( true, 1 );
                    s.ReadUnbindRequest();
                    s.SendUnbindFailureResponse( true );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindResult ) );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.UnbindRequest ),
                                exc.Payload.TestEquals( ( uint )sizeof( uint ) ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] UnbindFailureResponse",
                            "['test'::2] [MessageReceived] [PacketLength: 6] Begin handling UnbindFailureResponse",
                            """
                            ['test'::2] [MessageReceived] [PacketLength: 6] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Message broker server rejected an invalid UnbindRequest with payload 4 sent by client 'test'. Encountered 1 error(s):
                            1. Client is not bound to channel [1] 'foo'.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task
            UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithUnbindFailureResponseWithInvalidPayload()
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
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindRequest( "foo" );
                    s.Read( request.Length );
                    s.SendBoundResponse( true, 1 );
                    s.ReadUnbindRequest();
                    s.SendUnbindFailureResponse( true, payload: 0 );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindResult ) );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.UnbindFailureResponse ),
                                exc.Payload.TestEquals( 0U ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] UnbindFailureResponse",
                            "['test'::2] [MessageReceived] [PacketLength: 5] Begin handling UnbindFailureResponse",
                            """
                            ['test'::2] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid UnbindFailureResponse with payload 0 from the server. Encountered 1 error(s):
                            1. Expected header payload to be 1.
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
                new TimestampProvider(),
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
                    var request = new Protocol.BindRequest( "foo" );
                    s.Read( request.Length );
                    s.SendBoundResponse( true, 1 );
                    s.ReadUnbindRequest();
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindResult ) );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
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
                            ['test'::2] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid 0 with payload 0 from the server. Encountered 1 error(s):
                            1. Received unexpected client endpoint.
                            """
                        ] ) )
                .Go();
        }
    }
}
