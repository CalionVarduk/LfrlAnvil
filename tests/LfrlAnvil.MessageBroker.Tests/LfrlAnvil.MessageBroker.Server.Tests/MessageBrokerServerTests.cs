using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class MessageBrokerServerTests : TestsBase
{
    [Fact]
    public void Ctor_WithDefaultOptions_ShouldCreateCorrectServer()
    {
        var localEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = new MessageBrokerServer( () => new TimestampProvider(), localEndPoint );
        using ( new AssertionScope() )
        {
            sut.LocalEndPoint.Should().BeSameAs( localEndPoint );
            sut.HandshakeTimeout.Should().Be( Duration.FromSeconds( 15 ) );
            sut.AcceptableMessageTimeout.Should()
                .Be( Bounds.Create( Duration.FromMilliseconds( 1 ), Duration.FromMilliseconds( int.MaxValue ) ) );

            sut.AcceptablePingInterval.Should().Be( Bounds.Create( Duration.FromMilliseconds( 1 ), Duration.FromHours( 24 ) ) );
            sut.State.Should().Be( MessageBrokerServerState.Created );
            sut.Clients.Count.Should().Be( 0 );
            sut.Clients.GetAll().Should().BeEmpty();
        }
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
            () => new TimestampProvider(),
            localEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromTicks( handshakeTimeoutTicks ) )
                .SetAcceptableMessageTimeout(
                    Bounds.Create( Duration.FromTicks( minMessageTimeoutTicks ), Duration.FromTicks( maxMessageTimeoutTicks ) ) )
                .SetAcceptablePingInterval(
                    Bounds.Create( Duration.FromTicks( minPingIntervalTicks ), Duration.FromTicks( maxPingIntervalTicks ) ) ) );

        using ( new AssertionScope() )
        {
            sut.LocalEndPoint.Should().BeSameAs( localEndPoint );
            sut.HandshakeTimeout.Should().Be( Duration.FromMilliseconds( expectedHandshakeTimeoutMs ) );
            sut.AcceptableMessageTimeout.Should()
                .Be(
                    Bounds.Create(
                        Duration.FromMilliseconds( expectedMinMessageTimeoutMs ),
                        Duration.FromMilliseconds( expectedMaxMessageTimeoutMs ) ) );

            sut.AcceptablePingInterval.Should()
                .Be(
                    Bounds.Create(
                        Duration.FromMilliseconds( expectedMinPingIntervalMs ),
                        Duration.FromMilliseconds( expectedMaxPingIntervalMs ) ) );

            sut.State.Should().Be( MessageBrokerServerState.Created );
            sut.Clients.Count.Should().Be( 0 );
            sut.Clients.GetAll().Should().BeEmpty();
        }
    }

    [Fact]
    public async Task StartAsync_ShouldStartServer()
    {
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetEventHandler( logs.Add ) );

        var result = await server.StartAsync();
        var localEndPoint = server.LocalEndPoint;
        var events = logs.GetAll();

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeNull();
            server.LocalEndPoint.Address.Should().Be( IPAddress.Loopback );
            server.LocalEndPoint.Port.Should().NotBe( 0 );
            server.State.Should().Be( MessageBrokerServerState.Running );

            events.Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {localEndPoint}",
                    "[WaitingForClient]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldStartServer_WithThrowingEventHandler()
    {
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            originalEndPoint,
            MessageBrokerServerOptions.Default.SetEventHandler(
                e =>
                {
                    logs.Add( e );
                    throw new Exception( "ignored" );
                } ) );

        var result = await server.StartAsync();
        var localEndPoint = server.LocalEndPoint;
        var events = logs.GetAll();

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeNull();
            server.LocalEndPoint.Address.Should().Be( IPAddress.Loopback );
            server.LocalEndPoint.Port.Should().NotBe( 0 );
            server.State.Should().Be( MessageBrokerServerState.Running );

            events.Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {localEndPoint}",
                    "[WaitingForClient]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldThrowMessageBrokerServerDisposedException_WhenServerIsDisposed()
    {
        var sut = new MessageBrokerServer( () => new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ) );
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

        using ( new AssertionScope() )
        {
            exception.Should().BeOfType<MessageBrokerServerDisposedException>();
            ((exception as MessageBrokerServerDisposedException)?.Server).Should().BeSameAs( sut );
            sut.State.Should().Be( MessageBrokerServerState.Disposed );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldThrowMessageBrokerServerStateException_WhenServerHasAlreadyBeenStarted()
    {
        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        Exception? exception = null;
        try
        {
            await server.StartAsync();
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        using ( new AssertionScope() )
        {
            exception.Should().BeOfType<MessageBrokerServerStateException>();
            ((exception as MessageBrokerServerStateException)?.Server).Should().BeSameAs( server );
            ((exception as MessageBrokerServerStateException)?.Actual).Should().Be( MessageBrokerServerState.Running );
            ((exception as MessageBrokerServerStateException)?.Expected).Should().Be( MessageBrokerServerState.Created );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerServerDisposedException_WhenServerIsDisposedDuringStarting()
    {
        var logs = new EventLogger();
        var localEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            localEndPoint,
            MessageBrokerServerOptions.Default.SetEventHandler(
                e =>
                {
                    logs.Add( e );
                    if ( e.Type == MessageBrokerServerEventType.Starting )
                        e.Server.Dispose();
                } ) );

        var result = await server.StartAsync();

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerServerDisposedException>();
            ((result.Exception as MessageBrokerServerDisposedException)?.Server).Should().BeSameAs( server );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {localEndPoint} (HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    "[Disposing]",
                    "[Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerServerDisposedException_WhenServerIsDisposedAfterStarting()
    {
        var logs = new EventLogger();
        var localEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            localEndPoint,
            MessageBrokerServerOptions.Default.SetEventHandler(
                e =>
                {
                    logs.Add( e );
                    if ( e.Type == MessageBrokerServerEventType.Started )
                        e.Server.Dispose();
                } ) );

        var result = await server.StartAsync();

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerServerDisposedException>();
            ((result.Exception as MessageBrokerServerDisposedException)?.Server).Should().BeSameAs( server );

            logs.GetAll()
                .Take( 5 )
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {localEndPoint} (HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {server.LocalEndPoint}",
                    "[Disposing]",
                    "[Disposed]",
                    "[WaitingForClient]" );

            logs.GetAll()
                .Last()
                .Should()
                .StartWith(
                    """
                    [WaitingForClient] Encountered an error:
                    System.ObjectDisposedException:
                    """ );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnThrownException_WhenServerIsAttemptingToListenOnActivePort()
    {
        var logs = new EventLogger();

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ) );

        await server.StartAsync();

        await using var other = new MessageBrokerServer(
            () => new TimestampProvider(),
            server.LocalEndPoint,
            MessageBrokerServerOptions.Default.SetEventHandler( logs.Add ) );

        var result = await other.StartAsync();

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<SocketException>();
            logs.GetAll()
                .First()
                .Should()
                .Be(
                    $"[Starting] At {server.LocalEndPoint} (HandshakeTimeout = 15 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))" );

            logs.GetAll()
                .ElementAtOrDefault( 1 )
                .Should()
                .StartWith(
                    """
                    [Starting] Encountered an error:
                    System.Net.Sockets.SocketException
                    """ );

            logs.GetAll().TakeLast( 2 ).Should().BeSequentiallyEqualTo( "[Disposing]", "[Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var token = new CancellationToken( canceled: true );
        var sut = new MessageBrokerServer( () => new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 0 ) );

        Exception? exception = null;
        try
        {
            _ = await sut.StartAsync( token );
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        using ( new AssertionScope() )
        {
            exception.Should().BeOfType<OperationCanceledException>();
            sut.State.Should().Be( MessageBrokerServerState.Disposed );
        }
    }

    [Fact]
    public async Task ClientListener_ShouldEmitClientRejectedEvent_WhenClientCannotBeCreated()
    {
        var exception = new Exception( "failure" );
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
        await using var sut = new MessageBrokerServer(
            () => throw exception,
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerServerEventType.ClientRejected )
                            endSource.Complete();
                    } ) );

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

        using ( new AssertionScope() )
        {
            sut.Clients.Count.Should().Be( 0 );
            logs.GetAll()
                .Take( 3 )
                .Should()
                .BeSequentiallyEqualTo(
                    $"[Starting] At {originalEndPoint} (HandshakeTimeout = 1 second(s), AcceptableMessageTimeout = Bounds(0.001 second(s) : 2147483.647 second(s)), AcceptablePingInterval = Bounds(0.001 second(s) : 86400 second(s)))",
                    $"[Started] At {endPoint}",
                    "[WaitingForClient]" );

            logs.GetAll()
                .ElementAtOrDefault( 3 )
                .Should()
                .StartWith(
                    """
                    [ClientRejected] Encountered an error:
                    System.Exception: failure
                    """ );
        }
    }
}
