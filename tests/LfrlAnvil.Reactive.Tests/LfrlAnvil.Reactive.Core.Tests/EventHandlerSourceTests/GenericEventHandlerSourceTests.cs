using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Tests.EventHandlerSourceTests;

public abstract class GenericEventHandlerSourceTests<TEvent> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions_AndActiveEventHandler()
    {
        var target = new Target();
        var sut = new EventHandlerSource<TEvent>( h => target.Handler += h, h => target.Handler -= h );

        Assertion.All(
                sut.HasSubscribers.TestFalse(),
                target.IsHandlerNull().TestFalse() )
            .Go();
    }

    [Fact]
    public void Listen_ShouldCallListenerReact_WhenEventHandlerEmits()
    {
        var target = new Target();
        var sender = new object();
        var values = Fixture.CreateManyDistinct<TEvent>( count: 3 );
        var actualValues = new List<WithSender<TEvent>>();
        var sut = new EventHandlerSource<TEvent>( h => target.Handler += h, h => target.Handler -= h );
        var listener = EventListener.Create<WithSender<TEvent>>( actualValues.Add );

        _ = sut.Listen( listener );

        foreach ( var value in values ) target.Emit( sender, value );

        Assertion.All(
                actualValues.Select( v => v.Event ).TestSequence( values ),
                actualValues.Select( v => v.Sender ).TestAll( (s, _) => s.TestRefEquals( sender ) ) )
            .Go();
    }

    [Fact]
    public void Dispose_ShouldRemoveEventHandler()
    {
        var target = new Target();
        var sut = new EventHandlerSource<TEvent>( h => target.Handler += h, h => target.Handler -= h );

        sut.Dispose();

        target.IsHandlerNull().TestTrue().Go();
    }

    [Fact]
    public void FromEvent_ThenListen_ShouldCallListenerReact_WhenEventHandlerEmits()
    {
        var target = new Target();
        var sender = new object();
        var values = Fixture.CreateManyDistinct<TEvent>( count: 3 );
        var actualValues = new List<WithSender<TEvent>>();
        var sut = EventSource.FromEvent<TEvent>( h => target.Handler += h, h => target.Handler -= h );
        var listener = EventListener.Create<WithSender<TEvent>>( actualValues.Add );

        _ = sut.Listen( listener );

        foreach ( var value in values ) target.Emit( sender, value );

        Assertion.All(
                actualValues.Select( v => v.Event ).TestSequence( values ),
                actualValues.Select( v => v.Sender ).TestAll( (s, _) => s.TestRefEquals( sender ) ) )
            .Go();
    }

    private sealed class Target
    {
        public event EventHandler<TEvent>? Handler;

        public void Emit(object? sender, TEvent @event)
        {
            Handler?.Invoke( sender, @event );
        }

        public bool IsHandlerNull()
        {
            return Handler is null;
        }
    }
}
