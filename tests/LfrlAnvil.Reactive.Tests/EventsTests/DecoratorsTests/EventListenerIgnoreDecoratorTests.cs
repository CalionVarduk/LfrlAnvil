using System.Collections.Generic;
using FluentAssertions;
using LfrlAnvil.Reactive.Events;
using LfrlAnvil.Reactive.Events.Decorators;
using LfrlAnvil.Reactive.Events.Extensions;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.EventsTests.DecoratorsTests
{
    public class EventListenerIgnoreDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerIgnoreDecorator<int>();

            var _ = sut.Decorate( next, subscriber );

            subscriber.DidNotReceive().Dispose();
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactIgnoresEvents()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerIgnoreDecorator<int>();
            var listener = sut.Decorate( next, subscriber );

            foreach ( var e in sourceEvents )
                listener.React( e );

            actualEvents.Should().BeEmpty();
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
        {
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerIgnoreDecorator<int>();
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.Received().OnDispose( source );
        }

        [Fact]
        public void IgnoreExtension_ShouldCreateListenerWhoseReactIgnoresEvents()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.Ignore();
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeEmpty();
        }
    }
}
