using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Core.Tests.Helpers;
using LfrlAnvil.MessageBroker.Server;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class PublisherTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public PublisherTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task Server_ShouldCreateChannelAndStream_WhenClientBindsAsPublisherToNonExistingOne()
    {
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
                .SetDelaySource( _sharedDelaySource ) );

        await client.StartAsync();

        var result = await client.Publishers.BindAsync( "foo" );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding = channel?.Publishers.TryGetByClientId( 1 );

        Assertion.All(
                client.Publishers.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull( v => Assertion.All(
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
                remoteClient.TestNotNull( c => Assertion.All(
                    "remoteClient",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding ) ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Id.TestEquals( 1 ),
                    c.Name.TestEquals( "foo" ),
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.TryGetByClientId( 1 ).TestRefEquals( binding ),
                    c.State.TestEquals( MessageBrokerChannelState.Running ) ) ),
                stream.TestNotNull( q => Assertion.All(
                    "stream",
                    q.Id.TestEquals( 1 ),
                    q.Name.TestEquals( "foo" ),
                    q.Publishers.Count.TestEquals( 1 ),
                    q.Publishers.TryGetByKey( 1, 1 ).TestRefEquals( binding ),
                    q.State.TestEquals( MessageBrokerStreamState.Running ) ) ),
                binding.TestNotNull( b => Assertion.All(
                    "binding",
                    b.Channel.TestRefEquals( channel ),
                    b.Stream.TestRefEquals( stream ),
                    b.Client.TestRefEquals( remoteClient ),
                    b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldNotCreateChannelAndStream_WhenClientBindsAsPublisherToExistingOne()
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
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client1.StartAsync();

        await using var client2 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client2.StartAsync();

        await client1.Publishers.BindAsync( "foo" );
        var result = await client2.Publishers.BindAsync( "foo" );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding1 = channel?.Publishers.TryGetByClientId( 1 );
        var binding2 = channel?.Publishers.TryGetByClientId( 2 );

        Assertion.All(
                client1.Publishers.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull( v => Assertion.All(
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
                remoteClient1.TestNotNull( c => Assertion.All(
                    "remoteClient1",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding1 ) ) ),
                remoteClient2.TestNotNull( c => Assertion.All(
                    "remoteClient2",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding2 ) ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Id.TestEquals( 1 ),
                    c.Name.TestEquals( "foo" ),
                    c.Publishers.Count.TestEquals( 2 ),
                    c.Publishers.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                    c.Publishers.TryGetByClientId( 2 ).TestRefEquals( binding2 ),
                    c.State.TestEquals( MessageBrokerChannelState.Running ) ) ),
                stream.TestNotNull( q => Assertion.All(
                    "stream",
                    q.Id.TestEquals( 1 ),
                    q.Name.TestEquals( "foo" ),
                    q.Publishers.Count.TestEquals( 2 ),
                    q.Publishers.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ),
                    q.Publishers.TryGetByKey( 2, 1 ).TestRefEquals( binding2 ),
                    q.State.TestEquals( MessageBrokerStreamState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindChannelAndStreamAndRemoveThem_WhenLastClientUnbindsAsPublisher()
    {
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
                .SetDelaySource( _sharedDelaySource ) );

        await client.StartAsync();

        await client.Publishers.BindAsync( "foo" );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding = channel?.Publishers.TryGetByClientId( 1 );

        var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
        if ( publisher is not null )
            result = await publisher.UnbindAsync();

        Assertion.All(
                publisher.TestNotNull( c => c.State.TestEquals( MessageBrokerPublisherState.Disposed ) ),
                client.Publishers.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.ChannelRemoved.TestTrue(),
                result.Value.StreamRemoved.TestTrue(),
                remoteClient.TestNotNull( c => c.Publishers.Count.TestEquals( 0 ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                stream.TestNotNull( q => q.State.TestEquals( MessageBrokerStreamState.Disposed ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindChannelAndStreamAndNotRemoveThem_WhenNonLastClientUnbindsAsPublisher()
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
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client1.StartAsync();

        await using var client2 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client2.StartAsync();

        await client1.Publishers.BindAsync( "foo" );
        await client2.Publishers.BindAsync( "foo" );
        var publisher1 = client1.Publishers.TryGetByChannelId( 1 );
        var publisher2 = client2.Publishers.TryGetByChannelId( 1 );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding1 = channel?.Publishers.TryGetByClientId( 1 );
        var binding2 = channel?.Publishers.TryGetByClientId( 2 );

        var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
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
                remoteClient1.TestNotNull( c => Assertion.All(
                    "remoteClient1",
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.TryGetByChannelId( 1 ).TestRefEquals( binding1 ) ) ),
                remoteClient2.TestNotNull( c => c.Publishers.Count.TestEquals( 0 ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.TryGetByClientId( 1 ).TestRefEquals( binding1 ) ) ),
                stream.TestNotNull( q => Assertion.All(
                    "stream",
                    q.State.TestEquals( MessageBrokerStreamState.Running ),
                    q.Publishers.Count.TestEquals( 1 ),
                    q.Publishers.TryGetByKey( 1, 1 ).TestRefEquals( binding1 ) ) ),
                binding1.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ) ),
                binding2.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindChannelAndNotRemoveIt_WhenLastClientUnbindsAsPublishersButThereAreBoundListeners()
    {
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
                .SetDelaySource( _sharedDelaySource ) );

        await client.StartAsync();

        await client.Publishers.BindAsync( "foo" );
        await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var publisher = client.Publishers.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var stream = server.Streams.TryGetById( 1 );
        var binding = channel?.Publishers.TryGetByClientId( 1 );
        var listener = channel?.Listeners.TryGetByClientId( 1 );

        var result = Result.Create( default( MessageBrokerUnbindPublisherResult ) );
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
                remoteClient.TestNotNull( c => Assertion.All(
                    "remoteClient",
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.GetAll().TestSequence( [ (s, _) => s.TestRefEquals( listener ) ] ) ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.TryGetByClientId( 1 ).TestRefEquals( listener ) ) ),
                stream.TestNotNull( q => q.State.TestEquals( MessageBrokerStreamState.Disposed ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Disposed ) ),
                listener.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ) ) )
            .Go();
    }
}
