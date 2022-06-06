using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators
{
    public sealed class EventListenerDistinctDecorator<TEvent, TKey> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly Func<TEvent, TKey> _keySelector;
        private readonly IEqualityComparer<TKey> _equalityComparer;

        public EventListenerDistinctDecorator(Func<TEvent, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
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
}
