using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Functional;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests
{
    public class Publisher : TestsBase, IClassFixture<SharedClientResourceFixture>
    {
        private readonly ValueTaskDelaySource _sharedDelaySource;

        public Publisher(SharedClientResourceFixture fixture)
        {
            _sharedDelaySource = fixture.DelaySource;
        }

        [Theory]
        [InlineData( true, true )]
        [InlineData( false, true )]
        [InlineData( true, false )]
        [InlineData( false, false )]
        public async Task BindAsync_ShouldBindPublisherCorrectly(bool channelCreated, bool streamCreated)
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var streamName = "bar";
            var bindRequest = new Protocol.BindPublisherRequest( channelName, streamName );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendPublisherBoundResponse( channelCreated, streamCreated, 1, 2 );
                } );

            var result = await client.Publishers.BindAsync( channelName, streamName );
            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TestNotNull(
                        r => Assertion.All(
                            "result.Value",
                            r.AlreadyBound.TestFalse(),
                            r.ChannelCreated.TestEquals( channelCreated ),
                            r.StreamCreated.TestEquals( streamCreated ),
                            r.Publisher.TestRefEquals( client.Publishers.TryGetByChannelId( 1 ) ),
                            r.ToString()
                                .TestEquals(
                                    channelCreated
                                        ? streamCreated
                                            ? $"[1] 'test' => [1] '{channelName}' publisher (using [2] '{streamName}' stream) (Bound) (channel created) (stream created)"
                                            : $"[1] 'test' => [1] '{channelName}' publisher (using [2] '{streamName}' stream) (Bound) (channel created)"
                                        : streamCreated
                                            ? $"[1] 'test' => [1] '{channelName}' publisher (using [2] '{streamName}' stream) (Bound) (stream created)"
                                            : $"[1] 'test' => [1] '{channelName}' publisher (using [2] '{streamName}' stream) (Bound)" ) ) ),
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
                                publisher.StreamId.TestEquals( 2 ),
                                publisher.StreamName.TestEquals( streamName ),
                                publisher.State.TestEquals( MessageBrokerPublisherState.Bound ),
                                publisher.ToString()
                                    .TestEquals(
                                        $"[1] 'test' => [1] '{channelName}' publisher (using [2] '{streamName}' stream) (Bound)" ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                                $"[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = '{channelName}', StreamName = '{streamName}'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 16)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 16)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                                $"[PublisherChange:Bound] Client = [1] 'test', TraceId = 1, Channel = [1] '{channelName}', Stream = [2] '{streamName}'",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PublisherBoundResponse, Length = 14)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldNotThrow_WhenPublisherIsAlreadyLocallyBound()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var bindRequest = new Protocol.BindPublisherRequest( channelName, null );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
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
                            r.StreamCreated.TestFalse(),
                            r.Publisher.TestRefEquals( client.Publishers.TryGetByChannelId( 1 ) ),
                            r.ToString()
                                .TestEquals(
                                    $"[1] 'test' => [1] '{channelName}' publisher (using [1] 'foo' stream) (Bound) (already bound)" ) ) ),
                    client.Publishers.Count.TestEquals( 1 ) )
                .Go();
        }

        [Fact]
        public async Task ClientDispose_ShouldMarkAllPublishersAsDisposed()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var bindRequest = new Protocol.BindPublisherRequest( channelName, null );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
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
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenChannelNameIsEmpty()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Publishers.BindAsync( string.Empty ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenStreamNameIsNotNullAndEmpty()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Publishers.BindAsync( "foo", string.Empty ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenChannelNameIsTooLong()
        {
            var name = new string( 'x', 513 );
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Publishers.BindAsync( name ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowArgumentOutOfRangeException_WhenStreamNameIsNotNullAndTooLong()
        {
            var name = new string( 'x', 513 );
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.Publishers.BindAsync( "foo", name ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void BindAsync_ShouldThrowMessageBrokerClientStateException_WhenClientIsNotRunning()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
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
            var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
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
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );

            var result = await client.Publishers.BindAsync( "foo" );

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Value.TestNull(),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.BindPublisherRequest ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', StreamName = 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Server failed to respond to 'test' client's BindPublisherRequest in the specified amount of time (1000 milliseconds).
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket().Length.TestGreaterThan( 0 ) )
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        MessageBrokerClientLogger.Create(
                            sendPacket: e =>
                            {
                                if ( e.Type == MessageBrokerClientSendPacketEventType.Sending
                                    && e.Packet.Endpoint == MessageBrokerServerEndpoint.Ping )
                                    endSource.Complete( e.Source.Client.DisposeAsync().AsTask() );
                            } ) ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );

            var result = await client.Publishers.BindAsync( "foo" );
            await endSource.Task.Unwrap();

            Assertion.All(
                    result.Value.TestNull(),
                    result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithPublisherBoundResponseWithInvalidValues()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindPublisherRequest( "foo", null );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendPublisherBoundResponse( true, true, channelId: 0, streamId: -1 );
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
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.PublisherBoundResponse ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', StreamName = 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid PublisherBoundResponse from the server. Encountered 2 error(s):
                                1. Expected channel ID to be greater than 0 but found 0.
                                2. Expected stream ID to be greater than 0 but found -1.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PublisherBoundResponse, Length = 14)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithPublisherBoundResponseWithInvalidPayload()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindPublisherRequest( "foo", null );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendPublisherBoundResponse( true, true, 1, 1, payload: 8 );
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
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.PublisherBoundResponse ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', StreamName = 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 13)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid PublisherBoundResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 9 but found 8.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PublisherBoundResponse, Length = 13)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnError_WhenServerRespondsWithBindPublisherFailureResponse()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindPublisherRequest( "foo", null );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendBindPublisherFailureResponse( true, true );
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
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.BindPublisherRequest ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', StreamName = 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherFailureResponse, Length = 6)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherFailureResponse, Length = 6)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Server rejected an invalid BindPublisherRequest sent by client 'test'. Encountered 2 error(s):
                                1. Client is already bound as a publisher to channel 'foo'.
                                2. Binding client to channel 'foo' as a publisher has been cancelled by the server.
                                """,
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherFailureResponse, Length = 6)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task BindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithBindPublisherFailureResponseWithInvalidPayload()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindPublisherRequest( "foo", null );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendBindPublisherFailureResponse( true, true, payload: 0 );
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
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.BindPublisherFailureResponse ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', StreamName = 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherFailureResponse, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid BindPublisherFailureResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 1 but found 0.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherFailureResponse, Length = 5)"
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );

            var bindRequest = new Protocol.BindPublisherRequest( "foo", null );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
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
                                exc.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ) ) ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', StreamName = 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 13)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                                1. Received unexpected client endpoint.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (<unrecognized-endpoint-0>, Length = 5)"
                        ] ) )
                .Go();
        }

        [Theory]
        [InlineData( true, true )]
        [InlineData( false, true )]
        [InlineData( true, false )]
        [InlineData( false, false )]
        public async Task UnbindAsync_ShouldUnbindPublisherCorrectly(bool channelRemoved, bool streamRemoved)
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            var channelId = 1;
            var channelName = "foo";
            await server.EstablishHandshake( client );
            var bindRequest = new Protocol.BindPublisherRequest( channelName, null );

            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendPublisherBoundResponse( true, true, channelId, 2 );
                    s.ReadUnbindPublisherRequest();
                    s.SendPublisherUnboundResponse( channelRemoved, streamRemoved );
                } );

            var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
            await client.Publishers.BindAsync( channelName );
            var publisher = client.Publishers.TryGetByChannelId( channelId );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestFalse(),
                    result.Value.ChannelRemoved.TestEquals( channelRemoved ),
                    result.Value.StreamRemoved.TestEquals( streamRemoved ),
                    result.Value.ToString()
                        .TestEquals(
                            channelRemoved
                                ? streamRemoved ? "Success (channel removed) (stream removed)" : "Success (channel removed)"
                                : streamRemoved
                                    ? "Success (stream removed)"
                                    : "Success" ),
                    publisher.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Disposed ) ),
                    client.Publishers.Count.TestEquals( 0 ),
                    client.Publishers.GetAll().TestEmpty(),
                    client.Publishers.TryGetByChannelName( channelName ).TestNull(),
                    client.Publishers.TryGetByChannelId( channelId ).TestNull(),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                                $"[PublisherChange:Unbinding] Client = [1] 'test', TraceId = 2, Channel = [1] '{channelName}', Stream = [2] '{channelName}'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (PublisherUnboundResponse, Length = 6)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (PublisherUnboundResponse, Length = 6)",
                                $"[PublisherChange:Unbound] Client = [1] 'test', TraceId = 2, Channel = [1] '{channelName}', Stream = [2] '{channelName}'",
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PublisherUnboundResponse, Length = 6)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldNotThrow_WhenChannelIsAlreadyLocallyDisposed()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource ) );

            await server.EstablishHandshake( client );

            var channelName = "foo";
            var bindRequest = new Protocol.BindPublisherRequest( channelName, null );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( bindRequest );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindPublisherRequest();
                    s.SendPublisherUnboundResponse( true, true );
                } );

            var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
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
            var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var publisher = new MessageBrokerPublisher( client, 1, "foo", 1, "foo" );
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
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;

            var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.UnbindPublisherRequest ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                                "[PublisherChange:Unbinding] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Server failed to respond to 'test' client's UnbindPublisherRequest in the specified amount of time (1000 milliseconds).
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket().Length.TestGreaterThan( 0 ) )
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
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        MessageBrokerClientLogger.Create(
                            sendPacket: e =>
                            {
                                bool bound;
                                lock ( channelBound )
                                    bound = channelBound.Value;

                                if ( bound
                                    && e.Type == MessageBrokerClientSendPacketEventType.Sending
                                    && e.Packet.Endpoint == MessageBrokerServerEndpoint.Ping )
                                    endSource.Complete( e.Source.Client.DisposeAsync().AsTask() );
                            } ) ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            lock ( channelBound )
                channelBound.Value = true;

            var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await endSource.Task.Unwrap();

            result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithPublisherUnboundResponseWithInvalidPayload()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindPublisherRequest();
                    s.SendPublisherUnboundResponse( true, true, payload: 0 );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.PublisherUnboundResponse ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                                "[PublisherChange:Unbinding] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (PublisherUnboundResponse, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid PublisherUnboundResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 1 but found 0.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PublisherUnboundResponse, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task UnbindAsync_ShouldReturnError_WhenServerRespondsWithUnbindPublisherFailureResponse()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindPublisherRequest();
                    s.SendUnbindPublisherFailureResponse( true );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.UnbindPublisherRequest ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                                "[PublisherChange:Unbinding] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherFailureResponse, Length = 6)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherFailureResponse, Length = 6)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Server rejected an invalid UnbindPublisherRequest sent by client 'test'. Encountered 1 error(s):
                                1. Client is not bound as a publisher to channel [1] 'foo'.
                                """,
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherFailureResponse, Length = 6)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task
            UnbindAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithUnbindPublisherFailureResponseWithInvalidPayload()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindPublisherRequest();
                    s.SendUnbindPublisherFailureResponse( true, payload: 0 );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.UnbindPublisherFailureResponse ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                                "[PublisherChange:Unbinding] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherFailureResponse, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid UnbindPublisherFailureResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 1 but found 0.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherFailureResponse, Length = 5)"
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindPublisherRequest();
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
            if ( publisher is not null )
                result = await publisher.UnbindAsync();

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                                "[PublisherChange:Unbinding] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo'",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (UnbindPublisherRequest, Length = 9)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                                1. Received unexpected client endpoint.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (<unrecognized-endpoint-0>, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task GetSendContext_ShouldReturnContextWhichAllowsToSendSingleMessage()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;

            var data = new byte[] { 1, 2, 3, 4, 5 };
            serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.PushMessageHeader( 1, data.Length ) );
                    s.SendMessageAcceptedResponse( 1 );
                } );

            var result = Result.Create( MesageBrokerSendResult.CreateNotBound() );
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            if ( publisher is not null )
            {
                using var ctx = publisher.GetSendContext();
                result = await ctx.Append( data ).SendAsync();
            }

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestFalse(),
                    result.Value.Id.TestEquals( 1UL ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[MessagePushing] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, MessageId = 1",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task GetSendContext_ShouldReturnContextWhichAllowsToSendSingleMessage_UsingBufferWriter()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;

            var data = new byte[] { 1, 2, 3, 4, 5 };
            serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.PushMessageHeader( 1, data.Length ) );
                    s.SendMessageAcceptedResponse( 2 );
                } );

            var result = Result.Create( MesageBrokerSendResult.CreateNotBound() );
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            if ( publisher is not null )
            {
                using var ctx = publisher.GetSendContext();
                data.AsSpan( 0, 2 ).CopyTo( ctx.GetSpan( 2 ) );
                ctx.Advance( 2 );
                data.AsMemory( 2 ).CopyTo( ctx.GetMemory( 3 ) );
                ctx.Advance( 3 );
                result = await ctx.SendAsync();
            }

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestFalse(),
                    result.Value.Id.TestEquals( 2UL ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[MessagePushing] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5, MessageId = 2",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task GetSendContext_ShouldReturnContextWhichAllowsToSendSingleMessage_WhenMessageIsLargerThanInitialCapacity()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;

            var data = new byte[2048];
            serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.PushMessageHeader( 1, data.Length ) );
                    s.SendMessageAcceptedResponse( 3 );
                } );

            var result = Result.Create( MesageBrokerSendResult.CreateNotBound() );
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            if ( publisher is not null )
            {
                using var ctx = publisher.GetSendContext();
                result = await ctx.Append( data ).SendAsync();
            }

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestFalse(),
                    result.Value.Id.TestEquals( 3UL ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[MessagePushing] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 2048",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 2057)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 2057)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 2048, MessageId = 3",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task GetSendContext_ShouldReturnContextThatThrowsObjectDisposedExceptionWhenActedOnAfterDisposal()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;

            var getMemoryAction = Lambda.Of( () => { } );
            var getSpanAction = Lambda.Of( () => { } );
            var advanceAction = Lambda.Of( () => { } );
            var appendAction = Lambda.Of( () => { } );
            var sendAction = Lambda.Of( () => Task.CompletedTask );
            var disposeAction = Lambda.Of( () => throw new Exception() );

            var publisher = client.Publishers.TryGetByChannelId( 1 );
            if ( publisher is not null )
            {
                var ctx = publisher.GetSendContext();
                ctx.Dispose();
                getMemoryAction = Lambda.Of( () => { _ = ctx.GetMemory( 5 ); } );
                getSpanAction = Lambda.Of( () => { _ = ctx.GetSpan( 5 ); } );
                advanceAction = Lambda.Of( () => ctx.Advance( 5 ) );
                appendAction = Lambda.Of( () => { _ = ctx.Append( Array.Empty<byte>() ); } );
                sendAction = Lambda.Of( async () => await ctx.SendAsync() );
                disposeAction = Lambda.Of( () => ctx.Dispose() );
            }

            Assertion.All(
                    getMemoryAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                    getSpanAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                    advanceAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                    appendAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                    sendAction.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ),
                    disposeAction.Test( exc => exc.TestNull() ) )
                .Go();
        }

        [Fact]
        public async Task GetSendContext_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;

            var publisher = client.Publishers.TryGetByChannelId( 1 );
            await client.DisposeAsync();
            var action = publisher is not null ? Lambda.Of( () => { _ = publisher.GetSendContext(); } ) : Lambda.Of( () => { } );

            action.Test( exc => exc.TestType().Exact<MessageBrokerClientDisposedException>() ).Go();
        }

        [Fact]
        public async Task SendAsync_ShouldSendSingleMessage()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;

            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.PushMessageHeader( 1, data.Length ) );
                    s.SendMessageAcceptedResponse( 1 );
                } );

            var result = Result.Create( MesageBrokerSendResult.CreateNotBound() );
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            if ( publisher is not null )
                result = await publisher.SendAsync( data );

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestFalse(),
                    result.Value.Id.TestEquals( 1UL ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[MessagePushing] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 8",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 17)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 17)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 8, MessageId = 1",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SendAsync_ShouldNotThrow_WhenPublisherIsAlreadyLocallyDisposed()
        {
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                    s.ReadUnbindPublisherRequest();
                    s.SendPublisherUnboundResponse( true, true );
                } );

            var result = Result.Create( MesageBrokerSendResult.Create( 1 ) );
            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            if ( publisher is not null )
            {
                await publisher.UnbindAsync();
                result = await publisher.SendAsync( new byte[] { 1, 2, 3 } );
            }

            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.NotBound.TestTrue(),
                    result.Value.Id.TestNull() )
                .Go();
        }

        [Fact]
        public async Task SendAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var publisher = new MessageBrokerPublisher( client, 1, "foo", 1, "foo" );
            var context = publisher.GetSendContext();
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                _ = await context.Append( new byte[] { 1, 2, 3 } ).SendAsync();
            }
            catch ( Exception exc )
            {
                exception = exc;
            }

            exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task SendAsync_ShouldReturnErrorAndDisposeClient_WhenServerDoesNotRespondInTime()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;

            var result = Result.Create( MesageBrokerSendResult.Create( 1 ) );
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            if ( publisher is not null )
                result = await publisher.SendAsync( new byte[] { 1, 2, 3 } );

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.PushMessage ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[MessagePushing] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 3",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 12)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 12)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Server failed to respond to 'test' client's PushMessage in the specified amount of time (1000 milliseconds).
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)",
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket().Length.TestGreaterThan( 0 ) )
                .Go();
        }

        [Fact]
        public async Task SendAsync_ShouldReturnError_WhenClientIsDisposedBeforeServerResponds()
        {
            var publisherBound = Ref.Create( false );
            var endSource = new SafeTaskCompletionSource<Task>();
            using var server = new ServerMock();
            var remoteEndPoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndPoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        MessageBrokerClientLogger.Create(
                            sendPacket:
                            e =>
                            {
                                bool bound;
                                lock ( publisherBound )
                                    bound = publisherBound.Value;

                                if ( bound
                                    && e.Type == MessageBrokerClientSendPacketEventType.Sending
                                    && e.Packet.Endpoint == MessageBrokerServerEndpoint.Ping )
                                    endSource.Complete( e.Source.Client.DisposeAsync().AsTask() );
                            } ) ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await serverTask;
            var publisher = client.Publishers.TryGetByChannelId( 1 );
            lock ( publisherBound )
                publisherBound.Value = true;

            var result = Result.Create( default( MesageBrokerSendResult ) );
            if ( publisher is not null )
                result = await publisher.SendAsync( new byte[] { 1, 2, 3 } );

            await endSource.Task.Unwrap();

            result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task SendAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithMessageAcceptedResponseWithInvalidPayload()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            var data = new byte[] { 1, 2, 3, 4, 5 };
            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                    s.ReadPushMessage( data.Length );
                    s.SendMessageAcceptedResponse( 1, payload: 7 );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MesageBrokerSendResult ) );
            if ( publisher is not null )
                result = await publisher.SendAsync( data );

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.MessageAcceptedResponse ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[MessagePushing] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 12)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid MessageAcceptedResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 8 but found 7.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 12)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SendAsync_ShouldReturnError_WhenServerRespondsWithMessageRejectedResponse()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            var data = new byte[] { 1, 2, 3, 4, 5 };
            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                    s.ReadPushMessage( data.Length );
                    s.SendMessageRejectedResponse( true, true );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MesageBrokerSendResult ) );
            if ( publisher is not null )
                result = await publisher.SendAsync( data );

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.PushMessage ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[MessagePushing] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageRejectedResponse, Length = 6)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (MessageRejectedResponse, Length = 6)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Server rejected an invalid PushMessage sent by client 'test'. Encountered 2 error(s):
                                1. Client is not bound as a publisher to channel [1] 'foo'.
                                2. Message push to stream [1] 'foo' has been cancelled by the server.
                                """,
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageRejectedResponse, Length = 6)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SendAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithMessageRejectedResponseWithInvalidPayload()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            var data = new byte[] { 1, 2, 3, 4, 5 };
            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                    s.ReadPushMessage( data.Length );
                    s.SendMessageRejectedResponse( true, true, payload: 0 );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MesageBrokerSendResult ) );
            if ( publisher is not null )
                result = await publisher.SendAsync( data );

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.MessageRejectedResponse ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[MessagePushing] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (MessageRejectedResponse, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid MessageRejectedResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 1 but found 0.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageRejectedResponse, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task SendAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithInvalidEndpoint()
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger( logs.GetLogger() ) );

            var data = new byte[] { 1, 2, 3, 4, 5 };
            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    var request = new Protocol.BindPublisherRequest( "foo", null );
                    s.Read( request );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                    s.ReadPushMessage( data.Length );
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 );

            var result = Result.Create( default( MesageBrokerSendResult ) );
            if ( publisher is not null )
                result = await publisher.SendAsync( data );

            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ) ) ),
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[MessagePushing] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', Length = 5",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 14)",
                                """
                                [Error] Client = [1] 'test', TraceId = 2
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                                1. Received unexpected client endpoint.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (<unrecognized-endpoint-0>, Length = 5)"
                        ] ) )
                .Go();
        }
    }
}
