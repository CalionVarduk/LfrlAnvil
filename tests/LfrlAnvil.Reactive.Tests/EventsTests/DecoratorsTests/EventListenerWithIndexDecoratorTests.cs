using System.Collections.Generic;
using FluentAssertions;
using LfrlAnvil.Reactive.Events;
using LfrlAnvil.Reactive.Events.Composites;
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

            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactAttachesAnIndex()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[]
            {
                new WithIndex<int>( 1, 0 ),
                new WithIndex<int>( 2, 1 ),
                new WithIndex<int>( 3, 2 ),
                new WithIndex<int>( 5, 3 ),
                new WithIndex<int>( 7, 4 ),
                new WithIndex<int>( 11, 5 ),
                new WithIndex<int>( 13, 6 ),
                new WithIndex<int>( 17, 7 ),
                new WithIndex<int>( 19, 8 ),
                new WithIndex<int>( 23, 9 )
            };

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

            next.VerifyCalls().Received( x => x.OnDispose( source ) );
        }

        [Fact]
        public void WithIndexExtension_ShouldCreateEventStreamThatAttachesAndIndex()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[]
            {
                new WithIndex<int>( 1, 0 ),
                new WithIndex<int>( 2, 1 ),
                new WithIndex<int>( 3, 2 ),
                new WithIndex<int>( 5, 3 ),
                new WithIndex<int>( 7, 4 ),
                new WithIndex<int>( 11, 5 ),
                new WithIndex<int>( 13, 6 ),
                new WithIndex<int>( 17, 7 ),
                new WithIndex<int>( 19, 8 ),
                new WithIndex<int>( 23, 9 )
            };

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
