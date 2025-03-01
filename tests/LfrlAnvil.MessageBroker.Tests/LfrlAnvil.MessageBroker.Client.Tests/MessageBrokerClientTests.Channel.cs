using System.Net;
using System.Threading;
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
    public class Channel : TestsBase
    {
        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public async Task LinkAsync_ShouldLinkChannelCorrectly(bool created)
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
            var expectedResultType = created
                ? MessageBrokerChannelLinkResult.ResultType.CreatedAndLinked
                : MessageBrokerChannelLinkResult.ResultType.Linked;

            var linkRequest = new Protocol.LinkChannelRequest( channelName );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendChannelLinkedResponse( created, 1 );
                } );

            var result = await client.Channels.LinkAsync( channelName );
            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.Type.TestEquals( expectedResultType ),
                            r.Channel.TestRefEquals( client.Channels.TryGetById( 1 ) ) ) ),
                    client.Channels.Count.TestEquals( 1 ),
                    client.Channels.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( client.Channels.TryGetById( 1 ) ) ] ),
                    client.Channels.TryGetByName( channelName ).TestRefEquals( client.Channels.TryGetById( 1 ) ),
                    client.Channels.TryGetById( 1 )
                        .TestNotNull(
                            channel => Assertion.All(
                                "channel",
                                channel.Client.TestRefEquals( client ),
                                channel.Id.TestEquals( 1 ),
                                channel.Name.TestEquals( channelName ),
                                channel.State.TestEquals( MessageBrokerLinkedChannelState.Linked ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 9] LinkChannelRequest (ChannelName = 'foo')",
                            "['test'::1] [MessageSent] [PacketLength: 9] LinkChannelRequest",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 10] ChannelLinkedResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 10] Begin handling ChannelLinkedResponse",
                            "['test'::1] [MessageAccepted] [PacketLength: 10] ChannelLinkedResponse (Id = 1)"
                        ] ),
                    AssertServerData(
                        server.GetAllReceived(),
                        (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse),
                        (linkRequest.Length, MessageBrokerServerEndpoint.LinkChannelRequest) ) )
                .Go();
        }

        [Fact]
        public async Task LinkAsync_ShouldNotThrow_WhenChannelIsAlreadyLocallyLinked()
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
            var linkRequest = new Protocol.LinkChannelRequest( channelName );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendChannelLinkedResponse( true, 1 );
                } );

            await client.Channels.LinkAsync( channelName );
            await serverTask;

            var result = await client.Channels.LinkAsync( channelName );

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.Type.TestEquals( MessageBrokerChannelLinkResult.ResultType.AlreadyLinked ),
                            r.Channel.TestRefEquals( client.Channels.TryGetById( 1 ) ) ) ),
                    client.Channels.Count.TestEquals( 1 ) )
                .Go();
        }

        [Fact]
        public async Task ClientDispose_ShouldMarkAllChannelsAsUnlinked()
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
            var linkRequest = new Protocol.LinkChannelRequest( channelName );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendChannelLinkedResponse( true, 1 );
                } );

            var result = await client.Channels.LinkAsync( channelName );
            await serverTask;
            await client.DisposeAsync();

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull( r => r.Channel.State.TestEquals( MessageBrokerLinkedChannelState.Unlinked ) ),
                    client.Channels.Count.TestEquals( 0 ),
                    client.Channels.GetAll().TestEmpty() )
                .Go();
        }

        [Fact]
        public void LinkAsync_ShouldThrowTaskCanceledException_WhenCancellationTokenIsCancelled()
        {
            using var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Channels.LinkAsync( "foo", new CancellationToken( canceled: true ) ) );
            action.Test( exc => exc.TestType().AssignableTo<TaskCanceledException>() ).Go();
        }

        [Fact]
        public void LinkAsync_ShouldThrowArgumentOutOfRangeException_WhenNameIsEmpty()
        {
            using var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Channels.LinkAsync( string.Empty ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void LinkAsync_ShouldThrowArgumentOutOfRangeException_WhenNameIsTooLong()
        {
            var name = new string( 'x', 513 );
            using var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Channels.LinkAsync( name ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void LinkAsync_ShouldThrowMessageBrokerClientStateException_WhenClientIsNotRunning()
        {
            using var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Channels.LinkAsync( "foo" ) );
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
        public async Task LinkAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            var client = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                _ = await client.Channels.LinkAsync( "foo" );
            }
            catch ( Exception exc )
            {
                exception = exc;
            }

            exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task LinkAsync_ShouldReturnErrorAndDisposeClient_WhenServerDoesNotRespondInTime()
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

            var result = await client.Channels.LinkAsync( "foo" );

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.LinkChannelRequest ),
                                exc.ResponseEndpoint.TestEquals( MessageBrokerClientEndpoint.ChannelLinkedResponse ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 9] LinkChannelRequest (ChannelName = 'foo')",
                            "['test'::1] [MessageSent] [PacketLength: 9] LinkChannelRequest",
                            """
                            ['test'::<ROOT>] [WaitingForMessage] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Message broker server failed to respond with ChannelLinkedResponse packet to 'test' client's LinkChannelRequest request in the specified amount of time (1000 milliseconds).
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task LinkAsync_ShouldReturnError_WhenClientIsDisposedBeforeServerResponds()
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

            var result = await client.Channels.LinkAsync( "foo" );
            await endSource.Task.Unwrap();

            Assertion.All(
                    result.Value.TestNull(),
                    result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ) )
                .Go();
        }

        [Fact]
        public async Task LinkAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithChannelLinkedResponseWithInvalidId()
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

            var linkRequest = new Protocol.LinkChannelRequest( "foo" );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendChannelLinkedResponse( true, id: 0 );
                } );

            var result = await client.Channels.LinkAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.ChannelLinkedResponse ),
                                exc.Payload.TestEquals( ( uint )Protocol.ChannelLinkedResponse.Length ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 10] ChannelLinkedResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 10] Begin handling ChannelLinkedResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 10] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid ChannelLinkedResponse with payload 5 from the server. Encountered 1 error(s):
                            1. Expected channel ID to be greater than 0 but found 0.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task LinkAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithChannelLinkedResponseWithInvalidPayload()
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

            var linkRequest = new Protocol.LinkChannelRequest( "foo" );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendChannelLinkedResponse( true, 1, payload: 4 );
                } );

            var result = await client.Channels.LinkAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.ChannelLinkedResponse ),
                                exc.Payload.TestEquals( 4U ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 9] ChannelLinkedResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 9] Begin handling ChannelLinkedResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 9] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid ChannelLinkedResponse with payload 4 from the server. Encountered 1 error(s):
                            1. Expected header payload to be 5.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task LinkAsync_ShouldReturnError_WhenServerRespondsWithLinkChannelFailureResponse()
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

            var linkRequest = new Protocol.LinkChannelRequest( "foo" );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendLinkChannelFailureResponse( true, true );
                } );

            var result = await client.Channels.LinkAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.LinkChannelRequest ),
                                exc.Payload.TestEquals( linkRequest.Header.Payload ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] LinkChannelFailureResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 6] Begin handling LinkChannelFailureResponse",
                            """
                            ['test'::1] [MessageReceived] [PacketLength: 6] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Message broker server rejected an invalid LinkChannelRequest with payload 4 sent by client 'test'. Encountered 2 error(s):
                            1. Client is already linked to channel 'foo'.
                            2. Linking client to channel 'foo' has been cancelled by the server.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task LinkAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithLinkChannelFailureResponseWithInvalidPayload()
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

            var linkRequest = new Protocol.LinkChannelRequest( "foo" );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.SendLinkChannelFailureResponse( true, true, payload: 0 );
                } );

            var result = await client.Channels.LinkAsync( "foo" );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.LinkChannelFailureResponse ),
                                exc.Payload.TestEquals( 0U ) ) ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] LinkChannelFailureResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 5] Begin handling LinkChannelFailureResponse",
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid LinkChannelFailureResponse with payload 0 from the server. Encountered 1 error(s):
                            1. Expected header payload to be 1.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task LinkAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithInvalidEndpoint()
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

            var linkRequest = new Protocol.LinkChannelRequest( "foo" );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( linkRequest.Length );
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            var result = await client.Channels.LinkAsync( "foo" );
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
    }
}
