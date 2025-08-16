using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerRemoteClientTests
{
    public class Ping : TestsBase, IClassFixture<SharedResourceFixture>
    {
        private readonly ValueTaskDelaySource _sharedDelaySource;

        public Ping(SharedResourceFixture fixture)
        {
            _sharedDelaySource = fixture.DelaySource;
        }

        [Fact]
        public async Task MessageListener_ShouldReceivePingAndSendPongFromClientOnSchedule()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var logs = new ClientEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => logs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ping )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, pingInterval: Duration.FromSeconds( 0.2 ) );
            await client.GetTask(
                c =>
                {
                    Thread.Sleep( 150 );
                    c.SendPing();
                    c.ReadPong();
                    Thread.Sleep( 150 );
                    c.SendPing();
                    c.ReadPong();
                } );

            var remoteClient = server.Clients.TryGetByName( "test" );
            await endSource.Task;

            Assertion.All(
                    (remoteClient?.State).TestEquals( MessageBrokerRemoteClientState.Running ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (Pong, Length = 5)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (Pong, Length = 5)",
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ping] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (Ping, Length = 5)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (Ping, Length = 5)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (Pong, Length = 5)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (Pong, Length = 5)",
                                "[Trace:Ping] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Ping, Length = 5)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Ping, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageListener_ShouldDisposeGracefully_WhenClientSendsInvalidRequest()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => logs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c => c.SendConfirmHandshakeResponse( payload: 0 ) );
            await endSource.Task;

            Assertion.All(
                    server.Clients.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (start)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid ConfirmHandshakeResponse from client [1] 'test'. Encountered 1 error(s):
                                1. Received unexpected server endpoint.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ConfirmHandshakeResponse, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageListener_ShouldDisposeGracefully_WhenClientSendsPingWithInvalidPayload()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => logs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ping )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server );
            await client.GetTask( c => c.SendPing( payload: 1 ) );
            await endSource.Task;

            Assertion.All(
                    server.Clients.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (Ping, Length = 5)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid Ping from client [1] 'test'. Encountered 1 error(s):
                                1. Expected endianness verification payload to be 0102fdfe but found 00000001.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:Ping] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Ping, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task MessageListener_ShouldDisposeGracefully_WhenClientFailsToSendAnyRequestInTime()
        {
            Exception? exception = null;
            var endSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => logs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Dispose )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, messageTimeout: Duration.FromSeconds( 0.4 ), pingInterval: Duration.FromTicks( 1 ) );
            var remoteClient = server.Clients.TryGetById( 1 );
            await endSource.Task;

            Assertion.All(
                    exception.TestType()
                        .Exact<MessageBrokerRemoteClientRequestTimeoutException>( exc => exc.Client.TestRefEquals( remoteClient ) ),
                    server.Clients.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Dispose] Client = [1] 'test', TraceId = 1 (start)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientRequestTimeoutException: Client [1] 'test' failed to send a request to the server in the specified amount of time (0.401 second(s)).
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:Dispose] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            (e, _) => e.TestEquals( "[AwaitPacket] Client = [1] 'test'" ),
                            (e, _) => e.TestStartsWith(
                                """
                                [AwaitPacket] Client = [1] 'test'
                                System.OperationCanceledException:
                                """ )
                        ] ) )
                .Go();
        }
    }
}
