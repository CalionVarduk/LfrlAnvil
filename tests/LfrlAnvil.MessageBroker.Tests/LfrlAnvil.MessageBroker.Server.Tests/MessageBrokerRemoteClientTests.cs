using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerRemoteClientTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public MessageBrokerRemoteClientTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task Start_ShouldRegisterClientAndEstablishHandshake()
    {
        var endSource = new SafeTaskCompletionSource();
        var serverLogs = new ServerEventLogger();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetLogger( serverLogs.GetLogger() )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeAcceptedResponse();
                s.SendConfirmHandshakeResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ remoteClient! ] ),
                remoteClient.TestNotNull(
                    r => Assertion.All(
                        r.TestRefEquals( server.Clients.TryGetByName( "test" ) ),
                        r.Server.TestRefEquals( server ),
                        r.Id.TestEquals( 1 ),
                        r.Name.TestEquals( "test" ),
                        r.LocalEndPoint.TestNotNull(),
                        r.RemoteEndPoint.TestNotNull(),
                        r.IsLittleEndian.TestTrue(),
                        r.MessageTimeout.TestEquals( Duration.FromSeconds( 1 ) ),
                        r.PingInterval.TestEquals( Duration.FromSeconds( 10 ) ),
                        r.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        r.ToString().TestEquals( "[1] 'test' client (Running)" ),
                        r.Publishers.Count.TestEquals( 0 ),
                        r.Publishers.GetAll().TestEmpty(),
                        r.Listeners.Count.TestEquals( 0 ),
                        r.Listeners.GetAll().TestEmpty() ) ),
                serverLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:AcceptClient] Server = {server.LocalEndPoint}, TraceId = 1 (start)",
                            $"[ClientAccepted] Server = {server.LocalEndPoint}, TraceId = 1, ClientId = 1",
                            $"[Trace:AcceptClient] Server = {server.LocalEndPoint}, TraceId = 1 (end)"
                        ] )
                    ] ),
                serverLogs.GetAllAwaitClient()
                    .TestSequence(
                    [
                        $"[AwaitClient] Server = {server.LocalEndPoint}",
                        $"[AwaitClient] Server = {server.LocalEndPoint}, EndPoint = {remoteClient?.RemoteEndPoint}",
                        $"[AwaitClient] Server = {server.LocalEndPoint}"
                    ] ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            $"[Handshaking] Client = [1], TraceId = 0, ClientName = 'test', DesiredMessageTimeout = 1 second(s), DesiredPingInterval = 10 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[HandshakeEstablished] Client = [1] 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 10 second(s)",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 18)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (ConfirmHandshakeResponse, Length = 5)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldRegisterClientAndEstablishHandshake_WithStreamDecoratorAndThrowingLogger()
    {
        NetworkStream? stream = null;
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();

                                throw new Exception( "ignored" );
                            } ) ) )
                .SetStreamDecorator(
                    (_, ns) =>
                    {
                        stream = ns;
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "foo", Duration.FromSeconds( 1.5 ), Duration.FromSeconds( 15 ) );
                s.ReadHandshakeAcceptedResponse();
                s.SendConfirmHandshakeResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        await endSource.Task;

        Assertion.All(
                stream.TestNotNull(),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ remoteClient! ] ),
                remoteClient.TestNotNull(
                    r => Assertion.All(
                        r.TestRefEquals( server.Clients.TryGetByName( "foo" ) ),
                        r.Server.TestRefEquals( server ),
                        r.Id.TestEquals( 1 ),
                        r.Name.TestEquals( "foo" ),
                        r.LocalEndPoint.TestNotNull(),
                        r.RemoteEndPoint.TestNotNull(),
                        r.IsLittleEndian.TestTrue(),
                        r.MessageTimeout.TestEquals( Duration.FromSeconds( 1.5 ) ),
                        r.PingInterval.TestEquals( Duration.FromSeconds( 15 ) ),
                        r.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        r.Publishers.Count.TestEquals( 0 ),
                        r.Publishers.GetAll().TestEmpty(),
                        r.Listeners.Count.TestEquals( 0 ),
                        r.Listeners.GetAll().TestEmpty() ) ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 17)",
                            $"[Handshaking] Client = [1], TraceId = 0, ClientName = 'foo', DesiredMessageTimeout = 1.5 second(s), DesiredPingInterval = 15 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            "[ReadPacket:Accepted] Client = [1] 'foo', TraceId = 0, Packet = (HandshakeRequest, Length = 17)",
                            "[SendPacket:Sending] Client = [1] 'foo', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sent] Client = [1] 'foo', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[ReadPacket:Received] Client = [1] 'foo', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[ReadPacket:Accepted] Client = [1] 'foo', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[HandshakeEstablished] Client = [1] 'foo', TraceId = 0, MessageTimeout = 1.5 second(s), PingInterval = 15 second(s)",
                            "[Trace:Start] Client = [1] 'foo', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 17)",
                        "[AwaitPacket] Client = [1] 'foo'",
                        "[AwaitPacket] Client = [1] 'foo', Packet = (ConfirmHandshakeResponse, Length = 5)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldRegisterManyClientsAndEstablishHandshake()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var serverLogs = new ServerEventLogger();
        var clientLogIndex = new InterlockedInt32( 0 );
        var clientLogs = new[] { new ClientEventLogger(), new ClientEventLogger() };
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetAcceptableMessageTimeout( Bounds.Create( Duration.FromSeconds( 1 ), Duration.FromSeconds( 1.5 ) ) )
                .SetAcceptablePingInterval( Bounds.Create( Duration.FromSeconds( 10 ), Duration.FromSeconds( 15 ) ) )
                .SetLogger( serverLogs.GetLogger() )
                .SetClientLoggerFactory(
                    _ =>
                    {
                        var logs = clientLogs[clientLogIndex.Increment() - 1];
                        return logs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                        endSource.Complete();
                                } ) );
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client1 = new ClientMock();
        await client1.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "foo", Duration.FromSeconds( 0.5 ), Duration.FromSeconds( 5 ) );
                s.ReadHandshakeAcceptedResponse();
                s.SendConfirmHandshakeResponse();
            } );

        using var client2 = new ClientMock();
        await client2.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "bar", Duration.FromSeconds( 2 ), Duration.FromSeconds( 20 ) );
                s.ReadHandshakeAcceptedResponse();
                s.SendConfirmHandshakeResponse();
            } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 2 ),
                server.Clients.GetAll().TestSequence( [ remoteClient1!, remoteClient2! ] ),
                remoteClient1.TestNotNull(
                    r => Assertion.All(
                        r.TestRefEquals( server.Clients.TryGetByName( "foo" ) ),
                        r.Server.TestRefEquals( server ),
                        r.Id.TestEquals( 1 ),
                        r.Name.TestEquals( "foo" ),
                        r.LocalEndPoint.TestNotNull(),
                        r.RemoteEndPoint.TestNotNull(),
                        r.IsLittleEndian.TestTrue(),
                        r.MessageTimeout.TestEquals( Duration.FromSeconds( 1 ) ),
                        r.PingInterval.TestEquals( Duration.FromSeconds( 10 ) ),
                        r.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        r.Publishers.Count.TestEquals( 0 ),
                        r.Publishers.GetAll().TestEmpty(),
                        r.Listeners.Count.TestEquals( 0 ),
                        r.Listeners.GetAll().TestEmpty() ) ),
                remoteClient2.TestNotNull(
                    r => Assertion.All(
                        r.TestRefEquals( server.Clients.TryGetByName( "bar" ) ),
                        r.Server.TestRefEquals( server ),
                        r.Id.TestEquals( 2 ),
                        r.Name.TestEquals( "bar" ),
                        r.LocalEndPoint.TestNotNull(),
                        r.RemoteEndPoint.TestNotNull(),
                        r.IsLittleEndian.TestTrue(),
                        r.MessageTimeout.TestEquals( Duration.FromSeconds( 1.5 ) ),
                        r.PingInterval.TestEquals( Duration.FromSeconds( 15 ) ),
                        r.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                        r.Publishers.Count.TestEquals( 0 ),
                        r.Publishers.GetAll().TestEmpty(),
                        r.Listeners.Count.TestEquals( 0 ),
                        r.Listeners.GetAll().TestEmpty() ) ),
                serverLogs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:AcceptClient] Server = {server.LocalEndPoint}, TraceId = 1 (start)",
                            $"[ClientAccepted] Server = {server.LocalEndPoint}, TraceId = 1, ClientId = 1",
                            $"[Trace:AcceptClient] Server = {server.LocalEndPoint}, TraceId = 1 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:AcceptClient] Server = {server.LocalEndPoint}, TraceId = 2 (start)",
                            $"[ClientAccepted] Server = {server.LocalEndPoint}, TraceId = 2, ClientId = 2",
                            $"[Trace:AcceptClient] Server = {server.LocalEndPoint}, TraceId = 2 (end)"
                        ] )
                    ] ),
                serverLogs.GetAllAwaitClient()
                    .TestSequence(
                    [
                        $"[AwaitClient] Server = {server.LocalEndPoint}",
                        $"[AwaitClient] Server = {server.LocalEndPoint}, EndPoint = {remoteClient1?.RemoteEndPoint}",
                        $"[AwaitClient] Server = {server.LocalEndPoint}",
                        $"[AwaitClient] Server = {server.LocalEndPoint}, EndPoint = {remoteClient2?.RemoteEndPoint}",
                        $"[AwaitClient] Server = {server.LocalEndPoint}"
                    ] ),
                clientLogs[0]
                    .GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 17)",
                            $"[Handshaking] Client = [1], TraceId = 0, ClientName = 'foo', DesiredMessageTimeout = 0.5 second(s), DesiredPingInterval = 5 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            "[ReadPacket:Accepted] Client = [1] 'foo', TraceId = 0, Packet = (HandshakeRequest, Length = 17)",
                            "[SendPacket:Sending] Client = [1] 'foo', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sent] Client = [1] 'foo', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[ReadPacket:Received] Client = [1] 'foo', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[ReadPacket:Accepted] Client = [1] 'foo', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[HandshakeEstablished] Client = [1] 'foo', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 10 second(s)",
                            "[Trace:Start] Client = [1] 'foo', TraceId = 0 (end)"
                        ] )
                    ] ),
                clientLogs[0]
                    .GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 17)",
                        "[AwaitPacket] Client = [1] 'foo'",
                        "[AwaitPacket] Client = [1] 'foo', Packet = (ConfirmHandshakeResponse, Length = 5)"
                    ] ),
                clientLogs[1]
                    .GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [2], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [2], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                            "[ReadPacket:Received] Client = [2], TraceId = 0, Packet = (HandshakeRequest, Length = 17)",
                            $"[Handshaking] Client = [2], TraceId = 0, ClientName = 'bar', DesiredMessageTimeout = 2 second(s), DesiredPingInterval = 20 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            "[ReadPacket:Accepted] Client = [2] 'bar', TraceId = 0, Packet = (HandshakeRequest, Length = 17)",
                            "[SendPacket:Sending] Client = [2] 'bar', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sent] Client = [2] 'bar', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[ReadPacket:Received] Client = [2] 'bar', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[ReadPacket:Accepted] Client = [2] 'bar', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[HandshakeEstablished] Client = [2] 'bar', TraceId = 0, MessageTimeout = 1.5 second(s), PingInterval = 15 second(s)",
                            "[Trace:Start] Client = [2] 'bar', TraceId = 0 (end)"
                        ] )
                    ] ),
                clientLogs[1]
                    .GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [2]",
                        "[AwaitPacket] Client = [2], Packet = (HandshakeRequest, Length = 17)",
                        "[AwaitPacket] Client = [2] 'bar'",
                        "[AwaitPacket] Client = [2] 'bar', Packet = (ConfirmHandshakeResponse, Length = 5)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenStreamDecoratorThrows()
    {
        var exception = new Exception( "invalid" );
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) )
                .SetStreamDecorator( (_, _) => ValueTask.FromException<Stream>( exception ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s => s.Connect( endPoint ) );
        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            (e, _) => e.TestEquals( "[Trace:Start] Client = [1], TraceId = 0 (start)" ),
                            (e, _) => e.TestEquals(
                                $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)" ),
                            (e, _) => e.TestStartsWith(
                                """
                                [Error] Client = [1], TraceId = 0
                                System.Exception: invalid
                                """ ),
                            (e, _) => e.TestEquals( "[Disposing] Client = [1], TraceId = 0" ),
                            (e, _) => e.TestEquals( "[Disposed] Client = [1], TraceId = 0" ),
                            (e, _) => e.TestEquals( "[Trace:Start] Client = [1], TraceId = 0 (end)" )
                        ] )
                    ] ),
                logs.GetAllAwaitPacket().TestEmpty() )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenStreamDecoratorDisposesClient()
    {
        var endSource = new SafeTaskCompletionSource<(MessageBrokerRemoteClient Client, Task DisposeTask)>();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetStreamDecorator(
                    (c, ns) =>
                    {
                        endSource.Complete( (c, c.DisconnectAsync().AsTask()) );
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s => s.Connect( endPoint ) );
        var (remoteClient, disposeTask) = await endSource.Task;
        await disposeTask;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                remoteClient.LocalEndPoint.TestNull(),
                remoteClient.RemoteEndPoint.TestNull(),
                remoteClient.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenWritingHandshakeResponseToStreamFails()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
        Stream? stream = null;

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            sendPacket: _ => stream?.Dispose(),
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) )
                .SetStreamDecorator(
                    (_, ns) =>
                    {
                        stream = ns;
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            (e, _) => e.TestEquals( "[Trace:Start] Client = [1], TraceId = 0 (start)" ),
                            (e, _) => e.TestEquals(
                                $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)" ),
                            (e, _) => e.TestEquals(
                                "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 18)" ),
                            (e, _) => e.TestEquals(
                                $"[Handshaking] Client = [1], TraceId = 0, ClientName = 'test', DesiredMessageTimeout = 1 second(s), DesiredPingInterval = 10 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}" ),
                            (e, _) => e.TestEquals(
                                "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)" ),
                            (e, _) => e.TestEquals(
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)" ),
                            (e, _) => e.TestStartsWith(
                                """
                                [Error] Client = [1] 'test', TraceId = 0
                                System.ObjectDisposedException:
                                """ ),
                            (e, _) => e.TestEquals( "[Disposing] Client = [1] 'test', TraceId = 0" ),
                            (e, _) => e.TestEquals( "[Disposed] Client = [1] 'test', TraceId = 0" ),
                            (e, _) => e.TestEquals( "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)" )
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 18)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientDisconnectsAfterEstablishingConnection()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.Dispose();
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[Disposing] Client = [1], TraceId = 0",
                            "[Disposed] Client = [1], TraceId = 0",
                            "[Trace:Start] Client = [1], TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        (e, _) => e.TestEquals( "[AwaitPacket] Client = [1]" ),
                        (e, _) => e.TestStartsWith(
                            """
                            [AwaitPacket] Client = [1]
                            System.IO.EndOfStreamException:
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientDisconnectsAfterReceivingHandshakeResponse()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeAcceptedResponse();
                s.Dispose();
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            $"[Handshaking] Client = [1], TraceId = 0, ClientName = 'test', DesiredMessageTimeout = 1 second(s), DesiredPingInterval = 10 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[Disposing] Client = [1] 'test', TraceId = 0",
                            "[Disposed] Client = [1] 'test', TraceId = 0",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        (e, _) => e.TestEquals( "[AwaitPacket] Client = [1]" ),
                        (e, _) => e.TestEquals( "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 18)" ),
                        (e, _) => e.TestEquals( "[AwaitPacket] Client = [1] 'test'" ),
                        (e, _) => e.TestStartsWith(
                            """
                            [AwaitPacket] Client = [1] 'test'
                            System.IO.EndOfStreamException:
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestEndpoint()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendConfirmHandshakeResponse();
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            """
                            [Error] Client = [1], TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid ConfirmHandshakeResponse from client [1] ''. Encountered 1 error(s):
                            1. Received unexpected server endpoint.
                            """,
                            "[Disposing] Client = [1], TraceId = 0",
                            "[Disposed] Client = [1], TraceId = 0",
                            "[Trace:Start] Client = [1], TraceId = 0 (end)",
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (ConfirmHandshakeResponse, Length = 5)",
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestPayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), payload: 8 );
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 13)",
                            """
                            [Error] Client = [1], TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid HandshakeRequest from client [1] ''. Encountered 1 error(s):
                            1. Expected header payload to be at least 9 but found 8.
                            """,
                            "[Disposing] Client = [1], TraceId = 0",
                            "[Disposed] Client = [1], TraceId = 0",
                            "[Trace:Start] Client = [1], TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 13)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestWithTooLongName()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( new string( 'x', 513 ), Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeRejectedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 527)",
                            """
                            [Error] Client = [1], TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid HandshakeRequest from client [1] ''. Encountered 1 error(s):
                            1. Expected name length to be in [1, 512] range but found 513.
                            """,
                            "[SendPacket:Sending] Client = [1], TraceId = 0, Packet = (HandshakeRejectedResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1], TraceId = 0, Packet = (HandshakeRejectedResponse, Length = 6)",
                            "[Disposing] Client = [1], TraceId = 0",
                            "[Disposed] Client = [1], TraceId = 0",
                            "[Trace:Start] Client = [1], TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 527)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestWithEmptyName()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( string.Empty, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeRejectedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 14)",
                            """
                            [Error] Client = [1], TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid HandshakeRequest from client [1] ''. Encountered 1 error(s):
                            1. Expected name length to be in [1, 512] range but found 0.
                            """,
                            "[SendPacket:Sending] Client = [1], TraceId = 0, Packet = (HandshakeRejectedResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [1], TraceId = 0, Packet = (HandshakeRejectedResponse, Length = 6)",
                            "[Disposing] Client = [1], TraceId = 0",
                            "[Disposed] Client = [1], TraceId = 0",
                            "[Trace:Start] Client = [1], TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 14)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestWithDuplicatedName()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var serverLogs = new ServerEventLogger();
        var clientLogIndex = new InterlockedInt32( 0 );
        var clientLogs = new[] { new ClientEventLogger(), new ClientEventLogger() };
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetLogger( serverLogs.GetLogger() )
                .SetClientLoggerFactory(
                    _ =>
                    {
                        var index = clientLogIndex.Increment() - 1;
                        var logs = clientLogs[index];
                        return logs.GetLogger(
                            MessageBrokerRemoteClientLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                        endSource.Complete();
                                } ) );
                    } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client1 = new ClientMock();
        await client1.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeAcceptedResponse();
                s.SendConfirmHandshakeResponse();
            } );

        using var client2 = new ClientMock();
        await client2.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeRejectedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 1 ),
                clientLogs[0]
                    .GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            $"[Handshaking] Client = [1], TraceId = 0, ClientName = 'test', DesiredMessageTimeout = 1 second(s), DesiredPingInterval = 10 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[HandshakeEstablished] Client = [1] 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 10 second(s)",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)",
                        ] )
                    ] ),
                clientLogs[0]
                    .GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 18)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (ConfirmHandshakeResponse, Length = 5)"
                    ] ),
                clientLogs[1]
                    .GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [2], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [2], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                            "[ReadPacket:Received] Client = [2], TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            $"[Handshaking] Client = [2], TraceId = 0, ClientName = 'test', DesiredMessageTimeout = 1 second(s), DesiredPingInterval = 10 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            """
                            [Error] Client = [2] 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerDuplicateClientNameException: Client with name 'test' already exists.
                            """,
                            "[SendPacket:Sending] Client = [2] 'test', TraceId = 0, Packet = (HandshakeRejectedResponse, Length = 6)",
                            "[SendPacket:Sent] Client = [2] 'test', TraceId = 0, Packet = (HandshakeRejectedResponse, Length = 6)",
                            "[Disposing] Client = [2] 'test', TraceId = 0",
                            "[Disposed] Client = [2] 'test', TraceId = 0",
                            "[Trace:Start] Client = [2] 'test', TraceId = 0 (end)",
                        ] )
                    ] ),
                clientLogs[1]
                    .GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [2]",
                        "[AwaitPacket] Client = [2], Packet = (HandshakeRequest, Length = 18)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidConfirmHandshakeResponseEndpoint()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeAcceptedResponse();
                s.Send( [ 0, 0, 0, 0, 0 ] );
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            $"[Handshaking] Client = [1], TraceId = 0, ClientName = 'test', DesiredMessageTimeout = 1 second(s), DesiredPingInterval = 10 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 0, Packet = (<unrecognized-endpoint-0>, Length = 5)",
                            """
                            [Error] Client = [1] 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid <unrecognized-endpoint-0> from client [1] 'test'. Encountered 1 error(s):
                            1. Received unexpected server endpoint.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 0",
                            "[Disposed] Client = [1] 'test', TraceId = 0",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 18)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (<unrecognized-endpoint-0>, Length = 5)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidConfirmHandshakeResponsePayload()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeAcceptedResponse();
                s.SendConfirmHandshakeResponse( payload: 1 );
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            $"[Handshaking] Client = [1], TraceId = 0, ClientName = 'test', DesiredMessageTimeout = 1 second(s), DesiredPingInterval = 10 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            """
                            [Error] Client = [1] 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid ConfirmHandshakeResponse from client [1] 'test'. Encountered 1 error(s):
                            1. Expected endianness verification payload to be 0102fdfe but found 00000001.
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 0",
                            "[Disposed] Client = [1] 'test', TraceId = 0",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)",
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 18)",
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (ConfirmHandshakeResponse, Length = 5)",
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientFailsToSendHandshakeRequestInTime()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromTicks( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s => s.Connect( endPoint ) );
        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            """
                            [Error] Client = [1], TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientRequestTimeoutException: Client [1] '' failed to send a request to the server in the specified amount of time (1 milliseconds).
                            """,
                            "[Disposing] Client = [1], TraceId = 0",
                            "[Disposed] Client = [1], TraceId = 0",
                            "[Trace:Start] Client = [1], TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [1]"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientFailsToSendFullHandshakeRequestInTime()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromMilliseconds( 100 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHeader(
                    MessageBrokerServerEndpoint.HandshakeRequest,
                    Protocol.HandshakeRequestHeader.Length,
                    reverseEndianness: BitConverter.IsLittleEndian );
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 14)",
                            """
                            [Error] Client = [1], TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientRequestTimeoutException: Client [1] '' failed to send a request to the server in the specified amount of time (100 milliseconds).
                            """,
                            "[Disposing] Client = [1], TraceId = 0",
                            "[Disposed] Client = [1], TraceId = 0",
                            "[Trace:Start] Client = [1], TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 14)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldDisposeGracefully_WhenClientFailsToSendFullHandshakeConfirmationInTime()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromMilliseconds( 100 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeAcceptedResponse();
            } );

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [1], TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1], TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[ReadPacket:Received] Client = [1], TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            $"[Handshaking] Client = [1], TraceId = 0, ClientName = 'test', DesiredMessageTimeout = 0.1 second(s), DesiredPingInterval = 10 second(s), IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (HandshakeRequest, Length = 18)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 18)",
                            """
                            [Error] Client = [1] 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientRequestTimeoutException: Client [1] 'test' failed to send a request to the server in the specified amount of time (100 milliseconds).
                            """,
                            "[Disposing] Client = [1] 'test', TraceId = 0",
                            "[Disposed] Client = [1] 'test', TraceId = 0",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [1]",
                        "[AwaitPacket] Client = [1], Packet = (HandshakeRequest, Length = 18)",
                        "[AwaitPacket] Client = [1] 'test'"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task DisposalAfterEstablishingHandshake_ShouldBeHandledCorrectly()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Dispose )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeAcceptedResponse();
                s.SendConfirmHandshakeResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        await endSource.Task;

        Assertion.All(
                server.Clients.Count.TestEquals( 0 ),
                logs.GetAll()
                    .Skip( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 1 (start)",
                            "[Disposing] Client = [1] 'test', TraceId = 1",
                            "[Disposed] Client = [1] 'test', TraceId = 1",
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task DisconnectAsync_ShouldNotDisposeExternalDelaySource()
    {
        await using var delaySource = ValueTaskDelaySource.Start();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => delaySource ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeAcceptedResponse();
                s.SendConfirmHandshakeResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        var result = await delaySource.Schedule( Duration.FromMilliseconds( 1 ) );

        result.TestEquals( ValueTaskDelayResult.Completed ).Go();
    }

    [Fact]
    public async Task DisposingExternalDelaySource_ShouldDisconnectClient()
    {
        var endSource = new SafeTaskCompletionSource();
        var delaySource = ValueTaskDelaySource.Start();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => delaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Unexpected )
                                    endSource.Complete();
                            } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask(
            s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeAcceptedResponse();
                s.SendConfirmHandshakeResponse();
            } );

        var remoteClient = server.Clients.TryGetById( 1 );
        await delaySource.DisposeAsync();
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
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

    [Fact]
    public async Task Disposal_ShouldDiscardPendingMessageNotifications()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        var pushContinuation = new SafeTaskCompletionSource( completionCount: 2 );
        var disposeContinuation = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => logs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceStart: e =>
                            {
                                if ( e.Type != MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                    return;

                                pushContinuation.Task.Wait();
                                var __ = e.Source.Client.DisconnectAsync().AsTask();
                                disposeContinuation.Task.Wait();
                            },
                            traceEnd: e =>
                            {
                                if ( e.Type is MessageBrokerRemoteClientTraceEventType.MessageNotification
                                    or MessageBrokerRemoteClientTraceEventType.Dispose )
                                    endSource.Complete();
                            },
                            disposing: _ => disposeContinuation.Complete(),
                            error: e => exception = e.Exception ) ) )
                .SetQueueLoggerFactory( _ => MessageBrokerQueueLogger.Create( messageProcessed: _ => pushContinuation.Complete() ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask(
            c =>
            {
                c.SendBindPublisherRequest( "c" );
                c.ReadPublisherBoundResponse();
                c.SendBindListenerRequest( "c", createChannelIfNotExists: false, prefetchHint: 2 );
                c.ReadListenerBoundResponse();
                c.SendPushMessage( 1, [ 1 ], confirm: false );
                c.SendPushMessage( 1, [ 2, 3 ], confirm: false );
            } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerRemoteClientMessageException>(
                        exc => Assertion.All( exc.Client.TestRefEquals( remoteClient ), exc.Queue.TestNull() ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                logs.GetAll()
                    .Skip( 3 )
                    .Where( t => t.Logs.All( e => ! e.Contains( "[Trace:PushMessage]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:MessageNotification] Client = [1] 'test', TraceId = 5 (start)",
                            "[ProcessingMessage] Client = [1] 'test', TraceId = 5, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', MessageId = 0, RetryAttempt = 0, RedeliveryAttempt = 0, Length = 1",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 5, Packet = (MessageNotification, Length = 42)",
                            """
                            [Error] Client = [1] 'test', TraceId = 5
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientDisposedException: Operation has been cancelled because remote client [1] 'test' is disposed.
                            """,
                            "[Trace:MessageNotification] Client = [1] 'test', TraceId = 5 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 6 (start)",
                            "[Disposing] Client = [1] 'test', TraceId = 6",
                            """
                            [Error] Client = [1] 'test', TraceId = 6
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientMessageException: 1 stored pending message notification(s) have been discarded due to client disposal.
                            """,
                            "[Disposed] Client = [1] 'test', TraceId = 6",
                            "[Trace:Dispose] Client = [1] 'test', TraceId = 6 (end)"
                        ] )
                    ] ) )
            .Go();
    }
}
