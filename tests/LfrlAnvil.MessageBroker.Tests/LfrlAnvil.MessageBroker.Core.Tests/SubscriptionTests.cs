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
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        await using var client = new MessageBrokerClient(
            server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client.StartAsync();

        var result = await client.Listeners.SubscribeAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var queue = remoteClient?.Queues.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );

        Assertion.All(
                client.Listeners.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull(
                    v => Assertion.All(
                        "result.Value",
                        v.AlreadySubscribed.TestFalse(),
                        v.ChannelCreated.TestTrue(),
                        v.Listener.ChannelId.TestEquals( 1 ),
                        v.Listener.ChannelName.TestEquals( "foo" ),
                        v.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                        v.Listener.State.TestEquals( MessageBrokerListenerState.Subscribed ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "remoteClient",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.TryGetByName( "foo" ).TestRefEquals( queue ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "foo" ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ) ) ),
                queue.TestNotNull(
                    q => Assertion.All(
                        "queue",
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "foo" ),
                        q.Client.TestRefEquals( remoteClient ),
                        q.Subscriptions.Count.TestEquals( 1 ),
                        q.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ) ) ),
                subscription.TestNotNull(
                    s => Assertion.All(
                        "subscription",
                        s.Channel.TestRefEquals( channel ),
                        s.Client.TestRefEquals( remoteClient ),
                        s.Queue.TestRefEquals( queue ),
                        s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldNotCreateChannel_WhenClientSubscribesToExistingOne()
    {
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        await using var client1 = new MessageBrokerClient(
            server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client1.StartAsync();

        await using var client2 = new MessageBrokerClient(
            server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client2.StartAsync();

        await client1.Listeners.SubscribeAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var result = await client2.Listeners.SubscribeAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var queue1 = remoteClient1?.Queues.TryGetById( 1 );
        var queue2 = remoteClient2?.Queues.TryGetById( 1 );
        var subscription1 = channel?.Subscriptions.TryGetByClientId( 1 );
        var subscription2 = channel?.Subscriptions.TryGetByClientId( 2 );

        Assertion.All(
                client1.Listeners.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull(
                    v => Assertion.All(
                        "result.Value",
                        v.AlreadySubscribed.TestFalse(),
                        v.ChannelCreated.TestFalse(),
                        v.QueueCreated.TestTrue(),
                        v.Listener.ChannelId.TestEquals( 1 ),
                        v.Listener.ChannelName.TestEquals( "foo" ),
                        v.Listener.TestRefEquals( client2.Listeners.TryGetByChannelId( 1 ) ),
                        v.Listener.State.TestEquals( MessageBrokerListenerState.Subscribed ) ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "remoteClient1",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription1 ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.TryGetByName( "foo" ).TestRefEquals( queue1 ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "remoteClient2",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription2 ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.TryGetByName( "foo" ).TestRefEquals( queue2 ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "foo" ),
                        c.Subscriptions.Count.TestEquals( 2 ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription1 ),
                        c.Subscriptions.TryGetByClientId( 2 ).TestRefEquals( subscription2 ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ) ) ),
                queue1.TestNotNull(
                    q => Assertion.All(
                        "queue1",
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "foo" ),
                        q.Client.TestRefEquals( remoteClient1 ),
                        q.Subscriptions.Count.TestEquals( 1 ),
                        q.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription1 ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ) ) ),
                queue2.TestNotNull(
                    q => Assertion.All(
                        "queue2",
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "foo" ),
                        q.Client.TestRefEquals( remoteClient2 ),
                        q.Subscriptions.Count.TestEquals( 1 ),
                        q.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription2 ),
                        q.State.TestEquals( MessageBrokerQueueState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnsubscribeFromChannelAndQueueAndRemoveThem_WhenLastClientUnsubscribes()
    {
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        await using var client = new MessageBrokerClient(
            server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client.StartAsync();

        await client.Listeners.SubscribeAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var listener = client.Listeners.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var queue = remoteClient?.Queues.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );

        var result = Result.Create( default( MessageBrokerUnsubscribeResult ) );
        if ( listener is not null )
            result = await listener.UnsubscribeAsync();

        Assertion.All(
                listener.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client.Listeners.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.NotSubscribed.TestFalse(),
                result.Value.ChannelRemoved.TestTrue(),
                result.Value.QueueRemoved.TestTrue(),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "remoteClient",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Queues.Count.TestEquals( 0 ) ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                queue.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnsubscribeFromChannelAndNotRemoveIt_WhenNonLastClientUnsubscribes()
    {
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        await using var client1 = new MessageBrokerClient(
            server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client1.StartAsync();

        await using var client2 = new MessageBrokerClient(
            server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client2.StartAsync();

        await client1.Listeners.SubscribeAsync( "foo", (_, _) => ValueTask.CompletedTask );
        await client2.Listeners.SubscribeAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 );
        var listener2 = client2.Listeners.TryGetByChannelId( 1 );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var queue1 = remoteClient1?.Queues.TryGetById( 1 );
        var queue2 = remoteClient2?.Queues.TryGetById( 1 );
        var subscription1 = channel?.Subscriptions.TryGetByClientId( 1 );
        var subscription2 = channel?.Subscriptions.TryGetByClientId( 2 );

        var result = Result.Create( default( MessageBrokerUnsubscribeResult ) );
        if ( listener2 is not null )
            result = await listener2.UnsubscribeAsync();

        Assertion.All(
                listener1.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Subscribed ) ),
                listener2.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client1.Listeners.Count.TestEquals( 1 ),
                client2.Listeners.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.NotSubscribed.TestFalse(),
                result.Value.ChannelRemoved.TestFalse(),
                result.Value.QueueRemoved.TestTrue(),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "remoteClient1",
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByChannelId( 1 ).TestRefEquals( subscription1 ),
                        c.Queues.Count.TestEquals( 1 ),
                        c.Queues.TryGetByName( "foo" ).TestRefEquals( queue1 ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "remoteClient2",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Queues.Count.TestEquals( 0 ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription1 ) ) ),
                subscription1.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ),
                subscription2.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                queue1.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ),
                queue2.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnsubscribeFromChannelAndNotRemoveIt_WhenLastClientUnsubscribesButThereAreBoundClients()
    {
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default.SetHandshakeTimeout( Duration.FromSeconds( 1 ) ) );

        await server.StartAsync();

        await using var client = new MessageBrokerClient(
            server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client.StartAsync();

        await client.Listeners.SubscribeAsync( "foo", (_, _) => ValueTask.CompletedTask );
        await client.Publishers.BindAsync( "foo" );
        var listener = client.Listeners.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var queue = remoteClient?.Queues.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );

        var result = Result.Create( default( MessageBrokerUnsubscribeResult ) );
        if ( listener is not null )
            result = await listener.UnsubscribeAsync();

        Assertion.All(
                listener.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client.Publishers.Count.TestEquals( 1 ),
                client.Listeners.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.NotSubscribed.TestFalse(),
                result.Value.ChannelRemoved.TestFalse(),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "remoteClient",
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Queues.Count.TestEquals( 0 ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( binding ) ] ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.TryGetByClientId( 1 ).TestRefEquals( binding ) ) ),
                queue.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Disposed ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Disposed ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Running ) ) )
            .Go();
    }
}
