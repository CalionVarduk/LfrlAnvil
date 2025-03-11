using System.Diagnostics.Contracts;
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

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests : TestsBase
{
    [Fact]
    public void Ctor_WithDefaultOptions_ShouldCreateCorrectClient()
    {
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = new MessageBrokerClient( new TimestampProvider(), remoteEndPoint, "test" );
        Assertion.All(
                sut.Id.TestEquals( 0 ),
                sut.Name.TestEquals( "test" ),
                sut.RemoteEndPoint.TestRefEquals( remoteEndPoint ),
                sut.IsServerLittleEndian.TestFalse(),
                sut.ConnectionTimeout.TestEquals( Duration.FromSeconds( 15 ) ),
                sut.MessageTimeout.TestEquals( Duration.FromSeconds( 15 ) ),
                sut.PingInterval.TestEquals( Duration.FromSeconds( 15 ) ),
                sut.LocalEndPoint.TestNull(),
                sut.State.TestEquals( MessageBrokerClientState.Created ),
                sut.Publishers.Count.TestEquals( 0 ),
                sut.Publishers.GetAll().TestEmpty(),
                sut.ToString().TestEquals( "[0] 'test' client (Created)" ) )
            .Go();
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

        Assertion.All(
                sut.Id.TestEquals( 0 ),
                sut.Name.TestEquals( "test" ),
                sut.RemoteEndPoint.TestRefEquals( remoteEndPoint ),
                sut.IsServerLittleEndian.TestFalse(),
                sut.ConnectionTimeout.TestEquals( Duration.FromMilliseconds( expectedConnectionTimeoutMs ) ),
                sut.MessageTimeout.TestEquals( Duration.FromMilliseconds( expectedMessageTimeoutMs ) ),
                sut.PingInterval.TestEquals( Duration.FromMilliseconds( expectedPingIntervalMs ) ),
                sut.LocalEndPoint.TestNull(),
                sut.State.TestEquals( MessageBrokerClientState.Created ),
                sut.Publishers.Count.TestEquals( 0 ),
                sut.Publishers.GetAll().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenNameIsEmpty()
    {
        var timestamps = new TimestampProvider();
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var action = Lambda.Of( () => new MessageBrokerClient( timestamps, remoteEndPoint, string.Empty ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenNameIsTooLong()
    {
        var timestamps = new TimestampProvider();
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var name = new string( 'x', 513 );
        var action = Lambda.Of( () => new MessageBrokerClient( timestamps, remoteEndPoint, name ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public async Task StartAsync_ShouldConnectToServerAndEstablishHandshake()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
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
                .SetEventHandler(
                    e =>
                    {
                        logs.Add( e );
                        if ( e.Type == MessageBrokerClientEventType.WaitingForMessage )
                            endSource.Complete();
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
                s.ReadConfirmHandshakeResponse();
            } );

        var result = await client.StartAsync();
        await serverTask;
        await endSource.Task;

        var serverData = server.GetAllReceived();
        var localEndPoint = client.LocalEndPoint;
        var events = logs.GetAll();

        Assertion.All(
                result.Exception.TestNull(),
                client.Id.TestEquals( 1 ),
                client.IsServerLittleEndian.TestTrue(),
                client.MessageTimeout.TestEquals( Duration.FromSeconds( 2 ) ),
                client.PingInterval.TestEquals( Duration.FromSeconds( 10 ) ),
                client.ToString().TestEquals( "[1] 'test' client (Running)" ),
                AssertServerData(
                    serverData,
                    (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                    (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse) ),
                events.TestSequence(
                [
                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}", $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                    "['test'::<ROOT>] [WaitingForMessage]",
                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeAcceptedResponse",
                    "['test'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeAcceptedResponse (Id = 1, IsServerLittleEndian = True, MessageTimeout = 2 second(s), PingInterval = 10 second(s))",
                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['test'::<ROOT>] [MessageSent] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['test'::<ROOT>] [WaitingForMessage]"
                ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldConnectToServerAndEstablishHandshake_WithStreamDecoratorAndThrowingEventHandler()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
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
                        if ( e.Type == MessageBrokerClientEventType.WaitingForMessage )
                            endSource.Complete();

                        throw new Exception( "ignored" );
                    } )
                .SetStreamDecorator(
                    (_, ns, _) =>
                    {
                        stream = ns;
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 2, Duration.FromSeconds( 1.5 ), Duration.FromSeconds( 15 ) );
                s.ReadConfirmHandshakeResponse();
            } );

        var result = await client.StartAsync();
        await serverTask;
        await endSource.Task;

        var serverData = server.GetAllReceived();
        var localEndPoint = client.LocalEndPoint;
        var events = logs.GetAll();

        Assertion.All(
                stream.TestNotNull(),
                result.Exception.TestNull(),
                client.Id.TestEquals( 2 ),
                client.IsServerLittleEndian.TestTrue(),
                client.MessageTimeout.TestEquals( Duration.FromSeconds( 1.5 ) ),
                client.PingInterval.TestEquals( Duration.FromSeconds( 15 ) ),
                AssertServerData(
                    serverData,
                    (handshakeRequest.Length, MessageBrokerServerEndpoint.HandshakeRequest),
                    (Protocol.PacketHeader.Length, MessageBrokerServerEndpoint.ConfirmHandshakeResponse) ),
                events.TestSequence(
                [
                    $"['foo'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                    $"['foo'::<ROOT>] [Connected] From {localEndPoint}",
                    "['foo'::<ROOT>] [SendingMessage] [PacketLength: 17] HandshakeRequest",
                    "['foo'::<ROOT>] [MessageSent] [PacketLength: 17] HandshakeRequest",
                    "['foo'::<ROOT>] [WaitingForMessage]",
                    "['foo'::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeAcceptedResponse",
                    "['foo'::<ROOT>] [MessageAccepted] [PacketLength: 18] HandshakeAcceptedResponse (Id = 2, IsServerLittleEndian = True, MessageTimeout = 1.5 second(s), PingInterval = 15 second(s))",
                    "['foo'::<ROOT>] [SendingMessage] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['foo'::<ROOT>] [MessageSent] [PacketLength: 5] ConfirmHandshakeResponse",
                    "['foo'::<ROOT>] [WaitingForMessage]"
                ] ) )
            .Go();
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

        Assertion.All(
                exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( sut ) ),
                sut.State.TestEquals( MessageBrokerClientState.Disposed ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
                s.ReadConfirmHandshakeResponse();
            } );

        await client.StartAsync();
        await serverTask;

        var action = Lambda.Of( async () => await client.StartAsync() );

        action.Test(
                exc => exc.TestType()
                    .Exact<MessageBrokerClientStateException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Actual.TestEquals( MessageBrokerClientState.Running ),
                            e.Expected.TestEquals( MessageBrokerClientState.Created ) ) ) )
            .Go();
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

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestRefEquals( exception ),
                logs.GetAll()
                    .TestCount( count => count.TestEquals( 4 ) )
                    .Then(
                        l => Assertion.All(
                            l[0].TestEquals( $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}" ),
                            l[1]
                                .TestStartsWith(
                                    """
                                    ['test'::<ROOT>] [Connecting] Encountered an error:
                                    System.Exception: foo
                                    """ ),
                            l[2].TestEquals( "['test'::<ROOT>] [Disposing]" ),
                            l[3].TestEquals( "['test'::<ROOT>] [Disposed]" ) ) ) )
            .Go();
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

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                        "['test'::<ROOT>] [Disposing]",
                        "['test'::<ROOT>] [Disposed]",
                        "['test'::<ROOT>] [Connecting] Operation cancelled (client disposed)"
                    ] ) )
            .Go();
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

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<ObjectDisposedException>(),
                logs.GetAll()
                    .TestCount( count => count.TestEquals( 6 ) )
                    .Then(
                        l => Assertion.All(
                            l[0].TestEquals( $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}" ),
                            l[1].TestEquals( $"['test'::<ROOT>] [Connected] From {localEndPoint}" ),
                            l[2].TestEquals( "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest" ),
                            l[3]
                                .TestStartsWith(
                                    """
                                    ['test'::<ROOT>] [SendingMessage] [PacketLength: 18] Encountered an error:
                                    System.ObjectDisposedException:
                                    """ ),
                            l[4].TestEquals( "['test'::<ROOT>] [Disposing]" ),
                            l[5].TestEquals( "['test'::<ROOT>] [Disposed]" ) ) ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.Dispose();
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<EndOfStreamException>(),
                logs.GetAll()
                    .TestCount( count => count.TestEquals( 8 ) )
                    .Then(
                        l => Assertion.All(
                            l[0].TestEquals( $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}" ),
                            l[1].TestEquals( $"['test'::<ROOT>] [Connected] From {localEndPoint}" ),
                            l[2].TestEquals( "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest" ),
                            l[3].TestEquals( "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest" ),
                            l[4].TestEquals( "['test'::<ROOT>] [WaitingForMessage]" ),
                            l[5]
                                .TestStartsWith(
                                    """
                                    ['test'::<ROOT>] [WaitingForMessage] Encountered an error:
                                    System.IO.EndOfStreamException:
                                    """ ),
                            l[6].TestEquals( "['test'::<ROOT>] [Disposing]" ),
                            l[7].TestEquals( "['test'::<ROOT>] [Disposed]" ) ) ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeRejected( true, true, true );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType()
                    .Exact<MessageBrokerClientRequestException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Payload.TestEquals( handshakeRequest.Header.Payload ),
                            e.Endpoint.TestEquals( handshakeRequest.Header.GetServerEndpoint() ) ) ),
                logs.GetAll()
                    .TestSequence(
                    [
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
                        "['test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeRejected( true, true, true, payload: 2 );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType()
                    .Exact<MessageBrokerClientProtocolException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Payload.TestEquals( 2U ),
                            e.Endpoint.TestEquals( MessageBrokerClientEndpoint.HandshakeRejectedResponse ) ) ),
                logs.GetAll()
                    .TestSequence(
                    [
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
                        "['test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 0, Duration.Zero, Duration.Zero );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType()
                    .Exact<MessageBrokerClientProtocolException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Payload.TestEquals( ( uint )Protocol.HandshakeAcceptedResponse.Length ),
                            e.Endpoint.TestEquals( MessageBrokerClientEndpoint.HandshakeAcceptedResponse ) ) ),
                logs.GetAll()
                    .TestSequence(
                    [
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
                        "['test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), payload: 12 );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType()
                    .Exact<MessageBrokerClientProtocolException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Payload.TestEquals( 12U ),
                            e.Endpoint.TestEquals( MessageBrokerClientEndpoint.HandshakeAcceptedResponse ) ) ),
                logs.GetAll()
                    .TestSequence(
                    [
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
                        "['test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.Send( [ 0, 0, 0, 0, 0 ] );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType()
                    .Exact<MessageBrokerClientProtocolException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Payload.TestEquals( 0U ),
                            e.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ) ) ),
                logs.GetAll()
                    .TestSequence(
                    [
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
                        "['test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
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

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                        "['test'::<ROOT>] [Disposing]",
                        "['test'::<ROOT>] [Disposed]",
                        "['test'::<ROOT>] [Connecting] Operation cancelled (client disposed)"
                    ] ) )
            .Go();
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

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                        $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                        "['test'::<ROOT>] [Disposing]",
                        "['test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
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

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                        $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                        "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                        "['test'::<ROOT>] [Disposing]",
                        "['test'::<ROOT>] [Disposed]",
                        "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] Operation cancelled (client disposed)"
                    ] ) )
            .Go();
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

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                        $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                        "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                        "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                        "['test'::<ROOT>] [WaitingForMessage]",
                        "['test'::<ROOT>] [Disposing]",
                        "['test'::<ROOT>] [Disposed]",
                        "['test'::<ROOT>] [WaitingForMessage] Operation cancelled (client disposed)"
                    ] ) )
            .Go();
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

        Assertion.All(
                result.Exception.TestType().AssignableTo<OperationCanceledException>(),
                logs.GetAll()
                    .TestSequence(
                    [
                        $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                        "['test'::<ROOT>] [Connecting] Operation cancelled",
                        "['test'::<ROOT>] [Disposing]",
                        "['test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
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

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().AssignableTo<OperationCanceledException>(),
                logs.GetAll()
                    .TestSequence(
                    [
                        $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                        $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                        "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                        "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                        "['test'::<ROOT>] [WaitingForMessage]",
                        "['test'::<ROOT>] [WaitingForMessage] Operation cancelled",
                        "['test'::<ROOT>] [Disposing]",
                        "['test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().AssignableTo<ObjectDisposedException>(),
                logs.GetAll()
                    .TestCount( count => count.TestEquals( 9 ) )
                    .Then(
                        l => Assertion.All(
                            l.SkipLast( 1 )
                                .TestSequence(
                                [
                                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                                    "['test'::<ROOT>] [WaitingForMessage]",
                                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 18] Begin handling HandshakeAcceptedResponse",
                                    "['test'::<ROOT>] [Disposing]",
                                    "['test'::<ROOT>] [Disposed]"
                                ] ),
                            l[^1]
                                .TestStartsWith(
                                    """
                                    ['test'::<ROOT>] [MessageReceived] [PacketLength: 18] Encountered an error:
                                    System.ObjectDisposedException:
                                    """ ) ) ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeRejected( true, true, true );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().AssignableTo<ObjectDisposedException>(),
                logs.GetAll()
                    .TestCount( count => count.TestEquals( 9 ) )
                    .Then(
                        l => Assertion.All(
                            l.SkipLast( 1 )
                                .TestSequence(
                                [
                                    $"['test'::<ROOT>] [Connecting] To server at {remoteEndPoint}",
                                    $"['test'::<ROOT>] [Connected] From {localEndPoint}",
                                    "['test'::<ROOT>] [SendingMessage] [PacketLength: 18] HandshakeRequest",
                                    "['test'::<ROOT>] [MessageSent] [PacketLength: 18] HandshakeRequest",
                                    "['test'::<ROOT>] [WaitingForMessage]",
                                    "['test'::<ROOT>] [MessageReceived] [PacketLength: 6] HandshakeRejectedResponse",
                                    "['test'::<ROOT>] [Disposing]",
                                    "['test'::<ROOT>] [Disposed]"
                                ] ),
                            l[^1]
                                .TestStartsWith(
                                    """
                                    ['test'::<ROOT>] [MessageReceived] [PacketLength: 6] Encountered an error:
                                    System.ObjectDisposedException:
                                    """ ) ) ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
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
                        "['test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadConfirmHandshakeResponse();
                Thread.Sleep( 50 );
                s.SendPing( endiannessPayload: 0 );
            } );

        var result = await client.StartAsync();
        await serverTask;
        await endSource.Task;

        Assertion.All(
                result.Exception.TestNull(),
                logs.GetAll()
                    .TestSequence(
                    [
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
                        "['test'::<ROOT>] [Disposed]"
                    ] ) )
            .Go();
    }

    [Fact]
    public void StartAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var token = new CancellationToken( canceled: true );
        var sut = new MessageBrokerClient( new TimestampProvider(), new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );

        var action = Lambda.Of( async () => await sut.StartAsync( token ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().AssignableTo<OperationCanceledException>(),
                    sut.State.TestEquals( MessageBrokerClientState.Disposed ),
                    sut.LocalEndPoint.TestNull() ) )
            .Go();
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

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var action = Lambda.Of( async () => await client.StartAsync( cancellationSource.Token ) );
        var assertion = action.Test(
                exc => Assertion.All(
                    exc.TestType().AssignableTo<OperationCanceledException>(),
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    client.LocalEndPoint.TestNull() ) )
            .Invoke();

        await serverTask;

        assertion.Go();
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

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var action = Lambda.Of( async () => await client.StartAsync( cancellationSource.Token ) );
        var assertion = action.Test(
                exc => Assertion.All(
                    exc.TestType().AssignableTo<OperationCanceledException>(),
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    client.LocalEndPoint.TestNull() ) )
            .Invoke();

        await serverTask;

        assertion.Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
            } );

        var action = Lambda.Of( async () => await client.StartAsync( cancellationSource.Token ) );
        var assertion = action.Test(
                exc => Assertion.All(
                    exc.TestType().AssignableTo<OperationCanceledException>(),
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    client.LocalEndPoint.TestNull() ) )
            .Invoke();

        await serverTask;

        assertion.Go();
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
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest.Length );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
            } );

        var action = Lambda.Of( async () => await client.StartAsync( cancellationSource.Token ) );
        var assertion = action.Test(
                exc => Assertion.All(
                    exc.TestType().AssignableTo<OperationCanceledException>(),
                    client.State.TestEquals( MessageBrokerClientState.Disposed ),
                    client.LocalEndPoint.TestNull() ) )
            .Invoke();

        await serverTask;

        assertion.Go();
    }

    [Pure]
    private static Assertion AssertServerData(byte[][] received, params (int Length, MessageBrokerServerEndpoint Endpoint)[] expected)
    {
        return received.TestCount( count => count.TestEquals( expected.Length ) )
            .Then(
                r => r.TestAll(
                    (e, i) => Assertion.All(
                        "element",
                        e.Length.TestEquals( expected[i].Length ),
                        e.ElementAtOrDefault( 0 ).TestEquals( ( byte )expected[i].Endpoint ) ) ) );
    }
}
