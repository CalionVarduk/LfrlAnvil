using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues.Tests.QueueEventSourceTests;

public abstract class GenericQueueEventSourceTests<TEvent> : TestsBase
{
    protected GenericQueueEventSourceTests()
    {
        Fixture.Customize<int>( (_, _) => _ => Random.Shared.Next( 0, short.MaxValue ) );
        Fixture.Customize<long>( (_, _) => _ => Random.Shared.Next( 0, int.MaxValue ) );
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectQueue()
    {
        var queue = new MockEventQueue( Fixture.Create<long>() );
        var sut = new QueueEventSource<TEvent, long, int>( queue );
        sut.Queue.TestRefEquals( queue ).Go();
    }

    [Fact]
    public void Create_ShouldCreateWithCorrectQueue()
    {
        var queue = new MockEventQueue( Fixture.Create<long>() );
        var sut = QueueEventSource.Create( queue );
        sut.Queue.TestRefEquals( queue ).Go();
    }

    [Fact]
    public void Move_ShouldMoveUnderlyingQueueAndDequeueAndEmitAllPendingEvents()
    {
        var (queueStartPoint, firstDequeuePoint, secondDequeuePoint, thirdDequeuePoint) =
            Fixture.CreateManyDistinctSorted<long>( count: 4 );

        var (firstDelta, secondDelta, thirdDelta) = Fixture.CreateManyDistinct<int>( count: 3 );
        var (firstEvent, secondEvent, thirdEvent) = Fixture.CreateManyDistinct<TEvent>( count: 3 );
        var delta = ( int )(thirdDequeuePoint - queueStartPoint);

        var receivedEvents = new List<FromQueue<TEvent, long, int>>();
        var next = EventListener.Create<FromQueue<TEvent, long, int>>( receivedEvents.Add );
        var queue = new MockEventQueue( queueStartPoint );
        var sut = new QueueEventSource<TEvent, long, int>( queue );
        sut.Listen( next );

        var firstEnqueued = sut.Queue.EnqueueAt( firstEvent, firstDequeuePoint, firstDelta, repetitions: 1 );
        var secondEnqueued = sut.Queue.EnqueueAt( secondEvent, secondDequeuePoint, secondDelta, repetitions: 1 );
        var thirdEnqueued = sut.Queue.EnqueueAt( thirdEvent, thirdDequeuePoint, thirdDelta, repetitions: 1 );

        sut.Move( delta );

        Assertion.All(
                sut.Queue.TestEmpty(),
                sut.Queue.CurrentPoint.TestEquals( thirdDequeuePoint ),
                receivedEvents.TestSequence(
                [
                    new FromQueue<TEvent, long, int>( firstEnqueued, thirdDequeuePoint, delta ),
                    new FromQueue<TEvent, long, int>( secondEnqueued, thirdDequeuePoint, delta ),
                    new FromQueue<TEvent, long, int>( thirdEnqueued, thirdDequeuePoint, delta )
                ] ) )
            .Go();
    }

    [Fact]
    public void Move_ShouldThrowObjectDisposedException_WhenEventSourceIsDisposed()
    {
        var queue = new MockEventQueue( Fixture.Create<long>() );
        var sut = new QueueEventSource<TEvent, long, int>( queue );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Move( Fixture.Create<int>() ) );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void Dispose_ShouldClearUnderlyingQueue()
    {
        var queue = new MockEventQueue( Fixture.Create<long>() );
        queue.EnqueueAt( Fixture.Create<TEvent>(), Fixture.Create<long>() );
        var sut = new QueueEventSource<TEvent, long, int>( queue );

        sut.Dispose();

        sut.Queue.TestEmpty().Go();
    }

    private sealed class MockEventQueue : EventQueueBase<TEvent, long, int>
    {
        public MockEventQueue(long startPoint)
            : base( startPoint ) { }

        protected override long AddDelta(long point, int delta)
        {
            return point + delta;
        }
    }
}
