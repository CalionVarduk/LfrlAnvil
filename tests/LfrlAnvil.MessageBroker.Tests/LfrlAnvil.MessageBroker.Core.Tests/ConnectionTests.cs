using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Core.Tests.Helpers;
using LfrlAnvil.MessageBroker.Server;
using LfrlAnvil.MessageBroker.Server.Events;
using MessageBrokerClientEndpoint = LfrlAnvil.MessageBroker.Client.Events.MessageBrokerClientEndpoint;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class ConnectionTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public ConnectionTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task ClientAndServer_ShouldExchangePingsWhenIdle_AfterExchangingHandshake()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );

        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource ) );

        await server.StartAsync();

        await using var client = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger(
                    MessageBrokerClientLogger.Create(
                        readPacket: e =>
                        {
                            if ( e.Type == MessageBrokerClientReadPacketEventType.Accepted
                                && e.Packet.Endpoint == MessageBrokerClientEndpoint.Pong )
                                endSource.Complete();
                        } ) ) );

        await client.StartAsync();
        await endSource.Task;
        var remoteClient = server.Clients.TryGetById( 1 );

        remoteClient.TestNotNull(
                r => Assertion.All(
                    "client",
                    client.Id.TestEquals( r.Id ),
                    client.Name.TestEquals( r.Name ),
                    client.MessageTimeout.TestEquals( r.MessageTimeout ),
                    client.PingInterval.TestEquals( r.PingInterval ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldRejectClientHandshake_WhenNameAlreadyExists()
    {
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource ) );

        await server.StartAsync();

        await using var client1 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 10 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client1.StartAsync();

        await using var client2 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 10 ) )
                .SetDelaySource( _sharedDelaySource ) );

        var result = await client2.StartAsync();

        Assertion.All(
                server.Clients.Count.TestEquals( 1 ),
                result.Exception.TestType().AssignableTo<MessageBrokerClientRequestException>() )
            .Go();
    }

    [Fact]
    public async Task ClientDispose_ShouldRemoveClientFromServer()
    {
        var endSource = new SafeTaskCompletionSource();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetClientLoggerFactory(
                    _ => MessageBrokerRemoteClientLogger.Create(
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerRemoteClientTraceEventType.Deactivate )
                                endSource.Complete();
                        } ) ) );

        await server.StartAsync();

        var client = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client.StartAsync();
        var remoteClient = server.Clients.TryGetById( 1 );
        await client.DisposeAsync();
        await endSource.Task;

        Assertion.All(
                remoteClient.TestNotNull( c => c.State.TestEquals( MessageBrokerRemoteClientState.Disposed ) ),
                server.Clients.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public async Task RemoteClientDisconnect_ShouldDisposeClient()
    {
        var endSource = new SafeTaskCompletionSource();
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource ) );

        await server.StartAsync();

        await using var client = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource )
                .SetLogger( MessageBrokerClientLogger.Create( disposed: _ => endSource.Complete() ) ) );

        await client.StartAsync();
        var remoteClient = server.Clients.TryGetById( 1 );
        if ( remoteClient is not null )
            await remoteClient.DisconnectAsync();

        await endSource.Task;

        client.State.TestEquals( MessageBrokerClientState.Disposed ).Go();
    }
}
