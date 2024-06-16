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
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with emitted events, whose keys are different from the previously emitted event's key.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TKey">Event's key type.</typeparam>
public sealed class EventListenerDistinctUntilChangedDecorator<TEvent, TKey> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Func<TEvent, TKey> _keySelector;
    private readonly IEqualityComparer<TKey> _equalityComparer;

    /// <summary>
    /// Creates a new <see cref="EventListenerDistinctUntilChangedDecorator{TEvent,TKey}"/> instance.
    /// </summary>
    /// <param name="keySelector">Event key selector.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    public EventListenerDistinctUntilChangedDecorator(Func<TEvent, TKey> keySelector, IEqualityComparer<TKey> equalityComparer)
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
