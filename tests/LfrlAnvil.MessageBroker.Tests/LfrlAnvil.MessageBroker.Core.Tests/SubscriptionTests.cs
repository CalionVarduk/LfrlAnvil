using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Server;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class SubscriptionTests : TestsBase
{
    [Fact]
    public async Task Server_ShouldCreateSubscriptionAndChannel_WhenClientSubscribesToNonExistingChannel()
    {
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
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client.StartAsync();

        var result = await client.Listeners.SubscribeAsync( "foo" );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );

        Assertion.All(
                client.Listeners.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull(
                    v => Assertion.All(
                        "result.Value",
                        v.Type.TestEquals( MessageBrokerSubscriptionResult.ResultType.SubscribedAndChannelCreated ),
                        v.Listener.ChannelId.TestEquals( 1 ),
                        v.Listener.ChannelName.TestEquals( "foo" ),
                        v.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                        v.Listener.State.TestEquals( MessageBrokerListenerState.Listening ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "remoteClient",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "foo" ),
                        c.LinkedClients.Count.TestEquals( 0 ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ) ) ),
                subscription.TestNotNull(
                    s => Assertion.All(
                        "subscription",
                        s.Channel.TestRefEquals( channel ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ) ) )
            .Go();
    }
}
