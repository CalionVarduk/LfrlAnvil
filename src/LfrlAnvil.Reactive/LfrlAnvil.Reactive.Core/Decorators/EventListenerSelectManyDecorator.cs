using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Maps emitted event to a collection of events and notifies the decorated event listener
/// with each element of the result of that mapping separately.
/// </summary>
/// <typeparam name="TSourceEvent">Source event type.</typeparam>
/// <typeparam name="TNextEvent">Next event type.</typeparam>
public sealed class EventListenerSelectManyDecorator<TSourceEvent, TNextEvent> : IEventListenerDecorator<TSourceEvent, TNextEvent>
{
    private readonly Func<TSourceEvent, IEnumerable<TNextEvent>> _selector;

    /// <summary>
    /// Creates a new <see cref="EventListenerSelectManyDecorator{TSourceEvent,TNextEvent}"/> instance.
    /// </summary>
    /// <param name="selector">Next event collection selector.</param>
    public EventListenerSelectManyDecorator(Func<TSourceEvent, IEnumerable<TNextEvent>> selector)
    {
        _selector = selector;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TSourceEvent> Decorate(IEventListener<TNextEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _selector );
    }

    private sealed class EventListener : DecoratedEventListener<TSourceEvent, TNextEvent>
    {
        private readonly Func<TSourceEvent, IEnumerable<TNextEvent>> _selector;

        internal EventListener(IEventListener<TNextEvent> next, Func<TSourceEvent, IEnumerable<TNextEvent>> selector)
            : base( next )
        {
            _selector = selector;
        }

        public override void React(TSourceEvent @event)
        {
            foreach ( var nextEvent in _selector( @event ) )
                Next.React( nextEvent );
        }
    }
}
