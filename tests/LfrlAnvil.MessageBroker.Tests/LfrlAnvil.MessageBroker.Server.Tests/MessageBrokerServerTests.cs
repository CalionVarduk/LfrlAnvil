using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
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
                sut.AcceptableMessageTimeout.TestEquals(
                    Bounds.Create( Duration.FromMilliseconds( 1 ), Duration.FromMilliseconds( int.MaxValue ) ) ),
                sut.AcceptablePingInterval.TestEquals( Bounds.Create( Duration.FromMilliseconds( 1 ), Duration.FromHours( 24 ) ) ),
                sut.State.TestEquals( MessageBrokerServerState.Created ),
                sut.ToString().TestEquals( $"{localEndPoint} server (Created)" ),
                sut.Clients.Count.TestEquals( 0 ),
                sut.Clients.GetAll().TestEmpty(),
                sut.Channels.Count.TestEquals( 0 ),
                sut.Channels.GetAll().TestEmpty() )
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
        var localEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = new MessageBrokerServer(
            localEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromTicks( handshakeTimeoutTicks ) )
                .SetAcceptableMessageTimeout(
                    Bounds.Create( Duration.FromTicks( minMessageTimeoutTicks ), Duration.FromTicks( maxMessageTimeoutTicks ) ) )
                .SetAcceptablePingInterval(
                    Bounds.Create( Duration.FromTicks( minPingIntervalTicks ), Duration.FromTicks( maxPingIntervalTicks ) ) ) );

        Assertion.All(
                sut.LocalEndPoint.TestRefEquals( localEndPoint ),
                sut.HandshakeTimeout.TestEquals( Duration.FromMilliseconds( expectedHandshakeTimeoutMs ) ),
                sut.AcceptableMessageTimeout.TestEquals(
                    Bounds.Create(
                        Duration.FromMilliseconds( expectedMinMessageTimeoutMs ),
                        Duration.FromMilliseconds( expectedMaxMessageTimeoutMs ) ) ),
                sut.AcceptablePingInterval.TestEquals(
                    Bounds.Create(
                        Duration.FromMilliseconds( expectedMinPingIntervalMs ),
                        Duration.FromMilliseconds( expectedMaxPingIntervalMs ) ) ),
                sut.State.TestEquals( MessageBrokerServerState.Created ),
                sut.Clients.Count.TestEquals( 0 ),
                sut.Clients.GetAll().TestEmpty(),
                sut.Channels.Count.TestEquals( 0 ),
                sut.Channels.GetAll().TestEmpty() )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldStartServer()
    {
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetEventHandler( logs.Add ) );

        var result = await server.StartAsync();
        var localEndPoint = server.LocalEndPoint;
        var events = logs.GetAllServer();

        Assertion.All(
                result.Exception.TestNull(),
                server.LocalEndPoint.Address.TestEquals( IPAddress.Loopback ),
                server.LocalEndPoint.Port.TestNotEquals( 0 ),
                server.State.TestEquals( MessageBrokerServerState.Running ),
                server.ToString().TestEquals( $"{localEndPoint} server (Running)" ),
                events.TestSequence(
                [
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {localEndPoint}",
                    "[WaitingForClient]"
                ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldStartServer_WithThrowingEventHandler()
    {
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetEventHandler(
                e =>
                {
                    logs.Add( e );
                    throw new Exception( "ignored" );
                } ) );

        var result = await server.StartAsync();
        var localEndPoint = server.LocalEndPoint;
        var events = logs.GetAllServer();

        Assertion.All(
                result.Exception.TestNull(),
                server.LocalEndPoint.Address.TestEquals( IPAddress.Loopback ),
                server.LocalEndPoint.Port.TestNotEquals( 0 ),
                server.State.TestEquals( MessageBrokerServerState.Running ),
                events.TestSequence(
                [
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {localEndPoint}",
                    "[WaitingForClient]"
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
        var logs = new EventLogger();
        var localEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            localEndPoint,
            MessageBrokerServerOptions.Default.SetEventHandler(
                e =>
                {
                    logs.Add( e );
                    if ( e.Type == MessageBrokerServerEventType.Starting )
                        e.Server.Dispose();
                } ) );

        var result = await server.StartAsync();

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerServerDisposedException>( e => e.Server.TestRefEquals( server ) ),
                logs.GetAllServer()
                    .TestSequence(
                    [
                        $"[Starting] At {localEndPoint} (HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                        "[Disposing]",
                        "[Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerServerDisposedException_WhenServerIsDisposedAfterStarting()
    {
        var logs = new EventLogger();
        var localEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            localEndPoint,
            MessageBrokerServerOptions.Default.SetEventHandler(
                e =>
                {
                    logs.Add( e );
                    if ( e.Type == MessageBrokerServerEventType.Started )
                        e.Server.Dispose();
                } ) );

        var result = await server.StartAsync();

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerServerDisposedException>( e => e.Server.TestRefEquals( server ) ),
                logs.GetAllServer()
                    .TestCount( count => count.TestEquals( 6 ) )
                    .Then(
                        l => Assertion.All(
                            l.SkipLast( 1 )
                                .TestSequence(
                                [
                                    $"[Starting] At {localEndPoint} (HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                                    $"[Started] At {server.LocalEndPoint}",
                                    "[Disposing]",
                                    "[Disposed]",
                                    "[WaitingForClient]"
                                ] ),
                            l[^1]
                                .TestStartsWith(
                                    """
                                    [WaitingForClient] Encountered an error:
                                    System.ObjectDisposedException:
                                    """ ) ) ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnThrownException_WhenServerIsAttemptingToListenOnActivePort()
    {
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer( new IPEndPoint( IPAddress.Loopback, 0 ) );

        await server.StartAsync();

        await using var other = new MessageBrokerServer(
            server.LocalEndPoint,
            MessageBrokerServerOptions.Default.SetEventHandler( logs.Add ) );

        var result = await other.StartAsync();

        Assertion.All(
                result.Exception.TestType().AssignableTo<SocketException>(),
                logs.GetAllServer()
                    .TestCount( count => count.TestEquals( 4 ) )
                    .Then(
                        l => Assertion.All(
                            l[0]
                                .TestEquals(
                                    $"[Starting] At {server.LocalEndPoint} (HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))" ),
                            l[1]
                                .TestStartsWith(
                                    """
                                    [Starting] Encountered an error:
                                    System.Net.Sockets.SocketException
                                    """ ),
                            l[2].TestEquals( "[Disposing]" ),
                            l[3].TestEquals( "[Disposed]" ) ) ) )
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
                    sut.State.TestEquals( MessageBrokerServerState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task ClientListener_ShouldEmitClientRejectedEvent_WhenClientCannotBeCreated()
    {
        var exception = new Exception( "failure" );
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
        await using var sut = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerServerEventType.ClientRejected )
                            endSource.Complete();
                    } )
                .SetClientEventHandlerFactory( _ => throw exception ) );

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
                logs.GetAllServer()
                    .TestContainsContiguousSequence(
                    [
                        (l, _) => l.TestEquals(
                            $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))" ),
                        (l, _) => l.TestEquals( $"[Started] At {endPoint}" ),
                        (l, _) => l.TestEquals( "[WaitingForClient]" ),
                        (l, _) => l.TestStartsWith(
                            """
                            [ClientRejected] Encountered an error:
                            System.Exception: failure
                            """ )
                    ] ) )
            .Go();
    }
}
