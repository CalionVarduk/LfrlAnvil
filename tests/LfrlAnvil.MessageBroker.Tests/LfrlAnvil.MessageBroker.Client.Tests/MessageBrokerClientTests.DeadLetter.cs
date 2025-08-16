using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Functional;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests
{
    public class DeadLetter : TestsBase, IClassFixture<SharedResourceFixture>
    {
        private readonly ValueTaskDelaySource _sharedDelaySource;

        public DeadLetter(SharedResourceFixture fixture)
        {
            _sharedDelaySource = fixture.DelaySource;
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 5 )]
        public async Task QueryDeadLetterAsync_ShouldQueryDeadLetterCorrectly(int readCount)
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

            var nextExpirationAt = TimestampProvider.Shared.GetNow();
            await server.EstablishHandshake( client );
            var serverTask = server.GetTask(
                s =>
                {
                    s.ReadDeadLetterQuery();
                    s.SendDeadLetterQueryResponse( 100, readCount, nextExpirationAt );
                } );

            var result = await client.QueryDeadLetterAsync( 1, readCount );
            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TotalCount.TestEquals( 100 ),
                    result.Value.MaxReadCount.TestEquals( readCount ),
                    result.Value.NextExpirationAt.TestEquals( nextExpirationAt ),
                    result.Value.ToString()
                        .TestEquals( $"TotalCount = 100, MaxReadCount = {readCount}, NextExpirationAt = {nextExpirationAt}" ),
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (start)",
                                $"[QueryingDeadLetter] Client = [1] 'test', TraceId = 1, QueueId = 1, ReadCount = {readCount}",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQueryResponse, Length = 21)",
                                $"[DeadLetterQueried] Client = [1] 'test', TraceId = 1, TotalCount = 100, MaxReadCount = {readCount}, NextExpirationAt = {nextExpirationAt}",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task QueryDeadLetterAsync_ShouldQueryDeadLetterCorrectly_WhenResultTotalCountIsEqualToZero()
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
                    s.ReadDeadLetterQuery();
                    s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero );
                } );

            var result = await client.QueryDeadLetterAsync( 1, 100 );
            await serverTask;

            Assertion.All(
                    result.Exception.TestNull(),
                    result.Value.TotalCount.TestEquals( 0 ),
                    result.Value.MaxReadCount.TestEquals( 0 ),
                    result.Value.NextExpirationAt.TestEquals( Timestamp.Zero ),
                    result.Value.ToString().TestEquals( "TotalCount = 0" ),
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (start)",
                                $"[QueryingDeadLetter] Client = [1] 'test', TraceId = 1, QueueId = 1, ReadCount = 100",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQueryResponse, Length = 21)",
                                $"[DeadLetterQueried] Client = [1] 'test', TraceId = 1, TotalCount = 0",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task QueryDeadLetterAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
        {
            var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            await client.DisposeAsync();

            Exception? exception = null;
            try
            {
                _ = await client.QueryDeadLetterAsync( 1, 0 );
            }
            catch ( Exception exc )
            {
                exception = exc;
            }

            exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public void QueryDeadLetterAsync_ShouldThrowArgumentOutOfRangeException_WhenQueueIdIsLessThanOne()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.QueryDeadLetterAsync( 0, 0 ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void QueryDeadLetterAsync_ShouldThrowArgumentOutOfRangeException_WhenReadCountIsLessThanZero()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.QueryDeadLetterAsync( 1, -1 ) );
            action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
        }

        [Fact]
        public void QueryDeadLetterAsync_ShouldThrowMessageBrokerClientStateException_WhenClientIsNotRunning()
        {
            using var client = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
            var action = Lambda.Of( () => client.QueryDeadLetterAsync( 1, 0 ) );
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
        public async Task QueryDeadLetterAsync_ShouldReturnErrorAndDisposeClient_WhenServerDoesNotRespondInTime()
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
            var result = await client.QueryDeadLetterAsync( 1, 0 );

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientResponseTimeoutException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.RequestEndpoint.TestEquals( MessageBrokerServerEndpoint.DeadLetterQuery ) ) ),
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 1, QueueId = 1, ReadCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Server failed to respond to 'test' client's DeadLetterQuery in the specified amount of time (1 second(s)).
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket().Length.TestGreaterThan( 0 ) )
                .Go();
        }

        [Fact]
        public async Task QueryDeadLetterAsync_ShouldReturnError_WhenClientIsDisposedBeforeServerResponds()
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
                            traceStart: e =>
                            {
                                if ( e.Type == MessageBrokerClientTraceEventType.Ping )
                                    endSource.Complete( e.Source.Client.DisposeAsync().AsTask() );
                            } ) ) );

            await server.EstablishHandshake( client, pingInterval: Duration.FromSeconds( 0.2 ) );
            var result = await client.QueryDeadLetterAsync( 1, 0 );

            await endSource.Task.Unwrap();

            result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( exc => exc.Client.TestRefEquals( client ) ).Go();
        }

        [Fact]
        public async Task
            QueryDeadLetterAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithDeadLetterQueryResponseWithInvalidPayload()
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
                    s.ReadDeadLetterQuery();
                    s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero, payload: 15 );
                } );

            var result = await client.QueryDeadLetterAsync( 1, 0 );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.DeadLetterQueryResponse ) ) ),
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 1, QueueId = 1, ReadCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQueryResponse, Length = 20)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid DeadLetterQueryResponse from the server. Encountered 1 error(s):
                                1. Expected header payload to be 16 but found 15.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 20)"
                        ] ) )
                .Go();
        }

        [Theory]
        [InlineData(
            -2,
            0,
            0,
            """
            Encountered 1 error(s):
            1. Expected total count to not be less than -1 but found -2.
            """ )]
        [InlineData(
            -1,
            1,
            2,
            """
            Encountered 2 error(s):
            1. Expected max read count to be equal to 0 but found 1.
            2. Expected next expiration at to be equal to 0 but found 2 ticks.
            """ )]
        [InlineData(
            0,
            -1,
            2,
            """
            Encountered 2 error(s):
            1. Expected max read count to be in [0, 0] range but found -1.
            2. Expected next expiration at to be equal to 0 but found 2 ticks.
            """ )]
        [InlineData(
            10,
            11,
            0,
            """
            Encountered 1 error(s):
            1. Expected max read count to be in [0, 10] range but found 11.
            """ )]
        public async Task
            QueryDeadLetterAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithDeadLetterQueryResponseWithInvalidValues(
                int totalCount,
                int maxReadCount,
                long nextExpirationAtTicks,
                string expectedError)
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
                    s.ReadDeadLetterQuery();
                    s.SendDeadLetterQueryResponse( totalCount, maxReadCount, new Timestamp( nextExpirationAtTicks ) );
                } );

            var result = await client.QueryDeadLetterAsync( 1, 0 );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerClientEndpoint.DeadLetterQueryResponse ) ) ),
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 1, QueueId = 1, ReadCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQueryResponse, Length = 21)",
                                $"""
                                 [Error] Client = [1] 'test', TraceId = 1
                                 LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid DeadLetterQueryResponse from the server. {expectedError}
                                 """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task QueryDeadLetterAsync_ShouldReturnError_WhenServerRespondsWithFailureResponse()
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
                    s.ReadDeadLetterQuery();
                    s.SendDeadLetterQueryResponse( -1, 0, Timestamp.Zero );
                } );

            var result = await client.QueryDeadLetterAsync( 1, 0 );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Running ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientRequestException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( MessageBrokerServerEndpoint.DeadLetterQuery ) ) ),
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 1, QueueId = 1, ReadCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQueryResponse, Length = 21)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Server rejected an invalid DeadLetterQuery sent by client 'test'. Encountered 1 error(s):
                                1. Queue with ID 1 does not exist.
                                """,
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task DeadLetterQueryAsync_ShouldReturnErrorAndDisposeClient_WhenServerRespondsWithInvalidEndpoint()
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
                    s.ReadDeadLetterQuery();
                    s.Send( [ 0, 0, 0, 0, 0 ] );
                } );

            var result = await client.QueryDeadLetterAsync( 1, 0 );
            await serverTask;

            Assertion.All(
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    result.Exception.TestType()
                        .Exact<MessageBrokerClientProtocolException>(
                            exc => Assertion.All(
                                exc.Client.TestRefEquals( client ),
                                exc.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ) ) ),
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 1, QueueId = 1, ReadCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (DeadLetterQuery, Length = 13)",
                                """
                                [Error] Client = [1] 'test', TraceId = 1
                                LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                                1. Received unexpected client endpoint.
                                """,
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 1 (end)"
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
