﻿using System.Collections.Generic;
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
    public class EventListenerSelectManyDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var next = Substitute.For<IEventListener<string>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSelectManyDecorator<int, string>( x => new[] { x.ToString(), (x * 2).ToString() } );

            var _ = sut.Decorate( next, subscriber );

            subscriber.DidNotReceive().Dispose();
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactMapsEverySourceEventIntoMultipleNextEvents()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[]
                { "1", "2", "2", "4", "3", "6", "5", "10", "7", "14", "11", "22", "13", "26", "17", "34", "19", "38", "23", "46" };

            var actualEvents = new List<string>();

            var next = EventListener.Create<string>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerSelectManyDecorator<int, string>( x => new[] { x.ToString(), (x * 2).ToString() } );
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
            var sut = new EventListenerSelectManyDecorator<int, string>( x => new[] { x.ToString(), (x * 2).ToString() } );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.Received().OnDispose( source );
        }

        [Fact]
        public void SelectManyExtension_ShouldCreateEventStreamThatMapsEverySourceEventIntoMultipleNextEvents()
        {
            var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
            var expectedEvents = new[]
                { "1", "2", "2", "4", "3", "6", "5", "10", "7", "14", "11", "22", "13", "26", "17", "34", "19", "38", "23", "46" };

            var actualEvents = new List<string>();

            var next = EventListener.Create<string>( actualEvents.Add );
            var sut = new EventPublisher<int>();
            var decorated = sut.SelectMany( x => new[] { x.ToString(), (x * 2).ToString() } );
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            actualEvents.Should().BeSequentiallyEqualTo( expectedEvents );
        }
    }
}
