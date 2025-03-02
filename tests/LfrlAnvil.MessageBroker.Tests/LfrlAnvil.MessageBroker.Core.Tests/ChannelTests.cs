using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Server;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class ChannelTests : TestsBase
{
    [Fact]
    public async Task Server_ShouldCreateChannel_WhenClientLinksToNonExistingOne()
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

        var result = await client.Channels.LinkAsync( "foo" );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );

        Assertion.All(
                client.Channels.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull(
                    v => Assertion.All(
                        "result.Value",
                        v.Type.TestEquals( MessageBrokerChannelLinkResult.ResultType.CreatedAndLinked ),
                        v.Channel.Id.TestEquals( 1 ),
                        v.Channel.Name.TestEquals( "foo" ),
                        v.Channel.TestRefEquals( client.Channels.TryGetById( 1 ) ),
                        v.Channel.State.TestEquals( MessageBrokerLinkedChannelState.Linked ) ) ),
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
    public async Task Server_ShouldNotCreateChannel_WhenClientLinksToExistingOne()
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

        await client1.Channels.LinkAsync( "foo" );
        var result = await client2.Channels.LinkAsync( "foo" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );

        Assertion.All(
                client1.Channels.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull(
                    v => Assertion.All(
                        "result.Value",
                        v.Type.TestEquals( MessageBrokerChannelLinkResult.ResultType.Linked ),
                        v.Channel.Id.TestEquals( 1 ),
                        v.Channel.Name.TestEquals( "foo" ),
                        v.Channel.TestRefEquals( client2.Channels.TryGetById( 1 ) ),
                        v.Channel.State.TestEquals( MessageBrokerLinkedChannelState.Linked ) ) ),
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
    public async Task Server_ShouldUnlinkChannelAndRemoveIt_WhenLastClientUnlinks()
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

        await client.Channels.LinkAsync( "foo" );
        var linkedChannel = client.Channels.TryGetById( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );

        var result = Result.Create( MessageBrokerChannelUnlinkResult.NotLinked );
        if ( linkedChannel is not null )
            result = await linkedChannel.UnlinkAsync();

        Assertion.All(
                linkedChannel.TestNotNull( c => c.State.TestEquals( MessageBrokerLinkedChannelState.Unlinked ) ),
                client.Channels.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.TestEquals( MessageBrokerChannelUnlinkResult.UnlinkedAndChannelRemoved ),
                remoteClient.TestNotNull( c => c.LinkedChannels.Count.TestEquals( 0 ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnlinkChannelAndNotRemoveIt_WhenNonLastClientUnlinks()
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

        await client1.Channels.LinkAsync( "foo" );
        await client2.Channels.LinkAsync( "foo" );
        var linkedChannel1 = client1.Channels.TryGetById( 1 );
        var linkedChannel2 = client2.Channels.TryGetById( 1 );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );

        var result = Result.Create( MessageBrokerChannelUnlinkResult.NotLinked );
        if ( linkedChannel2 is not null )
            result = await linkedChannel2.UnlinkAsync();

        Assertion.All(
                linkedChannel1.TestNotNull( c => c.State.TestEquals( MessageBrokerLinkedChannelState.Linked ) ),
                linkedChannel2.TestNotNull( c => c.State.TestEquals( MessageBrokerLinkedChannelState.Unlinked ) ),
                client1.Channels.Count.TestEquals( 1 ),
                client2.Channels.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.TestEquals( MessageBrokerChannelUnlinkResult.Unlinked ),
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
}
