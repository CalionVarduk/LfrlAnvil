using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client;
using LfrlAnvil.MessageBroker.Server;

namespace LfrlAnvil.MessageBroker.Core.Tests;

public class MessageTests : TestsBase
{
    [Fact]
    public async Task Server_ShouldAcceptSentMessages_AndSendThemToAppropriateSubscribers()
    {
        var endSource = new SafeTaskCompletionSource( completionCount: 6 );
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

        await using var client2 = new MessageBrokerClient(
            server.LocalEndPoint,
            "test2",
            MessageBrokerClientOptions.Default
                .SetConnectionTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredMessageTimeout( Duration.FromSeconds( 1 ) )
                .SetDesiredPingInterval( Duration.FromSeconds( 0.2 ) ) );

        await client1.StartAsync();
        await client2.StartAsync();

        var sentMessageIds = new List<ulong?>();
        var receivedMessages1 = new List<MessageSnapshot>();
        var receivedMessages2 = new List<MessageSnapshot>();

        await client1.Publishers.BindAsync( "foo" );
        await client1.Listeners.SubscribeAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages1 )
                    receivedMessages1.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            } );

        await client2.Listeners.SubscribeAsync(
            "foo",
            (a, _) =>
            {
                lock ( receivedMessages2 )
                    receivedMessages2.Add( MessageSnapshot.FromArgs( a ) );

                endSource.Complete();
                return ValueTask.CompletedTask;
            } );

        var publisher = client1.Publishers.TryGetByChannelId( 1 );
        var listener1 = client1.Listeners.TryGetByChannelId( 1 )!;
        var listener2 = client2.Listeners.TryGetByChannelId( 1 )!;

        if ( publisher is not null )
        {
            var result = await publisher.SendAsync( new byte[] { 1 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.SendAsync( new byte[] { 2, 3 } );
            sentMessageIds.Add( result.Value.Id );
            result = await publisher.SendAsync( new byte[] { 4, 5, 6 } );
            sentMessageIds.Add( result.Value.Id );
        }

        await endSource.Task;

        Assertion.All(
                sentMessageIds.TestSequence( [ 0UL, 1UL, 2UL ] ),
                receivedMessages1.TestSequence(
                [
                    new MessageSnapshot( listener1, 0, 1, 1, 0, 0, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener1, 1, 1, 1, 0, 0, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener1, 2, 1, 1, 0, 0, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ),
                receivedMessages2.TestSequence(
                [
                    new MessageSnapshot( listener2, 0, 1, 1, 0, 0, MessageSnapshot.GetData( [ 1 ] ) ),
                    new MessageSnapshot( listener2, 1, 1, 1, 0, 0, MessageSnapshot.GetData( [ 2, 3 ] ) ),
                    new MessageSnapshot( listener2, 2, 1, 1, 0, 0, MessageSnapshot.GetData( [ 4, 5, 6 ] ) )
                ] ) )
            .Go();
    }

    private readonly record struct MessageSnapshot(
        MessageBrokerListener Listener,
        ulong Id,
        int SenderId,
        int StreamId,
        int RetryAttempt,
        int RedeliveryAttempt,
        string Data
    )
    {
        [Pure]
        internal static MessageSnapshot FromArgs(MessageBrokerListenerCallbackArgs args)
        {
            return new MessageSnapshot(
                args.Listener,
                args.MessageId,
                args.SenderId,
                args.StreamId,
                args.RetryAttempt,
                args.RedeliveryAttempt,
                GetData( args.Data.ToArray() ) );
        }

        [Pure]
        internal static string GetData(byte[] data)
        {
            return string.Join( ':', data );
        }
    }
}
