using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.EventHandlerSourceTests;

public abstract class GenericEventHandlerSourceTests<TEvent> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions_AndActiveEventHandler()
    {
        var target = new Target();
        var sut = new EventHandlerSource<TEvent>( h => target.Handler += h, h => target.Handler -= h );

        using ( new AssertionScope() )
        {
            sut.HasSubscribers.Should().BeFalse();
            target.IsHandlerNull().Should().BeFalse();
        }
    }

    [Fact]
    public void Listen_ShouldCallListenerReact_WhenEventHandlerEmits()
    {
        var target = new Target();
        var sender = new object();
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 3 );
        var actualValues = new List<WithSender<TEvent>>();
        var sut = new EventHandlerSource<TEvent>( h => target.Handler += h, h => target.Handler -= h );
        var listener = EventListener.Create<WithSender<TEvent>>( actualValues.Add );

        var _ = sut.Listen( listener );

        foreach ( var value in values )
            target.Emit( sender, value );

        using ( new AssertionScope() )
        {
            actualValues.Select( v => v.Event ).Should().BeSequentiallyEqualTo( values );
            actualValues.Select( v => v.Sender ).Should().OnlyContain( s => ReferenceEquals( s, sender ) );
        }
    }

    [Fact]
    public void Dispose_ShouldRemoveEventHandler()
    {
        var target = new Target();
        var sut = new EventHandlerSource<TEvent>( h => target.Handler += h, h => target.Handler -= h );

        sut.Dispose();

        target.IsHandlerNull().Should().BeTrue();
    }

    [Fact]
    public void FromEvent_ThenListen_ShouldCallListenerReact_WhenEventHandlerEmits()
    {
        var target = new Target();
        var sender = new object();
        var values = Fixture.CreateDistinctCollection<TEvent>( count: 3 );
        var actualValues = new List<WithSender<TEvent>>();
        var sut = EventSource.FromEvent<TEvent>( h => target.Handler += h, h => target.Handler -= h );
        var listener = EventListener.Create<WithSender<TEvent>>( actualValues.Add );

        var _ = sut.Listen( listener );

        foreach ( var value in values )
            target.Emit( sender, value );

        using ( new AssertionScope() )
        {
            actualValues.Select( v => v.Event ).Should().BeSequentiallyEqualTo( values );
            actualValues.Select( v => v.Sender ).Should().OnlyContain( s => ReferenceEquals( s, sender ) );
        }
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
