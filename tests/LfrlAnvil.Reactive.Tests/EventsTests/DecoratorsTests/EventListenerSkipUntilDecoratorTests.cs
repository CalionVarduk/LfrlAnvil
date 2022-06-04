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
    public class EventListenerSkipUntilDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSkipUntilDecorator<int, string>( target );

            var _ = sut.Decorate( next, subscriber );

            using ( new AssertionScope() )
            {
                subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
                target.HasSubscribers.Should().BeTrue();
            }
        }

        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber_WhenTargetPublishesEventImmediately()
        {
            var target = new HistoryEventPublisher<string>( capacity: 1 );
            target.Publish( Fixture.Create<string>() );

            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSkipUntilDecorator<int, string>( target );

            var _ = sut.Decorate( next, subscriber );

            using ( new AssertionScope() )
            {
                subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
                target.HasSubscribers.Should().BeFalse();
            }
        }

        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber_WhenTargetIsAlreadyDisposed()
        {
            var target = new EventPublisher<string>();
            target.Dispose();

            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSkipUntilDecorator<int, string>( target );

            var _ = sut.Decorate( next, subscriber );

            using ( new AssertionScope() )
            {
                subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
                target.HasSubscribers.Should().BeFalse();
            }
        }

        [Fact]
        public void Decorate_ShouldCreateListenerThatIgnoresEventsUntilTargetPublishesAnyEvent()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[] { 5, 7, 11, 13, 17, 19, 23 };
            var actualEvents = new List<int>();

            var target = new EventPublisher<string>();
            var next = EventListener.Create<int>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSkipUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            foreach ( var e in sourceEvents.Take( 3 ) )
                listener.React( e );

            target.Publish( Fixture.Create<string>() );

            foreach ( var e in sourceEvents.Skip( 3 ) )
                listener.React( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }

        [Fact]
        public void Decorate_ShouldCreateListenerThatDisposesTargetSubscriber_WhenTargetPublishesAnyEvent()
        {
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSkipUntilDecorator<int, string>( target );
            var _ = sut.Decorate( next, subscriber );

            target.Publish( Fixture.Create<string>() );

            target.HasSubscribers.Should().BeFalse();
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
        {
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSkipUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.VerifyCalls().Received( x => x.OnDispose( source ) );
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeDisposesTheTargetSubscriber(DisposalSource source)
        {
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSkipUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            target.HasSubscribers.Should().BeFalse();
        }

        [Fact]
        public void SkipUntilExtension_ShouldCreateEventStreamThatIgnoresEventsUntilTargetPublishesAnyEvent()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[] { 5, 7, 11, 13, 17, 19, 23 };
            var actualEvents = new List<int>();

            var target = new EventPublisher<string>();
            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.SkipUntil( target );
            decorated.Listen( next );

            foreach ( var e in sourceEvents.Take( 3 ) )
                sut.Publish( e );

            target.Publish( Fixture.Create<string>() );

            foreach ( var e in sourceEvents.Skip( 3 ) )
                sut.Publish( e );

            using ( new AssertionScope() )
            {
                actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
                target.HasSubscribers.Should().BeFalse();
            }
        }

        [Fact]
        public void SkipUntilExtension_ShouldDisposeTargetSubscriber_WhenTargetIsSourceAndSourcePublishedAnyEvent()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.SkipUntil( sut );
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            using ( new AssertionScope() )
            {
                sut.Subscribers.Should().HaveCount( 1 );
                actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
            }
        }
    }
}
