using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Server;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class ConnectionTests : TestsBase
{
    [Fact]
    public async Task ClientAndServer_ShouldExchangePingsWhenIdle_AfterExchangingHandshake()
    {
        var pingResponseCount = 0;
        var endSource = new SafeTaskCompletionSource();

        await using var server = new MessageBrokerServer(
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        await using var client = new MessageBrokerClient(
            new TimestampProvider(),
            server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetEventHandler(
                    e =>
                    {
                        if ( e.Type == MessageBrokerClientEventType.MessageAccepted
                            && e.GetClientEndpoint() == MessageBrokerClientEndpoint.PingResponse
                            && ++pingResponseCount == 2 )
                            endSource.Complete();
                    } ) );

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
            () => new TimestampProvider(),
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        await using var client1 = new MessageBrokerClient(
            new TimestampProvider(),
            server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 10 ) ) );

        await client1.StartAsync();

        await using var client2 = new MessageBrokerClient(
            new TimestampProvider(),
            server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 10 ) ) );

        var result = await client2.StartAsync();

        Assertion.All(
                server.Clients.Count.TestEquals( 1 ),
                result.Exception.TestType().AssignableTo<MessageBrokerClientRequestException>() )
            .Go();
    }
}
