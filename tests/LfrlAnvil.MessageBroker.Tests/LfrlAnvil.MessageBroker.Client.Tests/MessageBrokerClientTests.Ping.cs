using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Internal;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests
{
    public class Ping : TestsBase
    {
        [Fact]
        public async Task PingScheduler_ShouldSendPingAndReceivePingFromServerOnSchedule()
        {
            var pingResponseCount = 0;
            var endSource = new SafeTaskCompletionSource();
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
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.MessageAccepted
                                && e.GetClientEndpoint() == MessageBrokerClientEndpoint.PingResponse
                                && ++pingResponseCount == 2 )
                                endSource.Complete();
                        } ) );

            var handshakeRequest = new Protocol.HandshakeRequest( client );
            var serverTask = Task.Factory.StartNew(
                o =>
                {
                    var s = ( ServerMock )o!;
                    s.WaitForClient();
                    s.Read( handshakeRequest.Length );
                    s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 0.2 ) );
                    s.Read( Protocol.PacketHeader.Length );
                    Thread.Sleep( 150 );
                    s.Read( Protocol.PacketHeader.Length );
                    s.SendPing();
                    Thread.Sleep( 150 );
                    s.Read( Protocol.PacketHeader.Length );
                    s.SendPing();
                },
                server );

            await client.StartAsync();
            await serverTask;
            await endSource.Task;

            var serverData = server.GetAllReceived();

            Assertion.All(
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 5] PingRequest",
                            "['test'::1] [MessageSent] [PacketLength: 5] PingRequest",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] PingResponse",
                            "['test'::1] [MessageReceived] [PacketLength: 5] Begin handling PingResponse",
                            "['test'::1] [MessageAccepted] [PacketLength: 5] PingResponse",
                            "['test'::2] [SendingMessage] [PacketLength: 5] PingRequest",
                            "['test'::2] [MessageSent] [PacketLength: 5] PingRequest",
                            "['test'::2] [MessageReceived] [PacketLength: 5] Begin handling PingResponse",
                            "['test'::2] [MessageAccepted] [PacketLength: 5] PingResponse"
                        ] ),
                    AssertServerData(
                        serverData,
                        (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.PingRequest),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.PingRequest) ) )
                .Go();
        }

        [Fact]
        public async Task PingScheduler_ShouldReactCorrectlyToClientBeingDisposedBeforeReceivingPingResponseFromServer()
        {
            var endSource = new SafeTaskCompletionSource<Task>();
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
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.MessageSent
                                && e.GetServerEndpoint() == MessageBrokerServerEndpoint.PingRequest )
                                endSource.Complete( e.Client.DisposeAsync().AsTask() );
                        } ) );

            var handshakeRequest = new Protocol.HandshakeRequest( client );
            var serverTask = Task.Factory.StartNew(
                o =>
                {
                    var s = ( ServerMock )o!;
                    s.WaitForClient();
                    s.Read( handshakeRequest.Length );
                    s.SendHandshakeAccepted( 1, Duration.FromSeconds( 0.2 ), Duration.FromSeconds( 0.2 ) );
                    s.Read( Protocol.PacketHeader.Length );
                    Thread.Sleep( 150 );
                    s.Read( Protocol.PacketHeader.Length );
                },
                server );

            await client.StartAsync();
            await serverTask;
            var disposeTask = await endSource.Task;
            await disposeTask;

            var serverData = server.GetAllReceived();

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 5] PingRequest",
                            "['test'::1] [MessageSent] [PacketLength: 5] PingRequest"
                        ] ),
                    AssertServerData(
                        serverData,
                        (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.PingRequest) ) )
                .Go();
        }

        [Fact]
        public async Task PingScheduler_ShouldReactCorrectlyToPingResponseTimeout()
        {
            var endSource = new SafeTaskCompletionSource();
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
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.Disposed )
                                endSource.Complete();
                        } ) );

            var handshakeRequest = new Protocol.HandshakeRequest( client );
            var serverTask = Task.Factory.StartNew(
                o =>
                {
                    var s = ( ServerMock )o!;
                    s.WaitForClient();
                    s.Read( handshakeRequest.Length );
                    s.SendHandshakeAccepted( 1, Duration.FromSeconds( 0.2 ), Duration.FromSeconds( 0.2 ) );
                    s.Read( Protocol.PacketHeader.Length );
                    Thread.Sleep( 150 );
                    s.Read( Protocol.PacketHeader.Length );
                },
                server );

            await client.StartAsync();
            await serverTask;
            await endSource.Task;

            var serverData = server.GetAllReceived();

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            """
                            ['test'::<ROOT>] [WaitingForMessage] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Message broker server failed to respond with PingResponse packet to 'test' client's PingRequest request in the specified amount of time (200 milliseconds).
                            """
                        ] ),
                    AssertServerData(
                        serverData,
                        (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.PingRequest) ) )
                .Go();
        }

        [Fact]
        public async Task PingScheduler_ShouldReactCorrectlyToInvalidMessageSentByServerWhenExpectingPingResponse()
        {
            var endSource = new SafeTaskCompletionSource();
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
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.Disposed )
                                endSource.Complete();
                        } ) );

            var handshakeRequest = new Protocol.HandshakeRequest( client );
            var serverTask = Task.Factory.StartNew(
                o =>
                {
                    var s = ( ServerMock )o!;
                    s.WaitForClient();
                    s.Read( handshakeRequest.Length );
                    s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 0.2 ) );
                    s.Read( Protocol.PacketHeader.Length );
                    Thread.Sleep( 150 );
                    s.Read( Protocol.PacketHeader.Length );
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                },
                server );

            await client.StartAsync();
            await serverTask;
            await endSource.Task;

            var serverData = server.GetAllReceived();

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid 0 with payload 0 from the server. Encountered 1 error(s):
                            1. Received unexpected client endpoint.
                            """
                        ] ),
                    AssertServerData(
                        serverData,
                        (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.PingRequest) ) )
                .Go();
        }

        [Fact]
        public async Task PingScheduler_ShouldReactCorrectlyToInvalidPingResponsePayloadSentByServer()
        {
            var endSource = new SafeTaskCompletionSource();
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
                    .SetEventHandler(
                        e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerClientEventType.Disposed )
                                endSource.Complete();
                        } ) );

            var handshakeRequest = new Protocol.HandshakeRequest( client );
            var serverTask = Task.Factory.StartNew(
                o =>
                {
                    var s = ( ServerMock )o!;
                    s.WaitForClient();
                    s.Read( handshakeRequest.Length );
                    s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 0.2 ) );
                    s.Read( Protocol.PacketHeader.Length );
                    Thread.Sleep( 150 );
                    s.Read( Protocol.PacketHeader.Length );
                    s.SendPing( endiannessPayload: 1 );
                },
                server );

            await client.StartAsync();
            await serverTask;
            await endSource.Task;

            var serverData = server.GetAllReceived();

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid PingResponse with payload 1 from the server. Encountered 1 error(s):
                            1. Expected endianness verification payload to be 0102fdfe but found 00000001.
                            """
                        ] ),
                    AssertServerData(
                        serverData,
                        (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse),
                        (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.PingRequest) ) )
                .Go();
        }
    }
}
