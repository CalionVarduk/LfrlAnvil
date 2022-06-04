using System.Collections.Generic;
using FluentAssertions;
using LfrlAnvil.Reactive.Events;
using LfrlAnvil.Reactive.Events.Decorators;
using LfrlAnvil.Reactive.Events.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.EventsTests.DecoratorsTests
{
    public class EventListenerDistinctUntilChangedDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerDistinctUntilChangedDecorator<int, int>( v => v, EqualityComparer<int>.Default );

            var _ = sut.Decorate( next, subscriber );

            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactEmitsDistinctSourceEventsUntilChangeOccurs()
        {
            var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
            var expectedEvents = new[] { 1, 2, 3, 7, 5, 3, 5, 7, 1, 7 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerDistinctUntilChangedDecorator<int, int>( v => v, EqualityComparer<int>.Default );
            var listener = sut.Decorate( next, subscriber );

            foreach ( var e in sourceEvents )
                listener.React( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
        {
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerDistinctUntilChangedDecorator<int, int>( v => v, EqualityComparer<int>.Default );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.VerifyCalls().Received( x => x.OnDispose( source ) );
        }

        [Fact]
        public void DistinctUntilChangedExtension_ShouldCreateEventStreamThatEmitsDistinctSourceEventsUntilChangeOccurs()
        {
            var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
            var expectedEvents = new[] { 1, 2, 3, 7, 5, 3, 5, 7, 1, 7 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.DistinctUntilChanged();
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }

        [Fact]
        public void
            DistinctUntilChangedExtension_WithExplicitComparer_ShouldCreateEventStreamThatEmitsDistinctSourceEventsUntilChangeOccurs()
        {
            var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
            var expectedEvents = new[] { 1, 2, 3, 7, 5, 3, 5, 7, 1, 7 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.DistinctUntilChanged( EqualityComparer<int>.Default );
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }
}
