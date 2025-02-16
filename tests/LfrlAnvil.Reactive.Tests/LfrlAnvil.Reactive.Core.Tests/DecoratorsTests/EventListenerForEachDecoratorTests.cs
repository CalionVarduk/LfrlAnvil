using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerForEachDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerForEachDecorator<int>( _ => { } );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactCallsActionForEachSourceEvent()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var calledEvents = new List<int>();
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerForEachDecorator<int>( x => calledEvents.Add( x ) );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        Assertion.All(
                actualEvents.TestSequence( sourceEvents ),
                calledEvents.TestSequence( sourceEvents ) )
            .Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerForEachDecorator<int>( _ => { } );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCall( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void ForEachExtension_ShouldCreateEventStreamThatCallsActionForEachSourceEvent()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var calledEvents = new List<int>();
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.ForEach( x => calledEvents.Add( x ) );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        Assertion.All(
                actualEvents.TestSequence( sourceEvents ),
                calledEvents.TestSequence( sourceEvents ) )
            .Go();
    }
}
