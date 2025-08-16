using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests
{
    public class Ping : TestsBase, IClassFixture<SharedResourceFixture>
    {
        private readonly ValueTaskDelaySource _sharedDelaySource;

        public Ping(SharedResourceFixture fixture)
        {
            _sharedDelaySource = fixture.DelaySource;
        }

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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerClientTraceEventType.Ping )
                                        endSource.Complete();
                                } ) ) ) );

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
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (start)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (Pong, Length = 5)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (Pong, Length = 5)",
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ping] Client = [1] 'test', TraceId = 2 (start)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (Ping, Length = 5)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (Ping, Length = 5)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (Pong, Length = 5)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (Pong, Length = 5)",
                                "[Trace:Ping] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Pong, Length = 5)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Pong, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task PingScheduler_ShouldReactCorrectlyToClientBeingDisposedBeforeReceivingPongFromServer()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var disposeSource = new SafeTaskCompletionSource<Task>();
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
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerClientTraceEventType.Dispose or MessageBrokerClientTraceEventType.Ping )
                                        endSource.Complete();
                                },
                                sendPacket: e =>
                                {
                                    if ( e.Type == MessageBrokerClientSendPacketEventType.Sent
                                        && e.Packet.Endpoint == MessageBrokerServerEndpoint.Ping )
                                        disposeSource.Complete( e.Source.Client.DisposeAsync().AsTask() );
                                } ) ) ) );

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
            await disposeSource.Task.Unwrap();
            await endSource.Task;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (start)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientDisposedException: Operation has been cancelled because client 'test' is disposed.
                                """,
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Dispose] Client = [1] 'test', TraceId = 2 (start)",
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:Dispose] Client = [1] 'test', TraceId = 2 (end)",
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (e, _) => e.TestStartsWith(
                                """
                                [AwaitPacket] Client = [1] 'test'
                                System.OperationCanceledException:
                                """ )
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerClientTraceEventType.Ping )
                                        endSource.Complete();
                                } ) ) ) );

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
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (start)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Server failed to respond to 'test' client's Ping in the specified amount of time (0.2 second(s)).
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerClientTraceEventType.Ping )
                                        endSource.Complete();
                                } ) ) ) );

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
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (start)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                                1. Received unexpected client endpoint.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (<unrecognized-endpoint-0>, Length = 5)"
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
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerClientTraceEventType.Ping )
                                        endSource.Complete();
                                } ) ) ) );

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
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (start)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (Pong, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid Pong from the server. Encountered 1 error(s):
                                1. Expected endianness verification payload to be 0102fdfe but found 00000001.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Pong, Length = 5)"
                        ] ) )
                .Go();
        }
    }
}
