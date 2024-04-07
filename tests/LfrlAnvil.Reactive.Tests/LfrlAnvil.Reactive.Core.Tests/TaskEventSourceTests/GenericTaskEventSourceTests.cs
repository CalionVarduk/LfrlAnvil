using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;

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

        sut.HasSubscribers.Should().BeFalse();
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

        sut.HasSubscribers.Should().BeFalse();
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

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<FromTask<TEvent>>() ) );
        }
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<FromTask<TEvent>>() ) );
        }
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeTrue();
            subscriber.IsDisposed.Should().BeFalse();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<FromTask<TEvent>>() ) );
        }
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();

            actualValues.Should()
                .HaveCount( 1 )
                .And.Subject.First()
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        Status = TaskStatus.RanToCompletion,
                        Result = value,
                        Exception = ( AggregateException? )null,
                        IsCanceled = false,
                        IsFaulted = false,
                        IsCompletedSuccessfully = true
                    } );
        }
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();

            actualValues.Should()
                .HaveCount( 1 )
                .And.Subject.First()
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        Status = TaskStatus.Faulted,
                        Result = default( TEvent ),
                        Exception = new AggregateException( exception ),
                        IsCanceled = false,
                        IsFaulted = true,
                        IsCompletedSuccessfully = false
                    } );
        }
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();

            actualValues.Should()
                .HaveCount( 1 )
                .And.Subject.First()
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        Status = TaskStatus.Canceled,
                        Result = default( TEvent ),
                        Exception = ( AggregateException? )null,
                        IsCanceled = true,
                        IsFaulted = false,
                        IsCompletedSuccessfully = false
                    } );
        }
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();

            actualValues.Should()
                .HaveCount( 1 )
                .And.Subject.First()
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        Status = TaskStatus.Canceled,
                        Result = default( TEvent ),
                        Exception = ( AggregateException? )null,
                        IsCanceled = true,
                        IsFaulted = false,
                        IsCompletedSuccessfully = false
                    } );
        }
    }

    [Fact]
    public void Listen_ShouldCreateActiveSubscriberThatAutomaticallyStartsTaskInCreatedStatusOnDefaultScheduler()
    {
        var task = Task.Delay( 100 ).ContinueWith( _ => Fixture.Create<TEvent>() );

        var listener = Substitute.For<IEventListener<FromTask<TEvent>>>();
        var sut = new TaskEventSource<TEvent>(
            _ => task,
            new TaskSchedulerCapture( _scheduler ) );

        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeFalse();
            listener.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<FromTask<TEvent>>() ) );
            task.Status.Should().NotBe( TaskStatus.Created );
        }
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

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            subscriber.IsDisposed.Should().BeTrue();

            actualValues.Should()
                .HaveCount( 1 )
                .And.Subject.First()
                .Should()
                .BeEquivalentTo(
                    new
                    {
                        Status = TaskStatus.RanToCompletion,
                        Result = value,
                        Exception = ( AggregateException? )null,
                        IsCanceled = false,
                        IsFaulted = false,
                        IsCompletedSuccessfully = true
                    } );
        }
    }

    [Fact]
    public void FromTask_WithCallbackScheduler_ShouldCreateEventSourceWithoutSubscriptions()
    {
        var sut = EventSource.FromTask( _ => Task.FromResult( Fixture.Create<TEvent>() ), new TaskSchedulerCapture( _scheduler ) );
        sut.HasSubscribers.Should().BeFalse();
    }

    [Theory]
    [InlineData( TaskSchedulerCaptureStrategy.None )]
    [InlineData( TaskSchedulerCaptureStrategy.Current )]
    [InlineData( TaskSchedulerCaptureStrategy.Lazy )]
    public void FromTask_WithContextCapture_ShouldCreateEventSourceWithoutSubscriptions(TaskSchedulerCaptureStrategy strategy)
    {
        var sut = EventSource.FromTask( _ => Task.FromResult( Fixture.Create<TEvent>() ), new TaskSchedulerCapture( strategy ) );
        sut.HasSubscribers.Should().BeFalse();
    }
}
