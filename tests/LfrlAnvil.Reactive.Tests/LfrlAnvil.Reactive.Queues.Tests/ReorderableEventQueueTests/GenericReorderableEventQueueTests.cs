using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Queues.Tests.ReorderableEventQueueTests;

public abstract class GenericReorderableEventQueueTests<TEvent> : TestsBase
    where TEvent : notnull
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
        var eventComparer = EqualityComparerFactory<TEvent>.Create( (a, b) => a!.GetHashCode() == b!.GetHashCode() );
        var comparer = Comparer<long>.Create( (a, b) => a.GetHashCode().CompareTo( b.GetHashCode() ) );
        var sut = new MockEventQueue( startPoint, eventComparer, comparer );

        using ( new AssertionScope() )
        {
            sut.StartPoint.Should().Be( startPoint );
            sut.CurrentPoint.Should().Be( startPoint );
            sut.Count.Should().Be( 0 );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.EventComparer.Should().BeSameAs( eventComparer );
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
    public void Enqueue_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var queueStartPoint = Fixture.Create<long>();
        var (oldDelta, newDelta) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var expectedDequeuePoint = queueStartPoint + newDelta;
        var sut = new MockEventQueue( queueStartPoint );
        sut.Enqueue( @event, oldDelta );

        var result = sut.Enqueue( @event, newDelta );

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
    public void EnqueueAt_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var (oldDequeuePoint, newDequeuePoint) = Fixture.CreateDistinctCollection<long>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, oldDequeuePoint );

        var result = sut.EnqueueAt( @event, newDequeuePoint );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( newDequeuePoint );
            result.Delta.Should().Be( default );
            result.Repetitions.Should().Be( 1 );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
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

    [Fact]
    public void Enqueue_WithRepetitions_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var queueStartPoint = Fixture.Create<long>();
        var (oldDelta, newDelta) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var (oldRepetitions, newRepetitions) = (Fixture.CreatePositiveInt32(), Fixture.CreatePositiveInt32());
        var expectedDequeuePoint = queueStartPoint + newDelta;
        var sut = new MockEventQueue( queueStartPoint );
        sut.Enqueue( @event, oldDelta, oldRepetitions );

        var result = sut.Enqueue( @event, newDelta, newRepetitions );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( expectedDequeuePoint );
            result.Delta.Should().Be( newDelta );
            result.Repetitions.Should().Be( newRepetitions );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
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

    [Fact]
    public void EnqueueAt_WithRepetitions_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var (oldDequeuePoint, newDequeuePoint) = Fixture.CreateDistinctCollection<long>( count: 2 );
        var (oldDelta, newDelta) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var (oldRepetitions, newRepetitions) = (Fixture.CreatePositiveInt32(), Fixture.CreatePositiveInt32());
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, oldDequeuePoint, oldDelta, oldRepetitions );

        var result = sut.EnqueueAt( @event, newDequeuePoint, newDelta, newRepetitions );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( newDequeuePoint );
            result.Delta.Should().Be( newDelta );
            result.Repetitions.Should().Be( newRepetitions );
            result.IsInfinite.Should().BeFalse();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
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
    public void EnqueueInfinite_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var queueStartPoint = Fixture.Create<long>();
        var (oldDelta, newDelta) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var expectedDequeuePoint = queueStartPoint + newDelta;
        var sut = new MockEventQueue( queueStartPoint );
        sut.EnqueueInfinite( @event, oldDelta );

        var result = sut.EnqueueInfinite( @event, newDelta );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( expectedDequeuePoint );
            result.Delta.Should().Be( newDelta );
            result.Repetitions.Should().Be( 0 );
            result.IsInfinite.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
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
    public void EnqueueInfiniteAt_ShouldUpdateEvent_WhenEventAlreadyExists()
    {
        var @event = Fixture.Create<TEvent>();
        var (oldDequeuePoint, newDequeuePoint) = Fixture.CreateDistinctCollection<long>( count: 2 );
        var (oldDelta, newDelta) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueInfiniteAt( @event, oldDequeuePoint, oldDelta );

        var result = sut.EnqueueInfiniteAt( @event, newDequeuePoint, newDelta );

        using ( new AssertionScope() )
        {
            result.Event.Should().Be( @event );
            result.DequeuePoint.Should().Be( newDequeuePoint );
            result.Delta.Should().Be( newDelta );
            result.Repetitions.Should().Be( 0 );
            result.IsInfinite.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.Should().BeEquivalentTo( result );
        }
    }

    [Fact]
    public void SetDequeuePoint_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.SetDequeuePoint( @event, Fixture.Create<long>() );

        result.Should().BeNull();
    }

    [Fact]
    public void SetDequeuePoint_ShouldUpdateEvent_WhenEventExists()
    {
        var (other, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var (newDequeuePoint, otherDequeuePoint, oldDequeuePoint) = Fixture.CreateDistinctSortedCollection<long>( count: 3 );
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.CreatePositiveInt32();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( other, otherDequeuePoint );
        sut.EnqueueAt( @event, oldDequeuePoint, delta, repetitions );

        var result = sut.SetDequeuePoint( @event, newDequeuePoint );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            sut.GetNext().Should().BeEquivalentTo( result );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = newDequeuePoint,
                        Delta = delta,
                        Repetitions = repetitions,
                        IsInfinite = false
                    } );
        }
    }

    [Fact]
    public void DelayDequeuePoint_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.DelayDequeuePoint( @event, Fixture.Create<int>() );

        result.Should().BeNull();
    }

    [Fact]
    public void DelayDequeuePoint_ShouldUpdateEvent_WhenEventExists()
    {
        var (other, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var (oldDequeuePoint, otherDequeuePoint) = Fixture.CreateDistinctSortedCollection<long>( count: 2 );
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.CreatePositiveInt32();
        var delay = (int)(otherDequeuePoint - oldDequeuePoint) + 1;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var enqueuedOther = sut.EnqueueAt( other, otherDequeuePoint );
        sut.EnqueueAt( @event, oldDequeuePoint, delta, repetitions );

        var result = sut.DelayDequeuePoint( @event, delay );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            sut.GetNext().Should().BeEquivalentTo( enqueuedOther );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = otherDequeuePoint + 1,
                        Delta = delta,
                        Repetitions = repetitions,
                        IsInfinite = false
                    } );
        }
    }

    [Fact]
    public void AdvanceDequeuePoint_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.AdvanceDequeuePoint( @event, Fixture.Create<int>() );

        result.Should().BeNull();
    }

    [Fact]
    public void AdvanceDequeuePoint_ShouldUpdateEvent_WhenEventExists()
    {
        var (other, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var (otherDequeuePoint, oldDequeuePoint) = Fixture.CreateDistinctSortedCollection<long>( count: 2 );
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.CreatePositiveInt32();
        var advance = (int)(oldDequeuePoint - otherDequeuePoint) + 1;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( other, otherDequeuePoint );
        sut.EnqueueAt( @event, oldDequeuePoint, delta, repetitions );

        var result = sut.AdvanceDequeuePoint( @event, advance );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            sut.GetNext().Should().BeEquivalentTo( result );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = otherDequeuePoint - 1,
                        Delta = delta,
                        Repetitions = repetitions,
                        IsInfinite = false
                    } );
        }
    }

    [Fact]
    public void SetDelta_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.SetDelta( @event, Fixture.Create<int>() );

        result.Should().BeNull();
    }

    [Fact]
    public void SetDelta_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var (oldDelta, newDelta) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var repetitions = Fixture.CreatePositiveInt32();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, oldDelta, repetitions );

        var result = sut.SetDelta( @event, newDelta );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = dequeuePoint,
                        Delta = newDelta,
                        Repetitions = repetitions,
                        IsInfinite = false
                    } );
        }
    }

    [Fact]
    public void IncreaseDelta_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.IncreaseDelta( @event, Fixture.Create<int>() );

        result.Should().BeNull();
    }

    [Fact]
    public void IncreaseDelta_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var oldDelta = Fixture.Create<int>();
        var deltaIncrease = Fixture.Create<int>();
        var repetitions = Fixture.CreatePositiveInt32();
        var expectedDelta = oldDelta + deltaIncrease;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, oldDelta, repetitions );

        var result = sut.IncreaseDelta( @event, deltaIncrease );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = dequeuePoint,
                        Delta = expectedDelta,
                        Repetitions = repetitions,
                        IsInfinite = false
                    } );
        }
    }

    [Fact]
    public void DecreaseDelta_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.DecreaseDelta( @event, Fixture.Create<int>() );

        result.Should().BeNull();
    }

    [Fact]
    public void DecreaseDelta_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var oldDelta = Fixture.Create<int>();
        var deltaDecrease = Fixture.Create<int>();
        var repetitions = Fixture.CreatePositiveInt32();
        var expectedDelta = oldDelta - deltaDecrease;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, oldDelta, repetitions );

        var result = sut.DecreaseDelta( @event, deltaDecrease );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = dequeuePoint,
                        Delta = expectedDelta,
                        Repetitions = repetitions,
                        IsInfinite = false
                    } );
        }
    }

    [Fact]
    public void SetRepetitions_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.SetRepetitions( @event, Fixture.CreatePositiveInt32() );

        result.Should().BeNull();
    }

    [Fact]
    public void SetRepetitions_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var (oldRepetitions, newRepetitions) = (Fixture.CreatePositiveInt32(), Fixture.CreatePositiveInt32());
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, delta, oldRepetitions );

        var result = sut.SetRepetitions( @event, newRepetitions );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = dequeuePoint,
                        Delta = delta,
                        Repetitions = newRepetitions,
                        IsInfinite = false
                    } );
        }
    }

    [Fact]
    public void SetRepetitions_ShouldUpdateEvent_WhenEventExistsAndIsInfinite()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var repetitions = Fixture.CreatePositiveInt32();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueInfiniteAt( @event, dequeuePoint, delta );

        var result = sut.SetRepetitions( @event, repetitions );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = dequeuePoint,
                        Delta = delta,
                        Repetitions = repetitions,
                        IsInfinite = false
                    } );
        }
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

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IncreaseRepetitions_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.IncreaseRepetitions( @event, Fixture.CreatePositiveInt32() );

        result.Should().BeNull();
    }

    [Fact]
    public void IncreaseRepetitions_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var oldRepetitions = Fixture.CreatePositiveInt32();
        var repetitionsIncrease = Fixture.CreatePositiveInt32();
        var expectedRepetitions = oldRepetitions + repetitionsIncrease;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, delta, oldRepetitions );

        var result = sut.IncreaseRepetitions( @event, repetitionsIncrease );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = dequeuePoint,
                        Delta = delta,
                        Repetitions = expectedRepetitions,
                        IsInfinite = false
                    } );
        }
    }

    [Fact]
    public void IncreaseRepetitions_ShouldReturnUnchangedEvent_WhenEventExistsAndIsInfinite()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var oldEnqueuedEvent = sut.EnqueueInfiniteAt( @event, dequeuePoint, delta );

        var result = sut.IncreaseRepetitions( @event, Fixture.CreatePositiveInt32() );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should().BeEquivalentTo( oldEnqueuedEvent );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void IncreaseRepetitions_ShouldThrowArgumentOutOfRangeException_WhenNewRepetitionsIsLessThanOne(int targetRepetitions)
    {
        var @event = Fixture.Create<TEvent>();
        var repetitions = Fixture.CreatePositiveInt32();
        var repetitionsIncrease = targetRepetitions - repetitions;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.Enqueue( @event, Fixture.Create<int>(), repetitions );

        var action = Lambda.Of( () => sut.IncreaseRepetitions( @event, repetitionsIncrease ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DecreaseRepetitions_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.DecreaseRepetitions( @event, Fixture.CreatePositiveInt32() );

        result.Should().BeNull();
    }

    [Fact]
    public void DecreaseRepetitions_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var oldRepetitions = Fixture.CreatePositiveInt32();
        var repetitionsDecrease = Fixture.CreateNegativeInt32();
        var expectedRepetitions = oldRepetitions - repetitionsDecrease;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, delta, oldRepetitions );

        var result = sut.DecreaseRepetitions( @event, repetitionsDecrease );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = dequeuePoint,
                        Delta = delta,
                        Repetitions = expectedRepetitions,
                        IsInfinite = false
                    } );
        }
    }

    [Fact]
    public void DecreaseRepetitions_ShouldReturnUnchangedEvent_WhenEventExistsAndIsInfinite()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        var oldEnqueuedEvent = sut.EnqueueInfiniteAt( @event, dequeuePoint, delta );

        var result = sut.DecreaseRepetitions( @event, Fixture.CreatePositiveInt32() );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should().BeEquivalentTo( oldEnqueuedEvent );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void DecreaseRepetitions_ShouldThrowArgumentOutOfRangeException_WhenNewRepetitionsIsLessThanOne(int targetRepetitions)
    {
        var @event = Fixture.Create<TEvent>();
        var repetitions = Fixture.CreatePositiveInt32();
        var repetitionsDecrease = repetitions - targetRepetitions;
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.Enqueue( @event, Fixture.Create<int>(), repetitions );

        var action = Lambda.Of( () => sut.DecreaseRepetitions( @event, repetitionsDecrease ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MakeInfinite_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );

        var result = sut.MakeInfinite( @event );

        result.Should().BeNull();
    }

    [Fact]
    public void MakeInfinite_ShouldUpdateEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var delta = Fixture.Create<int>();
        var oldRepetitions = Fixture.CreatePositiveInt32();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint, delta, oldRepetitions );

        var result = sut.MakeInfinite( @event );

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        Event = @event,
                        DequeuePoint = dequeuePoint,
                        Delta = delta,
                        Repetitions = 0,
                        IsInfinite = true
                    } );
        }
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

        using ( new AssertionScope() )
        {
            sut.GetEvent( @event ).Should().BeEquivalentTo( result );
            result.Should().BeEquivalentTo( oldEnqueuedEvent );
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
    public void Remove_ShouldReturnCorrectResultAndRemoveEvent_WhenEventExists()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint );

        var result = sut.Remove( @event );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
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
    }

    [Fact]
    public void Remove_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var (other, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( other, dequeuePoint: Fixture.Create<long>() );

        var result = sut.Remove( @event );

        result.Should().BeNull();
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenEventIsQueued()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint: Fixture.Create<long>() );

        var result = sut.Contains( @event );

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenEventIsNotQueued()
    {
        var (other, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( other, dequeuePoint: Fixture.Create<long>() );

        var result = sut.Contains( @event );

        result.Should().BeFalse();
    }

    [Fact]
    public void GetEvent_ShouldReturnCorrectResult_WhenEventIsQueued()
    {
        var @event = Fixture.Create<TEvent>();
        var dequeuePoint = Fixture.Create<long>();
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( @event, dequeuePoint );

        var result = sut.GetEvent( @event );

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
    public void Contains_ShouldReturnNull_WhenEventIsNotQueued()
    {
        var (other, @event) = Fixture.CreateDistinctCollection<TEvent>( count: 2 );
        var sut = new MockEventQueue( Fixture.Create<long>() );
        sut.EnqueueAt( other, dequeuePoint: Fixture.Create<long>() );

        var result = sut.GetEvent( @event );

        result.Should().BeNull();
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
