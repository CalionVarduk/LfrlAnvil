using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Server;
using LfrlAnvil.MessageBroker.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Tests;

public class ListenerTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public ListenerTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task Server_ShouldCreateListenerAndChannel_WhenClientBindsAsListenerToNonExistingChannel()
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

        var result = await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var queue = remoteClient?.Queues.TryGetById( 1 );
        var binding = channel?.Listeners.TryGetByClientId( 1 );

        Assertion.All(
                client.Listeners.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull( v => Assertion.All(
                    "result.Value",
                    v.AlreadyBound.TestFalse(),
                    v.ChannelCreated.TestTrue(),
                    v.Listener.ChannelId.TestEquals( 1 ),
                    v.Listener.ChannelName.TestEquals( "foo" ),
                    v.Listener.TestRefEquals( client.Listeners.TryGetByChannelId( 1 ) ),
                    v.Listener.State.TestEquals( MessageBrokerListenerState.Bound ) ) ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "remoteClient",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding ),
                    c.Queues.Count.TestEquals( 1 ),
                    c.Queues.TryGetByName( "foo" ).TestRefEquals( queue ) ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Id.TestEquals( 1 ),
                    c.Name.TestEquals( "foo" ),
                    c.Publishers.Count.TestEquals( 0 ),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding ),
                    c.State.TestEquals( MessageBrokerChannelState.Running ) ) ),
                queue.TestNotNull( q => Assertion.All(
                    "queue",
                    q.Id.TestEquals( 1 ),
                    q.Name.TestEquals( "foo" ),
                    q.Client.TestRefEquals( remoteClient ),
                    q.Listeners.Count.TestEquals( 1 ),
                    q.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding?.QueueBindings.Primary ),
                    q.State.TestEquals( MessageBrokerQueueState.Running ) ) ),
                binding.TestNotNull( s => Assertion.All(
                    "binding",
                    s.Channel.TestRefEquals( channel ),
                    s.Client.TestRefEquals( remoteClient ),
                    s.Queue.TestRefEquals( queue ),
                    s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldNotCreateChannel_WhenClientBindsAsListenerToExistingOne()
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

        await client1.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var result = await client2.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var queue1 = remoteClient1?.Queues.TryGetById( 1 );
        var queue2 = remoteClient2?.Queues.TryGetById( 1 );
        var binding1 = channel?.Listeners.TryGetByClientId( 1 );
        var binding2 = channel?.Listeners.TryGetByClientId( 2 );

        Assertion.All(
                client1.Listeners.Count.TestEquals( 1 ),
                result.Exception.TestNull(),
                result.Value.TestNotNull( v => Assertion.All(
                    "result.Value",
                    v.AlreadyBound.TestFalse(),
                    v.ChannelCreated.TestFalse(),
                    v.QueueCreated.TestTrue(),
                    v.Listener.ChannelId.TestEquals( 1 ),
                    v.Listener.ChannelName.TestEquals( "foo" ),
                    v.Listener.TestRefEquals( client2.Listeners.TryGetByChannelId( 1 ) ),
                    v.Listener.State.TestEquals( MessageBrokerListenerState.Bound ) ) ),
                remoteClient1.TestNotNull( c => Assertion.All(
                    "remoteClient1",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding1 ),
                    c.Queues.Count.TestEquals( 1 ),
                    c.Queues.TryGetByName( "foo" ).TestRefEquals( queue1 ) ) ),
                remoteClient2.TestNotNull( c => Assertion.All(
                    "remoteClient2",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding2 ),
                    c.Queues.Count.TestEquals( 1 ),
                    c.Queues.TryGetByName( "foo" ).TestRefEquals( queue2 ) ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.Id.TestEquals( 1 ),
                    c.Name.TestEquals( "foo" ),
                    c.Listeners.Count.TestEquals( 2 ),
                    c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding1 ),
                    c.Listeners.TryGetByClientId( 2 ).TestRefEquals( binding2 ),
                    c.State.TestEquals( MessageBrokerChannelState.Running ) ) ),
                queue1.TestNotNull( q => Assertion.All(
                    "queue1",
                    q.Id.TestEquals( 1 ),
                    q.Name.TestEquals( "foo" ),
                    q.Client.TestRefEquals( remoteClient1 ),
                    q.Listeners.Count.TestEquals( 1 ),
                    q.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding1?.QueueBindings.Primary ),
                    q.State.TestEquals( MessageBrokerQueueState.Running ) ) ),
                queue2.TestNotNull( q => Assertion.All(
                    "queue2",
                    q.Id.TestEquals( 1 ),
                    q.Name.TestEquals( "foo" ),
                    q.Client.TestRefEquals( remoteClient2 ),
                    q.Listeners.Count.TestEquals( 1 ),
                    q.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding2?.QueueBindings.Primary ),
                    q.State.TestEquals( MessageBrokerQueueState.Running ) ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindFromChannelAndQueueAndRemoveThem_WhenLastClientUnbindsAsListener()
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

        await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var listener = client.Listeners.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var queue = remoteClient?.Queues.TryGetById( 1 );
        var binding = channel?.Listeners.TryGetByClientId( 1 );

        var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
        if ( listener is not null )
            result = await listener.UnbindAsync();

        Assertion.All(
                listener.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client.Listeners.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.ChannelRemoved.TestTrue(),
                result.Value.QueueRemoved.TestTrue(),
                remoteClient.TestNotNull( c => Assertion.All(
                    "remoteClient",
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Queues.Count.TestEquals( 0 ) ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                binding.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                queue.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindFromChannelAndNotRemoveIt_WhenNonLastClientUnbindsAsListener()
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

        await client1.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
        await client2.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 );
        var listener2 = client2.Listeners.TryGetByChannelId( 1 );
        var remoteClient1 = server.Clients.TryGetById( 1 );
        var remoteClient2 = server.Clients.TryGetById( 2 );
        var channel = server.Channels.TryGetById( 1 );
        var queue1 = remoteClient1?.Queues.TryGetById( 1 );
        var queue2 = remoteClient2?.Queues.TryGetById( 1 );
        var binding1 = channel?.Listeners.TryGetByClientId( 1 );
        var binding2 = channel?.Listeners.TryGetByClientId( 2 );

        var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
        if ( listener2 is not null )
            result = await listener2.UnbindAsync();

        Assertion.All(
                listener1.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Bound ) ),
                listener2.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client1.Listeners.Count.TestEquals( 1 ),
                client2.Listeners.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.ChannelRemoved.TestFalse(),
                result.Value.QueueRemoved.TestTrue(),
                remoteClient1.TestNotNull( c => Assertion.All(
                    "remoteClient1",
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.TryGetByChannelId( 1 ).TestRefEquals( binding1 ),
                    c.Queues.Count.TestEquals( 1 ),
                    c.Queues.TryGetByName( "foo" ).TestRefEquals( queue1 ) ) ),
                remoteClient2.TestNotNull( c => Assertion.All(
                    "remoteClient2",
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Queues.Count.TestEquals( 0 ) ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Listeners.Count.TestEquals( 1 ),
                    c.Listeners.TryGetByClientId( 1 ).TestRefEquals( binding1 ) ) ),
                binding1.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Running ) ),
                binding2.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                queue1.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Running ) ),
                queue2.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindFromChannelAndNotRemoveIt_WhenLastClientUnbindsAsListenerButThereAreBoundPublishers()
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

        await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
        await client.Publishers.BindAsync( "foo" );
        var listener = client.Listeners.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var queue = remoteClient?.Queues.TryGetById( 1 );
        var listenerBinding = channel?.Listeners.TryGetByClientId( 1 );
        var publisherBinding = channel?.Publishers.TryGetByClientId( 1 );

        var result = Result.Create( default( MessageBrokerUnbindListenerResult ) );
        if ( listener is not null )
            result = await listener.UnbindAsync();

        Assertion.All(
                listener.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client.Publishers.Count.TestEquals( 1 ),
                client.Listeners.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.NotBound.TestFalse(),
                result.Value.ChannelRemoved.TestFalse(),
                remoteClient.TestNotNull( c => Assertion.All(
                    "remoteClient",
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Queues.Count.TestEquals( 0 ),
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.GetAll().TestSequence( [ (b, _) => b.TestRefEquals( publisherBinding ) ] ) ) ),
                channel.TestNotNull( c => Assertion.All(
                    "channel",
                    c.State.TestEquals( MessageBrokerChannelState.Running ),
                    c.Listeners.Count.TestEquals( 0 ),
                    c.Publishers.Count.TestEquals( 1 ),
                    c.Publishers.TryGetByClientId( 1 ).TestRefEquals( publisherBinding ) ) ),
                queue.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Disposed ) ),
                listenerBinding.TestNotNull( s => s.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ),
                publisherBinding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelPublisherBindingState.Running ) ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldUnbindChannel_WhenClientUnbindsListenerByName()
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

        await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var listener = client.Listeners.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var channel = server.Channels.TryGetById( 1 );
        var binding = channel?.Listeners.TryGetByClientId( 1 );
        var queue = binding?.Queue;
        var result = await client.Listeners.UnbindAsync( "foo" );

        Assertion.All(
                listener.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client.Listeners.Count.TestEquals( 0 ),
                result.Exception.TestNull(),
                result.Value.ChannelRemoved.TestTrue(),
                result.Value.QueueRemoved.TestTrue(),
                result.Value.NotBound.TestFalse(),
                remoteClient.TestNotNull( c => Assertion.All(
                    "remoteClient",
                    c.Listeners.Count.TestEquals( 0 ) ) ),
                channel.TestNotNull( c => c.State.TestEquals( MessageBrokerChannelState.Disposed ) ),
                queue.TestNotNull( q => q.State.TestEquals( MessageBrokerQueueState.Disposed ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ) )
            .Go();
    }

    [Fact]
    public async Task DeleteListener_ShouldBePropagatedToClient()
    {
        var completionSource = new SafeTaskCompletionSource();

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
                        traceEnd: e =>
                        {
                            if ( e.Type == MessageBrokerClientTraceEventType.SystemNotification )
                                completionSource.Complete();
                        } ) ) );

        await client.StartAsync();

        await client.Listeners.BindAsync( "foo", (_, _) => ValueTask.CompletedTask );
        var listener = client.Listeners.TryGetByChannelId( 1 );
        var remoteClient = server.Clients.TryGetById( 1 );
        var binding = remoteClient?.Listeners.TryGetByChannelId( 1 );

        await (binding?.DeleteAsync().AsTask() ?? Task.CompletedTask);
        await completionSource.Task;

        Assertion.All(
                listener.TestNotNull( c => c.State.TestEquals( MessageBrokerListenerState.Disposed ) ),
                client.Listeners.Count.TestEquals( 0 ),
                remoteClient.TestNotNull( c => Assertion.All(
                    "remoteClient",
                    c.Listeners.Count.TestEquals( 0 ) ) ),
                binding.TestNotNull( b => b.State.TestEquals( MessageBrokerChannelListenerBindingState.Disposed ) ) )
            .Go();
    }
}
