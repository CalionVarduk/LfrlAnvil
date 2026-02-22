using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Tests.EnumerableEventSourceTests;

public abstract class GenericEnumerableEventSourceTests<TEvent> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions()
    {
        var values = Fixture.CreateMany<TEvent>();
        var sut = new EnumerableEventSource<TEvent>( values );
        sut.HasSubscribers.TestFalse().Go();
    }

    [Fact]
    public void Listen_ShouldCallListenerReactForEachElement()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 3 );
        var actualValues = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualValues.Add );
        var sut = new EnumerableEventSource<TEvent>( values );

        _ = sut.Listen( listener );

        actualValues.TestSequence( values ).Go();
    }

    [Fact]
    public void Listen_ShouldDisposeSubscriberImmediatelyAfterCallingItsReact()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 3 );
        var listener = Substitute.For<IEventListener<TEvent>>();
        var sut = new EnumerableEventSource<TEvent>( values );

        var subscriber = sut.Listen( listener );

        Assertion.All(
                subscriber.IsDisposed.TestTrue(),
                sut.HasSubscribers.TestFalse() )
            .Go();
    }

    [Fact]
    public void Listen_ShouldOnlyCallListenerReactAsLongAsEventSourceIsNotDisposed()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 3 ).ToList();
        var actualValues = new List<TEvent>();
        var sut = new EnumerableEventSource<TEvent>( values );
        var listener = EventListener.Create<TEvent>( e =>
        {
            actualValues.Add( e );
            sut.Dispose();
        } );

        _ = sut.Listen( listener );

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                actualValues.TestSequence( [ values[0] ] ) )
            .Go();
    }

    [Fact]
    public void From_ThenListen_ShouldCallListenerReactForEachElement()
    {
        var values = Fixture.CreateManyDistinct<TEvent>( count: 3 ).ToArray();
        var actualValues = new List<TEvent>();
        var listener = EventListener.Create<TEvent>( actualValues.Add );
        var sut = EventSource.From( values );

        _ = sut.Listen( listener );

        actualValues.TestSequence( values ).Go();
    }
}
