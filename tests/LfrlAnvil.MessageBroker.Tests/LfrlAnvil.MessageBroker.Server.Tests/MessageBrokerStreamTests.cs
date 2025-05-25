using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerStreamTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public MessageBrokerStreamTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task PushMessage_ShouldEnqueueMessagesCorrectly()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 6 );
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            } ) ) )
                .SetStreamEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerStreamEventType.MessageDequeued )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendPushMessage( 1, [ 1 ] );
                c.SendPushMessage( 1, [ 2, 3 ] );
                c.SendPushMessage( 1, [ 4, 5, 6 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 11)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Length = 1, ChannelId = 1, Confirm = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 11)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'c', Stream = [1] 'c', MessageId = 0",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 2, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (PushMessage, Length = 12)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 3, Length = 2, ChannelId = 1, Confirm = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 3, Packet = (PushMessage, Length = 12)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 3, Channel = [1] 'c', Stream = [1] 'c', MessageId = 1",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 3 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (PushMessage, Length = 13)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 4, Length = 3, ChannelId = 1, Confirm = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (PushMessage, Length = 13)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 4, Channel = [1] 'c', Stream = [1] 'c', MessageId = 2",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 11)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 12)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 13)"
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::2] [MessageEnqueued] MessageId = 0 by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 0 by publisher [1::'test'] => [1::'c']",
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::3] [MessageEnqueued] MessageId = 1 by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 1 by publisher [1::'test'] => [1::'c']",
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::4] [MessageEnqueued] MessageId = 2 by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 2 by publisher [1::'test'] => [1::'c']"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldEnqueueMessageCorrectly_WithoutConfirmation()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            } ) ) )
                .SetStreamEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerStreamEventType.MessageDequeued )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendPushMessage( 1, [ 1 ], confirm: false );
            } );

        await endSource.Task;

        Assertion.All(
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 11)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Length = 1, ChannelId = 1, Confirm = False",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 11)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'c', Stream = [1] 'c', MessageId = 0",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 11)"
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::2] [MessageEnqueued] MessageId = 0 by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 0 by publisher [1::'test'] => [1::'c']",
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldEnqueueMessagesCorrectly_WithDifferentChannels()
    {
        var enqueueEndSource = new SafeTaskCompletionSource( completionCount: 5 );
        var endSource = new SafeTaskCompletionSource( completionCount: 10 );
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            } ) ) )
                .SetStreamEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerStreamEventType.MessageEnqueued )
                            enqueueEndSource.Complete();
                        else if ( e.Type == MessageBrokerStreamEventType.MessageDequeued )
                        {
                            enqueueEndSource.Task.Wait();
                            endSource.Complete();
                        }
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindPublisherRequest( "d", "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindPublisherRequest( "e", "c" );
                c.ReadPublisherBoundResponse();
                c.SendPushMessage( 1, [ 1 ] );
                c.SendPushMessage( 2, [ 2, 3 ] );
                c.SendPushMessage( 3, [ 4, 5, 6 ] );
                c.SendPushMessage( 3, [ 7, 8, 9, 10 ] );
                c.SendPushMessage( 1, [ 11, 12, 13, 14, 15 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                clientLogs.GetAll()
                    .Skip( 4 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 4, Packet = (PushMessage, Length = 11)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 4, Length = 1, ChannelId = 1, Confirm = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 4, Packet = (PushMessage, Length = 11)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 4, Channel = [1] 'c', Stream = [1] 'c', MessageId = 0",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 4, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 4, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 4 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 5 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 5, Packet = (PushMessage, Length = 12)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 5, Length = 2, ChannelId = 2, Confirm = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 5, Packet = (PushMessage, Length = 12)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 5, Channel = [2] 'd', Stream = [1] 'c', MessageId = 1",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 5, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 5 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 6 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 6, Packet = (PushMessage, Length = 13)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 6, Length = 3, ChannelId = 3, Confirm = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 6, Packet = (PushMessage, Length = 13)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 6, Channel = [3] 'e', Stream = [1] 'c', MessageId = 2",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 6, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 6, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 6 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 7 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 7, Packet = (PushMessage, Length = 14)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 7, Length = 4, ChannelId = 3, Confirm = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 7, Packet = (PushMessage, Length = 14)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 7, Channel = [3] 'e', Stream = [1] 'c', MessageId = 3",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 7, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 7, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 7 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 8 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 8, Packet = (PushMessage, Length = 15)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 8, Length = 5, ChannelId = 1, Confirm = True",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 8, Packet = (PushMessage, Length = 15)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 8, Channel = [1] 'c', Stream = [1] 'c', MessageId = 4",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 8, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 8, Packet = (MessageAcceptedResponse, Length = 13)",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 8 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 11)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 12)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 13)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 14)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 15)"
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::4] [MessageEnqueued] MessageId = 0 by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 0 by publisher [1::'test'] => [1::'c']",
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::5] [MessageEnqueued] MessageId = 1 by publisher [1::'test'] => [2::'d']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 1 by publisher [1::'test'] => [2::'d']",
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::6] [MessageEnqueued] MessageId = 2 by publisher [1::'test'] => [3::'e']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 2 by publisher [1::'test'] => [3::'e']"
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::7] [MessageEnqueued] MessageId = 3 by publisher [1::'test'] => [3::'e']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 3 by publisher [1::'test'] => [3::'e']"
                    ] ),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::8] [MessageEnqueued] MessageId = 4 by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 4 by publisher [1::'test'] => [1::'c']"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldDisposeClient_WhenClientSendsInvalidPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            } ) ) )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );
        await client.GetTask( c => c.SendPushMessage( 1, [ ], payload: 4 ) );

        await endSource.Task;

        Assertion.All(
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Clients.GetAll().TestEmpty(),
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 9)",
                            """
                            [Error] Client = [1] 'test', TraceId = 2
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessage from client [1] 'test'. Encountered 1 error(s):
                            1. Expected header payload to be at least 5 but found 4.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 2",
                            "[Disposed] Client = [1] 'test', TraceId = 2",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 9)"
                    ] ),
                logs.GetAllStream()
                    .TestSequence(
                    [
                        "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldDisposeClient_WhenClientSendsNonPositiveChannelId()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendPushMessage( 0, [ ] );
            } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ),
                server.Clients.GetAll().TestEmpty(),
                clientLogs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 10)",
                            """
                            [Error] Client = [1] 'test', TraceId = 2
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid PushMessage from client [1] 'test'. Encountered 1 error(s):
                            1. Expected channel ID to be greater than 0 but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 2",
                            "[Disposed] Client = [1] 'test', TraceId = 2",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 10)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldBeRejected_WhenChannelDoesNotExist()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );

        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendPushMessage( 1, [ ] );
                c.ReadMessageRejectedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestNull(),
                            exc.Publisher.TestNull() ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( remoteClient ) ] ),
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessage, Length = 10)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 1, Length = 0, ChannelId = 1, Confirm = True",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Client [1] 'test' could not push message to channel with ID 1 because it is not bound as a publisher to it.
                            """,
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (MessageRejectedResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 1, Packet = (MessageRejectedResponse, Length = 6)",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 10)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldBeRejected_WhenChannelDoesNotExist_WithoutConfirmation()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );

        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c => c.SendPushMessage( 1, [ ], confirm: false ) );
        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient ),
                            exc.Channel.TestNull(),
                            exc.Publisher.TestNull() ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ (c, _) => c.TestRefEquals( remoteClient ) ] ),
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (PushMessage, Length = 10)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 1, Length = 0, ChannelId = 1, Confirm = False",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Client [1] 'test' could not push message to channel with ID 1 because it is not bound as a publisher to it.
                            """,
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 10)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldBeRejected_WhenClientIsNotBoundAsPublisherToChannel()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    c => c.Id == 2
                        ? clientLogs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) )
                        : null )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask(
            c =>
            {
                c.SendPushMessage( 1, [ ] );
                c.ReadMessageRejectedResponse();
            } );

        await endSource.Task;

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient2 ),
                            exc.Channel.TestNull(),
                            exc.Publisher.TestNull() ) ),
                remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 2 ),
                server.Clients.GetAll().TestSetEqual( [ remoteClient1, remoteClient2 ] ),
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [2] 'test2', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [2] 'test2', TraceId = 1, Packet = (PushMessage, Length = 10)",
                            "[PushingMessage] Client = [2] 'test2', TraceId = 1, Length = 0, ChannelId = 1, Confirm = True",
                            """
                            [Error] Client = [2] 'test2', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Client [2] 'test2' could not push message to channel with ID 1 because it is not bound as a publisher to it.
                            """,
                            "[SendPacket:Sending] Client = [2] 'test2', TraceId = 1, Packet = (MessageRejectedResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [2] 'test2', TraceId = 1, Packet = (MessageRejectedResponse, Length = 6)",
                            "[Trace:PushMessage] Client = [2] 'test2', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [2] 'test2'",
                        "[AwaitPacket] Client = [2] 'test2', Packet = (PushMessage, Length = 10)"
                    ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldBeRejected_WhenClientIsNotBoundAsPublisherToChannel_WithoutConfirmation()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    c => c.Id == 2
                        ? clientLogs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) )
                        : null )
                .SetStreamEventHandlerFactory( _ => logs.Add ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
            } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask( c => c.SendPushMessage( 1, [ ], confirm: false ) );
        await endSource.Task;

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>(
                        exc => Assertion.All(
                            exc.Client.TestRefEquals( remoteClient2 ),
                            exc.Channel.TestNull(),
                            exc.Publisher.TestNull() ) ),
                remoteClient1.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                remoteClient2.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Running ) ),
                server.Clients.Count.TestEquals( 2 ),
                server.Clients.GetAll().TestSetEqual( [ remoteClient1, remoteClient2 ] ),
                clientLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [2] 'test2', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [2] 'test2', TraceId = 1, Packet = (PushMessage, Length = 10)",
                            "[PushingMessage] Client = [2] 'test2', TraceId = 1, Length = 0, ChannelId = 1, Confirm = False",
                            """
                            [Error] Client = [2] 'test2', TraceId = 1
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingException: Client [2] 'test2' could not push message to channel with ID 1 because it is not bound as a publisher to it.
                            """,
                            "[Trace:PushMessage] Client = [2] 'test2', TraceId = 1 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [2] 'test2'",
                        "[AwaitPacket] Client = [2] 'test2', Packet = (PushMessage, Length = 10)"
                    ] ),
                logs.GetAllStream().TestSequence( [ "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']" ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamProcessing_ShouldDisposeStreamAutomatically_WhenStreamIsEmptyAndNoPublishersAreRelated()
    {
        var endSource = new SafeTaskCompletionSource();
        var publisherDisposedSource = new SafeTaskCompletionSource<MessageBrokerStreamState>();
        var logs = new EventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => MessageBrokerRemoteClientLogger.Create(
                        publisherUnbound: e => publisherDisposedSource.Complete( e.Publisher.Stream.State ) ) )
                .SetStreamEventHandlerFactory(
                    _ => e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerStreamEventType.MessageDequeued )
                            publisherDisposedSource.Task.Wait();
                        else if ( e.Type == MessageBrokerStreamEventType.Disposed )
                            endSource.Complete();
                    } ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendPushMessage( 1, [ 1 ] );
                c.SendPushMessage( 1, [ 1, 2 ] );
                c.ReadMessageAcceptedResponse();
                c.ReadMessageAcceptedResponse();
                c.SendUnbindPublisherRequest( 1 );
                c.ReadPublisherUnboundResponse();
            } );

        var streamStateOnPublisherDisposed = await publisherDisposedSource.Task;
        await endSource.Task;

        Assertion.All(
                streamStateOnPublisherDisposed.TestEquals( MessageBrokerStreamState.Running ),
                server.Streams.Count.TestEquals( 0 ),
                server.Streams.GetAll().TestEmpty(),
                logs.GetAllStream()
                    .TestContainsSequence(
                    [
                        "[1::'c'::1] [Created] by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::2] [MessageEnqueued] MessageId = 0 by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [MessageDequeued] MessageId = 0 by publisher [1::'test'] => [1::'c']",
                        "[1::'c'::<ROOT>] [Disposing]",
                        "[1::'c'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }
}
