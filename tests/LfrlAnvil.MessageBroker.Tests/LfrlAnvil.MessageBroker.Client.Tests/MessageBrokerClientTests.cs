using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Functional;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;
using LfrlAnvil.MessageBroker.Client.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Client.Tests;

public partial class MessageBrokerClientTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public MessageBrokerClientTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public void Ctor_WithDefaultOptions_ShouldCreateCorrectClient()
    {
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = new MessageBrokerClient( remoteEndPoint, "test" );
        Assertion.All(
                sut.Id.TestEquals( 0 ),
                sut.Name.TestEquals( "test" ),
                sut.RemoteEndPoint.TestRefEquals( remoteEndPoint ),
                sut.IsServerLittleEndian.TestFalse(),
                sut.ConnectionTimeout.TestEquals( Duration.FromSeconds( 15 ) ),
                sut.MessageTimeout.TestEquals( Duration.FromSeconds( 15 ) ),
                sut.PingInterval.TestEquals( Duration.FromSeconds( 15 ) ),
                sut.ListenerDisposalTimeout.TestEquals( Duration.FromSeconds( 15 ) ),
                sut.LocalEndPoint.TestNull(),
                sut.State.TestEquals( MessageBrokerClientState.Created ),
                sut.Publishers.Count.TestEquals( 0 ),
                sut.Publishers.GetAll().TestEmpty(),
                sut.ToString().TestEquals( "[0] 'test' client (Created)" ) )
            .Go();
    }

    [Theory]
    [InlineData( 0, 1, 0, 1, 0, 1, 0, 1 )]
    [InlineData( 29999, 2, 30001, 3, 600000, 60, 750000, 75 )]
    [InlineData(
        (int.MaxValue + 1L) * 10000,
        int.MaxValue,
        (int.MaxValue + 2L) * 10000,
        int.MaxValue,
        864000010000,
        86400000,
        (int.MaxValue + 3L) * 10000,
        int.MaxValue )]
    public void Ctor_WithCustomTimeouts_ShouldCreateCorrectClient(
        long connectionTimeoutTicks,
        int expectedConnectionTimeoutMs,
        long messageTimeoutTicks,
        int expectedMessageTimeoutMs,
        long pingIntervalTicks,
        int expectedPingIntervalMs,
        long listenerDisposalTimeoutTicks,
        int expectedListenerDisposalTimeoutMs)
    {
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var sut = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromTicks( connectionTimeoutTicks ) )
                .SetDesiredMessageTimeout( Duration.FromTicks( messageTimeoutTicks ) )
                .SetDesiredPingInterval( Duration.FromTicks( pingIntervalTicks ) )
                .SetListenerDisposalTimeout( Duration.FromTicks( listenerDisposalTimeoutTicks ) ) );

        Assertion.All(
                sut.Id.TestEquals( 0 ),
                sut.Name.TestEquals( "test" ),
                sut.RemoteEndPoint.TestRefEquals( remoteEndPoint ),
                sut.IsServerLittleEndian.TestFalse(),
                sut.ConnectionTimeout.TestEquals( Duration.FromMilliseconds( expectedConnectionTimeoutMs ) ),
                sut.MessageTimeout.TestEquals( Duration.FromMilliseconds( expectedMessageTimeoutMs ) ),
                sut.PingInterval.TestEquals( Duration.FromMilliseconds( expectedPingIntervalMs ) ),
                sut.ListenerDisposalTimeout.TestEquals( Duration.FromMilliseconds( expectedListenerDisposalTimeoutMs ) ),
                sut.LocalEndPoint.TestNull(),
                sut.State.TestEquals( MessageBrokerClientState.Created ),
                sut.Publishers.Count.TestEquals( 0 ),
                sut.Publishers.GetAll().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenNameIsEmpty()
    {
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var action = Lambda.Of( () => new MessageBrokerClient( remoteEndPoint, string.Empty ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenNameIsTooLong()
    {
        var remoteEndPoint = new IPEndPoint( IPAddress.Loopback, 12345 );
        var name = new string( 'x', 513 );
        var action = Lambda.Of( () => new MessageBrokerClient( remoteEndPoint, name ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public async Task StartAsync_ShouldConnectToServerAndEstablishHandshake()
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
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
                s.ReadConfirmHandshakeResponse();
            } );

        var result = await client.StartAsync();
        await serverTask;
        await endSource.Task;
        var localEndPoint = client.LocalEndPoint;

        Assertion.All(
                result.Exception.TestNull(),
                client.Id.TestEquals( 1 ),
                client.IsServerLittleEndian.TestTrue(),
                client.MessageTimeout.TestEquals( Duration.FromSeconds( 2 ) ),
                client.PingInterval.TestEquals( Duration.FromSeconds( 10 ) ),
                client.ToString().TestEquals( "[1] 'test' client (Running)" ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            $"[HandshakeEstablished] Client = [1] 'test', TraceId = 0, MessageTimeout = 2 second(s), PingInterval = 10 second(s), IsServerLittleEndian = {BitConverter.IsLittleEndian}",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .Take( 2 )
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'",
                        "[AwaitPacket] Client = 'test', Packet = (HandshakeAcceptedResponse, Length = 18)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldConnectToServerAndEstablishHandshake_WithStreamDecoratorAndThrowingLogger()
    {
        var endSource = new SafeTaskCompletionSource();
        NetworkStream? stream = null;
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "foo",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerClientTraceEventType.Start )
                                    endSource.Complete();

                                throw new Exception( "ignored" );
                            } ) ) )
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
                s.Read( handshakeRequest );
                s.SendHandshakeAccepted( 2, Duration.FromSeconds( 1.5 ), Duration.FromSeconds( 15 ) );
                s.ReadConfirmHandshakeResponse();
            } );

        var result = await client.StartAsync();
        await serverTask;
        await endSource.Task;
        var localEndPoint = client.LocalEndPoint;

        Assertion.All(
                stream.TestNotNull(),
                result.Exception.TestNull(),
                client.Id.TestEquals( 2 ),
                client.IsServerLittleEndian.TestTrue(),
                client.MessageTimeout.TestEquals( Duration.FromSeconds( 1.5 ) ),
                client.PingInterval.TestEquals( Duration.FromSeconds( 15 ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'foo', TraceId = 0 (start)",
                            $"[Connecting] Client = 'foo', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'foo', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'foo', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'foo', TraceId = 0, Packet = (HandshakeRequest, Length = 17)",
                            "[SendPacket:Sent] Client = 'foo', TraceId = 0, Packet = (HandshakeRequest, Length = 17)",
                            "[ReadPacket:Received] Client = 'foo', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[ReadPacket:Accepted] Client = [2] 'foo', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sending] Client = [2] 'foo', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[SendPacket:Sent] Client = [2] 'foo', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            $"[HandshakeEstablished] Client = [2] 'foo', TraceId = 0, MessageTimeout = 1.5 second(s), PingInterval = 15 second(s), IsServerLittleEndian = {BitConverter.IsLittleEndian}",
                            "[Trace:Start] Client = [2] 'foo', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .Take( 2 )
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'foo'",
                        "[AwaitPacket] Client = 'foo', Packet = (HandshakeAcceptedResponse, Length = 18)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldThrowMessageBrokerClientDisposedException_WhenClientIsDisposed()
    {
        var sut = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() )
                .SetStreamDecorator( (_, _, _) => ValueTask.FromException<Stream>( exception ) ) );

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestRefEquals( exception ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            (e, _) => e.TestEquals( "[Trace:Start] Client = 'test', TraceId = 0 (start)" ),
                            (e, _) => e.TestEquals( $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}" ),
                            (e, _) => e.TestStartsWith(
                                """
                                [Error] Client = 'test', TraceId = 0
                                System.Exception: foo
                                """ ),
                            (e, _) => e.TestEquals( "[Disposing] Client = 'test', TraceId = 0" ),
                            (e, _) => e.TestEquals( "[Disposed] Client = 'test', TraceId = 0" ),
                            (e, _) => e.TestEquals( "[Trace:Start] Client = 'test', TraceId = 0 (end)" )
                        ] )
                    ] ),
                logs.GetAllAwaitPacket().TestEmpty() )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerClientDisposedException_WhenStreamDecoratorDisposesClient()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetLogger( logs.GetLogger() )
                .SetDelaySource( _sharedDelaySource )
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
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientDisposedException: Operation has been cancelled because client 'test' is disposed.
                            """,
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (start)",
                            "[Disposing] Client = 'test', TraceId = 1",
                            "[Disposed] Client = 'test', TraceId = 1",
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket().TestEmpty() )
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            connected: e =>
                            {
                                localEndPoint = e.Source.Client.LocalEndPoint;
                                stream?.Dispose();
                            } ) ) )
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
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            (e, _) => e.TestEquals( "[Trace:Start] Client = 'test', TraceId = 0 (start)" ),
                            (e, _) => e.TestEquals( $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}" ),
                            (e, _) => e.TestEquals(
                                $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}" ),
                            (e, _) => e.TestEquals(
                                "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)" ),
                            (e, _) => e.TestEquals(
                                "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)" ),
                            (e, _) => e.TestStartsWith(
                                """
                                [Error] Client = 'test', TraceId = 0
                                System.ObjectDisposedException:
                                """ ),
                            (e, _) => e.TestEquals( "[Disposing] Client = 'test', TraceId = 0" ),
                            (e, _) => e.TestEquals( "[Disposed] Client = 'test', TraceId = 0" ),
                            (e, _) => e.TestEquals( "[Trace:Start] Client = 'test', TraceId = 0 (end)" )
                        ] )
                    ] ),
                logs.GetAllAwaitPacket().TestEmpty() )
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger( MessageBrokerClientLogger.Create( connected: e => localEndPoint = e.Source.Client.LocalEndPoint ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.Dispose();
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<EndOfStreamException>(),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[Disposing] Client = 'test', TraceId = 0",
                            "[Disposed] Client = 'test', TraceId = 0",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsSequence(
                    [
                        (e, _) => e.TestStartsWith(
                            """
                            [AwaitPacket] Client = 'test'
                            System.IO.EndOfStreamException:
                            """ )
                    ] ) )
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger( MessageBrokerClientLogger.Create( connected: e => localEndPoint = e.Source.Client.LocalEndPoint ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeRejected( true, true, true );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType()
                    .Exact<MessageBrokerClientRequestException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Endpoint.TestEquals( handshakeRequest.Header.GetServerEndpoint() ) ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (HandshakeRejectedResponse, Length = 6)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientRequestException: Server rejected an invalid HandshakeRequest sent by client 'test'. Encountered 3 error(s):
                            1. Server found client's name length to be out of bounds.
                            2. Server failed to decode client's name using Unicode (UTF-8) encoding.
                            3. Client name already exists.
                            """,
                            "[Disposing] Client = 'test', TraceId = 0",
                            "[Disposed] Client = 'test', TraceId = 0",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'",
                        "[AwaitPacket] Client = 'test', Packet = (HandshakeRejectedResponse, Length = 6)"
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger( MessageBrokerClientLogger.Create( connected: e => localEndPoint = e.Source.Client.LocalEndPoint ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeRejected( true, true, true, payload: 2 );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType()
                    .Exact<MessageBrokerClientProtocolException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Endpoint.TestEquals( MessageBrokerClientEndpoint.HandshakeRejectedResponse ) ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (HandshakeRejectedResponse, Length = 7)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid HandshakeRejectedResponse from the server. Encountered 1 error(s):
                            1. Expected header payload to be 1 but found 2.
                            """,
                            "[Disposing] Client = 'test', TraceId = 0",
                            "[Disposed] Client = 'test', TraceId = 0",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'",
                        "[AwaitPacket] Client = 'test', Packet = (HandshakeRejectedResponse, Length = 7)"
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger( MessageBrokerClientLogger.Create( connected: e => localEndPoint = e.Source.Client.LocalEndPoint ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeAccepted( 0, Duration.Zero, Duration.Zero );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType()
                    .Exact<MessageBrokerClientProtocolException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Endpoint.TestEquals( MessageBrokerClientEndpoint.HandshakeAcceptedResponse ) ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid HandshakeAcceptedResponse from the server. Encountered 3 error(s):
                            1. Expected client ID to be greater than 0 but found 0.
                            2. Expected received message timeout to be in [0.001 second(s), 2147483.647 second(s)] range but found 0 second(s).
                            3. Expected received ping interval to be in [0.001 second(s), 86400 second(s)] range but found 0 second(s).
                            """,
                            "[Disposing] Client = 'test', TraceId = 0",
                            "[Disposed] Client = 'test', TraceId = 0",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'",
                        "[AwaitPacket] Client = 'test', Packet = (HandshakeAcceptedResponse, Length = 18)"
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger( MessageBrokerClientLogger.Create( connected: e => localEndPoint = e.Source.Client.LocalEndPoint ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), payload: 12 );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType()
                    .Exact<MessageBrokerClientProtocolException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Endpoint.TestEquals( MessageBrokerClientEndpoint.HandshakeAcceptedResponse ) ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 17)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid HandshakeAcceptedResponse from the server. Encountered 1 error(s):
                            1. Expected header payload to be 13 but found 12.
                            """,
                            "[Disposing] Client = 'test', TraceId = 0",
                            "[Disposed] Client = 'test', TraceId = 0",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'",
                        "[AwaitPacket] Client = 'test', Packet = (HandshakeAcceptedResponse, Length = 17)"
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger( MessageBrokerClientLogger.Create( connected: e => localEndPoint = e.Source.Client.LocalEndPoint ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.Send( [ 0, 0, 0, 0, 0 ] );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType()
                    .Exact<MessageBrokerClientProtocolException>(
                        e => Assertion.All(
                            e.Client.TestRefEquals( client ),
                            e.Endpoint.TestEquals( ( MessageBrokerClientEndpoint )0 ) ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (<unrecognized-endpoint-0>, Length = 5)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid <unrecognized-endpoint-0> from the server. Encountered 1 error(s):
                            1. Received unexpected client endpoint.
                            """,
                            "[Disposing] Client = 'test', TraceId = 0",
                            "[Disposed] Client = 'test', TraceId = 0",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'",
                        "[AwaitPacket] Client = 'test', Packet = (<unrecognized-endpoint-0>, Length = 5)"
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger( MessageBrokerClientLogger.Create( connecting: e => e.Source.Client.Dispose() ) ) ) );

        var result = await client.StartAsync();

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientDisposedException: Operation has been cancelled because client 'test' is disposed.
                            """,
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (start)",
                            "[Disposing] Client = 'test', TraceId = 1",
                            "[Disposed] Client = 'test', TraceId = 1",
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket().TestEmpty() )
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            connected: e =>
                            {
                                localEndPoint = e.Source.Client.LocalEndPoint;
                                e.Source.Client.Dispose();
                            } ) ) ) );

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientDisposedException: Operation has been cancelled because client 'test' is disposed.
                            """,
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (start)",
                            "[Disposing] Client = 'test', TraceId = 1",
                            "[Disposed] Client = 'test', TraceId = 1",
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket().TestEmpty() )
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            handshaking: e =>
                            {
                                localEndPoint = e.Source.Client.LocalEndPoint;
                                e.Source.Client.Dispose();
                            } ) ) ) );

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientDisposedException: Operation has been cancelled because client 'test' is disposed.
                            """,
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (start)",
                            "[Disposing] Client = 'test', TraceId = 1",
                            "[Disposed] Client = 'test', TraceId = 1",
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket().TestEmpty() )
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            awaitPacket: e =>
                            {
                                localEndPoint = e.Client.LocalEndPoint;
                                e.Client.Dispose();
                            } ) ) ) );

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientDisposedException: Operation has been cancelled because client 'test' is disposed.
                            """,
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (start)",
                            "[Disposing] Client = 'test', TraceId = 1",
                            "[Disposed] Client = 'test', TraceId = 1",
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'"
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromTicks( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( logs.GetLogger() ) );

        var result = await client.StartAsync();

        Assertion.All(
                result.Exception.TestType().AssignableTo<OperationCanceledException>(),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            (e, _) => e.TestEquals( "[Trace:Start] Client = 'test', TraceId = 0 (start)" ),
                            (e, _) => e.TestEquals( $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}" ),
                            (e, _) => e.TestStartsWith(
                                """
                                [Error] Client = 'test', TraceId = 0
                                System.OperationCanceledException:
                                """ ),
                            (e, _) => e.TestEquals( "[Disposing] Client = 'test', TraceId = 0" ),
                            (e, _) => e.TestEquals( "[Disposed] Client = 'test', TraceId = 0" ),
                            (e, _) => e.TestEquals( "[Trace:Start] Client = 'test', TraceId = 0 (end)" )
                        ] ),
                    ] ),
                logs.GetAllAwaitPacket().TestEmpty() )
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromTicks( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger( MessageBrokerClientLogger.Create( connected: e => localEndPoint = e.Source.Client.LocalEndPoint ) ) ) );

        var serverTask = server.GetTask( s => s.WaitForClient() );
        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().AssignableTo<OperationCanceledException>(),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 0.001 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Server failed to respond to 'test' client's HandshakeRequest in the specified amount of time (1 milliseconds).
                            """,
                            "[Disposing] Client = 'test', TraceId = 0",
                            "[Disposed] Client = 'test', TraceId = 0",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnOperationCanceledException_WhenServerFailsToRespondWithFullHandshakeAcceptedInTime()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromMilliseconds( 100 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger( MessageBrokerClientLogger.Create( connected: e => localEndPoint = e.Source.Client.LocalEndPoint ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHeader( MessageBrokerClientEndpoint.HandshakeAcceptedResponse, Protocol.HandshakeAcceptedResponse.Length );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().AssignableTo<OperationCanceledException>(),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 0.1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Server failed to respond to 'test' client's HandshakeRequest in the specified amount of time (100 milliseconds).
                            """,
                            "[Disposing] Client = 'test', TraceId = 0",
                            "[Disposed] Client = 'test', TraceId = 0",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'",
                        "[AwaitPacket] Client = 'test', Packet = (HandshakeAcceptedResponse, Length = 18)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnOperationCanceledException_WhenServerFailsToRespondWithFullHandshakeRejectedInTime()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

        await using var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromMilliseconds( 100 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger( MessageBrokerClientLogger.Create( connected: e => localEndPoint = e.Source.Client.LocalEndPoint ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHeader( MessageBrokerClientEndpoint.HandshakeRejectedResponse, Protocol.HandshakeRejectedResponse.Length );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().AssignableTo<OperationCanceledException>(),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 0.1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (HandshakeRejectedResponse, Length = 6)",
                            """
                            [Error] Client = 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientResponseTimeoutException: Server failed to respond to 'test' client's HandshakeRequest in the specified amount of time (100 milliseconds).
                            """,
                            "[Disposing] Client = 'test', TraceId = 0",
                            "[Disposed] Client = 'test', TraceId = 0",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'",
                        "[AwaitPacket] Client = 'test', Packet = (HandshakeRejectedResponse, Length = 6)"
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            readPacket: e =>
                            {
                                if ( e.Type == MessageBrokerClientReadPacketEventType.Received )
                                {
                                    localEndPoint = e.Source.Client.LocalEndPoint;
                                    e.Source.Client.Dispose();
                                }
                            } ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().AssignableTo<ObjectDisposedException>(),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (start)",
                            "[Disposing] Client = 'test', TraceId = 1",
                            "[Disposed] Client = 'test', TraceId = 1",
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        (e, _) => e.TestEquals( "[AwaitPacket] Client = 'test'" ),
                        (e, _) => e.TestEquals( "[AwaitPacket] Client = 'test', Packet = (HandshakeAcceptedResponse, Length = 18)" ),
                        (e, _) => e.TestStartsWith(
                            """
                            [AwaitPacket] Client = 'test'
                            System.ObjectDisposedException:
                            """ )
                    ] ) )
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            readPacket: e =>
                            {
                                if ( e.Type == MessageBrokerClientReadPacketEventType.Received )
                                {
                                    localEndPoint = e.Source.Client.LocalEndPoint;
                                    e.Source.Client.Dispose();
                                }
                            } ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeRejected( true, true, true );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().AssignableTo<ObjectDisposedException>(),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (HandshakeRejectedResponse, Length = 6)",
                            "[Trace:Start] Client = 'test', TraceId = 0 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (start)",
                            "[Disposing] Client = 'test', TraceId = 1",
                            "[Disposed] Client = 'test', TraceId = 1",
                            "[Trace:Dispose] Client = 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        (e, _) => e.TestEquals( "[AwaitPacket] Client = 'test'" ),
                        (e, _) => e.TestEquals( "[AwaitPacket] Client = 'test', Packet = (HandshakeRejectedResponse, Length = 6)" ),
                        (e, _) => e.TestStartsWith(
                            """
                            [AwaitPacket] Client = 'test'
                            System.ObjectDisposedException:
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnMessageBrokerClientDisposedException_WhenClientIsDisposedAfterServerHandshakeIsEstablished()
    {
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();
        EndPoint? localEndPoint = null;

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
                            handshakeEstablished: e =>
                            {
                                localEndPoint = e.Source.Client.LocalEndPoint;
                                e.Source.Client.Dispose();
                            } ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 2 ), Duration.FromSeconds( 10 ) );
            } );

        var result = await client.StartAsync();
        await serverTask;

        Assertion.All(
                result.Exception.TestType().Exact<MessageBrokerClientDisposedException>( e => e.Client.TestRefEquals( client ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = 'test', TraceId = 0 (start)",
                            $"[Connecting] Client = 'test', TraceId = 0, Server = {remoteEndPoint}",
                            $"[Connected] Client = 'test', TraceId = 0, Server = {remoteEndPoint}, LocalEndPoint = {localEndPoint}",
                            "[Handshaking] Client = 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 15 second(s)",
                            "[SendPacket:Sending] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sent] Client = 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[ReadPacket:Received] Client = 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            $"[HandshakeEstablished] Client = [1] 'test', TraceId = 0, MessageTimeout = 2 second(s), PingInterval = 10 second(s), IsServerLittleEndian = {BitConverter.IsLittleEndian}",
                            """
                            [Error] Client = [1] 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientDisposedException: Operation has been cancelled because client 'test' is disposed.
                            """,
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 1 (start)",
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 1 (end)"
                        ] ),
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = 'test'",
                        "[AwaitPacket] Client = 'test', Packet = (HandshakeAcceptedResponse, Length = 18)"
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
                                if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                    endSource.Complete();
                            } ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadConfirmHandshakeResponse();
                Thread.Sleep( 50 );
                s.SendPong( endiannessPayload: 0 );
            } );

        var result = await client.StartAsync();
        await serverTask;
        await endSource.Task;

        Assertion.All(
                result.Exception.TestNull(),
                logs.GetAll()
                    .Skip( 1 )
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
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test', Packet = (Pong, Length = 5)",
                        """
                        [AwaitPacket] Client = [1] 'test', Packet = (Pong, Length = 5)
                        LfrlAnvil.MessageBroker.Client.Exceptions.MessageBrokerClientProtocolException: Client 'test' received an invalid Pong from the server. Encountered 1 error(s):
                        1. Received unexpected client endpoint.
                        """
                    ] ) )
            .Go();
    }

    [Fact]
    public void StartAsync_ShouldThrowOperationCanceledException_WhenCancellationTokenIsCancelled()
    {
        var token = new CancellationToken( canceled: true );
        var sut = new MessageBrokerClient( new IPEndPoint( IPAddress.Loopback, 12345 ), "test" );

        var action = Lambda.Of( async () => await sut.StartAsync( token ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().AssignableTo<OperationCanceledException>(),
                    sut.State.TestEquals( MessageBrokerClientState.Created ),
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( MessageBrokerClientLogger.Create( connected: _ => cancellationSource.Cancel() ) ) );

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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    MessageBrokerClientLogger.Create(
                        sendPacket: e =>
                        {
                            if ( e.Type == MessageBrokerClientSendPacketEventType.Sent
                                && e.Packet.Endpoint == MessageBrokerServerEndpoint.HandshakeRequest )
                                cancellationSource.Cancel();
                        } ) ) );

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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    MessageBrokerClientLogger.Create(
                        readPacket: e =>
                        {
                            if ( e.Type == MessageBrokerClientReadPacketEventType.Accepted )
                                cancellationSource.Cancel();
                        } ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
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
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    MessageBrokerClientLogger.Create(
                        sendPacket: e =>
                        {
                            if ( e.Type == MessageBrokerClientSendPacketEventType.Sent
                                && e.Packet.Endpoint == MessageBrokerServerEndpoint.ConfirmHandshakeResponse )
                                cancellationSource.Cancel();
                        } ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
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
    public async Task Dispose_ShouldNotDisposeExternalDelaySource()
    {
        await using var delaySource = ValueTaskDelaySource.Start();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( delaySource ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
            } );

        await client.StartAsync();
        await serverTask;
        await client.DisposeAsync();

        var result = await delaySource.Schedule( Duration.FromMilliseconds( 1 ) );

        result.TestEquals( ValueTaskDelayResult.Completed ).Go();
    }

    [Fact]
    public async Task DisposingExternalDelaySource_ShouldDisposeClient()
    {
        var endSource = new SafeTaskCompletionSource();
        var delaySource = ValueTaskDelaySource.Start();
        var logs = new EventLogger();
        using var server = new ServerMock();
        var remoteEndPoint = server.Start();

        var client = new MessageBrokerClient(
            remoteEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySource( delaySource )
                .SetLogger(
                    logs.GetLogger(
                        MessageBrokerClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerClientTraceEventType.Unexpected )
                                    endSource.Complete();
                            } ) ) ) );

        var handshakeRequest = new Protocol.HandshakeRequest( client );
        var serverTask = server.GetTask(
            s =>
            {
                s.WaitForClient();
                s.Read( handshakeRequest );
                s.SendHandshakeAccepted( 1, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
            } );

        await client.StartAsync();
        await serverTask;

        await delaySource.DisposeAsync();
        await endSource.Task;

        Assertion.All(
                client.State.TestEquals( MessageBrokerClientState.Disposed ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (start)",
                            """
                            [Error] Client = [1] 'test', TraceId = 1
                            System.OperationCanceledException: Operation has been cancelled because external delay value task source has been disposed.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket().Length.TestGreaterThan( 0 ) )
            .Go();
    }
}
