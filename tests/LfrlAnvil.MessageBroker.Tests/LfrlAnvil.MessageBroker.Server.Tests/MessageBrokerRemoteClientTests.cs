using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Diagnostics;
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
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s =>
        {
            s.Connect( endPoint );
            s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), 5, MemorySize.FromMegabytes( 1 ) );
            s.ReadHandshakeAcceptedResponse();
            s.SendConfirmHandshakeResponse();
        } );

        var remoteClient = server.Clients.TryGetById( 1 );
        await endSource.Task;

        Assertion.All(
                server.Connectors.Count.TestEquals( 0 ),
                server.Connectors.GetAll().TestEmpty(),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ remoteClient! ] ),
                remoteClient.TestNotNull( r => Assertion.All(
                    r.TestRefEquals( server.Clients.TryGetByName( "test" ) ),
                    r.Server.TestRefEquals( server ),
                    r.Id.TestEquals( 1 ),
                    r.Name.TestEquals( "test" ),
                    r.LocalEndPoint.TestNotNull(),
                    r.RemoteEndPoint.TestNotNull(),
                    r.IsLittleEndian.TestTrue(),
                    r.MessageTimeout.TestEquals( Duration.FromSeconds( 1 ) ),
                    r.PingInterval.TestEquals( Duration.FromSeconds( 10 ) ),
                    r.MaxBatchPacketCount.TestEquals( ( short )5 ),
                    r.MaxNetworkBatchPacketLength.TestEquals( MemorySize.FromMegabytes( 1 ) ),
                    r.SynchronizeExternalObjectNames.TestFalse(),
                    r.ClearBuffers.TestFalse(),
                    r.IsEphemeral.TestTrue(),
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
                            $"[ClientAccepted] Server = {server.LocalEndPoint}, TraceId = 1, ConnectorId = 1",
                            $"[ConnectorStarted] Server = {server.LocalEndPoint}, TraceId = 1, ConnectorId = 1",
                            $"[ReadPacket:Received] Server = {server.LocalEndPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 24)",
                            $"[HandshakeReceived] Server = {server.LocalEndPoint}, TraceId = 1, ConnectorId = 1, ClientName = 'test', DesiredMessageTimeout = 1 second(s), DesiredPingInterval = 10 second(s), DesiredBatchPacket = (MaxPacketCount = 5, MaxLength = 1048576 B), SynchronizeExternalObjectNames = False, ClearBuffers = False, IsEphemeral = True, IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            $"[ReadPacket:Accepted] Server = {server.LocalEndPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 24)",
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
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1] 'test', TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[HandshakeEstablished] Client = [1] 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 10 second(s), BatchPacket = (MaxPacketCount = 5, MaxLength = 1048576 B)",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (ConfirmHandshakeResponse, Length = 5)"
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Start_ShouldRegisterClientAndEstablishHandshake_WithThrowingLogger()
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                endSource.Complete();

                            throw new Exception( "ignored" );
                        } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s =>
        {
            s.Connect( endPoint );
            s.SendHandshake(
                "foo",
                Duration.FromSeconds( 1.5 ),
                Duration.FromSeconds( 15 ),
                maxBatchPacketCount: 1,
                synchronizeExternalObjectNames: true,
                clearBuffers: true );

            s.ReadHandshakeAcceptedResponse();
            s.SendConfirmHandshakeResponse();
        } );

        var remoteClient = server.Clients.TryGetById( 1 );
        await endSource.Task;

        Assertion.All(
                server.Connectors.Count.TestEquals( 0 ),
                server.Connectors.GetAll().TestEmpty(),
                server.Clients.Count.TestEquals( 1 ),
                server.Clients.GetAll().TestSequence( [ remoteClient! ] ),
                remoteClient.TestNotNull( r => Assertion.All(
                    r.TestRefEquals( server.Clients.TryGetByName( "foo" ) ),
                    r.Server.TestRefEquals( server ),
                    r.Id.TestEquals( 1 ),
                    r.Name.TestEquals( "foo" ),
                    r.LocalEndPoint.TestNotNull(),
                    r.RemoteEndPoint.TestNotNull(),
                    r.IsLittleEndian.TestTrue(),
                    r.MessageTimeout.TestEquals( Duration.FromSeconds( 1.5 ) ),
                    r.PingInterval.TestEquals( Duration.FromSeconds( 15 ) ),
                    r.MaxBatchPacketCount.TestEquals( ( short )0 ),
                    r.MaxNetworkBatchPacketLength.TestEquals( MemorySize.Zero ),
                    r.SynchronizeExternalObjectNames.TestTrue(),
                    r.ClearBuffers.TestTrue(),
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
                            "[Trace:Start] Client = [1] 'foo', TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1] 'foo', TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[SendPacket:Sending] Client = [1] 'foo', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[SendPacket:Sent] Client = [1] 'foo', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[ReadPacket:Received] Client = [1] 'foo', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[ReadPacket:Accepted] Client = [1] 'foo', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[HandshakeEstablished] Client = [1] 'foo', TraceId = 0, MessageTimeout = 1.5 second(s), PingInterval = 15 second(s), BatchPacket = <disabled>",
                            "[Trace:Start] Client = [1] 'foo', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
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
                .SetClientLoggerFactory( _ =>
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
        await client1.GetTask( s =>
        {
            s.Connect( endPoint );
            s.SendHandshake( "foo", Duration.FromSeconds( 0.5 ), Duration.FromSeconds( 5 ) );
            s.ReadHandshakeAcceptedResponse();
            s.SendConfirmHandshakeResponse();
        } );

        using var client2 = new ClientMock();
        await client2.GetTask( s =>
        {
            s.Connect( endPoint );
            s.SendHandshake( "bar", Duration.FromSeconds( 2 ), Duration.FromSeconds( 20 ), 10, MemorySize.FromMegabytes( 2 ) );
            s.ReadHandshakeAcceptedResponse();
            s.SendConfirmHandshakeResponse();
        } );

        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        await endSource.Task;

        Assertion.All(
                server.Connectors.Count.TestEquals( 0 ),
                server.Connectors.GetAll().TestEmpty(),
                server.Clients.Count.TestEquals( 2 ),
                server.Clients.GetAll().TestSequence( [ remoteClient1!, remoteClient2! ] ),
                remoteClient1.TestNotNull( r => Assertion.All(
                    r.TestRefEquals( server.Clients.TryGetByName( "foo" ) ),
                    r.Server.TestRefEquals( server ),
                    r.Id.TestEquals( 1 ),
                    r.Name.TestEquals( "foo" ),
                    r.LocalEndPoint.TestNotNull(),
                    r.RemoteEndPoint.TestNotNull(),
                    r.IsLittleEndian.TestTrue(),
                    r.IsEphemeral.TestTrue(),
                    r.MessageTimeout.TestEquals( Duration.FromSeconds( 1 ) ),
                    r.PingInterval.TestEquals( Duration.FromSeconds( 10 ) ),
                    r.MaxBatchPacketCount.TestEquals( ( short )0 ),
                    r.MaxNetworkBatchPacketLength.TestEquals( MemorySize.Zero ),
                    r.State.TestEquals( MessageBrokerRemoteClientState.Running ),
                    r.Publishers.Count.TestEquals( 0 ),
                    r.Publishers.GetAll().TestEmpty(),
                    r.Listeners.Count.TestEquals( 0 ),
                    r.Listeners.GetAll().TestEmpty() ) ),
                remoteClient2.TestNotNull( r => Assertion.All(
                    r.TestRefEquals( server.Clients.TryGetByName( "bar" ) ),
                    r.Server.TestRefEquals( server ),
                    r.Id.TestEquals( 2 ),
                    r.Name.TestEquals( "bar" ),
                    r.LocalEndPoint.TestNotNull(),
                    r.RemoteEndPoint.TestNotNull(),
                    r.IsLittleEndian.TestTrue(),
                    r.IsEphemeral.TestTrue(),
                    r.MessageTimeout.TestEquals( Duration.FromSeconds( 1.5 ) ),
                    r.PingInterval.TestEquals( Duration.FromSeconds( 15 ) ),
                    r.MaxBatchPacketCount.TestEquals( ( short )10 ),
                    r.MaxNetworkBatchPacketLength.TestEquals( MemorySize.FromMegabytes( 2 ) ),
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
                            $"[ClientAccepted] Server = {server.LocalEndPoint}, TraceId = 1, ConnectorId = 1",
                            $"[ConnectorStarted] Server = {server.LocalEndPoint}, TraceId = 1, ConnectorId = 1",
                            $"[ReadPacket:Received] Server = {server.LocalEndPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 23)",
                            $"[HandshakeReceived] Server = {server.LocalEndPoint}, TraceId = 1, ConnectorId = 1, ClientName = 'foo', DesiredMessageTimeout = 0.5 second(s), DesiredPingInterval = 5 second(s), DesiredBatchPacket = <disabled>, SynchronizeExternalObjectNames = False, ClearBuffers = False, IsEphemeral = True, IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            $"[ReadPacket:Accepted] Server = {server.LocalEndPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 23)",
                            $"[Trace:AcceptClient] Server = {server.LocalEndPoint}, TraceId = 1 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:AcceptClient] Server = {server.LocalEndPoint}, TraceId = 2 (start)",
                            $"[ClientAccepted] Server = {server.LocalEndPoint}, TraceId = 2, ConnectorId = 1",
                            $"[ConnectorStarted] Server = {server.LocalEndPoint}, TraceId = 2, ConnectorId = 1",
                            $"[ReadPacket:Received] Server = {server.LocalEndPoint}, TraceId = 2, ConnectorId = 1, Packet = (HandshakeRequest, Length = 23)",
                            $"[HandshakeReceived] Server = {server.LocalEndPoint}, TraceId = 2, ConnectorId = 1, ClientName = 'bar', DesiredMessageTimeout = 2 second(s), DesiredPingInterval = 20 second(s), DesiredBatchPacket = (MaxPacketCount = 10, MaxLength = 2097152 B), SynchronizeExternalObjectNames = False, ClearBuffers = False, IsEphemeral = True, IsClientLittleEndian = {BitConverter.IsLittleEndian}",
                            $"[ReadPacket:Accepted] Server = {server.LocalEndPoint}, TraceId = 2, ConnectorId = 1, Packet = (HandshakeRequest, Length = 23)",
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
                            "[Trace:Start] Client = [1] 'foo', TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1] 'foo', TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[SendPacket:Sending] Client = [1] 'foo', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[SendPacket:Sent] Client = [1] 'foo', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[ReadPacket:Received] Client = [1] 'foo', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[ReadPacket:Accepted] Client = [1] 'foo', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[HandshakeEstablished] Client = [1] 'foo', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 10 second(s), BatchPacket = <disabled>",
                            "[Trace:Start] Client = [1] 'foo', TraceId = 0 (end)"
                        ] )
                    ] ),
                clientLogs[0]
                    .GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [1] 'foo'",
                        "[AwaitPacket] Client = [1] 'foo', Packet = (ConfirmHandshakeResponse, Length = 5)"
                    ] ),
                clientLogs[1]
                    .GetAll()
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Start] Client = [2] 'bar', TraceId = 0 (start)",
                            $"[ServerTrace] Client = [2] 'bar', TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 2)",
                            "[SendPacket:Sending] Client = [2] 'bar', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[SendPacket:Sent] Client = [2] 'bar', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[ReadPacket:Received] Client = [2] 'bar', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[ReadPacket:Accepted] Client = [2] 'bar', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            "[HandshakeEstablished] Client = [2] 'bar', TraceId = 0, MessageTimeout = 1.5 second(s), PingInterval = 15 second(s), BatchPacket = (MaxPacketCount = 10, MaxLength = 2097152 B)",
                            "[Trace:Start] Client = [2] 'bar', TraceId = 0 (end)"
                        ] )
                    ] ),
                clientLogs[1]
                    .GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
                        "[AwaitPacket] Client = [2] 'bar'",
                        "[AwaitPacket] Client = [2] 'bar', Packet = (ConfirmHandshakeResponse, Length = 5)"
                    ] ) )
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
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        sendPacket: _ => stream?.Dispose(),
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                endSource.Complete();
                        } ) ) )
                .SetStreamDecorator( (_, ns) =>
                {
                    stream = ns;
                    return ValueTask.FromResult<Stream>( ns );
                } ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s =>
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
                            (e, _) => e.TestEquals( "[Trace:Start] Client = [1] 'test', TraceId = 0 (start)" ),
                            (e, _) => e.TestEquals(
                                $"[ServerTrace] Client = [1] 'test', TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)" ),
                            (e, _) => e.TestEquals(
                                "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)" ),
                            (e, _) => e.TestStartsWith(
                                """
                                [Error] Client = [1] 'test', TraceId = 0
                                System.ObjectDisposedException:
                                """ ),
                            (e, _) => e.TestEquals( "[Deactivating] Client = [1] 'test', TraceId = 0, IsAlive = False" ),
                            (e, _) => e.TestEquals( "[Deactivated] Client = [1] 'test', TraceId = 0, IsAlive = False" ),
                            (e, _) => e.TestEquals( "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)" )
                        ] )
                    ] ),
                logs.GetAllAwaitPacket().TestEmpty() )
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
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        var client = new ClientMock();
        await client.GetTask( s =>
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
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1] 'test', TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[Deactivating] Client = [1] 'test', TraceId = 0, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 0, IsAlive = False",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
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
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s =>
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
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1] 'test', TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 0, Packet = (<unrecognized-endpoint-0>, Length = 5)",
                            """
                            [Error] Client = [1] 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid <unrecognized-endpoint-0> from client [1] 'test'. Encountered 1 error(s):
                            1. Received unexpected server endpoint.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 0, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 0, IsAlive = False",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsContiguousSequence(
                    [
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
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s =>
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
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1] 'test', TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[ReadPacket:Received] Client = [1] 'test', TraceId = 0, Packet = (ConfirmHandshakeResponse, Length = 5)",
                            """
                            [Error] Client = [1] 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid ConfirmHandshakeResponse from client [1] 'test'. Encountered 1 error(s):
                            1. Expected endianness verification payload to be 0102fdfe but found 00000001.
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 0, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 0, IsAlive = False",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)",
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
                        "[AwaitPacket] Client = [1] 'test'",
                        "[AwaitPacket] Client = [1] 'test', Packet = (ConfirmHandshakeResponse, Length = 5)",
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
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s =>
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
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (start)",
                            $"[ServerTrace] Client = [1] 'test', TraceId = 0, Correlation = (Server = {server.LocalEndPoint}, TraceId = 1)",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            "[SendPacket:Sent] Client = [1] 'test', TraceId = 0, Packet = (HandshakeAcceptedResponse, Length = 32)",
                            """
                            [Error] Client = [1] 'test', TraceId = 0
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientRequestTimeoutException: Client [1] 'test' failed to send a request to the server in the specified amount of time (0.1 second(s)).
                            """,
                            "[Deactivating] Client = [1] 'test', TraceId = 0, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 0, IsAlive = False",
                            "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestSequence(
                    [
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
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Deactivate )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s =>
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
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 1 (start)",
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 1 (end)"
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
        await client.GetTask( s =>
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
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Unexpected )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();
        var endPoint = server.LocalEndPoint;

        using var client = new ClientMock();
        await client.GetTask( s =>
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
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket().Length.TestGreaterThan( 0 ) )
            .Go();
    }

    [Fact]
    public async Task Disposal_ShouldDiscardPendingMessageNotificationsAndRetries()
    {
        Exception? exception = null;
        var endSource = new SafeTaskCompletionSource( completionCount: 4 );
        var pushContinuation = new SafeTaskCompletionSource( completionCount: 4 );
        var disposeContinuation = new SafeTaskCompletionSource();
        var sentNotificationsCount = Atomic.Create( 0 );
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceStart: e =>
                        {
                            if ( e.Type != MessageBrokerRemoteClientTraceEventType.MessageNotification )
                                return;

                            if ( sentNotificationsCount.Value < 2 )
                            {
                                ++sentNotificationsCount.Value;
                                return;
                            }

                            pushContinuation.Task.Wait();
                            var __ = e.Source.Client.DisconnectAsync().AsTask();
                            disposeContinuation.Task.Wait();
                        },
                        traceEnd: e =>
                        {
                            if ( e.Type is MessageBrokerRemoteClientTraceEventType.MessageNotification
                                or MessageBrokerRemoteClientTraceEventType.Deactivate )
                                endSource.Complete();
                        },
                        deactivating: _ => disposeContinuation.Complete(),
                        error: e => exception = e.Exception ) ) )
                .SetQueueLoggerFactory( _ => MessageBrokerQueueLogger.Create( messageProcessed: _ => pushContinuation.Complete() ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "c" );
            c.ReadPublisherBoundResponse();
            c.SendBindListenerRequest(
                "c",
                createChannelIfNotExists: false,
                prefetchHint: 2,
                maxRetries: 1,
                retryDelay: Duration.FromSeconds( 10 ),
                deadLetterCapacityHint: 1,
                minDeadLetterRetention: Duration.FromMinutes( 1 ),
                minAckTimeout: Duration.FromMinutes( 10 ) );

            c.ReadListenerBoundResponse();
        } );

        var queue = remoteClient?.Queues.TryGetById( 1 );
        await client.GetTask( c =>
        {
            c.SendPushMessage( 1, [ 1 ], confirm: false );
            c.ReadMessageNotification( 1 );
            c.SendMessageNotificationNegativeAck( 1, 1, 1, 0, noRetry: true );
            c.SendPushMessage( 1, [ 2, 3 ], confirm: false );
            c.ReadMessageNotification( 1 );
            c.SendMessageNotificationNegativeAck( 1, 1, 1, 1 );
            c.SendPushMessage( 1, [ 4, 5, 6 ], confirm: false );
            c.SendPushMessage( 1, [ 7, 8, 9, 10 ], confirm: false );
        } );

        await endSource.Task;

        Assertion.All(
                exception.TestType()
                    .Exact<MessageBrokerRemoteClientException>( exc => Assertion.All( exc.Client.TestRefEquals( remoteClient ) ) ),
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                queue.TestNotNull( q => Assertion.All(
                    "queue",
                    q.State.TestEquals( MessageBrokerQueueState.Disposed ),
                    q.Messages.Pending.Count.TestEquals( 0 ),
                    q.Messages.Unacked.Count.TestEquals( 0 ),
                    q.Messages.Retries.Count.TestEquals( 0 ),
                    q.Messages.DeadLetter.Count.TestEquals( 0 ) ) ),
                logs.GetAll()
                    .Skip( 3 )
                    .Where( t => t.Logs.All( e => ! e.Contains( "[Trace:PushMessage]" ) && ! e.Contains( "[Trace:NegativeAck]" ) ) )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0, Length = 1",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 46)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 46)",
                            $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 0, Retry = 0, Redelivery = 0",
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (start)",
                            $"[ProcessingMessage] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 1, Retry = 0, Redelivery = 0, Length = 2",
                            $"[SendPacket:Sending] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 47)",
                            $"[SendPacket:Sent] Client = [1] 'test', TraceId = {t.Id}, Packet = (MessageNotification, Length = 47)",
                            $"[MessageProcessed] Client = [1] 'test', TraceId = {t.Id}, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 1, Retry = 0, Redelivery = 0",
                            $"[Trace:MessageNotification] Client = [1] 'test', TraceId = {t.Id} (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:MessageNotification] Client = [1] 'test', TraceId = 11 (start)",
                            "[ProcessingMessage] Client = [1] 'test', TraceId = 11, Sender = [1] 'test', Channel = [1] 'c', Stream = [1] 'c', Queue = [1] 'c', AckId = 1, MessageId = 2, Retry = 0, Redelivery = 0, Length = 3",
                            "[SendPacket:Sending] Client = [1] 'test', TraceId = 11, Packet = (MessageNotification, Length = 48)",
                            """
                            [Error] Client = [1] 'test', TraceId = 11
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientDeactivatedException: Operation has been cancelled because remote client [1] 'test' is disposed.
                            """,
                            "[Trace:MessageNotification] Client = [1] 'test', TraceId = 11 (end)"
                        ] ),
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 12 (start)",
                            "[Deactivating] Client = [1] 'test', TraceId = 12, IsAlive = False",
                            """
                            [Error] Client = [1] 'test', TraceId = 12
                            LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientException: 2 stored pending notification(s) have been discarded due to client disposal.
                            """,
                            "[Deactivated] Client = [1] 'test', TraceId = 12, IsAlive = False",
                            "[Trace:Deactivate] Client = [1] 'test', TraceId = 12 (end)"
                        ] )
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( MessageBrokerServerEndpoint.DeadLetterQuery, -1, ( int )MemorySize.BytesPerKilobyte * 16 )]
    [InlineData(
        MessageBrokerServerEndpoint.DeadLetterQuery,
        ( int )MemorySize.BytesPerKilobyte * 16 + 1,
        ( int )MemorySize.BytesPerKilobyte * 16 )]
    [InlineData(
        MessageBrokerServerEndpoint.PushMessage,
        -1,
        ( int )MemorySize.BytesPerKilobyte * 20 - Protocol.MessageNotificationHeader.Payload + Protocol.PushMessageHeader.Length )]
    [InlineData(
        MessageBrokerServerEndpoint.PushMessage,
        ( int )MemorySize.BytesPerKilobyte * 20 - Protocol.MessageNotificationHeader.Payload + Protocol.PushMessageHeader.Length + 1,
        ( int )MemorySize.BytesPerKilobyte * 20 - Protocol.MessageNotificationHeader.Payload + Protocol.PushMessageHeader.Length )]
    [InlineData( MessageBrokerServerEndpoint.Batch, -1, ( int )MemorySize.BytesPerKilobyte * 30 )]
    [InlineData( MessageBrokerServerEndpoint.Batch, ( int )MemorySize.BytesPerKilobyte * 30 + 1, ( int )MemorySize.BytesPerKilobyte * 30 )]
    public async Task InvalidPacketPayload_ShouldDisposeClient(MessageBrokerServerEndpoint endpoint, int payload, int expectedMax)
    {
        var endSource = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetNetworkPacketOptions(
                    MessageBrokerServerNetworkPacketOptions.Default
                        .SetMaxMessageLength( MemorySize.FromKilobytes( 20 ) )
                        .SetMaxBatchLength( MemorySize.FromKilobytes( 30 ) ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Unexpected )
                                endSource.Complete();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server, maxBatchPacketCount: 2, maxNetworkBatchPacketLength: MemorySize.FromKilobytes( 30 ) );
        await client.GetTask( c => c.SendHeader( endpoint, ( uint )payload ) );
        await endSource.Task;

        Assertion.All(
                logs.GetAll()
                    .TakeLast( 1 )
                    .TestSequence(
                    [
                        (t, _) => t.Logs.TestSequence(
                        [
                            "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (start)",
                            "[Deactivating] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Deactivated] Client = [1] 'test', TraceId = 1, IsAlive = False",
                            "[Trace:Unexpected] Client = [1] 'test', TraceId = 1 (end)"
                        ] )
                    ] ),
                logs.GetAllAwaitPacket()
                    .TestContainsSequence(
                    [
                        $"[AwaitPacket] Client = [1] 'test', Packet = ({endpoint}, Length = {Protocol.PacketHeader.Length + payload})",
                        $"""
                         [AwaitPacket] Client = [1] 'test', Packet = ({endpoint}, Length = {Protocol.PacketHeader.Length + payload})
                         LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server received an invalid {endpoint} from client [1] 'test'. Encountered 1 error(s):
                         1. Expected total packet length to be in [5, {expectedMax}] range but found {Protocol.PacketHeader.Length + payload}.
                         """
                    ] ) )
            .Go();
    }

    [Fact]
    public async Task Dispose_ShouldEndPendingResponseTraces()
    {
        var sendContinuation = new SafeTaskCompletionSource();
        var disposeContinuation = new SafeTaskCompletionSource();
        var logs = new ClientEventLogger();
        var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

        await using var server = new MessageBrokerServer(
            originalEndPoint,
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetNetworkPacketOptions(
                    MessageBrokerServerNetworkPacketOptions.Default
                        .SetMaxMessageLength( MemorySize.FromKilobytes( 20 ) )
                        .SetMaxBatchLength( MemorySize.FromKilobytes( 30 ) ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory( _ => logs.GetLogger(
                    MessageBrokerRemoteClientLogger.Create(
                        deactivating: _ => sendContinuation.Complete(),
                        listenerBound: _ => disposeContinuation.Complete(),
                        sendPacket: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientSendPacketEventType.Sending
                                && e.Packet.Endpoint == MessageBrokerClientEndpoint.PublisherBoundResponse )
                                sendContinuation.Task.Wait();
                        } ) ) ) );

        await server.StartAsync();

        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        await client.GetTask( c =>
        {
            c.SendBindPublisherRequest( "foo" );
            c.SendBindListenerRequest( "foo", false );
        } );

        await disposeContinuation.Task;
        await Task.Delay( 50 );

        var remoteClient = server.Clients.TryGetById( 1 );
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        logs.GetAll()
            .Skip( 1 )
            .TestSequence(
            [
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (start)",
                    "[ReadPacket:Received] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                    "[BindingPublisher] Client = [1] 'test', TraceId = 1, ChannelName = 'foo', IsEphemeral = True",
                    "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 1, Packet = (BindPublisherRequest, Length = 11)",
                    "[PublisherBound] Client = [1] 'test', TraceId = 1, Channel = [1] 'foo' (created), Stream = [1] 'foo' (created)",
                    "[SendPacket:Sending] Client = [1] 'test', TraceId = 1, Packet = (PublisherBoundResponse, Length = 14)",
                    """
                    [Error] Client = [1] 'test', TraceId = 1
                    LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientDeactivatedException: Operation has been cancelled because remote client [1] 'test' is disposed.
                    """,
                    "[Trace:BindPublisher] Client = [1] 'test', TraceId = 1 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:BindListener] Client = [1] 'test', TraceId = 2 (start)",
                    "[ReadPacket:Received] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 43)",
                    "[BindingListener] Client = [1] 'test', TraceId = 2, ChannelName = 'foo', PrefetchHint = 1, MaxRetries = 0, RetryDelay = 0 second(s), MaxRedeliveries = 0, MinAckTimeout = <disabled>, DeadLetter = <disabled>, IsEphemeral = True, CreateChannelIfNotExists = False",
                    "[ReadPacket:Accepted] Client = [1] 'test', TraceId = 2, Packet = (BindListenerRequest, Length = 43)",
                    "[ListenerBound] Client = [1] 'test', TraceId = 2, Channel = [1] 'foo', Queue = [1] 'foo' (created)",
                    """
                    [Error] Client = [1] 'test', TraceId = 2
                    LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientDeactivatedException: Operation has been cancelled because remote client [1] 'test' is disposed.
                    """,
                    "[Trace:BindListener] Client = [1] 'test', TraceId = 2 (end)"
                ] ),
                (t, _) => t.Logs.TestSequence(
                [
                    "[Trace:Deactivate] Client = [1] 'test', TraceId = 3 (start)",
                    "[Deactivating] Client = [1] 'test', TraceId = 3, IsAlive = False",
                    "[Deactivated] Client = [1] 'test', TraceId = 3, IsAlive = False",
                    "[Trace:Deactivate] Client = [1] 'test', TraceId = 3 (end)"
                ] )
            ] )
            .Go();
    }
}
