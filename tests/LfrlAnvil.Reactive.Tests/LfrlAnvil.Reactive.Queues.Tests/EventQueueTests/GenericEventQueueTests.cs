using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Reactive.Queues.Tests.EventQueueTests;

public abstract class GenericEventQueueTests<TEvent> : TestsBase
{
    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 10 )]
    public void Ctor_ShouldCreateWithCorrectStartAndCurrentPoints(long startPoint)
    {
        var sut = new MockEventQueue( startPoint );

        using ( new AssertionScope() )
        {
            sut.StartPoint.Should().Be( startPoint );
            sut.CurrentPoint.Should().Be( startPoint );
            sut.Count.Should().Be( 0 );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 10 )]
    public void Ctor_WithExplicitComparer_ShouldCreateWithCorrectStartAndCurrentPoints(long startPoint)
    {
        var comparer = Comparer<long>.Create( (a, b) => a.GetHashCode().CompareTo( b.GetHashCode() ) );
        var sut = new MockEventQueue( startPoint, comparer );

        using ( new AssertionScope() )
        {
            sut.StartPoint.Should().Be( startPoint );
            sut.CurrentPoint.Should().Be( startPoint );
            sut.Count.Should().Be( 0 );
            sut.Comparer.Should().BeSameAs( comparer );
        }
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

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( expectedDequeuePoint );
            result.Delta.Should().Be( default );
            result.Repetitions.Should().Be( 1 );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
        }
    }

    [Fact]
    public void Enqueue_ShouldAddAnotherEventWithPointEqualToCurrentQueuePointPlusDelta()
    {
        var (first, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var queueStartPoint = Fixture.Create<long>();
        var (firstDelta, eventDelta) = Fixture.CreateMany<int>( count: 2 ).ToList();
        var expectedEventDequeuePoint = queueStartPoint + eventDelta;
        var sut = new MockEventQueue( queueStartPoint );
        var firstEvent = sut.Enqueue( first, firstDelta );

        var result = sut.Enqueue( @event, eventDelta );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( expectedEventDequeuePoint );
            result.Delta.Should().Be( default );
            result.Repetitions.Should().Be( 1 );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 2 );
            sut.Should().BeEquivalentTo( firstEvent, result );
        }
    }

    [Fact]
    public void EnqueueAt_ShouldAddFirstEvent()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.EnqueueAt( @event, dequeuePoint );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( dequeuePoint );
            result.Delta.Should().Be( default );
            result.Repetitions.Should().Be( 1 );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
        }
    }

    [Fact]
    public void EnqueueAt_ShouldAddAnotherEvent()
    {
        var (first, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var (firstDequeuePoint, eventDequeuePoint) = Fixture.CreateMany<long>( count: 2 ).ToList();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var firstEvent = sut.EnqueueAt( first, firstDequeuePoint );

        var result = sut.EnqueueAt( @event, eventDequeuePoint );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( eventDequeuePoint );
            result.Delta.Should().Be( default );
            result.Repetitions.Should().Be( 1 );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 2 );
            sut.Should().BeEquivalentTo( firstEvent, result );
        }
    }

    [Fact]
    public void Enqueue_WithRepetitions_ShouldAddFirstEventWithPointEqualToCurrentQueuePointPlusDelta()
    {
        var @event = Fixture.Create<TEvent>();
        var queueStartPoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.CreatePositiveInt32();
        var expectedDequeuePoint = queueStartPoint + delta;
        var sut = new MockEventQueue( queueStartPoint );

        var result = sut.Enqueue( @event, delta, repetitions );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( expectedDequeuePoint );
            result.Delta.Should().Be( delta );
            result.Repetitions.Should().Be( repetitions );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
        }
    }

    [Fact]
    public void Enqueue_WithRepetitions_ShouldAddAnotherEventWithPointEqualToCurrentQueuePointPlusDelta()
    {
        var (first, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var queueStartPoint = Fixture.Create<long>();
        var (firstDelta, eventDelta) = Fixture.CreateMany<int>( count: 2 ).ToList();
        var (firstRepetitions, eventRepetitions) = (Fixture.CreatePositiveInt32(), Fixture.CreatePositiveInt32());
        var expectedEventDequeuePoint = queueStartPoint + eventDelta;
        var sut = new MockEventQueue( queueStartPoint );
        var firstEvent = sut.Enqueue( first, firstDelta, firstRepetitions );

        var result = sut.Enqueue( @event, eventDelta, eventRepetitions );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( expectedEventDequeuePoint );
            result.Delta.Should().Be( eventDelta );
            result.Repetitions.Should().Be( eventRepetitions );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 2 );
            sut.Should().BeEquivalentTo( firstEvent, result );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Enqueue_WithRepetitions_ShouldThrowArgumentOutOfRangeException_WhenRepetitionsIsLessThanOne(int repetitions)
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var action = Lambda.Of( () => sut.Enqueue( Fixture.Create<TEvent>(), Fixture.Create<int>(), repetitions ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void EnqueueAt_WithRepetitions_ShouldAddFirstEvent()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.CreatePositiveInt32();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.EnqueueAt( @event, dequeuePoint, delta, repetitions );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( dequeuePoint );
            result.Delta.Should().Be( delta );
            result.Repetitions.Should().Be( repetitions );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
        }
    }

    [Fact]
    public void EnqueueAt_WithRepetitions_ShouldAddAnotherEvent()
    {
        var (first, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var queueStartPoint = Fixture.Create<long>();
        var (firstDequeuePoint, eventDequeuePoint) = Fixture.CreateMany<long>( count: 2 ).ToList();
        var (firstDelta, eventDelta) = Fixture.CreateMany<int>( count: 2 ).ToList();
        var (firstRepetitions, eventRepetitions) = (Fixture.CreatePositiveInt32(), Fixture.CreatePositiveInt32());
        var sut = new MockEventQueue( queueStartPoint );
        var firstEvent = sut.EnqueueAt( first, firstDequeuePoint, firstDelta, firstRepetitions );

        var result = sut.EnqueueAt( @event, eventDequeuePoint, eventDelta, eventRepetitions );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( eventDequeuePoint );
            result.Delta.Should().Be( eventDelta );
            result.Repetitions.Should().Be( eventRepetitions );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 2 );
            sut.Should().BeEquivalentTo( firstEvent, result );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void EnqueueAt_WithRepetitions_ShouldThrowArgumentOutOfRangeException_WhenRepetitionsIsLessThanOne(int repetitions)
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var action = Lambda.Of(
            () => sut.EnqueueAt( Fixture.Create<TEvent>(), Fixture.Create<long>(), Fixture.Create<int>(), repetitions ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
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

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( expectedDequeuePoint );
            result.Delta.Should().Be( delta );
            result.Repetitions.Should().Be( 0 );
            result.IsInfinite.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
        }
    }

    [Fact]
    public void EnqueueInfinite_ShouldAddAnotherEventWithPointEqualToCurrentQueuePointPlusDelta()
    {
        var (first, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var queueStartPoint = Fixture.Create<long>();
        var (firstDelta, eventDelta) = Fixture.CreateMany<int>( count: 2 ).ToList();
        var expectedEventDequeuePoint = queueStartPoint + eventDelta;
        var sut = new MockEventQueue( queueStartPoint );
        var firstEvent = sut.EnqueueInfinite( first, firstDelta );

        var result = sut.EnqueueInfinite( @event, eventDelta );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( expectedEventDequeuePoint );
            result.Delta.Should().Be( eventDelta );
            result.Repetitions.Should().Be( 0 );
            result.IsInfinite.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Should().BeEquivalentTo( firstEvent, result );
        }
    }

    [Fact]
    public void EnqueueInfiniteAt_ShouldAddFirstEvent()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.EnqueueInfiniteAt( @event, dequeuePoint, delta );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( dequeuePoint );
            result.Delta.Should().Be( delta );
            result.Repetitions.Should().Be( 0 );
            result.IsInfinite.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
        }
    }

    [Fact]
    public void EnqueueInfiniteAt_ShouldAddAnotherEvent()
    {
        var (first, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var queueStartPoint = Fixture.Create<long>();
        var (firstDequeuePoint, eventDequeuePoint) = Fixture.CreateMany<long>( count: 2 ).ToList();
        var (firstDelta, eventDelta) = Fixture.CreateMany<int>( count: 2 ).ToList();
        var sut = new MockEventQueue( queueStartPoint );
        var firstEvent = sut.EnqueueInfiniteAt( first, firstDequeuePoint, firstDelta );

        var result = sut.EnqueueInfiniteAt( @event, eventDequeuePoint, eventDelta );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( eventDequeuePoint );
            result.Delta.Should().Be( eventDelta );
            result.Repetitions.Should().Be( 0 );
            result.IsInfinite.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            sut.Should().BeEquivalentTo( firstEvent, result );
        }
    }

    [Fact]
    public void Move_ShouldUpdateCurrentPoint()
    {
        var startPoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var expectedCurrentPoint = startPoint + delta;
        var sut = new MockEventQueue( startPoint );

        sut.Move( delta );

        sut.CurrentPoint.Should().Be( expectedCurrentPoint );
    }

    [Fact]
    public void Clear_ShouldRemoveAllEvents()
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.Enqueue( Fixture.Create<TEvent>(), Fixture.Create<int>() );
        sut.Enqueue( Fixture.Create<TEvent>(), Fixture.Create<int>() );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Should().BeEmpty();
        }
    }

    [Fact]
    public void Dequeue_ShouldReturnNull_WhenQueueIsEmpty()
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var result = sut.Dequeue();
        result.Should().BeNull();
    }

    [Fact]
    public void Dequeue_ShouldReturnNull_WhenNextEventPointIsLargerThanCurrentQueuePoint()
    {
        var (queueStartPoint, eventDequeuePoint) = Fixture.CreateDistinctSortedCollection<long>( count: 2 );
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueAt( Fixture.Create<TEvent>(), eventDequeuePoint );

        var result = sut.Dequeue();

        result.Should().BeNull();
    }

    [Fact]
    public void Dequeue_ShouldReturnNextEventAndRemoveIt_WhenNextEventPointIsLessThanOrEqualToCurrentQueuePointAndIsSingle()
    {
        var @event = Fixture.Create<TEvent>();
        var (eventDequeuePoint, queueStartPoint) = Fixture.CreateDistinctSortedCollection<long>( count: 2 );
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueAt( @event, eventDequeuePoint );

        var result = sut.Dequeue();

        using ( new AssertionScope() )
        {
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = eventDequeuePoint,
                        Delta = default( int ),
                        Repetitions = 1,
                        IsInfinite = false
                    } );

            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void Dequeue_ShouldReturnNextEventAndRemoveIt_WhenNextEventPointIsLessThanOrEqualToCurrentQueuePointAndHasOneRepetition()
    {
        var @event = Fixture.Create<TEvent>();
        var (eventDequeuePoint, queueStartPoint) = Fixture.CreateDistinctSortedCollection<long>( count: 2 );
        var delta = Fixture.Create<int>();
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueAt( @event, eventDequeuePoint, delta, repetitions: 1 );

        var result = sut.Dequeue();

        using ( new AssertionScope() )
        {
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = eventDequeuePoint,
                        Delta = delta,
                        Repetitions = 1,
                        IsInfinite = false
                    } );

            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void
        Dequeue_ShouldReturnNextEventAndReinsertIt_WhenNextEventPointIsLessThanOrEqualToCurrentQueuePointAndHasMoreThanOneRepetition()
    {
        var @event = Fixture.Create<TEvent>();
        var (eventDequeuePoint, queueStartPoint) = Fixture.CreateDistinctSortedCollection<long>( count: 2 );
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.CreatePositiveInt32() + 1;
        var expectedNewDequeuePoint = eventDequeuePoint + delta;
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueAt( @event, eventDequeuePoint, delta, repetitions );

        var result = sut.Dequeue();

        using ( new AssertionScope() )
        {
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = eventDequeuePoint,
                        Delta = delta,
                        Repetitions = repetitions,
                        IsInfinite = false
                    } );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = expectedNewDequeuePoint,
                        Delta = delta,
                        Repetitions = repetitions - 1,
                        IsInfinite = false
                    } );
        }
    }

    [Fact]
    public void Dequeue_ShouldReturnNextEventAndReinsertIt_WhenNextEventPointIsLessThanOrEqualToCurrentQueuePointAndIsInfinite()
    {
        var @event = Fixture.Create<TEvent>();
        var (eventDequeuePoint, queueStartPoint) = Fixture.CreateDistinctSortedCollection<long>( count: 2 );
        var delta = Fixture.Create<int>();
        var expectedNewDequeuePoint = eventDequeuePoint + delta;
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueInfiniteAt( @event, eventDequeuePoint, delta );

        var result = sut.Dequeue();

        using ( new AssertionScope() )
        {
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = eventDequeuePoint,
                        Delta = delta,
                        Repetitions = 0,
                        IsInfinite = true
                    } );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = expectedNewDequeuePoint,
                        Delta = delta,
                        Repetitions = 0,
                        IsInfinite = true
                    } );
        }
    }

    [Fact]
    public void Dequeue_ShouldReturnEventsInAnOrderedWayUsingTheirDequeuePoints()
    {
        var events = Fixture.CreateDistinctCollection<TEvent>( count: 10 );
        var firstDequeuePoints = Fixture.CreateDistinctCollection<long>( count: 10 );
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

        result.Should().BeSequentiallyEqualTo( expectedResult );
    }

    [Fact]
    public void GetNext_ShouldReturnNull_WhenQueueIsEmpty()
    {
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var result = sut.GetNext();
        result.Should().BeNull();
    }

    [Fact]
    public void GetNext_ShouldReturnCorrectEnqueuedEvent_WhenQueueIsNotEmptyAndNextEventDequeuePointIsGreaterThanCurrentQueuePoint()
    {
        var (expectedEvent, otherEvent) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var (queuePoint, expectedPoint, otherPoint) = Fixture.CreateDistinctSortedCollection<long>( count: 3 );
        var sut = new MockEventQueue( queuePoint );
        var expected = sut.EnqueueAt( expectedEvent, expectedPoint );
        sut.EnqueueAt( otherEvent, otherPoint );

        var result = sut.GetNext();

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetNext_ShouldReturnCorrectEnqueuedEvent_WhenQueueIsNotEmptyAndNextEventDequeuePointIsLessThanCurrentQueuePoint()
    {
        var (expectedEvent, otherEvent) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var (expectedPoint, queuePoint, otherPoint) = Fixture.CreateDistinctSortedCollection<long>( count: 3 );
        var sut = new MockEventQueue( queuePoint );
        var expected = sut.EnqueueAt( expectedEvent, expectedPoint );
        sut.EnqueueAt( otherEvent, otherPoint );

        var result = sut.GetNext();

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetNext_ShouldReturnCorrectEnqueuedEvent_WhenQueueIsNotEmptyAndNextEventDequeuePointIsEqualToCurrentQueuePoint()
    {
        var (expectedEvent, otherEvent) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var (queuePoint, otherPoint) = Fixture.CreateDistinctSortedCollection<long>( count: 2 );
        var sut = new MockEventQueue( queuePoint );
        var expected = sut.EnqueueAt( expectedEvent, queuePoint );
        sut.EnqueueAt( otherEvent, otherPoint );

        var result = sut.GetNext();

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetEvents_ShouldNotIncludeAnEventWhoseDequeuePointExceedsEndPoint()
    {
        var @event = Fixture.Create<TEvent>();
        var (endPoint, dequeuePoint) = Fixture.CreateDistinctSortedCollection<long>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint );

        var result = sut.GetEvents( endPoint );

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEvents_ShouldIncludeASingleEventWhoseDequeuePointDoesNotExceedEndPoint()
    {
        var @event = Fixture.Create<TEvent>();
        var (dequeuePoint, endPoint) = Fixture.CreateDistinctSortedCollection<long>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint );

        var result = sut.GetEvents( endPoint );

        result.Should()
            .BeEquivalentTo(
                new
                {
                    Event = @event,
                    DequeuePoint = dequeuePoint,
                    Delta = default( int ),
                    Repetitions = 1,
                    IsInfinite = false
                } );
    }

    [Fact]
    public void GetEvents_ShouldIncludeAllRepeatableEventInstancesWhenAllItsDequeuePointsDoNotExceedEndPoint()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.CreatePositiveInt32();
        var repetitions = Fixture.CreatePositiveInt32();
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

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetEvents_ShouldIncludeSomeRepeatableEventInstancesWhenSomeOfItsDequeuePointsDoNotExceedEndPoint()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.CreatePositiveInt32();
        var repetitions = Fixture.CreatePositiveInt32() + 3;
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

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetEvents_ShouldIncludeInfiniteEventInstancesUpUntilEndPoint()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.CreatePositiveInt32();
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

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetEvents_ShouldReturnEventsWhoseDequeuePointsDoNotExceedEndPointInAnUnorderedWay()
    {
        var events = Fixture.CreateDistinctCollection<TEvent>( count: 10 );
        var firstDequeuePoints = Fixture.CreateDistinctCollection<long>( count: 10 );
        var eventsWithDequeuePoints = events.Zip( firstDequeuePoints ).ToList();
        var expectedResult = eventsWithDequeuePoints.Select(
            x => new
            {
                Event = x.First,
                DequeuePoint = x.Second,
                Delta = default( int ),
                Repetitions = 1,
                IsInfinite = false
            } );

        var sut = new MockEventQueue( Fixture.Create<long>() );

        foreach ( var (@event, dequeuePoint) in eventsWithDequeuePoints )
            sut.EnqueueAt( @event, dequeuePoint );

        var result = sut.GetEvents( eventsWithDequeuePoints.Select( x => x.Second ).Max() );

        result.Should().BeEquivalentTo( expectedResult );
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