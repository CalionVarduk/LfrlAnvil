using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Exchanges;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Reactive.Tests.ExchangesTests.EventExchangesTests;

public class EventExchangesTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyExchange()
    {
        var sut = new EventExchange();

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeFalse();
            sut.GetRegisteredEventTypes().Should().BeEmpty();
        }
    }

    [Fact]
    public void RegisterPublisher_ShouldAddNewPublisher_WhenPublisherForTheSpecifiedEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var result = sut.RegisterPublisher<int>();

        using ( new AssertionScope() )
        {
            sut.GetRegisteredEventTypes().Should().BeEquivalentTo( typeof( int ) );
            result.Subscribers.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void RegisterPublisher_ShouldAddMultiplePublishers_WhenPublishersForTheSpecifiedEventTypesDoNotExist()
    {
        var sut = new EventExchange();
        var intPublisher = sut.RegisterPublisher<int>();
        var stringPublisher = sut.RegisterPublisher<string>();
        var guidPublisher = sut.RegisterPublisher<Guid>();

        using ( new AssertionScope() )
        {
            sut.GetRegisteredEventTypes().Should().BeEquivalentTo( typeof( int ), typeof( string ), typeof( Guid ) );
            intPublisher.Subscribers.Should().HaveCount( 1 );
            stringPublisher.Subscribers.Should().HaveCount( 1 );
            guidPublisher.Subscribers.Should().HaveCount( 1 );
        }
    }

    [Fact]
    public void RegisterPublisher_WithExplicitPublisher_ShouldAddNewPublisher_WhenPublisherForTheSpecifiedEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var publisher = new EventPublisher<int>();
        var result = sut.RegisterPublisher( publisher );

        using ( new AssertionScope() )
        {
            sut.GetRegisteredEventTypes().Should().BeEquivalentTo( typeof( int ) );
            result.Subscribers.Should().HaveCount( 1 );
            result.Should().BeSameAs( publisher );
        }
    }

    [Fact]
    public void RegisterPublisher_ShouldThrowObjectDisposedException_WhenExchangeIsDisposed()
    {
        var sut = new EventExchange();
        sut.Dispose();

        var action = Lambda.Of( () => sut.RegisterPublisher<int>() );

        action.Should().ThrowExactly<ObjectDisposedException>();
    }

    [Fact]
    public void RegisterPublisher_ShouldThrowEventPublisherAlreadyExistsException_WhenPublisherForTheEventTypeAlreadyExists()
    {
        var sut = new EventExchange();
        sut.RegisterPublisher<int>();

        var action = Lambda.Of( () => sut.RegisterPublisher<int>() );

        action.Should().ThrowExactly<EventPublisherAlreadyExistsException>().AndMatch( e => e.EventType == typeof( int ) );
    }

    [Fact]
    public void Dispose_ShouldRemoveAndDisposeAllOwnedPublishers()
    {
        var sut = new EventExchange();
        var intPublisher = sut.RegisterPublisher<int>();
        var stringPublisher = sut.RegisterPublisher<string>();
        var guidPublisher = sut.RegisterPublisher<Guid>();

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeTrue();
            sut.GetRegisteredEventTypes().Should().BeEmpty();
            intPublisher.IsDisposed.Should().BeTrue();
            stringPublisher.IsDisposed.Should().BeTrue();
            guidPublisher.IsDisposed.Should().BeTrue();
        }
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenExchangeIsAlreadyDisposed()
    {
        var sut = new EventExchange();
        sut.Dispose();

        sut.Dispose();

        using ( new AssertionScope() )
        {
            sut.IsDisposed.Should().BeTrue();
            sut.GetRegisteredEventTypes().Should().BeEmpty();
        }
    }

    [Fact]
    public void PublisherDispose_ShouldRemoveThePublisherFromExchange()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        publisher.Dispose();

        sut.GetRegisteredEventTypes().Should().BeEmpty();
    }

    [Fact]
    public void PublisherDisposalSubscriberDispose_ShouldThrowInvalidEventPublisherDisposalException()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();
        var subscriber = publisher.Subscribers.First();

        var action = Lambda.Of( () => subscriber.Dispose() );

        action.Should().ThrowExactly<InvalidEventPublisherDisposalException>();
    }

    [Fact]
    public void IsRegistered_ShouldReturnTrue_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        sut.RegisterPublisher<int>();

        var result = sut.IsRegistered<int>();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsRegistered_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var result = sut.IsRegistered<int>();
        result.Should().BeFalse();
    }

    [Fact]
    public void GetStream_Generic_ShouldReturnEventStream_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.GetStream<int>();

        result.Should().BeSameAs( publisher );
    }

    [Fact]
    public void GetStream_Generic_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var action = Lambda.Of( () => sut.GetStream<int>() );
        action.Should().ThrowExactly<EventPublisherNotFoundException>().AndMatch( e => e.EventType == typeof( int ) );
    }

    [Fact]
    public void TryGetStream_Generic_ShouldReturnTrueAndEventStream_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.TryGetStream<int>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( publisher );
        }
    }

    [Fact]
    public void TryGetStream_Generic_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();

        var result = sut.TryGetStream<int>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void GetPublisher_Generic_ShouldReturnEventPublisher_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.GetPublisher<int>();

        result.Should().BeSameAs( publisher );
    }

    [Fact]
    public void GetPublisher_Generic_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var action = Lambda.Of( () => sut.GetPublisher<int>() );
        action.Should().ThrowExactly<EventPublisherNotFoundException>().AndMatch( e => e.EventType == typeof( int ) );
    }

    [Fact]
    public void TryGetPublisher_Generic_ShouldReturnTrueAndEventPublisher_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.TryGetPublisher<int>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( publisher );
        }
    }

    [Fact]
    public void TryGetPublisher_Generic_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();

        var result = sut.TryGetPublisher<int>( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void GetStream_ShouldReturnEventStream_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.GetStream( typeof( int ) );

        result.Should().BeSameAs( publisher );
    }

    [Fact]
    public void GetStream_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var action = Lambda.Of( () => sut.GetStream( typeof( int ) ) );
        action.Should().ThrowExactly<EventPublisherNotFoundException>().AndMatch( e => e.EventType == typeof( int ) );
    }

    [Fact]
    public void TryGetStream_ShouldReturnTrueAndEventStream_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.TryGetStream( typeof( int ), out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( publisher );
        }
    }

    [Fact]
    public void TryGetStream_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();

        var result = sut.TryGetStream( typeof( int ), out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void GetPublisher_ShouldReturnEventPublisher_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.GetPublisher( typeof( int ) );

        result.Should().BeSameAs( publisher );
    }

    [Fact]
    public void GetPublisher_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var action = Lambda.Of( () => sut.GetPublisher( typeof( int ) ) );
        action.Should().ThrowExactly<EventPublisherNotFoundException>().AndMatch( e => e.EventType == typeof( int ) );
    }

    [Fact]
    public void TryGetPublisher_ShouldReturnTrueAndEventPublisher_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.TryGetPublisher( typeof( int ), out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( publisher );
        }
    }

    [Fact]
    public void TryGetPublisher_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();

        var result = sut.TryGetPublisher( typeof( int ), out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Listen_Generic_ShouldAddNewListenerToCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var subscriber = Substitute.For<IEventSubscriber>();
        var listener = Substitute.For<IEventListener<int>>();
        var publisher = Substitute.For<IEventPublisher<int>>();
        publisher.Listen( listener ).Returns( subscriber );
        sut.RegisterPublisher( publisher );

        var result = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( subscriber );
            publisher.VerifyCalls().Received( x => x.Listen( listener ) );
        }
    }

    [Fact]
    public void Listen_Generic_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var listener = Substitute.For<IEventListener<int>>();

        var action = Lambda.Of( () => sut.Listen( listener ) );

        action.Should().ThrowExactly<EventPublisherNotFoundException>().AndMatch( e => e.EventType == typeof( int ) );
    }

    [Fact]
    public void TryListen_Generic_ShouldAddNewListenerToCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var subscriber = Substitute.For<IEventSubscriber>();
        var listener = Substitute.For<IEventListener<int>>();
        var publisher = Substitute.For<IEventPublisher<int>>();
        publisher.Listen( listener ).Returns( subscriber );
        sut.RegisterPublisher( publisher );

        var result = sut.TryListen( listener, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( subscriber );
            publisher.VerifyCalls().Received( x => x.Listen( listener ) );
        }
    }

    [Fact]
    public void TryListen_Generic_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var listener = Substitute.For<IEventListener<int>>();

        var result = sut.TryListen( listener, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Listen_ShouldAddNewListenerToCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var subscriber = Substitute.For<IEventSubscriber>();
        var listener = Substitute.For<IEventListener>();
        var publisher = Substitute.For<IEventPublisher<int>>();
        publisher.Listen( listener ).Returns( subscriber );
        sut.RegisterPublisher( publisher );

        var result = sut.Listen( typeof( int ), listener );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( subscriber );
            publisher.VerifyCalls().Received( x => x.Listen( listener ) );
        }
    }

    [Fact]
    public void Listen_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var listener = Substitute.For<IEventListener>();

        var action = Lambda.Of( () => sut.Listen( typeof( int ), listener ) );

        action.Should().ThrowExactly<EventPublisherNotFoundException>().AndMatch( e => e.EventType == typeof( int ) );
    }

    [Fact]
    public void TryListen_ShouldAddNewListenerToCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var subscriber = Substitute.For<IEventSubscriber>();
        var listener = Substitute.For<IEventListener>();
        var publisher = Substitute.For<IEventPublisher<int>>();
        publisher.Listen( listener ).Returns( subscriber );
        sut.RegisterPublisher( publisher );

        var result = sut.TryListen( typeof( int ), listener, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSameAs( subscriber );
            publisher.VerifyCalls().Received( x => x.Listen( listener ) );
        }
    }

    [Fact]
    public void TryListen_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var listener = Substitute.For<IEventListener>();

        var result = sut.TryListen( typeof( int ), listener, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void Publish_Generic_ShouldPublishAnEventThroughCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var @event = Fixture.CreateNotDefault<int>();
        var sut = new EventExchange();
        var publisher = Substitute.For<IEventPublisher<int>>();
        sut.RegisterPublisher( publisher );

        sut.Publish( @event );

        publisher.VerifyCalls().Received( x => x.Publish( @event ) );
    }

    [Fact]
    public void Publish_Generic_ShouldThrowEventPublisherNotFoundException_WhenEventPublisherForEventTypeDoesNotExist()
    {
        var @event = Fixture.CreateNotDefault<int>();
        var sut = new EventExchange();

        var action = Lambda.Of( () => sut.Publish( @event ) );

        action.Should().ThrowExactly<EventPublisherNotFoundException>().AndMatch( e => e.EventType == typeof( int ) );
    }

    [Fact]
    public void TryPublish_Generic_ShouldPublishAnEventThroughCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var @event = Fixture.CreateNotDefault<int>();
        var sut = new EventExchange();
        var publisher = Substitute.For<IEventPublisher<int>>();
        sut.RegisterPublisher( publisher );

        var result = sut.TryPublish( @event );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            publisher.VerifyCalls().Received( x => x.Publish( @event ) );
        }
    }

    [Fact]
    public void TryPublish_Generic_ShouldReturnFalse_WhenEventPublisherForEventTypeDoesNotExist()
    {
        var @event = Fixture.CreateNotDefault<int>();
        var sut = new EventExchange();

        var result = sut.TryPublish( @event );

        result.Should().BeFalse();
    }

    [Fact]
    public void Publish_ShouldPublishAnEventThroughCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var @event = new object();
        var sut = new EventExchange();
        var publisher = Substitute.For<IEventPublisher<int>>();
        sut.RegisterPublisher( publisher );

        sut.Publish( typeof( int ), @event );

        publisher.VerifyCalls().Received( x => x.Publish( @event ) );
    }

    [Fact]
    public void Publish_ShouldThrowEventPublisherNotFoundException_WhenEventPublisherForEventTypeDoesNotExist()
    {
        var @event = new object();
        var sut = new EventExchange();

        var action = Lambda.Of( () => sut.Publish( typeof( int ), @event ) );

        action.Should().ThrowExactly<EventPublisherNotFoundException>().AndMatch( e => e.EventType == typeof( int ) );
    }

    [Fact]
    public void TryPublish_ShouldPublishAnEventThroughCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var @event = new object();
        var sut = new EventExchange();
        var publisher = Substitute.For<IEventPublisher<int>>();
        sut.RegisterPublisher( publisher );

        var result = sut.TryPublish( typeof( int ), @event );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            publisher.VerifyCalls().Received( x => x.Publish( @event ) );
        }
    }

    [Fact]
    public void TryPublish_ShouldReturnFalse_WhenEventPublisherForEventTypeDoesNotExist()
    {
        var @event = new object();
        var sut = new EventExchange();

        var result = sut.TryPublish( typeof( int ), @event );

        result.Should().BeFalse();
    }
}
