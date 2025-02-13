using System.Threading;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Tests.ConcurrentDecoratedEventSourceTests;

public class ConcurrentDecoratedEventSourceTests : TestsBase
{
    [Fact]
    public void RootEventSourceDecorate_ShouldCallBaseEventSourceDecorate()
    {
        var decorator = Substitute.For<IEventListenerDecorator<int, string>>();
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );

        var result = sut.Decorate( decorator );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.IsDisposed.TestEquals( @base.IsDisposed ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCallBaseEventSourceListenWithDecoratedListener()
    {
        var listener = Substitute.For<IEventListener<string>>();
        var decorator = Substitute.For<IEventListenerDecorator<int, string>>();
        decorator.Decorate( listener, Arg.Any<IEventSubscriber>() ).Returns( _ => Substitute.For<IEventListener<int>>() );

        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var result = sut.Decorate( decorator );

        var subscriber = result.Listen( listener );

        sut.Subscribers.TestSequence( [ subscriber ] ).Go();
    }

    [Fact]
    public void Listen_ShouldAcquireLock()
    {
        var listener = Substitute.For<IEventListener<string>>();
        var decorator = Substitute.For<IEventListenerDecorator<int, string>>();
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var result = sut.Decorate( decorator );
        var sync = sut.Sync;
        var hasLock = false;

        decorator.Decorate( listener, Arg.Any<IEventSubscriber>() )
            .Returns(
                _ =>
                {
                    hasLock = Monitor.IsEntered( sync );
                    return Substitute.For<IEventListener<int>>();
                } );

        result.Listen( listener );

        hasLock.TestTrue().Go();
    }

    [Fact]
    public void NestedEventSourceDecorate_ShouldCallBaseEventSourceDecorate()
    {
        var decorator = Substitute.For<IEventListenerDecorator<int, string>>();
        var nestedDecorator = Substitute.For<IEventListenerDecorator<string, Guid>>();
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );

        var result = sut.Decorate( decorator ).Decorate( nestedDecorator );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.IsDisposed.TestEquals( @base.IsDisposed ) )
            .Go();
    }

    [Fact]
    public void NestedListen_ShouldCallBaseEventSourceListenWithDecoratedListener()
    {
        var listener = Substitute.For<IEventListener<Guid>>();
        var decorator = Substitute.For<IEventListenerDecorator<int, string>>();
        decorator.Decorate( Arg.Any<IEventListener<string>>(), Arg.Any<IEventSubscriber>() )
            .Returns( _ => Substitute.For<IEventListener<int>>() );

        var nestedDecorator = Substitute.For<IEventListenerDecorator<string, Guid>>();
        nestedDecorator.Decorate( listener, Arg.Any<IEventSubscriber>() ).Returns( _ => Substitute.For<IEventListener<string>>() );

        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var result = sut.Decorate( decorator ).Decorate( nestedDecorator );

        var subscriber = result.Listen( listener );

        sut.Subscribers.TestSequence( [ subscriber ] ).Go();
    }

    [Fact]
    public void NestedListen_ShouldAcquireLock()
    {
        var listener = Substitute.For<IEventListener<Guid>>();
        var decorator = Substitute.For<IEventListenerDecorator<int, string>>();
        var nestedDecorator = Substitute.For<IEventListenerDecorator<string, Guid>>();
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var result = sut.Decorate( decorator ).Decorate( nestedDecorator );
        var sync = sut.Sync;
        var hasLock = false;

        decorator.Decorate( Arg.Any<IEventListener<string>>(), Arg.Any<IEventSubscriber>() )
            .Returns(
                _ =>
                {
                    hasLock = Monitor.IsEntered( sync );
                    return Substitute.For<IEventListener<int>>();
                } );

        nestedDecorator.Decorate( listener, Arg.Any<IEventSubscriber>() ).Returns( _ => Substitute.For<IEventListener<string>>() );

        result.Listen( listener );

        hasLock.TestTrue().Go();
    }

    [Fact]
    public void DeeplyNestedEventSourceDecorate_ShouldCallBaseEventSourceDecorate()
    {
        var decorator = Substitute.For<IEventListenerDecorator<int, string>>();
        var nestedDecorator = Substitute.For<IEventListenerDecorator<string, Guid>>();
        var deeplyNestedDecorator = Substitute.For<IEventListenerDecorator<Guid, int>>();
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );

        var result = sut.Decorate( decorator ).Decorate( nestedDecorator ).Decorate( deeplyNestedDecorator );

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.IsDisposed.TestEquals( @base.IsDisposed ) )
            .Go();
    }

    [Fact]
    public void IEventStreamListen_ShouldBeEquivalentToGenericListen_WhenListenerIsOfCorrectType()
    {
        var listener = Substitute.For<IEventListener<string>>();
        var decorator = Substitute.For<IEventListenerDecorator<int, string>>();
        decorator.Decorate( listener, Arg.Any<IEventSubscriber>() ).Returns( _ => Substitute.For<IEventListener<int>>() );

        var @base = new EventPublisher<int>();
        var source = new ConcurrentEventSource<int, EventSource<int>>( @base );
        IEventStream sut = source.Decorate( decorator );

        var subscriber = sut.Listen( listener );

        Assertion.All(
                source.HasSubscribers.TestTrue(),
                source.Subscribers.TestSequence( [ subscriber ] ) )
            .Go();
    }

    [Fact]
    public void IEventStreamListen_ShouldThrowInvalidArgumentTypeException_WhenListenerIsNotOfCorrectType()
    {
        var listener = Substitute.For<IEventListener<string[]>>();
        var decorator = Substitute.For<IEventListenerDecorator<int, string>>();
        var @base = new EventPublisher<int>();
        IEventStream sut = new ConcurrentEventSource<int, EventSource<int>>( @base ).Decorate( decorator );

        var action = Lambda.Of( () => sut.Listen( listener ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<InvalidArgumentTypeException>(
                        e => Assertion.All(
                            e.Argument.TestRefEquals( listener ),
                            e.ExpectedType.TestEquals( typeof( IEventListener<string> ) ) ) ) )
            .Go();
    }
}
