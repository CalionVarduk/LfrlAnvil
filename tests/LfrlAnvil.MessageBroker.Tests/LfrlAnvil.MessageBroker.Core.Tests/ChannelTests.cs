using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Server;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class ChannelTests : TestsBase
{
    [Fact]
    public async Task Server_ShouldCreateChannel_WhenClientBindsToNonExistingOne()
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

        var result = await client.Publishers.BindAsync( "foo" );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );

        Assertion.All(
                client.Publishers.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull(
                    v => Assertion.All(
                        "result.Value",
                        v.AlreadyBound.TestFalse(),
                        v.ChannelCreated.TestTrue(),
                        v.Publisher.ChannelId.TestEquals( 1 ),
                        v.Publisher.ChannelName.TestEquals( "foo" ),
                        v.Publisher.TestRefEquals( client.Publishers.TryGetByChannelId( 1 ) ),
                        v.Publisher.State.TestEquals( MessageBrokerPublisherState.Bound ) ) ),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "remoteClient",
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.TryGetById( 1 ).TestRefEquals( channel ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "foo" ),
                        c.LinkedClients.Count.TestEquals( 1 ),
                        c.LinkedClients.TryGetById( 1 ).TestRefEquals( remoteClient ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldNotCreateChannel_WhenClientBindsToExistingOne()
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

        await client1.Publishers.BindAsync( "foo" );
        var result = await client2.Publishers.BindAsync( "foo" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );

        Assertion.All(
                client1.Publishers.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull(
                    v => Assertion.All(
                        "result.Value",
                        v.AlreadyBound.TestFalse(),
                        v.ChannelCreated.TestFalse(),
                        v.Publisher.ChannelId.TestEquals( 1 ),
                        v.Publisher.ChannelName.TestEquals( "foo" ),
                        v.Publisher.TestRefEquals( client2.Publishers.TryGetByChannelId( 1 ) ),
                        v.Publisher.State.TestEquals( MessageBrokerPublisherState.Bound ) ) ),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "remoteClient1",
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.TryGetById( 1 ).TestRefEquals( channel ) ) ),
                remoteClient2.TestNotNull(
                    c => Assertion.All(
                        "remoteClient2",
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.TryGetById( 1 ).TestRefEquals( channel ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.Id.TestEquals( 1 ),
                        c.Name.TestEquals( "foo" ),
                        c.LinkedClients.Count.TestEquals( 2 ),
                        c.LinkedClients.TryGetById( 1 ).TestRefEquals( remoteClient1 ),
                        c.LinkedClients.TryGetById( 2 ).TestRefEquals( remoteClient2 ),
                        c.State.TestEquals( MessageBrokerChannelState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindChannelAndRemoveIt_WhenLastClientUnbinds()
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

        await client.Publishers.BindAsync( "foo" );
        var linkedChannel = client.Publishers.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );

        var result = Result.Create( default( MessageBrokerUnbindResult ) );
        if ( linkedChannel is not null )
            result = await linkedChannel.UnbindAsync();

        Assertion.All(
                linkedChannel.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Disposed ) ),
                client.Publishers.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.ChannelRemoved.TestTrue(),
                result.Value.NotBound.TestFalse(),
                remoteClient.TestNotNull( c => c.LinkedChannels.Count.TestEquals( 0 ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindChannelAndNotRemoveIt_WhenNonLastClientUnbinds()
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

        await client1.Publishers.BindAsync( "foo" );
        await client2.Publishers.BindAsync( "foo" );
        var linkedChannel1 = client1.Publishers.TryGetByChannelId( 1 );
        var linkedChannel2 = client2.Publishers.TryGetByChannelId( 1 );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );

        var result = Result.Create( default( MessageBrokerUnbindResult ) );
        if ( linkedChannel2 is not null )
            result = await linkedChannel2.UnbindAsync();

        Assertion.All(
                linkedChannel1.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Bound ) ),
                linkedChannel2.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Disposed ) ),
                client1.Publishers.Count.TestEquals( 1 ),
                client2.Publishers.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.ChannelRemoved.TestFalse(),
                result.Value.NotBound.TestFalse(),
                remoteClient1.TestNotNull(
                    c => Assertion.All(
                        "remoteClient1",
                        c.LinkedChannels.Count.TestEquals( 1 ),
                        c.LinkedChannels.TryGetById( 1 ).TestRefEquals( channel ) ) ),
                remoteClient2.TestNotNull( c => c.LinkedChannels.Count.TestEquals( 0 ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.LinkedClients.Count.TestEquals( 1 ),
                        c.LinkedClients.TryGetById( 1 ).TestRefEquals( remoteClient1 ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindChannelAndNotRemoveIt_WhenLastClientUnbindsButThereAreActiveSubscriptions()
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

        await client.Publishers.BindAsync( "foo" );
        await client.Listeners.SubscribeAsync( "foo" );
        var linkedChannel = client.Publishers.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var subscription = channel?.Subscriptions.TryGetByClientId( 1 );

        var result = Result.Create( default( MessageBrokerUnbindResult ) );
        if ( linkedChannel is not null )
            result = await linkedChannel.UnbindAsync();

        Assertion.All(
                linkedChannel.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Disposed ) ),
                client.Publishers.Count.TestEquals( 0 ),
                client.Listeners.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.ChannelRemoved.TestFalse(),
                result.Value.NotBound.TestFalse(),
                remoteClient.TestNotNull(
                    c => Assertion.All(
                        "remoteClient",
                        c.LinkedChannels.Count.TestEquals( 0 ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( subscription ) ] ) ) ),
                channel.TestNotNull(
                    c => Assertion.All(
                        "channel",
                        c.State.TestEquals( MessageBrokerChannelState.Running ),
                        c.LinkedClients.Count.TestEquals( 0 ),
                        c.Subscriptions.Count.TestEquals( 1 ),
                        c.Subscriptions.TryGetByClientId( 1 ).TestRefEquals( subscription ) ) ) )
            .Go();
    }
}
