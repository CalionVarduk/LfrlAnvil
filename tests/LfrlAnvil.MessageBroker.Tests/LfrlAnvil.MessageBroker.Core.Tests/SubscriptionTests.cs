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

    [Fact]
    public async Task Server_ShouldNotCreateChannel_WhenClientSubscribesToExistingOne()
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
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client1.StartAsync();

        await using var client2 = new MessageBrokerClient(
            new TimestampProvider(),
            server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client2.StartAsync();

        await client1.Listeners.SubscribeAsync( "foo" );
        var result = await client2.Listeners.SubscribeAsync( "foo" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var subscription1 = channel?.Subscriptions.TryGetByClientId( 1 );
        var subscription2 = channel?.Subscriptions.TryGetByClientId( 2 );

        Assertion.All(
                client1.Listeners.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull(
                    v => Assertion.All(
                        "result.Value",
                        v.Type.TestEquals( MessageBrokerSubscriptionResult.ResultType.Subscribed ),
                        v.Listener.ChannelId.TestEquals( 1 ),
                        v.Listener.ChannelName.TestEquals( "foo" ),
                        v.Listener.TestRefEquals( client2.Listeners.TryGetByChannelId( 1 ) ),
                        v.Listener.State.TestEquals( MessageBrokerListenerState.Listening ) ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "remoteClient1",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription1 ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "remoteClient2",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription2 ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "foo" ),
                        c.Subscriptions.Count.TestEquals( 2 ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription1 ),
                        c.Subscriptions.TryGetByClientId( 2 ).TestRefEquals( subscription2 ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnsubscribeFromChannelAndRemoveIt_WhenLastClientUnsubscribes()
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

        await client.Listeners.SubscribeAsync( "foo" );
        var listener = client.Listeners.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );

        var result = Result.Create( MessageBrokerChannelUnsubscribeResult.NotSubscribed );
        if ( listener is not null )
            result = await listener.UnsubscribeAsync();

        Assertion.All(
                listener.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client.Channels.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.TestEquals( MessageBrokerChannelUnsubscribeResult.UnsubscribedAndChannelRemoved ),
                remoteClient.TestNotNull( c => c.Subscriptions.Count.TestEquals( 0 ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnsubscribeFromChannelAndNotRemoveIt_WhenNonLastClientUnsubscribes()
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
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client1.StartAsync();

        await using var client2 = new MessageBrokerClient(
            new TimestampProvider(),
            server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client2.StartAsync();

        await client1.Listeners.SubscribeAsync( "foo" );
        await client2.Listeners.SubscribeAsync( "foo" );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 );
        var listener2 = client2.Listeners.TryGetByChannelId( 1 );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var subscription1 = channel?.Subscriptions.TryGetByClientId( 1 );
        var subscription2 = channel?.Subscriptions.TryGetByClientId( 2 );

        var result = Result.Create( MessageBrokerChannelUnsubscribeResult.NotSubscribed );
        if ( listener2 is not null )
            result = await listener2.UnsubscribeAsync();

        Assertion.All(
                listener1.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Listening ) ),
                listener2.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client1.Listeners.Count.TestEquals( 1 ),
                client2.Listeners.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.TestEquals( MessageBrokerChannelUnsubscribeResult.Unsubscribed ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "remoteClient1",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription1 ) ) ),
                remoteClient2.TestNotNull( c => c.Subscriptions.Count.TestEquals( 0 ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription1 ) ) ),
                subscription1.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ),
                subscription2.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnsubscribeFromChannelAndNotRemoveIt_WhenLastClientUnsubscribesButThereAreActiveLinks()
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

        await client.Listeners.SubscribeAsync( "foo" );
        await client.Channels.LinkAsync( "foo" );
        var listener = client.Listeners.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );

        var result = Result.Create( MessageBrokerChannelUnsubscribeResult.NotSubscribed );
        if ( listener is not null )
            result = await listener.UnsubscribeAsync();

        Assertion.All(
                listener.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client.Channels.Count.TestEquals( 1 ),
                client.Listeners.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.TestEquals( MessageBrokerChannelUnsubscribeResult.Unsubscribed ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "remoteClient",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.GetAll().TestSequence( [ (ch, _) => ch.TestRefEquals( channel ) ] ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.LinkedClients.Count.TestEquals( 1 ),
                        c.LinkedClients.TryGetById( 1 ).TestRefEquals( remoteClient ) ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ) )
            .Go();
    }
}
