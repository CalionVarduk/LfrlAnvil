using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerTakeWhileDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeWhileDecorator<int>( e => e < 10 );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardsAllEventsUntilPredicateFails()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 1, 2, 3, 5, 7 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeWhileDecorator<int>( e => e < 10 );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatDisposesTheSubscriberWhenPredicateFails()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeWhileDecorator<int>( e => e < 23 );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        subscriber.TestReceivedCall( x => x.Dispose() ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeWhileDecorator<int>( e => e < 10 );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void TakeWhileExtension_ShouldCreateEventStreamThatForwardsAllEventsUntilPredicateFails()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 1, 2, 3, 5, 7 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.TakeWhile( e => e < 10 );
        var subscriber = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        Assertion.All(
                actualEvents.TestSequence( expectedEvents ),
                subscriber.IsDisposed.TestTrue() )
            .Go();
    }
}
