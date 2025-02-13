using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Reactive.Queues.Tests.EventQueueTests;

public abstract class GenericEventQueueTests<TEvent> : TestsBase
{
    protected GenericEventQueueTests()
    {
        Fixture.Customize<long>( (_, _) => _ => Random.Shared.Next( 0, int.MaxValue ) );
        Fixture.Customize<int>( (_, _) => _ => Random.Shared.Next( 0, byte.MaxValue ) );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 10 )]
    public void Ctor_ShouldCreateWithCorrectStartAndCurrentPoints(long startPoint)
    {
        var sut = new MockEventQueue( startPoint );

        Assertion.All(
                sut.StartPoint.TestEquals( startPoint ),
                sut.CurrentPoint.TestEquals( startPoint ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 10 )]
    public void Ctor_WithExplicitComparer_ShouldCreateWithCorrectStartAndCurrentPoints(long startPoint)
    {
        var comparer = Comparer<long>.Create( (a, b) => a.GetHashCode().CompareTo( b.GetHashCode() ) );
        var sut = new MockEventQueue( startPoint, comparer );

        Assertion.All(
                sut.StartPoint.TestEquals( startPoint ),
                sut.CurrentPoint.TestEquals( startPoint ),
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestRefEquals( comparer ) )
            .Go();
    }

    [Fact]
    public void Enqueue_ShouldAddFirstEventWithPointEqualToCurrentQueuePointPlusDelta()
    {
        var @event = Fixture.Create<TEvent>();
        var queueStartPoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var expectedDequeuePoint = queueStartPoint + delta;
        var sut = new MockEventQueue( queueStartPoint );

        var result = sut.Enqueue( @event, delta );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( expectedDequeuePoint ),
                result.Delta.TestEquals( default ),
                result.Repetitions.TestEquals( 1 ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
            .Go();
    }

    [Fact]
    public void Enqueue_ShouldAddAnotherEventWithPointEqualToCurrentQueuePointPlusDelta()
    {
        var (first, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var queueStartPoint = Fixture.Create<long>();
        var (firstDelta, eventDelta) = Fixture.CreateMany<int>( count: 2 ).ToList();
        var expectedEventDequeuePoint = queueStartPoint + eventDelta;
        var sut = new MockEventQueue( queueStartPoint );
        var firstEvent = sut.Enqueue( first, firstDelta );

        var result = sut.Enqueue( @event, eventDelta );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( expectedEventDequeuePoint ),
                result.Delta.TestEquals( default ),
                result.Repetitions.TestEquals( 1 ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 2 ),
                sut.TestSetEqual( [ firstEvent, result ] ) )
            .Go();
    }

    [Fact]
    public void EnqueueAt_ShouldAddFirstEvent()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.EnqueueAt( @event, dequeuePoint );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( dequeuePoint ),
                result.Delta.TestEquals( default ),
                result.Repetitions.TestEquals( 1 ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
            .Go();
    }

    [Fact]
    public void EnqueueAt_ShouldAddAnotherEvent()
    {
        var (first, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var (firstDequeuePoint, eventDequeuePoint) = Fixture.CreateMany<long>( count: 2 ).ToList();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var firstEvent = sut.EnqueueAt( first, firstDequeuePoint );

        var result = sut.EnqueueAt( @event, eventDequeuePoint );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( eventDequeuePoint ),
                result.Delta.TestEquals( default ),
                result.Repetitions.TestEquals( 1 ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 2 ),
                sut.TestSetEqual( [ firstEvent, result ] ) )
            .Go();
    }

    [Fact]
    public void Enqueue_WithRepetitions_ShouldAddFirstEventWithPointEqualToCurrentQueuePointPlusDelta()
    {
        var @event = Fixture.Create<TEvent>();
        var queueStartPoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var expectedDequeuePoint = queueStartPoint + delta;
        var sut = new MockEventQueue( queueStartPoint );

        var result = sut.Enqueue( @event, delta, repetitions );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( expectedDequeuePoint ),
                result.Delta.TestEquals( delta ),
                result.Repetitions.TestEquals( repetitions ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
            .Go();
    }

    [Fact]
    public void Enqueue_WithRepetitions_ShouldAddAnotherEventWithPointEqualToCurrentQueuePointPlusDelta()
    {
        var (first, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var queueStartPoint = Fixture.Create<long>();
        var (firstDelta, eventDelta) = Fixture.CreateMany<int>( count: 2 ).ToList();
        var (firstRepetitions, eventRepetitions) = (Fixture.Create<int>( x => x > 0 ), Fixture.Create<int>( x => x > 0 ));
        var expectedEventDequeuePoint = queueStartPoint + eventDelta;
        var sut = new MockEventQueue( queueStartPoint );
        var firstEvent = sut.Enqueue( first, firstDelta, firstRepetitions );

        var result = sut.Enqueue( @event, eventDelta, eventRepetitions );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( expectedEventDequeuePoint ),
                result.Delta.TestEquals( eventDelta ),
                result.Repetitions.TestEquals( eventRepetitions ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 2 ),
                sut.TestSetEqual( [ firstEvent, result ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Enqueue_WithRepetitions_ShouldThrowArgumentOutOfRangeException_WhenRepetitionsIsLessThanOne(int repetitions)
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var action = Lambda.Of( () => sut.Enqueue( Fixture.Create<TEvent>(), Fixture.Create<int>(), repetitions ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void EnqueueAt_WithRepetitions_ShouldAddFirstEvent()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.EnqueueAt( @event, dequeuePoint, delta, repetitions );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( dequeuePoint ),
                result.Delta.TestEquals( delta ),
                result.Repetitions.TestEquals( repetitions ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
            .Go();
    }

    [Fact]
    public void EnqueueAt_WithRepetitions_ShouldAddAnotherEvent()
    {
        var (first, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var queueStartPoint = Fixture.Create<long>();
        var (firstDequeuePoint, eventDequeuePoint) = Fixture.CreateMany<long>( count: 2 ).ToList();
        var (firstDelta, eventDelta) = Fixture.CreateMany<int>( count: 2 ).ToList();
        var (firstRepetitions, eventRepetitions) = (Fixture.Create<int>( x => x > 0 ), Fixture.Create<int>( x => x > 0 ));
        var sut = new MockEventQueue( queueStartPoint );
        var firstEvent = sut.EnqueueAt( first, firstDequeuePoint, firstDelta, firstRepetitions );

        var result = sut.EnqueueAt( @event, eventDequeuePoint, eventDelta, eventRepetitions );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( eventDequeuePoint ),
                result.Delta.TestEquals( eventDelta ),
                result.Repetitions.TestEquals( eventRepetitions ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 2 ),
                sut.TestSetEqual( [ firstEvent, result ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void EnqueueAt_WithRepetitions_ShouldThrowArgumentOutOfRangeException_WhenRepetitionsIsLessThanOne(int repetitions)
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var action = Lambda.Of(
            () => sut.EnqueueAt( Fixture.Create<TEvent>(), Fixture.Create<long>(), Fixture.Create<int>(), repetitions ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void EnqueueInfinite_ShouldAddFirstEventWithPointEqualToCurrentQueuePointPlusDelta()
    {
        var @event = Fixture.Create<TEvent>();
        var queueStartPoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var expectedDequeuePoint = queueStartPoint + delta;
        var sut = new MockEventQueue( queueStartPoint );

        var result = sut.EnqueueInfinite( @event, delta );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( expectedDequeuePoint ),
                result.Delta.TestEquals( delta ),
                result.Repetitions.TestEquals( 0 ),
                result.IsInfinite.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
            .Go();
    }

    [Fact]
    public void EnqueueInfinite_ShouldAddAnotherEventWithPointEqualToCurrentQueuePointPlusDelta()
    {
        var (first, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var queueStartPoint = Fixture.Create<long>();
        var (firstDelta, eventDelta) = Fixture.CreateMany<int>( count: 2 ).ToList();
        var expectedEventDequeuePoint = queueStartPoint + eventDelta;
        var sut = new MockEventQueue( queueStartPoint );
        var firstEvent = sut.EnqueueInfinite( first, firstDelta );

        var result = sut.EnqueueInfinite( @event, eventDelta );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( expectedEventDequeuePoint ),
                result.Delta.TestEquals( eventDelta ),
                result.Repetitions.TestEquals( 0 ),
                result.IsInfinite.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.TestSetEqual( [ firstEvent, result ] ) )
            .Go();
    }

    [Fact]
    public void EnqueueInfiniteAt_ShouldAddFirstEvent()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.EnqueueInfiniteAt( @event, dequeuePoint, delta );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( dequeuePoint ),
                result.Delta.TestEquals( delta ),
                result.Repetitions.TestEquals( 0 ),
                result.IsInfinite.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
            .Go();
    }

    [Fact]
    public void EnqueueInfiniteAt_ShouldAddAnotherEvent()
    {
        var (first, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var queueStartPoint = Fixture.Create<long>();
        var (firstDequeuePoint, eventDequeuePoint) = Fixture.CreateMany<long>( count: 2 ).ToList();
        var (firstDelta, eventDelta) = Fixture.CreateMany<int>( count: 2 ).ToList();
        var sut = new MockEventQueue( queueStartPoint );
        var firstEvent = sut.EnqueueInfiniteAt( first, firstDequeuePoint, firstDelta );

        var result = sut.EnqueueInfiniteAt( @event, eventDequeuePoint, eventDelta );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( eventDequeuePoint ),
                result.Delta.TestEquals( eventDelta ),
                result.Repetitions.TestEquals( 0 ),
                result.IsInfinite.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.TestSetEqual( [ firstEvent, result ] ) )
            .Go();
    }

    [Fact]
    public void Move_ShouldUpdateCurrentPoint()
    {
        var startPoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var expectedCurrentPoint = startPoint + delta;
        var sut = new MockEventQueue( startPoint );

        sut.Move( delta );

        sut.CurrentPoint.TestEquals( expectedCurrentPoint ).Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllEvents()
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.Enqueue( Fixture.Create<TEvent>(), Fixture.Create<int>() );
        sut.Enqueue( Fixture.Create<TEvent>(), Fixture.Create<int>() );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Dequeue_ShouldReturnNull_WhenQueueIsEmpty()
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var result = sut.Dequeue();
        result.TestNull().Go();
    }

    [Fact]
    public void Dequeue_ShouldReturnNull_WhenNextEventPointIsLargerThanCurrentQueuePoint()
    {
        var (queueStartPoint, eventDequeuePoint) = Fixture.CreateManyDistinctSorted<long>( count: 2 );
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueAt( Fixture.Create<TEvent>(), eventDequeuePoint );

        var result = sut.Dequeue();

        result.TestNull().Go();
    }

    [Fact]
    public void Dequeue_ShouldReturnNextEventAndRemoveIt_WhenNextEventPointIsLessThanOrEqualToCurrentQueuePointAndIsSingle()
    {
        var @event = Fixture.Create<TEvent>();
        var (eventDequeuePoint, queueStartPoint) = Fixture.CreateManyDistinctSorted<long>( count: 2 );
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueAt( @event, eventDequeuePoint );

        var result = sut.Dequeue();

        Assertion.All(
                result.TestNotNull(
                    r => Assertion.All(
                        "result.Value",
                        r.Event.TestEquals( @event ),
                        r.DequeuePoint.TestEquals( eventDequeuePoint ),
                        r.Delta.TestEquals( default ),
                        r.Repetitions.TestEquals( 1 ),
                        r.IsInfinite.TestFalse() ) ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Dequeue_ShouldReturnNextEventAndRemoveIt_WhenNextEventPointIsLessThanOrEqualToCurrentQueuePointAndHasOneRepetition()
    {
        var @event = Fixture.Create<TEvent>();
        var (eventDequeuePoint, queueStartPoint) = Fixture.CreateManyDistinctSorted<long>( count: 2 );
        var delta = Fixture.Create<int>();
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueAt( @event, eventDequeuePoint, delta, repetitions: 1 );

        var result = sut.Dequeue();

        Assertion.All(
                result.TestNotNull(
                    r => Assertion.All(
                        "result.Value",
                        r.Event.TestEquals( @event ),
                        r.DequeuePoint.TestEquals( eventDequeuePoint ),
                        r.Delta.TestEquals( delta ),
                        r.Repetitions.TestEquals( 1 ),
                        r.IsInfinite.TestFalse() ) ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void
        Dequeue_ShouldReturnNextEventAndReinsertIt_WhenNextEventPointIsLessThanOrEqualToCurrentQueuePointAndHasMoreThanOneRepetition()
    {
        var @event = Fixture.Create<TEvent>();
        var (eventDequeuePoint, queueStartPoint) = Fixture.CreateManyDistinctSorted<long>( count: 2 );
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.Create<int>( x => x > 0 ) + 1;
        var expectedNewDequeuePoint = eventDequeuePoint + delta;
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueAt( @event, eventDequeuePoint, delta, repetitions );

        var result = sut.Dequeue();

        Assertion.All(
                result.TestNotNull(
                    r => Assertion.All(
                        "result.Value",
                        r.Event.TestEquals( @event ),
                        r.DequeuePoint.TestEquals( eventDequeuePoint ),
                        r.Delta.TestEquals( delta ),
                        r.Repetitions.TestEquals( repetitions ),
                        r.IsInfinite.TestFalse() ) ),
                sut.TestAll(
                    (e, _) => Assertion.All(
                        e.Event.TestEquals( @event ),
                        e.DequeuePoint.TestEquals( expectedNewDequeuePoint ),
                        e.Delta.TestEquals( delta ),
                        e.Repetitions.TestEquals( repetitions - 1 ),
                        e.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void Dequeue_ShouldReturnNextEventAndReinsertIt_WhenNextEventPointIsLessThanOrEqualToCurrentQueuePointAndIsInfinite()
    {
        var @event = Fixture.Create<TEvent>();
        var (eventDequeuePoint, queueStartPoint) = Fixture.CreateManyDistinctSorted<long>( count: 2 );
        var delta = Fixture.Create<int>();
        var expectedNewDequeuePoint = eventDequeuePoint + delta;
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueInfiniteAt( @event, eventDequeuePoint, delta );

        var result = sut.Dequeue();

        Assertion.All(
                result.TestNotNull(
                    r => Assertion.All(
                        "result.Value",
                        r.Event.TestEquals( @event ),
                        r.DequeuePoint.TestEquals( eventDequeuePoint ),
                        r.Delta.TestEquals( delta ),
                        r.Repetitions.TestEquals( 0 ),
                        r.IsInfinite.TestTrue() ) ),
                sut.TestAll(
                    (e, _) => Assertion.All(
                        e.Event.TestEquals( @event ),
                        e.DequeuePoint.TestEquals( expectedNewDequeuePoint ),
                        e.Delta.TestEquals( delta ),
                        e.Repetitions.TestEquals( 0 ),
                        e.IsInfinite.TestTrue() ) ) )
            .Go();
    }

    [Fact]
    public void Dequeue_ShouldReturnEventsInAnOrderedWayUsingTheirDequeuePoints()
    {
        var events = Fixture.CreateManyDistinct<TEvent>( count: 10 );
        var firstDequeuePoints = Fixture.CreateManyDistinct<long>( count: 10 );
        var eventsWithDequeuePoints = events.Zip( firstDequeuePoints ).ToList();
        var allOrderedEventsWithDequeuePoints = eventsWithDequeuePoints.OrderBy( x => x.Second ).ToList();

        var result = new List<TEvent>();
        var expectedResult = allOrderedEventsWithDequeuePoints.Select( x => x.First );

        var sut = new MockEventQueue( allOrderedEventsWithDequeuePoints[^1].Second );

        foreach ( var (@event, dequeuePoint) in eventsWithDequeuePoints )
            sut.EnqueueAt( @event, dequeuePoint );

        var dequeuedEvent = sut.Dequeue();
        while ( dequeuedEvent is not null )
        {
            result.Add( dequeuedEvent.Value.Event );
            dequeuedEvent = sut.Dequeue();
        }

        result.TestSequence( expectedResult ).Go();
    }

    [Fact]
    public void GetNext_ShouldReturnNull_WhenQueueIsEmpty()
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var result = sut.GetNext();
        result.TestNull().Go();
    }

    [Fact]
    public void GetNext_ShouldReturnCorrectEnqueuedEvent_WhenQueueIsNotEmptyAndNextEventDequeuePointIsGreaterThanCurrentQueuePoint()
    {
        var (expectedEvent, otherEvent) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var (queuePoint, expectedPoint, otherPoint) = Fixture.CreateManyDistinctSorted<long>( count: 3 );
        var sut = new MockEventQueue( queuePoint );
        var expected = sut.EnqueueAt( expectedEvent, expectedPoint );
        sut.EnqueueAt( otherEvent, otherPoint );

        var result = sut.GetNext();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetNext_ShouldReturnCorrectEnqueuedEvent_WhenQueueIsNotEmptyAndNextEventDequeuePointIsLessThanCurrentQueuePoint()
    {
        var (expectedEvent, otherEvent) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var (expectedPoint, queuePoint, otherPoint) = Fixture.CreateManyDistinctSorted<long>( count: 3 );
        var sut = new MockEventQueue( queuePoint );
        var expected = sut.EnqueueAt( expectedEvent, expectedPoint );
        sut.EnqueueAt( otherEvent, otherPoint );

        var result = sut.GetNext();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetNext_ShouldReturnCorrectEnqueuedEvent_WhenQueueIsNotEmptyAndNextEventDequeuePointIsEqualToCurrentQueuePoint()
    {
        var (expectedEvent, otherEvent) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var (queuePoint, otherPoint) = Fixture.CreateManyDistinctSorted<long>( count: 2 );
        var sut = new MockEventQueue( queuePoint );
        var expected = sut.EnqueueAt( expectedEvent, queuePoint );
        sut.EnqueueAt( otherEvent, otherPoint );

        var result = sut.GetNext();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetEvents_ShouldNotIncludeAnEventWhoseDequeuePointExceedsEndPoint()
    {
        var @event = Fixture.Create<TEvent>();
        var (endPoint, dequeuePoint) = Fixture.CreateManyDistinctSorted<long>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint );

        var result = sut.GetEvents( endPoint );

        result.TestEmpty().Go();
    }

    [Fact]
    public void GetEvents_ShouldIncludeASingleEventWhoseDequeuePointDoesNotExceedEndPoint()
    {
        var @event = Fixture.Create<TEvent>();
        var (dequeuePoint, endPoint) = Fixture.CreateManyDistinctSorted<long>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint );

        var result = sut.GetEvents( endPoint );

        result.TestCount( count => count.TestEquals( 1 ) )
            .Then(
                e =>
                {
                    var r = e[0];
                    return Assertion.All(
                        "result",
                        r.Event.TestEquals( @event ),
                        r.DequeuePoint.TestEquals( dequeuePoint ),
                        r.Delta.TestEquals( default ),
                        r.Repetitions.TestEquals( 1 ),
                        r.IsInfinite.TestFalse() );
                } )
            .Go();
    }

    [Fact]
    public void GetEvents_ShouldIncludeAllRepeatableEventInstancesWhenAllItsDequeuePointsDoNotExceedEndPoint()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>( x => x > 0 );
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var endPoint = dequeuePoint + delta * (repetitions - 1);
        var expected = new[]
        {
            new
            {
                Event = @event,
                DequeuePoint = dequeuePoint,
                Delta = delta,
                Repetitions = repetitions,
                IsInfinite = false
            }
        }.AsEnumerable();

        for ( var r = 1; r < repetitions; ++r )
        {
            expected = expected.Append(
                new
                {
                    Event = @event,
                    DequeuePoint = dequeuePoint + delta * r,
                    Delta = delta,
                    Repetitions = repetitions - r,
                    IsInfinite = false
                } );
        }

        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, delta, repetitions );

        var result = sut.GetEvents( endPoint );

        result.TestAll(
                (r, i) =>
                {
                    var values = expected.ElementAt( i );
                    return Assertion.All(
                        "result",
                        r.Event.TestEquals( values.Event ),
                        r.DequeuePoint.TestEquals( values.DequeuePoint ),
                        r.Delta.TestEquals( values.Delta ),
                        r.Repetitions.TestEquals( values.Repetitions ),
                        r.IsInfinite.TestEquals( values.IsInfinite ) );
                } )
            .Go();
    }

    [Fact]
    public void GetEvents_ShouldIncludeSomeRepeatableEventInstancesWhenSomeOfItsDequeuePointsDoNotExceedEndPoint()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>( x => x > 0 );
        var repetitions = Fixture.Create<int>( x => x > 0 ) + 3;
        var endPoint = dequeuePoint + delta * 2;
        var expected = new[]
        {
            new
            {
                Event = @event,
                DequeuePoint = dequeuePoint,
                Delta = delta,
                Repetitions = repetitions,
                IsInfinite = false
            },
            new
            {
                Event = @event,
                DequeuePoint = dequeuePoint + delta,
                Delta = delta,
                Repetitions = repetitions - 1,
                IsInfinite = false
            },
            new
            {
                Event = @event,
                DequeuePoint = dequeuePoint + delta * 2,
                Delta = delta,
                Repetitions = repetitions - 2,
                IsInfinite = false
            }
        }.AsEnumerable();

        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, delta, repetitions );

        var result = sut.GetEvents( endPoint );

        result.TestAll(
                (r, i) =>
                {
                    var values = expected.ElementAt( i );
                    return Assertion.All(
                        "result",
                        r.Event.TestEquals( values.Event ),
                        r.DequeuePoint.TestEquals( values.DequeuePoint ),
                        r.Delta.TestEquals( values.Delta ),
                        r.Repetitions.TestEquals( values.Repetitions ),
                        r.IsInfinite.TestEquals( values.IsInfinite ) );
                } )
            .Go();
    }

    [Fact]
    public void GetEvents_ShouldIncludeInfiniteEventInstancesUpUntilEndPoint()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>( x => x > 0 );
        var endPoint = dequeuePoint + delta * 2;
        var expected = new[]
        {
            new
            {
                Event = @event,
                DequeuePoint = dequeuePoint,
                Delta = delta,
                Repetitions = 0,
                IsInfinite = true
            },
            new
            {
                Event = @event,
                DequeuePoint = dequeuePoint + delta,
                Delta = delta,
                Repetitions = 0,
                IsInfinite = true
            },
            new
            {
                Event = @event,
                DequeuePoint = dequeuePoint + delta * 2,
                Delta = delta,
                Repetitions = 0,
                IsInfinite = true
            }
        }.AsEnumerable();

        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueInfiniteAt( @event, dequeuePoint, delta );

        var result = sut.GetEvents( endPoint );

        result.TestAll(
                (r, i) =>
                {
                    var values = expected.ElementAt( i );
                    return Assertion.All(
                        "result",
                        r.Event.TestEquals( values.Event ),
                        r.DequeuePoint.TestEquals( values.DequeuePoint ),
                        r.Delta.TestEquals( values.Delta ),
                        r.Repetitions.TestEquals( values.Repetitions ),
                        r.IsInfinite.TestEquals( values.IsInfinite ) );
                } )
            .Go();
    }

    [Fact]
    public void GetEvents_ShouldReturnEventsWhoseDequeuePointsDoNotExceedEndPointInAnUnorderedWay()
    {
        var events = Fixture.CreateManyDistinct<TEvent>( count: 10 );
        var firstDequeuePoints = Fixture.CreateManyDistinct<long>( count: 10 );
        var eventsWithDequeuePoints = events.Zip( firstDequeuePoints ).ToList();
        var expectedResult = eventsWithDequeuePoints.Select(
                x => new
                {
                    Event = x.First,
                    DequeuePoint = x.Second,
                    Delta = default( int ),
                    Repetitions = 1,
                    IsInfinite = false
                } )
            .OrderBy( x => x.DequeuePoint )
            .ToList();

        var sut = new MockEventQueue( Fixture.Create<long>() );

        foreach ( var (@event, dequeuePoint) in eventsWithDequeuePoints )
            sut.EnqueueAt( @event, dequeuePoint );

        var result = sut.GetEvents( eventsWithDequeuePoints.Select( x => x.Second ).Max() ).OrderBy( e => e.DequeuePoint );

        result.TestAll(
                (r, i) =>
                {
                    var values = expectedResult.ElementAt( i );
                    return Assertion.All(
                        "result",
                        r.Event.TestEquals( values.Event ),
                        r.DequeuePoint.TestEquals( values.DequeuePoint ),
                        r.Delta.TestEquals( values.Delta ),
                        r.Repetitions.TestEquals( values.Repetitions ),
                        r.IsInfinite.TestEquals( values.IsInfinite ) );
                } )
            .Go();
    }

    private sealed class MockEventQueue : EventQueueBase<TEvent, long, int>
    {
        public MockEventQueue(long startPoint)
            : base( startPoint ) { }

        public MockEventQueue(long startPoint, IComparer<long> comparer)
            : base( startPoint, comparer ) { }

        protected override long AddDelta(long point, int delta)
        {
            return point + delta;
        }
    }
}
