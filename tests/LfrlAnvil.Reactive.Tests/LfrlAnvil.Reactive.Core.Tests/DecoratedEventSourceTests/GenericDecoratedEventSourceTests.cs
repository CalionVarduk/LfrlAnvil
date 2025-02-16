using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Tests.DecoratedEventSourceTests;

public abstract class GenericDecoratedEventSourceTests<TRootEvent, TNextEvent, TLastEvent> : TestsBase
{
    [Fact]
    public void RootEventSourceDecorate_ShouldReturnNewNotDisposedEventStream()
    {
        var listener = Substitute.For<IEventListener<TRootEvent>>();
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        var sut = new EventPublisher<TRootEvent>();
        var subscriber = sut.Listen( listener );

        var result = sut.Decorate( decorator );

        Assertion.All(
                sut.Subscribers.TestSequence( [ subscriber ] ),
                result.TestNotRefEquals( sut ),
                result.IsDisposed.TestFalse() )
            .Go();
    }

    [Fact]
    public void RootEventSourceDecorate_ShouldReturnNewDisposedEventStream_WhenDisposed()
    {
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        var sut = new EventPublisher<TRootEvent>();
        sut.Dispose();

        var result = sut.Decorate( decorator );

        Assertion.All(
                sut.Subscribers.TestEmpty(),
                result.TestNotRefEquals( sut ),
                result.IsDisposed.TestTrue() )
            .Go();
    }

    [Fact]
    public void Listen_ShouldAddNewRootSubscriber_WhenNotDisposed()
    {
        var listener = Substitute.For<IEventListener<TNextEvent>>();
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        decorator.Decorate( listener, Arg.Any<IEventSubscriber>() ).Returns( _ => Substitute.For<IEventListener<TRootEvent>>() );

        var sut = new EventPublisher<TRootEvent>();
        var result = sut.Decorate( decorator );

        var subscriber = result.Listen( listener );

        sut.Subscribers.TestSequence( [ subscriber ] ).Go();
    }

    [Fact]
    public void Listen_ShouldReturnDisposedRootSubscriber_WhenDisposed()
    {
        var listener = Substitute.For<IEventListener<TNextEvent>>();
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        decorator.Decorate( listener, Arg.Any<IEventSubscriber>() ).Returns( _ => Substitute.For<IEventListener<TRootEvent>>() );

        var sut = new EventPublisher<TRootEvent>();
        var result = sut.Decorate( decorator );
        sut.Dispose();

        var subscriber = result.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                sut.Subscribers.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Listen_ShouldReturnDisposedSubscriber_WhenDecoratorDisposesItBeforeRegistration()
    {
        var listener = Substitute.For<IEventListener<TNextEvent>>();
        var rootListener = Substitute.For<IEventListener<TRootEvent>>();
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        decorator.Decorate( listener, Arg.Any<IEventSubscriber>() )
            .Returns(
                c =>
                {
                    c.ArgAt<IEventSubscriber>( 1 ).Dispose();
                    return rootListener;
                } );

        var sut = new EventPublisher<TRootEvent>();
        var result = sut.Decorate( decorator );

        var subscriber = result.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                sut.Subscribers.TestEmpty(),
                rootListener.TestReceivedCall( x => x.OnDispose( DisposalSource.Subscriber ) ) )
            .Go();
    }

    [Fact]
    public void NestedEventStreamDecorate_ShouldReturnNewNotDisposedEventStream()
    {
        var listener = Substitute.For<IEventListener<TRootEvent>>();
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        var nestedDecorator = Substitute.For<IEventListenerDecorator<TNextEvent, TLastEvent>>();
        var sut = new EventPublisher<TRootEvent>();
        var subscriber = sut.Listen( listener );

        var result = sut.Decorate( decorator ).Decorate( nestedDecorator );

        Assertion.All(
                sut.Subscribers.TestSequence( [ subscriber ] ),
                result.TestNotRefEquals( sut ),
                result.IsDisposed.TestFalse() )
            .Go();
    }

    [Fact]
    public void NestedEventStreamDecorate_ShouldReturnNewDisposedEventStream_WhenDisposed()
    {
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        var nestedDecorator = Substitute.For<IEventListenerDecorator<TNextEvent, TLastEvent>>();
        var sut = new EventPublisher<TRootEvent>();
        sut.Dispose();

        var result = sut.Decorate( decorator ).Decorate( nestedDecorator );

        Assertion.All(
                sut.Subscribers.TestEmpty(),
                result.TestNotRefEquals( sut ),
                result.IsDisposed.TestTrue() )
            .Go();
    }

    [Fact]
    public void NestedListen_ShouldAddNewRootSubscriber_WhenNotDisposed()
    {
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        decorator.Decorate( Arg.Any<IEventListener<TNextEvent>>(), Arg.Any<IEventSubscriber>() )
            .Returns( _ => Substitute.For<IEventListener<TRootEvent>>() );

        var listener = Substitute.For<IEventListener<TLastEvent>>();
        var nestedDecorator = Substitute.For<IEventListenerDecorator<TNextEvent, TLastEvent>>();
        nestedDecorator.Decorate( listener, Arg.Any<IEventSubscriber>() ).Returns( _ => Substitute.For<IEventListener<TNextEvent>>() );

        var sut = new EventPublisher<TRootEvent>();
        var result = sut.Decorate( decorator ).Decorate( nestedDecorator );

        var subscriber = result.Listen( listener );

        sut.Subscribers.TestSequence( [ subscriber ] ).Go();
    }

    [Fact]
    public void NestedListen_ShouldReturnDisposedRootSubscriber_WhenDisposed()
    {
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        decorator.Decorate( Arg.Any<IEventListener<TNextEvent>>(), Arg.Any<IEventSubscriber>() )
            .Returns( _ => Substitute.For<IEventListener<TRootEvent>>() );

        var listener = Substitute.For<IEventListener<TLastEvent>>();
        var nestedDecorator = Substitute.For<IEventListenerDecorator<TNextEvent, TLastEvent>>();
        nestedDecorator.Decorate( listener, Arg.Any<IEventSubscriber>() ).Returns( _ => Substitute.For<IEventListener<TNextEvent>>() );

        var sut = new EventPublisher<TRootEvent>();
        var result = sut.Decorate( decorator ).Decorate( nestedDecorator );
        sut.Dispose();

        var subscriber = result.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                sut.Subscribers.TestEmpty() )
            .Go();
    }

    [Fact]
    public void NestedListen_ShouldReturnDisposedSubscriber_WhenDecoratorDisposesItBeforeRegistration()
    {
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        decorator.Decorate( Arg.Any<IEventListener<TNextEvent>>(), Arg.Any<IEventSubscriber>() )
            .Returns( _ => Substitute.For<IEventListener<TRootEvent>>() );

        var listener = Substitute.For<IEventListener<TLastEvent>>();
        var nestedDecorator = Substitute.For<IEventListenerDecorator<TNextEvent, TLastEvent>>();
        nestedDecorator.Decorate( listener, Arg.Any<IEventSubscriber>() )
            .Returns(
                c =>
                {
                    c.ArgAt<IEventSubscriber>( 1 ).Dispose();
                    return Substitute.For<IEventListener<TNextEvent>>();
                } );

        var sut = new EventPublisher<TRootEvent>();
        var result = sut.Decorate( decorator ).Decorate( nestedDecorator );

        var subscriber = result.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                sut.Subscribers.TestEmpty() )
            .Go();
    }

    [Fact]
    public void DeeplyNestedEventStreamDecorate_ShouldReturnNewNotDisposedEventStream()
    {
        var listener = Substitute.For<IEventListener<TRootEvent>>();
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        var nestedDecorator = Substitute.For<IEventListenerDecorator<TNextEvent, TLastEvent>>();
        var deeplyNestedDecorator = Substitute.For<IEventListenerDecorator<TLastEvent, TRootEvent>>();
        var sut = new EventPublisher<TRootEvent>();
        var subscriber = sut.Listen( listener );

        var result = sut.Decorate( decorator ).Decorate( nestedDecorator ).Decorate( deeplyNestedDecorator );

        Assertion.All(
                sut.Subscribers.TestSequence( [ subscriber ] ),
                result.TestNotRefEquals( sut ),
                result.IsDisposed.TestFalse() )
            .Go();
    }

    [Fact]
    public void DeeplyNestedEventStreamDecorate_ShouldReturnNewDisposedEventStream_WhenDisposed()
    {
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        var nestedDecorator = Substitute.For<IEventListenerDecorator<TNextEvent, TLastEvent>>();
        var deeplyNestedDecorator = Substitute.For<IEventListenerDecorator<TLastEvent, TRootEvent>>();
        var sut = new EventPublisher<TRootEvent>();
        sut.Dispose();

        var result = sut.Decorate( decorator ).Decorate( nestedDecorator ).Decorate( deeplyNestedDecorator );

        Assertion.All(
                sut.Subscribers.TestEmpty(),
                result.TestNotRefEquals( sut ),
                result.IsDisposed.TestTrue() )
            .Go();
    }

    [Fact]
    public void IEventStreamListen_ShouldBeEquivalentToGenericListen_WhenListenerIsOfCorrectType()
    {
        var listener = Substitute.For<IEventListener<TNextEvent>>();
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        decorator.Decorate( listener, Arg.Any<IEventSubscriber>() ).Returns( _ => Substitute.For<IEventListener<TRootEvent>>() );

        var source = new EventPublisher<TRootEvent>();
        IEventStream sut = source.Decorate( decorator );

        var subscriber = sut.Listen( EventListener<TNextEvent>.Empty );

        source.Subscribers.TestSequence( [ subscriber ] ).Go();
    }

    [Fact]
    public void IEventStreamListen_ShouldThrowInvalidArgumentTypeException_WhenListenerIsNotOfCorrectType()
    {
        var listener = EventListener<TLastEvent>.Empty;
        var decorator = Substitute.For<IEventListenerDecorator<TRootEvent, TNextEvent>>();
        IEventStream sut = new EventPublisher<TRootEvent>().Decorate( decorator );

        var action = Lambda.Of( () => sut.Listen( listener ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<InvalidArgumentTypeException>(
                        e => Assertion.All(
                            e.Argument.TestRefEquals( listener ),
                            e.ExpectedType.TestEquals( typeof( IEventListener<TNextEvent> ) ) ) ) )
            .Go();
    }
}
