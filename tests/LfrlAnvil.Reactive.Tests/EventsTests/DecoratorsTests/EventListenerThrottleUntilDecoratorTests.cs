using System.Collections.Generic;
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
    public class EventListenerThrottleUntilDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber_WhenTargetIsNotDisposed()
        {
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target );

            var _ = sut.Decorate( next, subscriber );

            using ( new AssertionScope() )
            {
                subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
                target.HasSubscribers.Should().BeFalse();
            }
        }

        [Fact]
        public void Decorate_ShouldDisposeTheSubscriber_WhenTargetIsDisposed()
        {
            var target = new EventPublisher<string>();
            target.Dispose();

            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target );

            var _ = sut.Decorate( next, subscriber );

            using ( new AssertionScope() )
            {
                subscriber.VerifyCalls().Received( x => x.Dispose() );
                target.HasSubscribers.Should().BeFalse();
            }
        }

        [Fact]
        public void Decorate_ShouldCreateListenerThatDoesNothing_WhenTargetEmitsAnyEventFirst()
        {
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target );
            var _ = sut.Decorate( next, subscriber );

            target.Publish( Fixture.Create<string>() );

            using ( new AssertionScope() )
            {
                next.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<int>() ) );
                subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
            }
        }

        [Fact]
        public void Decorate_ShouldCreateListenerThatOnReactAddsNewSubscriberToTarget()
        {
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            listener.React( Fixture.Create<int>() );

            using ( new AssertionScope() )
            {
                next.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<int>() ) );
                subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
                target.Subscribers.Should().HaveCount( 1 );
            }
        }

        [Fact]
        public void Decorate_ShouldCreateListenerThatAfterReactAndTargetEventEmissionForwardsItsEventAndDisposesTemporaryTargetSubscriber()
        {
            var sourceEvent = 1;
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            listener.React( sourceEvent );
            target.Publish( Fixture.Create<string>() );

            using ( new AssertionScope() )
            {
                next.VerifyCalls().Received( x => x.React( sourceEvent ) );
                subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
                target.HasSubscribers.Should().BeFalse();
            }
        }

        [Fact]
        public void
            Decorate_ShouldCreateListenerThatAfterReactAndTargetEventEmissionForwardsItsEventAndDisposesTemporaryTargetSubscriber_WhenTargetEmitsImmediately()
        {
            var sourceEvent = 1;
            var target = new HistoryEventPublisher<string>( capacity: 1 );
            target.Publish( Fixture.Create<string>() );

            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            listener.React( sourceEvent );

            using ( new AssertionScope() )
            {
                next.VerifyCalls().Received( x => x.React( sourceEvent ) );
                subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
                target.HasSubscribers.Should().BeFalse();
            }
        }

        [Fact]
        public void Decorate_ShouldCreateListenerThatAfterReactWaitsForTargetEmissionAndForwardsOnlyItsFirstEvent()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[] { 1, 5, 13 };
            var actualEvents = new List<int>();

            var target = new EventPublisher<string>();
            var next = EventListener.Create<int>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target.ElementAt( 1 ) );
            var listener = sut.Decorate( next, subscriber );

            listener.React( sourceEvents[0] );
            target.Publish( Fixture.Create<string>() );

            listener.React( sourceEvents[1] );
            listener.React( sourceEvents[2] );
            target.Publish( Fixture.Create<string>() );

            listener.React( sourceEvents[3] );
            target.Publish( Fixture.Create<string>() );

            listener.React( sourceEvents[4] );
            listener.React( sourceEvents[5] );
            target.Publish( Fixture.Create<string>() );

            target.Publish( Fixture.Create<string>() );

            listener.React( sourceEvents[6] );
            listener.React( sourceEvents[7] );
            listener.React( sourceEvents[8] );
            listener.React( sourceEvents[9] );
            target.Publish( Fixture.Create<string>() );
            target.Publish( Fixture.Create<string>() );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }

        [Fact]
        public void Decorate_ShouldCreateListenerThatAfterReactDisposesSubscriber_WhenTargetDisposes()
        {
            var sourceEvent = 1;
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            listener.React( sourceEvent );
            target.Dispose();

            using ( new AssertionScope() )
            {
                next.VerifyCalls().DidNotReceive( x => x.React( sourceEvent ) );
                subscriber.VerifyCalls().Received( x => x.Dispose() );
                target.HasSubscribers.Should().BeFalse();
            }
        }

        [Fact]
        public void Decorate_ShouldCreateListenerThatAfterReactDisposesSubscriber_WhenTargetIsAlreadyDisposed()
        {
            var sourceEvent = 1;
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            target.Dispose();
            listener.React( sourceEvent );

            using ( new AssertionScope() )
            {
                next.VerifyCalls().DidNotReceive( x => x.React( sourceEvent ) );
                subscriber.VerifyCalls().Received( x => x.Dispose() );
                target.HasSubscribers.Should().BeFalse();
            }
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
        {
            var target = new EventPublisher<string>();
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target );
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
            var sut = new EventListenerThrottleUntilDecorator<int, string>( target );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            target.HasSubscribers.Should().BeFalse();
        }

        [Fact]
        public void ThrottleUntilExtension_ShouldCreateEventStreamThatWaitsForTargetEmissionAndForwardsOnlyItsFirstEvent()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[] { 1, 5, 13 };
            var actualEvents = new List<int>();

            var target = new EventPublisher<string>();
            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.ThrottleUntil( target.ElementAt( 1 ) );
            decorated.Listen( next );

            sut.Publish( sourceEvents[0] );
            target.Publish( Fixture.Create<string>() );

            sut.Publish( sourceEvents[1] );
            sut.Publish( sourceEvents[2] );
            target.Publish( Fixture.Create<string>() );

            sut.Publish( sourceEvents[3] );
            target.Publish( Fixture.Create<string>() );

            sut.Publish( sourceEvents[4] );
            sut.Publish( sourceEvents[5] );
            target.Publish( Fixture.Create<string>() );

            target.Publish( Fixture.Create<string>() );

            sut.Publish( sourceEvents[6] );
            sut.Publish( sourceEvents[7] );
            sut.Publish( sourceEvents[8] );
            sut.Publish( sourceEvents[9] );
            target.Publish( Fixture.Create<string>() );
            target.Publish( Fixture.Create<string>() );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }

        [Fact]
        public void ThrottleUntilExtension_ShouldCreateEventStreamThatEmitsEveryOtherEvent_WhenTargetIsSource()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[] { 1, 3, 7, 13, 19 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.ThrottleUntil( sut );
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }
}
