using System.Collections.Generic;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerDistinctUntilDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDistinctUntilDecorator<int, int, string>( v => v, EqualityComparer<int>.Default, target );

        _ = sut.Decorate( next, subscriber );

        Assertion.All(
                subscriber.TestDidNotReceiveCall( x => x.Dispose() ),
                target.HasSubscribers.TestTrue() )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerWhoseReactOnlyEmitsDistinctSourceEvents()
    {
        var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
        var expectedEvents = new[] { 1, 2, 3, 7, 5 };
        var actualEvents = new List<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDistinctUntilDecorator<int, int, string>( v => v, EqualityComparer<int>.Default, target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatClearsRememberedKeysWhenTargetEmits()
    {
        var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
        var expectedEvents = new[] { 1, 2, 3, 7, 5, 1, 2, 3, 7, 5 };
        var actualEvents = new List<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDistinctUntilDecorator<int, int, string>( v => v, EqualityComparer<int>.Default, target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents )
            listener.React( e );

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDistinctUntilDecorator<int, int, string>( v => v, EqualityComparer<int>.Default, target );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        next.TestReceivedCalls( x => x.OnDispose( source ) ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeDisposesTheTargetSubscriber(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<int>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerDistinctUntilDecorator<int, int, string>( v => v, EqualityComparer<int>.Default, target );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        target.HasSubscribers.TestFalse().Go();
    }

    [Fact]
    public void DistinctUntilExtension_ShouldCreateEventStreamThatEmitsDistinctEventsAndClearsRememberedKeysWhenTargetEmits()
    {
        var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
        var expectedEvents = new[] { 1, 2, 3, 7, 5, 1, 2, 3, 7, 5 };
        var actualEvents = new List<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.DistinctUntil( target );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Fact]
    public void
        DistinctUntilExtension_WithExplicitComparer_ShouldCreateEventStreamThatEmitsDistinctEventsAndClearsRememberedKeysWhenTargetEmits()
    {
        var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
        var expectedEvents = new[] { 1, 2, 3, 7, 5, 1, 2, 3, 7, 5 };
        var actualEvents = new List<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.DistinctUntil( EqualityComparer<int>.Default, target );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        actualEvents.TestSequence( expectedEvents ).Go();
    }

    [Fact]
    public void DistinctUntilExtension_ShouldForwardAllEvents_WhenTargetIsSource()
    {
        var sourceEvents = new[] { 1, 2, 2, 3, 3, 3, 7, 5, 3, 3, 5, 7, 1, 7, 7 };
        var actualEvents = new List<int>();

        var next = EventListener.Create<int>( actualEvents.Add );
        var sut = new EventPublisher<int>();
        var decorated = sut.DistinctUntil( sut );
        decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        Assertion.All(
                actualEvents.TestSequence( sourceEvents ),
                sut.Subscribers.Count.TestEquals( 2 ) )
            .Go();
    }
}
