using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions.Execution;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Reactive.Tests.ConcurrentEventHandlerSourceTests;

public class ConcurrentEventHandlerSourceTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEventSourceWithoutSubscriptions_AndActiveEventHandler()
    {
        var target = new Target();
        var sut = new ConcurrentEventHandlerSource<int>( h => target.Handler += h, h => target.Handler -= h );

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
        var values = Fixture.CreateDistinctCollection<int>( count: 3 );
        var actualValues = new List<WithSender<int>>();
        var sut = new ConcurrentEventHandlerSource<int>( h => target.Handler += h, h => target.Handler -= h );
        var listener = EventListener.Create<WithSender<int>>( actualValues.Add );

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
    public void Listen_ShouldAcquireLock_WhenEventHandlerEmits()
    {
        var target = new Target();
        var sender = new object();
        var @event = Fixture.Create<int>();
        var sut = new ConcurrentEventHandlerSource<int>( h => target.Handler += h, h => target.Handler -= h );
        var sync = sut.Sync;
        var hasLock = false;
        var listener = EventListener.Create<WithSender<int>>( _ => hasLock = Monitor.IsEntered( sync ) );

        var _ = sut.Listen( listener );
        target.Emit( sender, @event );

        hasLock.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldRemoveEventHandler()
    {
        var target = new Target();
        var sut = new ConcurrentEventHandlerSource<int>( h => target.Handler += h, h => target.Handler -= h );

        sut.Dispose();

        target.IsHandlerNull().Should().BeTrue();
    }

    [Fact]
    public void ConcurrentFromEvent_ThenListen_ShouldCallListenerReact_WhenEventHandlerEmits()
    {
        var target = new Target();
        var sender = new object();
        var values = Fixture.CreateDistinctCollection<int>( count: 3 );
        var actualValues = new List<WithSender<int>>();
        var sut = EventSource.ConcurrentFromEvent<int>( h => target.Handler += h, h => target.Handler -= h );
        var listener = EventListener.Create<WithSender<int>>( actualValues.Add );

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
        public event EventHandler<int>? Handler;

        public void Emit(object? sender, int @event)
        {
            Handler?.Invoke( sender, @event );
        }

        public bool IsHandlerNull()
        {
            return Handler is null;
        }
    }
}
