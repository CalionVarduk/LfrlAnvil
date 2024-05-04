using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Skips events at the beginning of the sequence until an event passes the provided predicate,
/// before starting to notify the decorated event listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerSkipWhileDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Func<TEvent, bool> _predicate;

    /// <summary>
    /// Creates a new <see cref="EventListenerSkipWhileDecorator{TEvent}"/> instance,
    /// </summary>
    /// <param name="predicate">Predicate that skips events until the first event that passes it (returns <b>true</b>).</param>
    public EventListenerSkipWhileDecorator(Func<TEvent, bool> predicate)
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
        private bool _isDone;

        internal EventListener(IEventListener<TEvent> next, Func<TEvent, bool> predicate)
            : base( next )
        {
            _isDone = false;
            _predicate = predicate;
        }

        public override void React(TEvent @event)
        {
            if ( _isDone )
            {
                Next.React( @event );
                return;
            }

            if ( _predicate( @event ) )
                return;

            _isDone = true;
            Next.React( @event );
        }
    }
}
