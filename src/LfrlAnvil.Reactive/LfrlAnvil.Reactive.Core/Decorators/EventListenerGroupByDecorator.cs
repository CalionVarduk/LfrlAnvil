using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Decorators
{
    public sealed class EventListenerGroupByDecorator<TEvent, TKey> : IEventListenerDecorator<TEvent, EventGrouping<TKey, TEvent>>
    {
        private readonly Func<TEvent, TKey> _keySelector;
        private readonly IEqualityComparer<TKey> _equalityComparer;

        public EventListenerGroupByDecorator(Func<TEvent, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
        {
            _keySelector = keySelector;
            _equalityComparer = equalityComparer;
        }

        [Pure]
        public IEventListener<TEvent> Decorate(IEventListener<EventGrouping<TKey, TEvent>> listener, IEventSubscriber _)
        {
            return new EventListener( listener, _keySelector, _equalityComparer );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, EventGrouping<TKey, TEvent>>
        {
            private readonly Func<TEvent, TKey> _keySelector;
            private readonly Dictionary<TKey, GrowingBuffer<TEvent>> _groups;

            internal EventListener(
                IEventListener<EventGrouping<TKey, TEvent>> next,
                Func<TEvent, TKey> keySelector,
                IEqualityComparer<TKey> equalityComparer)
                : base( next )
            {
                _keySelector = keySelector;
                _groups = new Dictionary<TKey, GrowingBuffer<TEvent>>( equalityComparer );
            }

            public override void React(TEvent @event)
            {
                var key = _keySelector( @event );

                if ( ! _groups.TryGetValue( key, out var group ) )
                {
                    group = new GrowingBuffer<TEvent>();
                    _groups.Add( key, group );
                }

                group.Add( @event );

                var eventGrouping = new EventGrouping<TKey, TEvent>( key, @event, group.AsMemory() );
                Next.React( eventGrouping );
            }

            public override void OnDispose(DisposalSource source)
            {
                foreach ( var (_, group) in _groups )
                    group.Clear();

                _groups.Clear();
                _groups.TrimExcess();

                base.OnDispose( source );
            }
        }
    }
}
