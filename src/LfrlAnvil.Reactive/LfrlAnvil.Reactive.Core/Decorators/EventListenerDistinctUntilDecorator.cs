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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with distinct emitted events, excluding duplicates, until the target event stream
/// emits its own event, which causes the underlying distinct keys tracker to be reset.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TKey">Event's key type.</typeparam>
/// <typeparam name="TTargetEvent">Target event type.</typeparam>
public sealed class EventListenerDistinctUntilDecorator<TEvent, TKey, TTargetEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Func<TEvent, TKey> _keySelector;
    private readonly IEqualityComparer<TKey> _equalityComparer;
    private readonly IEventStream<TTargetEvent> _target;

    /// <summary>
    /// Creates a new <see cref="EventListenerDistinctUntilDecorator{TEvent,TKey,TTargetEvent}"/> instance.
    /// </summary>
    /// <param name="keySelector">Event key selector.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    /// <param name="target">Target event stream whose events cause the underlying distinct keys tracker to be reset.</param>
    public EventListenerDistinctUntilDecorator(
        Func<TEvent, TKey> keySelector,
        IEqualityComparer<TKey> equalityComparer,
        IEventStream<TTargetEvent> target)
    {
        _keySelector = keySelector;
        _equalityComparer = equalityComparer;
        _target = target;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _keySelector, _equalityComparer, _target );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly IEventSubscriber _targetSubscriber;
        private readonly Func<TEvent, TKey> _keySelector;
        private readonly HashSet<TKey> _keySet;

        internal EventListener(
            IEventListener<TEvent> next,
            Func<TEvent, TKey> keySelector,
            IEqualityComparer<TKey> equalityComparer,
            IEventStream<TTargetEvent> target)
            : base( next )
        {
            _keySelector = keySelector;
            _keySet = new HashSet<TKey>( equalityComparer );
            var targetListener = new TargetEventListener( this );
            _targetSubscriber = target.Listen( targetListener );
        }

        public override void React(TEvent @event)
        {
            if ( _keySet.Add( _keySelector( @event ) ) )
                Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _targetSubscriber.Dispose();
            _keySet.Clear();
            _keySet.TrimExcess();

            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTargetEvent()
        {
            _keySet.Clear();
        }
    }

    private sealed class TargetEventListener : EventListener<TTargetEvent>
    {
        private EventListener? _sourceListener;

        internal TargetEventListener(EventListener sourceListener)
        {
            _sourceListener = sourceListener;
        }

        public override void React(TTargetEvent _)
        {
            Assume.IsNotNull( _sourceListener );
            _sourceListener.OnTargetEvent();
        }

        public override void OnDispose(DisposalSource _)
        {
            Assume.IsNotNull( _sourceListener );
            _sourceListener.OnTargetEvent();
            _sourceListener = null;
        }
    }
}
