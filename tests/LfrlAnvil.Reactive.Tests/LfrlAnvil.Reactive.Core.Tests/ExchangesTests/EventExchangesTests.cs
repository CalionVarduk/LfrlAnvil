using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Exchanges;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.ExchangesTests;

public class EventExchangesTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyExchange()
    {
        var sut = new EventExchange();

        Assertion.All( sut.IsDisposed.TestFalse(), sut.GetRegisteredEventTypes().TestEmpty() ).Go();
    }

    [Fact]
    public void RegisterPublisher_ShouldAddNewPublisher_WhenPublisherForTheSpecifiedEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var result = sut.RegisterPublisher<int>();

        Assertion.All( sut.GetRegisteredEventTypes().TestSetEqual( [ typeof( int ) ] ), result.Subscribers.Count.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public void RegisterPublisher_ShouldAddMultiplePublishers_WhenPublishersForTheSpecifiedEventTypesDoNotExist()
    {
        var sut = new EventExchange();
        var intPublisher = sut.RegisterPublisher<int>();
        var stringPublisher = sut.RegisterPublisher<string>();
        var guidPublisher = sut.RegisterPublisher<Guid>();

        Assertion.All(
                sut.GetRegisteredEventTypes().TestSetEqual( [ typeof( int ), typeof( string ), typeof( Guid ) ] ),
                intPublisher.Subscribers.Count.TestEquals( 1 ),
                stringPublisher.Subscribers.Count.TestEquals( 1 ),
                guidPublisher.Subscribers.Count.TestEquals( 1 ) )
            .Go();
    }

    [Fact]
    public void RegisterPublisher_WithExplicitPublisher_ShouldAddNewPublisher_WhenPublisherForTheSpecifiedEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var publisher = new EventPublisher<int>();
        var result = sut.RegisterPublisher( publisher );

        Assertion.All(
                sut.GetRegisteredEventTypes().TestSetEqual( [ typeof( int ) ] ),
                result.Subscribers.Count.TestEquals( 1 ),
                result.TestRefEquals( publisher ) )
            .Go();
    }

    [Fact]
    public void RegisterPublisher_ShouldThrowObjectDisposedException_WhenExchangeIsDisposed()
    {
        var sut = new EventExchange();
        sut.Dispose();

        var action = Lambda.Of( () => sut.RegisterPublisher<int>() );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void RegisterPublisher_ShouldThrowEventPublisherAlreadyExistsException_WhenPublisherForTheEventTypeAlreadyExists()
    {
        var sut = new EventExchange();
        sut.RegisterPublisher<int>();

        var action = Lambda.Of( () => sut.RegisterPublisher<int>() );

        action.Test( exc => exc.TestType().Exact<EventPublisherAlreadyExistsException>( e => e.EventType.TestEquals( typeof( int ) ) ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldRemoveAndDisposeAllOwnedPublishers()
    {
        var sut = new EventExchange();
        var intPublisher = sut.RegisterPublisher<int>();
        var stringPublisher = sut.RegisterPublisher<string>();
        var guidPublisher = sut.RegisterPublisher<Guid>();

        sut.Dispose();

        Assertion.All(
                sut.IsDisposed.TestTrue(),
                sut.GetRegisteredEventTypes().TestEmpty(),
                intPublisher.IsDisposed.TestTrue(),
                stringPublisher.IsDisposed.TestTrue(),
                guidPublisher.IsDisposed.TestTrue() )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenExchangeIsAlreadyDisposed()
    {
        var sut = new EventExchange();
        sut.Dispose();

        sut.Dispose();

        Assertion.All( sut.IsDisposed.TestTrue(), sut.GetRegisteredEventTypes().TestEmpty() ).Go();
    }

    [Fact]
    public void PublisherDispose_ShouldRemoveThePublisherFromExchange()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        publisher.Dispose();

        sut.GetRegisteredEventTypes().TestEmpty().Go();
    }

    [Fact]
    public void PublisherDisposalSubscriberDispose_ShouldThrowInvalidEventPublisherDisposalException()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();
        var subscriber = publisher.Subscribers.First();

        var action = Lambda.Of( () => subscriber.Dispose() );

        action.Test( exc => exc.TestType().Exact<InvalidEventPublisherDisposalException>() ).Go();
    }

    [Fact]
    public void IsRegistered_ShouldReturnTrue_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        sut.RegisterPublisher<int>();

        var result = sut.IsRegistered<int>();

        result.TestTrue().Go();
    }

    [Fact]
    public void IsRegistered_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var result = sut.IsRegistered<int>();
        result.TestFalse().Go();
    }

    [Fact]
    public void GetStream_Generic_ShouldReturnEventStream_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.GetStream<int>();

        result.TestRefEquals( publisher ).Go();
    }

    [Fact]
    public void GetStream_Generic_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var action = Lambda.Of( () => sut.GetStream<int>() );
        action.Test( exc => exc.TestType().Exact<EventPublisherNotFoundException>( e => e.EventType.TestEquals( typeof( int ) ) ) ).Go();
    }

    [Fact]
    public void TryGetStream_Generic_ShouldReturnTrueAndEventStream_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.TryGetStream<int>( out var outResult );

        Assertion.All( result.TestTrue(), outResult.TestRefEquals( publisher ) ).Go();
    }

    [Fact]
    public void TryGetStream_Generic_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();

        var result = sut.TryGetStream<int>( out var outResult );

        Assertion.All( result.TestFalse(), outResult.TestNull() ).Go();
    }

    [Fact]
    public void GetPublisher_Generic_ShouldReturnEventPublisher_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.GetPublisher<int>();

        result.TestRefEquals( publisher ).Go();
    }

    [Fact]
    public void GetPublisher_Generic_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var action = Lambda.Of( () => sut.GetPublisher<int>() );
        action.Test( exc => exc.TestType().Exact<EventPublisherNotFoundException>( e => e.EventType.TestEquals( typeof( int ) ) ) ).Go();
    }

    [Fact]
    public void TryGetPublisher_Generic_ShouldReturnTrueAndEventPublisher_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.TryGetPublisher<int>( out var outResult );

        Assertion.All( result.TestTrue(), outResult.TestRefEquals( publisher ) ).Go();
    }

    [Fact]
    public void TryGetPublisher_Generic_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();

        var result = sut.TryGetPublisher<int>( out var outResult );

        Assertion.All( result.TestFalse(), outResult.TestNull() ).Go();
    }

    [Fact]
    public void GetStream_ShouldReturnEventStream_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.GetStream( typeof( int ) );

        result.TestRefEquals( publisher ).Go();
    }

    [Fact]
    public void GetStream_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var action = Lambda.Of( () => sut.GetStream( typeof( int ) ) );
        action.Test( exc => exc.TestType().Exact<EventPublisherNotFoundException>( e => e.EventType.TestEquals( typeof( int ) ) ) ).Go();
    }

    [Fact]
    public void TryGetStream_ShouldReturnTrueAndEventStream_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.TryGetStream( typeof( int ), out var outResult );

        Assertion.All( result.TestTrue(), outResult.TestRefEquals( publisher ) ).Go();
    }

    [Fact]
    public void TryGetStream_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();

        var result = sut.TryGetStream( typeof( int ), out var outResult );

        Assertion.All( result.TestFalse(), outResult.TestNull() ).Go();
    }

    [Fact]
    public void GetPublisher_ShouldReturnEventPublisher_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.GetPublisher( typeof( int ) );

        result.TestRefEquals( publisher ).Go();
    }

    [Fact]
    public void GetPublisher_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var action = Lambda.Of( () => sut.GetPublisher( typeof( int ) ) );
        action.Test( exc => exc.TestType().Exact<EventPublisherNotFoundException>( e => e.EventType.TestEquals( typeof( int ) ) ) ).Go();
    }

    [Fact]
    public void TryGetPublisher_ShouldReturnTrueAndEventPublisher_WhenPublisherForEventTypeExists()
    {
        var sut = new EventExchange();
        var publisher = sut.RegisterPublisher<int>();

        var result = sut.TryGetPublisher( typeof( int ), out var outResult );

        Assertion.All( result.TestTrue(), outResult.TestRefEquals( publisher ) ).Go();
    }

    [Fact]
    public void TryGetPublisher_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();

        var result = sut.TryGetPublisher( typeof( int ), out var outResult );

        Assertion.All( result.TestFalse(), outResult.TestNull() ).Go();
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

        Assertion.All( result.TestRefEquals( subscriber ), publisher.TestReceivedCall( x => x.Listen( listener ) ) ).Go();
    }

    [Fact]
    public void Listen_Generic_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var listener = Substitute.For<IEventListener<int>>();

        var action = Lambda.Of( () => sut.Listen( listener ) );

        action.Test( exc => exc.TestType().Exact<EventPublisherNotFoundException>( e => e.EventType.TestEquals( typeof( int ) ) ) ).Go();
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

        Assertion.All( result.TestTrue(), outResult.TestRefEquals( subscriber ), publisher.TestReceivedCall( x => x.Listen( listener ) ) )
            .Go();
    }

    [Fact]
    public void TryListen_Generic_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var listener = Substitute.For<IEventListener<int>>();

        var result = sut.TryListen( listener, out var outResult );

        Assertion.All( result.TestFalse(), outResult.TestNull() ).Go();
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

        Assertion.All( result.TestRefEquals( subscriber ), publisher.TestReceivedCall( x => x.Listen( listener ) ) ).Go();
    }

    [Fact]
    public void Listen_ShouldThrowEventPublisherNotFoundException_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var listener = Substitute.For<IEventListener>();

        var action = Lambda.Of( () => sut.Listen( typeof( int ), listener ) );

        action.Test( exc => exc.TestType().Exact<EventPublisherNotFoundException>( e => e.EventType.TestEquals( typeof( int ) ) ) ).Go();
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

        Assertion.All( result.TestTrue(), outResult.TestRefEquals( subscriber ), publisher.TestReceivedCall( x => x.Listen( listener ) ) )
            .Go();
    }

    [Fact]
    public void TryListen_ShouldReturnFalse_WhenPublisherForEventTypeDoesNotExist()
    {
        var sut = new EventExchange();
        var listener = Substitute.For<IEventListener>();

        var result = sut.TryListen( typeof( int ), listener, out var outResult );

        Assertion.All( result.TestFalse(), outResult.TestNull() ).Go();
    }

    [Fact]
    public void Publish_Generic_ShouldPublishAnEventThroughCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var @event = Fixture.CreateNotDefault<int>();
        var sut = new EventExchange();
        var publisher = Substitute.For<IEventPublisher<int>>();
        sut.RegisterPublisher( publisher );

        sut.Publish( @event );

        publisher.TestReceivedCall( x => x.Publish( @event ) ).Go();
    }

    [Fact]
    public void Publish_Generic_ShouldThrowEventPublisherNotFoundException_WhenEventPublisherForEventTypeDoesNotExist()
    {
        var @event = Fixture.CreateNotDefault<int>();
        var sut = new EventExchange();

        var action = Lambda.Of( () => sut.Publish( @event ) );

        action.Test( exc => exc.TestType().Exact<EventPublisherNotFoundException>( e => e.EventType.TestEquals( typeof( int ) ) ) ).Go();
    }

    [Fact]
    public void TryPublish_Generic_ShouldPublishAnEventThroughCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var @event = Fixture.CreateNotDefault<int>();
        var sut = new EventExchange();
        var publisher = Substitute.For<IEventPublisher<int>>();
        sut.RegisterPublisher( publisher );

        var result = sut.TryPublish( @event );

        Assertion.All( result.TestTrue(), publisher.TestReceivedCall( x => x.Publish( @event ) ) ).Go();
    }

    [Fact]
    public void TryPublish_Generic_ShouldReturnFalse_WhenEventPublisherForEventTypeDoesNotExist()
    {
        var @event = Fixture.CreateNotDefault<int>();
        var sut = new EventExchange();

        var result = sut.TryPublish( @event );

        result.TestFalse().Go();
    }

    [Fact]
    public void Publish_ShouldPublishAnEventThroughCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var @event = new object();
        var sut = new EventExchange();
        var publisher = Substitute.For<IEventPublisher<int>>();
        sut.RegisterPublisher( publisher );

        sut.Publish( typeof( int ), @event );

        publisher.TestReceivedCall( x => x.Publish( @event ) ).Go();
    }

    [Fact]
    public void Publish_ShouldThrowEventPublisherNotFoundException_WhenEventPublisherForEventTypeDoesNotExist()
    {
        var @event = new object();
        var sut = new EventExchange();

        var action = Lambda.Of( () => sut.Publish( typeof( int ), @event ) );

        action.Test( exc => exc.TestType().Exact<EventPublisherNotFoundException>( e => e.EventType.TestEquals( typeof( int ) ) ) ).Go();
    }

    [Fact]
    public void TryPublish_ShouldPublishAnEventThroughCorrectEventPublisher_WhenEventPublisherForEventTypeExists()
    {
        var @event = new object();
        var sut = new EventExchange();
        var publisher = Substitute.For<IEventPublisher<int>>();
        sut.RegisterPublisher( publisher );

        var result = sut.TryPublish( typeof( int ), @event );

        Assertion.All( result.TestTrue(), publisher.TestReceivedCall( x => x.Publish( @event ) ) ).Go();
    }

    [Fact]
    public void TryPublish_ShouldReturnFalse_WhenEventPublisherForEventTypeDoesNotExist()
    {
        var @event = new object();
        var sut = new EventExchange();

        var result = sut.TryPublish( typeof( int ), @event );

        result.TestFalse().Go();
    }
}
