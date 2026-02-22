using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Tests.EventPublisherTests;

public abstract class GenericEventPublisherTests<TEvent> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceInDefaultState()
    {
        var sut = new EventPublisher<TEvent>();

        Assertion.All( sut.IsDisposed.TestFalse(), sut.HasSubscribers.TestFalse(), sut.Subscribers.TestEmpty() ).Go();
    }

    [Fact]
    public void Listen_ShouldAddNewSubscriber_WhenNotDisposed()
    {
        var sut = new EventPublisher<TEvent>();

        var subscriber = sut.Listen( EventListener<TEvent>.Empty );

        Assertion.All( sut.HasSubscribers.TestTrue(), sut.Subscribers.TestSequence( [ subscriber ] ) ).Go();
    }

    [Fact]
    public void Listen_ShouldReturnDisposedSubscriber_WhenDisposed()
    {
        var sut = new EventPublisher<TEvent>();
        sut.Dispose();

        var subscriber = sut.Listen( EventListener<TEvent>.Empty );

        Assertion.All( subscriber.IsDisposed.TestTrue(), sut.HasSubscribers.TestFalse(), sut.Subscribers.TestEmpty() ).Go();
    }

    [Fact]
    public void Listen_ShouldCallListenerOnDispose_WhenDisposed()
    {
        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new EventPublisher<TEvent>();
        sut.Dispose();

        sut.Listen( listener );

        listener.TestReceivedCall( x => x.OnDispose( DisposalSource.EventSource ) ).Go();
    }

    [Fact]
    public void Listen_ShouldAddAnotherSubscriber_WhenAlreadyContainsOne()
    {
        var sut = new EventPublisher<TEvent>();
        var subscriber1 = sut.Listen( EventListener<TEvent>.Empty );

        var subscriber2 = sut.Listen( EventListener<TEvent>.Empty );

        Assertion.All( sut.HasSubscribers.TestTrue(), sut.Subscribers.TestSequence( [ subscriber1, subscriber2 ] ) ).Go();
    }

    [Fact]
    public void EventSubscriberDispose_ShouldRemoveSubscriber()
    {
        var sut = new EventPublisher<TEvent>();
        var subscriber = sut.Listen( EventListener<TEvent>.Empty );

        subscriber.Dispose();

        Assertion.All( subscriber.IsDisposed.TestTrue(), sut.HasSubscribers.TestFalse(), sut.Subscribers.TestEmpty() ).Go();
    }

    [Fact]
    public void EventSubscriberDispose_ShouldCallListenerOnDispose()
    {
        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new EventPublisher<TEvent>();
        var subscriber = sut.Listen( listener );

        subscriber.Dispose();

        listener.TestReceivedCall( x => x.OnDispose( DisposalSource.Subscriber ) ).Go();
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

        listener.TestDidNotReceiveCall( x => x.OnDispose( Arg.Any<DisposalSource>() ) ).Go();
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

        Assertion.All( listener1.TestReceivedCall( x => x.React( @event ) ), listener2.TestReceivedCall( x => x.React( @event ) ) ).Go();
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

        Assertion.All(
                sut.Subscribers.TestSequence( [ subscriber1 ] ),
                subscriber2.IsDisposed.TestTrue(),
                listener2.TestDidNotReceiveCall( x => x.React( @event ) ) )
            .Go();
    }

    [Fact]
    public void Publish_ShouldListenerCorrectAmountOfTimes_WhenListenerCausesThePublisherToPublish()
    {
        var (firstEvent, secondEvent) = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var sut = new EventPublisher<TEvent>();
        var listener1 = EventListener.Create( (TEvent e) =>
        {
            if ( e!.Equals( firstEvent ) ) sut.Publish( secondEvent );
        } );

        var listener2 = Substitute.For<IEventListener<TEvent>>();
        sut.Listen( listener1 );
        sut.Listen( listener2 );

        sut.Publish( firstEvent );

        Assertion.All(
                listener2.TestReceivedCall( x => x.React( firstEvent ) ),
                listener2.TestReceivedCall( x => x.React( secondEvent ) ) )
            .Go();
    }

    [Fact]
    public void Publish_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new EventPublisher<TEvent>();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Publish( @event ) );

        action.Test( exc => exc.TestType().Exact<ObjectDisposedException>() ).Go();
    }

    [Fact]
    public void Dispose_ShouldDisposeAllSubscribers()
    {
        var sut = new EventPublisher<TEvent>();
        var subscriber1 = sut.Listen( EventListener<TEvent>.Empty );
        var subscriber2 = sut.Listen( EventListener<TEvent>.Empty );

        sut.Dispose();

        Assertion.All(
                sut.IsDisposed.TestTrue(),
                sut.Subscribers.TestEmpty(),
                subscriber1.IsDisposed.TestTrue(),
                subscriber2.IsDisposed.TestTrue() )
            .Go();
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

        Assertion.All(
                listener1.TestReceivedCall( x => x.OnDispose( DisposalSource.EventSource ) ),
                listener2.TestReceivedCall( x => x.OnDispose( DisposalSource.EventSource ) ) )
            .Go();
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

        Assertion.All(
                listener2.TestDidNotReceiveCall( x => x.OnDispose( DisposalSource.EventSource ) ),
                listener2.TestReceivedCall( x => x.OnDispose( DisposalSource.Subscriber ) ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldDoNothing_WhenAlreadyDisposed()
    {
        var sut = new EventPublisher<TEvent>();
        sut.Dispose();

        var action = Lambda.Of( () => sut.Dispose() );

        action.Test( exc => exc.TestNull() ).Go();
    }

    [Fact]
    public void IEventStreamListen_ShouldBeEquivalentToGenericListen_WhenListenerIsOfCorrectType()
    {
        var source = new EventPublisher<TEvent>();
        IEventStream sut = source;

        var subscriber = sut.Listen( EventListener<TEvent>.Empty );

        Assertion.All( source.HasSubscribers.TestTrue(), source.Subscribers.TestSequence( [ subscriber ] ) ).Go();
    }

    [Fact]
    public void IEventStreamListen_ShouldThrowInvalidArgumentTypeException_WhenListenerIsNotOfCorrectType()
    {
        var listener = EventListener<TEvent[]>.Empty;
        IEventStream sut = new EventPublisher<TEvent>();

        var action = Lambda.Of( () => sut.Listen( listener ) );

        action.Test( exc => exc.TestType()
                .Exact<InvalidArgumentTypeException>( e => Assertion.All(
                    e.Argument.TestRefEquals( listener ),
                    e.ExpectedType.TestEquals( typeof( IEventListener<TEvent> ) ) ) ) )
            .Go();
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

        listener.TestReceivedCall( x => x.React( @event ) ).Go();
    }

    [Fact]
    public void IEventPublisherPublish_ShouldThrowInvalidArgumentTypeException_WhenEventIsNotOfCorrectType()
    {
        var @event = Fixture.CreateMany<TEvent>().ToArray();
        IEventPublisher sut = new EventPublisher<TEvent>();

        var action = Lambda.Of( () => sut.Publish( @event ) );

        action.Test( exc => exc.TestType()
                .Exact<InvalidArgumentTypeException>( e => Assertion.All(
                    e.Argument.TestRefEquals( @event ),
                    e.ExpectedType.TestEquals( typeof( TEvent ) ) ) ) )
            .Go();
    }
}
