using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerRemoteClientTests
{
    public class Ping : TestsBase
    {
        [Fact]
        public async Task MessageListener_ShouldReceivePingAndSendPingFromClientOnSchedule()
        {
            var pingResponseCount = 0;
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                () => new TimestampProvider(),
                originalEndPoint,
                MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetClientEventHandlerFactory(
                        _ => e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerRemoteClientEventType.MessageSent
                                && e.GetClientEndpoint() == MessageBrokerClientEndpoint.PingResponse
                                && ++pingResponseCount == 2 ) endSource.Complete();
                        } ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            var clientTask = Task.Factory.StartNew(
                o =>
                {
                    var c = ( ClientMock )o!;
                    c.Connect( endPoint );
                    c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 0.2 ) );
                    c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                    c.SendConfirmHandshakeResponse();
                    Thread.Sleep( 150 );
                    c.SendPing();
                    c.Read( Protocol.PacketHeader.Length );
                    Thread.Sleep( 150 );
                    c.SendPing();
                    c.Read( Protocol.PacketHeader.Length );
                },
                client );

            await clientTask;
            var remoteClient = server.Clients.TryGetByName( "test" );
            await endSource.Task;

            Assertion.All(
                    (remoteClient?.State).TestEquals( MessageBrokerRemoteClientState.Running ),
                    logs.GetAllClient()
                        .TestContainsSequence(
                        [
                            "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 5] PingRequest",
                            "[1::'test'::1] [MessageReceived] [PacketLength: 5] Begin handling PingRequest",
                            "[1::'test'::1] [MessageAccepted] [PacketLength: 5] PingRequest",
                            "[1::'test'::1] [SendingMessage] [PacketLength: 5] PingResponse",
                            "[1::'test'::1] [MessageSent] [PacketLength: 5] PingResponse",
                            "[1::'test'::2] [MessageReceived] [PacketLength: 5] Begin handling PingRequest",
                            "[1::'test'::2] [MessageAccepted] [PacketLength: 5] PingRequest",
                            "[1::'test'::2] [SendingMessage] [PacketLength: 5] PingResponse",
                            "[1::'test'::2] [MessageSent] [PacketLength: 5] PingResponse"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageListener_ShouldDisposeGracefully_WhenClientSendsInvalidRequest()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                () => new TimestampProvider(),
                originalEndPoint,
                MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetClientEventHandlerFactory(
                        _ => e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                                endSource.Complete();
                        } ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            var clientTask = Task.Factory.StartNew(
                o =>
                {
                    var c = ( ClientMock )o!;
                    c.Connect( endPoint );
                    c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                    c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                    c.SendConfirmHandshakeResponse();
                    c.SendConfirmHandshakeResponse();
                },
                client );

            await clientTask;
            await endSource.Task;

            Assertion.All(
                    server.Clients.Count.TestEquals( 0 ),
                    logs.GetAllClient()
                        .TestAny(
                            (x, _) => x.TestStartsWith(
                                """
                                [1::'test'::<ROOT>] [MessageRejected] [PacketLength: 5] Encountered an error:
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid ConfirmHandshakeResponse with payload 16973310 from client [1] 'test'. Encountered 1 error(s):
                                1. Received unexpected server endpoint.
                                """ ) ) )
                .Go();
        }

        [Fact]
        public async Task MessageListener_ShouldDisposeGracefully_WhenClientSendsPingRequestWithInvalidPayload()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                () => new TimestampProvider(),
                originalEndPoint,
                MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetClientEventHandlerFactory(
                        _ => e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                                endSource.Complete();
                        } ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            var clientTask = Task.Factory.StartNew(
                o =>
                {
                    var c = ( ClientMock )o!;
                    c.Connect( endPoint );
                    c.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                    c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                    c.SendConfirmHandshakeResponse();
                    c.SendPing( payload: 1 );
                },
                client );

            await clientTask;
            await endSource.Task;

            Assertion.All(
                    server.Clients.Count.TestEquals( 0 ),
                    logs.GetAllClient()
                        .TestContainsSequence(
                        [
                            "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 5] PingRequest",
                            "[1::'test'::1] [MessageReceived] [PacketLength: 5] Begin handling PingRequest"
                        ] ),
                    logs.GetAllClient()
                        .TestAny(
                            (x, _) => x.TestStartsWith(
                                """
                                [1::'test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Message broker server received an invalid PingRequest with payload 1 from client [1] 'test'. Encountered 1 error(s):
                                1. Expected endianness verification payload to be 0102fdfe but found 00000001.
                                """ ) ) )
                .Go();
        }

        [Fact]
        public async Task MessageListener_ShouldDisposeGracefully_WhenClientFailsToSendAnyRequestInTime()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                () => new TimestampProvider(),
                originalEndPoint,
                MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetClientEventHandlerFactory(
                        _ => e =>
                        {
                            logs.Add( e );
                            if ( e.Type == MessageBrokerRemoteClientEventType.Disposed )
                                endSource.Complete();
                        } ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            var clientTask = Task.Factory.StartNew(
                o =>
                {
                    var c = ( ClientMock )o!;
                    c.Connect( endPoint );
                    c.SendHandshake( "test", Duration.FromSeconds( 0.4 ), Duration.FromTicks( 1 ) );
                    c.Read( Protocol.PacketHeader.Length + Protocol.HandshakeAcceptedResponse.Payload );
                    c.SendConfirmHandshakeResponse();
                },
                client );

            await clientTask;
            await endSource.Task;

            Assertion.All(
                    server.Clients.Count.TestEquals( 0 ),
                    logs.GetAllClient()
                        .TestContainsSequence(
                        [
                            "[1::<ROOT>] [WaitingForMessage]",
                            "[1::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeRequest",
                            "[1::'test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeRequest (IsLittleEndian = True, MessageTimeout = 0.4 second(s), PingInterval = 0.001 second(s))",
                            "[1::'test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeAcceptedResponse",
                            "[1::'test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeAcceptedResponse",
                            "[1::'test'::<ROOT>] [MessageReceived] [PacketLength: 5] ConfirmHandshakeResponse",
                            "[1::'test'::<ROOT>] [MessageAccepted] [PacketLength: 5] ConfirmHandshakeResponse",
                            "[1::'test'::<ROOT>] [WaitingForMessage] Operation cancelled",
                            "[1::'test'::<ROOT>] [Disposing]",
                            "[1::'test'::<ROOT>] [Disposed]"
                        ] ) )
                .Go();
        }
    }
}
