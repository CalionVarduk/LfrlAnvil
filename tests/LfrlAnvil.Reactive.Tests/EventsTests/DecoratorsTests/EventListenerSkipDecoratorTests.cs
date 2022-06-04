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
    public class EventListenerSkipDecoratorTests : TestsBase
    {
        [Theory]
        [InlineData( -1 )]
        [InlineData( 0 )]
        [InlineData( 1 )]
        public void Decorate_ShouldNotDisposeTheSubscriber(int count)
        {
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSkipDecorator<int>( count );

            var _ = sut.Decorate( next, subscriber );

            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactIgnoresFirstCountEvents()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[] { 5, 7, 11, 13, 17, 19, 23 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSkipDecorator<int>( count: 3 );
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
            var sut = new EventListenerSkipDecorator<int>( count: 3 );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.VerifyCalls().Received( x => x.OnDispose( source ) );
        }

        [Fact]
        public void SkipExtension_ShouldCreateEventStreamThatIgnoresFirstCountEvents()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[] { 5, 7, 11, 13, 17, 19, 23 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.Skip( 3 );
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }
}
