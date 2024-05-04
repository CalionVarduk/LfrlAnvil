using System;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a concurrent version of an <see cref="EventHandlerSource{TEvent}"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class ConcurrentEventHandlerSource<TEvent> : ConcurrentEventSource<WithSender<TEvent>, EventHandlerSource<TEvent>>
{
    /// <summary>
    /// Creates a new <see cref="ConcurrentEventHandlerSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="setup">Delegate that handles initialization of this event source.</param>
    /// <param name="teardown">Delegate that handles disposal of this event source.</param>
    public ConcurrentEventHandlerSource(Action<EventHandler<TEvent>> setup, Action<EventHandler<TEvent>> teardown)
        : base( new EventHandlerSource<TEvent>() )
    {
        setup( Handle );
        Base.Teardown = _ => teardown( Handle );
    }

    private void Handle(object? sender, TEvent args)
    {
        lock ( Sync )
        {
            Base.Handle( sender, args );
        }
    }
}
