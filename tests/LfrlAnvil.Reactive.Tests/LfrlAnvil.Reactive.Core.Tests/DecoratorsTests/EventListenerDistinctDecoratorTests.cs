using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerDistinctDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDistinctDecorator<int, int>( v => v, EqualityComparer<int>.Default );

        _ = sut.Decorate( next, subscriber );

        subscriber.TestDidNotReceiveCall( x => x.Dispose() ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactOnlyEmitsDistinctSourceEvents()
    {
        var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
        var expectedEvents = new[] { 1, 2, 3, 7, 5 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDistinctDecorator<int, int>( v => v, EqualityComparer<int>.Default );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDistinctDecorator<int, int>( v => v, EqualityComparer<int>.Default );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCalls( x => x.OnDispose( source ) ).Go();
    }

    [Fact]
    public void DistinctExtension_ShouldCreateEventStreamThatOnlyEmitsDistinctSourceEvents()
    {
        var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
        var expectedEvents = new[] { 1, 2, 3, 7, 5 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Distinct();
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Fact]
    public void DistinctExtension_WithExplicitComparer_ShouldCreateEventStreamThatOnlyEmitsDistinctSourceEvents()
    {
        var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
        var expectedEvents = new[] { 1, 2, 3, 7, 5 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.Distinct( EqualityComparer<int>.Default );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.TestSequence( expectedEvents ).Go();
    }
}
