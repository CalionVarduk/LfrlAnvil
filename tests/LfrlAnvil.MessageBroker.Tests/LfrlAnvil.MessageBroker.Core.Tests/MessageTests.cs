using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.Computable.Expressions.Extensions;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Core.Tests.Helpers;
using LfrlAnvil.MessageBroker.Server;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class MessageTests : TestsBase, IClassFixture<SharedResourceFixture>
{
    private readonly ValueTaskDelaySource _sharedDelaySource;

    public MessageTests(SharedResourceFixture fixture)
    {
        _sharedDelaySource = fixture.DelaySource;
    }

    [Fact]
    public async Task Server_ShouldAcceptPushedMessages_AndSendThemToAppropriateListeners()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 6 );
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

        await using var client2 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client1.StartAsync();
        await client2.StartAsync();

        var sentMessageIds = new List<ulong?>();
        var receivedMessages1 = new List<MessageSnapshot>();
        var receivedMessages2 = new List<MessageSnapshot>();

        await client1.Publishers.BindAsync( "foo" );
        await client1.Listeners.BindAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages1 )
                    receivedMessages1.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            },
            MessageBrokerListenerOptions.Default.EnableAcks( false ) );

        await client2.Listeners.BindAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages2 )
                    receivedMessages2.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            },
            MessageBrokerListenerOptions.Default.EnableAcks( false ) );

        var publisher = client1.Publishers.TryGetByChannelId( 1 );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 )!;
        var listener2 = client2.Listeners.TryGetByChannelId( 1 )!;

        if ( publisher is not null )
        {
            var result = await publisher.PushAsync( new byte[] { 1 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 2, 3 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 4, 5, 6 } );
            sentMessageIds.Add( result.Value.Id );
        }

        await endSource.Task;

        var sender = new MessageBrokerExternalObject( 1, "test" );
        var stream = new MessageBrokerExternalObject( 1, "foo" );

        Assertion.All(
                sentMessageIds.TestSequence( [ 0UL, 1UL, 2UL ] ),
                receivedMessages1.TestSequence(
                [
                    new MessageSnapshot( listener1, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ),
                receivedMessages2.TestSequence(
                [
                    new MessageSnapshot( listener2, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldAcceptPushedMessages_AndSendThemToAppropriateListeners_WithoutExternalObjectNameSynchronization()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 6 );
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
                .SetSynchronizeExternalObjectNames( false )
                .SetDelaySource( _sharedDelaySource ) );

        await using var client2 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetSynchronizeExternalObjectNames( false )
                .SetDelaySource( _sharedDelaySource ) );

        await client1.StartAsync();
        await client2.StartAsync();

        var sentMessageIds = new List<ulong?>();
        var receivedMessages1 = new List<MessageSnapshot>();
        var receivedMessages2 = new List<MessageSnapshot>();

        await client1.Publishers.BindAsync( "foo" );
        await client1.Listeners.BindAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages1 )
                    receivedMessages1.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            },
            MessageBrokerListenerOptions.Default.EnableAcks( false ) );

        await client2.Listeners.BindAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages2 )
                    receivedMessages2.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            },
            MessageBrokerListenerOptions.Default.EnableAcks( false ) );

        var publisher = client1.Publishers.TryGetByChannelId( 1 );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 )!;
        var listener2 = client2.Listeners.TryGetByChannelId( 1 )!;

        if ( publisher is not null )
        {
            var result = await publisher.PushAsync( new byte[] { 1 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 2, 3 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 4, 5, 6 } );
            sentMessageIds.Add( result.Value.Id );
        }

        await endSource.Task;

        var sender1 = new MessageBrokerExternalObject( 1, "test" );
        var sender2 = new MessageBrokerExternalObject( 1 );
        var stream = new MessageBrokerExternalObject( 1 );

        Assertion.All(
                sentMessageIds.TestSequence( [ 0UL, 1UL, 2UL ] ),
                receivedMessages1.TestSequence(
                [
                    new MessageSnapshot( listener1, 0, sender1, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, sender1, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, sender1, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ),
                receivedMessages2.TestSequence(
                [
                    new MessageSnapshot( listener2, 0, sender2, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, sender2, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, sender2, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldAcceptPushedMessages_AndSendThemToAppropriateListeners_WithAcks()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 6 );
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

        await using var client2 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client1.StartAsync();
        await client2.StartAsync();

        var sentMessageIds = new List<ulong?>();
        var receivedMessages1 = new List<MessageSnapshot>();
        var receivedMessages2 = new List<MessageSnapshot>();

        await client1.Publishers.BindAsync( "foo" );
        await client1.Listeners.BindAsync(
            "foo",
            async (a, _) =>
            {
                lock ( receivedMessages1 )
                    receivedMessages1.Add( MessageSnapshot.FromArgs( a ) );

                await a.AckAsync();
                endSource.Complete();
            } );

        await client2.Listeners.BindAsync(
            "foo",
            async (a, _) =>
            {
                lock ( receivedMessages2 )
                    receivedMessages2.Add( MessageSnapshot.FromArgs( a ) );

                await a.AckAsync();
                endSource.Complete();
            } );

        var publisher = client1.Publishers.TryGetByChannelId( 1 );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 )!;
        var listener2 = client2.Listeners.TryGetByChannelId( 1 )!;

        if ( publisher is not null )
        {
            var result = await publisher.PushAsync( new byte[] { 1 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 2, 3 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 4, 5, 6 } );
            sentMessageIds.Add( result.Value.Id );
        }

        await endSource.Task;

        var sender = new MessageBrokerExternalObject( 1, "test" );
        var stream = new MessageBrokerExternalObject( 1, "foo" );

        Assertion.All(
                sentMessageIds.TestSequence( [ 0UL, 1UL, 2UL ] ),
                receivedMessages1.TestSequence(
                [
                    new MessageSnapshot( listener1, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ),
                receivedMessages2.TestSequence(
                [
                    new MessageSnapshot( listener2, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldAcceptPushedMessages_AndSendThemToAppropriateListeners_WithNegativeAcks()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 18 );
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

        await using var client2 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client1.StartAsync();
        await client2.StartAsync();

        var sentMessageIds = new List<ulong?>();
        var receivedMessages1 = new List<MessageSnapshot>();
        var receivedMessages2 = new List<MessageSnapshot>();

        await client1.Publishers.BindAsync( "foo" );
        await client1.Listeners.BindAsync(
            "foo",
            async (a, _) =>
            {
                lock ( receivedMessages1 )
                    receivedMessages1.Add( MessageSnapshot.FromArgs( a ) );

                await a.NegativeAckAsync();
                endSource.Complete();
            },
            MessageBrokerListenerOptions.Default.SetRetryPolicy( 2, Duration.FromSeconds( 0.2 ) ) );

        await client2.Listeners.BindAsync(
            "foo",
            async (a, _) =>
            {
                lock ( receivedMessages2 )
                    receivedMessages2.Add( MessageSnapshot.FromArgs( a ) );

                await a.NegativeAckAsync();
                endSource.Complete();
            },
            MessageBrokerListenerOptions.Default.SetRetryPolicy( 2, Duration.FromSeconds( 0.2 ) ) );

        var publisher = client1.Publishers.TryGetByChannelId( 1 );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 )!;
        var listener2 = client2.Listeners.TryGetByChannelId( 1 )!;

        if ( publisher is not null )
        {
            var result = await publisher.PushAsync( new byte[] { 1 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 2, 3 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 4, 5, 6 } );
            sentMessageIds.Add( result.Value.Id );
        }

        await endSource.Task;

        var sender = new MessageBrokerExternalObject( 1, "test" );
        var stream = new MessageBrokerExternalObject( 1, "foo" );

        Assertion.All(
                sentMessageIds.TestSequence( [ 0UL, 1UL, 2UL ] ),
                receivedMessages1.TestSequence(
                [
                    new MessageSnapshot( listener1, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener1, 0, sender, stream, 1, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 1, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 1, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener1, 0, sender, stream, 2, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 2, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 2, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ),
                receivedMessages2.TestSequence(
                [
                    new MessageSnapshot( listener2, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener2, 0, sender, stream, 1, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 1, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 1, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener2, 0, sender, stream, 2, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 2, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 2, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldAcceptPushedMessages_AndSendThemToAppropriateListeners_WithRedelivery()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 24 );
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetQueueLoggerFactory( _ => MessageBrokerQueueLogger.Create( messageDiscarded: _ => endSource.Complete() ) ) );

        await server.StartAsync();

        await using var client1 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await using var client2 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client1.StartAsync();
        await client2.StartAsync();

        var sentMessageIds = new List<ulong?>();
        var receivedMessages1 = new List<MessageSnapshot>();
        var receivedMessages2 = new List<MessageSnapshot>();

        await client1.Publishers.BindAsync( "foo" );
        await client1.Listeners.BindAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages1 )
                    receivedMessages1.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            },
            MessageBrokerListenerOptions.Default.SetMaxRedeliveries( 2 ).SetMinAckTimeout( Duration.FromSeconds( 0.1 ) ) );

        await client2.Listeners.BindAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages2 )
                    receivedMessages2.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            },
            MessageBrokerListenerOptions.Default.SetMaxRedeliveries( 2 ).SetMinAckTimeout( Duration.FromSeconds( 0.1 ) ) );

        var publisher = client1.Publishers.TryGetByChannelId( 1 );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 )!;
        var listener2 = client2.Listeners.TryGetByChannelId( 1 )!;

        if ( publisher is not null )
        {
            var result = await publisher.PushAsync( new byte[] { 1 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 2, 3 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 4, 5, 6 } );
            sentMessageIds.Add( result.Value.Id );
        }

        await endSource.Task;

        var sender = new MessageBrokerExternalObject( 1, "test" );
        var stream = new MessageBrokerExternalObject( 1, "foo" );

        Assertion.All(
                sentMessageIds.TestSequence( [ 0UL, 1UL, 2UL ] ),
                receivedMessages1.TestSequence(
                [
                    new MessageSnapshot( listener1, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 0, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 0, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ),
                receivedMessages2.TestSequence(
                [
                    new MessageSnapshot( listener2, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 0, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 0, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldAcceptPushedMessages_AndSendThemToAppropriateListeners_WithRedeliveryAndLargePrefetchHint()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 24 );
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetQueueLoggerFactory( _ => MessageBrokerQueueLogger.Create( messageDiscarded: _ => endSource.Complete() ) ) );

        await server.StartAsync();

        await using var client1 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await using var client2 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client1.StartAsync();
        await client2.StartAsync();

        var sentMessageIds = new List<ulong?>();
        var receivedMessages1 = new List<MessageSnapshot>();
        var receivedMessages2 = new List<MessageSnapshot>();

        await client1.Publishers.BindAsync( "foo" );
        await client1.Listeners.BindAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages1 )
                    receivedMessages1.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            },
            MessageBrokerListenerOptions.Default
                .SetPrefetchHint( 3 )
                .SetMaxRedeliveries( 2 )
                .SetMinAckTimeout( Duration.FromSeconds( 0.2 ) ) );

        await client2.Listeners.BindAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages2 )
                    receivedMessages2.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            },
            MessageBrokerListenerOptions.Default
                .SetPrefetchHint( 3 )
                .SetMaxRedeliveries( 2 )
                .SetMinAckTimeout( Duration.FromSeconds( 0.2 ) ) );

        var publisher = client1.Publishers.TryGetByChannelId( 1 );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 )!;
        var listener2 = client2.Listeners.TryGetByChannelId( 1 )!;

        if ( publisher is not null )
        {
            var result = await publisher.PushAsync( new byte[] { 1 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 2, 3 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 4, 5, 6 } );
            sentMessageIds.Add( result.Value.Id );
        }

        await endSource.Task;

        var sender = new MessageBrokerExternalObject( 1, "test" );
        var stream = new MessageBrokerExternalObject( 1, "foo" );

        Assertion.All(
                sentMessageIds.TestSequence( [ 0UL, 1UL, 2UL ] ),
                receivedMessages1.TestSequence(
                [
                    new MessageSnapshot( listener1, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener1, 0, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener1, 0, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ),
                receivedMessages2.TestSequence(
                [
                    new MessageSnapshot( listener2, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener2, 0, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 0, 1, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener2, 0, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, sender, stream, 0, 2, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldAcceptPushedMessages_AndSendThemToAppropriateListeners_WithRouting()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
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

        await using var client2 = new MessageBrokerClient(
            ( IPEndPoint )server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) )
                .SetDelaySource( _sharedDelaySource ) );

        await client1.StartAsync();
        await client2.StartAsync();

        var sentMessageIds = new List<ulong?>();
        var receivedMessages1 = new List<MessageSnapshot>();
        var receivedMessages2 = new List<MessageSnapshot>();

        await client1.Publishers.BindAsync( "foo" );
        await client1.Listeners.BindAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages1 )
                    receivedMessages1.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            },
            MessageBrokerListenerOptions.Default.EnableAcks( false ) );

        await client2.Listeners.BindAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages2 )
                    receivedMessages2.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            },
            MessageBrokerListenerOptions.Default.EnableAcks( false ) );

        var publisher = client1.Publishers.TryGetByChannelId( 1 );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 )!;
        var listener2 = client2.Listeners.TryGetByChannelId( 1 )!;

        if ( publisher is not null )
        {
            var result = await publisher.PushAsync( new byte[] { 1 }, targets: [ 2 ] );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 2, 3 }, targets: [ "test" ] );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 4, 5, 6 }, targets: [ 3, "bar" ] );
            sentMessageIds.Add( result.Value.Id );
        }

        await endSource.Task;

        var sender = new MessageBrokerExternalObject( 1, "test" );
        var stream = new MessageBrokerExternalObject( 1, "foo" );

        Assertion.All(
                sentMessageIds.TestSequence( [ 0UL, 1UL, 2UL ] ),
                receivedMessages1.TestSequence(
                [
                    new MessageSnapshot( listener1, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) )
                ] ),
                receivedMessages2.TestSequence(
                [
                    new MessageSnapshot( listener2, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) )
                ] ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldSendDeadLetterMessagesAfterQuery()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 6 );
        var deadLetterQueryContinuation = new SafeTaskCompletionSource( completionCount: 3 );
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

        var sentMessageIds = new List<ulong?>();
        var receivedMessages = new List<MessageSnapshot>();

        await client.Publishers.BindAsync( "foo" );
        await client.Listeners.BindAsync(
            "foo",
            async (a, _) =>
            {
                lock ( receivedMessages )
                    receivedMessages.Add( MessageSnapshot.FromArgs( a ) );

                if ( ! a.IsFromDeadLetter )
                {
                    await a.NegativeAckAsync();
                    deadLetterQueryContinuation.Complete();
                }

                endSource.Complete();
            },
            MessageBrokerListenerOptions.Default.SetDeadLetterPolicy( 10, Duration.FromHours( 1 ) ) );

        var publisher = client.Publishers.TryGetByChannelId( 1 );
        var listener = client.Listeners.TryGetByChannelId( 1 )!;

        if ( publisher is not null )
        {
            var result = await publisher.PushAsync( new byte[] { 1 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 2, 3 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 4, 5, 6 } );
            sentMessageIds.Add( result.Value.Id );
        }

        await deadLetterQueryContinuation.Task;
        var queryResult = await client.QueryDeadLetterAsync( listener.QueueId, 10 );
        await endSource.Task;

        var sender = new MessageBrokerExternalObject( 1, "test" );
        var stream = new MessageBrokerExternalObject( 1, "foo" );

        Assertion.All(
                queryResult.Exception.TestNull(),
                queryResult.Value.TotalCount.TestEquals( 3 ),
                queryResult.Value.MaxReadCount.TestEquals( 3 ),
                sentMessageIds.TestSequence( [ 0UL, 1UL, 2UL ] ),
                receivedMessages.TestSequence(
                [
                    new MessageSnapshot( listener, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener, 1, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) ),
                    new MessageSnapshot( listener, 0, sender, stream, 0, 0, true, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener, 1, sender, stream, 0, 0, true, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener, 2, sender, stream, 0, 0, true, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ) )
            .Go();
    }

    [Fact]
    public async Task Server_ShouldNotSendMessagesFilteredOutByListenerFilterExpression()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 2 );
        await using var server = new MessageBrokerServer(
            new IPEndPoint( IPAddress.Loopback, 0 ),
            MessageBrokerServerOptions.Default
                .SetHandshakeTimeout( Duration.FromSeconds( 1 ) )
                .SetDelaySourceFactory( _ => _sharedDelaySource )
                .SetExpressionFactory(
                    new ParsedExpressionFactoryBuilder()
                        .AddGenericArithmeticOperators()
                        .AddGenericBitwiseOperators()
                        .AddGenericLogicalOperators()
                        .AddInt32TypeDefinition()
                        .Build() ) );

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

        var sentMessageIds = new List<ulong?>();
        var receivedMessages = new List<MessageSnapshot>();

        await client.Publishers.BindAsync( "foo" );
        await client.Listeners.BindAsync(
            "foo",
            async (a, _) =>
            {
                lock ( receivedMessages )
                    receivedMessages.Add( MessageSnapshot.FromArgs( a ) );

                await a.AckAsync();
                endSource.Complete();
            },
            MessageBrokerListenerOptions.Default.SetFilterExpression( "c.Data.Length != 2i" ) );

        var publisher = client.Publishers.TryGetByChannelId( 1 );
        var listener = client.Listeners.TryGetByChannelId( 1 )!;

        if ( publisher is not null )
        {
            var result = await publisher.PushAsync( new byte[] { 1 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 2, 3 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.PushAsync( new byte[] { 4, 5, 6 } );
            sentMessageIds.Add( result.Value.Id );
        }

        await endSource.Task;

        var sender = new MessageBrokerExternalObject( 1, "test" );
        var stream = new MessageBrokerExternalObject( 1, "foo" );

        Assertion.All(
                sentMessageIds.TestSequence( [ 0UL, 1UL, 2UL ] ),
                receivedMessages.TestSequence(
                [
                    new MessageSnapshot( listener, 0, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener, 2, sender, stream, 0, 0, false, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ) )
            .Go();
    }

    private readonly record struct MessageSnapshot(
        MessageBrokerListener Listener,
        ulong Id,
        MessageBrokerExternalObject Sender,
        MessageBrokerExternalObject Stream,
        int Retry,
        int Redelivery,
        bool IsFromDeadLetter,
        string Data
    )
    {
        [Pure]
        internal static MessageSnapshot FromArgs(MessageBrokerListenerCallbackArgs args)
        {
            return new MessageSnapshot(
                args.Listener,
                args.MessageId,
                args.Sender,
                args.Stream,
                args.Retry,
                args.Redelivery,
                args.IsFromDeadLetter,
                GetData( args.Data.ToArray() ) );
        }

        [Pure]
        internal static string GetData(byte[] data)
        {
            return string.Join( ':', data );
        }
    }
}
