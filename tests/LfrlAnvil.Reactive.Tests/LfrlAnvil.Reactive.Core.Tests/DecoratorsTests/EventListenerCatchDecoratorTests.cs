using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerCatchDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var onError = Substitute.For<Action<Exception>>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerCatchDecorator<int, Exception>( onError );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactForwardsEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerCatchDecorator<int, Exception>( onError );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatCallsOnError_WhenNextReactThrowsAnException()
    {
        var exception = new Exception();
        var sourceEvent = Fixture.Create<int>();
        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( _ => throw exception );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerCatchDecorator<int, Exception>( onError );
        var listener = sut.Decorate( next, subscriber );

        listener.React( sourceEvent );

        onError.CallAt( 0 ).Arguments.TestSequence( [ exception ] ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerThatCallsOnError_WhenNextOnDisposeThrowsAnException(DisposalSource source)
    {
        var exception = new Exception();
        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( _ => { }, _ => throw exception );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerCatchDecorator<int, Exception>( onError );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        onError.CallAt( 0 ).Arguments.TestSequence( [ exception ] ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var onError = Substitute.For<Action<Exception>>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerCatchDecorator<int, Exception>( onError );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCalls( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void CatchExtension_ShouldCreateListenerWhoseReactForwardsEvents()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = new List<int>();

        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Catch( onError );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Fact]
    public void CatchExtension_ShouldCreateListenerThatCallsOnError_WhenNextReactThrowsAnException()
    {
        var exception = new Exception();
        var sourceEvent = Fixture.Create<int>();
        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( _ => throw exception );
        var sut = new EventPublisher<int>();
        var decorated = sut.Catch( onError );
        decorated.Listen( next );

        sut.Publish( sourceEvent );

        onError.CallAt( 0 ).Arguments.TestSequence( [ exception ] ).Go();
    }

    [Fact]
    public void CatchExtension_ShouldCreateEventStreamThatCallsOnError_WhenNextOnDisposeThrowsAnException()
    {
        var exception = new Exception();
        var onError = Substitute.For<Action<Exception>>();
        var next = EventListener.Create<int>( _ => { }, _ => throw exception );
        var sut = new EventPublisher<int>();
        var decorated = sut.Catch( onError );
        var subscriber = decorated.Listen( next );

        subscriber.Dispose();

        onError.CallAt( 0 ).Arguments.TestSequence( [ exception ] ).Go();
    }
}
