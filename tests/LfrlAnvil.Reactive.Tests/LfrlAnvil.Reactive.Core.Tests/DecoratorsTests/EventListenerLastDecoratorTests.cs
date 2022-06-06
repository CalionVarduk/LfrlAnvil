using System.Collections.Generic;
using FluentAssertions;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests
{
    public class EventListenerLastDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerLastDecorator<int>();

            var _ = sut.Decorate( next, subscriber );

            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactDoesNotForwardAnyEvents()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerLastDecorator<int>();
            var listener = sut.Decorate( next, subscriber );

            foreach ( var e in sourceEvents )
                listener.React( e );

            actualEvents.Should().BeEmpty();
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextReactWithLastEvent(DisposalSource source)
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerLastDecorator<int>();
            var listener = sut.Decorate( next, subscriber );

            foreach ( var e in sourceEvents )
                listener.React( e );

            listener.OnDispose( source );

            actualEvents.Should().BeSequentiallyEqualTo( sourceEvents[^1] );
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeDoesNotCallNextReactWhenNoEventHasBeenReceived(DisposalSource source)
        {
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerLastDecorator<int>();
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.VerifyCalls().DidNotReceive( x => x.React( Arg.Any<int>() ) );
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
        {
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerLastDecorator<int>();
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.VerifyCalls().Received( x => x.OnDispose( source ) );
        }

        [Fact]
        public void LastExtension_ShouldCreateEventStreamThatForwardsTheLastEventWhenEventSourceDisposes()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.Last();
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            sut.Dispose();

            actualEvents.Should().BeSequentiallyEqualTo( sourceEvents[^1] );
        }
    }
}
