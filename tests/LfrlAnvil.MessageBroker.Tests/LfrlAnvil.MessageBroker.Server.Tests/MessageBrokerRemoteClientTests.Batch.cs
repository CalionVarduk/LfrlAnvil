using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerRemoteClientTests
{
    public class Batch : TestsBase, IClassFixture<SharedResourceFixture>
    {
        private readonly ValueTaskDelaySource _sharedDelaySource;

        public Batch(SharedResourceFixture fixture)
        {
            _sharedDelaySource = fixture.DelaySource;
        }

        [Fact]
        public async Task ReceivingValidBatch_ShouldHandleElementPacketsCorrectly()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 3 );
            var pushMessageContinuation = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => logs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceStart: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                        pushMessageContinuation.Task.Wait();
                                },
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                        or MessageBrokerRemoteClientTraceEventType.PushMessage
                                        or MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                        endSource.Complete();

                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                        pushMessageContinuation.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, maxBatchPacketCount: 10, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await client.GetTask(
                c =>
                {
                    c.SendBatch(
                    [
                        ClientMock.PrepareBindPublisherRequest( "foo" ),
                        ClientMock.PreparePushMessage( 1, [ 1, 2, 3 ], confirm: false ),
                        ClientMock.PrepareUnbindPublisherRequest( 1 )
                    ] );

                    c.ReadPublisherBoundResponse();
                    c.ReadPublisherUnboundResponse();
                } );

            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', IsEphemeral = True",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                                "[PublisherBound] Client = [1] 'test', TraceId = 1, Channel = [1] 'foo' (created), Stream = [1] 'foo' (created)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 13)",
                                "[PushingMessage] Client = [1] 'test', TraceId = 2, Length = 3, ChannelId = 1, Confirm = False",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 13)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', MessageId = 0",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 3 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[UnbindingPublisher] Client = [1] 'test', TraceId = 3, ChannelId = 1",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[PublisherUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo' (removed), Stream = [1] 'foo' (removed)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (PublisherUnboundResponse, Length = 6)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (PublisherUnboundResponse, Length = 6)",
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 3 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 40)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 40), PacketCount = 3",
                            "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 11)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 13)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherRequest, Length = 9)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingValidBatch_ShouldHandleElementPacketsCorrectly_WithLargeBatch()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 3 );
            var pushMessageContinuation = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            var streamRemoved = Atomic.Create( false );

            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => logs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceStart: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                        pushMessageContinuation.Task.Wait();
                                },
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                        or MessageBrokerRemoteClientTraceEventType.PushMessage
                                        or MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                        endSource.Complete();

                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                        pushMessageContinuation.Complete();
                                },
                                publisherUnbound: e => streamRemoved.Value = e.StreamRemoved ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, maxBatchPacketCount: 10, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await client.GetTask(
                c =>
                {
                    c.SendBatch(
                    [
                        ClientMock.PrepareBindPublisherRequest( "foo" ),
                        ClientMock.PreparePushMessage(
                            1,
                            Enumerable.Range( 0, ( int )MemorySize.FromKilobytes( 20 ).Bytes ).Select( static x => ( byte )x ).ToArray(),
                            confirm: false ),
                        ClientMock.PrepareUnbindPublisherRequest( 1 )
                    ] );

                    c.ReadPublisherBoundResponse();
                    c.ReadPublisherUnboundResponse();
                } );

            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', IsEphemeral = True",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                                "[PublisherBound] Client = [1] 'test', TraceId = 1, Channel = [1] 'foo' (created), Stream = [1] 'foo' (created)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 20490)",
                                "[PushingMessage] Client = [1] 'test', TraceId = 2, Length = 20480, ChannelId = 1, Confirm = False",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 20490)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Stream = [1] 'foo', MessageId = 0",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 3 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[UnbindingPublisher] Client = [1] 'test', TraceId = 3, ChannelId = 1",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (UnbindPublisherRequest, Length = 9)",
                                $"[PublisherUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo' (removed), Stream = [1] 'foo'{(streamRemoved.Value ? " (removed)" : string.Empty)}",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (PublisherUnboundResponse, Length = 6)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (PublisherUnboundResponse, Length = 6)",
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 3 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 20517)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 20517), PacketCount = 3",
                            "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 11)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 20490)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (UnbindPublisherRequest, Length = 9)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingValidBatch_ShouldHandleElementPacketsCorrectly_WhenLastElementEndsWithEmptyPacket()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var pingContinuation = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => logs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceStart: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Ping )
                                        pingContinuation.Task.Wait();
                                },
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                        or MessageBrokerRemoteClientTraceEventType.Ping )
                                        endSource.Complete();

                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                        pingContinuation.Complete();
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, maxBatchPacketCount: 10, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await client.GetTask(
                c =>
                {
                    c.SendBatch(
                    [
                        ClientMock.PrepareBindPublisherRequest( "foo" ),
                        ClientMock.PreparePing()
                    ] );

                    c.ReadPublisherBoundResponse();
                    c.ReadPong();
                } );

            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', IsEphemeral = True",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                                "[PublisherBound] Client = [1] 'test', TraceId = 1, Channel = [1] 'foo' (created), Stream = [1] 'foo' (created)",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
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
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 23)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 23), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (BindPublisherRequest, Length = 11)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Ping, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingUnsupportedBatch_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
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
            await client.GetTask( c => c.SendHeader( MessageBrokerServerEndpoint.Batch, 0 ) );
            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (start)",
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 5)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 5)
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid Batch from client [1] 'test'. Encountered 1 error(s):
                            1. Received unexpected server endpoint.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithInvalidPayload_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
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
            await client.EstablishHandshake( server, maxBatchPacketCount: 2, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await client.GetTask( c => c.SendBatch( [ ], payload: 1 ) );
            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (start)",
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 6)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 6)
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid Batch from client [1] 'test'. Encountered 1 error(s):
                            1. Expected header payload to be at least 2 but found 1.
                            """
                        ] ) )
                .Go();
        }

        [Theory]
        [InlineData( -1 )]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 4 )]
        public async Task ReceivingBatchWithInvalidPacketCount_ShouldDisposeClient(short packetCount)
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
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
            await client.EstablishHandshake( server, maxBatchPacketCount: 3, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await client.GetTask( c => c.SendBatch( [ ], packetCount ) );
            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (start)",
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 7)",
                            $"""
                             [AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 7)
                             LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid Batch from client [1] 'test'. Encountered 1 error(s):
                             1. Expected batch packet count to be in [2, 3] range but found {packetCount}.
                             """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithIncompleteElementHeader_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
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
            await client.EstablishHandshake( server, maxBatchPacketCount: 2, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await client.GetTask( c => c.SendBatch( [ [ 1, 0, 0, 0 ] ], packetCount: 2 ) );
            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (start)",
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 11)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 11)
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid Batch from client [1] 'test'. Encountered 1 error(s):
                            1. Expected length of the packet at index 0 in batch to be greater than or equal to 5 but found 4.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithTooLargePushMessageElementPayload_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetNetworkPacketOptions(
                        MessageBrokerServerNetworkPacketOptions.Default.SetMaxMessageLength( MemorySize.FromKilobytes( 32 ) ) )
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
            await client.EstablishHandshake( server, maxBatchPacketCount: 2, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await client.GetTask(
                c => c.SendBatch(
                    [
                        ClientMock.PreparePushMessage(
                            1,
                            [ ],
                            confirm: false,
                            payload: ( uint )MemorySize.BytesPerKilobyte * 32
                            - Protocol.PacketHeader.Length
                            - Protocol.MessageNotificationHeader.Payload
                            + Protocol.PushMessageHeader.Length
                            + 1 )
                    ],
                    packetCount: 2 ) );

            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (start)",
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 17)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 17), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 32734)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 32734)
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessage from client [1] 'test'. Encountered 1 error(s):
                            1. Expected total packet length to be in [5, 32733] range but found 32734.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithElementPayloadExceedingBatchEnd_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
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
            await client.EstablishHandshake( server, maxBatchPacketCount: 2, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await client.GetTask(
                c => c.SendBatch(
                    [ ClientMock.PreparePushMessage( 1, [ ], confirm: false, payload: Protocol.PushMessageHeader.Length + 1 ) ],
                    packetCount: 2 ) );

            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (start)",
                                "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 17)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 17), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 11)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 11)
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessage from client [1] 'test'. Encountered 1 error(s):
                            1. Expected payload of the packet at index 0 in batch to be less than or equal to 5 but found 6.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithTooMuchData_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
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
            await client.EstablishHandshake( server, maxBatchPacketCount: 2, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await client.GetTask(
                c => c.SendBatch(
                    [
                        ClientMock.PreparePushMessage( 1, [ 1 ], confirm: false ),
                        ClientMock.PreparePushMessage( 1, [ 2, 3 ], confirm: false ),
                        [ 0 ]
                    ],
                    packetCount: 2 ) );

            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:Unexpected] Client = [1] 'test', TraceId = {t.Id} (start)",
                                $"[Deactivating] Client = [1] 'test', TraceId = {t.Id}, IsAlive = False",
                                $"[Deactivated] Client = [1] 'test', TraceId = {t.Id}, IsAlive = False",
                                $"[Trace:Unexpected] Client = [1] 'test', TraceId = {t.Id} (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 31)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 31), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 11)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 12)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 31)
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid Batch from client [1] 'test'. Encountered 1 error(s):
                            1. Expected an end of the batch packet but found 1 remaining byte(s).
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Sending_ShouldHandleElementsCorrectly()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 11 );
            var pingContinuation = new SafeTaskCompletionSource();
            var pushMessageContinuation = new SafeTaskCompletionSource();
            var messageNotificationContinuation = new SafeTaskCompletionSource();
            var sendContinuation = new SafeTaskCompletionSource();
            var pingCount = Atomic.Create( 0 );
            var pongCount = Atomic.Create( 0 );
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        c => c.Id == 2
                            ? logs.GetLogger(
                                MessageBrokerRemoteClientLogger.Create(
                                    pushingMessage: _ => pushMessageContinuation.Complete(),
                                    listenerUnbound: _ => sendContinuation.Complete(),
                                    sendingSenderName: _ => messageNotificationContinuation.Complete(),
                                    traceStart: e =>
                                    {
                                        if ( e.Type != MessageBrokerRemoteClientTraceEventType.Ping )
                                            return;

                                        if ( pingCount.Value == 1 )
                                            pingContinuation.Task.Wait();

                                        ++pingCount.Value;
                                    },
                                    traceEnd: e =>
                                    {
                                        if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                            or MessageBrokerRemoteClientTraceEventType.BindListener
                                            or MessageBrokerRemoteClientTraceEventType.UnbindPublisher
                                            or MessageBrokerRemoteClientTraceEventType.UnbindListener
                                            or MessageBrokerRemoteClientTraceEventType.DeadLetterQuery
                                            or MessageBrokerRemoteClientTraceEventType.PushMessage
                                            or MessageBrokerRemoteClientTraceEventType.Ping
                                            or MessageBrokerRemoteClientTraceEventType.SystemNotification
                                            or MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                            endSource.Complete();
                                    },
                                    sendPacket: e =>
                                    {
                                        if ( e.Type == MessageBrokerRemoteClientSendPacketEventType.Sending
                                            && e.Packet.Endpoint == MessageBrokerClientEndpoint.Pong
                                            && pongCount.Value == 0 )
                                        {
                                            pingContinuation.Complete();
                                            ++pongCount.Value;
                                            sendContinuation.Task.Wait();
                                            Task.Delay( 50 ).Wait();
                                        }
                                    } ) )
                            : null ) );

            await server.StartAsync();

            using var client1 = new ClientMock();
            await client1.EstablishHandshake( server );
            await client1.GetTask(
                c =>
                {
                    c.SendBindPublisherRequest( "foo" );
                    c.ReadPublisherBoundResponse();
                } );

            using var client2 = new ClientMock();
            await client2.EstablishHandshake(
                server,
                name: "test2",
                maxBatchPacketCount: 20,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ),
                synchronizeExternalObjectNames: true );

            await client2.GetTask(
                c =>
                {
                    c.SendPing();
                    c.SendPing();
                    c.SendBindPublisherRequest( "bar" );
                    c.SendBindListenerRequest( "foo", false );
                    c.SendPushMessage( 2, [ 1 ] );
                } );

            await pushMessageContinuation.Task;
            await Task.Delay( 50 );
            await client1.GetTask( c => c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false ) );

            await messageNotificationContinuation.Task;
            await client2.GetTask(
                c =>
                {
                    c.SendDeadLetterQuery( 1, 0 );
                    c.SendUnbindPublisherRequest( 2 );
                    c.SendUnbindListenerRequest( 1 );

                    c.ReadPong();
                    c.ReadBatch(
                    [
                        (MessageBrokerClientEndpoint.Pong, Protocol.PacketHeader.Length),
                        (MessageBrokerClientEndpoint.PublisherBoundResponse,
                            Protocol.PacketHeader.Length + Protocol.PublisherBoundResponse.Payload),
                        (MessageBrokerClientEndpoint.ListenerBoundResponse,
                            Protocol.PacketHeader.Length + Protocol.ListenerBoundResponse.Payload),
                        (MessageBrokerClientEndpoint.MessageAcceptedResponse,
                            Protocol.PacketHeader.Length + Protocol.MessageAcceptedResponse.Payload),
                        (MessageBrokerClientEndpoint.SystemNotification,
                            Protocol.PacketHeader.Length
                            + ( int )new Protocol.ObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 1, "test" ).Header
                                .Payload),
                        (MessageBrokerClientEndpoint.SystemNotification,
                            Protocol.PacketHeader.Length
                            + ( int )new Protocol.ObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "foo" ).Header
                                .Payload),
                        (MessageBrokerClientEndpoint.MessageNotification,
                            Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload + 3),
                        (MessageBrokerClientEndpoint.DeadLetterQueryResponse,
                            Protocol.PacketHeader.Length + Protocol.DeadLetterQueryResponse.Payload),
                        (MessageBrokerClientEndpoint.PublisherUnboundResponse,
                            Protocol.PacketHeader.Length + Protocol.PublisherUnboundResponse.Payload),
                        (MessageBrokerClientEndpoint.ListenerUnboundResponse,
                            Protocol.PacketHeader.Length + Protocol.ListenerUnboundResponse.Payload)
                    ] );
                } );

            await endSource.Task;

            logs.GetAll()
                .Skip( 2 )
                .TestSequence(
                [
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:Ping] Client = [2] 'test2', TraceId = 2 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 2, Packet = (Ping, Length = 5)",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 2, Packet = (Ping, Length = 5)",
                        "[SendPacket:Sending] Client = [2] 'test2', TraceId = 2, Packet = (Batch, Length = 161), PacketCount = 10",
                        "[SendPacket:Sent] Client = [2] 'test2', TraceId = 2, Packet = (Batch, Length = 161)",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 2, BatchTraceId = 2, Packet = (Pong, Length = 5)",
                        "[Trace:Ping] Client = [2] 'test2', TraceId = 2 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:BindPublisher] Client = [2] 'test2', TraceId = 3 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 3, Packet = (BindPublisherRequest, Length = 11)",
                        "[BindingPublisher] Client = [2] 'test2', TraceId = 3, ChannelName = 'bar', IsEphemeral = True",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 3, Packet = (BindPublisherRequest, Length = 11)",
                        "[PublisherBound] Client = [2] 'test2', TraceId = 3, Channel = [2] 'bar' (created), Stream = [2] 'bar' (created)",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 3, BatchTraceId = 2, Packet = (PublisherBoundResponse, Length = 14)",
                        "[Trace:BindPublisher] Client = [2] 'test2', TraceId = 3 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:BindListener] Client = [2] 'test2', TraceId = 4 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 4, Packet = (BindListenerRequest, Length = 43)",
                        "[BindingListener] Client = [2] 'test2', TraceId = 4, ChannelName = 'foo', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = False",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 4, Packet = (BindListenerRequest, Length = 43)",
                        "[ListenerBound] Client = [2] 'test2', TraceId = 4, Channel = [1] 'foo', Queue = [1] 'foo' (created)",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 4, BatchTraceId = 2, Packet = (ListenerBoundResponse, Length = 14)",
                        "[Trace:BindListener] Client = [2] 'test2', TraceId = 4 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:PushMessage] Client = [2] 'test2', TraceId = 5 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 5, Packet = (PushMessage, Length = 11)",
                        "[PushingMessage] Client = [2] 'test2', TraceId = 5, Length = 1, ChannelId = 2, Confirm = True",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 5, Packet = (PushMessage, Length = 11)",
                        "[MessagePushed] Client = [2] 'test2', TraceId = 5, Channel = [2] 'bar', Stream = [2] 'bar', MessageId = 0",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 5, BatchTraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                        "[Trace:PushMessage] Client = [2] 'test2', TraceId = 5 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:SystemNotification] Client = [2] 'test2', TraceId = 6 (start)",
                        "[SendingSenderName] Client = [2] 'test2', TraceId = 6, Sender = [1] 'test'",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 6, BatchTraceId = 2, Packet = (SystemNotification, Length = 14)",
                        "[SystemNotificationSent] Client = [2] 'test2', TraceId = 6, Type = SenderName",
                        "[Trace:SystemNotification] Client = [2] 'test2', TraceId = 6 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:DeadLetterQuery] Client = [2] 'test2', TraceId = 7 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 7, Packet = (DeadLetterQuery, Length = 13)",
                        "[QueryingDeadLetter] Client = [2] 'test2', TraceId = 7, QueueId = 1, ReadCount = 0",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 7, Packet = (DeadLetterQuery, Length = 13)",
                        "[DeadLetterQueried] Client = [2] 'test2', TraceId = 7, Queue = [1] 'foo', TotalCount = 0",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 7, BatchTraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                        "[Trace:DeadLetterQuery] Client = [2] 'test2', TraceId = 7 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:UnbindPublisher] Client = [2] 'test2', TraceId = 8 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 8, Packet = (UnbindPublisherRequest, Length = 9)",
                        "[UnbindingPublisher] Client = [2] 'test2', TraceId = 8, ChannelId = 2",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 8, Packet = (UnbindPublisherRequest, Length = 9)",
                        "[PublisherUnbound] Client = [2] 'test2', TraceId = 8, Channel = [2] 'bar' (removed), Stream = [2] 'bar' (removed)",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 8, BatchTraceId = 2, Packet = (PublisherUnboundResponse, Length = 6)",
                        "[Trace:UnbindPublisher] Client = [2] 'test2', TraceId = 8 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:UnbindListener] Client = [2] 'test2', TraceId = 9 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 9, Packet = (UnbindListenerRequest, Length = 9)",
                        "[UnbindingListener] Client = [2] 'test2', TraceId = 9, ChannelId = 1",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 9, Packet = (UnbindListenerRequest, Length = 9)",
                        "[ListenerUnbound] Client = [2] 'test2', TraceId = 9, Channel = [1] 'foo', Queue = [1] 'foo' (removed)",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 9, BatchTraceId = 2, Packet = (ListenerUnboundResponse, Length = 6)",
                        "[Trace:UnbindListener] Client = [2] 'test2', TraceId = 9 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:SystemNotification] Client = [2] 'test2', TraceId = 10 (start)",
                        "[SendingStreamName] Client = [2] 'test2', TraceId = 10, Stream = [1] 'foo'",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 10, BatchTraceId = 2, Packet = (SystemNotification, Length = 13)",
                        "[SystemNotificationSent] Client = [2] 'test2', TraceId = 10, Type = StreamName",
                        "[Trace:SystemNotification] Client = [2] 'test2', TraceId = 10 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:MessageNotification] Client = [2] 'test2', TraceId = 11 (start)",
                        "[ProcessingMessage] Client = [2] 'test2', TraceId = 11, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Queue = [1] 'foo', MessageId = 0, Retry = 0, Redelivery = 0, Length = 3",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 11, BatchTraceId = 2, Packet = (MessageNotification, Length = 48)",
                        "[MessageProcessed] Client = [2] 'test2', TraceId = 11, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Queue = [1] 'foo', MessageId = 0, Retry = 0, Redelivery = 0",
                        "[Trace:MessageNotification] Client = [2] 'test2', TraceId = 11 (end)"
                    ] ),
                ] )
                .Go();
        }

        [Fact]
        public async Task Sending_ShouldHandleElementsCorrectly_WhenPacketCountLimitIsReached()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 4 );
            var bindPublisherContinuation = new SafeTaskCompletionSource();
            var sendContinuation = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        _ => logs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                deadLetterQueried: _ => sendContinuation.Complete(),
                                traceStart: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindPublisher )
                                        bindPublisherContinuation.Task.Wait();
                                },
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                        or MessageBrokerRemoteClientTraceEventType.BindListener
                                        or MessageBrokerRemoteClientTraceEventType.DeadLetterQuery
                                        or MessageBrokerRemoteClientTraceEventType.Ping )
                                        endSource.Complete();
                                },
                                sendPacket: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientSendPacketEventType.Sending
                                        && e.Packet.Endpoint == MessageBrokerClientEndpoint.Pong )
                                    {
                                        bindPublisherContinuation.Complete();
                                        sendContinuation.Task.Wait();
                                        Task.Delay( 50 ).Wait();
                                    }
                                } ) ) ) );

            await server.StartAsync();

            using var client = new ClientMock();
            await client.EstablishHandshake( server, maxBatchPacketCount: 2, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await client.GetTask(
                c =>
                {
                    c.SendPing();
                    c.SendBindPublisherRequest( "foo" );
                    c.SendBindListenerRequest( "foo", false );
                    c.SendDeadLetterQuery( 1, 0 );
                    c.ReadPong();
                    c.ReadBatch(
                    [
                        (MessageBrokerClientEndpoint.PublisherBoundResponse,
                            Protocol.PacketHeader.Length + Protocol.PublisherBoundResponse.Payload),
                        (MessageBrokerClientEndpoint.ListenerBoundResponse,
                            Protocol.PacketHeader.Length + Protocol.ListenerBoundResponse.Payload)
                    ] );

                    c.ReadDeadLetterQueryResponse();
                } );

            await endSource.Task;

            logs.GetAll()
                .Skip( 2 )
                .TestSequence(
                [
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:BindPublisher] Client = [1] 'test', TraceId = 2 (start)",
                        "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindPublisherRequest, Length = 11)",
                        "[BindingPublisher] Client = [1] 'test', TraceId = 2, ChannelName = 'foo', IsEphemeral = True",
                        "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (BindPublisherRequest, Length = 11)",
                        "[PublisherBound] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo' (created), Stream = [1] 'foo' (created)",
                        "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (Batch, Length = 35), PacketCount = 2",
                        "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (Batch, Length = 35)",
                        "[SendPacket:Batched] Client = [1] 'test', TraceId = 2, BatchTraceId = 2, Packet = (PublisherBoundResponse, Length = 14)",
                        "[Trace:BindPublisher] Client = [1] 'test', TraceId = 2 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (start)",
                        "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 43)",
                        "[BindingListener] Client = [1] 'test', TraceId = 3, ChannelName = 'foo', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = False",
                        "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (BindListenerRequest, Length = 43)",
                        "[ListenerBound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Queue = [1] 'foo' (created)",
                        "[SendPacket:Batched] Client = [1] 'test', TraceId = 3, BatchTraceId = 2, Packet = (ListenerBoundResponse, Length = 14)",
                        "[Trace:BindListener] Client = [1] 'test', TraceId = 3 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 4 (start)",
                        "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (DeadLetterQuery, Length = 13)",
                        "[QueryingDeadLetter] Client = [1] 'test', TraceId = 4, QueueId = 1, ReadCount = 0",
                        "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (DeadLetterQuery, Length = 13)",
                        "[DeadLetterQueried] Client = [1] 'test', TraceId = 4, Queue = [1] 'foo', TotalCount = 0",
                        "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (DeadLetterQueryResponse, Length = 21)",
                        "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (DeadLetterQueryResponse, Length = 21)",
                        "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 4 (end)"
                    ] )
                ] )
                .Go();
        }

        [Fact]
        public async Task Sending_ShouldHandleElementsCorrectly_WhenPacketLengthLimitIsReached()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 5 );
            var bindListenerContinuation = new SafeTaskCompletionSource();
            var pushMessageContinuation = new SafeTaskCompletionSource();
            var deadLetterContinuation = new SafeTaskCompletionSource();
            var sendContinuation = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        c => c.Id == 2
                            ? logs.GetLogger(
                                MessageBrokerRemoteClientLogger.Create(
                                    listenerBound: _ => pushMessageContinuation.Complete(),
                                    listenerUnbound: _ => sendContinuation.Complete(),
                                    processingMessage: _ => deadLetterContinuation.Complete(),
                                    traceStart: e =>
                                    {
                                        if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                            bindListenerContinuation.Task.Wait();
                                    },
                                    traceEnd: e =>
                                    {
                                        if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindListener
                                            or MessageBrokerRemoteClientTraceEventType.UnbindListener
                                            or MessageBrokerRemoteClientTraceEventType.DeadLetterQuery
                                            or MessageBrokerRemoteClientTraceEventType.MessageNotification
                                            or MessageBrokerRemoteClientTraceEventType.Ping )
                                            endSource.Complete();
                                    },
                                    sendPacket: e =>
                                    {
                                        if ( e.Type == MessageBrokerRemoteClientSendPacketEventType.Sending
                                            && e.Packet.Endpoint == MessageBrokerClientEndpoint.Pong )
                                        {
                                            bindListenerContinuation.Complete();
                                            sendContinuation.Task.Wait();
                                            Task.Delay( 50 ).Wait();
                                        }
                                    } ) )
                            : null ) );

            await server.StartAsync();

            using var client1 = new ClientMock();
            await client1.EstablishHandshake( server );
            await client1.GetTask( c => c.SendBindPublisherRequest( "foo" ) );

            using var client2 = new ClientMock();
            await client2.EstablishHandshake(
                server,
                name: "test2",
                maxBatchPacketCount: 10,
                maxNetworkBatchPacketLength: MemorySize.FromKilobytes( 16 ) );

            await client2.GetTask(
                c =>
                {
                    c.SendPing();
                    c.SendBindListenerRequest( "foo", false );
                } );

            await pushMessageContinuation.Task;
            await Task.Delay( 50 );

            var messageLength = ( int )(MemorySize.FromKilobytes( 16 ).Bytes
                - Protocol.PacketHeader.Length * 4
                - Protocol.BatchHeader.Length
                - Protocol.ListenerBoundResponse.Payload
                - Protocol.MessageNotificationHeader.Payload
                - Protocol.DeadLetterQueryResponse.Payload);

            await client1.GetTask(
                c => c.SendPushMessage( 1, Enumerable.Range( 0, messageLength ).Select( x => ( byte )x ).ToArray(), confirm: false ) );

            await deadLetterContinuation.Task;
            await client2.GetTask(
                c =>
                {
                    c.SendDeadLetterQuery( 1, 0 );
                    c.SendUnbindListenerRequest( 1 );
                    c.ReadPong();
                    c.ReadBatch(
                    [
                        (MessageBrokerClientEndpoint.ListenerBoundResponse,
                            Protocol.PacketHeader.Length + Protocol.ListenerBoundResponse.Payload),
                        (MessageBrokerClientEndpoint.MessageNotification,
                            Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload + messageLength),
                        (MessageBrokerClientEndpoint.DeadLetterQueryResponse,
                            Protocol.PacketHeader.Length + Protocol.DeadLetterQueryResponse.Payload)
                    ] );

                    c.ReadListenerUnboundResponse();
                } );

            await endSource.Task;

            logs.GetAll()
                .Skip( 2 )
                .TestSequence(
                [
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:BindListener] Client = [2] 'test2', TraceId = 2 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 2, Packet = (BindListenerRequest, Length = 43)",
                        "[BindingListener] Client = [2] 'test2', TraceId = 2, ChannelName = 'foo', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = False",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 2, Packet = (BindListenerRequest, Length = 43)",
                        "[ListenerBound] Client = [2] 'test2', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo' (created)",
                        "[SendPacket:Sending] Client = [2] 'test2', TraceId = 2, Packet = (Batch, Length = 16384), PacketCount = 3",
                        "[SendPacket:Sent] Client = [2] 'test2', TraceId = 2, Packet = (Batch, Length = 16384)",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 2, BatchTraceId = 2, Packet = (ListenerBoundResponse, Length = 14)",
                        "[Trace:BindListener] Client = [2] 'test2', TraceId = 2 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:MessageNotification] Client = [2] 'test2', TraceId = 3 (start)",
                        "[ProcessingMessage] Client = [2] 'test2', TraceId = 3, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Queue = [1] 'foo', MessageId = 0, Retry = 0, Redelivery = 0, Length = 16297",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 3, BatchTraceId = 2, Packet = (MessageNotification, Length = 16342)",
                        "[MessageProcessed] Client = [2] 'test2', TraceId = 3, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Queue = [1] 'foo', MessageId = 0, Retry = 0, Redelivery = 0",
                        "[Trace:MessageNotification] Client = [2] 'test2', TraceId = 3 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:DeadLetterQuery] Client = [2] 'test2', TraceId = 4 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 4, Packet = (DeadLetterQuery, Length = 13)",
                        "[QueryingDeadLetter] Client = [2] 'test2', TraceId = 4, QueueId = 1, ReadCount = 0",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 4, Packet = (DeadLetterQuery, Length = 13)",
                        "[DeadLetterQueried] Client = [2] 'test2', TraceId = 4, Queue = [1] 'foo', TotalCount = 0",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 4, BatchTraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                        "[Trace:DeadLetterQuery] Client = [2] 'test2', TraceId = 4 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:UnbindListener] Client = [2] 'test2', TraceId = 5 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 5, Packet = (UnbindListenerRequest, Length = 9)",
                        "[UnbindingListener] Client = [2] 'test2', TraceId = 5, ChannelId = 1",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 5, Packet = (UnbindListenerRequest, Length = 9)",
                        "[ListenerUnbound] Client = [2] 'test2', TraceId = 5, Channel = [1] 'foo', Queue = [1] 'foo' (removed)",
                        "[SendPacket:Sending] Client = [2] 'test2', TraceId = 5, Packet = (ListenerUnboundResponse, Length = 6)",
                        "[SendPacket:Sent] Client = [2] 'test2', TraceId = 5, Packet = (ListenerUnboundResponse, Length = 6)",
                        "[Trace:UnbindListener] Client = [2] 'test2', TraceId = 5 (end)"
                    ] )
                ] )
                .Go();
        }

        [Fact]
        public async Task Sending_ShouldHandleElementsCorrectly_WhenPacketLengthLimitIsReachedOnTheFirstElement()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 5 );
            var bindListenerContinuation = new SafeTaskCompletionSource();
            var pushMessageContinuation = new SafeTaskCompletionSource();
            var deadLetterContinuation = new SafeTaskCompletionSource();
            var sendContinuation = new SafeTaskCompletionSource();
            var logs = new ClientEventLogger();
            await using var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySourceFactory( _ => _sharedDelaySource )
                    .SetClientLoggerFactory(
                        c => c.Id == 2
                            ? logs.GetLogger(
                                MessageBrokerRemoteClientLogger.Create(
                                    listenerBound: _ => pushMessageContinuation.Complete(),
                                    listenerUnbound: _ => sendContinuation.Complete(),
                                    processingMessage: _ => deadLetterContinuation.Complete(),
                                    traceStart: e =>
                                    {
                                        if ( e.Type == MessageBrokerRemoteClientTraceEventType.BindListener )
                                            bindListenerContinuation.Task.Wait();
                                    },
                                    traceEnd: e =>
                                    {
                                        if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindListener
                                            or MessageBrokerRemoteClientTraceEventType.UnbindListener
                                            or MessageBrokerRemoteClientTraceEventType.DeadLetterQuery
                                            or MessageBrokerRemoteClientTraceEventType.MessageNotification
                                            or MessageBrokerRemoteClientTraceEventType.Ping )
                                            endSource.Complete();
                                    },
                                    sendPacket: e =>
                                    {
                                        if ( e.Type == MessageBrokerRemoteClientSendPacketEventType.Sending
                                            && e.Packet.Endpoint == MessageBrokerClientEndpoint.Pong )
                                        {
                                            bindListenerContinuation.Complete();
                                            sendContinuation.Task.Wait();
                                            Task.Delay( 50 ).Wait();
                                        }
                                    } ) )
                            : null ) );

            await server.StartAsync();

            using var client1 = new ClientMock();
            await client1.EstablishHandshake( server );
            await client1.GetTask( c => c.SendBindPublisherRequest( "foo" ) );

            using var client2 = new ClientMock();
            await client2.EstablishHandshake(
                server,
                name: "test2",
                maxBatchPacketCount: 10,
                maxNetworkBatchPacketLength: MemorySize.FromKilobytes( 16 ) );

            await client2.GetTask(
                c =>
                {
                    c.SendPing();
                    c.SendBindListenerRequest( "foo", false );
                } );

            await pushMessageContinuation.Task;
            await Task.Delay( 50 );

            var messageLength = ( int )(MemorySize.FromKilobytes( 16 ).Bytes
                - Protocol.PacketHeader.Length * 2
                - Protocol.BatchHeader.Length
                - Protocol.MessageNotificationHeader.Payload);

            await client1.GetTask(
                c => c.SendPushMessage( 1, Enumerable.Range( 0, messageLength ).Select( x => ( byte )x ).ToArray(), confirm: false ) );

            await deadLetterContinuation.Task;
            await client2.GetTask(
                c =>
                {
                    c.SendDeadLetterQuery( 1, 0 );
                    c.SendUnbindListenerRequest( 1 );
                    c.ReadPong();
                    c.ReadListenerBoundResponse();
                    c.ReadMessageNotification( messageLength );
                    c.ReadBatch(
                    [
                        (MessageBrokerClientEndpoint.DeadLetterQueryResponse,
                            Protocol.PacketHeader.Length + Protocol.DeadLetterQueryResponse.Payload),
                        (MessageBrokerClientEndpoint.ListenerUnboundResponse,
                            Protocol.PacketHeader.Length + Protocol.ListenerUnboundResponse.Payload)
                    ] );
                } );

            await endSource.Task;

            logs.GetAll()
                .Skip( 2 )
                .TestSequence(
                [
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:BindListener] Client = [2] 'test2', TraceId = 2 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 2, Packet = (BindListenerRequest, Length = 43)",
                        "[BindingListener] Client = [2] 'test2', TraceId = 2, ChannelName = 'foo', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = False",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 2, Packet = (BindListenerRequest, Length = 43)",
                        "[ListenerBound] Client = [2] 'test2', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo' (created)",
                        "[SendPacket:Sending] Client = [2] 'test2', TraceId = 2, Packet = (ListenerBoundResponse, Length = 14)",
                        "[SendPacket:Sent] Client = [2] 'test2', TraceId = 2, Packet = (ListenerBoundResponse, Length = 14)",
                        "[Trace:BindListener] Client = [2] 'test2', TraceId = 2 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:MessageNotification] Client = [2] 'test2', TraceId = 3 (start)",
                        "[ProcessingMessage] Client = [2] 'test2', TraceId = 3, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Queue = [1] 'foo', MessageId = 0, Retry = 0, Redelivery = 0, Length = 16332",
                        "[SendPacket:Sending] Client = [2] 'test2', TraceId = 3, Packet = (MessageNotification, Length = 16377)",
                        "[SendPacket:Sent] Client = [2] 'test2', TraceId = 3, Packet = (MessageNotification, Length = 16377)",
                        "[MessageProcessed] Client = [2] 'test2', TraceId = 3, Sender = [1] 'test', Channel = [1] 'foo', Stream = [1] 'foo', Queue = [1] 'foo', MessageId = 0, Retry = 0, Redelivery = 0",
                        "[Trace:MessageNotification] Client = [2] 'test2', TraceId = 3 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:DeadLetterQuery] Client = [2] 'test2', TraceId = 4 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 4, Packet = (DeadLetterQuery, Length = 13)",
                        "[QueryingDeadLetter] Client = [2] 'test2', TraceId = 4, QueueId = 1, ReadCount = 0",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 4, Packet = (DeadLetterQuery, Length = 13)",
                        "[DeadLetterQueried] Client = [2] 'test2', TraceId = 4, Queue = [1] 'foo', TotalCount = 0",
                        "[SendPacket:Sending] Client = [2] 'test2', TraceId = 4, Packet = (Batch, Length = 34), PacketCount = 2",
                        "[SendPacket:Sent] Client = [2] 'test2', TraceId = 4, Packet = (Batch, Length = 34)",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 4, BatchTraceId = 4, Packet = (DeadLetterQueryResponse, Length = 21)",
                        "[Trace:DeadLetterQuery] Client = [2] 'test2', TraceId = 4 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:UnbindListener] Client = [2] 'test2', TraceId = 5 (start)",
                        "[ReadPacket:Received] Client = [2] 'test2', TraceId = 5, Packet = (UnbindListenerRequest, Length = 9)",
                        "[UnbindingListener] Client = [2] 'test2', TraceId = 5, ChannelId = 1",
                        "[ReadPacket:Accepted] Client = [2] 'test2', TraceId = 5, Packet = (UnbindListenerRequest, Length = 9)",
                        "[ListenerUnbound] Client = [2] 'test2', TraceId = 5, Channel = [1] 'foo', Queue = [1] 'foo' (removed)",
                        "[SendPacket:Batched] Client = [2] 'test2', TraceId = 5, BatchTraceId = 4, Packet = (ListenerUnboundResponse, Length = 6)",
                        "[Trace:UnbindListener] Client = [2] 'test2', TraceId = 5 (end)"
                    ] )
                ] )
                .Go();
        }
    }
}
