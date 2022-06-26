using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Queues.Composites;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Reactive.Queues.Tests.ReorderableQueueEventSourceTests;

public abstract class GenericReorderableQueueEventSourceTests<TEvent> : TestsBase
    where TEvent : notnull
{
    [Fact]
    public void Ctor_ShouldCreateWithCorrectQueue()
    {
        var queue = new MockEventQueue( Fixture.Create<long>() );
        var sut = new ReorderableQueueEventSource<TEvent, long, int>( queue );
        sut.Queue.Should().BeSameAs( queue );
    }

    [Fact]
    public void Create_ShouldCreateWithCorrectQueue()
    {
        var queue = new MockEventQueue( Fixture.Create<long>() );
        var sut = QueueEventSource.Create( queue );
        sut.Queue.Should().BeSameAs( queue );
    }

    [Fact]
    public void Move_ShouldMoveUnderlyingQueueAndDequeueAndEmitAllPendingEvents()
    {
        var (queueStartPoint, firstDequeuePoint, secondDequeuePoint, thirdDequeuePoint) =
            Fixture.CreateDistinctSortedCollection<long>( count: 4 );

        var (firstDelta, secondDelta, thirdDelta) = Fixture.CreateDistinctCollection<int>( count: 3 );
        var (firstEvent, secondEvent, thirdEvent) = Fixture.CreateDistinctCollection<TEvent>( count: 3 );
        var delta = (int)(thirdDequeuePoint - queueStartPoint);

        var receivedEvents = new List<FromQueue<TEvent, long, int>>();
        var next = EventListener.Create<FromQueue<TEvent, long, int>>( receivedEvents.Add );
        var queue = new MockEventQueue( queueStartPoint );
        var sut = new ReorderableQueueEventSource<TEvent, long, int>( queue );
        sut.Listen( next );

        var firstEnqueued = sut.Queue.EnqueueAt( firstEvent, firstDequeuePoint, firstDelta, repetitions: 1 );
        var secondEnqueued = sut.Queue.EnqueueAt( secondEvent, secondDequeuePoint, secondDelta, repetitions: 1 );
        var thirdEnqueued = sut.Queue.EnqueueAt( thirdEvent, thirdDequeuePoint, thirdDelta, repetitions: 1 );

        sut.Move( delta );

        using ( new AssertionScope() )
        {
            sut.Queue.Should().BeEmpty();
            sut.Queue.CurrentPoint.Should().Be( thirdDequeuePoint );
            receivedEvents.Should()
                .BeSequentiallyEqualTo(
                    new FromQueue<TEvent, long, int>( firstEnqueued, thirdDequeuePoint, delta ),
                    new FromQueue<TEvent, long, int>( secondEnqueued, thirdDequeuePoint, delta ),
                    new FromQueue<TEvent, long, int>( thirdEnqueued, thirdDequeuePoint, delta ) );
        }
    }

    [Fact]
    public void Move_ShouldThrowObjectDisposedException_WhenEventSourceIsDisposed()
    {
        var queue = new MockEventQueue( Fixture.Create<long>() );
        var sut = new ReorderableQueueEventSource<TEvent, long, int>( queue );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Move( Fixture.Create<int>() ) );

        action.Should().ThrowExactly<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_ShouldClearUnderlyingQueue()
    {
        var queue = new MockEventQueue( Fixture.Create<long>() );
        queue.EnqueueAt( Fixture.Create<TEvent>(), Fixture.Create<long>() );
        var sut = new ReorderableQueueEventSource<TEvent, long, int>( queue );

        sut.Dispose();

        sut.Queue.Should().BeEmpty();
    }

    private sealed class MockEventQueue : ReorderableEventQueueBase<TEvent, long, int>
    {
        public MockEventQueue(long startPoint)
            : base( startPoint ) { }

        protected override long AddDelta(long point, int delta)
        {
            return point + delta;
        }

        protected override long SubtractDelta(long point, int delta)
        {
            return point - delta;
        }

        protected override int Add(int a, int b)
        {
            return a + b;
        }

        protected override int Subtract(int a, int b)
        {
            return a - b;
        }
    }
}
