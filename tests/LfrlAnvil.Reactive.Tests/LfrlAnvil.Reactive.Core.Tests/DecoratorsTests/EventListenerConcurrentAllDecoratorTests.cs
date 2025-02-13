using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerConcurrentAllDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<IEventStream<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentAllDecorator<int>( null );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactAppliesLockDecoratorWithTheSameSyncObjectToEvents()
    {
        var sourceEvents = new[]
        {
            Substitute.For<IEventStream<int>>(), Substitute.For<IEventStream<int>>(), Substitute.For<IEventStream<int>>()
        };

        var expectedSourceEvents = new[]
        {
            Substitute.For<IEventStream<int>>(), Substitute.For<IEventStream<int>>(), Substitute.For<IEventStream<int>>()
        };

        foreach ( var (source, result) in sourceEvents.Zip( expectedSourceEvents ) )
            source.Decorate( Arg.Any<IEventListenerDecorator<int, int>>() ).Returns( result );

        var actualEvents = new List<IEventStream<int>>();
        var next = EventListener.Create<IEventStream<int>>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentAllDecorator<int>( null );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        Assertion.All(
                actualEvents.TestSequence( expectedSourceEvents ),
                sourceEvents.TestAll(
                    (e, _) => e.CallAt( 0 ).Arguments.FirstOrDefault().TestType().AssignableTo<EventListenerConcurrentDecorator<int>>() ) )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseEmittedEventStreamsAcquireUnderlyingLock()
    {
        var sync = new object();
        var hasLock = false;

        var inner = new EventPublisher<int>();
        var innerNext = Substitute.For<IEventListener<int>>();
        innerNext.When( x => x.React( Arg.Any<int>() ) ).Do( _ => { hasLock = Monitor.IsEntered( sync ); } );

        var next = EventListener.Create<IEventStream<int>>( e => e.Listen( innerNext ) );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentAllDecorator<int>( sync );
        var listener = sut.Decorate( next, subscriber );

        listener.React( inner );
        inner.Publish( Fixture.Create<int>() );

        hasLock.TestTrue().Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<IEventStream<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerConcurrentAllDecorator<int>( null );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCalls( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void ConcurrentAllExtension_ShouldCreateEventStreamThatAppliesLockDecoratorWithTheSameSyncObjectToEvents()
    {
        var inner = Substitute.For<IEventStream<int>>();
        var expectedInner = Substitute.For<IEventStream<int>>();
        inner.Decorate( Arg.Any<IEventListenerDecorator<int, int>>() ).Returns( expectedInner );

        var actualEvents = new List<IEventStream<int>>();
        var next = EventListener.Create<IEventStream<int>>( actualEvents.Add );
        var sut = new EventPublisher<IEventStream<int>>();
        var decorated = sut.ConcurrentAll();
        decorated.Listen( next );

        sut.Publish( inner );

        Assertion.All(
                actualEvents.TestSequence( [ expectedInner ] ),
                inner.CallAt( 0 ).Arguments.FirstOrDefault().TestType().AssignableTo<EventListenerConcurrentDecorator<int>>() )
            .Go();
    }

    [Fact]
    public void ConcurrentAllExtension_WithExplicitSyncObject_ShouldCreateEventStreamThatAppliesLockDecoratorWithTheSameSyncObjectToEvents()
    {
        var sync = new object();
        var inner = Substitute.For<IEventStream<int>>();
        var expectedInner = Substitute.For<IEventStream<int>>();
        inner.Decorate( Arg.Any<IEventListenerDecorator<int, int>>() ).Returns( expectedInner );

        var actualEvents = new List<IEventStream<int>>();
        var next = EventListener.Create<IEventStream<int>>( actualEvents.Add );
        var sut = new EventPublisher<IEventStream<int>>();
        var decorated = sut.ConcurrentAll( sync );
        decorated.Listen( next );

        sut.Publish( inner );

        Assertion.All(
                actualEvents.TestSequence( [ expectedInner ] ),
                inner.CallAt( 0 ).Arguments.FirstOrDefault().TestType().AssignableTo<EventListenerConcurrentDecorator<int>>() )
            .Go();
    }

    [Fact]
    public void ShareConcurrencyWithAllExtension_ShouldApplyLockToOuterStreamAndAllInnerStreams()
    {
        var expectedSecondDecorateResult = Substitute.For<IEventStream<IEventStream<int>>>();

        var expectedFirstDecorateResult = Substitute.For<IEventStream<IEventStream<int>>>();
        expectedFirstDecorateResult.Decorate( Arg.Any<IEventListenerDecorator<IEventStream<int>, IEventStream<int>>>() )
            .Returns( expectedSecondDecorateResult );

        var sut = Substitute.For<IEventStream<IEventStream<int>>>();
        sut.Decorate( Arg.Any<IEventListenerDecorator<IEventStream<int>, IEventStream<int>>>() )
            .Returns( expectedFirstDecorateResult );

        var result = sut.ShareConcurrencyWithAll();

        Assertion.All(
                result.TestRefEquals( expectedSecondDecorateResult ),
                sut.CallAt( 0 ).Arguments.FirstOrDefault().TestType().AssignableTo<EventListenerConcurrentDecorator<IEventStream<int>>>(),
                expectedFirstDecorateResult.CallAt( 0 )
                    .Arguments.FirstOrDefault()
                    .TestType()
                    .AssignableTo<EventListenerConcurrentAllDecorator<int>>() )
            .Go();
    }
}
