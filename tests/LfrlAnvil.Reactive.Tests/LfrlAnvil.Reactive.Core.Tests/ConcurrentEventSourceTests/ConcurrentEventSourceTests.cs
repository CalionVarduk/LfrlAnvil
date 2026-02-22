using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Tests.ConcurrentEventSourceTests;

public class ConcurrentEventSourceTests : TestsBase
{
    [Fact]
    public void Listen_ShouldCallBaseEventSourceListen()
    {
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var listener = Substitute.For<IEventListener<int>>();

        var subscriber = sut.Listen( listener );

        Assertion.All(
                @base.Subscribers.TestSequence( [ subscriber ] ),
                sut.Subscribers.TestSequence( @base.Subscribers ),
                sut.HasSubscribers.TestEquals( @base.HasSubscribers ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldAcquireLock()
    {
        var @base = new HistoryEventPublisher<int>( capacity: 1 );
        @base.Publish( Fixture.Create<int>() );

        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var sync = sut.Sync;
        var hasLock = false;

        var listener = EventListener.Create<int>( _ => hasLock = Monitor.IsEntered( sync ) );

        sut.Listen( listener );

        hasLock.TestTrue().Go();
    }

    [Fact]
    public void EventSubscriberDispose_ShouldRemoveSubscriberFromBaseEventSource()
    {
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var listener = Substitute.For<IEventListener<int>>();
        var subscriber = sut.Listen( listener );

        subscriber.Dispose();

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                @base.Subscribers.TestEmpty(),
                sut.Subscribers.TestEmpty(),
                sut.HasSubscribers.TestFalse() )
            .Go();
    }

    [Fact]
    public async Task EventSubscriberDispose_ShouldAcquireLock()
    {
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var sync = sut.Sync;
        var listener = Substitute.For<IEventListener<int>>();
        var subscriber = sut.Listen( listener );

        var action = Lambda.Of( () => subscriber.Dispose() );

        (await action.TestLockAcquisitionAsync( sync )).Go();
    }

    [Fact]
    public void Dispose_ShouldCallBaseEventSourceDispose()
    {
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var listener = Substitute.For<IEventListener<int>>();
        sut.Listen( listener );

        sut.Dispose();

        Assertion.All(
                @base.IsDisposed.TestTrue(),
                sut.IsDisposed.TestTrue() )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldAcquireLock()
    {
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var sync = sut.Sync;
        var hasLock = false;

        var listener = EventListener.Create<int>( _ => { }, _ => hasLock = Monitor.IsEntered( sync ) );
        sut.Listen( listener );

        sut.Dispose();

        hasLock.TestTrue().Go();
    }

    [Fact]
    public void SubscribersEnumerator_ShouldAcquireLock()
    {
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var sync = sut.Sync;

        using var result = sut.Subscribers.GetEnumerator();
        var hasLock = Monitor.IsEntered( sync );

        hasLock.TestTrue().Go();
    }

    [Fact]
    public void SubscribersEnumeratorDispose_ShouldReleaseLock()
    {
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var sync = sut.Sync;

        var result = sut.Subscribers.GetEnumerator();
        result.Dispose();
        var hasLock = Monitor.IsEntered( sync );

        hasLock.TestFalse().Go();
    }

    [Fact]
    public void IEventStreamListen_ShouldBeEquivalentToGenericListen_WhenListenerIsOfCorrectType()
    {
        var @base = new EventPublisher<int>();
        var source = new ConcurrentEventSource<int, EventSource<int>>( @base );
        IEventStream sut = source;
        var listener = Substitute.For<IEventListener<int>>();

        var subscriber = sut.Listen( listener );

        Assertion.All(
                source.HasSubscribers.TestTrue(),
                source.Subscribers.TestSequence( [ subscriber ] ) )
            .Go();
    }

    [Fact]
    public void IEventStreamListen_ShouldThrowInvalidArgumentTypeException_WhenListenerIsNotOfCorrectType()
    {
        var listener = Substitute.For<IEventListener<int[]>>();
        var @base = new EventPublisher<int>();
        IEventStream sut = new ConcurrentEventSource<int, EventSource<int>>( @base );

        var action = Lambda.Of( () => sut.Listen( listener ) );

        action.Test( exc => exc.TestType()
                .Exact<InvalidArgumentTypeException>( e => Assertion.All(
                    e.Argument.TestRefEquals( listener ),
                    e.ExpectedType.TestEquals( typeof( IEventListener<int> ) ) ) ) )
            .Go();
    }

    [Fact]
    public void ToConcurrentExtension_ShouldCreateConcurrentWrapperForEventSource()
    {
        EventSource<int> @base = new EventPublisher<int>();
        var sut = @base.ToConcurrent();
        sut.TestType().AssignableTo<ConcurrentEventSource<int, EventSource<int>>>().Go();
    }
}
