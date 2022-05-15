using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Events;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.EventsTests.EventSourceTests
{
    public abstract class GenericEventSourceTests<TEvent> : TestsBase
    {
        [Fact]
        public void Ctor_ShouldCreateEventSourceInDefaultState()
        {
            var sut = new EventSource<TEvent>();

            using ( new AssertionScope() )
            {
                sut.IsDisposed.Should().BeFalse();
                sut.HasSubscribers.Should().BeFalse();
                sut.Subscribers.Should().BeEmpty();
            }
        }

        [Fact]
        public void Listen_ShouldAddNewSubscriber_WhenNotDisposed()
        {
            var sut = new EventSource<TEvent>();

            var subscriber = sut.Listen( EventListener<TEvent>.Empty );

            using ( new AssertionScope() )
            {
                sut.HasSubscribers.Should().BeTrue();
                sut.Subscribers.Should().BeSequentiallyEqualTo( subscriber );
            }
        }

        [Fact]
        public void Listen_ShouldReturnDisposedSubscriber_WhenDisposed()
        {
            var sut = new EventSource<TEvent>();
            sut.Dispose();

            var subscriber = sut.Listen( EventListener<TEvent>.Empty );

            using ( new AssertionScope() )
            {
                subscriber.IsDisposed.Should().BeTrue();
                sut.HasSubscribers.Should().BeFalse();
                sut.Subscribers.Should().BeEmpty();
            }
        }

        [Fact]
        public void Listen_ShouldCallListenerOnDispose_WhenDisposed()
        {
            var listener = Substitute.For<IEventListener<TEvent>>();
            var sut = new EventSource<TEvent>();
            sut.Dispose();

            sut.Listen( listener );

            listener.Received().OnDispose();
        }

        [Fact]
        public void Listen_ShouldAddAnotherSubscriber_WhenAlreadyContainsOne()
        {
            var sut = new EventSource<TEvent>();
            var subscriber1 = sut.Listen( EventListener<TEvent>.Empty );

            var subscriber2 = sut.Listen( EventListener<TEvent>.Empty );

            using ( new AssertionScope() )
            {
                sut.HasSubscribers.Should().BeTrue();
                sut.Subscribers.Should().BeSequentiallyEqualTo( subscriber1, subscriber2 );
            }
        }

        [Fact]
        public void EventSubscriberDispose_ShouldRemoveSubscriber()
        {
            var sut = new EventSource<TEvent>();
            var subscriber = sut.Listen( EventListener<TEvent>.Empty );

            subscriber.Dispose();

            using ( new AssertionScope() )
            {
                subscriber.IsDisposed.Should().BeTrue();
                sut.HasSubscribers.Should().BeFalse();
                sut.Subscribers.Should().BeEmpty();
            }
        }

        [Fact]
        public void EventSubscriberDispose_ShouldDoNothing_WhenSubscriberIsAlreadyDisposed()
        {
            var sut = new EventSource<TEvent>();
            var subscriber = sut.Listen( EventListener<TEvent>.Empty );
            subscriber.Dispose();

            var action = Lambda.Of( () => subscriber.Dispose() );

            action.Should().NotThrow();
        }

        [Fact]
        public void Publish_ShouldCallEveryListenerReact_WhenNotDisposed()
        {
            var @event = Fixture.Create<TEvent>();
            var listener1 = Substitute.For<IEventListener<TEvent>>();
            var listener2 = Substitute.For<IEventListener<TEvent>>();
            var sut = new EventSource<TEvent>();
            sut.Listen( listener1 );
            sut.Listen( listener2 );

            sut.Publish( @event );

            using ( new AssertionScope() )
            {
                listener1.Received().React( @event );
                listener2.Received().React( @event );
            }
        }

        [Fact]
        public void Publish_ShouldCallOnlyFirstListenerReact_WhenFirstListenerReactDisposedNextListener()
        {
            var @event = Fixture.Create<TEvent>();
            var sut = new EventSource<TEvent>();
            var listener1 = EventListener.Create<TEvent>( _ => sut.Subscribers.Last().Dispose() );
            var listener2 = Substitute.For<IEventListener<TEvent>>();
            var subscriber1 = sut.Listen( listener1 );
            var subscriber2 = sut.Listen( listener2 );

            sut.Publish( @event );

            using ( new AssertionScope() )
            {
                sut.Subscribers.Should().BeSequentiallyEqualTo( subscriber1 );
                subscriber2.IsDisposed.Should().BeTrue();
                listener2.DidNotReceive().React( @event );
            }
        }

        [Fact]
        public void Publish_ShouldThrowObjectDisposedException_WhenDisposed()
        {
            var @event = Fixture.Create<TEvent>();
            var sut = new EventSource<TEvent>();
            sut.Dispose();

            var action = Lambda.Of( () => sut.Publish( @event ) );

            action.Should().ThrowExactly<ObjectDisposedException>();
        }

        [Fact]
        public void Dispose_ShouldDisposeAllSubscribers()
        {
            var sut = new EventSource<TEvent>();
            var subscriber1 = sut.Listen( EventListener<TEvent>.Empty );
            var subscriber2 = sut.Listen( EventListener<TEvent>.Empty );

            sut.Dispose();

            using ( new AssertionScope() )
            {
                sut.IsDisposed.Should().BeTrue();
                sut.Subscribers.Should().BeEmpty();
                subscriber1.IsDisposed.Should().BeTrue();
                subscriber2.IsDisposed.Should().BeTrue();
            }
        }

        [Fact]
        public void Dispose_ShouldCallEveryListenerOnDispose()
        {
            var listener1 = Substitute.For<IEventListener<TEvent>>();
            var listener2 = Substitute.For<IEventListener<TEvent>>();
            var sut = new EventSource<TEvent>();
            sut.Listen( listener1 );
            sut.Listen( listener2 );

            sut.Dispose();

            using ( new AssertionScope() )
            {
                listener1.Received().OnDispose();
                listener2.Received().OnDispose();
            }
        }

        [Fact]
        public void Dispose_ShouldCallOnlyFirstListenerOnDispose_WhenFirstListenerOnDisposeDisposedNextListener()
        {
            var sut = new EventSource<TEvent>();
            IEventSubscriber? subscriber2 = null;
            var listener1 = EventListener.Create<TEvent>( _ => { }, () => subscriber2?.Dispose() );
            var listener2 = Substitute.For<IEventListener<TEvent>>();
            sut.Listen( listener1 );
            subscriber2 = sut.Listen( listener2 );

            sut.Dispose();

            listener2.DidNotReceive().OnDispose();
        }

        [Fact]
        public void Dispose_ShouldDoNothing_WhenAlreadyDisposed()
        {
            var sut = new EventSource<TEvent>();
            sut.Dispose();

            var action = Lambda.Of( () => sut.Dispose() );

            action.Should().NotThrow();
        }

        [Fact]
        public void IEventStreamListen_ShouldBeEquivalentToGenericListen_WhenListenerIsOfCorrectType()
        {
            var source = new EventSource<TEvent>();
            IEventStream sut = source;

            var subscriber = sut.Listen( EventListener<TEvent>.Empty );

            using ( new AssertionScope() )
            {
                source.HasSubscribers.Should().BeTrue();
                source.Subscribers.Should().BeSequentiallyEqualTo( subscriber );
            }
        }

        [Fact]
        public void IEventStreamListen_ShouldThrowInvalidArgumentTypeException_WhenListenerIsNotOfCorrectType()
        {
            var listener = EventListener<Invalid>.Empty;
            IEventStream sut = new EventSource<TEvent>();

            var action = Lambda.Of( () => sut.Listen( listener ) );

            action.Should()
                .ThrowExactly<InvalidArgumentTypeException>()
                .AndMatch( e => e.Argument == listener && e.ExpectedType == typeof( IEventListener<TEvent> ) );
        }

        [Fact]
        public void IEventSourcePublish_ShouldBeEquivalentToGenericPublish_WhenEventIsOfCorrectType()
        {
            var @event = Fixture.Create<TEvent>();
            var listener = Substitute.For<IEventListener<TEvent>>();
            var source = new EventSource<TEvent>();
            source.Listen( listener );
            IEventSource sut = source;

            sut.Publish( @event );

            listener.Received().React( @event );
        }

        [Fact]
        public void IEventSourcePublish_ShouldThrowInvalidArgumentTypeException_WhenEventIsNotOfCorrectType()
        {
            var @event = Fixture.Create<Invalid>();
            IEventSource sut = new EventSource<TEvent>();

            var action = Lambda.Of( () => sut.Publish( @event ) );

            action.Should()
                .ThrowExactly<InvalidArgumentTypeException>()
                .AndMatch( e => e.Argument == @event && e.ExpectedType == typeof( TEvent ) );
        }

        private sealed class Invalid
        {
            public TEvent? Event { get; set; }
        }
    }
}
