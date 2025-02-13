using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerTakeDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber_WhenCountIsGreaterThanZero()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeDecorator<int>( count: 1 );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Decorate_ShouldDisposeTheSubscriber_WhenCountIsLessThanOne(int count)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeDecorator<int>( count );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestReceivedCalls( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardsFirstCountEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeDecorator<int>( count: sourceEvents.Length );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatDisposesTheSubscriberAfterForwardingFirstCountEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeDecorator<int>( count: sourceEvents.Length );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        subscriber.TestReceivedCalls( x => x.Dispose() ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerTakeDecorator<int>( count: 3 );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCalls( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void TakeExtension_ShouldCreateEventStreamThatForwardsFirstCountEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { 1, 2, 3, 5, 7 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Take( expectedEvents.Length );
        var subscriber = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        Assertion.All(
                actualEvents.TestSequence( expectedEvents ),
                subscriber.IsDisposed.TestTrue() )
            .Go();
    }
}
