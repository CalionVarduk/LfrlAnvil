using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.Events;
using LfrlAnvil.Reactive.Events.Decorators;
using LfrlAnvil.Reactive.Events.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.EventsTests.DecoratorsTests
{
    public class EventListenerTakeUntilDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber_WhenTargetDoesNotPublishEventImmediately()
        {
            var target = new EventSource<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerTakeUntilDecorator<int, string>( target );

            var _ = sut.Decorate( next, subscriber );

            using ( new AssertionScope() )
            {
                subscriber.DidNotReceive().Dispose();
                target.HasSubscribers.Should().BeTrue();
            }
        }

        [Fact]
        public void Decorate_ShouldDisposeTheSubscriber_WhenTargetPublishesEventImmediately()
        {
            var target = new HistoryEventSource<string>( capacity: 1 );
            target.Publish( Fixture.Create<string>() );

            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerTakeUntilDecorator<int, string>( target );

            var _ = sut.Decorate( next, subscriber );

            subscriber.Received().Dispose();
        }

        [Fact]
        public void Decorate_ShouldDisposeTheSubscriber_WhenTargetIsAlreadyDisposed()
        {
            var target = new EventSource<string>();
            target.Dispose();

            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerTakeUntilDecorator<int, string>( target );

            var _ = sut.Decorate( next, subscriber );

            subscriber.Received().Dispose();
        }

        [Fact]
        public void Decorate_ShouldDisposeTheSubscriber_WhenTargetPublishesAnyEvent()
        {
            var target = new EventSource<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerTakeUntilDecorator<int, string>( target );
            var _ = sut.Decorate( next, subscriber );

            target.Publish( Fixture.Create<string>() );

            subscriber.Received().Dispose();
        }

        [Fact]
        public void Decorate_ShouldDisposeTheSubscriber_WhenTargetDisposes()
        {
            var target = new EventSource<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerTakeUntilDecorator<int, string>( target );
            var _ = sut.Decorate( next, subscriber );

            target.Dispose();

            subscriber.Received().Dispose();
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactForwardsEvents()
        {
            var sourceEvents = new[] { 0, 1, 15 };
            var actualEvents = new List<int>();

            var target = new EventSource<string>();
            var next = EventListener.Create<int>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerTakeUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            foreach ( var e in sourceEvents )
                listener.React( e );

            actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
        {
            var target = new EventSource<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerTakeUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.Received().OnDispose( source );
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeDisposesTheTargetSubscriber(DisposalSource source)
        {
            var target = new EventSource<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerTakeUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            target.HasSubscribers.Should().BeFalse();
        }

        [Fact]
        public void TakeUntilExtension_ShouldCreateEventStreamThatForwardsEvents()
        {
            var sourceEvents = new[] { 0, 1, 15 };
            var actualEvents = new List<int>();

            var target = new EventSource<string>();
            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventSource<int>();
            var decorated = sut.TakeUntil( target );
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
        }

        [Fact]
        public void TakeUntilExtension_ShouldCreateEventStreamThatForwardsEvents_UntilTargetPublishesAnyEvent()
        {
            var sourceEvents = new[] { 0, 1, 15 };
            var expectedEvents = new[] { 0, 1 };
            var actualEvents = new List<int>();

            var target = new EventSource<string>();
            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventSource<int>();
            var decorated = sut.TakeUntil( target );
            decorated.Listen( next );

            foreach ( var e in sourceEvents.Take( 2 ) )
                sut.Publish( e );

            target.Publish( Fixture.Create<string>() );

            foreach ( var e in sourceEvents.Skip( 2 ) )
                sut.Publish( e );

            using ( new AssertionScope() )
            {
                actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
                sut.HasSubscribers.Should().BeFalse();
                target.HasSubscribers.Should().BeFalse();
            }
        }

        [Fact]
        public void TakeUntilExtension_ShouldCreateEventStreamThatDisposes_WhenTargetPublishesEventImmediately()
        {
            var target = new HistoryEventSource<string>( capacity: 1 );
            target.Publish( Fixture.Create<string>() );

            var next = Substitute.For<IEventListener<int>>();
            var sut = new EventSource<int>();
            var decorated = sut.TakeUntil( target );
            decorated.Listen( next );

            using ( new AssertionScope() )
            {
                sut.HasSubscribers.Should().BeFalse();
                target.HasSubscribers.Should().BeFalse();
            }
        }
    }
}
