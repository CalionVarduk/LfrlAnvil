// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Groups emitted events by their keys and notifies the decorated event listener with an <see cref="EventGrouping{TKey,TEvent}"/>
/// instance to which the emitted event belongs to.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TKey">Event's key type.</typeparam>
public sealed class EventListenerGroupByDecorator<TEvent, TKey> : IEventListenerDecorator<TEvent, EventGrouping<TKey, TEvent>>
    where TKey : notnull
{
    private readonly Func<TEvent, TKey> _keySelector;
    private readonly IEqualityComparer<TKey> _equalityComparer;

    /// <summary>
    /// Creates a new <see cref="EventListenerGroupByDecorator{TEvent,TKey}"/> instance.
    /// </summary>
    /// <param name="keySelector">Event's key selector.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    public EventListenerGroupByDecorator(Func<TEvent, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
    {
        _keySelector = keySelector;
        _equalityComparer = equalityComparer;
    }

    /// <inheritdoc />
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

            ref var group = ref CollectionsMarshal.GetValueRefOrAddDefault( _groups, key, out var exists )!;
            if ( ! exists )
                group = new GrowingBuffer<TEvent>();

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
