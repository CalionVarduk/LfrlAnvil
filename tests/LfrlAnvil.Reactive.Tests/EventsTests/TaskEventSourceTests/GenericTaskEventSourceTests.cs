using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.Events;
using LfrlAnvil.Reactive.Events.Composites;
using LfrlAnvil.Reactive.Events.Internal;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.EventsTests.TaskEventSourceTests
{
    public abstract class GenericTaskEventSourceTests<TEvent> : TestsBase
    {
        private readonly SynchronousTaskScheduler _scheduler = new SynchronousTaskScheduler();

        [Fact]
        public void Ctor_WithCallbackScheduler_ShouldCreateEventSourceWithoutSubscriptions()
        {
            var sut = new TaskEventSource<TEvent>(
                _ => Task.FromResult( Fixture.Create<TEvent>() ),
                _scheduler );

            sut.HasSubscribers.Should().BeFalse();
        }

        [Theory]
        [InlineData( TaskEventSourceContextCapture.None )]
        [InlineData( TaskEventSourceContextCapture.Current )]
        [InlineData( TaskEventSourceContextCapture.FromListener )]
        public void Ctor_WithContextCapture_ShouldCreateEventSourceWithoutSubscriptions(TaskEventSourceContextCapture contextCapture)
        {
            var sut = new TaskEventSource<TEvent>(
                _ => Task.FromResult( Fixture.Create<TEvent>() ),
                contextCapture );

            sut.HasSubscribers.Should().BeFalse();
        }

        [Fact]
        public void Listen_ShouldReturnDisposedSubscriber_WhenEventSourceIsDisposed()
        {
            var listener = Substitute.For<IEventListener<FromTask<TEvent>>>();
            var sut = new TaskEventSource<TEvent>(
                _ => Task.FromResult( Fixture.Create<TEvent>() ),
                _scheduler );

            sut.Dispose();

            var subscriber = sut.Listen( listener );

            using ( new AssertionScope() )
            {
                subscriber.IsDisposed.Should().BeTrue();
                listener.DidNotReceive().React( Arg.Any<FromTask<TEvent>>() );
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
                _scheduler );

            var subscriber = sut.Listen( listener );

            using ( new AssertionScope() )
            {
                sut.HasSubscribers.Should().BeTrue();
                subscriber.IsDisposed.Should().BeFalse();
                listener.DidNotReceive().React( Arg.Any<FromTask<TEvent>>() );
            }
        }

        [Fact]
        public void Listen_WithCallbackCaptureFromListener_ShouldCreateActiveSubscriberThatDoesNothing_UntilTaskCompletes()
        {
            var listener = Substitute.For<IEventListener<FromTask<TEvent>>>();
            var sut = new TaskEventSource<TEvent>(
                async ct =>
                {
                    await Task.Delay( 100, ct );
                    return Fixture.Create<TEvent>();
                },
                TaskEventSourceContextCapture.FromListener );

            var subscriber = sut.Listen( listener );

            using ( new AssertionScope() )
            {
                sut.HasSubscribers.Should().BeTrue();
                subscriber.IsDisposed.Should().BeFalse();
                listener.DidNotReceive().React( Arg.Any<FromTask<TEvent>>() );
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
                        Task.Delay( 1, ct ).Wait( ct );
                        return value;
                    },
                    ct ),
                _scheduler );

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
                            Exception = (AggregateException?)null,
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
                        Task.Delay( 1, ct ).Wait( ct );
                        throw exception;
                    },
                    ct ),
                _scheduler );

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
        public void Listen_ShouldCreateActiveSubscriberThatEmitsResultOnceAndThenDisposes_WhenTaskCancelledDueToSubscriberDispose()
        {
            var actualValues = new List<FromTask<TEvent>>();

            var listener = EventListener.Create<FromTask<TEvent>>( actualValues.Add );
            var sut = new TaskEventSource<TEvent>(
                async ct =>
                {
                    await Task.Delay( 100, ct );
                    return Fixture.Create<TEvent>();
                },
                _scheduler );

            var subscriber = sut.Listen( listener );
            subscriber.Dispose();

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
                            Exception = (AggregateException?)null,
                            IsCanceled = true,
                            IsFaulted = false,
                            IsCompletedSuccessfully = false
                        } );
            }
        }

        [Fact]
        public void Listen_ShouldCreateActiveSubscriberThatEmitsResultOnceAndThenDisposes_WhenTaskCancelledDueToEventSourceDispose()
        {
            var actualValues = new List<FromTask<TEvent>>();

            var listener = EventListener.Create<FromTask<TEvent>>( actualValues.Add );
            var sut = new TaskEventSource<TEvent>(
                async ct =>
                {
                    await Task.Delay( 100, ct );
                    return Fixture.Create<TEvent>();
                },
                _scheduler );

            var subscriber = sut.Listen( listener );
            sut.Dispose();

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
                            Exception = (AggregateException?)null,
                            IsCanceled = true,
                            IsFaulted = false,
                            IsCompletedSuccessfully = false
                        } );
            }
        }

        [Fact]
        public void Listen_ShouldCreateActiveSubscriberThatAutomaticallyStartsTaskInCreatedStatusOnDefaultScheduler()
        {
            var task = new Task<TEvent>(
                () =>
                {
                    Task.Delay( 100 ).Wait();
                    return Fixture.Create<TEvent>();
                } );

            var listener = Substitute.For<IEventListener<FromTask<TEvent>>>();
            var sut = new TaskEventSource<TEvent>(
                _ => task,
                _scheduler );

            var subscriber = sut.Listen( listener );

            using ( new AssertionScope() )
            {
                subscriber.IsDisposed.Should().BeFalse();
                listener.DidNotReceive().React( Arg.Any<FromTask<TEvent>>() );
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
                TaskEventSourceContextCapture.None );

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
                            Exception = (AggregateException?)null,
                            IsCanceled = false,
                            IsFaulted = false,
                            IsCompletedSuccessfully = true
                        } );
            }
        }

        [Fact]
        public void FromTask_WithCallbackScheduler_ShouldCreateEventSourceWithoutSubscriptions()
        {
            var sut = EventSource.FromTask( _ => Task.FromResult( Fixture.Create<TEvent>() ), _scheduler );
            sut.HasSubscribers.Should().BeFalse();
        }

        [Theory]
        [InlineData( TaskEventSourceContextCapture.None )]
        [InlineData( TaskEventSourceContextCapture.Current )]
        [InlineData( TaskEventSourceContextCapture.FromListener )]
        public void FromTask_WithContextCapture_ShouldCreateEventSourceWithoutSubscriptions(TaskEventSourceContextCapture contextCapture)
        {
            var sut = EventSource.FromTask( _ => Task.FromResult( Fixture.Create<TEvent>() ), contextCapture );
            sut.HasSubscribers.Should().BeFalse();
        }
    }
}
