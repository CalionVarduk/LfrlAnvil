using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Tests.DecoratorsTests;

public class EventListenerBufferUntilDecoratorTests : TestsBase
{
    [Fact]
    public void Decorate_ShouldNotDisposeTheSubscriber_WhenTargetIsNotDisposed()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );

        _ = sut.Decorate( next, subscriber );

        Assertion.All(
                subscriber.TestDidNotReceiveCall( x => x.Dispose() ),
                target.HasSubscribers.TestTrue() )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldDisposeTheSubscriber_WhenTargetIsDisposed()
    {
        var target = new EventPublisher<string>();
        target.Dispose();

        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );

        _ = sut.Decorate( next, subscriber );

        Assertion.All(
                subscriber.TestReceivedCalls( x => x.Dispose() ),
                target.HasSubscribers.TestFalse() )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatCallsNextReactWithBufferedEventsWhenTargetEmits()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { new[] { 1, 2 }, new[] { 3, 5, 7 }, new[] { 11, 13, 17, 19 }, Array.Empty<int>(), new[] { 23 } };
        var actualEvents = new List<int[]>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents.Add( e.ToArray() ) );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents.Take( 2 ) )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 2 ).Take( 3 ) )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 5 ).Take( 4 ) )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );
        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 9 ) )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );

        Assertion.All(
                actualEvents.Count.TestEquals( expectedEvents.Length ),
                actualEvents.TestAll( (e, i) => e.TestSequence( expectedEvents[i] ) ) )
            .Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatCallsNextReactCorrectly_WhenBufferGetsLarge()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47 };
        var actualEvents = Array.Empty<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents = e.ToArray() );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        target.Publish( Fixture.Create<string>() );

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeCallsNextOnDispose(DisposalSource source)
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
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
        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        listener.OnDispose( source );

        target.HasSubscribers.TestFalse().Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatDisposes_WhenTargetDisposes()
    {
        var target = new EventPublisher<string>();
        var next = Substitute.For<IEventListener<ReadOnlyMemory<int>>>();
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        _ = sut.Decorate( next, subscriber );

        target.Dispose();

        subscriber.TestReceivedCalls( x => x.Dispose() ).Go();
    }

    [Theory]
    [InlineData( DisposalSource.EventSource )]
    [InlineData( DisposalSource.Subscriber )]
    public void Decorate_ShouldCreateListenerWhoseOnDisposeEmitsRemainingBufferedEvents(
        DisposalSource source)
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = Array.Empty<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents = e.ToArray() );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        listener.OnDispose( source );

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Fact]
    public void Decorate_ShouldCreateListenerThatEmitsRemainingBufferedEvents_WhenTargetDisposes()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = Array.Empty<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents = e.ToArray() );
        var subscriber = Substitute.For<IEventSubscriber>();
        var sut = new EventListenerBufferUntilDecorator<int, string>( target );
        var listener = sut.Decorate( next, subscriber );

        foreach ( var e in sourceEvents )
            listener.React( e );

        target.Dispose();

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Fact]
    public void BufferUntilExtension_ShouldCreateEventStreamThatCallsNextReactWithBufferedEventsWhenTargetEmits()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var expectedEvents = new[] { new[] { 1, 2 }, new[] { 3, 5, 7 }, new[] { 11, 13, 17, 19 }, Array.Empty<int>(), new[] { 23 } };
        var actualEvents = new List<int[]>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents.Add( e.ToArray() ) );
        var sut = new EventPublisher<int>();
        var decorated = sut.BufferUntil( target );
        decorated.Listen( next );

        foreach ( var e in sourceEvents.Take( 2 ) )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 2 ).Take( 3 ) )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 5 ).Take( 4 ) )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );
        target.Publish( Fixture.Create<string>() );

        foreach ( var e in sourceEvents.Skip( 9 ) )
            sut.Publish( e );

        target.Publish( Fixture.Create<string>() );

        Assertion.All(
                actualEvents.Count.TestEquals( expectedEvents.Length ),
                actualEvents.TestAll( (e, i) => e.TestSequence( expectedEvents[i] ) ) )
            .Go();
    }

    [Fact]
    public void BufferUntilExtension_ShouldCreateEventStreamThatEmitsRemainingBufferedEventsWhenSubscriptionIsDisposed()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        var actualEvents = Array.Empty<int>();

        var target = new EventPublisher<string>();
        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents = e.ToArray() );
        var sut = new EventPublisher<int>();
        var decorated = sut.BufferUntil( target );
        var subscriber = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        subscriber.Dispose();

        actualEvents.TestSequence( sourceEvents ).Go();
    }

    [Fact]
    public void BufferUntilExtension_ShouldEmitBufferWithOnePreviousEvent_WhenTargetIsSource()
    {
        var sourceEvents = new[] { 1, 2, 3, 5, 7 };
        var actualEvents = new List<int[]>();

        var next = EventListener.Create<ReadOnlyMemory<int>>( e => actualEvents.Add( e.ToArray() ) );
        var sut = new EventPublisher<int>();
        var decorated = sut.BufferUntil( sut );
        _ = decorated.Listen( next );

        foreach ( var e in sourceEvents )
            sut.Publish( e );

        Assertion.All(
                actualEvents.Count.TestEquals( sourceEvents.Length ),
                actualEvents.Take( 1 ).TestAll( (e, _) => e.TestEmpty() ),
                actualEvents.Skip( 1 ).TestAll( (e, i) => e.TestSequence( [ sourceEvents[i] ] ) ) )
            .Go();
    }
}
