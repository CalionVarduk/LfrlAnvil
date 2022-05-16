﻿using System.Collections.Generic;
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
    public class EventListenerForEachDecoratorTests : TestsBase
    {
        [Fact]
        public void Decorate_ShouldNotDisposeTheSubscriber()
        {
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerForEachDecorator<int>( _ => { } );

            var _ = sut.Decorate( next, subscriber );

            subscriber.DidNotReceive().Dispose();
        }

        [Fact]
        public void Decorate_ShouldCreateListenerWhoseReactCallsActionForEachSourceEvent()
        {
            var sourceEvents = new[] { 0, 1, 15 };
            var calledEvents = new List<int>();
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerForEachDecorator<int>( x => calledEvents.Add( x ) );
            var listener = sut.Decorate( next, subscriber );

            foreach ( var e in sourceEvents )
                listener.React( e );

            using ( new AssertionScope() )
            {
                actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
                calledEvents.Should().BeSequentiallyEqualTo( sourceEvents );
            }
        }

        [Theory]
        [InlineData( DisposalSource.EventSource )]
        [InlineData( DisposalSource.Subscriber )]
        public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
        {
            var next = Substitute.For<IEventListener<int>>();
            var subscriber = Substitute.For<IEventSubscriber>();
            var sut = new EventListenerForEachDecorator<int>( _ => { } );
            var listener = sut.Decorate( next, subscriber );

            listener.OnDispose( source );

            next.Received().OnDispose( source );
        }

        [Fact]
        public void ForEachExtension_ShouldCreateEventStreamThatCallsActionForEachSourceEvent()
        {
            var sourceEvents = new[] { 0, 1, 15 };
            var calledEvents = new List<int>();
            var actualEvents = new List<int>();

            var next = EventListener.Create<int>( actualEvents.Add );
            var sut = new EventSource<int>();
            var decorated = sut.ForEach( x => calledEvents.Add( x ) );
            decorated.Listen( next );

            foreach ( var e in sourceEvents )
                sut.Publish( e );

            using ( new AssertionScope() )
            {
                actualEvents.Should().BeSequentiallyEqualTo( sourceEvents );
                calledEvents.Should().BeSequentiallyEqualTo( sourceEvents );
            }
        }
    }
}
