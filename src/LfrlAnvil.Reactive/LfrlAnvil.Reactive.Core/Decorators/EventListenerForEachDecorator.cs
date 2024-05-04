using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with all emitted events and invokes the provided delegate for each event.
/// Delegate invocation happens before the decorated event listener gets notified.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerForEachDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Action<TEvent> _action;

    /// <summary>
    /// Creates a new <see cref="EventListenerForEachDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="action">Delegate to invoke on each event.</param>
    public EventListenerForEachDecorator(Action<TEvent> action)
    {
        _action = action;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _action );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Action<TEvent> _action;

        internal EventListener(IEventListener<TEvent> next, Action<TEvent> action)
            : base( next )
        {
            _action = action;
        }

        public override void React(TEvent @event)
        {
            _action( @event );
            Next.React( @event );
        }
    }
}
