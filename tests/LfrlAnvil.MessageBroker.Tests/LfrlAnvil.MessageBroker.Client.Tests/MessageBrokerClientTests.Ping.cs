using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests
{
    public class Ping : TestsBase
    {
        [Fact]
        public async Task PingScheduler_ShouldSendPingAndReceivePongFromServerOnSchedule()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
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
                                && e.GetClientEndpoint() == MessageBrokerClientEndpoint.Pong )
                                endSource.Complete();
                        } ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    Thread.Sleep( 150 );
                    s.ReadPing();
                    s.SendPong();
                    Thread.Sleep( 150 );
                    s.ReadPing();
                    s.SendPong();
                } );

            await serverTask;
            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 5] Ping",
                            "['test'::1] [MessageSent] [PacketLength: 5] Ping",
                            "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] Pong",
                            "['test'::1] [MessageReceived] [PacketLength: 5] Begin handling Pong",
                            "['test'::1] [MessageAccepted] [PacketLength: 5] Pong",
                            "['test'::2] [SendingMessage] [PacketLength: 5] Ping",
                            "['test'::2] [MessageSent] [PacketLength: 5] Ping",
                            "['test'::2] [MessageReceived] [PacketLength: 5] Begin handling Pong",
                            "['test'::2] [MessageAccepted] [PacketLength: 5] Pong"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task PingScheduler_ShouldReactCorrectlyToClientBeingDisposedBeforeReceivingPongFromServer()
        {
            var endSource = new SafeTaskCompletionSource<Task>();
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
                            if ( e.Type == MessageBrokerClientEventType.MessageSent
                                && e.GetServerEndpoint() == MessageBrokerServerEndpoint.Ping )
                                endSource.Complete( e.Client.DisposeAsync().AsTask() );
                        } ) );

            await server.EstablishHandshake(
                client,
                messageTimeout: Duration.FromSeconds( 0.2 ),
                pingInterval: Duration.FromSeconds( 0.2 ) );

            var serverTask = server.GetTask(
                s =>
                {
                    Thread.Sleep( 150 );
                    s.ReadPing();
                } );

            await serverTask;
            await endSource.Task.Unwrap();

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            "['test'::1] [SendingMessage] [PacketLength: 5] Ping",
                            "['test'::1] [MessageSent] [PacketLength: 5] Ping"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task PingScheduler_ShouldReactCorrectlyToPongTimeout()
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

            await server.EstablishHandshake(
                client,
                messageTimeout: Duration.FromSeconds( 0.2 ),
                pingInterval: Duration.FromSeconds( 0.2 ) );

            var serverTask = server.GetTask(
                s =>
                {
                    Thread.Sleep( 150 );
                    s.ReadPing();
                } );

            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            """
                            ['test'::<ROOT>] [WaitingForMessage] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Message broker server failed to respond to 'test' client's Ping in the specified amount of time (200 milliseconds).
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task PingScheduler_ShouldReactCorrectlyToInvalidMessageSentByServerWhenExpectingPong()
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

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    Thread.Sleep( 150 );
                    s.ReadPing();
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                            1. Received unexpected client endpoint.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task PingScheduler_ShouldReactCorrectlyToInvalidPongPayloadSentByServer()
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

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    Thread.Sleep( 150 );
                    s.ReadPing();
                    s.SendPong( endiannessPayload: 1 );
                } );

            await serverTask;
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .TestContainsSequence(
                        [
                            """
                            ['test'::1] [MessageRejected] [PacketLength: 5] Encountered an error:
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid Pong from the server. Encountered 1 error(s):
                            1. Expected endianness verification payload to be 0102fdfe but found 00000001.
                            """
                        ] ) )
                .Go();
        }
    }
}
