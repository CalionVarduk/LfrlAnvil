using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;
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
        var tasks = new[] { new TimerTask( "foo" ) };
        var source = new EventPublisher<WithInterval<long>>();

        var result = source.RegisterTasks( tasks );

        using ( new AssertionScope() )
        {
            source.Subscribers.Should().HaveCount( 1 );
            result.FirstTimestamp.Should().BeNull();
            result.LastTimestamp.Should().BeNull();
            result.EventCount.Should().Be( 0 );
            result.TaskKeys.Should().BeSequentiallyEqualTo( "foo" );
            AssertTaskSnapshot( result.TryGetTaskSnapshot( "foo" ), tasks[0] );
        }
    }

    [Fact]
    public void RegisterTasks_ShouldThrow_WhenSomeTasksShareTheSameKey()
    {
        var tasks = new[] { new TimerTask( "foo" ), new TimerTask( "foo" ) };
        var source = new EventPublisher<WithInterval<long>>();

        var action = Lambda.Of( () => source.RegisterTasks( tasks ) );

        using ( new AssertionScope() )
        {
            action.Should().ThrowExactly<ArgumentException>();
            source.HasSubscribers.Should().BeFalse();
            tasks.Should().OnlyContain( t => t.IsDisposed );
        }
    }

    [Fact]
    public void RegisterTasks_ShouldDisposeAllTasks_WhenSourceIsDisposed()
    {
        var tasks = new[] { new TimerTask( "foo" ) };
        var source = new EventPublisher<WithInterval<long>>();
        source.Dispose();

        var result = source.RegisterTasks( tasks );

        using ( new AssertionScope() )
        {
            result.FirstTimestamp.Should().BeNull();
            result.LastTimestamp.Should().BeNull();
            result.EventCount.Should().Be( 0 );
            tasks.Should().OnlyContain( t => t.IsDisposed );
        }
    }

    [Fact]
    public void SourceEvent_ShouldInvokeAllTasks()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;
        var timestamp3 = timestamp2 + interval;

        var fooTask = new TimerTask( "foo" );
        var barTask = new TimerTask( "bar" );
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
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ), null, null ) );

            barTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ),
                    new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ),
                    new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ) );

            barTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ), null, null ) );

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                completedInvocations: 3 );

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "bar" ),
                barTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                completedInvocations: 3 );
        }
    }

    [Theory]
    [InlineData( 122 )]
    [InlineData( 123 )]
    public void TaskWithNextInvocationTimestamp_ShouldBeInvoked_WhenEventTimestampIsLessThanOrEqualToIt(long nextInvocationTicks)
    {
        var timestamp = new Timestamp( 123 );
        var fooTask = new TimerTask( "foo", nextInvocationTimestamp: new Timestamp( nextInvocationTicks ) );
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
                .BeSequentiallyEqualTo( new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp, timestamp ), null, null ) );

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp,
                lastInvocationTimestamp: timestamp,
                totalInvocations: 1,
                completedInvocations: 1 );
        }
    }

    [Fact]
    public void TaskWithNextInvocationTimestamp_ShouldNotBeInvoked_WhenEventTimestampIsGreaterThanIt()
    {
        var timestamp = new Timestamp( 123 );
        var fooTask = new TimerTask( "foo", nextInvocationTimestamp: timestamp + Duration.FromTicks( 1 ) );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp );
            sut.LastTimestamp.Should().Be( timestamp );
            sut.EventCount.Should().Be( 1 );
            fooTask.Invocations.Should().BeEmpty();
            AssertTaskSnapshot( sut.TryGetTaskSnapshot( "foo" ), fooTask );
        }
    }

    [Fact]
    public void MultipleConcurrentTaskInvocations_ShouldBeTrackedCorrectly()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;
        var timestamp3 = timestamp2 + interval;

        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource() };
        var fooTask = new TimerTask( "foo", onInvoke: (_, _, p, _) => taskSources[p.InvocationId].Task, maxConcurrentInvocations: 3 );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        var snapshot1 = sut.TryGetTaskSnapshot( "foo" );
        PublishEvents( source, timestamp2 );
        var snapshot2 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[1].SetResult();
        var snapshot3 = sut.TryGetTaskSnapshot( "foo" );
        PublishEvents( source, timestamp3 );
        var snapshot4 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[0].SetResult();
        var snapshot5 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[2].SetResult();
        var snapshot6 = sut.TryGetTaskSnapshot( "foo" );

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
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ), null, null ) );

            AssertTaskSnapshot(
                snapshot1,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskSnapshot(
                snapshot2,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 2,
                maxActiveTasks: 2 );

            AssertTaskSnapshot(
                snapshot3,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                completedInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 2 );

            AssertTaskSnapshot(
                snapshot4,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                completedInvocations: 1,
                activeTasks: 2,
                maxActiveTasks: 2 );

            AssertTaskSnapshot(
                snapshot5,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                completedInvocations: 2,
                activeTasks: 1,
                maxActiveTasks: 2 );

            AssertTaskSnapshot(
                snapshot6,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                completedInvocations: 3,
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

        var taskSources = new[]
        {
            new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource()
        };

        var fooTask = new TimerTask(
            "foo",
            onInvoke: (_, _, p, _) => taskSources[p.InvocationId].Task,
            maxConcurrentInvocations: 2,
            maxEnqueuedInvocations: 2 );

        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        var snapshot1 = sut.TryGetTaskSnapshot( "foo" );
        PublishEvents( source, timestamp2 );
        var snapshot2 = sut.TryGetTaskSnapshot( "foo" );
        PublishEvents( source, timestamp3 );
        var snapshot3 = sut.TryGetTaskSnapshot( "foo" );
        PublishEvents( source, timestamp4 );
        var snapshot4 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[1].SetResult();
        var snapshot5 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[2].SetResult();
        var snapshot6 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[0].SetResult();
        var snapshot7 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[3].SetResult();
        var snapshot8 = sut.TryGetTaskSnapshot( "foo" );

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
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp4 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 3, timestamp4, timestamp4 ), null, null ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo(
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ), 0 ),
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 3, timestamp4, timestamp4 ), 1 ) );

            AssertTaskSnapshot(
                snapshot1,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskSnapshot(
                snapshot2,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 2,
                maxActiveTasks: 2 );

            AssertTaskSnapshot(
                snapshot3,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                activeTasks: 2,
                maxActiveTasks: 2,
                queuedInvocations: 1,
                maxQueuedInvocations: 1 );

            AssertTaskSnapshot(
                snapshot4,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                activeTasks: 2,
                maxActiveTasks: 2,
                queuedInvocations: 2,
                maxQueuedInvocations: 2 );

            AssertTaskSnapshot(
                snapshot5,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                completedInvocations: 1,
                activeTasks: 2,
                maxActiveTasks: 2,
                queuedInvocations: 1,
                maxQueuedInvocations: 2,
                delayedInvocations: 1 );

            AssertTaskSnapshot(
                snapshot6,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                completedInvocations: 2,
                activeTasks: 2,
                maxActiveTasks: 2,
                maxQueuedInvocations: 2,
                delayedInvocations: 2 );

            AssertTaskSnapshot(
                snapshot7,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                completedInvocations: 3,
                activeTasks: 1,
                maxActiveTasks: 2,
                maxQueuedInvocations: 2,
                delayedInvocations: 2 );

            AssertTaskSnapshot(
                snapshot8,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                completedInvocations: 4,
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

        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource(), new TaskCompletionSource() };
        var fooTask = new TimerTask( "foo", onInvoke: (_, _, p, _) => taskSources[p.InvocationId].Task, maxConcurrentInvocations: 2 );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        var snapshot1 = sut.TryGetTaskSnapshot( "foo" );
        PublishEvents( source, timestamp2 );
        var snapshot2 = sut.TryGetTaskSnapshot( "foo" );
        PublishEvents( source, timestamp3 );
        var snapshot3 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[0].SetResult();
        var snapshot4 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[1].SetResult();
        var snapshot5 = sut.TryGetTaskSnapshot( "foo" );

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
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ),
                        null,
                        TaskCancellationReason.MaxQueueSizeLimit ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), null, null ) );

            AssertTaskSnapshot(
                snapshot1,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskSnapshot(
                snapshot2,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 2,
                maxActiveTasks: 2 );

            AssertTaskSnapshot(
                snapshot3,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                activeTasks: 2,
                maxActiveTasks: 2,
                skippedInvocations: 1 );

            AssertTaskSnapshot(
                snapshot4,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                completedInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 2,
                skippedInvocations: 1 );

            AssertTaskSnapshot(
                snapshot5,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                completedInvocations: 2,
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
        var fooTask = new TimerTask( "foo", onInvoke: (_, _, _, _) => taskSources[taskIndex++].Task, maxEnqueuedInvocations: 2 );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        var snapshot1 = sut.TryGetTaskSnapshot( "foo" );
        PublishEvents( source, timestamp2 );
        var snapshot2 = sut.TryGetTaskSnapshot( "foo" );
        PublishEvents( source, timestamp3 );
        var snapshot3 = sut.TryGetTaskSnapshot( "foo" );
        PublishEvents( source, timestamp4 );
        var snapshot4 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[0].SetResult();
        var snapshot5 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[1].SetResult();
        var snapshot6 = sut.TryGetTaskSnapshot( "foo" );
        taskSources[2].SetResult();
        var snapshot7 = sut.TryGetTaskSnapshot( "foo" );

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp4 );
            sut.EventCount.Should().Be( 4 );

            fooTask.Invocations.Should()
                .BeSequentiallyEqualTo(
                    new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ),
                    new ReactiveTaskInvocationParams( 2, timestamp3, timestamp4 ),
                    new ReactiveTaskInvocationParams( 3, timestamp4, timestamp4 ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, timestamp2, timestamp4 ),
                        null,
                        TaskCancellationReason.MaxQueueSizeLimit ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp4 ), null, null ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 3, timestamp4, timestamp4 ), null, null ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo(
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), 0 ),
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 2, timestamp3, timestamp3 ), 1 ),
                    new TaskEnqueue( new ReactiveTaskInvocationParams( 3, timestamp4, timestamp4 ), 2 ) );

            AssertTaskSnapshot(
                snapshot1,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1 );

            AssertTaskSnapshot(
                snapshot2,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                activeTasks: 1,
                maxActiveTasks: 1,
                queuedInvocations: 1,
                maxQueuedInvocations: 1 );

            AssertTaskSnapshot(
                snapshot3,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp3,
                totalInvocations: 3,
                activeTasks: 1,
                maxActiveTasks: 1,
                queuedInvocations: 2,
                maxQueuedInvocations: 2 );

            AssertTaskSnapshot(
                snapshot4,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                activeTasks: 1,
                maxActiveTasks: 1,
                queuedInvocations: 2,
                maxQueuedInvocations: 2,
                skippedInvocations: 1 );

            AssertTaskSnapshot(
                snapshot5,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                completedInvocations: 1,
                activeTasks: 1,
                maxActiveTasks: 1,
                queuedInvocations: 1,
                maxQueuedInvocations: 2,
                skippedInvocations: 1,
                delayedInvocations: 1 );

            AssertTaskSnapshot(
                snapshot6,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                completedInvocations: 2,
                activeTasks: 1,
                maxActiveTasks: 1,
                maxQueuedInvocations: 2,
                skippedInvocations: 1,
                delayedInvocations: 2 );

            AssertTaskSnapshot(
                snapshot7,
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp4,
                totalInvocations: 4,
                completedInvocations: 3,
                maxActiveTasks: 1,
                maxQueuedInvocations: 2,
                skippedInvocations: 1,
                delayedInvocations: 2 );
        }
    }

    [Fact]
    public void TaskEnqueueCancellation_ShouldCompleteItImmediatelyWithCancellationRequestedReason()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;

        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource() };
        var fooTask = new TimerTask(
            "foo",
            onInvoke: (_, _, p, _) => taskSources[p.InvocationId].Task,
            onEnqueue: (_, _, _, _) => false,
            maxEnqueuedInvocations: 1 );

        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1, timestamp2 );
        taskSources[0].SetResult();

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ),
                        null,
                        TaskCancellationReason.CancellationRequested ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo( new TaskEnqueue( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), 0 ) );

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                completedInvocations: 1,
                skippedInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public void ExceptionDuringTaskInvocation_ShouldBeCaughtAndHandled()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var exception = new Exception();

        var fooTask = new TimerTask( "foo", onInvoke: (_, _, _, _) => throw exception );
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
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), exception, null ) );

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                completedInvocations: 1,
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
        var fooTask = new TimerTask( "foo", onInvoke: (_, _, _, _) => taskSource.Task );
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
            fooTask.Completions.ElementAtOrDefault( 0 ).CancellationReason.Should().BeNull();

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                completedInvocations: 1,
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

        var fooTask = new TimerTask( "foo", onComplete: (_, _, _) => throw exception );
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
                .BeSequentiallyEqualTo( new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ) );

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                completedInvocations: 1 );
        }
    }

    [Fact]
    public void ExceptionDuringTaskEnqueue_ShouldBeCaughtAndIgnoredAndCauseInvocationToBeCancelled()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;
        var exception = new Exception();

        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource() };
        var fooTask = new TimerTask(
            "foo",
            onInvoke: (_, _, p, _) => taskSources[p.InvocationId].Task,
            onEnqueue: (_, _, _, _) => throw exception,
            maxEnqueuedInvocations: 1 );

        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1, timestamp2 );
        taskSources[0].SetResult();

        using ( new AssertionScope() )
        {
            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ),
                        null,
                        TaskCancellationReason.CancellationRequested ),
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo( new TaskEnqueue( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), 0 ) );

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                completedInvocations: 1,
                skippedInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public void TaskCancellation_ShouldBeCaughtAndHandled()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;

        var taskSource = new TaskCompletionSource();
        var fooTask = new TimerTask( "foo", onInvoke: (_, _, _, _) => taskSource.Task );
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
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ),
                        null,
                        TaskCancellationReason.CancellationRequested ) );

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                completedInvocations: 1,
                cancelledInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public void TaskInCreatedState_ShouldBeStartedAutomatically()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;

        var after = new ManualResetEventSlim();
        var fooTask = new TimerTask( "foo", onInvoke: (_, _, _, _) => new Task( () => { } ), onComplete: (_, _, _) => after.Set() );
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask } );

        PublishEvents( source, timestamp1 );
        after.Wait();

        using ( new AssertionScope() )
        {
            sut.FirstTimestamp.Should().Be( timestamp1 );
            sut.LastTimestamp.Should().Be( timestamp1 );
            sut.EventCount.Should().Be( 1 );

            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );
            fooTask.Completions.Should()
                .BeSequentiallyEqualTo( new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ) );

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp1,
                totalInvocations: 1,
                completedInvocations: 1,
                maxActiveTasks: 1 );
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeTaskCollectionAndAllTasks()
    {
        var tasks = new[] { new TimerTask( "foo" ) };
        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( tasks );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeFalse();
            tasks[0].IsDisposed.Should().BeTrue();
            AssertTaskSnapshot( sut.TryGetTaskSnapshot( "foo" ), tasks[0] );
        }
    }

    [Fact]
    public void Dispose_ShouldSignalActiveAndQueuedTasksToBeCancelled()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;

        var taskSource = new TaskCompletionSource();
        var fooTask = new TimerTask(
            "foo",
            onInvoke: (_, _, _, ct) =>
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
                .BeSequentiallyEqualTo(
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ),
                        null,
                        TaskCancellationReason.CancellationRequested ),
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ),
                        null,
                        TaskCancellationReason.TaskDisposed ) );

            AssertTaskSnapshot(
                sut.TryGetTaskSnapshot( "foo" ),
                fooTask,
                firstInvocationTimestamp: timestamp1,
                lastInvocationTimestamp: timestamp2,
                totalInvocations: 2,
                completedInvocations: 1,
                cancelledInvocations: 1,
                skippedInvocations: 1,
                delayedInvocations: 1,
                maxActiveTasks: 1,
                maxQueuedInvocations: 1 );
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeAllTasksCorrectly_AndRethrowAnyExceptionsAtTheEnd()
    {
        var timestamp = Timestamp.Zero;
        var tokenException = new Exception( "foo" );
        var disposalException = new Exception( "bar" );
        var fooTask = new TimerTask( "foo", onDispose: _ => throw disposalException );
        var barTask = new TimerTask(
            "bar",
            onInvoke: (_, _, _, ct) =>
            {
                ct.Register( () => throw tokenException );
                return Task.CompletedTask;
            } );

        var source = new EventPublisher<WithInterval<long>>();
        var sut = source.RegisterTasks( new[] { fooTask, barTask } );
        PublishEvents( source, timestamp );

        var action = Lambda.Of( () => sut.Dispose() );

        using ( new AssertionScope() )
        {
            var exception = action.Should().ThrowExactly<AggregateException>().And;
            (exception?.InnerExceptions).Should().HaveCount( 2 );
            (exception?.InnerExceptions.ElementAtOrDefault( 0 )).Should().BeSameAs( disposalException );
            ((exception?.InnerExceptions.ElementAtOrDefault( 1 ) as AggregateException)?.InnerExceptions).Should()
                .BeSequentiallyEqualTo( tokenException );

            fooTask.IsDisposed.Should().BeTrue();
            barTask.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void SourceDisposal_FromTaskInvocation_ShouldDisposeSourceAndFinishCurrentInvocation()
    {
        var timestamp = Timestamp.Zero;
        var afterAll = new ManualResetEventSlim();
        var fooTask = new TimerTask(
            "foo",
            onInvoke: (_, source, _, _) =>
            {
                source.Dispose();
                return new Task( () => { } );
            },
            onComplete: (_, _, _) => afterAll.Set() );

        var source = new EventPublisher<WithInterval<long>>();
        _ = source.RegisterTasks( new[] { fooTask } );
        PublishEvents( source, timestamp );
        afterAll.Wait();

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeFalse();
            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp, timestamp ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo( new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp, timestamp ), null, null ) );
        }
    }

    [Fact]
    public void SourceDisposal_FromTaskCompletion_ShouldDisposeSourceAndFinishCurrentInvocation()
    {
        var timestamp = Timestamp.Zero;
        var fooTask = new TimerTask( "foo", onComplete: (_, source, _) => source.Dispose() );

        var source = new EventPublisher<WithInterval<long>>();
        _ = source.RegisterTasks( new[] { fooTask } );
        PublishEvents( source, timestamp );

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeFalse();
            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp, timestamp ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo( new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp, timestamp ), null, null ) );
        }
    }

    [Fact]
    public void SourceDisposal_FromTaskEnqueue_ShouldDisposeSourceAndFinishCurrentInvocation()
    {
        var interval = Duration.FromMilliseconds( 15 );
        var timestamp1 = Timestamp.Zero + interval;
        var timestamp2 = timestamp1 + interval;

        var taskSources = new[] { new TaskCompletionSource(), new TaskCompletionSource() };
        var fooTask = new TimerTask(
            "foo",
            onInvoke: (_, _, p, _) => taskSources[p.InvocationId].Task,
            onEnqueue: (_, source, _, _) =>
            {
                source.Dispose();
                return true;
            },
            maxEnqueuedInvocations: 1 );

        var source = new EventPublisher<WithInterval<long>>();
        _ = source.RegisterTasks( new[] { fooTask } );
        PublishEvents( source, timestamp1, timestamp2 );
        taskSources[0].SetResult();

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeFalse();
            fooTask.Invocations.Should().BeSequentiallyEqualTo( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ) );

            fooTask.Completions.Should()
                .BeSequentiallyEqualTo(
                    new TaskCompletion( new ReactiveTaskInvocationParams( 0, timestamp1, timestamp1 ), null, null ),
                    new TaskCompletion(
                        new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ),
                        null,
                        TaskCancellationReason.TaskDisposed ) );

            fooTask.Enqueues.Should()
                .BeSequentiallyEqualTo( new TaskEnqueue( new ReactiveTaskInvocationParams( 1, timestamp2, timestamp2 ), 0 ) );
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

    private static void AssertTaskSnapshot(
        ReactiveTaskSnapshot<ITimerTask<string>>? snapshot,
        TimerTask task,
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
        (snapshot?.Task).Should().BeSameAs( task );
        (snapshot?.FirstInvocationTimestamp).Should().Be( firstInvocationTimestamp );
        (snapshot?.LastInvocationTimestamp).Should().Be( lastInvocationTimestamp );
        (snapshot?.TotalInvocations).Should().Be( totalInvocations );
        (snapshot?.ActiveInvocations).Should().Be( activeInvocations );
        (snapshot?.CompletedInvocations).Should().Be( completedInvocations );
        (snapshot?.SkippedInvocations).Should().Be( skippedInvocations );
        (snapshot?.DelayedInvocations).Should().Be( delayedInvocations );
        (snapshot?.FailedInvocations).Should().Be( failedInvocations );
        (snapshot?.CancelledInvocations).Should().Be( cancelledInvocations );
        (snapshot?.QueuedInvocations).Should().Be( queuedInvocations );
        (snapshot?.MaxQueuedInvocations).Should().Be( maxQueuedInvocations );
        (snapshot?.ActiveTasks).Should().Be( activeTasks );
        (snapshot?.MaxActiveTasks).Should().Be( maxActiveTasks );
        if ( completedInvocations > 0 )
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

    private sealed class TimerTask : TimerTask<string>
    {
        public TimerTask(
            string key,
            int maxEnqueuedInvocations = 0,
            int maxConcurrentInvocations = 1,
            Timestamp? nextInvocationTimestamp = null,
            Func<TimerTask, TimerTaskCollection<string>, ReactiveTaskInvocationParams, CancellationToken, Task>? onInvoke = null,
            Action<TimerTask, TimerTaskCollection<string>, ReactiveTaskCompletionParams>? onComplete = null,
            Func<TimerTask, TimerTaskCollection<string>, ReactiveTaskInvocationParams, int, bool>? onEnqueue = null,
            Action<TimerTask>? onDispose = null)
            : base( key, nextInvocationTimestamp, maxEnqueuedInvocations, maxConcurrentInvocations )
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

        public Func<TimerTask, TimerTaskCollection<string>, ReactiveTaskInvocationParams, CancellationToken, Task>?
            OnInvokeCallback { get; }

        public Action<TimerTask, TimerTaskCollection<string>, ReactiveTaskCompletionParams>? OnCompleteCallback { get; }
        public Func<TimerTask, TimerTaskCollection<string>, ReactiveTaskInvocationParams, int, bool>? OnEnqueueCallback { get; }
        public Action<TimerTask>? OnDisposeCallback { get; }

        public override void Dispose()
        {
            base.Dispose();
            IsDisposed = true;
            OnDisposeCallback?.Invoke( this );
        }

        public override Task InvokeAsync(
            TimerTaskCollection<string> source,
            ReactiveTaskInvocationParams parameters,
            CancellationToken cancellationToken)
        {
            Invocations.Add( parameters );
            return OnInvokeCallback is null ? Task.CompletedTask : OnInvokeCallback( this, source, parameters, cancellationToken );
        }

        public override void OnCompleted(TimerTaskCollection<string> source, ReactiveTaskCompletionParams parameters)
        {
            base.OnCompleted( source, parameters );
            Completions.Add( new TaskCompletion( parameters.Invocation, parameters.Exception, parameters.CancellationReason ) );
            OnCompleteCallback?.Invoke( this, source, parameters );
        }

        public override bool OnEnqueue(TimerTaskCollection<string> source, ReactiveTaskInvocationParams parameters, int positionInQueue)
        {
            Enqueues.Add( new TaskEnqueue( parameters, positionInQueue ) );
            return OnEnqueueCallback?.Invoke( this, source, parameters, positionInQueue )
                ?? base.OnEnqueue( source, parameters, positionInQueue );
        }
    }

    private readonly record struct TaskCompletion(
        ReactiveTaskInvocationParams Invocation,
        Exception? Exception,
        TaskCancellationReason? CancellationReason
    );

    private readonly record struct TaskEnqueue(ReactiveTaskInvocationParams Invocation, int PositionInQueue);
}
