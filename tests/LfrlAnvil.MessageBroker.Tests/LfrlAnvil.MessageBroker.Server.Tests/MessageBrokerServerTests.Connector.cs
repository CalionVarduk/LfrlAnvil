using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public partial class MessageBrokerServerTests
{
    public class Connector : TestsBase
    {
        [Fact]
        public async Task Start_ShouldDiscardClient_WhenClientCannotBeCreated()
        {
            var exception = new Exception( "failure" );
            var endSource = new SafeTaskCompletionSource( completionCount: 3 );
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );
            await using var sut = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                },
                                awaitClient: e =>
                                {
                                    if ( e.EndPoint is null )
                                        endSource.Complete();
                                } ) ) )
                    .SetClientLoggerFactory( _ => throw exception ) );

            await sut.StartAsync();
            var endPoint = sut.LocalEndPoint;

            var client = new ClientMock();
            var clientTask = client.GetTask( c =>
            {
                c.Connect( endPoint );
                c.SendHandshake( "foo", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
            } );

            await clientTask;
            await endSource.Task;

            Assertion.All(
                    sut.Connectors.Count.TestEquals( 0 ),
                    sut.Clients.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                (e, _) => e.TestEquals( $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)" ),
                                (e, _) => e.TestEquals( $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1" ),
                                (e, _) => e.TestEquals( $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1" ),
                                (e, _) => e.TestEquals(
                                    $"[ReadPacket:Received] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 23)" ),
                                (e, _) => e.TestEquals(
                                    $"[HandshakeReceived] Server = {endPoint}, TraceId = 1, ConnectorId = 1, ClientName = 'foo', DesiredMessageTimeout = 1 second(s), DesiredPingInterval = 10 second(s), DesiredBatchPacket = <disabled>, SynchronizeExternalObjectNames = False, ClearBuffers = False, IsEphemeral = True, IsClientLittleEndian = {BitConverter.IsLittleEndian}" ),
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

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenStreamDecoratorThrows()
        {
            var exception = new Exception( "invalid" );
            var endSource = new SafeTaskCompletionSource();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                } ) ) )
                    .SetStreamDecorator( (_, _) => ValueTask.FromException<Stream>( exception ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s => s.Connect( endPoint ) );
            await endSource.Task;

            Assertion.All(
                    server.Connectors.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                (e, _) => e.TestEquals( $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)" ),
                                (e, _) => e.TestEquals( $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1" ),
                                (e, _) => e.TestEquals( $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1" ),
                                (e, _) => e.TestStartsWith(
                                    $"""
                                     [Error] Server = {endPoint}, TraceId = 1
                                     System.Exception: invalid
                                     """ ),
                                (e, _) => e.TestEquals( $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)" )
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenClientDisconnectsAfterEstablishingConnection()
        {
            var endSource = new SafeTaskCompletionSource();
            var startContinuation = new SafeTaskCompletionSource();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                },
                                connectorStarted: _ => startContinuation.Complete() ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            var client = new ClientMock();
            await client.GetTask( c => c.Connect( endPoint ) );

            await startContinuation.Task;
            var connectorCount = server.Connectors.Count;
            var connectors = server.Connectors.GetAll();
            var connector = server.Connectors.TryGetById( 1 );
            var localEndPoint = connector?.LocalEndPoint;
            var remoteEndPoint = connector?.RemoteEndPoint;
            var state = connector?.State ?? MessageBrokerRemoteClientConnectorState.Failed;

            await client.GetTask( c => c.Dispose() );
            await endSource.Task;

            Assertion.All(
                    connectorCount.TestEquals( 1 ),
                    connectors.Select( c => c.Id ).TestSequence( [ 1 ] ),
                    localEndPoint.TestNotNull(),
                    remoteEndPoint.TestNotNull(),
                    state.TestInRange(
                        MessageBrokerRemoteClientConnectorState.Created,
                        MessageBrokerRemoteClientConnectorState.Handshaking ),
                    connector.TestNotNull( c => Assertion.All(
                        "connector",
                        c.Id.TestEquals( 1 ),
                        c.Server.TestRefEquals( server ),
                        c.State.TestEquals( MessageBrokerRemoteClientConnectorState.Failed ),
                        c.LocalEndPoint.TestNull(),
                        c.RemoteEndPoint.TestNull(),
                        c.ToString().TestEquals( "[1] connector (Failed)" ) ) ),
                    server.Connectors.Count.TestEquals( 0 ),
                    server.Clients.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                (e, _) => e.TestEquals( $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)" ),
                                (e, _) => e.TestEquals( $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1" ),
                                (e, _) => e.TestEquals( $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1" ),
                                (e, _) => e.TestStartsWith(
                                    $"""
                                     [Error] Server = {endPoint}, TraceId = 1
                                     System.IO.EndOfStreamException:
                                     """ ),
                                (e, _) => e.TestEquals( $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)" )
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenClientFailsToSendFullHandshakeRequestInTime()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromMilliseconds( 100 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s =>
            {
                s.Connect( endPoint );
                s.SendHeader(
                    MessageBrokerServerEndpoint.HandshakeRequest,
                    Protocol.HandshakeRequestHeader.Length,
                    reverseEndianness: BitConverter.IsLittleEndian );
            } );

            await endSource.Task;

            Assertion.All(
                    server.Connectors.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)",
                                $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ReadPacket:Received] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 20)",
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientRequestTimeoutException: Client failed to send a request to the server-side connector [1] in the specified amount of time (0.1 second(s)).
                                 """,
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenClientFailsToSendHandshakeRequestInTime()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromMilliseconds( 15 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s => s.Connect( endPoint ) );
            await endSource.Task;

            Assertion.All(
                    server.Connectors.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)",
                                $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientRequestTimeoutException: Client failed to send a request to the server-side connector [1] in the specified amount of time (0.015 second(s)).
                                 """,
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestEndpoint()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s =>
            {
                s.Connect( endPoint );
                s.SendConfirmHandshakeResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    server.Connectors.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)",
                                $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ReadPacket:Received] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (ConfirmHandshakeResponse, Length = 5)",
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server-side connector [1] received an invalid ConfirmHandshakeResponse from client. Encountered 1 error(s):
                                 1. Received unexpected server endpoint.
                                 """,
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestPayload()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), payload: 14 );
            } );

            await endSource.Task;

            Assertion.All(
                    server.Connectors.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)",
                                $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ReadPacket:Received] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 19)",
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server-side connector [1] received an invalid HandshakeRequest from client. Encountered 1 error(s):
                                 1. Expected header payload to be at least 15 but found 14.
                                 """,
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenClientSendsTooLargeHandshakeRequest()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s =>
            {
                s.Connect( endPoint );
                s.SendHandshake(
                    "test",
                    Duration.FromSeconds( 1 ),
                    Duration.FromSeconds( 10 ),
                    payload: ( uint )MemorySize.BytesPerKilobyte * 16 - Protocol.PacketHeader.Length + 1 );
            } );

            await endSource.Task;

            Assertion.All(
                    server.Connectors.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)",
                                $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ReadPacket:Received] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 16385)",
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server-side connector [1] received an invalid HandshakeRequest from client. Encountered 1 error(s):
                                 1. Expected total packet length to be in [5, 16384] range but found 16385.
                                 """,
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestWithTooLongName()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( new string( 'x', 513 ), Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeRejectedResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    server.Connectors.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)",
                                $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ReadPacket:Received] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 533)",
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server-side connector [1] received an invalid HandshakeRequest from client. Encountered 1 error(s):
                                 1. Expected client name length to be in [1, 512] range but found 513.
                                 """,
                                $"[SendPacket:Sending] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRejectedResponse, Length = 6)",
                                $"[SendPacket:Sent] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRejectedResponse, Length = 6)",
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestWithEmptyName()
        {
            var endSource = new SafeTaskCompletionSource();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                } ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( string.Empty, Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeRejectedResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    server.Connectors.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)",
                                $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ReadPacket:Received] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 20)",
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerProtocolException: Server-side connector [1] received an invalid HandshakeRequest from client. Encountered 1 error(s):
                                 1. Expected client name length to be in [1, 512] range but found 0.
                                 """,
                                $"[SendPacket:Sending] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRejectedResponse, Length = 6)",
                                $"[SendPacket:Sent] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRejectedResponse, Length = 6)",
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenClientSendsInvalidHandshakeRequestWithDuplicatedName()
        {
            Exception? exception = null;
            var endSource = new SafeTaskCompletionSource( completionCount: 3 );
            var serverLogs = new ServerEventLogger();
            var clientLogs = new ClientEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        serverLogs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) ) )
                    .SetClientLoggerFactory( _ => clientLogs.GetLogger(
                        MessageBrokerRemoteClientLogger.Create(
                            traceEnd: e =>
                            {
                                if ( e.Type == MessageBrokerRemoteClientTraceEventType.Start )
                                    endSource.Complete();
                            } ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client1 = new ClientMock();
            await client1.GetTask( s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeAcceptedResponse();
                s.SendConfirmHandshakeResponse();
            } );

            using var client2 = new ClientMock();
            await client2.GetTask( s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ) );
                s.ReadHandshakeRejectedResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<MessageBrokerServerException>( e => e.Server.TestRefEquals( server ) ),
                    server.Connectors.Count.TestEquals( 0 ),
                    server.Clients.Count.TestEquals( 1 ),
                    clientLogs
                        .GetAll()
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
                                "[HandshakeEstablished] Client = [1] 'test', TraceId = 0, MessageTimeout = 1 second(s), PingInterval = 10 second(s), BatchPacket = <disabled>",
                                "[Trace:Start] Client = [1] 'test', TraceId = 0 (end)",
                            ] )
                        ] ),
                    clientLogs
                        .GetAllAwaitPacket()
                        .TestContainsContiguousSequence(
                        [
                            "[AwaitPacket] Client = [1] 'test'",
                            "[AwaitPacket] Client = [1] 'test', Packet = (ConfirmHandshakeResponse, Length = 5)"
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task Start_ShouldDisposeGracefully_WhenServerIsEphemeralAndClientIsNot()
        {
            Exception? exception = null;
            var endSource = new SafeTaskCompletionSource();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                traceEnd: e =>
                                {
                                    if ( e.Type == MessageBrokerServerTraceEventType.AcceptClient )
                                        endSource.Complete();
                                },
                                error: e => exception = e.Exception ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s =>
            {
                s.Connect( endPoint );
                s.SendHandshake( "test", Duration.FromSeconds( 1 ), Duration.FromSeconds( 10 ), isEphemeral: false );
                s.ReadHandshakeRejectedResponse();
            } );

            await endSource.Task;

            Assertion.All(
                    exception.TestType().Exact<MessageBrokerServerException>( e => e.Server.TestRefEquals( server ) ),
                    server.Connectors.Count.TestEquals( 0 ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)",
                                $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ReadPacket:Received] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRequest, Length = 24)",
                                $"[HandshakeReceived] Server = {endPoint}, TraceId = 1, ConnectorId = 1, ClientName = 'test', DesiredMessageTimeout = 1 second(s), DesiredPingInterval = 10 second(s), DesiredBatchPacket = <disabled>, SynchronizeExternalObjectNames = False, ClearBuffers = False, IsEphemeral = False, IsClientLittleEndian = True",
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerException: Non-ephemeral client with name 'test' cannot be connected because the server is ephemeral.
                                 """,
                                $"[SendPacket:Sending] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRejectedResponse, Length = 6)",
                                $"[SendPacket:Sent] Server = {endPoint}, TraceId = 1, ConnectorId = 1, Packet = (HandshakeRejectedResponse, Length = 6)",
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task CancelAsync_ShouldDisposeGracefully()
        {
            Exception? exception = null;
            var endSource = new SafeTaskCompletionSource<(MessageBrokerRemoteClientConnector Connector, Task DisposeTask)>();
            var logs = new ServerEventLogger();
            var originalEndPoint = new IPEndPoint( IPAddress.Loopback, 0 );

            await using var server = new MessageBrokerServer(
                originalEndPoint,
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger( logs.GetLogger( MessageBrokerServerLogger.Create( error: e => exception = e.Exception ) ) )
                    .SetStreamDecorator( (c, ns) =>
                    {
                        endSource.Complete( (c, c.CancelAsync().AsTask()) );
                        return ValueTask.FromResult<Stream>( ns );
                    } ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s => s.Connect( endPoint ) );
            var (connector, disposeTask) = await endSource.Task;
            await disposeTask;

            Assertion.All(
                    exception.TestType()
                        .Exact<MessageBrokerRemoteClientConnectorDisposedException>( e => e.Connector.TestRefEquals( connector ) ),
                    server.Connectors.Count.TestEquals( 0 ),
                    connector.Id.TestEquals( 1 ),
                    connector.LocalEndPoint.TestNull(),
                    connector.RemoteEndPoint.TestNull(),
                    connector.State.TestEquals( MessageBrokerRemoteClientConnectorState.Cancelled ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)",
                                $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientConnectorDisposedException: Operation has been cancelled because remote client connector [1] is disposed.
                                 """,
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)"
                            ] )
                        ] ) )
                .Go();
        }

        [Fact]
        public async Task ServerDispose_ShouldCancelPendingConnectors()
        {
            var logs = new ServerEventLogger();
            var connectorSource = new SafeTaskCompletionSource<MessageBrokerRemoteClientConnector>();
            var connectorContinuation = new SafeTaskCompletionSource();
            var server = new MessageBrokerServer(
                new IPEndPoint( IPAddress.Loopback, 0 ),
                MessageBrokerServerOptions.Default
                    .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                    .SetLogger(
                        logs.GetLogger(
                            MessageBrokerServerLogger.Create(
                                connectorStarted: e =>
                                {
                                    connectorSource.Complete( e.Connector );
                                    connectorContinuation.Task.Wait();
                                    Task.Delay( 15 ).Wait();
                                },
                                disposing: _ => connectorContinuation.Complete() ) ) ) );

            await server.StartAsync();
            var endPoint = server.LocalEndPoint;

            using var client = new ClientMock();
            await client.GetTask( s => s.Connect( endPoint ) );

            var connector = await connectorSource.Task;
            await server.DisposeAsync();

            Assertion.All(
                    connector.State.TestEquals( MessageBrokerRemoteClientConnectorState.Cancelled ),
                    logs.GetAll()
                        .Skip( 1 )
                        .TestSequence(
                        [
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (start)",
                                $"[ClientAccepted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"[ConnectorStarted] Server = {endPoint}, TraceId = 1, ConnectorId = 1",
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerRemoteClientConnectorDisposedException: Operation has been cancelled because remote client connector [1] is disposed.
                                 """,
                                $"""
                                 [Error] Server = {endPoint}, TraceId = 1
                                 LfrlAnvil.MessageBroker.Server.Exceptions.MessageBrokerServerDisposedException: Operation has been cancelled because server is disposed.
                                 """,
                                $"[Trace:AcceptClient] Server = {endPoint}, TraceId = 1 (end)"
                            ] ),
                            (t, _) => t.Logs.TestSequence(
                            [
                                $"[Trace:Dispose] Server = {endPoint}, TraceId = 2 (start)",
                                $"[Disposing] Server = {endPoint}, TraceId = 2",
                                $"[Disposed] Server = {endPoint}, TraceId = 2",
                                $"[Trace:Dispose] Server = {endPoint}, TraceId = 2 (end)"
                            ] )
                        ] ) )
                .Go();
        }
    }
}
