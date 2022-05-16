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
    public class EventListenerWithIndexDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var next = Substitute.For<IEventListener<WithIndex<int>>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerWithIndexDecorator<int>();

            var _ = sut.Decorate( next, subscriber );

            subscriber.DidNotReceive().Dispose();
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactAttachesAnIndex()
        {
            var sourceEvents = new[] { 3, 7, 15 };
            var expectedEvents = new[] { new WithIndex<int>( 3, 0 ), new WithIndex<int>( 7, 1 ), new WithIndex<int>( 15, 2 ) };
            var actualEvents = new List<WithIndex<int>>();

            var next = EventListener.Create<WithIndex<int>>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerWithIndexDecorator<int>();
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
            var next = Substitute.For<IEventListener<WithIndex<int>>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerWithIndexDecorator<int>();
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.Received().OnDispose( source );
        }

        [Fact]
        public void WithIndexExtension_ShouldCreateEventStreamThatAttachesAndIndex()
        {
            var sourceEvents = new[] { 3, 7, 15 };
            var expectedEvents = new[] { new WithIndex<int>( 3, 0 ), new WithIndex<int>( 7, 1 ), new WithIndex<int>( 15, 2 ) };
            var actualEvents = new List<WithIndex<int>>();

            var next = EventListener.Create<WithIndex<int>>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.WithIndex();
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }
}
