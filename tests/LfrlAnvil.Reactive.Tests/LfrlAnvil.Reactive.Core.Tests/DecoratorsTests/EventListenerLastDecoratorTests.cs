using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerLastDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerLastDecorator<int>();

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactDoesNotForwardAnyEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerLastDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestEmpty().Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextReactWithLastEvent(DisposalSource source)
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerLastDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        listener.OnDispose( source );

        actualEvents.TestSequence( [ sourceEvents[^1] ] ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeDoesNotCallNextReactWhenNoEventHasBeenReceived(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerLastDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestDidNotReceiveCall( x => x.React( Arg.Any<int>() ) ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerLastDecorator<int>();
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void LastExtension_ShouldCreateEventStreamThatForwardsTheLastEventWhenEventSourceDisposes()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Last();
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        sut.Dispose();

        actualEvents.TestSequence( [ sourceEvents[^1] ] ).Go();
    }
}
