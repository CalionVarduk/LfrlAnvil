using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Functional;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerServerTests : TestsBase
{
    [Fact]
    public void Ctor_WithDefaultOptions_ShouldCreateCorrectServer()
    {
        var localEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = new MessageBrokerServer( localEndPoint );
        Assertion.All(
                sut.LocalEndPoint.TestRefEquals( localEndPoint ),
                sut.HandshakeTimeout.TestEquals( Duration.FromSeconds( 15 ) ),
                sut.MaxNetworkPacketLength.TestEquals( MemorySize.FromKilobytes( 16 ) ),
                sut.MaxNetworkMessagePacketLength.TestEquals( MemorySize.FromMegabytes( 10 ) ),
                sut.AcceptableMessageTimeout.TestEquals(
                    Bounds.Create( Duration.FromMilliseconds( 1 ), Duration.FromMilliseconds( int.MaxValue ) ) ),
                sut.AcceptablePingInterval.TestEquals( Bounds.Create( Duration.FromMilliseconds( 1 ), Duration.FromHours( 24 ) ) ),
                sut.AcceptableMaxBatchPacketCount.TestEquals( Bounds.Create<short>( 0, 100 ) ),
                sut.AcceptableMaxNetworkBatchPacketLength.TestEquals(
                    Bounds.Create( MemorySize.FromKilobytes( 16 ), MemorySize.FromMegabytes( 10 ) ) ),
                sut.ExpressionFactory.TestNull(),
                sut.State.TestEquals( MessageBrokerServerState.Created ),
                sut.ToString().TestEquals( $"{localEndPoint} server (Created)" ),
                sut.Clients.Count.TestEquals( 0 ),
                sut.Clients.GetAll().TestEmpty(),
                sut.Channels.Count.TestEquals( 0 ),
                sut.Channels.GetAll().TestEmpty(),
                sut.Streams.Count.TestEquals( 0 ),
                sut.Streams.GetAll().TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 )]
    [InlineData( 29999, 2, 30001, 3, 49999, 4, 600000, 60, 700000, 70 )]
    [InlineData( (int.MaxValue + 1L) * 10000, int.MaxValue, 0, 1, (int.MaxValue + 2L) * 10000, int.MaxValue, 0, 1, 864000010000, 86400000 )]
    public void Ctor_WithCustomTimeouts_ShouldCreateCorrectServer(
        long handshakeTimeoutTicks,
        int expectedHandshakeTimeoutMs,
        long minMessageTimeoutTicks,
        int expectedMinMessageTimeoutMs,
        long maxMessageTimeoutTicks,
        int expectedMaxMessageTimeoutMs,
        long minPingIntervalTicks,
        int expectedMinPingIntervalMs,
        long maxPingIntervalTicks,
        int expectedMaxPingIntervalMs)
    {
        var expressionFactory = Substitute.For<IParsedExpressionFactory>();
        var localEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = new MessageBrokerServer(
            localEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromTicks( handshakeTimeoutTicks ) )
                .SetAcceptableMessageTimeout(
                    Bounds.Create( Duration.FromTicks( minMessageTimeoutTicks ), Duration.FromTicks( maxMessageTimeoutTicks ) ) )
                .SetAcceptablePingInterval(
                    Bounds.Create( Duration.FromTicks( minPingIntervalTicks ), Duration.FromTicks( maxPingIntervalTicks ) ) )
                .SetNetworkPacketOptions(
                    MessageBrokerServerNetworkPacketOptions.Default
                        .SetMaxLength( MemorySize.FromKilobytes( 30 ) )
                        .SetMaxMessageLength( MemorySize.FromMegabytes( 100 ) )
                        .SetMaxBatchPacketCount( 100 )
                        .SetMaxBatchLength( MemorySize.FromMegabytes( 80 ) ) )
                .SetExpressionFactory( expressionFactory ) );

        Assertion.All(
                sut.LocalEndPoint.TestRefEquals( localEndPoint ),
                sut.HandshakeTimeout.TestEquals( Duration.FromMilliseconds( expectedHandshakeTimeoutMs ) ),
                sut.MaxNetworkPacketLength.TestEquals( MemorySize.FromKilobytes( 30 ) ),
                sut.MaxNetworkMessagePacketLength.TestEquals( MemorySize.FromMegabytes( 100 ) ),
                sut.AcceptableMaxBatchPacketCount.TestEquals( Bounds.Create<short>( 0, 100 ) ),
                sut.AcceptableMaxNetworkBatchPacketLength.TestEquals(
                    Bounds.Create( MemorySize.FromKilobytes( 30 ), MemorySize.FromMegabytes( 80 ) ) ),
                sut.AcceptableMessageTimeout.TestEquals(
                    Bounds.Create(
                        Duration.FromMilliseconds( expectedMinMessageTimeoutMs ),
                        Duration.FromMilliseconds( expectedMaxMessageTimeoutMs ) ) ),
                sut.AcceptablePingInterval.TestEquals(
                    Bounds.Create(
                        Duration.FromMilliseconds( expectedMinPingIntervalMs ),
                        Duration.FromMilliseconds( expectedMaxPingIntervalMs ) ) ),
                sut.ExpressionFactory.TestRefEquals( expressionFactory ),
                sut.State.TestEquals( MessageBrokerServerState.Created ),
                sut.Clients.Count.TestEquals( 0 ),
                sut.Clients.GetAll().TestEmpty(),
                sut.Channels.Count.TestEquals( 0 ),
                sut.Channels.GetAll().TestEmpty(),
                sut.Streams.Count.TestEquals( 0 ),
                sut.Streams.GetAll().TestEmpty() )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldStartServer()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ServerEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetLogger(
                logs.GetLogger( MessageBrokerServerLogger.Create( awaitClient: _ => endSource.Complete() ) ) ) );

        var result = await server.StartAsync();
        var localEndPoint = server.LocalEndPoint;
        await endSource.Task;

        Assertion.All(
                result.Exception.TestNull(),
                server.LocalEndPoint.TestType()
                    .AssignableTo<IPEndPoint>(
                        e => Assertion.All(
                            "LocalEndPoint",
                            e.Address.TestEquals( IPAddress.Loopback ),
                            e.Port.TestNotEquals( 0 ) ) ),
                server.State.TestEquals( MessageBrokerServerState.Running ),
                server.ToString().TestEquals( $"{localEndPoint} server (Running)" ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:Start] Server = {originalEndPoint}, TraceId = 0 (start)",
                            $"[ListenerStarting] Server = {originalEndPoint}, TraceId = 0, HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = [0.001 second(s), 2147483.647 second(s)], AcceptablePingInterval = [0.001 second(s), 86400 second(s)]",
                            $"[ListenerStarted] Server = {localEndPoint}, TraceId = 0",
                            $"[Trace:Start] Server = {localEndPoint}, TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitClient()
                    .TestSequence(
                    [
                        $"[AwaitClient] Server = {localEndPoint}"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldStartServer_WithThrowingEventHandler()
    {
        var logs = new ServerEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetLogger(
                logs.GetLogger( MessageBrokerServerLogger.Create( traceStart: _ => throw new Exception( "ignored" ) ) ) ) );

        var result = await server.StartAsync();
        var localEndPoint = server.LocalEndPoint;

        Assertion.All(
                result.Exception.TestNull(),
                server.LocalEndPoint.TestType()
                    .AssignableTo<IPEndPoint>(
                        e => Assertion.All(
                            "LocalEndPoint",
                            e.Address.TestEquals( IPAddress.Loopback ),
                            e.Port.TestNotEquals( 0 ) ) ),
                server.State.TestEquals( MessageBrokerServerState.Running ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:Start] Server = {originalEndPoint}, TraceId = 0 (start)",
                            $"[ListenerStarting] Server = {originalEndPoint}, TraceId = 0, HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = [0.001 second(s), 2147483.647 second(s)], AcceptablePingInterval = [0.001 second(s), 86400 second(s)]",
                            $"[ListenerStarted] Server = {localEndPoint}, TraceId = 0",
                            $"[Trace:Start] Server = {localEndPoint}, TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitClient()
                    .TestSequence(
                    [
                        $"[AwaitClient] Server = {localEndPoint}"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldThrowMessageBrokerServerDisposedException_WhenServerIsDisposed()
    {
        var sut = new MessageBrokerServer( new IPEndPoint( IPAddress.Loopback, 12345 ) );
        sut.Dispose();

        Exception? exception = null;
        try
        {
            _ = await sut.StartAsync();
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        Assertion.All(
                exception.TestType().Exact<MessageBrokerServerDisposedException>( e => e.Server.TestRefEquals( sut ) ),
                sut.State.TestEquals( MessageBrokerServerState.Disposed ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldThrowMessageBrokerServerStateException_WhenServerHasAlreadyBeenStarted()
    {
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        var action = Lambda.Of( async () => await server.StartAsync() );

        action.Test(
                exc => exc.TestType()
                    .Exact<MessageBrokerServerStateException>(
                        e => Assertion.All(
                            e.Server.TestRefEquals( server ),
                            e.Actual.TestEquals( MessageBrokerServerState.Running ),
                            e.Expected.TestEquals( MessageBrokerServerState.Created ) ) ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerServerDisposedException_WhenServerIsDisposedDuringStarting()
    {
        var logs = new ServerEventLogger();
        var localEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            localEndPoint,
            MessageBrokerServerOptions.Default.SetLogger(
                logs.GetLogger( MessageBrokerServerLogger.Create( listenerStarting: e => e.Source.Server.Dispose() ) ) ) );

        var result = await server.StartAsync();

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerServerDisposedException>( e => e.Server.TestRefEquals( server ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:Start] Server = {localEndPoint}, TraceId = 0 (start)",
                            $"[ListenerStarting] Server = {localEndPoint}, TraceId = 0, HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = [0.001 second(s), 2147483.647 second(s)], AcceptablePingInterval = [0.001 second(s), 86400 second(s)]",
                            $"""
                             [Error] Server = {localEndPoint}, TraceId = 0
                             LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerDisposedException: Operation has been cancelled because server is disposed.
                             """,
                            $"[Trace:Start] Server = {localEndPoint}, TraceId = 0 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:Dispose] Server = {localEndPoint}, TraceId = 1 (start)",
                            $"[Disposing] Server = {localEndPoint}, TraceId = 1",
                            $"[Disposed] Server = {localEndPoint}, TraceId = 1",
                            $"[Trace:Dispose] Server = {localEndPoint}, TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitClient().TestEmpty() )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerServerDisposedException_WhenServerIsDisposedAfterStarting()
    {
        var logs = new ServerEventLogger();
        var localEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            localEndPoint,
            MessageBrokerServerOptions.Default.SetLogger(
                logs.GetLogger( MessageBrokerServerLogger.Create( listenerStarted: e => e.Source.Server.Dispose() ) ) ) );

        var result = await server.StartAsync();

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerServerDisposedException>( e => e.Server.TestRefEquals( server ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:Start] Server = {localEndPoint}, TraceId = 0 (start)",
                            $"[ListenerStarting] Server = {localEndPoint}, TraceId = 0, HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = [0.001 second(s), 2147483.647 second(s)], AcceptablePingInterval = [0.001 second(s), 86400 second(s)]",
                            $"[ListenerStarted] Server = {server.LocalEndPoint}, TraceId = 0",
                            $"""
                             [Error] Server = {server.LocalEndPoint}, TraceId = 0
                             LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerDisposedException: Operation has been cancelled because server is disposed.
                             """,
                            $"[Trace:Start] Server = {server.LocalEndPoint}, TraceId = 0 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:Dispose] Server = {server.LocalEndPoint}, TraceId = 1 (start)",
                            $"[Disposing] Server = {server.LocalEndPoint}, TraceId = 1",
                            $"[Disposed] Server = {server.LocalEndPoint}, TraceId = 1",
                            $"[Trace:Dispose] Server = {server.LocalEndPoint}, TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitClient()
                    .TestSequence(
                    [
                        (e, _) => e.TestEquals( $"[AwaitClient] Server = {server.LocalEndPoint}" ),
                        (e, _) => e.TestStartsWith(
                            $"""
                             [AwaitClient] Server = {server.LocalEndPoint}
                             System.InvalidOperationException:
                             """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnThrownException_WhenServerIsAttemptingToListenOnActivePort()
    {
        var logs = new ServerEventLogger();

        await using var server = new MessageBrokerServer( new IPEndPoint( IPAddress.Loopback, 0 ) );

        await server.StartAsync();

        await using var other = new MessageBrokerServer(
            ( IPEndPoint )server.LocalEndPoint,
            MessageBrokerServerOptions.Default.SetLogger( logs.GetLogger() ) );

        var result = await other.StartAsync();

        Assertion.All(
                result.Exception.TestType().AssignableTo<SocketException>(),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            (e, _) => e.TestEquals( $"[Trace:Start] Server = {server.LocalEndPoint}, TraceId = 0 (start)" ),
                            (e, _) => e.TestEquals(
                                $"[ListenerStarting] Server = {server.LocalEndPoint}, TraceId = 0, HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = [0.001 second(s), 2147483.647 second(s)], AcceptablePingInterval = [0.001 second(s), 86400 second(s)]" ),
                            (e, _) => e.TestStartsWith(
                                $"""
                                 [Error] Server = {server.LocalEndPoint}, TraceId = 0
                                 System.Net.Sockets.SocketException
                                 """ ),
                            (e, _) => e.TestEquals( $"[Disposing] Server = {server.LocalEndPoint}, TraceId = 0" ),
                            (e, _) => e.TestEquals( $"[Disposed] Server = {server.LocalEndPoint}, TraceId = 0" ),
                            (e, _) => e.TestEquals( $"[Trace:Start] Server = {server.LocalEndPoint}, TraceId = 0 (end)" )
                        ] )
                    ] ),
                logs.GetAllAwaitClient().TestEmpty() )
            .Go();
    }

    [Fact]
    public void StartAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var token = new CancellationToken( canceled: true );
        var sut = new MessageBrokerServer( new IPEndPoint( IPAddress.Loopback, 0 ) );

        var action = Lambda.Of( async () => await sut.StartAsync( token ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().AssignableTo<OperationCanceledException>(),
                    sut.State.TestEquals( MessageBrokerServerState.Created ) ) )
            .Go();
    }

    [Fact]
    public async Task ClientListener_ShouldDiscardClient_WhenClientCannotBeCreated()
    {
        var exception = new Exception( "failure" );
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var logs = new ServerEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
        await using var sut = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerServerLogger.Create(
                            awaitClient: e =>
                            {
                                if ( e.EndPoint is null )
                                    endSource.Complete();
                            } ) ) )
                .SetClientLoggerFactory( _ => throw exception ) );

        await sut.StartAsync();
        var endPoint = sut.LocalEndPoint;

        var client = new ClientMock();
        var clientTask = Task.Factory.StartNew(
            o =>
            {
                var c = ( ClientMock )o!;
                c.Connect( endPoint );
            },
            client );

        await clientTask;
        await endSource.Task;

        Assertion.All(
                sut.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            (e, _) => e.TestEquals( $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)" ),
                            (e, _) => e.TestStartsWith(
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 System.Exception: failure
                                 """ ),
                            (e, _) => e.TestEquals( $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)" )
                        ] )
                    ] ),
                logs.GetAllAwaitClient()
                    .TestSequence(
                    [
                        (e, _) => e.TestEquals( $"[AwaitClient] Server = {endPoint}" ),
                        (e, _) => e.TestStartsWith( $"[AwaitClient] Server = {endPoint}, EndPoint = " ),
                        (e, _) => e.TestEquals( $"[AwaitClient] Server = {endPoint}" )
                    ] ) )
            .Go();
    }
}
