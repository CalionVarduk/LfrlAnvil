using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Filters out emitted events with which to notify the decorated event listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerWhereDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Func<TEvent, bool> _predicate;

    /// <summary>
    /// Creates a new <see cref="EventListenerWhereDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="predicate">Predicate used for filtering events. Events that return <b>false</b> will be skipped.</param>
    public EventListenerWhereDecorator(Func<TEvent, bool> predicate)
    {
        _predicate = predicate;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _predicate );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Func<TEvent, bool> _predicate;

        internal EventListener(IEventListener<TEvent> next, Func<TEvent, bool> predicate)
            : base( next )
        {
            _predicate = predicate;
        }

        public override void React(TEvent @event)
        {
            if ( _predicate( @event ) )
                Next.React( @event );
        }
    }
}
