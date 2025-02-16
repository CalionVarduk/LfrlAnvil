using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerSwitchAllDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSwitchAllDecorator<int>();

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactSubscribesToFirstInnerStream()
    {
        var inner = new EventPublisher<int>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSwitchAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( inner );

        inner.HasSubscribers.TestTrue().Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactDoesNotDispose_WhenFirstInnerStreamIsDisposed()
    {
        var inner = new EventPublisher<int>();
        inner.Dispose();

        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSwitchAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( inner );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatForwardsFirstInnerStreamEvents()
    {
        var firstStreamValues = new[] { 1, 2 };
        var actualEvents = new List<int>();

        var inner = new EventPublisher<int>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSwitchAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( inner );

        foreach ( var e in firstStreamValues )
            inner.Publish( e );

        Assertion.All(
                inner.HasSubscribers.TestTrue(),
                actualEvents.TestSequence( firstStreamValues ) )
            .Go();
    }

    [Fact]
    public void
        Decorate_ShouldCreateListenerThatForwardsFirstAndSecondInnerStreamEvents_WhenFirstStreamDisposesBeforeSecondStreamSubscriberIsCreated()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var expectedEvents = firstStreamValues.Concat( secondStreamValues );
        var actualEvents = new List<int>();

        var firstInner = new EventPublisher<int>();
        var secondInner = new EventPublisher<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSwitchAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( firstInner );

        foreach ( var e in firstStreamValues )
            firstInner.Publish( e );

        firstInner.Dispose();

        listener.React( secondInner );

        foreach ( var e in secondStreamValues )
            secondInner.Publish( e );

        Assertion.All(
                firstInner.HasSubscribers.TestFalse(),
                secondInner.HasSubscribers.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Fact]
    public void
        Decorate_ShouldCreateListenerThatStopsForwardingActiveStreamEvents_WhenActiveStreamDoesNotDisposeBeforeNextStreamSubscriberIsCreated()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = firstStreamValues.Take( 1 ).Concat( secondStreamValues.Take( 1 ) ).Concat( thirdStreamValues );
        var actualEvents = new List<int>();

        var firstInner = new EventPublisher<int>();
        var secondInner = new EventPublisher<int>();
        var thirdInner = new EventPublisher<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSwitchAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( firstInner );
        firstInner.Publish( firstStreamValues[0] );
        listener.React( secondInner );
        firstInner.Publish( firstStreamValues[1] );
        secondInner.Publish( secondStreamValues[0] );
        listener.React( thirdInner );
        secondInner.Publish( secondStreamValues[1] );

        foreach ( var e in thirdStreamValues )
            thirdInner.Publish( e );

        Assertion.All(
                firstInner.HasSubscribers.TestFalse(),
                secondInner.HasSubscribers.TestFalse(),
                thirdInner.HasSubscribers.TestTrue(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeDisposesInnerStreamSubscriber(DisposalSource source)
    {
        var inner = new EventPublisher<int>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSwitchAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.React( inner );
        listener.OnDispose( source );

        inner.HasSubscribers.TestFalse().Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerSwitchAllDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void
        SwitchAllExtension_ShouldCreateEventStreamThatStopsForwardingActiveStreamEvents_WhenActiveStreamDoesNotDisposeBeforeNextStreamSubscriberIsCreated()
    {
        var firstStreamValues = new[] { 1, 2 };
        var secondStreamValues = new[] { 3, 5 };
        var thirdStreamValues = new[] { 7, 11 };
        var expectedEvents = firstStreamValues.Take( 1 ).Concat( secondStreamValues.Take( 1 ) ).Concat( thirdStreamValues );
        var actualEvents = new List<int>();

        var firstInner = new EventPublisher<int>();
        var secondInner = new EventPublisher<int>();
        var thirdInner = new EventPublisher<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<IEventStream<int>>();
        var decorated = sut.SwitchAll();
        var subscriber = decorated.Listen( next );

        sut.Publish( firstInner );
        firstInner.Publish( firstStreamValues[0] );
        sut.Publish( secondInner );
        firstInner.Publish( firstStreamValues[1] );
        secondInner.Publish( secondStreamValues[0] );
        sut.Publish( thirdInner );
        secondInner.Publish( secondStreamValues[1] );

        foreach ( var e in thirdStreamValues )
            thirdInner.Publish( e );

        Assertion.All(
                firstInner.HasSubscribers.TestFalse(),
                secondInner.HasSubscribers.TestFalse(),
                thirdInner.HasSubscribers.TestTrue(),
                sut.HasSubscribers.TestTrue(),
                subscriber.IsDisposed.TestFalse(),
                actualEvents.TestSequence( expectedEvents ) )
            .Go();
    }
}
