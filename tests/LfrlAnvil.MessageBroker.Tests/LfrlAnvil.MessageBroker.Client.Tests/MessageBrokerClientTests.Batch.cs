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
        public async Task ReceivingValid_ShouldHandleElementPacketsCorrectly()
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
                            true,
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
        public async Task ReceivingValid_ShouldHandleElementPacketsCorrectly_WhenLastElementEndsWithEmptyPacket()
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
        public async Task ReceivingUnsupported_ShouldDisposeClient()
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
        public async Task ReceivingWithInvalidPayload_ShouldDisposeClient()
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
        public async Task ReceivingWithInvalidPacketCount_ShouldDisposeClient(short packetCount)
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
        public async Task ReceivingWithIncompleteElementHeader_ShouldDisposeClient()
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
        public async Task ReceivingWithTooLargeMessageNotificationElementPayload_ShouldDisposeClient()
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
        public async Task ReceivingWithMessageNotificationElementPayloadExceedingBatchEnd_ShouldDisposeClient()
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
        public async Task ReceivingWithTooLargeSystemNotificationElementPayload_ShouldDisposeClient()
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
        public async Task ReceivingWithSystemNotificationElementPayloadExceedingBatchEnd_ShouldDisposeClient()
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
        public async Task ReceivingWithUnexpectedResponseElement_ShouldDisposeClient()
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
        public async Task ReceivingWithTooLargeResponseElementPayload_ShouldDisposeClient()
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
        public async Task ReceivingWithResponseElementPayloadExceedingBatchEnd_ShouldDisposeClient()
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
        public async Task ReceivingWithTooMuchData_ShouldDisposeClient()
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

        [Fact]
        public async Task Sending_ShouldHandleElementsCorrectly()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 6 );
            var batchStart = new SafeTaskCompletionSource();
            var releaseWriter = new SafeTaskCompletionSource();
            var sentPacketCount = Atomic.Create( 0 );
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
                                    if ( e.Type == MessageBrokerClientTraceEventType.Ping )
                                        releaseWriter.Complete();
                                },
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerClientTraceEventType.DeadLetterQuery
                                        or MessageBrokerClientTraceEventType.BindListener
                                        or MessageBrokerClientTraceEventType.BindPublisher
                                        or MessageBrokerClientTraceEventType.Ping )
                                        endSource.Complete();
                                },
                                sendPacket: e =>
                                {
                                    if ( e.Type == MessageBrokerClientSendPacketEventType.Sent
                                        && e.Packet.Endpoint == MessageBrokerServerEndpoint.DeadLetterQuery
                                        && sentPacketCount.Value == 0 )
                                    {
                                        ++sentPacketCount.Value;
                                        batchStart.Complete();
                                        releaseWriter.Task.Wait();
                                        Task.Delay( 50 ).Wait();
                                    }
                                } ) ) ) );

            await server.EstablishHandshake(
                client,
                pingInterval: Duration.FromSeconds( 0.2 ),
                maxBatchPacketCount: 10,
                maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );

            var nextExpirationAt = TimestampProvider.Shared.GetNow();
            var batchTask = Task.Run(
                async () =>
                {
                    await batchStart.Task;
                    var a = client.QueryDeadLetterAsync( 1, 0 );
                    var b = client.QueryDeadLetterAsync( 2, 1 );
                    var c = client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
                    var d = client.Publishers.BindAsync( "foo" );

                    using ( client.AcquireLock() )
                        client.EventScheduler.SchedulePing( client );

                    await server.GetTask(
                        s =>
                        {
                            s.ReadDeadLetterQuery();
                            s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero );
                            s.ReadBatch(
                            [
                                (MessageBrokerServerEndpoint.DeadLetterQuery, Protocol.DeadLetterQuery.Length),
                                (MessageBrokerServerEndpoint.DeadLetterQuery, Protocol.DeadLetterQuery.Length),
                                (MessageBrokerServerEndpoint.BindListenerRequest, Protocol.PacketHeader.Length
                                    + ( int )new Protocol.BindListenerRequest(
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
                                        true,
                                        true ).Header.Payload),
                                (MessageBrokerServerEndpoint.BindPublisherRequest, Protocol.PacketHeader.Length
                                    + ( int )new Protocol.BindPublisherRequest( "foo", null, true ).Header.Payload),
                                (MessageBrokerServerEndpoint.Ping, Protocol.PacketHeader.Length)
                            ] );

                            s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero );
                            s.SendDeadLetterQueryResponse( 5, 1, nextExpirationAt );
                            s.SendListenerBoundResponse( true, true, 1, 1 );
                            s.SendPublisherBoundResponse( false, true, 1, 1 );
                            s.SendPong();
                        } );

                    await a;
                    await b;
                    await c;
                    await d;
                } );

            await client.QueryDeadLetterAsync( 1, 0 );
            await batchTask;
            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 2, QueueId = 1, ReadCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (Batch, Length = 92), PacketCount = 5",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (Batch, Length = 92)",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 2, BatchTraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[DeadLetterQueried] Client = [1] 'test', TraceId = 2, TotalCount = 0",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 3 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 3, QueueId = 2, ReadCount = 1",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 3, BatchTraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (DeadLetterQueryResponse, Length = 21)",
                                $"[DeadLetterQueried] Client = [1] 'test', TraceId = 3, TotalCount = 5, MaxReadCount = 1, NextExpirationAt = {nextExpirationAt}",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 4 (start)",
                                "[BindingListener] Client = [1] 'test', TraceId = 4, ChannelName = 'foo', QueueName = 'foo', PrefetchHint = 1, MaxRetries = 0, MaxRedeliveries = 0, MinAckTimeout = 600 second(s), DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = True",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 4, BatchTraceId = 2, Packet = (BindListenerRequest, Length = 43)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (ListenerBoundResponse, Length = 14)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (ListenerBoundResponse, Length = 14)",
                                "[ListenerBound] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo' (created), Queue = [1] 'foo' (created)",
                                "[Trace:BindListener] Client = [1] 'test', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 5 (start)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 5, ChannelName = 'foo', StreamName = 'foo', IsEphemeral = True",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 5, BatchTraceId = 2, Packet = (BindPublisherRequest, Length = 11)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 5, Packet = (PublisherBoundResponse, Length = 14)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 5, Packet = (PublisherBoundResponse, Length = 14)",
                                "[PublisherBound] Client = [1] 'test', TraceId = 5, Channel = [1] 'foo', Stream = [1] 'foo' (created)",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 5 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ping] Client = [1] 'test', TraceId = 6 (start)",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 6, BatchTraceId = 2, Packet = (Ping, Length = 5)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 6, Packet = (Pong, Length = 5)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 6, Packet = (Pong, Length = 5)",
                                "[Trace:Ping] Client = [1] 'test', TraceId = 6 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerBoundResponse, Length = 14)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PublisherBoundResponse, Length = 14)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (Pong, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Sending_ShouldHandleElementsCorrectly_WithBindingsRelatedRequests()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 8 );
            var batchStart = new SafeTaskCompletionSource();
            var releaseWriter = new SafeTaskCompletionSource();
            var sentPacketCount = Atomic.Create( 0 );
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
                                unbindingPublisher: _ => releaseWriter.Complete(),
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerClientTraceEventType.DeadLetterQuery
                                        or MessageBrokerClientTraceEventType.PushMessage
                                        or MessageBrokerClientTraceEventType.UnbindListener
                                        or MessageBrokerClientTraceEventType.UnbindPublisher
                                        or MessageBrokerClientTraceEventType.Ack
                                        or MessageBrokerClientTraceEventType.NegativeAck )
                                        endSource.Complete();
                                },
                                sendPacket: e =>
                                {
                                    if ( e.Type == MessageBrokerClientSendPacketEventType.Sent
                                        && e.Packet.Endpoint == MessageBrokerServerEndpoint.DeadLetterQuery
                                        && sentPacketCount.Value == 0 )
                                    {
                                        ++sentPacketCount.Value;
                                        batchStart.Complete();
                                        releaseWriter.Task.Wait();
                                        Task.Delay( 50 ).Wait();
                                    }
                                } ) ) ) );

            await server.EstablishHandshake( client, maxBatchPacketCount: 15, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null, true ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
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
                            true,
                            true ) );

                    s.SendListenerBoundResponse( false, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
            var publisher = client.Publishers.TryGetByChannelId( 1 )!;
            var listener = client.Listeners.TryGetByChannelId( 1 )!;
            await serverTask;

            var batchTask = Task.Run(
                async () =>
                {
                    await batchStart.Task;
                    var a = client.QueryDeadLetterAsync( 3, 2 );
                    var b = listener.SendMessageAckAsync( 1, 1, 0, 0, 0 );
                    var c = listener.SendNegativeMessageAckAsync( 2, 1, 1, 0, 0 );
                    var d = publisher.PushAsync( new byte[] { 1 } );
                    var e = publisher.PushAsync( new byte[] { 2, 3 }, [ MessageBrokerClientRoutingTarget.FromId( 2 ) ] );
                    var f = listener.UnbindAsync();
                    var g = publisher.PushAsync( new byte[] { 4, 5, 6 }, confirm: false );
                    var h = publisher.PushAsync(
                        new byte[] { 7, 8, 9, 10 },
                        [ MessageBrokerClientRoutingTarget.FromName( "bar" ) ],
                        confirm: false );

                    var i = publisher.UnbindAsync();

                    await server.GetTask(
                        s =>
                        {
                            s.ReadDeadLetterQuery();
                            s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero );
                            s.ReadBatch(
                            [
                                (MessageBrokerServerEndpoint.DeadLetterQuery, Protocol.DeadLetterQuery.Length),
                                (MessageBrokerServerEndpoint.MessageNotificationAck, Protocol.MessageNotificationAck.Length),
                                (MessageBrokerServerEndpoint.MessageNotificationNack, Protocol.MessageNotificationNegativeAck.Length),
                                (MessageBrokerServerEndpoint.PushMessage,
                                    Protocol.PacketHeader.Length + ( int )new Protocol.PushMessageHeader( 1, 1, true ).Header.Payload),
                                (MessageBrokerServerEndpoint.PushMessageRouting,
                                    Protocol.PacketHeader.Length + ( int )new Protocol.PushMessageRoutingHeader( 1, 5 ).Header.Payload),
                                (MessageBrokerServerEndpoint.PushMessage,
                                    Protocol.PacketHeader.Length + ( int )new Protocol.PushMessageHeader( 1, 2, true ).Header.Payload),
                                (MessageBrokerServerEndpoint.UnbindListenerRequest, Protocol.UnbindListenerRequest.Length),
                                (MessageBrokerServerEndpoint.PushMessage,
                                    Protocol.PacketHeader.Length + ( int )new Protocol.PushMessageHeader( 1, 3, false ).Header.Payload),
                                (MessageBrokerServerEndpoint.PushMessageRouting,
                                    Protocol.PacketHeader.Length + ( int )new Protocol.PushMessageRoutingHeader( 1, 5 ).Header.Payload),
                                (MessageBrokerServerEndpoint.PushMessage,
                                    Protocol.PacketHeader.Length + ( int )new Protocol.PushMessageHeader( 1, 4, false ).Header.Payload),
                                (MessageBrokerServerEndpoint.UnbindPublisherRequest, Protocol.UnbindPublisherRequest.Length)
                            ] );

                            s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero );
                            s.SendMessageAcceptedResponse( 0 );
                            s.SendMessageAcceptedResponse( 1 );
                            s.SendListenerUnboundResponse( false, true );
                            s.SendPublisherUnboundResponse( true, true );
                        } );

                    await a;
                    await b;
                    await c;
                    await d;
                    await e;
                    await f;
                    await g;
                    await h;
                    await i;
                } );

            await client.QueryDeadLetterAsync( 1, 0 );
            await batchTask;
            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .Skip( 4 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 4 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 4, QueueId = 3, ReadCount = 2",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (Batch, Length = 183), PacketCount = 11",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (Batch, Length = 183)",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 4, BatchTraceId = 4, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[DeadLetterQueried] Client = [1] 'test', TraceId = 4, TotalCount = 0",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:Ack] Client = [1] 'test', TraceId = 5 (start)",
                                "[AcknowledgingMessage] Client = [1] 'test', TraceId = 5, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 5, BatchTraceId = 4, Packet = (MessageNotificationAck, Length = 33)",
                                "[MessageAcknowledged] Client = [1] 'test', TraceId = 5, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 1, StreamId = 1, MessageId = 0, Retry = 0, Redelivery = 0, IsNack = False",
                                "[Trace:Ack] Client = [1] 'test', TraceId = 5 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 6 (start)",
                                "[AcknowledgingMessage] Client = [1] 'test', TraceId = 6, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 2, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, NACK = (SkipRetry = False, SkipDeadLetter = False, IsAutomatic = False)",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 6, BatchTraceId = 4, Packet = (MessageNotificationNack, Length = 38)",
                                "[MessageAcknowledged] Client = [1] 'test', TraceId = 6, Channel = [1] 'foo', Queue = [1] 'foo', AckId = 2, StreamId = 1, MessageId = 1, Retry = 0, Redelivery = 0, IsNack = True",
                                "[Trace:NegativeAck] Client = [1] 'test', TraceId = 6 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 7 (start)",
                                "[PushingMessage] Client = [1] 'test', TraceId = 7, Channel = [1] 'foo', Stream = [1] 'foo', Length = 1, Confirm = True",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 7, BatchTraceId = 4, Packet = (PushMessage, Length = 11)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 7, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 7, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 7, Channel = [1] 'foo', Stream = [1] 'foo', Length = 1, MessageId = 0",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 7 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 8 (start)",
                                "[PushingMessage] Client = [1] 'test', TraceId = 8, Channel = [1] 'foo', Stream = [1] 'foo', Length = 2, RoutingTargetCount = 1, Confirm = True",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 8, BatchTraceId = 4, Packet = (PushMessageRouting, Length = 12)",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 8, BatchTraceId = 4, Packet = (PushMessage, Length = 12)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 8, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 8, Packet = (MessageAcceptedResponse, Length = 13)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 8, Channel = [1] 'foo', Stream = [1] 'foo', Length = 2, MessageId = 1",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 8 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 9 (start)",
                                "[UnbindingListener] Client = [1] 'test', TraceId = 9, Channel = [1] 'foo', Queue = [1] 'foo'",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 9, BatchTraceId = 4, Packet = (UnbindListenerRequest, Length = 9)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 9, Packet = (ListenerUnboundResponse, Length = 6)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 9, Packet = (ListenerUnboundResponse, Length = 6)",
                                "[ListenerUnbound] Client = [1] 'test', TraceId = 9, Channel = [1] 'foo', Queue = [1] 'foo' (removed)",
                                "[Trace:UnbindListener] Client = [1] 'test', TraceId = 9 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 10 (start)",
                                "[PushingMessage] Client = [1] 'test', TraceId = 10, Channel = [1] 'foo', Stream = [1] 'foo', Length = 3, Confirm = False",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 10, BatchTraceId = 4, Packet = (PushMessage, Length = 13)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 10, Channel = [1] 'foo', Stream = [1] 'foo', Length = 3",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 10 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 11 (start)",
                                "[PushingMessage] Client = [1] 'test', TraceId = 11, Channel = [1] 'foo', Stream = [1] 'foo', Length = 4, RoutingTargetCount = 1, Confirm = False",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 11, BatchTraceId = 4, Packet = (PushMessageRouting, Length = 12)",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 11, BatchTraceId = 4, Packet = (PushMessage, Length = 14)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 11, Channel = [1] 'foo', Stream = [1] 'foo', Length = 4",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 11 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 12 (start)",
                                "[UnbindingPublisher] Client = [1] 'test', TraceId = 12, Channel = [1] 'foo', Stream = [1] 'foo'",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 12, BatchTraceId = 4, Packet = (UnbindPublisherRequest, Length = 9)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 12, Packet = (PublisherUnboundResponse, Length = 6)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 12, Packet = (PublisherUnboundResponse, Length = 6)",
                                "[PublisherUnbound] Client = [1] 'test', TraceId = 12, Channel = [1] 'foo' (removed), Stream = [1] 'foo' (removed)",
                                "[Trace:UnbindPublisher] Client = [1] 'test', TraceId = 12 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (MessageAcceptedResponse, Length = 13)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ListenerUnboundResponse, Length = 6)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PublisherUnboundResponse, Length = 6)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Sending_ShouldHandleElementsCorrectly_WhenPacketCountLimitIsReached()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 4 );
            var batchStart = new SafeTaskCompletionSource();
            var releaseWriter = new SafeTaskCompletionSource();
            var sentPacketCount = Atomic.Create( 0 );
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
                                bindingPublisher: _ => releaseWriter.Complete(),
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerClientTraceEventType.DeadLetterQuery
                                        or MessageBrokerClientTraceEventType.BindPublisher )
                                        endSource.Complete();
                                },
                                sendPacket: e =>
                                {
                                    if ( e.Type == MessageBrokerClientSendPacketEventType.Sent
                                        && e.Packet.Endpoint == MessageBrokerServerEndpoint.DeadLetterQuery
                                        && sentPacketCount.Value == 0 )
                                    {
                                        ++sentPacketCount.Value;
                                        batchStart.Complete();
                                        releaseWriter.Task.Wait();
                                        Task.Delay( 50 ).Wait();
                                    }
                                } ) ) ) );

            await server.EstablishHandshake( client, maxBatchPacketCount: 2, maxNetworkBatchPacketLength: MemorySize.FromMegabytes( 1 ) );
            var nextExpirationAt = TimestampProvider.Shared.GetNow();
            var batchTask = Task.Run(
                async () =>
                {
                    await batchStart.Task;
                    var a = client.QueryDeadLetterAsync( 1, 0 );
                    var b = client.QueryDeadLetterAsync( 2, 1 );
                    var c = client.Publishers.BindAsync( "foo" );

                    await server.GetTask(
                        s =>
                        {
                            s.ReadDeadLetterQuery();
                            s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero );
                            s.ReadBatch(
                            [
                                (MessageBrokerServerEndpoint.DeadLetterQuery, Protocol.DeadLetterQuery.Length),
                                (MessageBrokerServerEndpoint.DeadLetterQuery, Protocol.DeadLetterQuery.Length)
                            ] );

                            s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero );
                            s.SendDeadLetterQueryResponse( 5, 1, nextExpirationAt );
                            s.Read( new Protocol.BindPublisherRequest( "foo", null, true ) );
                            s.SendPublisherBoundResponse( true, true, 1, 1 );
                        } );

                    await a;
                    await b;
                    await c;
                } );

            await client.QueryDeadLetterAsync( 1, 0 );
            await batchTask;
            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .Skip( 2 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 2, QueueId = 1, ReadCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (Batch, Length = 33), PacketCount = 2",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (Batch, Length = 33)",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 2, BatchTraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[DeadLetterQueried] Client = [1] 'test', TraceId = 2, TotalCount = 0",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 2 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 3 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 3, QueueId = 2, ReadCount = 1",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 3, BatchTraceId = 2, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (DeadLetterQueryResponse, Length = 21)",
                                $"[DeadLetterQueried] Client = [1] 'test', TraceId = 3, TotalCount = 5, MaxReadCount = 1, NextExpirationAt = {nextExpirationAt}",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 4 (start)",
                                "[BindingPublisher] Client = [1] 'test', TraceId = 4, ChannelName = 'foo', StreamName = 'foo', IsEphemeral = True",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (BindPublisherRequest, Length = 11)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (BindPublisherRequest, Length = 11)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (PublisherBoundResponse, Length = 14)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (PublisherBoundResponse, Length = 14)",
                                "[PublisherBound] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo' (created), Stream = [1] 'foo' (created)",
                                "[Trace:BindPublisher] Client = [1] 'test', TraceId = 4 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (PublisherBoundResponse, Length = 14)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Sending_ShouldHandleElementsCorrectly_WhenPacketLengthLimitIsReached()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 5 );
            var batchStart = new SafeTaskCompletionSource();
            var releaseWriter = new SafeTaskCompletionSource();
            var sentPacketCount = Atomic.Create( 0 );
            var messageCount = Atomic.Create( 0 );
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
                                pushingMessage: _ =>
                                {
                                    if ( ++messageCount.Value == 3 )
                                        releaseWriter.Complete();
                                },
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerClientTraceEventType.DeadLetterQuery
                                        or MessageBrokerClientTraceEventType.PushMessage )
                                        endSource.Complete();
                                },
                                sendPacket: e =>
                                {
                                    if ( e.Type == MessageBrokerClientSendPacketEventType.Sent
                                        && e.Packet.Endpoint == MessageBrokerServerEndpoint.DeadLetterQuery
                                        && sentPacketCount.Value == 0 )
                                    {
                                        ++sentPacketCount.Value;
                                        batchStart.Complete();
                                        releaseWriter.Task.Wait();
                                        Task.Delay( 50 ).Wait();
                                    }
                                } ) ) ) );

            await server.EstablishHandshake( client, maxBatchPacketCount: 10, maxNetworkBatchPacketLength: MemorySize.FromKilobytes( 16 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null, true ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 )!;
            await serverTask;

            var batchTask = Task.Run(
                async () =>
                {
                    var firstMessageLength = ( int )MemorySize.FromKilobytes( 12 ).Bytes;
                    var secondMessageLength = ( int )(client.MaxNetworkBatchPacketLength.Bytes
                        - Protocol.PacketHeader.Length
                        - Protocol.BatchHeader.Length
                        - Protocol.DeadLetterQuery.Length
                        - Protocol.PacketHeader.Length
                        - new Protocol.PushMessageHeader( 1, firstMessageLength, false ).Header.Payload
                        - Protocol.PushMessageHeader.Length);

                    await batchStart.Task;
                    var a = client.QueryDeadLetterAsync( 1, 0 );
                    var b = publisher.PushAsync(
                        Enumerable.Range( 0, firstMessageLength ).Select( x => ( byte )x ).ToArray(),
                        confirm: false );

                    var c = publisher.PushAsync(
                        Enumerable.Range( 0, secondMessageLength ).Select( x => ( byte )x ).ToArray(),
                        confirm: false );

                    var d = publisher.PushAsync( Array.Empty<byte>(), confirm: false );

                    await server.GetTask(
                        s =>
                        {
                            s.ReadDeadLetterQuery();
                            s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero );
                            s.ReadBatch(
                            [
                                (MessageBrokerServerEndpoint.DeadLetterQuery, Protocol.DeadLetterQuery.Length),
                                (MessageBrokerServerEndpoint.PushMessage, Protocol.PacketHeader.Length
                                    + ( int )new Protocol.PushMessageHeader( 1, firstMessageLength, false ).Header.Payload),
                                (MessageBrokerServerEndpoint.PushMessage, Protocol.PacketHeader.Length
                                    + ( int )new Protocol.PushMessageHeader( 1, secondMessageLength, false ).Header.Payload)
                            ] );

                            s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero );
                            s.ReadPushMessage( 0 );
                        } );

                    await a;
                    await b;
                    await c;
                    await d;
                } );

            await client.QueryDeadLetterAsync( 1, 0 );
            await batchTask;
            await endSource.Task;

            Assertion.All(
                    logs.GetAll()
                        .Skip( 3 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 3 (start)",
                                "[QueryingDeadLetter] Client = [1] 'test', TraceId = 3, QueueId = 1, ReadCount = 0",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (Batch, Length = 16384), PacketCount = 3",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (Batch, Length = 16384)",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 3, BatchTraceId = 3, Packet = (DeadLetterQuery, Length = 13)",
                                "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (DeadLetterQueryResponse, Length = 21)",
                                "[DeadLetterQueried] Client = [1] 'test', TraceId = 3, TotalCount = 0",
                                "[Trace:DeadLetterQuery] Client = [1] 'test', TraceId = 3 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (start)",
                                "[PushingMessage] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo', Stream = [1] 'foo', Length = 12288, Confirm = False",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 4, BatchTraceId = 3, Packet = (PushMessage, Length = 12298)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo', Stream = [1] 'foo', Length = 12288",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 5 (start)",
                                "[PushingMessage] Client = [1] 'test', TraceId = 5, Channel = [1] 'foo', Stream = [1] 'foo', Length = 4056, Confirm = False",
                                "[SendPacket:Batched] Client = [1] 'test', TraceId = 5, BatchTraceId = 3, Packet = (PushMessage, Length = 4066)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 5, Channel = [1] 'foo', Stream = [1] 'foo', Length = 4056",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 5 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 6 (start)",
                                "[PushingMessage] Client = [1] 'test', TraceId = 6, Channel = [1] 'foo', Stream = [1] 'foo', Length = 0, Confirm = False",
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (PushMessage, Length = 10)",
                                "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (PushMessage, Length = 10)",
                                "[MessagePushed] Client = [1] 'test', TraceId = 6, Channel = [1] 'foo', Stream = [1] 'foo', Length = 0",
                                "[Trace:PushMessage] Client = [1] 'test', TraceId = 6 (end)"
                            ] )
                        ] ),
                    logs.GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)",
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (DeadLetterQueryResponse, Length = 21)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Sending_ShouldHandleElementsCorrectly_WhenPacketLengthLimitIsReachedOnTheFirstElement()
        {
            var endSource = new SafeTaskCompletionSource( completionCount: 3 );
            var batchStart = new SafeTaskCompletionSource();
            var releaseWriter = new SafeTaskCompletionSource();
            var messageCount = Atomic.Create( 0 );
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
                                pushingMessage: _ =>
                                {
                                    if ( ++messageCount.Value == 2 )
                                        releaseWriter.Complete();
                                },
                                traceEnd: e =>
                                {
                                    if ( e.Type is MessageBrokerClientTraceEventType.DeadLetterQuery
                                        or MessageBrokerClientTraceEventType.PushMessage )
                                        endSource.Complete();
                                },
                                sendPacket: e =>
                                {
                                    if ( e.Type == MessageBrokerClientSendPacketEventType.Sent
                                        && e.Packet.Endpoint == MessageBrokerServerEndpoint.DeadLetterQuery )
                                    {
                                        batchStart.Complete();
                                        releaseWriter.Task.Wait();
                                        Task.Delay( 50 ).Wait();
                                    }
                                } ) ) ) );

            await server.EstablishHandshake( client, maxBatchPacketCount: 10, maxNetworkBatchPacketLength: MemorySize.FromKilobytes( 16 ) );
            var serverTask = server.GetTask(
                s =>
                {
                    s.Read( new Protocol.BindPublisherRequest( "foo", null, true ) );
                    s.SendPublisherBoundResponse( true, true, 1, 1 );
                } );

            await client.Publishers.BindAsync( "foo" );
            var publisher = client.Publishers.TryGetByChannelId( 1 )!;
            await serverTask;

            var batchTask = Task.Run(
                async () =>
                {
                    var messageLength = ( int )client.MaxNetworkBatchPacketLength.Bytes
                        - Protocol.PacketHeader.Length
                        - Protocol.BatchHeader.Length
                        - Protocol.PushMessageHeader.Length;

                    await batchStart.Task;
                    var a = publisher.PushAsync(
                        Enumerable.Range( 0, messageLength ).Select( x => ( byte )x ).ToArray(),
                        confirm: false );

                    var b = publisher.PushAsync( Array.Empty<byte>(), confirm: false );

                    await server.GetTask(
                        s =>
                        {
                            s.ReadDeadLetterQuery();
                            s.SendDeadLetterQueryResponse( 0, 0, Timestamp.Zero );
                            s.ReadPushMessage( messageLength );
                            s.ReadPushMessage( 0 );
                        } );

                    await a;
                    await b;
                } );

            await client.QueryDeadLetterAsync( 1, 0 );
            await batchTask;
            await endSource.Task;

            logs.GetAll()
                .Skip( 3 )
                .TestSequence(
                [
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:PushMessage] Client = [1] 'test', TraceId = 3 (start)",
                        "[PushingMessage] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [1] 'foo', Length = 16367, Confirm = False",
                        "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (PushMessage, Length = 16377)",
                        "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (PushMessage, Length = 16377)",
                        "[MessagePushed] Client = [1] 'test', TraceId = 3, Channel = [1] 'foo', Stream = [1] 'foo', Length = 16367",
                        "[Trace:PushMessage] Client = [1] 'test', TraceId = 3 (end)"
                    ] ),
                    (t, _) => t.Logs.TestSequence(
                    [
                        "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (start)",
                        "[PushingMessage] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo', Stream = [1] 'foo', Length = 0, Confirm = False",
                        "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (PushMessage, Length = 10)",
                        "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (PushMessage, Length = 10)",
                        "[MessagePushed] Client = [1] 'test', TraceId = 4, Channel = [1] 'foo', Stream = [1] 'foo', Length = 0",
                        "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (end)"
                    ] )
                ] )
                .Go();
        }
    }
}
