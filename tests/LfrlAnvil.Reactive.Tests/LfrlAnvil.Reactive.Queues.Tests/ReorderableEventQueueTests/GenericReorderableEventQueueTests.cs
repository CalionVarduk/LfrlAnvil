using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Reactive.Queues.Tests.ReorderableEventQueueTests;

public abstract class GenericReorderableEventQueueTests<TEvent> : TestsBase
    where TEvent : notnull
{
    protected GenericReorderableEventQueueTests()
    {
        Fixture.Customize<long>( (_, _) => _ => Random.Shared.NextInt64( 0, int.MaxValue ) );
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
        var eventComparer = EqualityComparerFactory<TEvent>.Create( (a, b) => a!.GetHashCode() == b!.GetHashCode() );
        var comparer = Comparer<long>.Create( (a, b) => a.GetHashCode().CompareTo( b.GetHashCode() ) );
        var sut = new MockEventQueue( startPoint, eventComparer, comparer );

        Assertion.All(
                sut.StartPoint.TestEquals( startPoint ),
                sut.CurrentPoint.TestEquals( startPoint ),
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestRefEquals( comparer ),
                sut.EventComparer.TestRefEquals( eventComparer ) )
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
    public void Enqueue_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var queueStartPoint = Fixture.Create<long>();
        var (oldDelta, newDelta) = Fixture.CreateManyDistinct<int>( count: 2 );
        var expectedDequeuePoint = queueStartPoint + newDelta;
        var sut = new MockEventQueue( queueStartPoint );
        sut.Enqueue( @event, oldDelta );

        var result = sut.Enqueue( @event, newDelta );

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
    public void EnqueueAt_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var (oldDequeuePoint, newDequeuePoint) = Fixture.CreateManyDistinct<long>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, oldDequeuePoint );

        var result = sut.EnqueueAt( @event, newDequeuePoint );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( newDequeuePoint ),
                result.Delta.TestEquals( default ),
                result.Repetitions.TestEquals( 1 ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
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

    [Fact]
    public void Enqueue_WithRepetitions_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var queueStartPoint = Fixture.Create<long>();
        var (oldDelta, newDelta) = Fixture.CreateManyDistinct<int>( count: 2 );
        var (oldRepetitions, newRepetitions) = (Fixture.Create<int>( x => x > 0 ), Fixture.Create<int>( x => x > 0 ));
        var expectedDequeuePoint = queueStartPoint + newDelta;
        var sut = new MockEventQueue( queueStartPoint );
        sut.Enqueue( @event, oldDelta, oldRepetitions );

        var result = sut.Enqueue( @event, newDelta, newRepetitions );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( expectedDequeuePoint ),
                result.Delta.TestEquals( newDelta ),
                result.Repetitions.TestEquals( newRepetitions ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
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

    [Fact]
    public void EnqueueAt_WithRepetitions_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var (oldDequeuePoint, newDequeuePoint) = Fixture.CreateManyDistinct<long>( count: 2 );
        var (oldDelta, newDelta) = Fixture.CreateManyDistinct<int>( count: 2 );
        var (oldRepetitions, newRepetitions) = (Fixture.Create<int>( x => x > 0 ), Fixture.Create<int>( x => x > 0 ));
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, oldDequeuePoint, oldDelta, oldRepetitions );

        var result = sut.EnqueueAt( @event, newDequeuePoint, newDelta, newRepetitions );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( newDequeuePoint ),
                result.Delta.TestEquals( newDelta ),
                result.Repetitions.TestEquals( newRepetitions ),
                result.IsInfinite.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void EnqueueAt_WithRepetitions_ShouldThrowArgumentOutOfRangeException_WhenRepetitionsIsLessThanOne(int repetitions)
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var action
            = Lambda.Of( () => sut.EnqueueAt( Fixture.Create<TEvent>(), Fixture.Create<long>(), Fixture.Create<int>(), repetitions ) );

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
    public void EnqueueInfinite_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var queueStartPoint = Fixture.Create<long>();
        var (oldDelta, newDelta) = Fixture.CreateManyDistinct<int>( count: 2 );
        var expectedDequeuePoint = queueStartPoint + newDelta;
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueInfinite( @event, oldDelta );

        var result = sut.EnqueueInfinite( @event, newDelta );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( expectedDequeuePoint ),
                result.Delta.TestEquals( newDelta ),
                result.Repetitions.TestEquals( 0 ),
                result.IsInfinite.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
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
    public void EnqueueInfiniteAt_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var (oldDequeuePoint, newDequeuePoint) = Fixture.CreateManyDistinct<long>( count: 2 );
        var (oldDelta, newDelta) = Fixture.CreateManyDistinct<int>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueInfiniteAt( @event, oldDequeuePoint, oldDelta );

        var result = sut.EnqueueInfiniteAt( @event, newDequeuePoint, newDelta );

        Assertion.All(
                result.Event.TestEquals( @event ),
                result.DequeuePoint.TestEquals( newDequeuePoint ),
                result.Delta.TestEquals( newDelta ),
                result.Repetitions.TestEquals( 0 ),
                result.IsInfinite.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.TestSetEqual( [ result ] ) )
            .Go();
    }

    [Fact]
    public void SetDequeuePoint_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.SetDequeuePoint( @event, Fixture.Create<long>() );

        result.TestNull().Go();
    }

    [Fact]
    public void SetDequeuePoint_ShouldUpdateEvent_WhenEventExists()
    {
        var (other, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var (newDequeuePoint, otherDequeuePoint, oldDequeuePoint) = Fixture.CreateManyDistinctSorted<long>( count: 3 );
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( other, otherDequeuePoint );
        sut.EnqueueAt( @event, oldDequeuePoint, delta, repetitions );

        var result = sut.SetDequeuePoint( @event, newDequeuePoint );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                sut.GetNext().TestEquals( result ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( newDequeuePoint ),
                    r.Delta.TestEquals( delta ),
                    r.Repetitions.TestEquals( repetitions ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void DelayDequeuePoint_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.DelayDequeuePoint( @event, Fixture.Create<int>() );

        result.TestNull().Go();
    }

    [Fact]
    public void DelayDequeuePoint_ShouldUpdateEvent_WhenEventExists()
    {
        var (other, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var (oldDequeuePoint, otherDequeuePoint) = Fixture.CreateManyDistinctSorted<long>( count: 2 );
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var delay = ( int )(otherDequeuePoint - oldDequeuePoint) + 1;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var enqueuedOther = sut.EnqueueAt( other, otherDequeuePoint );
        sut.EnqueueAt( @event, oldDequeuePoint, delta, repetitions );

        var result = sut.DelayDequeuePoint( @event, delay );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                sut.GetNext().TestEquals( enqueuedOther ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( otherDequeuePoint + 1 ),
                    r.Delta.TestEquals( delta ),
                    r.Repetitions.TestEquals( repetitions ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void AdvanceDequeuePoint_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.AdvanceDequeuePoint( @event, Fixture.Create<int>() );

        result.TestNull().Go();
    }

    [Fact]
    public void AdvanceDequeuePoint_ShouldUpdateEvent_WhenEventExists()
    {
        var (other, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var (otherDequeuePoint, oldDequeuePoint) = Fixture.CreateManyDistinctSorted<long>( count: 2 );
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var advance = ( int )(oldDequeuePoint - otherDequeuePoint) + 1;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( other, otherDequeuePoint );
        sut.EnqueueAt( @event, oldDequeuePoint, delta, repetitions );

        var result = sut.AdvanceDequeuePoint( @event, advance );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                sut.GetNext().TestEquals( result ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( otherDequeuePoint - 1 ),
                    r.Delta.TestEquals( delta ),
                    r.Repetitions.TestEquals( repetitions ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void SetDelta_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.SetDelta( @event, Fixture.Create<int>() );

        result.TestNull().Go();
    }

    [Fact]
    public void SetDelta_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var (oldDelta, newDelta) = Fixture.CreateManyDistinct<int>( count: 2 );
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, oldDelta, repetitions );

        var result = sut.SetDelta( @event, newDelta );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( dequeuePoint ),
                    r.Delta.TestEquals( newDelta ),
                    r.Repetitions.TestEquals( repetitions ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void IncreaseDelta_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.IncreaseDelta( @event, Fixture.Create<int>() );

        result.TestNull().Go();
    }

    [Fact]
    public void IncreaseDelta_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var oldDelta = Fixture.Create<int>();
        var deltaIncrease = Fixture.Create<int>();
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var expectedDelta = oldDelta + deltaIncrease;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, oldDelta, repetitions );

        var result = sut.IncreaseDelta( @event, deltaIncrease );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( dequeuePoint ),
                    r.Delta.TestEquals( expectedDelta ),
                    r.Repetitions.TestEquals( repetitions ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void DecreaseDelta_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.DecreaseDelta( @event, Fixture.Create<int>() );

        result.TestNull().Go();
    }

    [Fact]
    public void DecreaseDelta_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var oldDelta = Fixture.Create<int>();
        var deltaDecrease = Fixture.Create<int>();
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var expectedDelta = oldDelta - deltaDecrease;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, oldDelta, repetitions );

        var result = sut.DecreaseDelta( @event, deltaDecrease );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( dequeuePoint ),
                    r.Delta.TestEquals( expectedDelta ),
                    r.Repetitions.TestEquals( repetitions ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void SetRepetitions_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.SetRepetitions( @event, Fixture.Create<int>( x => x > 0 ) );

        result.TestNull().Go();
    }

    [Fact]
    public void SetRepetitions_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var (oldRepetitions, newRepetitions) = (Fixture.Create<int>( x => x > 0 ), Fixture.Create<int>( x => x > 0 ));
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, delta, oldRepetitions );

        var result = sut.SetRepetitions( @event, newRepetitions );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( dequeuePoint ),
                    r.Delta.TestEquals( delta ),
                    r.Repetitions.TestEquals( newRepetitions ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void SetRepetitions_ShouldUpdateEvent_WhenEventExistsAndIsInfinite()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueInfiniteAt( @event, dequeuePoint, delta );

        var result = sut.SetRepetitions( @event, repetitions );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( dequeuePoint ),
                    r.Delta.TestEquals( delta ),
                    r.Repetitions.TestEquals( repetitions ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void SetRepetitions_ShouldThrowArgumentOutOfRangeException_WhenValueIsLessThanOne(int repetitions)
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, Fixture.Create<long>() );

        var action = Lambda.Of( () => sut.SetRepetitions( @event, repetitions ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void IncreaseRepetitions_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.IncreaseRepetitions( @event, Fixture.Create<int>( x => x > 0 ) );

        result.TestNull().Go();
    }

    [Fact]
    public void IncreaseRepetitions_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var oldRepetitions = Fixture.Create<int>( x => x > 0 );
        var repetitionsIncrease = Fixture.Create<int>( x => x > 0 );
        var expectedRepetitions = oldRepetitions + repetitionsIncrease;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, delta, oldRepetitions );

        var result = sut.IncreaseRepetitions( @event, repetitionsIncrease );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( dequeuePoint ),
                    r.Delta.TestEquals( delta ),
                    r.Repetitions.TestEquals( expectedRepetitions ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void IncreaseRepetitions_ShouldReturnUnchangedEvent_WhenEventExistsAndIsInfinite()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var oldEnqueuedEvent = sut.EnqueueInfiniteAt( @event, dequeuePoint, delta );

        var result = sut.IncreaseRepetitions( @event, Fixture.Create<int>( x => x > 0 ) );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestEquals( oldEnqueuedEvent ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void IncreaseRepetitions_ShouldThrowArgumentOutOfRangeException_WhenNewRepetitionsIsLessThanOne(int targetRepetitions)
    {
        var @event = Fixture.Create<TEvent>();
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var repetitionsIncrease = targetRepetitions - repetitions;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.Enqueue( @event, Fixture.Create<int>(), repetitions );

        var action = Lambda.Of( () => sut.IncreaseRepetitions( @event, repetitionsIncrease ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void DecreaseRepetitions_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.DecreaseRepetitions( @event, Fixture.Create<int>( x => x > 0 ) );

        result.TestNull().Go();
    }

    [Fact]
    public void DecreaseRepetitions_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var oldRepetitions = Fixture.Create<int>( x => x > 0 );
        var repetitionsDecrease = -Fixture.Create<int>( x => x > 0 );
        var expectedRepetitions = oldRepetitions - repetitionsDecrease;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, delta, oldRepetitions );

        var result = sut.DecreaseRepetitions( @event, repetitionsDecrease );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( dequeuePoint ),
                    r.Delta.TestEquals( delta ),
                    r.Repetitions.TestEquals( expectedRepetitions ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void DecreaseRepetitions_ShouldReturnUnchangedEvent_WhenEventExistsAndIsInfinite()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var oldEnqueuedEvent = sut.EnqueueInfiniteAt( @event, dequeuePoint, delta );

        var result = sut.DecreaseRepetitions( @event, Fixture.Create<int>( x => x > 0 ) );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestEquals( oldEnqueuedEvent ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void DecreaseRepetitions_ShouldThrowArgumentOutOfRangeException_WhenNewRepetitionsIsLessThanOne(int targetRepetitions)
    {
        var @event = Fixture.Create<TEvent>();
        var repetitions = Fixture.Create<int>( x => x > 0 );
        var repetitionsDecrease = repetitions - targetRepetitions;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.Enqueue( @event, Fixture.Create<int>(), repetitions );

        var action = Lambda.Of( () => sut.DecreaseRepetitions( @event, repetitionsDecrease ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void MakeInfinite_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.MakeInfinite( @event );

        result.TestNull().Go();
    }

    [Fact]
    public void MakeInfinite_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var oldRepetitions = Fixture.Create<int>( x => x > 0 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, delta, oldRepetitions );

        var result = sut.MakeInfinite( @event );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( dequeuePoint ),
                    r.Delta.TestEquals( delta ),
                    r.Repetitions.TestEquals( 0 ),
                    r.IsInfinite.TestTrue() ) ) )
            .Go();
    }

    [Fact]
    public void MakeInfinite_ShouldReturnUnchangedEvent_WhenEventExistsAndIsInfinite()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var oldEnqueuedEvent = sut.EnqueueInfiniteAt( @event, dequeuePoint, delta );

        var result = sut.MakeInfinite( @event );

        Assertion.All(
                sut.GetEvent( @event ).TestEquals( result ),
                result.TestEquals( oldEnqueuedEvent ) )
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
                result.TestNotNull( r => Assertion.All(
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
                result.TestNotNull( r => Assertion.All(
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
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( eventDequeuePoint ),
                    r.Delta.TestEquals( delta ),
                    r.Repetitions.TestEquals( repetitions ),
                    r.IsInfinite.TestFalse() ) ),
                sut.TestAll( (e, _) => Assertion.All(
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
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( eventDequeuePoint ),
                    r.Delta.TestEquals( delta ),
                    r.Repetitions.TestEquals( 0 ),
                    r.IsInfinite.TestTrue() ) ),
                sut.TestAll( (e, _) => Assertion.All(
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
    public void Remove_ShouldReturnCorrectResultAndRemoveEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint );

        var result = sut.Remove( @event );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                result.TestNotNull( r => Assertion.All(
                    "result.Value",
                    r.Event.TestEquals( @event ),
                    r.DequeuePoint.TestEquals( dequeuePoint ),
                    r.Delta.TestEquals( default ),
                    r.Repetitions.TestEquals( 1 ),
                    r.IsInfinite.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var (other, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( other, dequeuePoint: Fixture.Create<long>() );

        var result = sut.Remove( @event );

        result.TestNull().Go();
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenEventIsQueued()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint: Fixture.Create<long>() );

        var result = sut.Contains( @event );

        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenEventIsNotQueued()
    {
        var (other, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( other, dequeuePoint: Fixture.Create<long>() );

        var result = sut.Contains( @event );

        result.TestFalse().Go();
    }

    [Fact]
    public void GetEvent_ShouldReturnCorrectResult_WhenEventIsQueued()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint );

        var result = sut.GetEvent( @event );

        result.TestNotNull( r => Assertion.All(
                "result.Value",
                r.Event.TestEquals( @event ),
                r.DequeuePoint.TestEquals( dequeuePoint ),
                r.Delta.TestEquals( default ),
                r.Repetitions.TestEquals( 1 ),
                r.IsInfinite.TestFalse() ) )
            .Go();
    }

    [Fact]
    public void Contains_ShouldReturnNull_WhenEventIsNotQueued()
    {
        var (other, @event) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( other, dequeuePoint: Fixture.Create<long>() );

        var result = sut.GetEvent( @event );

        result.TestNull().Go();
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
            .Then( e =>
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

        result.TestAll( (r, i) =>
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

        result.TestAll( (r, i) =>
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

        result.TestAll( (r, i) =>
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
        var expectedResult = eventsWithDequeuePoints.Select( x => new
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

        result.TestAll( (r, i) =>
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

    private sealed class MockEventQueue : ReorderableEventQueueBase<TEvent, long, int>
    {
        public MockEventQueue(long startPoint)
            : base( startPoint ) { }

        public MockEventQueue(long startPoint, IEqualityComparer<TEvent> eventComparer, IComparer<long> comparer)
            : base( startPoint, eventComparer, comparer ) { }

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
