using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with distinct emitted events, excluding duplicates.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TKey">Event's key type.</typeparam>
public sealed class EventListenerDistinctDecorator<TEvent, TKey> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Func<TEvent, TKey> _keySelector;
    private readonly IEqualityComparer<TKey> _equalityComparer;

    /// <summary>
    /// Creates a new <see cref="EventListenerDistinctDecorator{TEvent,TKey}"/> instance.
    /// </summary>
    /// <param name="keySelector">Event key selector.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    public EventListenerDistinctDecorator(Func<TEvent, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
    {
        _keySelector = keySelector;
        _equalityComparer = equalityComparer;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _keySelector, _equalityComparer );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Func<TEvent, TKey> _keySelector;
        private readonly HashSet<TKey> _keySet;

        internal EventListener(IEventListener<TEvent> next, Func<TEvent, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
            : base( next )
        {
            _keySelector = keySelector;
            _keySet = new HashSet<TKey>( equalityComparer );
        }

        public override void React(TEvent @event)
        {
            if ( _keySet.Add( _keySelector( @event ) ) )
                Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _keySet.Clear();
            _keySet.TrimExcess();

            base.OnDispose( source );
        }
    }
}
