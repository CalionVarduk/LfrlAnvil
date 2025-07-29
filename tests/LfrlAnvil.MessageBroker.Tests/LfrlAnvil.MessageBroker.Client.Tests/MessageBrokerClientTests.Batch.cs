using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Internal;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests
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
            var batchContinuation = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerClientLogger.Create(
                                traceStart: e =>
                                {
                                    if ( e.Type == MessageBrokerClientTraceEventType.DeadLetterQuery )
                                        batchContinuation.Complete();
                                },
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerClientTraceEventType.DeadLetterQuery
                                        or MessageBrokerClientTraceEventType.MessageNotification
                                        or MessageBrokerClientTraceEventType.SystemNotification )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake( client, maxBatchPacketCount: 10, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read(
                        new Protocol.BindListenerRequest(
                            "foo",
                            null,
                            1,
                            0,
                            Duration.Zero,
                            0,
                            Duration.Zero,
                            0,
                            Duration.Zero,
                            null,
                            true ) );

                    s.SendListenerBoundResponse( true, true, 1, 1 );
                } );

            await client.Listeners.BindAsync(
                "foo",
                (_, _) => ValueTask.CompletedTask,
                MessageBrokerListenerOptions.Default.EnableAcks( false ) );

            await serverTask;

            var queryTask = client.QueryDeadLetterAsync( 1, 0 );
            await batchContinuation.Task;
            await server.GetTask(
                s => s.SendBatch(
                [
                    ServerMock.PrepareObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "foo" ),
                    ServerMock.PrepareMessageNotification( 0, 0, 2, 1, 1, [ 1, 2, 3 ] ),
                    ServerMock.PrepareDeadLetterQueryResponse( 0, 0, Timestamp.Zero )
                ] ) );

            await endSource.Task;
            await queryTask;

            Assertion.All(
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 2, QueueId = 1, ReadCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[DeadLetterQueried] Client = [1] 'test', TraceId = 2, TotalCount = 0",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:SystemNotification] Client = [1] 'test', TraceId = 3 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (SystemNotification, Length = 13)",
                                "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 3, Type = SenderName",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (SystemNotification, Length = 13)",
                                "[SenderNameProcessed] Client = [1] 'test', TraceId = 3, SenderId = 2, NewName = 'foo'",
                                "[Trace:SystemNotification] Client = [1] 'test', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[ProcessingMessage] Client = [1] 'test', TraceId = 4, StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0, ChannelId = 1, SenderId = 2, Length = 3",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (MessageNotification, Length = 48)",
                                "[MessageProcessed] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo', Queue = [1] 'foo', StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[Trace:MessageNotification] Client = [1] 'test', TraceId = 4 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 89)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 89), PacketCount = 3",
                            "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 48)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingValidBatch_ShouldHandleElementPacketsCorrectly_WhenLastElementEndsWithEmptyPacket()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 2 );
            var start = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
                "test",
                MessageBrokerClientOptions.Default
                    .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                    .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                    .SetDelaySource( _sharedDelaySource )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerClientLogger.Create(
                                sendPacket: e =>
                                {
                                    if ( e.Type == MessageBrokerClientSendPacketEventType.Sending
                                        && e.Packet.Endpoint == MessageBrokerServerEndpoint.Ping )
                                        start.Complete();
                                },
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerClientTraceEventType.Ping
                                        or MessageBrokerClientTraceEventType.SystemNotification )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake(
                client,
                pingInterval: Duration.FromSeconds( 0.2 ),
                maxBatchPacketCount: 10,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );

            await start.Task;
            await server.GetTask(
                s => s.SendBatch(
                [
                    ServerMock.PrepareObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "foo" ),
                    ServerMock.PreparePong()
                ] ) );

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
                                "[Trace:SystemNotification] Client = [1] 'test', TraceId = 2 (start)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 13)",
                                "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 2, Type = StreamName",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 13)",
                                "[StreamNameProcessed] Client = [1] 'test', TraceId = 2, StreamId = 1, NewName = 'foo'",
                                "[Trace:SystemNotification] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 25)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 25), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Pong, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingUnsupportedBatch_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake( client );
            await server.GetTask( s => s.SendHeader( MessageBrokerClientEndpoint.Batch, 0 ) );
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
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid Batch from the server. Encountered 1 error(s):
                            1. Received unexpected client endpoint.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithInvalidPayload_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake( client, maxBatchPacketCount: 2, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await server.GetTask( s => s.SendBatch( [ ], payload: 1 ) );
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
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid Batch from the server. Encountered 1 error(s):
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
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake( client, maxBatchPacketCount: 3, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await server.GetTask( s => s.SendBatch( [ ], packetCount ) );
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
                             LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid Batch from the server. Encountered 1 error(s):
                             1. Expected batch packet count to be in [2, 3] range but found {packetCount}.
                             """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithIncompleteElementHeader_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake( client, maxBatchPacketCount: 2, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            await server.GetTask( s => s.SendBatch( [ [ 1, 0, 0, 0 ] ], packetCount: 2 ) );
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
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid Batch from the server. Encountered 1 error(s):
                            1. Expected length of the packet at index 0 in batch to be greater than or equal to 5 but found 4.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithTooLargeMessageNotificationElementPayload_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake(
                client,
                maxNetworkMessagePacketLength: MemorySize.FromKilobytes( 32 ),
                maxBatchPacketCount: 2,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );

            await server.GetTask(
                s => s.SendBatch(
                    [
                        ServerMock.PrepareMessageNotification(
                            1,
                            0,
                            1,
                            1,
                            1,
                            [ ],
                            payload: ( uint )MemorySize.BytesPerKilobyte * 32 - Protocol.PacketHeader.Length + 1 )
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
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 52)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 52), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 32769)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 32769)
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid MessageNotification from the server. Encountered 1 error(s):
                            1. Expected total packet length to be in [5, 32768] range but found 32769.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithMessageNotificationElementPayloadExceedingBatchEnd_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake(
                client,
                maxBatchPacketCount: 2,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );

            await server.GetTask(
                s => s.SendBatch(
                    [
                        ServerMock.PrepareMessageNotification(
                            1,
                            0,
                            1,
                            1,
                            1,
                            [ ],
                            payload: Protocol.MessageNotificationHeader.Length + 1 )
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
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 52)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 52), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 46)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (MessageNotification, Length = 46)
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid MessageNotification from the server. Encountered 1 error(s):
                            1. Expected payload of the packet at index 0 in batch to be less than or equal to 40 but found 41.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithTooLargeSystemNotificationElementPayload_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake(
                client,
                maxNetworkPacketLength: MemorySize.FromKilobytes( 16 ),
                maxBatchPacketCount: 2,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );

            await server.GetTask(
                s => s.SendBatch(
                    [
                        ServerMock.PrepareObjectNameNotification(
                            MessageBrokerSystemNotificationType.SenderName,
                            1,
                            string.Empty,
                            payload: ( uint )MemorySize.BytesPerKilobyte * 16 - Protocol.PacketHeader.Length + 1 )
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
                            "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 16385)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 16385)
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected total packet length to be in [5, 16384] range but found 16385.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithSystemNotificationElementPayloadExceedingBatchEnd_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake(
                client,
                maxBatchPacketCount: 2,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );

            await server.GetTask(
                s => s.SendBatch(
                    [
                        ServerMock.PrepareObjectNameNotification(
                            MessageBrokerSystemNotificationType.SenderName,
                            1,
                            string.Empty,
                            payload: Protocol.SystemNotificationHeader.Length + Protocol.ObjectNameNotificationHeader.Length + 1 )
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
                            "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 11)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 11)
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected payload of the packet at index 0 in batch to be less than or equal to 5 but found 6.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithUnexpectedResponseElement_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake(
                client,
                maxBatchPacketCount: 2,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );

            await server.GetTask(
                s => s.SendBatch( [ ServerMock.PrepareDeadLetterQueryResponse( 0, 0, Timestamp.Zero ) ], packetCount: 2 ) );

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
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 28)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 28), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid DeadLetterQueryResponse from the server. Encountered 1 error(s):
                            1. Received unexpected client endpoint.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithTooLargeResponseElementPayload_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake(
                client,
                maxNetworkPacketLength: MemorySize.FromKilobytes( 16 ),
                maxBatchPacketCount: 2,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );

            var queryTask = client.QueryDeadLetterAsync( 1, 0 );
            await server.GetTask(
                s =>
                {
                    s.ReadDeadLetterQuery();
                    s.SendBatch(
                        [
                            ServerMock.PrepareDeadLetterQueryResponse(
                                0,
                                0,
                                Timestamp.Zero,
                                payload: ( uint )MemorySize.BytesPerKilobyte * 16 - Protocol.PacketHeader.Length + 1 )
                        ],
                        packetCount: 2 );
                } );

            await endSource.Task;
            await queryTask;

            Assertion.All(
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (start)",
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 28)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 28), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 16385)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 16385)
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid DeadLetterQueryResponse from the server. Encountered 1 error(s):
                            1. Expected total packet length to be in [5, 16384] range but found 16385.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithResponseElementPayloadExceedingBatchEnd_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake(
                client,
                maxBatchPacketCount: 2,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );

            var queryTask = client.QueryDeadLetterAsync( 1, 0 );
            await server.GetTask(
                s =>
                {
                    s.ReadDeadLetterQuery();
                    s.SendBatch(
                        [
                            ServerMock.PrepareDeadLetterQueryResponse(
                                0,
                                0,
                                Timestamp.Zero,
                                payload: Protocol.DeadLetterQueryResponse.Length + 1 )
                        ],
                        packetCount: 2 );
                } );

            await endSource.Task;
            await queryTask;

            Assertion.All(
                    logs.GetAll()
                        .TakeLast( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (start)",
                                "[Disposing] Client = [1] 'test', TraceId = 2",
                                "[Disposed] Client = [1] 'test', TraceId = 2",
                                "[Trace:Unexpected] Client = [1] 'test', TraceId = 2 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 28)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 28), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 22)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 22)
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid DeadLetterQueryResponse from the server. Encountered 1 error(s):
                            1. Expected payload of the packet at index 0 in batch to be less than or equal to 16 but found 17.
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ReceivingBatchWithTooMuchData_ShouldDisposeClient()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new EventLogger();
            using var server = new ServerMock();
            var remoteEndpoint = server.Start();

            await using var client = new MessageBrokerClient(
                remoteEndpoint,
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                        endSource.Complete();
                                } ) ) ) );

            await server.EstablishHandshake(
                client,
                maxBatchPacketCount: 2,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );

            await server.GetTask(
                s => s.SendBatch(
                    [
                        ServerMock.PrepareObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "foo" ),
                        ServerMock.PrepareObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "bar" ),
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
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 34)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 34), PacketCount = 2",
                            "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)",
                            "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)",
                            """
                            [AwaitPacket] Client = [1] 'test', Packet = (Batch, Length = 34)
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid Batch from the server. Encountered 1 error(s):
                            1. Expected an end of the batch packet but found 1 remaining byte(s).
                            """
                        ] ) )
                .Go();
        }
    }
}
