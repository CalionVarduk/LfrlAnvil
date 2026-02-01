using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Collections;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Internal;
using LfrlAnvil.MessageBroker.Server.Tests.Helpers;

namespace LfrlAnvil.MessageBroker.Server.Tests;

public class QueueHeapTests : TestsBase
{
    [Fact]
    public async Task RetryHeap_Add_ShouldSatisfyHeapInvariant()
    {
        var client = await CreateClient();
        var channel = CreateChannel( client );
        var publisher = CreatePublisher( client, channel );
        var listener = CreateListener( client, channel );

        var delays = Fixture.CreateMany<TimeSpan>( x => x.Ticks > 0, count: 18 ).ToList();
        delays.Add( delays.Max() + TimeSpan.FromTicks( 1 ) );
        delays.Add( delays.Min() - TimeSpan.FromTicks( 1 ) );
        var sut = QueueRetryHeap.Create();

        foreach ( var delay in delays )
            sut.Add( new QueueMessage( publisher, listener, 0 ), 0, 0, new Duration( delay ) );

        Assertion.All(
                sut.IsEmpty.TestFalse(),
                sut.Count.TestEquals( delays.Count ),
                sut.First().SendAt.TestEquals( GetMinTimestamp( in sut ) ),
                AssertHeapInvariant( in sut ) )
            .Go();
    }

    [Fact]
    public async Task RetryHeap_Pop_ShouldCauseEntriesToBeRemovedInOrder()
    {
        var client = await CreateClient();
        var channel = CreateChannel( client );
        var publisher = CreatePublisher( client, channel );
        var listener = CreateListener( client, channel );

        var delays = new long[] { 1, 3, 2, 5, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }
            .Select( Duration.FromTicks )
            .ToArray();

        var sut = QueueRetryHeap.Create();
        foreach ( var delay in delays )
            sut.Add( new QueueMessage( publisher, listener, 0 ), 0, 0, new Duration( delay ) );

        var popped = new List<Timestamp>();
        while ( ! sut.IsEmpty )
        {
            popped.Add( sut.First().SendAt );
            sut.Pop();
        }

        Assertion.All(
                popped.Count.TestEquals( delays.Length ),
                popped.IsOrdered().TestTrue() )
            .Go();
    }

    [Fact]
    public async Task EventHeap_Add_ShouldAddWithMaxTimestamp()
    {
        var client = await CreateClient();
        var queues = Enumerable.Range( 1, 10 ).Select( id => CreateQueue( client, id ) ).ToList();

        var sut = QueueEventHeap.Create();
        foreach ( var queue in queues )
            sut.Add( queue );

        var result = sut.TryGetNextTimestamp( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( new Timestamp( long.MaxValue ) ),
                queues.TestAll( (q, i) => q.EventHeapIndex.TestEquals( i ) ) )
            .Go();
    }

    [Fact]
    public async Task EventHeap_UpdateAndRemove_ShouldPreserveHeapInvariant()
    {
        var timestamps = new TimestampProviderMock();
        var client = await CreateClient( timestamps );
        var channel = CreateChannel( client );
        var publisher = CreatePublisher( client, channel );
        var listener = CreateListener( client, channel );
        var queues = Enumerable.Range( 1, 10 ).Select( id => CreateQueue( client, id ) ).ToList();

        var sut = QueueEventHeap.Create();
        sut.Add( queues[0] );
        queues[0]
            .MessageStore
            .AddUnacked( new QueueMessage( publisher, listener, 0 ), 0, 0, 1, out _ );

        timestamps.Next -= Duration.FromSeconds( 0.5 );
        queues[0]
            .MessageStore
            .AddUnacked( new QueueMessage( publisher, listener, 1 ), 0, 0, 2, out _ );

        timestamps.Next += Duration.FromSeconds( 0.5 );
        sut.Update( queues[0] );

        sut.Add( queues[1] );
        queues[1]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 2 ), 0, 0, Duration.FromSeconds( 5 ) );

        sut.Update( queues[1] );

        sut.Add( queues[2] );
        timestamps.Next += Duration.FromSeconds( 1 );
        queues[2]
            .MessageStore
            .AddUnacked( new QueueMessage( publisher, listener, 3 ), 3, 0, 1, out _ );

        timestamps.Next -= Duration.FromSeconds( 1 );
        queues[2]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 4 ), 0, 0, Duration.FromSeconds( 5 ) );

        sut.Update( queues[2] );

        sut.Add( queues[3] );
        queues[3]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 5 ), 0, 0, Duration.FromSeconds( 2.5 ) );

        sut.Update( queues[3] );
        queues[3]
            .MessageStore
            .Retries
            .Pop();

        queues[3]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 6 ), 0, 0, Duration.FromSeconds( 3 ) );

        sut.Update( queues[3] );

        sut.Add( queues[4] );
        queues[4]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 7 ), 0, 0, Duration.FromSeconds( 4 ) );

        sut.Update( queues[4] );

        timestamps.Next += Duration.FromSeconds( 3 );
        sut.Add( queues[5] );
        queues[5]
            .MessageStore
            .AddToDeadLetter( new QueueMessage( publisher, listener, 8 ), 0, 0 );

        timestamps.Next -= Duration.FromSeconds( 3 );
        queues[5]
            .MessageStore
            .AddToDeadLetter( new QueueMessage( publisher, listener, 9 ), 0, 0 );

        sut.Update( queues[5] );

        sut.Add( queues[6] );
        queues[6]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 10 ), 0, 0, Duration.FromSeconds( 7 ) );

        sut.Update( queues[6] );

        sut.Add( queues[7] );
        queues[7]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 11 ), 0, 0, Duration.FromSeconds( 8 ) );

        sut.Update( queues[7] );

        sut.Add( queues[8] );
        sut.Update( queues[8] );

        sut.Add( queues[9] );
        queues[9]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 12 ), 0, 0, Duration.FromSeconds( 1 ) );

        listener.TryIncrementPrefetchCounter( out _ );
        sut.Update( queues[9] );
        listener.DecrementPrefetchCounter();

        var indexes = new List<int[]>();
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Remove( queues[9] );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Remove( queues[8] );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Remove( queues[5] );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Remove( queues[6] );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Remove( queues[7] );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Remove( queues[1] );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Remove( queues[0] );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Remove( queues[2] );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Remove( queues[3] );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Remove( queues[4] );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );

        indexes.TestSequence(
            [
                (x, _) => x.TestSequence( [ 0, 3, 2, 1, 4, 5, 6, 7, 8, 9 ] ),
                (x, _) => x.TestSequence( [ 0, 3, 2, 1, 4, 5, 6, 7, 8, -1 ] ),
                (x, _) => x.TestSequence( [ 0, 3, 2, 1, 4, 5, 6, 7, -1, -1 ] ),
                (x, _) => x.TestSequence( [ 0, 3, 2, 1, 4, -1, 6, 5, -1, -1 ] ),
                (x, _) => x.TestSequence( [ 0, 3, 2, 1, 4, -1, -1, 5, -1, -1 ] ),
                (x, _) => x.TestSequence( [ 0, 3, 2, 1, 4, -1, -1, -1, -1, -1 ] ),
                (x, _) => x.TestSequence( [ 0, -1, 2, 1, 3, -1, -1, -1, -1, -1 ] ),
                (x, _) => x.TestSequence( [ -1, -1, 0, 1, 2, -1, -1, -1, -1, -1 ] ),
                (x, _) => x.TestSequence( [ -1, -1, -1, 0, 1, -1, -1, -1, -1, -1 ] ),
                (x, _) => x.TestSequence( [ -1, -1, -1, -1, 0, -1, -1, -1, -1, -1 ] ),
                (x, _) => x.TestSequence( [ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 ] )
            ] )
            .Go();
    }

    [Fact]
    public async Task EventHeap_Process_ShouldConsumeNextQueueEvent()
    {
        var timestamps = new TimestampProviderMock();
        var client = await CreateClient( timestamps );
        var channel = CreateChannel( client );
        var publisher = CreatePublisher( client, channel );
        var listener = CreateListener( client, channel );
        var queues = Enumerable.Range( 1, 5 ).Select( id => CreateQueue( client, id ) ).ToList();

        var sut = QueueEventHeap.Create();
        sut.Add( queues[0] );
        queues[0]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 0 ), 0, 0, Duration.FromSeconds( 1 ) );

        sut.Update( queues[0] );

        sut.Add( queues[1] );
        queues[1]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 1 ), 0, 0, Duration.FromSeconds( 5 ) );

        sut.Update( queues[1] );

        sut.Add( queues[2] );
        queues[2]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 2 ), 0, 0, Duration.FromSeconds( 2 ) );

        sut.Update( queues[2] );

        sut.Add( queues[3] );
        queues[3]
            .MessageStore
            .AddToDeadLetter( new QueueMessage( publisher, listener, 3 ), 0, 0 );

        sut.Update( queues[3] );

        sut.Add( queues[4] );
        queues[4]
            .MessageStore
            .ScheduleRetry( new QueueMessage( publisher, listener, 4 ), 0, 0, Duration.FromSeconds( 4 ) );

        sut.Update( queues[4] );

        var indexes = new List<int[]>();
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Process( timestamps.Next );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Process( timestamps.Next + Duration.FromSeconds( 3 ) );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Process( timestamps.Next + Duration.FromSeconds( 4 ) );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );
        sut.Process( timestamps.Next + Duration.FromSeconds( 5 ) );
        indexes.Add( queues.Select( q => q.EventHeapIndex ).ToArray() );

        indexes.TestSequence(
            [
                (x, _) => x.TestSequence( [ 0, 3, 2, 1, 4 ] ),
                (x, _) => x.TestSequence( [ 0, 3, 2, 1, 4 ] ),
                (x, _) => x.TestSequence( [ 2, 1, 4, 3, 0 ] ),
                (x, _) => x.TestSequence( [ 2, 0, 4, 3, 1 ] ),
                (x, _) => x.TestSequence( [ 2, 0, 4, 3, 1 ] )
            ] )
            .Go();
    }

    [Pure]
    private static Timestamp GetMinTimestamp(in QueueRetryHeap heap)
    {
        var result = new Timestamp( long.MaxValue );
        for ( var i = 0; i < heap.Count; ++i )
            result = result.Min( heap[i].SendAt );

        return result;
    }

    [Pure]
    private static Assertion AssertHeapInvariant(in QueueRetryHeap heap)
    {
        var maxParentIndex = (heap.Count >> 1) - 1;

        var assertions = new List<Assertion>();
        for ( var parentIndex = 0; parentIndex <= maxParentIndex; ++parentIndex )
        {
            var parent = heap[parentIndex];

            var leftChildIndex = Heap.GetLeftChildIndex( parentIndex );
            var rightChildIndex = Heap.GetRightChildIndex( parentIndex );

            var leftChild = heap[leftChildIndex];
            var leftChildComparisonResult = parent.SendAt.CompareTo( leftChild.SendAt );

            assertions.Add(
                leftChildComparisonResult.TestLessThanOrEqualTo(
                    0,
                    $"parent[@{parentIndex}: '{parent}'] <=> left-child[@{leftChildIndex}: '{leftChild}']" ) );

            if ( rightChildIndex >= heap.Count )
                continue;

            var rightChild = heap[rightChildIndex];
            var rightChildComparisonResult = parent.SendAt.CompareTo( rightChild.SendAt );

            assertions.Add(
                rightChildComparisonResult.TestLessThanOrEqualTo(
                    0,
                    $"parent[@{parentIndex}: '{parent}'] <=> right-child[@{rightChildIndex}: '{rightChild}']" ) );
        }

        return Assertion.All( "HeapInvariant", assertions );
    }

    [Pure]
    private static async ValueTask<MessageBrokerRemoteClient> CreateClient(ITimestampProvider? timestamps = null)
    {
        var options = MessageBrokerServerOptions.Default;
        if ( timestamps is not null )
            options = options.SetTimestampsFactory( _ => timestamps );

        await using var server = new MessageBrokerServer( new IPEndPoint( IPAddress.Loopback, 0 ), options );
        await server.StartAsync();
        using var client = new ClientMock();
        await client.EstablishHandshake( server );
        var remoteClient = server.Clients.TryGetById( 1 );
        Assume.IsNotNull( remoteClient );
        return remoteClient;
    }

    [Pure]
    private static MessageBrokerChannel CreateChannel(MessageBrokerRemoteClient client)
    {
        return new MessageBrokerChannel( client.Server, 1, "foo" );
    }

    [Pure]
    private static MessageBrokerQueue CreateQueue(MessageBrokerRemoteClient client, int id)
    {
        return MessageBrokerQueue.Create( client, id, $"foo_{id}" );
    }

    [Pure]
    private static MessageBrokerChannelPublisherBinding CreatePublisher(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        var stream = new MessageBrokerStream( client.Server, 1, "foo" );
        return MessageBrokerChannelPublisherBinding.Create( client, channel, stream, true );
    }

    [Pure]
    private static MessageBrokerChannelListenerBinding CreateListener(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        var queue = CreateQueue( client, 1 );
        return MessageBrokerChannelListenerBinding.Create(
            client,
            channel,
            queue,
            new Protocol.BindListenerRequestHeader(
                0,
                1,
                1,
                Duration.Zero,
                0,
                Duration.FromSeconds( 1 ),
                1,
                Duration.FromSeconds( 3 ),
                0,
                0 ),
            null,
            null,
            true );
    }

    private sealed class TimestampProviderMock : TimestampProviderBase
    {
        internal Timestamp Next = Timestamp.Zero;

        public override Timestamp GetNow()
        {
            return Next;
        }
    }
}
