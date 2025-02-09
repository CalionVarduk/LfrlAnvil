using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests : TestsBase
{
    [Fact]
    public void Ctor_WithDefaultOptions_ShouldCreateCorrectClient()
    {
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = new MessageBrokerClient( new TimestampProvider(), remoteEndPoint, "test" );
        using ( new AssertionScope() )
        {
            sut.Id.Should().Be( 0 );
            sut.Name.Should().Be( "test" );
            sut.RemoteEndPoint.Should().BeSameAs( remoteEndPoint );
            sut.IsServerLittleEndian.Should().BeFalse();
            sut.ConnectionTimeout.Should().Be( Duration.FromSeconds( 15 ) );
            sut.MessageTimeout.Should().Be( Duration.FromSeconds( 15 ) );
            sut.PingInterval.Should().Be( Duration.FromSeconds( 15 ) );
            sut.LocalEndPoint.Should().BeNull();
            sut.State.Should().Be( MessageBrokerClientState.Created );
        }
    }

    [Theory]
    [InlineData( 0, 1, 0, 1, 0, 1 )]
    [InlineData( 29999, 2, 30001, 3, 600000, 60 )]
    [InlineData( (int.MaxValue + 1L) * 10000, int.MaxValue, (int.MaxValue + 2L) * 10000, int.MaxValue, 864000010000, 86400000 )]
    public void Ctor_WithCustomTimeouts_ShouldCreateCorrectClient(
        long connectionTimeoutTicks,
        int expectedConnectionTimeoutMs,
        long messageTimeoutTicks,
        int expectedMessageTimeoutMs,
        long pingIntervalTicks,
        int expectedPingIntervalMs)
    {
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromTicks( connectionTimeoutTicks ) )
                .SetDesiredMessageTimeout( Duration.FromTicks( messageTimeoutTicks ) )
                .SetDesiredPingInterval( Duration.FromTicks( pingIntervalTicks ) ) );

        using ( new AssertionScope() )
        {
            sut.Id.Should().Be( 0 );
            sut.Name.Should().Be( "test" );
            sut.RemoteEndPoint.Should().BeSameAs( remoteEndPoint );
            sut.IsServerLittleEndian.Should().BeFalse();
            sut.ConnectionTimeout.Should().Be( Duration.FromMilliseconds( expectedConnectionTimeoutMs ) );
            sut.MessageTimeout.Should().Be( Duration.FromMilliseconds( expectedMessageTimeoutMs ) );
            sut.PingInterval.Should().Be( Duration.FromMilliseconds( expectedPingIntervalMs ) );
            sut.LocalEndPoint.Should().BeNull();
            sut.State.Should().Be( MessageBrokerClientState.Created );
        }
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenNameIsEmpty()
    {
        var timestamps = new TimestampProvider();
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var action = Lambda.Of( () => new MessageBrokerClient( timestamps, remoteEndPoint, string.Empty ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenNameIsTooLong()
    {
        var timestamps = new TimestampProvider();
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var name = new string( 'x', 513 );
        var action = Lambda.Of( () => new MessageBrokerClient( timestamps, remoteEndPoint, name ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task StartAsync_ShouldConnectToServerAndEstablishHandshake()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
                s.Read( Protocol.PacketHeader.Length );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        var serverData = server.GetAllReceived();
        var localEndPoint = client.LocalEndPoint;
        var events = logs.GetAll();

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeNull();
            client.Id.Should().Be( 1 );
            client.IsServerLittleEndian.Should().BeTrue();
            client.MessageTimeout.Should().Be( Duration.FromSeconds( 2 ) );
            client.PingInterval.Should().Be( Duration.FromSeconds( 10 ) );

            AssertServerData(
                serverData,
                (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse) );

            events.Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeAcceptedResponse",
                    "['test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeAcceptedResponse (Id = 1, IsServerLittleEndian = True, MessageTimeout = 2 second(s), PingInterval = 10 second(s))",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['test'::<ROOT>] [WaitingForMessage]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldConnectToServerAndEstablishHandshake_WithStreamDecoratorAndThrowingEventHandler()
    {
        NetworkStream? stream = null;
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "foo",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        throw new Exception( "ignored" );
                    } )
                .SetStreamDecorator(
                    (_, ns, _) =>
                    {
                        stream = ns;
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 2, Duration.FromSeconds( 1.5 ), Duration.FromSeconds( 15 ) );
                s.Read( Protocol.PacketHeader.Length );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        var serverData = server.GetAllReceived();
        var localEndPoint = client.LocalEndPoint;
        var events = logs.GetAll();

        using ( new AssertionScope() )
        {
            stream.Should().NotBeNull();
            result.Exception.Should().BeNull();
            client.Id.Should().Be( 2 );
            client.IsServerLittleEndian.Should().BeTrue();
            client.MessageTimeout.Should().Be( Duration.FromSeconds( 1.5 ) );
            client.PingInterval.Should().Be( Duration.FromSeconds( 15 ) );

            AssertServerData(
                serverData,
                (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse) );

            events.Should()
                .BeSequentiallyEqualTo(
                    $"['foo'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['foo'::<ROOT>] [Connected] From {localEndPoint}",
                    "['foo'::<ROOT>] [SendingMessage] [PacketLength: 17] HandshakeRequest",
                    "['foo'::<ROOT>] [MessageSent] [PacketLength: 17] HandshakeRequest",
                    "['foo'::<ROOT>] [WaitingForMessage]",
                    "['foo'::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeAcceptedResponse",
                    "['foo'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeAcceptedResponse (Id = 2, IsServerLittleEndian = True, MessageTimeout = 1.5 second(s), PingInterval = 15 second(s))",
                    "['foo'::<ROOT>] [SendingMessage] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['foo'::<ROOT>] [MessageSent] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['foo'::<ROOT>] [WaitingForMessage]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
    {
        var sut = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
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
            exception.Should().BeOfType<MessageBrokerClientDisposedException>();
            ((exception as MessageBrokerClientDisposedException)?.Client).Should().BeSameAs( sut );
            sut.State.Should().Be( MessageBrokerClientState.Disposed );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldThrowMessageBrokerClientStateException_WhenClientHasAlreadyBeenStarted()
    {
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
                s.Read( Protocol.PacketHeader.Length );
            },
            server );

        await client.StartAsync();
        await serverTask;

        Exception? exception = null;
        try
        {
            await client.StartAsync();
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        using ( new AssertionScope() )
        {
            exception.Should().BeOfType<MessageBrokerClientStateException>();
            ((exception as MessageBrokerClientStateException)?.Client).Should().BeSameAs( client );
            ((exception as MessageBrokerClientStateException)?.Actual).Should().Be( MessageBrokerClientState.Running );
            ((exception as MessageBrokerClientStateException)?.Expected).Should().Be( MessageBrokerClientState.Created );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnThrownException_WhenStreamDecoratorThrows()
    {
        var exception = new Exception( "foo" );
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetStreamDecorator( (_, _, _) => ValueTask.FromException<Stream>( exception ) ) );

        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeSameAs( exception );
            logs.GetAll().FirstOrDefault().Should().Be( $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}" );
            logs.GetAll()
                .ElementAtOrDefault( 1 )
                .Should()
                .StartWith(
                    """
                    ['test'::<ROOT>] [Connecting] Encountered an error:
                    System.Exception: foo
                    """ );

            logs.GetAll()
                .Skip( 2 )
                .Should()
                .BeSequentiallyEqualTo(
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerClientDisposedException_WhenStreamDecoratorDisposesClient()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add )
                .SetStreamDecorator(
                    (c, ns, _) =>
                    {
                        c.Dispose();
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientDisposedException>();
            ((result.Exception as MessageBrokerClientDisposedException)?.Client).Should().BeSameAs( client );
            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]",
                    "['test'::<ROOT>] [Connecting] Operation cancelled (client disposed)" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnObjectDisposedException_WhenWritingHandshakeRequestToStreamFails()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;
        Stream? stream = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                        {
                            localEndPoint = e.Client.LocalEndPoint;
                            stream?.Dispose();
                        }
                    } )
                .SetStreamDecorator(
                    (_, ns, _) =>
                    {
                        stream = ns;
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<ObjectDisposedException>();
            logs.GetAll()
                .Take( 3 )
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest" );

            logs.GetAll()
                .ElementAtOrDefault( 3 )
                .Should()
                .StartWith(
                    """
                    ['test'::<ROOT>] [SendingMessage] [PacketLength: 18] Encountered an error:
                    System.ObjectDisposedException:
                    """ );

            logs.GetAll()
                .Skip( 4 )
                .Should()
                .BeSequentiallyEqualTo(
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnEndOfStreamException_WhenServerDisconnectsAfterReceivingHandshakeRequest()
    {
        var logs = new EventLogger();
        var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                            localEndPoint = e.Client.LocalEndPoint;
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.Dispose();
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<EndOfStreamException>();
            logs.GetAll()
                .Take( 5 )
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]" );

            logs.GetAll()
                .ElementAtOrDefault( 5 )
                .Should()
                .StartWith(
                    """
                    ['test'::<ROOT>] [WaitingForMessage] Encountered an error:
                    System.IO.EndOfStreamException:
                    """ );

            logs.GetAll()
                .Skip( 6 )
                .Should()
                .BeSequentiallyEqualTo(
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerClientRequestException_WhenServerRejectsHandshake()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                            localEndPoint = e.Client.LocalEndPoint;
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeRejected( true, true, true );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientRequestException>();
            ((result.Exception as MessageBrokerClientRequestException)?.Client).Should().BeSameAs( client );
            ((result.Exception as MessageBrokerClientRequestException)?.Payload).Should().Be( handshakeRequest.Header.Payload );
            ((result.Exception as MessageBrokerClientRequestException)?.Endpoint).Should()
                .Be( handshakeRequest.Header.GetServerEndpoint() );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] HandshakeRejectedResponse",
                    """
                    ['test'::<ROOT>] [MessageReceived] [PacketLength: 6] Encountered an error:
                    LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Message broker server rejected an invalid HandshakeRequest with payload 13 sent by client 'test'. Encountered 3 error(s):
                    1. Server found client's name length to be out of bounds.
                    2. Server failed to decode client's name using Unicode (UTF-8) encoding.
                    3. Client name already exists.
                    """,
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerClientProtocolException_WhenServerRejectsHandshakeWithInvalidPayload()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                            localEndPoint = e.Client.LocalEndPoint;
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeRejected( true, true, true, payload: 2 );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientProtocolException>();
            ((result.Exception as MessageBrokerClientProtocolException)?.Client).Should().BeSameAs( client );
            ((result.Exception as MessageBrokerClientProtocolException)?.Payload).Should().Be( 2 );
            ((result.Exception as MessageBrokerClientProtocolException)?.Endpoint).Should()
                .Be( MessageBrokerClientEndpoint.HandshakeRejectedResponse );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 7] HandshakeRejectedResponse",
                    """
                    ['test'::<ROOT>] [MessageRejected] [PacketLength: 7] Encountered an error:
                    LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid HandshakeRejectedResponse with payload 2 from the server. Encountered 1 error(s):
                    1. Expected header payload to be 1.
                    """,
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerClientProtocolException_WhenServerAcceptsHandshakeWithInvalidPacket()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                            localEndPoint = e.Client.LocalEndPoint;
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 0, Duration.Zero, Duration.Zero );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientProtocolException>();
            ((result.Exception as MessageBrokerClientProtocolException)?.Client).Should().BeSameAs( client );
            ((result.Exception as MessageBrokerClientProtocolException)?.Payload).Should().Be( Protocol.HandshakeAcceptedResponse.Length );
            ((result.Exception as MessageBrokerClientProtocolException)?.Endpoint).Should()
                .Be( MessageBrokerClientEndpoint.HandshakeAcceptedResponse );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeAcceptedResponse",
                    """
                    ['test'::<ROOT>] [MessageRejected] [PacketLength: 18] Encountered an error:
                    LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid HandshakeAcceptedResponse with payload 13 from the server. Encountered 3 error(s):
                    1. Expected client ID to be greater than 0 but found 0.
                    2. Expected received message timeout to be in [0.001 second(s), 2147483.647 second(s)] range but found 0 second(s).
                    3. Expected received ping interval to be in [0.001 second(s), 86400 second(s)] range but found 0 second(s).
                    """,
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerClientProtocolException_WhenServerAcceptsHandshakeWithInvalidPayload()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                            localEndPoint = e.Client.LocalEndPoint;
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), payload: 12 );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientProtocolException>();
            ((result.Exception as MessageBrokerClientProtocolException)?.Client).Should().BeSameAs( client );
            ((result.Exception as MessageBrokerClientProtocolException)?.Payload).Should().Be( 12 );
            ((result.Exception as MessageBrokerClientProtocolException)?.Endpoint).Should()
                .Be( MessageBrokerClientEndpoint.HandshakeAcceptedResponse );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 17] Begin handling HandshakeAcceptedResponse",
                    """
                    ['test'::<ROOT>] [MessageRejected] [PacketLength: 17] Encountered an error:
                    LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid HandshakeAcceptedResponse with payload 12 from the server. Encountered 1 error(s):
                    1. Expected header payload to be 13.
                    """,
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerClientProtocolException_WhenServerRespondsToHandshakeWithInvalidClientEndpoint()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                            localEndPoint = e.Client.LocalEndPoint;
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.Send( [ 0, 0, 0, 0, 0 ] );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientProtocolException>();
            ((result.Exception as MessageBrokerClientProtocolException)?.Client).Should().BeSameAs( client );
            ((result.Exception as MessageBrokerClientProtocolException)?.Payload).Should().Be( 0 );
            ((result.Exception as MessageBrokerClientProtocolException)?.Endpoint).Should().Be( ( MessageBrokerClientEndpoint )0 );

            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] 0",
                    """
                    ['test'::<ROOT>] [MessageRejected] [PacketLength: 5] Encountered an error:
                    LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid 0 with payload 0 from the server. Encountered 1 error(s):
                    1. Received unexpected client endpoint.
                    """,
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task
        StartAsync_ShouldReturnMessageBrokerClientDisposedException_WhenClientIsDisposedBeforeConnectionToServerAttemptIsMade()
    {
        var logs = new EventLogger();
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connecting )
                            e.Client.Dispose();
                    } ) );

        var result = await client.StartAsync();

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientDisposedException>();
            ((result.Exception as MessageBrokerClientDisposedException)?.Client).Should().BeSameAs( client );
            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]",
                    "['test'::<ROOT>] [Connecting] Operation cancelled (client disposed)" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerClientDisposedException_WhenClientIsDisposedAfterServerConnectionIsEstablished()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                        {
                            localEndPoint = e.Client.LocalEndPoint;
                            e.Client.Dispose();
                        }
                    } ) );

        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientDisposedException>();
            ((result.Exception as MessageBrokerClientDisposedException)?.Client).Should().BeSameAs( client );
            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerClientDisposedException_WhenClientIsDisposedBeforeSendingHandshakeRequest()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.SendingMessage )
                        {
                            localEndPoint ??= e.Client.LocalEndPoint;
                            e.Client.Dispose();
                        }
                    } ) );

        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientDisposedException>();
            ((result.Exception as MessageBrokerClientDisposedException)?.Client).Should().BeSameAs( client );
            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] Operation cancelled (client disposed)" );
        }
    }

    [Fact]
    public async Task
        StartAsync_ShouldReturnMessageBrokerClientDisposedException_WhenClientIsDisposedBeforeReceivingServerHandshakeResponse()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.WaitingForMessage )
                        {
                            localEndPoint ??= e.Client.LocalEndPoint;
                            e.Client.Dispose();
                        }
                    } ) );

        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientDisposedException>();
            ((result.Exception as MessageBrokerClientDisposedException)?.Client).Should().BeSameAs( client );
            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]",
                    "['test'::<ROOT>] [WaitingForMessage] Operation cancelled (client disposed)" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnOperationCanceledException_WhenClientFailsToConnectToServerInTime()
    {
        var logs = new EventLogger();
        var server = new ServerMock();
        var remoteEndPoint = server.Start();
        server.Dispose();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromTicks( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler( logs.Add ) );

        var result = await client.StartAsync();

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<OperationCanceledException>();
            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    "['test'::<ROOT>] [Connecting] Operation cancelled",
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnOperationCanceledException_WhenServerFailsToRespondToHandshakeInTime()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromTicks( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                            localEndPoint = e.Client.LocalEndPoint;
                    } ) );

        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<OperationCanceledException>();
            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [WaitingForMessage] Operation cancelled",
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnObjectDisposedException_WhenClientIsDisposedAfterReceivingHandshakeAcceptedResponse()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.MessageReceived )
                        {
                            localEndPoint ??= e.Client.LocalEndPoint;
                            e.Client.Dispose();
                        }
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<ObjectDisposedException>();
            logs.GetAll()
                .SkipLast( 1 )
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeAcceptedResponse",
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );

            logs.GetAll()
                .LastOrDefault()
                .Should()
                .StartWith(
                    """
                    ['test'::<ROOT>] [MessageReceived] [PacketLength: 18] Encountered an error:
                    System.ObjectDisposedException:
                    """ );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnObjectDisposedException_WhenClientIsDisposedAfterReceivingHandshakeRejectedResponse()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.MessageReceived )
                        {
                            localEndPoint ??= e.Client.LocalEndPoint;
                            e.Client.Dispose();
                        }
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeRejected( true, true, true );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<ObjectDisposedException>();
            logs.GetAll()
                .SkipLast( 1 )
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] HandshakeRejectedResponse",
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );

            logs.GetAll()
                .LastOrDefault()
                .Should()
                .StartWith(
                    """
                    ['test'::<ROOT>] [MessageReceived] [PacketLength: 6] Encountered an error:
                    System.ObjectDisposedException:
                    """ );
        }
    }

    [Fact]
    public async Task
        StartAsync_ShouldReturnMessageBrokerClientDisposedException_WhenClientIsDisposedAfterServerHandshakeIsEstablished()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.MessageSent
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.ConfirmHandshakeResponse )
                        {
                            localEndPoint = e.Client.LocalEndPoint;
                            e.Client.Dispose();
                        }
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeOfType<MessageBrokerClientDisposedException>();
            ((result.Exception as MessageBrokerClientDisposedException)?.Client).Should().BeSameAs( client );
            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeAcceptedResponse",
                    "['test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeAcceptedResponse (Id = 1, IsServerLittleEndian = True, MessageTimeout = 2 second(s), PingInterval = 10 second(s))",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task Dispose_ShouldBeAutomaticallyInvoked_WhenServerSendsInvalidMessageAfterEstablishingHandshake()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                            localEndPoint = e.Client.LocalEndPoint;
                        else if ( e.Type == MessageBrokerClientEventType.Disposed )
                            endSource.Complete();
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.Read( Protocol.PacketHeader.Length );
                Thread.Sleep( 50 );
                s.SendPing( endiannessPayload: 0 );
            },
            server );

        var result = await client.StartAsync();
        await serverTask;
        await endSource.Task;

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeNull();
            logs.GetAll()
                .Should()
                .BeSequentiallyEqualTo(
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeAcceptedResponse",
                    "['test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeAcceptedResponse (Id = 1, IsServerLittleEndian = True, MessageTimeout = 1 second(s), PingInterval = 10 second(s))",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 5] PingResponse",
                    """
                    ['test'::<ROOT>] [MessageRejected] [PacketLength: 5] Encountered an error:
                    LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Message broker client 'test' received an invalid PingResponse with payload 0 from the server. Encountered 1 error(s):
                    1. Received unexpected client endpoint.
                    """,
                    "['test'::<ROOT>] [Disposing]",
                    "['test'::<ROOT>] [Disposed]" );
        }
    }

    [Fact]
    public async Task StartAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var token = new CancellationToken( canceled: true );
        var sut = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );

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
            sut.State.Should().Be( MessageBrokerClientState.Disposed );
            sut.LocalEndPoint.Should().BeNull();
        }
    }

    [Fact]
    public async Task
        StartAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCanceledDirectlyAfterEstablishingConnectionToServer()
    {
        var cancellationSource = new CancellationTokenSource();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        if ( e.Type == MessageBrokerClientEventType.Connected )
                            cancellationSource.Cancel();
                    } ) );

        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
            },
            server );

        Exception? exception = null;
        try
        {
            await client.StartAsync( cancellationSource.Token );
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        await serverTask;

        using ( new AssertionScope() )
        {
            exception.Should().BeOfType<OperationCanceledException>();
            client.State.Should().Be( MessageBrokerClientState.Disposed );
            client.LocalEndPoint.Should().BeNull();
        }
    }

    [Fact]
    public async Task StartAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCanceledDirectlyAfterSendingHandshakeRequest()
    {
        var cancellationSource = new CancellationTokenSource();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        if ( e.Type == MessageBrokerClientEventType.MessageSent
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.HandshakeRequest )
                            cancellationSource.Cancel();
                    } ) );

        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
            },
            server );

        Exception? exception = null;
        try
        {
            await client.StartAsync( cancellationSource.Token );
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        await serverTask;

        using ( new AssertionScope() )
        {
            exception.Should().BeOfType<OperationCanceledException>();
            client.State.Should().Be( MessageBrokerClientState.Disposed );
            client.LocalEndPoint.Should().BeNull();
        }
    }

    [Fact]
    public async Task
        StartAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCanceledDirectlyAfterReceivingHandshakeResponse()
    {
        var cancellationSource = new CancellationTokenSource();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        if ( e.Type == MessageBrokerClientEventType.MessageAccepted )
                            cancellationSource.Cancel();
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
            },
            server );

        Exception? exception = null;
        try
        {
            await client.StartAsync( cancellationSource.Token );
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        await serverTask;

        using ( new AssertionScope() )
        {
            exception.Should().BeOfType<OperationCanceledException>();
            client.State.Should().Be( MessageBrokerClientState.Disposed );
            client.LocalEndPoint.Should().BeNull();
        }
    }

    [Fact]
    public async Task
        StartAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCanceledDirectlyAfterEstablishingServerHandshake()
    {
        var cancellationSource = new CancellationTokenSource();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetEventHandler(
                    e =>
                    {
                        if ( e.Type == MessageBrokerClientEventType.MessageSent
                            && e.GetServerEndpoint() == MessageBrokerServerEndpoint.ConfirmHandshakeResponse )
                            cancellationSource.Cancel();
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = Task.Factory.StartNew(
            o =>
            {
                var s = ( ServerMock )o!;
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
            },
            server );

        Exception? exception = null;
        try
        {
            await client.StartAsync( cancellationSource.Token );
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        await serverTask;

        using ( new AssertionScope() )
        {
            exception.Should().BeOfType<OperationCanceledException>();
            client.State.Should().Be( MessageBrokerClientState.Disposed );
            client.LocalEndPoint.Should().BeNull();
        }
    }

    private static void AssertServerData(byte[][] received, params (int Length, MessageBrokerServerEndpoint Endpoint)[] expected)
    {
        received.Should().HaveCount( expected.Length );
        for ( var i = 0; i < expected.Length; ++i )
        {
            received.ElementAtOrDefault( i ).Should().HaveCount( expected[i].Length );
            (received.ElementAtOrDefault( i )?.ElementAtOrDefault( 0 )).Should().Be( ( byte )expected[i].Endpoint );
        }
    }
}
