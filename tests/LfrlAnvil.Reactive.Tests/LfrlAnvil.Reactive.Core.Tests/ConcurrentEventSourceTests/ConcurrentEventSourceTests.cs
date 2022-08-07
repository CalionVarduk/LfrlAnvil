using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Extensions;
using LfrlAnvil.Reactive.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;

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

        using ( new AssertionScope() )
        {
            @base.Subscribers.Should().BeSequentiallyEqualTo( subscriber );
            sut.Subscribers.Should().BeSequentiallyEqualTo( @base.Subscribers );
            sut.HasSubscribers.Should().Be( @base.HasSubscribers );
        }
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

        hasLock.Should().BeTrue();
    }

    [Fact]
    public void EventSubscriberDispose_ShouldRemoveSubscriberFromBaseEventSource()
    {
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var listener = Substitute.For<IEventListener<int>>();
        var subscriber = sut.Listen( listener );

        subscriber.Dispose();

        using ( new AssertionScope() )
        {
            subscriber.IsDisposed.Should().BeTrue();
            @base.Subscribers.Should().BeEmpty();
            sut.Subscribers.Should().BeEmpty();
            sut.HasSubscribers.Should().BeFalse();
        }
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

        await action.Should().AcquireLockOn( sync );
    }

    [Fact]
    public void Dispose_ShouldCallBaseEventSourceDispose()
    {
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var listener = Substitute.For<IEventListener<int>>();
        sut.Listen( listener );

        sut.Dispose();

        using ( new AssertionScope() )
        {
            @base.IsDisposed.Should().BeTrue();
            sut.IsDisposed.Should().BeTrue();
        }
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

        hasLock.Should().BeTrue();
    }

    [Fact]
    public void SubscribersEnumerator_ShouldAcquireLock()
    {
        var @base = new EventPublisher<int>();
        var sut = new ConcurrentEventSource<int, EventSource<int>>( @base );
        var sync = sut.Sync;

        using var result = sut.Subscribers.GetEnumerator();
        var hasLock = Monitor.IsEntered( sync );

        hasLock.Should().BeTrue();
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

        hasLock.Should().BeFalse();
    }

    [Fact]
    public void IEventStreamListen_ShouldBeEquivalentToGenericListen_WhenListenerIsOfCorrectType()
    {
        var @base = new EventPublisher<int>();
        var source = new ConcurrentEventSource<int, EventSource<int>>( @base );
        IEventStream sut = source;
        var listener = Substitute.For<IEventListener<int>>();

        var subscriber = sut.Listen( listener );

        using ( new AssertionScope() )
        {
            source.HasSubscribers.Should().BeTrue();
            source.Subscribers.Should().BeSequentiallyEqualTo( subscriber );
        }
    }

    [Fact]
    public void IEventStreamListen_ShouldThrowInvalidArgumentTypeException_WhenListenerIsNotOfCorrectType()
    {
        var listener = Substitute.For<IEventListener<int[]>>();
        var @base = new EventPublisher<int>();
        IEventStream sut = new ConcurrentEventSource<int, EventSource<int>>( @base );

        var action = Lambda.Of( () => sut.Listen( listener ) );

        action.Should()
            .ThrowExactly<InvalidArgumentTypeException>()
            .AndMatch( e => e.Argument == listener && e.ExpectedType == typeof( IEventListener<int> ) );
    }

    [Fact]
    public void ToConcurrentExtension_ShouldCreateConcurrentWrapperForEventSource()
    {
        EventSource<int> @base = new EventPublisher<int>();
        var sut = @base.ToConcurrent();
        sut.Should().BeOfType<ConcurrentEventSource<int, EventSource<int>>>();
    }
}
