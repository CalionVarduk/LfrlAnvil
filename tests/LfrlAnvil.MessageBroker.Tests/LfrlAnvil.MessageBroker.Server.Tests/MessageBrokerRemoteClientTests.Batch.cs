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
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                        or MessageBrokerRemoteClientTraceEventType.PushMessage
                                        or MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                        endSource.Complete();
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
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo'",
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
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                        or MessageBrokerRemoteClientTraceEventType.PushMessage
                                        or MessageBrokerRemoteClientTraceEventType.UnbindPublisher )
                                        endSource.Complete();
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
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo'",
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
                                "[PublisherUnbound] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo' (removed), Stream = [1] 'foo' (removed)",
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
                                    if ( e.Type is MessageBrokerRemoteClientTraceEventType.BindPublisher
                                        or MessageBrokerRemoteClientTraceEventType.Ping )
                                        endSource.Complete();
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
                                "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo'",
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
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
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
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
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
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
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
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
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
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
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
                                "[Disposing] Client = [1] 'test', TraceId = 1",
                                "[Disposed] Client = [1] 'test', TraceId = 1",
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
                                $"[Disposing] Client = [1] 'test', TraceId = {t.Id}",
                                $"[Disposed] Client = [1] 'test', TraceId = {t.Id}",
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
    }
}
