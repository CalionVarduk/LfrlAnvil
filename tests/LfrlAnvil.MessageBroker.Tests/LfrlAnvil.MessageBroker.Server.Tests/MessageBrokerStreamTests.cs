using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Diagnostics;
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
        var storeKeyByMessageId = new ConcurrentDictionary<ulong, int>();
        var clientLogs = new ClientEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                endSource.Complete();
                        } ) ) )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger(
                    MessageBrokerStreamLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerStreamTraceEventType.ProcessMessage )
                                endSource.Complete();
                        },
                        messagePushed: e => storeKeyByMessageId[e.MessageId] = e.StoreKey ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
            c.SendPushMessage( 1, [ 1 ] );
            c.ReadMessageAcceptedResponse();
            c.SendPushMessage( 1, [ 2, 3 ] );
            c.ReadMessageAcceptedResponse();
            c.SendPushMessage( 1, [ 4, 5, 6 ] );
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
                streamLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.Contains( "[Trace:PushMessage]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ClientTrace] Stream = [1] 'c', TraceId = {t.Id}, Correlation = (Client = [1] 'test', TraceId = 2)",
                            $"[MessagePushed] Stream = [1] 'c', TraceId = {t.Id}, Client = [1] 'test', Channel = [1] 'c', MessageId = 0, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 0UL )}, Length = 1",
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ClientTrace] Stream = [1] 'c', TraceId = {t.Id}, Correlation = (Client = [1] 'test', TraceId = 3)",
                            $"[MessagePushed] Stream = [1] 'c', TraceId = {t.Id}, Client = [1] 'test', Channel = [1] 'c', MessageId = 1, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 1UL )}, Length = 2",
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ClientTrace] Stream = [1] 'c', TraceId = {t.Id}, Correlation = (Client = [1] 'test', TraceId = 4)",
                            $"[MessagePushed] Stream = [1] 'c', TraceId = {t.Id}, Client = [1] 'test', Channel = [1] 'c', MessageId = 2, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 2UL )}, Length = 3",
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.Contains( "[Trace:ProcessMessage]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Stream = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, Length = 1, HasRouting = False, ListenerCount = 0",
                            $"[MessageProcessed] Stream = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, Failures = 0, Filtered = 0",
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Stream = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 1, Length = 2, HasRouting = False, ListenerCount = 0",
                            $"[MessageProcessed] Stream = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 1, Failures = 0, Filtered = 0",
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Stream = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 2, Length = 3, HasRouting = False, ListenerCount = 0",
                            $"[MessageProcessed] Stream = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 2, Failures = 0, Filtered = 0",
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldEnqueueMessageCorrectly_WithoutConfirmation()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var clientLogs = new ClientEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                endSource.Complete();
                        } ) ) )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger(
                    MessageBrokerStreamLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerStreamTraceEventType.ProcessMessage )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
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
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Stream = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[MessagePushed] Stream = [1] 'c', TraceId = 1, Client = [1] 'test', Channel = [1] 'c', MessageId = 0, StoreKey = 0, Length = 1",
                            "[Trace:PushMessage] Stream = [1] 'c', TraceId = 1 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:ProcessMessage] Stream = [1] 'c', TraceId = 2 (start)",
                            "[ProcessingMessage] Stream = [1] 'c', TraceId = 2, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, Length = 1, HasRouting = False, ListenerCount = 0",
                            "[MessageProcessed] Stream = [1] 'c', TraceId = 2, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, Failures = 0, Filtered = 0",
                            "[Trace:ProcessMessage] Stream = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldEnqueueMessagesCorrectly_WithDifferentChannels()
    {
        var enqueueEndSource = new SafeTaskCompletionSource( completionCount: 5 );
        var endSource = new SafeTaskCompletionSource( completionCount: 10 );
        var storeKeyByMessageId = new ConcurrentDictionary<ulong, int>();
        var clientLogs = new ClientEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                endSource.Complete();
                        } ) ) )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger(
                    MessageBrokerStreamLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerStreamTraceEventType.ProcessMessage )
                            {
                                enqueueEndSource.Task.Wait();
                                endSource.Complete();
                            }
                            else if ( e.Type == MessageBrokerStreamTraceEventType.PushMessage )
                                enqueueEndSource.Complete();
                        },
                        messagePushed: e => storeKeyByMessageId[e.MessageId] = e.StoreKey ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
            c.SendBindPublisherRequest( "d", true, "c" );
            c.ReadPublisherBoundResponse();
            c.SendBindPublisherRequest( "e", true, "c" );
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
                streamLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.Contains( "[Trace:PushMessage]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ClientTrace] Stream = [1] 'c', TraceId = {t.Id}, Correlation = (Client = [1] 'test', TraceId = 4)",
                            $"[MessagePushed] Stream = [1] 'c', TraceId = {t.Id}, Client = [1] 'test', Channel = [1] 'c', MessageId = 0, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 0UL )}, Length = 1",
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ClientTrace] Stream = [1] 'c', TraceId = {t.Id}, Correlation = (Client = [1] 'test', TraceId = 5)",
                            $"[MessagePushed] Stream = [1] 'c', TraceId = {t.Id}, Client = [1] 'test', Channel = [2] 'd', MessageId = 1, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 1UL )}, Length = 2",
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ClientTrace] Stream = [1] 'c', TraceId = {t.Id}, Correlation = (Client = [1] 'test', TraceId = 6)",
                            $"[MessagePushed] Stream = [1] 'c', TraceId = {t.Id}, Client = [1] 'test', Channel = [3] 'e', MessageId = 2, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 2UL )}, Length = 3",
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ClientTrace] Stream = [1] 'c', TraceId = {t.Id}, Correlation = (Client = [1] 'test', TraceId = 7)",
                            $"[MessagePushed] Stream = [1] 'c', TraceId = {t.Id}, Client = [1] 'test', Channel = [3] 'e', MessageId = 3, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 3UL )}, Length = 4",
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ClientTrace] Stream = [1] 'c', TraceId = {t.Id}, Correlation = (Client = [1] 'test', TraceId = 8)",
                            $"[MessagePushed] Stream = [1] 'c', TraceId = {t.Id}, Client = [1] 'test', Channel = [1] 'c', MessageId = 4, StoreKey = {storeKeyByMessageId.GetValueOrDefault( 4UL )}, Length = 5",
                            $"[Trace:PushMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] )
                    ] ),
                streamLogs.GetAll()
                    .Where( t => t.Logs.Any( e => e.Contains( "[Trace:ProcessMessage]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Stream = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, Length = 1, HasRouting = False, ListenerCount = 0",
                            $"[MessageProcessed] Stream = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, Failures = 0, Filtered = 0",
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Stream = [1] 'c', TraceId = {t.Id}, Channel = [2] 'd', Sender = [1] 'test', MessageId = 1, Length = 2, HasRouting = False, ListenerCount = 0",
                            $"[MessageProcessed] Stream = [1] 'c', TraceId = {t.Id}, Channel = [2] 'd', Sender = [1] 'test', MessageId = 1, Failures = 0, Filtered = 0",
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Stream = [1] 'c', TraceId = {t.Id}, Channel = [3] 'e', Sender = [1] 'test', MessageId = 2, Length = 3, HasRouting = False, ListenerCount = 0",
                            $"[MessageProcessed] Stream = [1] 'c', TraceId = {t.Id}, Channel = [3] 'e', Sender = [1] 'test', MessageId = 2, Failures = 0, Filtered = 0",
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Stream = [1] 'c', TraceId = {t.Id}, Channel = [3] 'e', Sender = [1] 'test', MessageId = 3, Length = 4, HasRouting = False, ListenerCount = 0",
                            $"[MessageProcessed] Stream = [1] 'c', TraceId = {t.Id}, Channel = [3] 'e', Sender = [1] 'test', MessageId = 3, Failures = 0, Filtered = 0",
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Stream = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 4, Length = 5, HasRouting = False, ListenerCount = 0",
                            $"[MessageProcessed] Stream = [1] 'c', TraceId = {t.Id}, Channel = [1] 'c', Sender = [1] 'test', MessageId = 4, Failures = 0, Filtered = 0",
                            $"[Trace:ProcessMessage] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldEnqueueLargeMessageCorrectly()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var clientLogs = new ClientEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                endSource.Complete();
                        } ) ) )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger(
                    MessageBrokerStreamLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerStreamTraceEventType.ProcessMessage )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
            c.SendPushMessage(
                1,
                Enumerable.Range( 0, ( int )MemorySize.BytesPerKilobyte * 20 ).Select( static x => ( byte )x ).ToArray(),
                confirm: false );
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
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 20490)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 2, Length = 20480, ChannelId = 1, Confirm = False",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (PushMessage, Length = 20490)",
                            "[MessagePushed] Client = [1] 'test', TraceId = 2, Channel = [1] 'c', Stream = [1] 'c', MessageId = 0",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 20490)"
                    ] ),
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Stream = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[MessagePushed] Stream = [1] 'c', TraceId = 1, Client = [1] 'test', Channel = [1] 'c', MessageId = 0, StoreKey = 0, Length = 20480",
                            "[Trace:PushMessage] Stream = [1] 'c', TraceId = 1 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:ProcessMessage] Stream = [1] 'c', TraceId = 2 (start)",
                            "[ProcessingMessage] Stream = [1] 'c', TraceId = 2, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, Length = 20480, HasRouting = False, ListenerCount = 0",
                            "[MessageProcessed] Stream = [1] 'c', TraceId = 2, Channel = [1] 'c', Sender = [1] 'test', MessageId = 0, Failures = 0, Filtered = 0",
                            "[Trace:ProcessMessage] Stream = [1] 'c', TraceId = 2 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldDisposeClient_WhenClientSendsInvalidPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                endSource.Complete();
                        } ) ) )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
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
                            "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 9)"
                    ] ),
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Stream = [1] 'c', TraceId = 1, Client = [1] 'test', Channel = [1] 'c'",
                            "[Disposing] Stream = [1] 'c', TraceId = 1",
                            "[Disposed] Stream = [1] 'c', TraceId = 1",
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldDisposeClient_WhenClientSendsNonPositiveChannelId()
    {
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                endSource.Complete();
                        } ) ) )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger() ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c =>
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
                            "[Deactivating] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 2, IsAlive = False",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                clientLogs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (PushMessage, Length = 10)"
                    ] ),
                streamLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 1 (start)",
                            "[ClientTrace] Stream = [1] 'c', TraceId = 1, Correlation = (Client = [1] 'test', TraceId = 2)",
                            "[PublisherUnbound] Stream = [1] 'c', TraceId = 1, Client = [1] 'test', Channel = [1] 'c'",
                            "[Disposing] Stream = [1] 'c', TraceId = 1",
                            "[Disposed] Stream = [1] 'c', TraceId = 1",
                            "[Trace:UnbindPublisher] Stream = [1] 'c', TraceId = 1 (end)"
                        ] )
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
        await client.GetTask( c =>
        {
            c.SendPushMessage( 1, [ ] );
            c.ReadMessageRejectedResponse();
        } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient ),
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
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
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
                    .Exact<MessageBrokerChannelPublisherBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient ),
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
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 2
                    ? clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) )
                    : null ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        using var client2 = new ClientMock();
        await client2.EstablishHandshake( server, "test2" );
        await client2.GetTask( c =>
        {
            c.SendPushMessage( 1, [ ] );
            c.ReadMessageRejectedResponse();
        } );

        await endSource.Task;

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerChannelPublisherBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient2 ),
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
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldBeRejected_WhenClientIsNotBoundAsPublisherToChannel_WithoutConfirmation()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( c => c.Id == 2
                    ? clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                    endSource.Complete();
                            },
                            error: e => exception = e.Exception ) )
                    : null ) );

        await server.StartAsync();

        using var client1 = new ClientMock();
        await client1.EstablishHandshake( server );
        await client1.GetTask( c =>
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
                    .Exact<MessageBrokerChannelPublisherBindingException>( exc => Assertion.All(
                        exc.Client.TestRefEquals( remoteClient2 ),
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
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamProcessing_ShouldDisposeStreamAutomatically_WhenStreamIsEmptyAndNoPublishersAreRelated()
    {
        var endSource = new SafeTaskCompletionSource();
        var publisherDisposedSource = new SafeTaskCompletionSource<MessageBrokerStreamState>();
        var streamLogs = new StreamEventLogger();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ =>
                    MessageBrokerRemoteClientLogger.Create(
                        publisherUnbound: e => publisherDisposedSource.Complete( e.Publisher.Stream.State ) ) )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger(
                    MessageBrokerStreamLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerStreamTraceEventType.ProcessMessage )
                                publisherDisposedSource.Task.Wait();
                            else if ( e.Type == MessageBrokerStreamTraceEventType.Dispose )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
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
                streamLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:Dispose] Stream = [1] 'c', TraceId = {t.Id} (start)",
                            $"[Disposing] Stream = [1] 'c', TraceId = {t.Id}",
                            $"[Disposed] Stream = [1] 'c', TraceId = {t.Id}",
                            $"[Trace:Dispose] Stream = [1] 'c', TraceId = {t.Id} (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Disposal_ShouldDiscardUnprocessedMessages()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource();
        var disposeContinuation = new SafeTaskCompletionSource();
        var processingContinuation = new SafeTaskCompletionSource( completionCount: 3 );
        var pushContinuation = new SafeTaskCompletionSource();
        var streamLogs = new StreamEventLogger();
        var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetLogger(
                    MessageBrokerServerLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerServerTraceEventType.Dispose )
                                endSource.Complete();
                        } ) )
                .SetClientLoggerFactory( _ => MessageBrokerRemoteClientLogger.Create(
                    traceStart: e =>
                    {
                        if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage && e.Source.TraceId > 2 )
                            pushContinuation.Task.Wait();
                    } ) )
                .SetStreamLoggerFactory( _ => streamLogs.GetLogger(
                    MessageBrokerStreamLogger.Create(
                        traceStart: e =>
                        {
                            if ( e.Type == MessageBrokerStreamTraceEventType.ProcessMessage )
                            {
                                pushContinuation.Complete();
                                processingContinuation.Task.Wait();
                                var __ = e.Source.Stream.Server.DisposeAsync().AsTask();
                                disposeContinuation.Task.Wait();
                            }
                        },
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerStreamTraceEventType.PushMessage )
                                processingContinuation.Complete();
                        },
                        disposing: _ => disposeContinuation.Complete(),
                        error: e => exception = e.Exception ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
        } );

        var stream = server.Streams.TryGetById( 1 );
        await client.GetTask( c =>
        {
            c.SendPushMessage( 1, [ 1 ], confirm: false );
            c.SendPushMessage( 1, [ 1, 2 ], confirm: false );
            c.SendPushMessage( 1, [ 1, 2, 3 ], confirm: false );
        } );

        await endSource.Task;

        Assertion.All(
                exception.TestType().Exact<MessageBrokerStreamException>( exc => exc.Stream.TestRefEquals( stream ) ),
                stream.TestNotNull( s => s.Messages.Count.TestEquals( 0 ) ),
                server.Streams.Count.TestEquals( 0 ),
                server.Streams.GetAll().TestEmpty(),
                streamLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Stream = [1] 'c', TraceId = 5 (start)",
                            $"[ServerTrace] Stream = [1] 'c', TraceId = 5, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                            "[Disposing] Stream = [1] 'c', TraceId = 5",
                            """
                            [Error] Stream = [1] 'c', TraceId = 5
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerStreamException: 2 stored pending message(s) have been discarded due to server disposal.
                            """,
                            "[Disposed] Stream = [1] 'c', TraceId = 5",
                            "[Trace:Dispose] Stream = [1] 'c', TraceId = 5 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task PushMessage_ShouldBeIgnoredForInactivePublisher()
    {
        using var storage = StorageScope.Create();
        storage.WriteServerMetadata();
        storage.WriteChannelMetadata( channelId: 1, channelName: "foo" );
        storage.WriteStreamMetadata( streamId: 1, streamName: "foo" );
        storage.WriteStreamMessages( streamId: 1, messages: [ ] );
        storage.WriteClientMetadata( clientId: 1, clientName: "test" );
        storage.WritePublisherMetadata( clientId: 1, channelId: 1, streamId: 1 );

        var endSource = new SafeTaskCompletionSource();
        var clientLogs = new ClientEventLogger();

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetRootStoragePath( storage.Path )
                .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.PushMessage )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Publishers.TryGetByChannelId( 1 );

        using var client = new ClientMock();
        await client.EstablishHandshake( server, isEphemeral: false );
        await client.GetTask( c =>
        {
            c.SendPushMessage( channelId: 1, data: [ 1 ] );
            c.ReadMessageRejectedResponse();
        } );

        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => Assertion.All(
                    "client",
                    c.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                    c.Publishers.Count.TestEquals( 1 ) ) ),
                binding.TestNotNull( p => Assertion.All(
                    "publisher",
                    p.State.TestEquals( MessageBrokerChannelPublisherBindingState.Inactive ) ) ),
                clientLogs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 3 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 3, Packet = (PushMessage, Length = 11)",
                            "[PushingMessage] Client = [1] 'test', TraceId = 3, Length = 1, ChannelId = 1, Confirm = True",
                            """
                            [Error] Client = [1] 'test', TraceId = 3
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerChannelPublisherBindingDeactivatedException: Operation has been cancelled because publisher binding between client [1] 'test' and channel [1] 'foo' is deactivated.
                            """,
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 3, Packet = (MessageRejectedResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 3, Packet = (MessageRejectedResponse, Length = 6)",
                            "[Trace:PushMessage] Client = [1] 'test', TraceId = 3 (end)"
                        ] )
                    ] ) )
            .Go();
    }
}
