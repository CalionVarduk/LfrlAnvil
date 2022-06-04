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
    public class EventListenerSelectDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var next = Substitute.For<IEventListener<string>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSelectDecorator<int, string>( x => x.ToString() );

            var _ = sut.Decorate( next, subscriber );

            subscriber.VerifyCalls().DidNotReceive( x => x.Dispose() );
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactMapsEverySourceEventIntoNextEvent()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[] { "1", "2", "3", "5", "7", "11", "13", "17", "19", "23" };
            var actualEvents = new List<string>();

            var next = EventListener.Create<string>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSelectDecorator<int, string>( x => x.ToString() );
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
            var next = Substitute.For<IEventListener<string>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSelectDecorator<int, string>( x => x.ToString() );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.VerifyCalls().Received( x => x.OnDispose( source ) );
        }

        [Fact]
        public void SelectExtension_ShouldCreateEventStreamThatMapsEverySourceEventIntoNextEvent()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[] { "1", "2", "3", "5", "7", "11", "13", "17", "19", "23" };
            var actualEvents = new List<string>();

            var next = EventListener.Create<string>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.Select( x => x.ToString() );
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }
}
