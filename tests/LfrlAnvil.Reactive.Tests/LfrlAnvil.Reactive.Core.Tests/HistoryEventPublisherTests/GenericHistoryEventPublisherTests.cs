using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Reactive.Tests.HistoryEventPublisherTests;

public abstract class GenericHistoryEventPublisherTests<TEvent> : TestsBase
{
    [Theory]
    [InlineData( 1 )]
    [InlineData( 5 )]
    [InlineData( 10 )]
    public void Ctor_ShouldCreateWithEmptyHistoryAndCorrectCapacity(int capacity)
    {
        var sut = new HistoryEventPublisher<TEvent>( capacity );

        Assertion.All(
                sut.Subscribers.TestEmpty(),
                sut.History.TestEmpty(),
                sut.Capacity.TestEquals( capacity ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCapacityIsLessThanOne(int capacity)
    {
        var action = Lambda.Of( () => new HistoryEventPublisher<TEvent>( capacity ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Publish_ShouldAddFirstEventToHistory()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );

        sut.Publish( @event );

        sut.History.TestSequence( [ @event ] ).Go();
    }

    [Fact]
    public void Publish_ShouldAddNextEventAsLastEntryToHistory()
    {
        var events = Fixture.CreateManyDistinct<TEvent>( count: 3 );
        var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );

        foreach ( var @event in events ) sut.Publish( @event );

        sut.History.TestSequence( events ).Go();
    }

    [Fact]
    public void Publish_ShouldAddNextEventAsLastEntryToHistoryAndRemoveFirstEntry_WhenCapacityIsExceeded()
    {
        var events = Fixture.CreateManyDistinct<TEvent>( count: 3 );
        var sut = new HistoryEventPublisher<TEvent>( capacity: 2 );

        foreach ( var @event in events ) sut.Publish( @event );

        sut.History.TestSequence( [ events[1], events[2] ] ).Go();
    }

    [Fact]
    public void Publish_ShouldCallListenerReact()
    {
        var @event = Fixture.Create<TEvent>();
        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );
        sut.Listen( listener );

        sut.Publish( @event );

        listener.TestReceivedCalls( x => x.React( @event ) ).Go();
    }

    [Fact]
    public void Listen_ShouldCallListenerReactImmediatelyForEachHistoryEntry()
    {
        var events = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );
        foreach ( var @event in events ) sut.Publish( @event );

        sut.Listen( listener );

        Assertion.All(
                listener.TestReceivedCalls( x => x.React( events[0] ) ),
                listener.TestReceivedCalls( x => x.React( events[1] ) ) )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCallImmediatelyListenerReactOnlyForTheFirstHistoryEntry_WhenFirstListenerReactDisposedTheSubscriber()
    {
        var events = Fixture.CreateManyDistinct<TEvent>( count: 2 );
        var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );
        var listener = Substitute.For<IEventListener<TEvent>>();
        listener.When( l => l.React( events[0] ) ).Do( _ => sut.Subscribers.First().Dispose() );
        foreach ( var @event in events ) sut.Publish( @event );

        var subscriber = sut.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                listener.TestReceivedCalls( x => x.React( events[0] ) ),
                listener.TestDidNotReceiveCall( x => x.React( events[1] ) ) )
            .Go();
    }

    [Fact]
    public void ClearHistory_ShouldRemoveAllHistoryEntries()
    {
        var @event = Fixture.Create<TEvent>();
        var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );
        sut.Publish( @event );

        sut.ClearHistory();

        sut.History.TestEmpty().Go();
    }

    [Fact]
    public void Dispose_ShouldClearHistory()
    {
        var events = Fixture.CreateManyDistinct<TEvent>( count: 3 );
        var sut = new HistoryEventPublisher<TEvent>( capacity: 10 );
        foreach ( var @event in events ) sut.Publish( @event );

        sut.Dispose();

        Assertion.All(
                sut.IsDisposed.TestTrue(),
                sut.History.TestEmpty() )
            .Go();
    }
}
