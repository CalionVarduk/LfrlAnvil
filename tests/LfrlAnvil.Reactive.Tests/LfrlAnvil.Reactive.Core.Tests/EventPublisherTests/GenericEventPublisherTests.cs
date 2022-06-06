using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.EventPublisherTests
{
    public abstract class GenericEventPublisherTests<TEvent> : TestsBase
    {
        [Fact]
        public void Ctor_ShouldCreateEventSourceInDefaultState()
        {
            var sut = new EventPublisher<TEvent>();

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
            var sut = new EventPublisher<TEvent>();

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
            var sut = new EventPublisher<TEvent>();
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
            var sut = new EventPublisher<TEvent>();
            sut.Dispose();

            sut.Listen( listener );

            listener.VerifyCalls().Received( x => x.OnDispose( DisposalSource.EventSource ) );
        }

        [Fact]
        public void Listen_ShouldAddAnotherSubscriber_WhenAlreadyContainsOne()
        {
            var sut = new EventPublisher<TEvent>();
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
            var sut = new EventPublisher<TEvent>();
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
        public void EventSubscriberDispose_ShouldCallListenerOnDispose()
        {
            var listener = Substitute.For<IEventListener<TEvent>>();
            var sut = new EventPublisher<TEvent>();
            var subscriber = sut.Listen( listener );

            subscriber.Dispose();

            listener.VerifyCalls().Received( x => x.OnDispose( DisposalSource.Subscriber ) );
        }

        [Fact]
        public void EventSubscriberDispose_ShouldDoNothing_WhenSubscriberIsAlreadyDisposed()
        {
            var listener = Substitute.For<IEventListener<TEvent>>();
            var sut = new EventPublisher<TEvent>();
            var subscriber = sut.Listen( listener );
            subscriber.Dispose();
            listener.ClearReceivedCalls();

            subscriber.Dispose();

            listener.VerifyCalls().DidNotReceive( x => x.OnDispose( Arg.Any<DisposalSource>() ) );
        }

        [Fact]
        public void Publish_ShouldCallEveryListenerReact_WhenNotDisposed()
        {
            var @event = Fixture.Create<TEvent>();
            var listener1 = Substitute.For<IEventListener<TEvent>>();
            var listener2 = Substitute.For<IEventListener<TEvent>>();
            var sut = new EventPublisher<TEvent>();
            sut.Listen( listener1 );
            sut.Listen( listener2 );

            sut.Publish( @event );

            using ( new AssertionScope() )
            {
                listener1.VerifyCalls().Received( x => x.React( @event ) );
                listener2.VerifyCalls().Received( x => x.React( @event ) );
            }
        }

        [Fact]
        public void Publish_ShouldCallOnlyFirstListenerReact_WhenFirstListenerReactDisposedNextListener()
        {
            var @event = Fixture.Create<TEvent>();
            var sut = new EventPublisher<TEvent>();
            var listener1 = EventListener.Create<TEvent>( _ => sut.Subscribers.Last().Dispose() );
            var listener2 = Substitute.For<IEventListener<TEvent>>();
            var subscriber1 = sut.Listen( listener1 );
            var subscriber2 = sut.Listen( listener2 );

            sut.Publish( @event );

            using ( new AssertionScope() )
            {
                sut.Subscribers.Should().BeSequentiallyEqualTo( subscriber1 );
                subscriber2.IsDisposed.Should().BeTrue();
                listener2.VerifyCalls().DidNotReceive( x => x.React( @event ) );
            }
        }

        [Fact]
        public void Publish_ShouldThrowObjectDisposedException_WhenDisposed()
        {
            var @event = Fixture.Create<TEvent>();
            var sut = new EventPublisher<TEvent>();
            sut.Dispose();

            var action = Lambda.Of( () => sut.Publish( @event ) );

            action.Should().ThrowExactly<ObjectDisposedException>();
        }

        [Fact]
        public void Dispose_ShouldDisposeAllSubscribers()
        {
            var sut = new EventPublisher<TEvent>();
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
            var sut = new EventPublisher<TEvent>();
            sut.Listen( listener1 );
            sut.Listen( listener2 );

            sut.Dispose();

            using ( new AssertionScope() )
            {
                listener1.VerifyCalls().Received( x => x.OnDispose( DisposalSource.EventSource ) );
                listener2.VerifyCalls().Received( x => x.OnDispose( DisposalSource.EventSource ) );
            }
        }

        [Fact]
        public void Dispose_ShouldCallOnlyFirstListenerOnDisposeWithEventSource_WhenFirstListenerOnDisposeDisposedNextListener()
        {
            var sut = new EventPublisher<TEvent>();
            IEventSubscriber? subscriber2 = null;
            var listener1 = EventListener.Create<TEvent>( _ => { }, _ => subscriber2?.Dispose() );
            var listener2 = Substitute.For<IEventListener<TEvent>>();
            sut.Listen( listener1 );
            subscriber2 = sut.Listen( listener2 );

            sut.Dispose();

            listener2.VerifyCalls().DidNotReceive( x => x.OnDispose( DisposalSource.EventSource ) );
            listener2.VerifyCalls().Received( x => x.OnDispose( DisposalSource.Subscriber ) );
        }

        [Fact]
        public void Dispose_ShouldDoNothing_WhenAlreadyDisposed()
        {
            var sut = new EventPublisher<TEvent>();
            sut.Dispose();

            var action = Lambda.Of( () => sut.Dispose() );

            action.Should().NotThrow();
        }

        [Fact]
        public void IEventStreamListen_ShouldBeEquivalentToGenericListen_WhenListenerIsOfCorrectType()
        {
            var source = new EventPublisher<TEvent>();
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
            IEventStream sut = new EventPublisher<TEvent>();

            var action = Lambda.Of( () => sut.Listen( listener ) );

            action.Should()
                .ThrowExactly<InvalidArgumentTypeException>()
                .AndMatch( e => e.Argument == listener && e.ExpectedType == typeof( IEventListener<TEvent> ) );
        }

        [Fact]
        public void IEventPublisherPublish_ShouldBeEquivalentToGenericPublish_WhenEventIsOfCorrectType()
        {
            var @event = Fixture.Create<TEvent>();
            var listener = Substitute.For<IEventListener<TEvent>>();
            var source = new EventPublisher<TEvent>();
            source.Listen( listener );
            IEventPublisher sut = source;

            sut.Publish( @event );

            listener.VerifyCalls().Received( x => x.React( @event ) );
        }

        [Fact]
        public void IEventPublisherPublish_ShouldThrowInvalidArgumentTypeException_WhenEventIsNotOfCorrectType()
        {
            var @event = Fixture.Create<Invalid>();
            IEventPublisher sut = new EventPublisher<TEvent>();

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
