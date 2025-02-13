using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Tests.TaskEventSourceTests;

public abstract class GenericTaskEventSourceTests<TEvent> : TestsBase
{
    private readonly SynchronousTaskScheduler _scheduler = new SynchronousTaskScheduler();

    [Fact]
    public void Ctor_WithCapturedTaskScheduler_ShouldCreateEventSourceWithoutSubscriptions()
    {
        var sut = new TaskEventSource<TEvent>(
            _ => Task.FromResult( Fixture.Create<TEvent>() ),
            new TaskSchedulerCapture( _scheduler ) );

        sut.HasSubscribers.TestFalse().Go();
    }

    [Theory]
    [InlineData( TaskSchedulerCaptureStrategy.None )]
    [InlineData( TaskSchedulerCaptureStrategy.Current )]
    [InlineData( TaskSchedulerCaptureStrategy.Lazy )]
    public void Ctor_WithTaskSchedulerCaptureStrategy_ShouldCreateEventSourceWithoutSubscriptions(TaskSchedulerCaptureStrategy strategy)
    {
        var sut = new TaskEventSource<TEvent>(
            _ => Task.FromResult( Fixture.Create<TEvent>() ),
            new TaskSchedulerCapture( strategy ) );

        sut.HasSubscribers.TestFalse().Go();
    }

    [Fact]
    public void Listen_ShouldReturnDisposedSubscriber_WhenEventSourceIsDisposed()
    {
        var listener = Substitute.For<IEventListener<FromTask<TEvent>>>();
        var sut = new TaskEventSource<TEvent>(
            _ => Task.FromResult( Fixture.Create<TEvent>() ),
            new TaskSchedulerCapture( _scheduler ) );

        sut.Dispose();

        var subscriber = sut.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<FromTask<TEvent>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatDoesNothing_UntilTaskCompletes()
    {
        var listener = Substitute.For<IEventListener<FromTask<TEvent>>>();
        var sut = new TaskEventSource<TEvent>(
            async ct =>
            {
                await Task.Delay( 100, ct );
                return Fixture.Create<TEvent>();
            },
            new TaskSchedulerCapture( _scheduler ) );

        var subscriber = sut.Listen( listener );

        Assertion.All(
                sut.HasSubscribers.TestTrue(),
                subscriber.IsDisposed.TestFalse(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<FromTask<TEvent>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_WithLazyTaskSchedulerCaptureStrategy_ShouldCreateActiveSubscriberThatDoesNothing_UntilTaskCompletes()
    {
        var listener = Substitute.For<IEventListener<FromTask<TEvent>>>();
        var sut = new TaskEventSource<TEvent>(
            async ct =>
            {
                await Task.Delay( 100, ct );
                return Fixture.Create<TEvent>();
            },
            new TaskSchedulerCapture( TaskSchedulerCaptureStrategy.Lazy ) );

        var subscriber = sut.Listen( listener );

        Assertion.All(
                sut.HasSubscribers.TestTrue(),
                subscriber.IsDisposed.TestFalse(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<FromTask<TEvent>>() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatEmitsResultOnceAndThenDisposes_WhenTaskRanToCompletion()
    {
        var value = Fixture.Create<TEvent>();
        var actualValues = new List<FromTask<TEvent>>();

        var listener = EventListener.Create<FromTask<TEvent>>( actualValues.Add );
        var sut = new TaskEventSource<TEvent>(
            ct => new TaskFactory<TEvent>( _scheduler ).StartNew(
                () =>
                {
                    Thread.Sleep( 1 );
                    return value;
                },
                ct ),
            new TaskSchedulerCapture( _scheduler ) );

        var subscriber = sut.Listen( listener );

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                actualValues.Count.TestEquals( 1 ),
                actualValues.TestAll(
                    (task, _) => Assertion.All(
                        "task",
                        task.Status.TestEquals( TaskStatus.RanToCompletion ),
                        task.Result.TestEquals( value ),
                        task.Exception.TestNull(),
                        task.IsCanceled.TestFalse(),
                        task.IsFaulted.TestFalse(),
                        task.IsCompletedSuccessfully.TestTrue() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatEmitsResultOnceAndThenDisposes_WhenTaskFaulted()
    {
        var exception = new Exception();
        var actualValues = new List<FromTask<TEvent>>();

        var listener = EventListener.Create<FromTask<TEvent>>( actualValues.Add );
        var sut = new TaskEventSource<TEvent>(
            ct => new TaskFactory<TEvent>( _scheduler ).StartNew(
                () =>
                {
                    Thread.Sleep( 1 );
                    throw exception;
                },
                ct ),
            new TaskSchedulerCapture( _scheduler ) );

        var subscriber = sut.Listen( listener );

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                actualValues.Count.TestEquals( 1 ),
                actualValues.TestAll(
                    (task, _) => Assertion.All(
                        "task",
                        task.Status.TestEquals( TaskStatus.Faulted ),
                        task.Result.TestEquals( default ),
                        task.Exception.TestType().AssignableTo<AggregateException>( e => e.InnerExceptions.TestSequence( [ exception ] ) ),
                        task.IsCanceled.TestFalse(),
                        task.IsFaulted.TestTrue(),
                        task.IsCompletedSuccessfully.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public async Task Listen_ShouldCreateActiveSubscriberThatEmitsResultOnceAndThenDisposes_WhenTaskCancelledDueToSubscriberDispose()
    {
        var eventReceivedTaskSource = new TaskCompletionSource();
        var actualValues = new List<FromTask<TEvent>>();

        var listener = EventListener.Create<FromTask<TEvent>>(
            e =>
            {
                actualValues.Add( e );
                eventReceivedTaskSource.SetResult();
            } );

        var sut = new TaskEventSource<TEvent>(
            async ct =>
            {
                await Task.Delay( 100, ct );
                return Fixture.Create<TEvent>();
            },
            new TaskSchedulerCapture( _scheduler ) );

        var subscriber = sut.Listen( listener );
        subscriber.Dispose();
        await eventReceivedTaskSource.Task;

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                actualValues.Count.TestEquals( 1 ),
                actualValues.TestAll(
                    (task, _) => Assertion.All(
                        "task",
                        task.Status.TestEquals( TaskStatus.Canceled ),
                        task.Result.TestEquals( default ),
                        task.Exception.TestNull(),
                        task.IsCanceled.TestTrue(),
                        task.IsFaulted.TestFalse(),
                        task.IsCompletedSuccessfully.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public async Task Listen_ShouldCreateActiveSubscriberThatEmitsResultOnceAndThenDisposes_WhenTaskCancelledDueToEventSourceDispose()
    {
        var eventReceivedTaskSource = new TaskCompletionSource();
        var actualValues = new List<FromTask<TEvent>>();

        var listener = EventListener.Create<FromTask<TEvent>>(
            e =>
            {
                actualValues.Add( e );
                eventReceivedTaskSource.SetResult();
            } );

        var sut = new TaskEventSource<TEvent>(
            async ct =>
            {
                await Task.Delay( 100, ct );
                return Fixture.Create<TEvent>();
            },
            new TaskSchedulerCapture( _scheduler ) );

        var subscriber = sut.Listen( listener );
        sut.Dispose();
        await eventReceivedTaskSource.Task;

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                actualValues.Count.TestEquals( 1 ),
                actualValues.TestAll(
                    (task, _) => Assertion.All(
                        "task",
                        task.Status.TestEquals( TaskStatus.Canceled ),
                        task.Result.TestEquals( default ),
                        task.Exception.TestNull(),
                        task.IsCanceled.TestTrue(),
                        task.IsFaulted.TestFalse(),
                        task.IsCompletedSuccessfully.TestFalse() ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatAutomaticallyStartsTaskInCreatedStatusOnDefaultScheduler()
    {
        var task = Task.Delay( 100 ).ContinueWith( _ => Fixture.Create<TEvent>() );

        var listener = Substitute.For<IEventListener<FromTask<TEvent>>>();
        var sut = new TaskEventSource<TEvent>( _ => task, new TaskSchedulerCapture( _scheduler ) );

        var subscriber = sut.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestFalse(),
                listener.TestDidNotReceiveCall( x => x.React( Arg.Any<FromTask<TEvent>>() ) ),
                task.Status.TestNotEquals( TaskStatus.Created ) )
            .Go();
    }

    [Fact]
    public async Task Listen_ShouldCreateActiveSubscriberThatEmitsResultOnceAndThenDisposes_WhenTaskIsTrulyAsyncWithDefaultSchedulers()
    {
        var value = Fixture.Create<TEvent>();
        var actualValues = new List<FromTask<TEvent>>();

        var listener = EventListener.Create<FromTask<TEvent>>( actualValues.Add );
        var sut = new TaskEventSource<TEvent>(
            async ct =>
            {
                await Task.Delay( 1, ct );
                return value;
            },
            new TaskSchedulerCapture( TaskSchedulerCaptureStrategy.None ) );

        var subscriber = sut.Listen( listener );
        await Task.Delay( 100 );

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                subscriber.IsDisposed.TestTrue(),
                actualValues.Count.TestEquals( 1 ),
                actualValues.TestAll(
                    (task, _) => Assertion.All(
                        "task",
                        task.Status.TestEquals( TaskStatus.RanToCompletion ),
                        task.Result.TestEquals( value ),
                        task.Exception.TestNull(),
                        task.IsCanceled.TestFalse(),
                        task.IsFaulted.TestFalse(),
                        task.IsCompletedSuccessfully.TestTrue() ) ) )
            .Go();
    }

    [Fact]
    public void FromTask_WithCallbackScheduler_ShouldCreateEventSourceWithoutSubscriptions()
    {
        var sut = EventSource.FromTask( _ => Task.FromResult( Fixture.Create<TEvent>() ), new TaskSchedulerCapture( _scheduler ) );
        sut.HasSubscribers.TestFalse().Go();
    }

    [Theory]
    [InlineData( TaskSchedulerCaptureStrategy.None )]
    [InlineData( TaskSchedulerCaptureStrategy.Current )]
    [InlineData( TaskSchedulerCaptureStrategy.Lazy )]
    public void FromTask_WithContextCapture_ShouldCreateEventSourceWithoutSubscriptions(TaskSchedulerCaptureStrategy strategy)
    {
        var sut = EventSource.FromTask( _ => Task.FromResult( Fixture.Create<TEvent>() ), new TaskSchedulerCapture( strategy ) );
        sut.HasSubscribers.TestFalse().Go();
    }
}
