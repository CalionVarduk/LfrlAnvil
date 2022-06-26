using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

public sealed class EventListenerDistinctUntilChangedDecorator<TEvent, TKey> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Func<TEvent, TKey> _keySelector;
    private readonly IEqualityComparer<TKey> _equalityComparer;

    public EventListenerDistinctUntilChangedDecorator(Func<TEvent, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
    {
        _keySelector = keySelector;
        _equalityComparer = equalityComparer;
    }

    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _keySelector, _equalityComparer );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Func<TEvent, TKey> _keySelector;
        private readonly IEqualityComparer<TKey> _equalityComparer;
        private Optional<TKey> _lastKey;

        internal EventListener(IEventListener<TEvent> next, Func<TEvent, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
            : base( next )
        {
            _keySelector = keySelector;
            _equalityComparer = equalityComparer;
            _lastKey = Optional<TKey>.Empty;
        }

        public override void React(TEvent @event)
        {
            var key = _keySelector( @event );

            if ( ! _lastKey.HasValue || ! _equalityComparer.Equals( _lastKey.Event!, key ) )
                Next.React( @event );

            _lastKey = new Optional<TKey>( key );
        }

        public override void OnDispose(DisposalSource source)
        {
            _lastKey = Optional<TKey>.Empty;
            base.OnDispose( source );
        }
    }
}
