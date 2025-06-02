using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public class SystemNotificationTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public SystemNotificationTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task SenderName_ShouldBeHandledCorrectly_WhenFirstForParticularSender()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "foo" ) );
        await endSource.Task;

        Assertion.All(
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 13)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = SenderName",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 13)",
                            "[SenderNameProcessed] Client = [1] 'test', TraceId = 1, SenderId = 2, NewName = 'foo'",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task SenderName_ShouldBeHandledCorrectly_WhenNotFirstForParticularSender()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask(
            s =>
            {
                s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 3, "foo" );
                s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 3, "lorem" );
            } );

        await endSource.Task;

        Assertion.All(
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 2, Type = SenderName",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                            "[SenderNameProcessed] Client = [1] 'test', TraceId = 2, SenderId = 3, OldName = 'foo', NewName = 'lorem'",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 15)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task SenderName_ShouldDisposeClient_WhenExternalObjectNameSynchronizationIsDisabled()
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
                .SetSynchronizeExternalObjectNames( false )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "foo" ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 13)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = SenderName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. External object name synchronization is disabled.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task SenderName_ShouldDisposeClient_WhenInitialPayloadIsInvalid()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "foo", payload: 0 ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Where( t => t.Logs.Any( e => e.Contains( "[Trace:SystemNotification]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 5)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected header payload to be at least 1 but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 5)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task SenderName_ShouldDisposeClient_WhenPayloadIsInvalid()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "foo", payload: 4 ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 9)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = SenderName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected header payload to be at least 5 but found 4.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 9)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task SenderName_ShouldDisposeClient_WhenSenderIdIsInvalid()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 0, "foo" ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 13)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = SenderName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected sender ID to be greater than 0 but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task SenderName_ShouldDisposeClient_WhenSenderIdEqualsClientId()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 1, "foo" ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 13)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = SenderName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected sender ID 1 to not be equal to client's ID.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task SenderName_ShouldDisposeClient_WhenNameIsEmpty()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, "" ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 10)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = SenderName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected sender name length to be in [1, 512] range but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 10)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task SenderName_ShouldDisposeClient_WhenNameIsTooLong()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask(
            s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.SenderName, 2, new string( 'x', 513 ) ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 523)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = SenderName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected sender name length to be in [1, 512] range but found 513.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 523)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamName_ShouldBeHandledCorrectly_WhenFirstForParticularSender()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "foo" ) );
        await endSource.Task;

        Assertion.All(
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 13)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = StreamName",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 13)",
                            "[StreamNameProcessed] Client = [1] 'test', TraceId = 1, StreamId = 1, NewName = 'foo'",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamName_ShouldBeHandledCorrectly_WhenNotFirstForParticularSender()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask(
            s =>
            {
                s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 2, "foo" );
                s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 2, "lorem" );
            } );

        await endSource.Task;

        Assertion.All(
                logs.GetAll()
                    .Skip( 2 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 2 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 2, Type = StreamName",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (SystemNotification, Length = 15)",
                            "[StreamNameProcessed] Client = [1] 'test', TraceId = 2, StreamId = 2, OldName = 'foo', NewName = 'lorem'",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 2 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 15)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamName_ShouldDisposeClient_WhenExternalObjectNameSynchronizationIsDisabled()
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
                .SetSynchronizeExternalObjectNames( false )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "foo" ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 13)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = StreamName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. External object name synchronization is disabled.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamName_ShouldDisposeClient_WhenInitialPayloadIsInvalid()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "foo", payload: 0 ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Where( t => t.Logs.Any( e => e.Contains( "[Trace:SystemNotification]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestContainsSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 5)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected header payload to be at least 1 but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 5)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamName_ShouldDisposeClient_WhenPayloadIsInvalid()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "foo", payload: 4 ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 9)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = StreamName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected header payload to be at least 5 but found 4.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 9)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamName_ShouldDisposeClient_WhenSenderIdIsInvalid()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 0, "foo" ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 13)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = StreamName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected stream ID to be greater than 0 but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamName_ShouldDisposeClient_WhenNameIsEmpty()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, "" ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 10)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = StreamName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected stream name length to be in [1, 512] range but found 0.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 10)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StreamName_ShouldDisposeClient_WhenNameIsTooLong()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask(
            s => s.SendObjectNameNotification( MessageBrokerSystemNotificationType.StreamName, 1, new string( 'x', 513 ) ) );

        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 523)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = StreamName",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Expected stream name length to be in [1, 512] range but found 513.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 523)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task InvalidType_ShouldDisposeClient()
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
                                if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                    endSource.Complete();
                            } ) ) ) );

        await server.EstablishHandshake( client );
        await server.GetTask( s => s.SendObjectNameNotification( 0, 0, "" ) );
        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (start)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (SystemNotification, Length = 10)",
                            "[ProcessingSystemNotification] Client = [1] 'test', TraceId = 1, Type = <unrecognized-type-0>",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid SystemNotification from the server. Encountered 1 error(s):
                            1. Received unexpected system notification type <unrecognized-type-0>.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:SystemNotification] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (SystemNotification, Length = 10)"
                    ] ) )
            .Go();
    }
}
