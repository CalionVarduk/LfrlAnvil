using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Chrono.Tests;

public class TimerTaskCollectionTests : TestsBase
{
    [Fact]
    public void RegisterTasks_ShouldReturnDisposedCollection_WhenTasksAreEmpty()
    {
        var tasks = Array.Empty<ITimerTask<string>>();
        var source = new EventPublisher<WithInterval<long>>();

        var result = source.RegisterTasks( tasks );

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeFalse();
            result.FirstTimestamp.Should().BeNull();
            result.LastTimestamp.Should().BeNull();
            result.EventCount.Should().Be( 0 );
        }
    }

    [Fact]
    public void RegisterTasks_ShouldReturnActiveCollection_WhenTasksAreNotEmpty()
    {
        var tasks = new[] { TimerTask.CreateCompleted( "foo" ) };
        var source = new EventPublisher<WithInterval<long>>();

        var result = source.RegisterTasks( tasks );

        using ( new AssertionScope() )
        {
            source.Subscribers.Should().HaveCount( 1 );
            result.FirstTimestamp.Should().BeNull();
            result.LastTimestamp.Should().BeNull();
            result.EventCount.Should().Be( 0 );
            result.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskStateSnapshot( result.TryGetTaskState( "foo" ), tasks[0] );
        }
    }

    [Fact]
    public void SourceEvent_ShouldInvokeAllTasks()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;
        var timestamp3 = timestamp2 + interval;

        var fooTask = TimerTask.CreateCompleted( "foo" );
        var barTask = TimerTask.CreateCompleted( "bar" );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask, barTask } );

        PublishEvents( source, timestamp1, timestamp2, timestamp3 );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp3 );
            sut.EventCount.Should().Be( 3 );

            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ),
                    new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ),
                    new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ), null, false ) );

            barTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ),
                    new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ),
                    new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ) );

            barTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ), null, false ) );

            AssertTaskStateSnapshot(
                sut.TryGetTaskState( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3 );

            AssertTaskStateSnapshot(
                sut.TryGetTaskState( "bar" ),
                barTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3 );
        }
    }

    [Theory]
    [InlineData( 122 )]
    [InlineData( 123 )]
    public void TaskWithNextInvocationTimestamp_ShouldBeInvoked_WhenEventTimestampIsLessThanOrEqualToIt(long nextInvocationTicks)
    {
        var timestamp = new Timestamp( 123 );
        var fooTask = TimerTask.CreateCompleted( "foo", _ => new Timestamp( nextInvocationTicks ) );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp );
            sut.LastTimestamp.Should().Be( timestamp );
            sut.EventCount.Should().Be( 1 );

            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp, timestamp ) );
            fooTask.Completions.Should()
                .BeSequentiallyEqualTo( new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp, timestamp ), null, false ) );

            AssertTaskStateSnapshot(
                sut.TryGetTaskState( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp,
                lastInvocationTimestamp: timestamp,
                totalInvocations: 1 );
        }
    }

    [Fact]
    public void TaskWithNextInvocationTimestamp_ShouldNotBeInvoked_WhenEventTimestampIsGreaterThanIt()
    {
        var timestamp = new Timestamp( 123 );
        var fooTask = TimerTask.CreateCompleted( "foo", _ => timestamp + Duration.FromTicks( 1 ) );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp );
            sut.LastTimestamp.Should().Be( timestamp );
            sut.EventCount.Should().Be( 1 );
            fooTask.Invocations.Should().BeEmpty();
            AssertTaskStateSnapshot( sut.TryGetTaskState( "foo" ), fooTask );
        }
    }

    [Fact]
    public void MultipleConcurrentTaskInvocations_ShouldBeTrackedCorrectly()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;
        var timestamp3 = timestamp2 + interval;

        var taskIndex = 0;
        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource() };
        var fooTask = TimerTask.CreateFromSource( "foo", _ => taskSources[taskIndex++], maxConcurrentInvocations: 3 );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        var snapshot1 = sut.TryGetTaskState( "foo" );
        PublishEvents( source, timestamp2 );
        var snapshot2 = sut.TryGetTaskState( "foo" );
        taskSources[1].SetResult();
        var snapshot3 = sut.TryGetTaskState( "foo" );
        PublishEvents( source, timestamp3 );
        var snapshot4 = sut.TryGetTaskState( "foo" );
        taskSources[0].SetResult();
        var snapshot5 = sut.TryGetTaskState( "foo" );
        taskSources[2].SetResult();
        var snapshot6 = sut.TryGetTaskState( "foo" );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp3 );
            sut.EventCount.Should().Be( 3 );

            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ),
                    new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ),
                    new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ), null, false ) );

            AssertTaskStateSnapshot(
                snapshot1,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskStateSnapshot(
                snapshot2,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 2,
                maxActiveTasks: 2 );

            AssertTaskStateSnapshot(
                snapshot3,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 1,
                maxActiveTasks: 2 );

            AssertTaskStateSnapshot(
                snapshot4,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                activeTasks: 2,
                maxActiveTasks: 2 );

            AssertTaskStateSnapshot(
                snapshot5,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                activeTasks: 1,
                maxActiveTasks: 2 );

            AssertTaskStateSnapshot(
                snapshot6,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                maxActiveTasks: 2 );
        }
    }

    [Fact]
    public void MultipleConcurrentTaskInvocations_ShouldBeTrackedCorrectly_WhenMaxConcurrentInvocationsHasBeenReachedWithAvailableQueue()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;
        var timestamp3 = timestamp2 + interval;
        var timestamp4 = timestamp3 + interval;

        var taskIndex = 0;
        var taskSources = new[]
        {
            new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource()
        };

        var fooTask = TimerTask.CreateFromSource(
            "foo",
            _ => taskSources[taskIndex++],
            maxConcurrentInvocations: 2,
            maxEnqueuedInvocations: 2 );

        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        var snapshot1 = sut.TryGetTaskState( "foo" );
        PublishEvents( source, timestamp2 );
        var snapshot2 = sut.TryGetTaskState( "foo" );
        PublishEvents( source, timestamp3 );
        var snapshot3 = sut.TryGetTaskState( "foo" );
        PublishEvents( source, timestamp4 );
        var snapshot4 = sut.TryGetTaskState( "foo" );
        taskSources[1].SetResult();
        var snapshot5 = sut.TryGetTaskState( "foo" );
        taskSources[2].SetResult();
        var snapshot6 = sut.TryGetTaskState( "foo" );
        taskSources[0].SetResult();
        var snapshot7 = sut.TryGetTaskState( "foo" );
        taskSources[3].SetResult();
        var snapshot8 = sut.TryGetTaskState( "foo" );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp4 );
            sut.EventCount.Should().Be( 4 );

            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ),
                    new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ),
                    new ReactiveTaskInvocationParams( 2, timestamp3, timestamp4 ),
                    new ReactiveTaskInvocationParams( 3, timestamp4, timestamp4 ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp4 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 3, timestamp4, timestamp4 ), null, false ) );

            AssertTaskStateSnapshot(
                snapshot1,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskStateSnapshot(
                snapshot2,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 2,
                maxActiveTasks: 2 );

            AssertTaskStateSnapshot(
                snapshot3,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 2,
                maxActiveTasks: 2,
                queuedInvocations: 1,
                maxQueuedInvocations: 1 );

            AssertTaskStateSnapshot(
                snapshot4,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 2,
                maxActiveTasks: 2,
                queuedInvocations: 2,
                maxQueuedInvocations: 2 );

            AssertTaskStateSnapshot(
                snapshot5,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                activeTasks: 2,
                maxActiveTasks: 2,
                queuedInvocations: 1,
                maxQueuedInvocations: 2,
                delayedInvocations: 1 );

            AssertTaskStateSnapshot(
                snapshot6,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                activeTasks: 2,
                maxActiveTasks: 2,
                maxQueuedInvocations: 2,
                delayedInvocations: 2 );

            AssertTaskStateSnapshot(
                snapshot7,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                activeTasks: 1,
                maxActiveTasks: 2,
                maxQueuedInvocations: 2,
                delayedInvocations: 2 );

            AssertTaskStateSnapshot(
                snapshot8,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                maxActiveTasks: 2,
                maxQueuedInvocations: 2,
                delayedInvocations: 2 );
        }
    }

    [Fact]
    public void MultipleConcurrentTaskInvocations_ShouldBeTrackedCorrectly_WhenMaxConcurrentInvocationsHasBeenReachedWithDisabledQueue()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;
        var timestamp3 = timestamp2 + interval;

        var taskIndex = 0;
        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource() };

        var fooTask = TimerTask.CreateFromSource(
            "foo",
            _ => taskSources[taskIndex++],
            maxConcurrentInvocations: 2 );

        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        var snapshot1 = sut.TryGetTaskState( "foo" );
        PublishEvents( source, timestamp2 );
        var snapshot2 = sut.TryGetTaskState( "foo" );
        PublishEvents( source, timestamp3 );
        var snapshot3 = sut.TryGetTaskState( "foo" );
        taskSources[0].SetResult();
        var snapshot4 = sut.TryGetTaskState( "foo" );
        taskSources[1].SetResult();
        var snapshot5 = sut.TryGetTaskState( "foo" );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp3 );
            sut.EventCount.Should().Be( 3 );

            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ),
                    new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), null, false ) );

            AssertTaskStateSnapshot(
                snapshot1,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskStateSnapshot(
                snapshot2,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 2,
                maxActiveTasks: 2 );

            AssertTaskStateSnapshot(
                snapshot3,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 2,
                maxActiveTasks: 2,
                skippedInvocations: 1 );

            AssertTaskStateSnapshot(
                snapshot4,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 1,
                maxActiveTasks: 2,
                skippedInvocations: 1 );

            AssertTaskStateSnapshot(
                snapshot5,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                maxActiveTasks: 2,
                skippedInvocations: 1 );
        }
    }

    [Fact]
    public void SingleConcurrentTaskInvocations_ShouldBeTrackedCorrectly_WithLimitedQueue()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;
        var timestamp3 = timestamp2 + interval;
        var timestamp4 = timestamp3 + interval;

        var taskIndex = 0;
        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource() };

        var fooTask = TimerTask.CreateFromSource(
            "foo",
            _ => taskSources[taskIndex++],
            maxEnqueuedInvocations: 2 );

        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        var snapshot1 = sut.TryGetTaskState( "foo" );
        PublishEvents( source, timestamp2 );
        var snapshot2 = sut.TryGetTaskState( "foo" );
        PublishEvents( source, timestamp3 );
        var snapshot3 = sut.TryGetTaskState( "foo" );
        PublishEvents( source, timestamp4 );
        var snapshot4 = sut.TryGetTaskState( "foo" );
        taskSources[0].SetResult();
        var snapshot5 = sut.TryGetTaskState( "foo" );
        taskSources[1].SetResult();
        var snapshot6 = sut.TryGetTaskState( "foo" );
        taskSources[2].SetResult();
        var snapshot7 = sut.TryGetTaskState( "foo" );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp4 );
            sut.EventCount.Should().Be( 4 );

            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ),
                    new ReactiveTaskInvocationParams( 1, timestamp3, timestamp4 ),
                    new ReactiveTaskInvocationParams( 2, timestamp4, timestamp4 ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp3, timestamp4 ), null, false ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, timestamp4, timestamp4 ), null, false ) );

            AssertTaskStateSnapshot(
                snapshot1,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskStateSnapshot(
                snapshot2,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1,
                queuedInvocations: 1,
                maxQueuedInvocations: 1 );

            AssertTaskStateSnapshot(
                snapshot3,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1,
                queuedInvocations: 2,
                maxQueuedInvocations: 2 );

            AssertTaskStateSnapshot(
                snapshot4,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1,
                queuedInvocations: 2,
                maxQueuedInvocations: 2,
                skippedInvocations: 1 );

            AssertTaskStateSnapshot(
                snapshot5,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 2,
                activeTasks: 1,
                maxActiveTasks: 1,
                queuedInvocations: 1,
                maxQueuedInvocations: 2,
                skippedInvocations: 1,
                delayedInvocations: 1 );

            AssertTaskStateSnapshot(
                snapshot6,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 3,
                activeTasks: 1,
                maxActiveTasks: 1,
                maxQueuedInvocations: 2,
                skippedInvocations: 1,
                delayedInvocations: 2 );

            AssertTaskStateSnapshot(
                snapshot7,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 3,
                maxActiveTasks: 1,
                maxQueuedInvocations: 2,
                skippedInvocations: 1,
                delayedInvocations: 2 );
        }
    }

    [Fact]
    public void ExceptionDuringTaskInvocation_ShouldBeCaughtAndHandled()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var exception = new Exception();

        var fooTask = new TimerTask( "foo", _ => throw exception );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp1 );
            sut.EventCount.Should().Be( 1 );

            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );
            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), exception, false ) );

            AssertTaskStateSnapshot(
                sut.TryGetTaskState( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                failedInvocations: 1 );
        }
    }

    [Fact]
    public void ExceptionDuringTaskProcessing_ShouldBeCaughtAndHandled()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var exception = new Exception();

        var taskSource = new TaskCompletionSource();
        var fooTask = new TimerTask( "foo", _ => taskSource.Task );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        taskSource.SetException( exception );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp1 );
            sut.EventCount.Should().Be( 1 );

            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );
            fooTask.Completions.Should().HaveCount( 1 );
            fooTask.Completions.ElementAtOrDefault( 0 )
                .Invocation.Should()
                .Be( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );

            fooTask.Completions.ElementAtOrDefault( 0 ).Exception.Should().BeEquivalentTo( taskSource.Task.Exception );
            fooTask.Completions.ElementAtOrDefault( 0 ).IsCancelled.Should().BeFalse();

            AssertTaskStateSnapshot(
                sut.TryGetTaskState( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                failedInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public void ExceptionDuringTaskCompletion_ShouldBeCaughtAndIgnored()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var exception = new Exception();

        var fooTask = new TimerTask( "foo", _ => Task.CompletedTask, completionException: exception );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp1 );
            sut.EventCount.Should().Be( 1 );

            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );
            fooTask.Completions.Should()
                .BeSequentiallyEqualTo( new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, false ) );

            AssertTaskStateSnapshot(
                sut.TryGetTaskState( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1 );
        }
    }

    [Fact]
    public void TaskCancellation_ShouldBeCaughtAndHandled()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;

        var taskSource = new TaskCompletionSource();
        var fooTask = new TimerTask( "foo", _ => taskSource.Task );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        taskSource.SetCanceled();

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp1 );
            sut.EventCount.Should().Be( 1 );

            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );
            fooTask.Completions.Should()
                .BeSequentiallyEqualTo( new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, true ) );

            AssertTaskStateSnapshot(
                sut.TryGetTaskState( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                cancelledInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public async Task TaskInCreatedState_ShouldBeStartedAutomatically()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;

        var fooTask = new TimerTask( "foo", _ => new Task( () => { } ) );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        await Task.Delay( 1 );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp1 );
            sut.EventCount.Should().Be( 1 );

            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );
            fooTask.Completions.Should()
                .BeSequentiallyEqualTo( new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, false ) );

            AssertTaskStateSnapshot(
                sut.TryGetTaskState( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeTaskCollectionAndAllTasks()
    {
        var tasks = new[] { TimerTask.CreateCompleted( "foo" ) };
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( tasks );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeFalse();
            tasks[0].IsDisposed.Should().BeTrue();
            AssertTaskStateSnapshot( sut.TryGetTaskState( "foo" ), tasks[0] );
        }
    }

    [Fact]
    public void Dispose_ShouldImmediatelySkipAllQueuedInvocationsAndSignalActiveTasksToBeCancelled()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;

        var taskSource = new TaskCompletionSource();
        var fooTask = new TimerTask(
            "foo",
            ct =>
            {
                ct.Register( () => taskSource.SetCanceled( ct ) );
                return taskSource.Task;
            },
            maxEnqueuedInvocations: 1 );

        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );
        PublishEvents( source, timestamp1, timestamp2 );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp2 );
            sut.EventCount.Should().Be( 2 );

            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );
            fooTask.Completions.Should()
                .BeSequentiallyEqualTo( new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, true ) );

            AssertTaskStateSnapshot(
                sut.TryGetTaskState( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                cancelledInvocations: 1,
                skippedInvocations: 1,
                maxActiveTasks: 1,
                maxQueuedInvocations: 1 );
        }
    }

    private static void PublishEvents(EventPublisher<WithInterval<long>> publisher, params Timestamp[] timestamps)
    {
        Timestamp? prevTs = null;
        for ( var i = 0; i < timestamps.Length; ++i )
        {
            var ts = timestamps[i];
            publisher.Publish( new WithInterval<long>( i, ts, prevTs is null ? Duration.Zero : ts - prevTs.Value ) );
            prevTs = ts;
        }
    }

    private static void AssertTaskStateSnapshot(
        TimerTaskStateSnapshot? snapshot,
        TimerTask task,
        Timestamp? firstInvocationTimestamp = null,
        Timestamp? lastInvocationTimestamp = null,
        long totalInvocations = 0,
        long delayedInvocations = 0,
        long skippedInvocations = 0,
        long failedInvocations = 0,
        long cancelledInvocations = 0,
        long queuedInvocations = 0,
        long maxQueuedInvocations = 0,
        long activeTasks = 0,
        long maxActiveTasks = 0)
    {
        (snapshot?.Task).Should().BeSameAs( task );
        (snapshot?.FirstInvocationTimestamp).Should().Be( firstInvocationTimestamp );
        (snapshot?.LastInvocationTimestamp).Should().Be( lastInvocationTimestamp );
        (snapshot?.TotalInvocations).Should().Be( totalInvocations );
        (snapshot?.DelayedInvocations).Should().Be( delayedInvocations );
        (snapshot?.SkippedInvocations).Should().Be( skippedInvocations );
        (snapshot?.FailedInvocations).Should().Be( failedInvocations );
        (snapshot?.CancelledInvocations).Should().Be( cancelledInvocations );
        (snapshot?.QueuedInvocations).Should().Be( queuedInvocations );
        (snapshot?.MaxQueuedInvocations).Should().Be( maxQueuedInvocations );
        (snapshot?.ActiveTasks).Should().Be( activeTasks );
        (snapshot?.MaxActiveTasks).Should().Be( maxActiveTasks );
        if ( totalInvocations - activeTasks > 0 )
        {
            (snapshot?.MinElapsedTime.Ticks).Should().BeGreaterOrEqualTo( 0 );
            (snapshot?.MinElapsedTime.Ticks).Should().BeLessOrEqualTo( snapshot?.MaxElapsedTime.Ticks ?? 0 );
            (snapshot?.MaxElapsedTime.Ticks).Should().BeGreaterOrEqualTo( snapshot?.MinElapsedTime.Ticks ?? 0 );
            (snapshot?.AverageElapsedTime.Ticks).Should().BeGreaterOrEqualTo( snapshot?.MinElapsedTime.Ticks ?? 0 );
            (snapshot?.AverageElapsedTime.Ticks).Should().BeLessOrEqualTo( snapshot?.MaxElapsedTime.Ticks ?? 0 );
        }
        else
        {
            (snapshot?.MinElapsedTime).Should().Be( Duration.MaxValue );
            (snapshot?.MaxElapsedTime).Should().Be( Duration.MinValue );
            (snapshot?.AverageElapsedTime).Should().Be( FloatingDuration.Zero );
        }
    }

    private readonly record struct TaskCompletion(ReactiveTaskInvocationParams Invocation, Exception? Exception, bool IsCancelled);

    private sealed class TimerTask : TimerTask<string>
    {
        public TimerTask(
            string key,
            Func<CancellationToken, Task> taskFactory,
            Func<Timestamp?, Timestamp?>? nextInvocationTimestampFactory = null,
            Exception? completionException = null,
            int maxEnqueuedInvocations = 0,
            int maxConcurrentInvocations = 1)
            : base( key, null, maxEnqueuedInvocations, maxConcurrentInvocations )
        {
            IsDisposed = false;
            Invocations = new List<ReactiveTaskInvocationParams>();
            Completions = new List<TaskCompletion>();
            NextInvocationTimestampFactory = nextInvocationTimestampFactory;
            TaskFactory = taskFactory;
            CompletionException = completionException;
            NextInvocationTimestamp = NextInvocationTimestampFactory?.Invoke( NextInvocationTimestamp );
        }

        public List<ReactiveTaskInvocationParams> Invocations { get; }
        public List<TaskCompletion> Completions { get; }
        public Func<Timestamp?, Timestamp?>? NextInvocationTimestampFactory { get; }
        public Func<CancellationToken, Task> TaskFactory { get; }
        public Exception? CompletionException { get; }
        public bool IsDisposed { get; private set; }

        [Pure]
        public static TimerTask CreateCompleted(string key, Func<Timestamp?, Timestamp?>? nextInvocationTimestampFactory = null)
        {
            return new TimerTask( key, _ => Task.CompletedTask, nextInvocationTimestampFactory );
        }

        [Pure]
        public static TimerTask CreateFromSource(
            string key,
            Func<CancellationToken, TaskCompletionSource> source,
            int maxEnqueuedInvocations = 0,
            int maxConcurrentInvocations = 1,
            Func<Timestamp?, Timestamp?>? nextInvocationTimestampFactory = null)
        {
            return new TimerTask(
                key,
                t => source( t ).Task,
                nextInvocationTimestampFactory,
                null,
                maxEnqueuedInvocations,
                maxConcurrentInvocations );
        }

        public override void Dispose()
        {
            base.Dispose();
            IsDisposed = true;
        }

        public override Task InvokeAsync(ReactiveTaskInvocationParams parameters, CancellationToken cancellationToken)
        {
            NextInvocationTimestamp = NextInvocationTimestampFactory?.Invoke( NextInvocationTimestamp );
            Invocations.Add( parameters );
            return TaskFactory( cancellationToken );
        }

        public override void OnCompleted(ReactiveTaskCompletionParams parameters)
        {
            base.OnCompleted( parameters );
            Completions.Add( new TaskCompletion( parameters.Invocation, parameters.Exception, parameters.IsCancelled ) );

            if ( CompletionException is not null )
                throw CompletionException;
        }
    }
}
