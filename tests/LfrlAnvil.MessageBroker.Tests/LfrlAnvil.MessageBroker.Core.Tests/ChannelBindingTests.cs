using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Server;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class ChannelBindingTests : TestsBase
{
    [Fact]
    public async Task Server_ShouldCreateChannelAndStream_WhenClientBindsToNonExistingOne()
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

        var result = await client.Publishers.BindAsync( "foo" );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );

        Assertion.All(
                client.Publishers.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull(
                    v => Assertion.All(
                        "result.Value",
                        v.AlreadyBound.TestFalse(),
                        v.ChannelCreated.TestTrue(),
                        v.StreamCreated.TestTrue(),
                        v.Publisher.ChannelId.TestEquals( 1 ),
                        v.Publisher.ChannelName.TestEquals( "foo" ),
                        v.Publisher.StreamId.TestEquals( 1 ),
                        v.Publisher.StreamName.TestEquals( "foo" ),
                        v.Publisher.TestRefEquals( client.Publishers.TryGetByChannelId( 1 ) ),
                        v.Publisher.State.TestEquals( MessageBrokerPublisherState.Bound ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "remoteClient",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.TryGetByChannelId( 1 ).TestRefEquals( binding ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "foo" ),
                        c.Subscriptions.Count.TestEquals( 0 ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.TryGetByClientId( 1 ).TestRefEquals( binding ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ) ) ),
                stream.TestNotNull(
                    q => Assertion.All(
                        "stream",
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "foo" ),
                        q.Bindings.Count.TestEquals( 1 ),
                        q.Bindings.TryGetByKey( 1, 1 ).TestRefEquals( binding ),
                        q.State.TestEquals( MessageBrokerStreamState.Running ) ) ),
                binding.TestNotNull(
                    b => Assertion.All(
                        "binding",
                        b.Channel.TestRefEquals( channel ),
                        b.Stream.TestRefEquals( stream ),
                        b.Client.TestRefEquals( remoteClient ),
                        b.State.TestEquals( MessageBrokerChannelBindingState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldNotCreateChannelAndStream_WhenClientBindsToExistingOne()
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

        await client1.Publishers.BindAsync( "foo" );
        var result = await client2.Publishers.BindAsync( "foo" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding1 = channel?.Bindings.TryGetByClientId( 1 );
        var binding2 = channel?.Bindings.TryGetByClientId( 2 );

        Assertion.All(
                client1.Publishers.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull(
                    v => Assertion.All(
                        "result.Value",
                        v.AlreadyBound.TestFalse(),
                        v.ChannelCreated.TestFalse(),
                        v.StreamCreated.TestFalse(),
                        v.Publisher.ChannelId.TestEquals( 1 ),
                        v.Publisher.ChannelName.TestEquals( "foo" ),
                        v.Publisher.StreamId.TestEquals( 1 ),
                        v.Publisher.StreamName.TestEquals( "foo" ),
                        v.Publisher.TestRefEquals( client2.Publishers.TryGetByChannelId( 1 ) ),
                        v.Publisher.State.TestEquals( MessageBrokerPublisherState.Bound ) ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "remoteClient1",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.TryGetByChannelId( 1 ).TestRefEquals( binding1 ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "remoteClient2",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.TryGetByChannelId( 1 ).TestRefEquals( binding2 ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "foo" ),
                        c.Bindings.Count.TestEquals( 2 ),
                        c.Bindings.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                        c.Bindings.TryGetByClientId( 2 ).TestRefEquals( binding2 ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ) ) ),
                stream.TestNotNull(
                    q => Assertion.All(
                        "stream",
                        q.Id.TestEquals( 1 ),
                        q.Name.TestEquals( "foo" ),
                        q.Bindings.Count.TestEquals( 2 ),
                        q.Bindings.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ),
                        q.Bindings.TryGetByKey( 2, 1 ).TestRefEquals( binding2 ),
                        q.State.TestEquals( MessageBrokerStreamState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindChannelAndStreamAndRemoveThem_WhenLastClientUnbinds()
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

        await client.Publishers.BindAsync( "foo" );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );

        var result = Result.Create( default( MessageBrokerUnbindResult ) );
        if ( publisher is not null )
            result = await publisher.UnbindAsync();

        Assertion.All(
                publisher.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Disposed ) ),
                client.Publishers.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.ChannelRemoved.TestTrue(),
                result.Value.StreamRemoved.TestTrue(),
                remoteClient.TestNotNull( c => c.Bindings.Count.TestEquals( 0 ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                stream.TestNotNull( q => q.State.TestEquals( MessageBrokerStreamState.Disposed ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindChannelAndStreamAndNotRemoveThem_WhenNonLastClientUnbinds()
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

        await client1.Publishers.BindAsync( "foo" );
        await client2.Publishers.BindAsync( "foo" );
        var publisher1 = client1.Publishers.TryGetByChannelId( 1 );
        var publisher2 = client2.Publishers.TryGetByChannelId( 1 );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding1 = channel?.Bindings.TryGetByClientId( 1 );
        var binding2 = channel?.Bindings.TryGetByClientId( 2 );

        var result = Result.Create( default( MessageBrokerUnbindResult ) );
        if ( publisher2 is not null )
            result = await publisher2.UnbindAsync();

        Assertion.All(
                publisher1.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Bound ) ),
                publisher2.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Disposed ) ),
                client1.Publishers.Count.TestEquals( 1 ),
                client2.Publishers.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.ChannelRemoved.TestFalse(),
                result.Value.StreamRemoved.TestFalse(),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "remoteClient1",
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.TryGetByChannelId( 1 ).TestRefEquals( binding1 ) ) ),
                remoteClient2.TestNotNull( c => c.Bindings.Count.TestEquals( 0 ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 1 ),
                        c.Bindings.TryGetByClientId( 1 ).TestRefEquals( binding1 ) ) ),
                stream.TestNotNull(
                    q => Assertion.All(
                        "stream",
                        q.State.TestEquals( MessageBrokerStreamState.Running ),
                        q.Bindings.Count.TestEquals( 1 ),
                        q.Bindings.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ) ) ),
                binding1.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Running ) ),
                binding2.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindChannelAndNotRemoveIt_WhenLastClientUnbindsButThereAreActiveSubscriptions()
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

        await client.Publishers.BindAsync( "foo" );
        await client.Listeners.SubscribeAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding = channel?.Bindings.TryGetByClientId( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );

        var result = Result.Create( default( MessageBrokerUnbindResult ) );
        if ( publisher is not null )
            result = await publisher.UnbindAsync();

        Assertion.All(
                publisher.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Disposed ) ),
                client.Publishers.Count.TestEquals( 0 ),
                client.Listeners.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.ChannelRemoved.TestFalse(),
                result.Value.StreamRemoved.TestTrue(),
                result.Value.NotBound.TestFalse(),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "remoteClient",
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.Bindings.Count.TestEquals( 0 ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription ) ) ),
                stream.TestNotNull( q => q.State.TestEquals( MessageBrokerStreamState.Disposed ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelBindingState.Disposed ) ),
                subscription.TestNotNull( s => s.State.TestEquals( MessageBrokerSubscriptionState.Running ) ) )
            .Go();
    }
}
