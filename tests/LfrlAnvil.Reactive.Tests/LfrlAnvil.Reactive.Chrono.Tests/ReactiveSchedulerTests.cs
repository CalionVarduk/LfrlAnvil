using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Chrono.Tests;

public class ReactiveSchedulerTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateSchedulerInCreatedState()
    {
        var start = new Timestamp( 123 );
        var timestamps = new Timestamps( start );
        var sut = new ReactiveScheduler<string>( timestamps );

        using ( new AssertionScope() )
        {
            sut.Timestamps.Should().BeSameAs( timestamps );
            sut.State.Should().Be( ReactiveSchedulerState.Created );
            sut.DefaultInterval.Should().Be( Duration.FromHours( 1 ) );
            sut.SpinWaitDurationHint.Should().Be( Duration.FromMicroseconds( 1 ) );
            sut.StartTimestamp.Should().Be( start );
            sut.KeyComparer.Should().BeSameAs( EqualityComparer<string>.Default );
            sut.TaskKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public void Ctor_WithCustomParameters_ShouldCreateSchedulerInCreatedState()
    {
        var start = new Timestamp( 123 );
        var timestamps = new Timestamps( start );
        var defaultInterval = Duration.FromTicks( 1 );
        var spinWaitDurationHint = Duration.FromTicks( 0 );
        var keyComparer = EqualityComparerFactory<string>.Create( (a, b) => a!.Equals( b ) );
        var sut = new ReactiveScheduler<string>( timestamps, keyComparer, defaultInterval, spinWaitDurationHint );

        using ( new AssertionScope() )
        {
            sut.Timestamps.Should().BeSameAs( timestamps );
            sut.State.Should().Be( ReactiveSchedulerState.Created );
            sut.DefaultInterval.Should().Be( defaultInterval );
            sut.SpinWaitDurationHint.Should().Be( spinWaitDurationHint );
            sut.StartTimestamp.Should().Be( start );
            sut.KeyComparer.Should().BeSameAs( keyComparer );
            sut.TaskKeys.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, 0 )]
    [InlineData( 1, -1 )]
    [InlineData( int.MaxValue * TimeSpan.TicksPerMillisecond + 1, 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenDurationParameterIsInvalid(
        long defaultIntervalTicks,
        long spinWaitDurationHintTicks)
    {
        var timestamps = new Timestamps();
        var defaultInterval = Duration.FromTicks( defaultIntervalTicks );
        var spinWaitDurationHint = Duration.FromTicks( spinWaitDurationHintTicks );

        var action = Lambda.Of(
            () => new ReactiveScheduler<string>(
                timestamps,
                defaultInterval: defaultInterval,
                spinWaitDurationHint: spinWaitDurationHint ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Schedule_ShouldRegisterTask()
    {
        var next = new Timestamp( 123 );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );

        var result = sut.Schedule( task, next );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskState( sut.TryGetTaskState( "foo" ), task, nextTimestamp: next, interval: Duration.Zero, repetitions: 1 );
        }
    }

    [Fact]
    public void Schedule_WithMultipleRepetitions_ShouldRegisterTask()
    {
        var next = new Timestamp( 123 );
        var interval = Duration.FromTicks( 456 );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );

        var result = sut.Schedule( task, next, interval, 42 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskState( sut.TryGetTaskState( "foo" ), task, nextTimestamp: next, interval: interval, repetitions: 42 );
        }
    }

    [Fact]
    public void ScheduleInfinite_ShouldRegisterTask()
    {
        var next = new Timestamp( 123 );
        var interval = Duration.FromTicks( 456 );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );

        var result = sut.ScheduleInfinite( task, next, interval );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskState(
                sut.TryGetTaskState( "foo" ),
                task,
                nextTimestamp: next,
                interval: interval,
                repetitions: null,
                isInfinite: true );
        }
    }

    [Fact]
    public void Schedule_ShouldReturnFalse_WhenTaskKeyIsTakenByDifferentTask()
    {
        var next = new Timestamp( 123 );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Schedule( task, next );

        var result = sut.Schedule( new ScheduleTask( "foo" ), new Timestamp( 456 ) );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskState( sut.TryGetTaskState( "foo" ), task, nextTimestamp: next, interval: Duration.Zero, repetitions: 1 );
        }
    }

    [Fact]
    public void Schedule_WithMultipleRepetitions_ShouldReturnFalse_WhenTaskKeyIsTakenByDifferentTask()
    {
        var next = new Timestamp( 123 );
        var interval = Duration.FromTicks( 456 );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Schedule( task, next, interval, 42 );

        var result = sut.Schedule( new ScheduleTask( "foo" ), new Timestamp( 789 ), Duration.FromTicks( 234 ), 84 );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskState( sut.TryGetTaskState( "foo" ), task, nextTimestamp: next, interval: interval, repetitions: 42 );
        }
    }

    [Fact]
    public void ScheduleInfinite_ShouldReturnFalse_WhenTaskKeyIsTakenByDifferentTask()
    {
        var next = new Timestamp( 123 );
        var interval = Duration.FromTicks( 456 );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.ScheduleInfinite( task, next, interval );

        var result = sut.ScheduleInfinite( new ScheduleTask( "foo" ), new Timestamp( 789 ), Duration.FromTicks( 234 ) );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskState(
                sut.TryGetTaskState( "foo" ),
                task,
                nextTimestamp: next,
                interval: interval,
                repetitions: null,
                isInfinite: true );
        }
    }

    [Fact]
    public void Schedule_ShouldUpdateTask_WhenTaskAlreadyExists()
    {
        var next = new Timestamp( 789 );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.ScheduleInfinite( task, new Timestamp( 123 ), Duration.FromTicks( 456 ) );

        var result = sut.Schedule( task, next );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskState( sut.TryGetTaskState( "foo" ), task, nextTimestamp: next, interval: Duration.Zero, repetitions: 1 );
        }
    }

    [Fact]
    public void Schedule_WithMultipleRepetitions_ShouldUpdateTask_WhenTaskAlreadyExists()
    {
        var next = new Timestamp( 456 );
        var interval = Duration.FromTicks( 789 );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Schedule( task, new Timestamp( 123 ) );

        var result = sut.Schedule( task, next, interval, 42 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskState( sut.TryGetTaskState( "foo" ), task, nextTimestamp: next, interval: interval, repetitions: 42 );
        }
    }

    [Fact]
    public void ScheduleInfinite_ShouldUpdateTask_WhenTaskAlreadyExists()
    {
        var next = new Timestamp( 789 );
        var interval = Duration.FromTicks( 234 );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Schedule( task, new Timestamp( 123 ), Duration.FromTicks( 456 ), 42 );

        var result = sut.ScheduleInfinite( task, next, interval );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskState(
                sut.TryGetTaskState( "foo" ),
                task,
                nextTimestamp: next,
                interval: interval,
                repetitions: null,
                isInfinite: true );
        }
    }

    [Fact]
    public void Schedule_ShouldDoNothing_WhenSchedulerHasBeenDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Dispose();

        var result = sut.Schedule( task, new Timestamp( 123 ) );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.TaskKeys.Should().BeEmpty();
            sut.TryGetTaskState( "foo" ).Should().BeNull();
        }
    }

    [Fact]
    public void Schedule_WithMultipleRepetitions_ShouldDoNothing_WhenSchedulerHasBeenDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Dispose();

        var result = sut.Schedule( task, new Timestamp( 123 ), Duration.FromTicks( 456 ), 42 );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.TaskKeys.Should().BeEmpty();
            sut.TryGetTaskState( "foo" ).Should().BeNull();
        }
    }

    [Fact]
    public void ScheduleInfinite_ShouldDoNothing_WhenSchedulerHasBeenDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Dispose();

        var result = sut.ScheduleInfinite( task, new Timestamp( 123 ), Duration.FromTicks( 456 ) );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.TaskKeys.Should().BeEmpty();
            sut.TryGetTaskState( "foo" ).Should().BeNull();
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Schedule_WithMultipleRepetitions_ShouldDoNothing_WhenRepetitionsAreLessThanOne(int repetitions)
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );

        var result = sut.Schedule( task, new Timestamp( 123 ), Duration.FromTicks( 456 ), repetitions );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.TaskKeys.Should().BeEmpty();
            sut.TryGetTaskState( "foo" ).Should().BeNull();
        }
    }

    [Fact]
    public void Schedule_WithMultipleRepetitions_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanZero()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );

        var action = Lambda.Of( () => sut.Schedule( task, new Timestamp( 123 ), Duration.FromTicks( -1 ), 1 ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ScheduleInfinite_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanZero()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );

        var action = Lambda.Of( () => sut.ScheduleInfinite( task, new Timestamp( 123 ), Duration.FromTicks( -1 ) ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetInterval_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var result = sut.SetInterval( "foo", Duration.FromHours( 1 ) );

        result.Should().BeFalse();
    }

    [Fact]
    public void SetInterval_ShouldReturnFalse_WhenSchedulerIsDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Dispose();

        var result = sut.SetInterval( "foo", Duration.FromHours( 1 ) );

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 123 )]
    [InlineData( 456 )]
    public void SetInterval_ShouldUpdateExistingTask_WhenKeyExists(long ticks)
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Schedule( task, new Timestamp( 123 ), Duration.FromTicks( 123 ), 42 );

        var result = sut.SetInterval( "foo", Duration.FromTicks( ticks ) );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            AssertTaskState(
                sut.TryGetTaskState( "foo" ),
                task,
                interval: Duration.FromTicks( ticks ),
                nextTimestamp: new Timestamp( 123 ),
                repetitions: 42 );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void SetInterval_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanOneTick(long ticks)
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var action = Lambda.Of( () => sut.SetInterval( "foo", Duration.FromTicks( ticks ) ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetRepetitions_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var result = sut.SetRepetitions( "foo", 1 );

        result.Should().BeFalse();
    }

    [Fact]
    public void SetRepetitions_ShouldReturnFalse_WhenSchedulerIsDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Dispose();

        var result = sut.SetRepetitions( "foo", 1 );

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 42 )]
    [InlineData( 17 )]
    public void SetRepetitions_ShouldUpdateExistingTask_WhenKeyExists(int repetitions)
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Schedule( task, new Timestamp( 123 ), Duration.FromTicks( 123 ), 42 );

        var result = sut.SetRepetitions( "foo", repetitions );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            AssertTaskState(
                sut.TryGetTaskState( "foo" ),
                task,
                interval: Duration.FromTicks( 123 ),
                nextTimestamp: new Timestamp( 123 ),
                repetitions: repetitions );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void SetRepetitions_ShouldThrowArgumentOutOfRangeException_WhenIntervalIsLessThanOne(int repetitions)
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var action = Lambda.Of( () => sut.SetRepetitions( "foo", repetitions ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MakeInfinite_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var result = sut.MakeInfinite( "foo" );

        result.Should().BeFalse();
    }

    [Fact]
    public void MakeInfinite_ShouldReturnFalse_WhenSchedulerIsDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Dispose();

        var result = sut.MakeInfinite( "foo" );

        result.Should().BeFalse();
    }

    [Fact]
    public void MakeInfinite_ShouldUpdateExistingTask_WhenKeyExists()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Schedule( task, new Timestamp( 123 ), Duration.FromTicks( 123 ), 42 );

        var result = sut.MakeInfinite( "foo" );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            AssertTaskState(
                sut.TryGetTaskState( "foo" ),
                task,
                interval: Duration.FromTicks( 123 ),
                nextTimestamp: new Timestamp( 123 ),
                isInfinite: true );
        }
    }

    [Fact]
    public void SetNextTimestamp_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var result = sut.SetNextTimestamp( "foo", new Timestamp( 123 ) );

        result.Should().BeFalse();
    }

    [Fact]
    public void SetNextTimestamp_ShouldReturnFalse_WhenSchedulerIsDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Dispose();

        var result = sut.SetNextTimestamp( "foo", new Timestamp( 123 ) );

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 123 )]
    [InlineData( 456 )]
    public void SetNextTimestamp_ShouldUpdateExistingTask_WhenKeyExists(long ticks)
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask( "foo" );
        sut.Schedule( task, new Timestamp( 123 ), Duration.FromTicks( 123 ), 42 );

        var result = sut.SetNextTimestamp( "foo", new Timestamp( ticks ) );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            AssertTaskState(
                sut.TryGetTaskState( "foo" ),
                task,
                interval: Duration.FromTicks( 123 ),
                nextTimestamp: new Timestamp( ticks ),
                repetitions: 42 );
        }
    }

    [Fact]
    public async Task AllTaskModifications_ShouldReturnFalse_WhenTaskHasBeenRemoved()
    {
        var invocationOrder = new InvocationOrder( InvocationContinuation.Deferred( 0 ) );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var sut = new ReactiveScheduler<string>( timestamps );
        var task = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) => invocationOrder.OnInvoke( p.InvocationId ),
            onDispose: _ => invocationOrder.EndAction() );

        sut.Schedule( task, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();

        invocationOrder.WaitForStart();
        var state1 = sut.TryGetTaskState( "foo" );
        sut.Remove( "foo" );
        var state2 = sut.TryGetTaskState( "foo" );
        var scheduleResult = sut.Schedule( task, new Timestamp( 456 ) );
        var setIntervalResult = sut.SetInterval( "foo", Duration.FromTicks( 123 ) );
        var setRepetitionsResult = sut.SetRepetitions( "foo", 42 );
        var makeInfiniteResult = sut.MakeInfinite( "foo" );
        var setNextTimestampResult = sut.SetNextTimestamp( "foo", new Timestamp( 456 ) );
        var state3 = sut.TryGetTaskState( "foo" );
        invocationOrder.Continue( 0 );
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            scheduleResult.Should().BeFalse();
            setIntervalResult.Should().BeFalse();
            setRepetitionsResult.Should().BeFalse();
            makeInfiniteResult.Should().BeFalse();
            setNextTimestampResult.Should().BeFalse();

            AssertTaskState(
                state1,
                task,
                Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskState(
                state2,
                task,
                Duration.Zero,
                isDisposed: true,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            state3.Should().BeEquivalentTo( state2 );
        }
    }

    [Fact]
    public async Task Schedule_ShouldValidateSchedule_WhenNewTaskInvocationShouldOccurBeforeAnyOtherEvent()
    {
        var invocationOrder = new InvocationOrder( InvocationContinuation.Deferred( 0 ) );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        ScheduleTaskState<string>? state1 = null;
        ScheduleTaskState<string>? state2 = null;
        ScheduleTaskState<string>? state3 = null;
        var task = new ScheduleTask(
            "foo",
            onInvoke: (_, scheduler, p, _) =>
            {
                state1 = scheduler.TryGetTaskState( "foo" );
                return invocationOrder.OnInvoke( p.InvocationId, callback: () => state2 = scheduler.TryGetTaskState( "foo" ) );
            },
            onComplete: (_, scheduler, _) => state3 = scheduler.TryGetTaskState( "foo" ),
            onDispose: _ => invocationOrder.EndAction() );

        var schedule = sut.StartAsync();
        await Task.Delay( 1 );
        sut.Schedule( task, new Timestamp( 123 ) );
        timestamps.Move( Duration.FromTicks( 123 ) );
        invocationOrder.Continue( 0 );
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            AssertTaskState(
                state1,
                task,
                Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeInvocations: 1 );

            AssertTaskState(
                state2,
                task,
                Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskState(
                state3,
                task,
                Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                completedInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public async Task SetNextTimestamp_ShouldValidateSchedule_WhenUpdatedTaskInvocationShouldOccurBeforeAnyOtherEvent()
    {
        var invocationOrder = new InvocationOrder( InvocationContinuation.Deferred( 0 ) );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        ScheduleTaskState<string>? state = null;
        var task = new ScheduleTask(
            "foo",
            onInvoke: (_, scheduler, p, _) => invocationOrder.OnInvoke(
                p.InvocationId,
                callback: () => state = scheduler.TryGetTaskState( "foo" ) ),
            onDispose: _ => invocationOrder.EndAction() );

        sut.Schedule( task, new Timestamp( Duration.FromHours( 1 ).Ticks ) );
        var schedule = sut.StartAsync();
        await Task.Delay( 1 );
        sut.SetNextTimestamp( "foo", new Timestamp( 123 ) );
        timestamps.Move( Duration.FromTicks( 123 ) );
        invocationOrder.Continue( 0 );
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            AssertTaskState(
                state,
                task,
                Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var result = sut.Remove( "foo" );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_ShouldMarkTaskForDisposal_WhenKeyExists()
    {
        var invocationOrder = new InvocationOrder( InvocationContinuation.Deferred( 0 ) );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var sut = new ReactiveScheduler<string>( timestamps );

        ScheduleTaskState<string>? state3 = null;
        var task = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) => invocationOrder.OnInvoke( p.InvocationId ),
            onDispose: _ =>
            {
                state3 = sut.TryGetTaskState( "foo" );
                invocationOrder.EndAction();
            } );

        sut.Schedule( task, new Timestamp( 123 ) );
        _ = sut.StartAsync();
        invocationOrder.WaitForStart();
        var state1 = sut.TryGetTaskState( "foo" );
        var result = sut.Remove( "foo" );
        var state2 = sut.TryGetTaskState( "foo" );
        invocationOrder.Continue( 0 );
        invocationOrder.WaitForEnd();

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            AssertTaskState(
                state1,
                task,
                Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskState(
                state2,
                task,
                Duration.Zero,
                isDisposed: true,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            state3.Should().BeNull();
        }
    }

    [Fact]
    public void Clear_ShouldMarkAllTasksForDisposal()
    {
        var invocationOrder = new InvocationOrder( actions: 2, InvocationContinuation.Deferred( 0 ) );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var sut = new ReactiveScheduler<string>( timestamps );

        ScheduleTaskState<string>? fooState3 = null;
        ScheduleTaskState<string>? barState3 = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) =>
            {
                timestamps.Move( Duration.FromTicks( 123 ) );
                return invocationOrder.OnInvoke( p.InvocationId, autoStart: false );
            },
            onDispose: _ =>
            {
                fooState3 = sut.TryGetTaskState( "foo" );
                invocationOrder.EndAction();
            } );

        var barTask = new ScheduleTask(
            "bar",
            onInvoke: (_, _, p, _) => invocationOrder.OnInvoke( p.InvocationId ),
            onDispose: _ =>
            {
                barState3 = sut.TryGetTaskState( "bar" );
                invocationOrder.EndAction();
            } );

        sut.Schedule( fooTask, new Timestamp( 123 ) );
        sut.Schedule( barTask, new Timestamp( 246 ) );
        _ = sut.StartAsync();

        invocationOrder.WaitForStart();
        var fooState1 = sut.TryGetTaskState( "foo" );
        var barState1 = sut.TryGetTaskState( "bar" );
        sut.Clear();
        var fooState2 = sut.TryGetTaskState( "foo" );
        var barState2 = sut.TryGetTaskState( "bar" );
        invocationOrder.Continue( 0 );
        invocationOrder.WaitForEnd();

        using ( new AssertionScope() )
        {
            AssertTaskState(
                fooState1,
                fooTask,
                Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskState(
                barState1,
                barTask,
                Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 246 ),
                lastInvocationTimestamp: new Timestamp( 246 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskState(
                fooState2,
                fooTask,
                Duration.Zero,
                isDisposed: true,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskState(
                barState2,
                barTask,
                Duration.Zero,
                isDisposed: true,
                firstInvocationTimestamp: new Timestamp( 246 ),
                lastInvocationTimestamp: new Timestamp( 246 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            fooState3.Should().BeNull();
            barState3.Should().BeNull();
        }
    }

    [Fact]
    public void Clear_ShouldThrowAggregateException_WhenTaskDisposalThrows()
    {
        var fooException = new Exception( "foo" );
        var barException = new Exception( "bar" );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var fooTask = new ScheduleTask( "foo", onDispose: _ => throw fooException );
        var barTask = new ScheduleTask( "bar", onDispose: _ => throw barException );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        sut.Schedule( barTask, new Timestamp( 456 ) );

        var action = Lambda.Of( () => sut.Clear() );

        using ( new AssertionScope() )
        {
            var exception = action.Should().ThrowExactly<AggregateException>().And;
            exception.InnerExceptions.Should().HaveCount( 2 );

            ((exception.InnerExceptions.ElementAtOrDefault( 0 ) as AggregateException)?.InnerExceptions).Should()
                .BeSequentiallyEqualTo( fooException );

            ((exception.InnerExceptions.ElementAtOrDefault( 1 ) as AggregateException)?.InnerExceptions).Should()
                .BeSequentiallyEqualTo( barException );

            sut.TaskKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Start_ShouldStartBlockingOperationThatEndsWhenSchedulerGetsDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var state = sut.State;

        var disposer = Task.Run(
            async () =>
            {
                await Task.Delay( 15 );
                state = sut.State;
                sut.Dispose();
            } );

        sut.Start();
        await disposer;

        state.Should().Be( ReactiveSchedulerState.Running );
    }

    [Fact]
    public async Task Start_ShouldDoNothing_WhenSchedulerIsAlreadyRunning()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var scheduler = sut.StartAsync();

        Exception? exception = null;
        try
        {
            sut.Start();
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        sut.Dispose();
        await scheduler;

        exception.Should().BeNull();
    }

    [Fact]
    public async Task Start_ShouldDoNothing_WhenSchedulerIsDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var scheduler = sut.StartAsync();
        sut.Dispose();

        Exception? exception = null;
        try
        {
            sut.Start();
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        await scheduler;

        exception.Should().BeNull();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnTaskThatEndsWhenSchedulerGetsDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var result = sut.StartAsync();
        var state = sut.State;
        var isRunning = ! result.IsCompleted;
        sut.Dispose();
        await result;

        using ( new AssertionScope() )
        {
            state.Should().Be( ReactiveSchedulerState.Running );
            isRunning.Should().BeTrue();
        }
    }

    [Fact]
    public async Task StartAsync_WithCustomScheduler_ShouldReturnTaskThatEndsWhenSchedulerGetsDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var result = sut.StartAsync( TaskScheduler.Current );
        var state = sut.State;
        var isRunning = ! result.IsCompleted;
        sut.Dispose();
        await result;

        using ( new AssertionScope() )
        {
            state.Should().Be( ReactiveSchedulerState.Running );
            isRunning.Should().BeTrue();
        }
    }

    [Fact]
    public async Task StartAsync_ShouldReturnCompletedTask_WhenSchedulerIsAlreadyRunning()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var scheduler = sut.StartAsync();
        var result = sut.StartAsync();
        var isCompleted = result.IsCompleted;
        sut.Dispose();
        await scheduler;
        await result;

        isCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_ShouldReturnCompletedTask_WhenSchedulerIsDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );

        var scheduler = sut.StartAsync();
        sut.Dispose();
        var result = sut.StartAsync();
        var isCompleted = result.IsCompleted;
        await scheduler;

        isCompleted.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldDisposeAllTasks()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var fooTask = new ScheduleTask( "foo" );
        var barTask = new ScheduleTask( "bar" );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        sut.Schedule( barTask, new Timestamp( 456 ) );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( ReactiveSchedulerState.Disposed );
            sut.TaskKeys.Should().BeEmpty();
            fooTask.IsDisposed.Should().BeTrue();
            barTask.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenSchedulerIsAlreadyDisposed()
    {
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var fooTask = new ScheduleTask( "foo" );
        var barTask = new ScheduleTask( "bar" );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        sut.Schedule( barTask, new Timestamp( 456 ) );
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Should().NotThrow();
    }

    [Fact]
    public async Task Dispose_ShouldStopSchedulerAndDisposeAllTasks()
    {
        var invocationOrder = new InvocationOrder( InvocationContinuation.Deferred( 0 ) );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var sut = new ReactiveScheduler<string>( timestamps );

        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) => invocationOrder.OnInvoke( p.InvocationId ),
            onDispose: _ => invocationOrder.EndAction() );

        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();

        invocationOrder.WaitForStart();
        var state1 = sut.TryGetTaskState( "foo" );
        sut.Dispose();
        var state2 = sut.TryGetTaskState( "foo" );
        invocationOrder.Continue( 0 );
        invocationOrder.WaitForEnd();
        await schedule;

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( ReactiveSchedulerState.Disposed );

            AssertTaskState(
                state1,
                fooTask,
                Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskState(
                state2,
                fooTask,
                Duration.Zero,
                isDisposed: true,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public void Dispose_ShouldThrowAggregateException_WhenTaskDisposalThrows()
    {
        var fooException = new Exception( "foo" );
        var barException = new Exception( "bar" );
        var timestamps = new Timestamps();
        var sut = new ReactiveScheduler<string>( timestamps );
        var fooTask = new ScheduleTask( "foo", onDispose: _ => throw fooException );
        var barTask = new ScheduleTask( "bar", onDispose: _ => throw barException );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        sut.Schedule( barTask, new Timestamp( 456 ) );

        var action = Lambda.Of( () => sut.Dispose() );

        using ( new AssertionScope() )
        {
            var exception = action.Should().ThrowExactly<AggregateException>().And;
            exception.InnerExceptions.Should().HaveCount( 2 );

            ((exception.InnerExceptions.ElementAtOrDefault( 0 ) as AggregateException)?.InnerExceptions).Should()
                .BeSequentiallyEqualTo( fooException );

            ((exception.InnerExceptions.ElementAtOrDefault( 1 ) as AggregateException)?.InnerExceptions).Should()
                .BeSequentiallyEqualTo( barException );

            sut.State.Should().Be( ReactiveSchedulerState.Disposed );
            sut.TaskKeys.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task MultipleConcurrentTaskInvocations_ShouldBeTrackedCorrectly()
    {
        var invocationOrder = new InvocationOrder(
            InvocationContinuation.Deferred( 2 ),
            InvocationContinuation.Deferred( 0 ),
            InvocationContinuation.Immediate( 1 ) );

        var interval = Duration.FromTicks( 123 );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) =>
            {
                timestamps.Move( interval );
                return invocationOrder.OnInvoke( p.InvocationId );
            },
            onComplete: (_, scheduler, p) =>
            {
                lastState = scheduler.TryGetTaskState( "foo" );
                invocationOrder.OnCompleted( p.Invocation.InvocationId );
            },
            onDispose: _ => invocationOrder.EndAction(),
            maxConcurrentInvocations: 3 );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), interval, 3 );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ),
                    new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ),
                    new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 369 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 369 ) ), null, null ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.FromTicks( 123 ),
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 369 ),
                totalInvocations: 3,
                completedInvocations: 3,
                maxActiveTasks: 3 );
        }
    }

    [Fact]
    public async Task
        MultipleConcurrentTaskInvocations_ShouldBeTrackedCorrectly_WhenMaxConcurrentInvocationsHasBeenReachedWithAvailableQueue()
    {
        var invocationOrder = new InvocationOrder(
            InvocationContinuation.Deferred( 3 ),
            InvocationContinuation.Deferred( 2 ),
            InvocationContinuation.Deferred( 0 ),
            InvocationContinuation.Manual() );

        var interval = Duration.FromTicks( 123 );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) =>
            {
                timestamps.Move( interval, count: p.InvocationId == 1 ? 2 : 1 );
                if ( p.InvocationId == 1 )
                    invocationOrder.ContinueManual();

                return invocationOrder.OnInvoke( p.InvocationId );
            },
            onComplete: (_, scheduler, p) =>
            {
                lastState = scheduler.TryGetTaskState( "foo" );
                invocationOrder.OnCompleted( p.Invocation.InvocationId );
            },
            onDispose: _ => invocationOrder.EndAction(),
            maxConcurrentInvocations: 2,
            maxEnqueuedInvocations: 2 );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), Duration.FromTicks( 123 ), 4 );
        var schedule = sut.StartAsync();
        await invocationOrder.WaitForManual();
        invocationOrder.Continue( 1 );
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ),
                    new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ),
                    new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 492 ) ),
                    new ReactiveTaskInvocationParams( 3, new Timestamp( 492 ), new Timestamp( 615 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 492 ) ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 3, new Timestamp( 492 ), new Timestamp( 615 ) ), null, null ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo(
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 492 ) ), 0 ),
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 3, new Timestamp( 492 ), new Timestamp( 492 ) ), 1 ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.FromTicks( 123 ),
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 492 ),
                totalInvocations: 4,
                completedInvocations: 4,
                delayedInvocations: 2,
                maxActiveTasks: 2,
                maxQueuedInvocations: 2 );
        }
    }

    [Fact]
    public async Task
        MultipleConcurrentTaskInvocations_ShouldBeTrackedCorrectly_WhenMaxConcurrentInvocationsHasBeenReachedWithDisabledQueue()
    {
        var invocationOrder = new InvocationOrder(
            InvocationContinuation.Deferred( 1 ),
            InvocationContinuation.Deferred( 2 ),
            InvocationContinuation.Deferred( 0 ) );

        var interval = Duration.FromTicks( 123 );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) =>
            {
                timestamps.Move( interval );
                return invocationOrder.OnInvoke( p.InvocationId );
            },
            onComplete: (_, scheduler, p) =>
            {
                lastState = scheduler.TryGetTaskState( "foo" );
                invocationOrder.OnCompleted( p.Invocation.InvocationId );
            },
            onDispose: _ => invocationOrder.EndAction(),
            maxConcurrentInvocations: 2 );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), Duration.FromTicks( 123 ), 3 );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ),
                    new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 369 ) ),
                        null,
                        TaskCancellationReason.MaxQueueSizeLimit ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ), null, null ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.FromTicks( 123 ),
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 369 ),
                totalInvocations: 3,
                completedInvocations: 2,
                skippedInvocations: 1,
                maxActiveTasks: 2 );
        }
    }

    [Fact]
    public async Task SingleConcurrentTaskInvocations_ShouldBeTrackedCorrectly_WithLimitedQueue()
    {
        var invocationOrder = new InvocationOrder(
            InvocationContinuation.Deferred( 2 ),
            InvocationContinuation.Deferred( 0 ),
            InvocationContinuation.Deferred( 3 ),
            InvocationContinuation.Deferred( 1 ) );

        var interval = Duration.FromTicks( 123 );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) =>
            {
                timestamps.Move( interval, count: p.InvocationId == 0 ? 3 : 1 );
                return invocationOrder.OnInvoke( p.InvocationId );
            },
            onComplete: (_, scheduler, p) =>
            {
                lastState = scheduler.TryGetTaskState( "foo" );
                invocationOrder.OnCompleted( p.Invocation.InvocationId );
            },
            onDispose: _ => invocationOrder.EndAction(),
            maxEnqueuedInvocations: 2 );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), Duration.FromTicks( 123 ), 4 );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ),
                    new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 492 ) ),
                    new ReactiveTaskInvocationParams( 3, new Timestamp( 492 ), new Timestamp( 615 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 492 ) ),
                        null,
                        TaskCancellationReason.MaxQueueSizeLimit ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 492 ) ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 3, new Timestamp( 492 ), new Timestamp( 615 ) ), null, null ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo(
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 492 ) ), 0 ),
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 492 ) ), 1 ),
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 3, new Timestamp( 492 ), new Timestamp( 492 ) ), 2 ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.FromTicks( 123 ),
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 492 ),
                totalInvocations: 4,
                completedInvocations: 3,
                delayedInvocations: 2,
                skippedInvocations: 1,
                maxActiveTasks: 1,
                maxQueuedInvocations: 2 );
        }
    }

    [Fact]
    public async Task TaskEnqueueCancellation_ShouldCompleteItImmediatelyWithCancellationRequestedReason()
    {
        var invocationOrder = new InvocationOrder( InvocationContinuation.Immediate( 1 ), InvocationContinuation.Deferred( 0 ) );
        var interval = Duration.FromTicks( 123 );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) =>
            {
                timestamps.Move( interval );
                return invocationOrder.OnInvoke( p.InvocationId );
            },
            onEnqueue: (_, _, _, _) => false,
            onComplete: (_, scheduler, p) =>
            {
                lastState = scheduler.TryGetTaskState( "foo" );
                invocationOrder.OnCompleted( p.Invocation.InvocationId );
            },
            onDispose: _ => invocationOrder.EndAction(),
            maxEnqueuedInvocations: 1 );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), interval, 2 );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ),
                        null,
                        TaskCancellationReason.CancellationRequested ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo(
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ), 0 ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: interval,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 246 ),
                totalInvocations: 2,
                completedInvocations: 1,
                skippedInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public async Task ExceptionDuringTaskInvocation_ShouldBeCaughtAndHandled()
    {
        var exception = new Exception();
        var invocationOrder = new InvocationOrder();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, _, _) => throw exception,
            onComplete: (_, scheduler, _) => lastState = scheduler.TryGetTaskState( "foo" ),
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ),
                        exception,
                        null ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                completedInvocations: 1,
                failedInvocations: 1 );
        }
    }

    [Fact]
    public async Task ExceptionDuringTaskProcessing_ShouldBeCaughtAndHandled()
    {
        var exception = new Exception();
        var invocationOrder = new InvocationOrder( InvocationContinuation.Immediate( 0 ) );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) => invocationOrder.OnInvoke( p.InvocationId, callback: () => throw exception ),
            onComplete: (_, scheduler, _) => lastState = scheduler.TryGetTaskState( "foo" ),
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should().HaveCount( 1 );
            fooTask.Completions.ElementAtOrDefault( 0 )
                .Invocation.Should()
                .Be( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            ((fooTask.Completions.ElementAtOrDefault( 0 ).Exception as AggregateException)?.InnerExceptions).Should().HaveCount( 1 );
            ((fooTask.Completions.ElementAtOrDefault( 0 ).Exception as AggregateException)?.InnerExceptions.ElementAtOrDefault( 0 ))
                .Should()
                .BeSameAs( exception );

            fooTask.Completions.ElementAtOrDefault( 0 ).CancellationReason.Should().BeNull();

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                completedInvocations: 1,
                failedInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public async Task ExceptionDuringTaskCompletion_ShouldBeCaughtAndIgnored()
    {
        var exception = new Exception();
        var invocationOrder = new InvocationOrder( InvocationContinuation.Immediate( 0 ) );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) => invocationOrder.OnInvoke( p.InvocationId ),
            onComplete: (_, scheduler, _) =>
            {
                lastState = scheduler.TryGetTaskState( "foo" );
                throw exception;
            },
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                completedInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public async Task ExceptionDuringTaskEnqueue_ShouldBeCaughtAndIgnoredAndCauseInvocationToBeCancelled()
    {
        var exception = new Exception();
        var invocationOrder = new InvocationOrder( InvocationContinuation.Immediate( 1 ), InvocationContinuation.Deferred( 0 ) );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) =>
            {
                timestamps.Move( Duration.FromTicks( 123 ) );
                return invocationOrder.OnInvoke( p.InvocationId );
            },
            onEnqueue: (_, scheduler, _, _) =>
            {
                lastState = scheduler.TryGetTaskState( "foo" );
                throw exception;
            },
            onComplete: (_, _, p) => invocationOrder.OnCompleted( p.Invocation.InvocationId ),
            onDispose: _ => invocationOrder.EndAction(),
            maxEnqueuedInvocations: 1 );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), Duration.FromTicks( 123 ), 2 );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ),
                        null,
                        TaskCancellationReason.CancellationRequested ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo(
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ), 0 ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.FromTicks( 123 ),
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 246 ),
                totalInvocations: 2,
                skippedInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public async Task TaskCancellation_ShouldBeCaughtAndHandled()
    {
        var invocationOrder = new InvocationOrder();
        var taskSource = new TaskCompletionSource();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, _, _) =>
            {
                invocationOrder.ContinueManual();
                return taskSource.Task;
            },
            onComplete: (_, scheduler, _) => lastState = scheduler.TryGetTaskState( "foo" ),
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();
        await invocationOrder.WaitForManual();
        taskSource.SetCanceled();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ),
                        null,
                        TaskCancellationReason.CancellationRequested ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                completedInvocations: 1,
                cancelledInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public async Task TaskInCreatedState_ShouldBeStartedAutomatically()
    {
        var invocationOrder = new InvocationOrder();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, _, _) => new Task( () => { } ),
            onComplete: (_, scheduler, _) => lastState = scheduler.TryGetTaskState( "foo" ),
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                completedInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public async Task SynchronousTask_ShouldBeInvokedCorrectly()
    {
        var invocationOrder = new InvocationOrder();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        ScheduleTaskState<string>? lastState = null;
        var fooTask = new ScheduleTask(
            "foo",
            onComplete: (_, scheduler, _) => lastState = scheduler.TryGetTaskState( "foo" ),
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ) );

            AssertTaskState(
                lastState,
                fooTask,
                interval: Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                completedInvocations: 1 );
        }
    }

    [Fact]
    public void Remove_ShouldRethrowSafely_WhenTaskCancellationTokenSourceDisposalThrows()
    {
        var exception = new Exception();
        var invocationOrder = new InvocationOrder();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, _, ct) =>
            {
                ct.Register( () => throw exception );
                return Task.CompletedTask;
            },
            onComplete: (_, _, _) => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), Duration.FromHours( 1 ), 2 );
        _ = sut.StartAsync();
        invocationOrder.WaitForEnd();

        var action = Lambda.Of( () => sut.Remove( "foo" ) );

        using ( new AssertionScope() )
        {
            var exc = action.Should().ThrowExactly<AggregateException>().And;
            var inner = exc.InnerExceptions.ElementAtOrDefault( 0 ) as AggregateException;
            (inner?.InnerExceptions).Should().BeSequentiallyEqualTo( exception );
        }
    }

    [Fact]
    public async Task TaskDisposal_WithQueuedTasks_ShouldInvokeCorrectCallbacks()
    {
        var interval = Duration.FromTicks( 123 );
        var invocationOrder = new InvocationOrder(
            InvocationContinuation.Deferred( 2 ),
            InvocationContinuation.Manual( isImmediate: false ),
            InvocationContinuation.Deferred( 1 ) );

        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, p, _) =>
            {
                timestamps.Move( interval, count: p.InvocationId == 0 ? 2 : 1 );
                return invocationOrder.OnInvoke( p.InvocationId );
            },
            onComplete: (_, _, p) => invocationOrder.OnCompleted( p.Invocation.InvocationId ),
            onDispose: _ => invocationOrder.EndAction(),
            maxEnqueuedInvocations: 1 );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), interval, 3 );
        var schedule = sut.StartAsync();
        await invocationOrder.WaitForManual();
        sut.Remove( "foo" );
        invocationOrder.Continue( 0 );
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 369 ) ),
                        null,
                        TaskCancellationReason.MaxQueueSizeLimit ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ),
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 369 ) ),
                        null,
                        TaskCancellationReason.TaskDisposed ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo(
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 369 ) ), 0 ),
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 369 ) ), 1 ) );
        }
    }

    [Fact]
    public async Task TaskWithSingleInvocation_ShouldBePossibleToRescheduleAndPreserveState()
    {
        var interval = Duration.FromTicks( 123 );
        var invocationOrder = new InvocationOrder();
        var state = new List<ScheduleTaskState<string>?>();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, _, _) => Task.CompletedTask,
            onComplete: (t, scheduler, p) =>
            {
                state.Add( scheduler.TryGetTaskState( "foo" ) );
                if ( p.Invocation.InvocationId <= 1 )
                    scheduler.Schedule( t, new Timestamp( 123 ) + interval * (p.Invocation.InvocationId + 1) );

                invocationOrder.ContinueManual();
            },
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();
        await invocationOrder.WaitForManual();
        timestamps.Move( interval );
        await invocationOrder.WaitForManual();
        timestamps.Move( interval );
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ),
                    new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ),
                    new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 369 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, new Timestamp( 369 ), new Timestamp( 369 ) ), null, null ) );

            state.Should().HaveCount( 3 );

            AssertTaskState(
                state.ElementAtOrDefault( 0 ),
                fooTask,
                interval: Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 123 ),
                totalInvocations: 1,
                completedInvocations: 1 );

            AssertTaskState(
                state.ElementAtOrDefault( 1 ),
                fooTask,
                interval: Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 246 ),
                totalInvocations: 2,
                completedInvocations: 2 );

            AssertTaskState(
                state.ElementAtOrDefault( 2 ),
                fooTask,
                interval: Duration.Zero,
                repetitions: 0,
                firstInvocationTimestamp: new Timestamp( 123 ),
                lastInvocationTimestamp: new Timestamp( 369 ),
                totalInvocations: 3,
                completedInvocations: 3 );
        }
    }

    [Fact]
    public async Task SchedulerDisposal_FromTaskInvocation_ShouldDisposeSchedulerAndFinishCurrentInvocation()
    {
        var invocationOrder = new InvocationOrder();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, scheduler, _, _) =>
            {
                timestamps.Move( Duration.FromTicks( 123 ) );
                scheduler.Dispose();
                return new Task( () => { } );
            },
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), Duration.FromHours( 2 ), 2 );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ) );
        }
    }

    [Fact]
    public async Task SchedulerDisposal_FromTaskCompletion_ShouldDisposeSchedulerAndFinishCurrentInvocation()
    {
        var invocationOrder = new InvocationOrder();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, _, _) => new Task( () => { } ),
            onComplete: (_, scheduler, _) =>
            {
                timestamps.Move( Duration.FromTicks( 123 ) );
                scheduler.Dispose();
            },
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), Duration.FromHours( 2 ), 2 );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ) );
        }
    }

    [Fact]
    public async Task SchedulerDisposal_FromTaskEnqueue_ShouldDisposeSchedulerAndFinishCurrentInvocation()
    {
        var invocationOrder = new InvocationOrder( InvocationContinuation.Immediate( 1 ), InvocationContinuation.Deferred( 0 ) );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (t, schedule, p, _) =>
            {
                schedule.Schedule( t, new Timestamp( 246 ) );
                timestamps.Move( Duration.FromTicks( 123 ) );
                return invocationOrder.OnInvoke( p.InvocationId );
            },
            onEnqueue: (_, scheduler, p, _) =>
            {
                scheduler.Dispose();
                invocationOrder.OnCompleted( p.InvocationId );
                return true;
            },
            onDispose: _ => invocationOrder.EndAction(),
            maxEnqueuedInvocations: 1 );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ),
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ),
                        null,
                        TaskCancellationReason.TaskDisposed ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo(
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ), 0 ) );
        }
    }

    [Fact]
    public async Task SchedulerDisposal_FromTaskDisposal_ShouldDisposeSchedulerAndFinishCurrentInvocation()
    {
        var invocationOrder = new InvocationOrder();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var sut = new ReactiveScheduler<string>( timestamps );
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, _, _) => new Task( () => { } ),
            onDispose: _ =>
            {
                sut.Dispose();
                invocationOrder.EndAction();
            } );

        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ) );
        }
    }

    [Fact]
    public async Task TaskDisposal_FromTaskInvocation_ShouldDisposeTaskAndFinishCurrentInvocation()
    {
        var invocationOrder = new InvocationOrder();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, scheduler, _, _) =>
            {
                timestamps.Move( Duration.FromTicks( 123 ) );
                scheduler.Remove( "foo" );
                return new Task( () => { } );
            },
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), Duration.FromHours( 2 ), 2 );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ) );
        }
    }

    [Fact]
    public async Task TaskDisposal_FromTaskCompletion_ShouldDisposeTaskAndFinishCurrentInvocation()
    {
        var invocationOrder = new InvocationOrder();
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (_, _, _, _) => new Task( () => { } ),
            onComplete: (_, scheduler, _) =>
            {
                timestamps.Move( Duration.FromTicks( 123 ) );
                scheduler.Remove( "foo" );
            },
            onDispose: _ => invocationOrder.EndAction() );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), Duration.FromHours( 2 ), 2 );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ) );
        }
    }

    [Fact]
    public async Task TaskDisposal_FromTaskEnqueue_ShouldDisposeTaskAndFinishCurrentInvocation()
    {
        var invocationOrder = new InvocationOrder( InvocationContinuation.Immediate( 1 ), InvocationContinuation.Deferred( 0 ) );
        var timestamps = new Timestamps( Timestamp.Zero, new Timestamp( 123 ) );
        var fooTask = new ScheduleTask(
            "foo",
            onInvoke: (t, schedule, p, _) =>
            {
                schedule.Schedule( t, new Timestamp( 246 ) );
                timestamps.Move( Duration.FromTicks( 123 ) );
                return invocationOrder.OnInvoke( p.InvocationId );
            },
            onEnqueue: (_, scheduler, p, _) =>
            {
                scheduler.Remove( "foo" );
                invocationOrder.OnCompleted( p.InvocationId );
                return true;
            },
            onDispose: _ => invocationOrder.EndAction(),
            maxEnqueuedInvocations: 1 );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ) );
        var schedule = sut.StartAsync();
        invocationOrder.WaitForEnd();
        sut.Dispose();
        await schedule;

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) ), null, null ),
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ),
                        null,
                        TaskCancellationReason.TaskDisposed ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo(
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 1, new Timestamp( 246 ), new Timestamp( 246 ) ), 0 ) );
        }
    }

    [Fact]
    public void ManyScheduledTasks_ShouldBeInvokedInCorrectOrder()
    {
        var invocationOrder = new InvocationOrder();
        var timestamps = new Timestamps();
        var invocations = new List<(string Key, ReactiveTaskInvocationParams Parameters)>();
        var fooTask = new ScheduleTask(
            "foo",
            onComplete: (_, _, p) =>
            {
                invocations.Add( ("foo", p.Invocation) );

                if ( p.Invocation.InvocationId == 0 )
                    timestamps.AddValues( new Timestamp( 150 ) );

                if ( p.Invocation.InvocationId == 1 )
                    timestamps.AddValues( new Timestamp( 200 ) );
            } );

        var barTask = new ScheduleTask(
            "bar",
            onComplete: (_, _, p) =>
            {
                invocations.Add( ("bar", p.Invocation) );
                if ( p.Invocation.InvocationId == 0 )
                    timestamps.AddValues( new Timestamp( 165 ) );
            } );

        var quxTask = new ScheduleTask(
            "qux",
            onComplete: (_, _, p) =>
            {
                invocations.Add( ("qux", p.Invocation) );

                if ( p.Invocation.InvocationId == 0 )
                    timestamps.AddValues( new Timestamp( 123 ) );

                if ( p.Invocation.InvocationId == 1 )
                    timestamps.Move( Duration.FromTicks( 150 ) );

                if ( p.Invocation.InvocationId == 2 )
                    invocationOrder.EndAction();
            } );

        var sut = new ReactiveScheduler<string>( timestamps );
        sut.Schedule( fooTask, new Timestamp( 123 ), Duration.FromTicks( 42 ), 2 );
        sut.Schedule( barTask, new Timestamp( 150 ) );
        sut.ScheduleInfinite( quxTask, new Timestamp( 50 ), Duration.FromTicks( 150 ) );
        _ = sut.StartAsync();
        timestamps.AddValues( new Timestamp( 50 ) );
        invocationOrder.WaitForEnd();

        invocations.Should()
            .BeSequentiallyEqualTo(
                ("qux", new ReactiveTaskInvocationParams( 0, new Timestamp( 50 ), new Timestamp( 50 ) )),
                ("foo", new ReactiveTaskInvocationParams( 0, new Timestamp( 123 ), new Timestamp( 123 ) )),
                ("bar", new ReactiveTaskInvocationParams( 0, new Timestamp( 150 ), new Timestamp( 150 ) )),
                ("foo", new ReactiveTaskInvocationParams( 1, new Timestamp( 165 ), new Timestamp( 165 ) )),
                ("qux", new ReactiveTaskInvocationParams( 1, new Timestamp( 200 ), new Timestamp( 200 ) )),
                ("qux", new ReactiveTaskInvocationParams( 2, new Timestamp( 350 ), new Timestamp( 350 ) )) );
    }

    private static void AssertTaskState(
        ScheduleTaskState<string>? state,
        ScheduleTask task,
        Duration interval,
        Timestamp? nextTimestamp = null,
        int? repetitions = null,
        bool isDisposed = false,
        bool isInfinite = false,
        Timestamp? firstInvocationTimestamp = null,
        Timestamp? lastInvocationTimestamp = null,
        long totalInvocations = 0,
        long activeInvocations = 0,
        long completedInvocations = 0,
        long skippedInvocations = 0,
        long delayedInvocations = 0,
        long failedInvocations = 0,
        long cancelledInvocations = 0,
        long queuedInvocations = 0,
        long maxQueuedInvocations = 0,
        long activeTasks = 0,
        long maxActiveTasks = 0)
    {
        (state?.NextTimestamp).Should().Be( nextTimestamp );
        (state?.Interval).Should().Be( interval );
        (state?.Repetitions).Should().Be( repetitions );
        (state?.IsDisposed).Should().Be( isDisposed );
        (state?.IsInfinite).Should().Be( isInfinite );
        (state?.State.Task).Should().BeSameAs( task );
        (state?.State.FirstInvocationTimestamp).Should().Be( firstInvocationTimestamp );
        (state?.State.LastInvocationTimestamp).Should().Be( lastInvocationTimestamp );
        (state?.State.TotalInvocations).Should().Be( totalInvocations );
        (state?.State.ActiveInvocations).Should().Be( activeInvocations );
        (state?.State.CompletedInvocations).Should().Be( completedInvocations );
        (state?.State.SkippedInvocations).Should().Be( skippedInvocations );
        (state?.State.DelayedInvocations).Should().Be( delayedInvocations );
        (state?.State.FailedInvocations).Should().Be( failedInvocations );
        (state?.State.CancelledInvocations).Should().Be( cancelledInvocations );
        (state?.State.QueuedInvocations).Should().Be( queuedInvocations );
        (state?.State.MaxQueuedInvocations).Should().Be( maxQueuedInvocations );
        (state?.State.ActiveTasks).Should().Be( activeTasks );
        (state?.State.MaxActiveTasks).Should().Be( maxActiveTasks );
        if ( completedInvocations > 0 )
        {
            (state?.State.MinElapsedTime.Ticks).Should().BeGreaterOrEqualTo( 0 );
            (state?.State.MinElapsedTime.Ticks).Should().BeLessOrEqualTo( state?.State.MaxElapsedTime.Ticks ?? 0 );
            (state?.State.MaxElapsedTime.Ticks).Should().BeGreaterOrEqualTo( state?.State.MinElapsedTime.Ticks ?? 0 );
            (state?.State.AverageElapsedTime.Ticks).Should().BeGreaterOrEqualTo( state?.State.MinElapsedTime.Ticks ?? 0 );
            (state?.State.AverageElapsedTime.Ticks).Should().BeLessOrEqualTo( state?.State.MaxElapsedTime.Ticks ?? 0 );
        }
        else
        {
            (state?.State.MinElapsedTime).Should().Be( Duration.MaxValue );
            (state?.State.MaxElapsedTime).Should().Be( Duration.MinValue );
            (state?.State.AverageElapsedTime).Should().Be( FloatingDuration.Zero );
        }
    }

    private sealed class ScheduleTask : ScheduleTask<string>
    {
        public ScheduleTask(
            string key,
            int maxEnqueuedInvocations = 0,
            int maxConcurrentInvocations = 1,
            Func<ScheduleTask, IReactiveScheduler<string>, ReactiveTaskInvocationParams, CancellationToken, Task>? onInvoke = null,
            Action<ScheduleTask, IReactiveScheduler<string>, ReactiveTaskCompletionParams>? onComplete = null,
            Func<ScheduleTask, IReactiveScheduler<string>, ReactiveTaskInvocationParams, int, bool>? onEnqueue = null,
            Action<ScheduleTask>? onDispose = null)
            : base( key, maxEnqueuedInvocations, maxConcurrentInvocations )
        {
            IsDisposed = false;
            Invocations = new List<ReactiveTaskInvocationParams>();
            Completions = new List<TaskCompletion>();
            Enqueues = new List<TaskEnqueue>();
            OnInvokeCallback = onInvoke;
            OnCompleteCallback = onComplete;
            OnEnqueueCallback = onEnqueue;
            OnDisposeCallback = onDispose;
        }

        public bool IsDisposed { get; private set; }
        public List<ReactiveTaskInvocationParams> Invocations { get; }
        public List<TaskCompletion> Completions { get; }
        public List<TaskEnqueue> Enqueues { get; }

        public Func<ScheduleTask, IReactiveScheduler<string>, ReactiveTaskInvocationParams, CancellationToken, Task>?
            OnInvokeCallback { get; }

        public Action<ScheduleTask, IReactiveScheduler<string>, ReactiveTaskCompletionParams>? OnCompleteCallback { get; }
        public Func<ScheduleTask, IReactiveScheduler<string>, ReactiveTaskInvocationParams, int, bool>? OnEnqueueCallback { get; }
        public Action<ScheduleTask>? OnDisposeCallback { get; }

        public override void Dispose()
        {
            base.Dispose();
            IsDisposed = true;
            OnDisposeCallback?.Invoke( this );
        }

        public override Task InvokeAsync(
            IReactiveScheduler<string> scheduler,
            ReactiveTaskInvocationParams parameters,
            CancellationToken cancellationToken)
        {
            Invocations.Add( parameters );
            return OnInvokeCallback is null ? Task.CompletedTask : OnInvokeCallback( this, scheduler, parameters, cancellationToken );
        }

        public override void OnCompleted(IReactiveScheduler<string> scheduler, ReactiveTaskCompletionParams parameters)
        {
            base.OnCompleted( scheduler, parameters );
            Completions.Add( new TaskCompletion( parameters.Invocation, parameters.Exception, parameters.CancellationReason ) );
            OnCompleteCallback?.Invoke( this, scheduler, parameters );
        }

        public override bool OnEnqueue(IReactiveScheduler<string> scheduler, ReactiveTaskInvocationParams parameters, int positionInQueue)
        {
            Enqueues.Add( new TaskEnqueue( parameters, positionInQueue ) );
            return OnEnqueueCallback?.Invoke( this, scheduler, parameters, positionInQueue )
                ?? base.OnEnqueue( scheduler, parameters, positionInQueue );
        }
    }

    private readonly record struct TaskCompletion(
        ReactiveTaskInvocationParams Invocation,
        Exception? Exception,
        TaskCancellationReason? CancellationReason
    );

    private readonly record struct TaskEnqueue(ReactiveTaskInvocationParams Invocation, int PositionInQueue);

    private readonly record struct InvocationContinuation(long TargetId, bool IsImmediate)
    {
        [Pure]
        public static InvocationContinuation Deferred(long targetId)
        {
            return new InvocationContinuation( targetId, false );
        }

        [Pure]
        public static InvocationContinuation Immediate(long targetId)
        {
            return new InvocationContinuation( targetId, true );
        }

        [Pure]
        public static InvocationContinuation Manual(bool isImmediate = true)
        {
            return new InvocationContinuation( -1, isImmediate );
        }
    }

    private sealed class InvocationOrder
    {
        private readonly InvocationContinuation[] _continuations;
        private readonly ManualResetEventSlim[] _resetEvents;
        private readonly ManualResetEventSlim _manual;
        private readonly ManualResetEventSlim _start;
        private readonly ManualResetEventSlim _end;
        private readonly int _actions;
        private InterlockedInt32 _actionCount;

        public InvocationOrder(params InvocationContinuation[] continuations)
            : this( 1, continuations ) { }

        public InvocationOrder(int actions, params InvocationContinuation[] continuations)
        {
            _actions = actions;
            _actionCount = new InterlockedInt32( 0 );
            _continuations = continuations;
            _resetEvents = new ManualResetEventSlim[continuations.Length];
            for ( var i = 0; i < _resetEvents.Length; ++i )
                _resetEvents[i] = new ManualResetEventSlim();

            _manual = new ManualResetEventSlim();
            _start = new ManualResetEventSlim();
            _end = new ManualResetEventSlim();
        }

        public void Start()
        {
            _start.Set();
        }

        public void EndAction()
        {
            if ( _actionCount.Increment() >= _actions )
                _end.Set();
        }

        public void WaitForStart()
        {
            _start.Wait();
        }

        public void WaitForEnd()
        {
            _end.Wait();
        }

        public async Task WaitForManual()
        {
            _manual.Wait();
            _manual.Reset();
            await Task.Delay( 15 );
        }

        public void ContinueManual()
        {
            _manual.Set();
        }

        public void Continue(long invocationId)
        {
            _resetEvents[invocationId].Set();
        }

        public void WaitFor(long invocationId)
        {
            _resetEvents[invocationId].Wait();
        }

        public Task OnInvoke(long invocationId, bool autoStart = true, Action? callback = null)
        {
            if ( autoStart )
                Start();

            var continuation = _continuations[invocationId];
            return Task.Run(
                () =>
                {
                    callback?.Invoke();
                    if ( continuation.IsImmediate )
                        ContinueCore( continuation.TargetId );

                    WaitFor( invocationId );
                } );
        }

        public void OnCompleted(long invocationId)
        {
            var continuation = _continuations[invocationId];
            if ( ! continuation.IsImmediate )
                ContinueCore( continuation.TargetId );
        }

        private void ContinueCore(long invocationId)
        {
            if ( invocationId == -1 )
                ContinueManual();
            else
                Continue( invocationId );
        }
    }

    private sealed class Timestamps : TimestampProviderBase
    {
        private readonly List<Timestamp> _values;

        public Timestamps(params Timestamp[] values)
        {
            Index = 0;
            _values = new List<Timestamp>( values );
            if ( _values.Count == 0 )
                _values.Add( Timestamp.Zero );
        }

        public int Index { get; private set; }

        [Pure]
        public override Timestamp GetNow()
        {
            using ( ExclusiveLock.Enter( this ) )
                return Index >= _values.Count ? _values[^1] : _values[Index++];
        }

        public void Move(Duration interval, int count = 1)
        {
            using ( ExclusiveLock.Enter( this ) )
            {
                for ( var i = 0; i < count; ++i )
                    _values.Add( _values[^1] + interval );
            }
        }

        public void AddValues(params Timestamp[] values)
        {
            using ( ExclusiveLock.Enter( this ) )
                _values.AddRange( values );
        }
    }
}
