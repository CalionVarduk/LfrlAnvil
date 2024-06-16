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
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Aggregates emitted events and notifies the decorated event listener with the result of aggregation on each event.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerAggregateDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Optional<TEvent> _seed;
    private readonly Func<TEvent, TEvent, TEvent> _func;

    /// <summary>
    /// Creates a new <see cref="EventListenerAggregateDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="func">Aggregator delegate.</param>
    public EventListenerAggregateDecorator(Func<TEvent, TEvent, TEvent> func)
    {
        _seed = Optional<TEvent>.Empty;
        _func = func;
    }

    /// <summary>
    /// Creates a new <see cref="EventListenerAggregateDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="func">Aggregator delegate.</param>
    /// <param name="seed">Initial event to publish immediately.</param>
    public EventListenerAggregateDecorator(Func<TEvent, TEvent, TEvent> func, TEvent seed)
    {
        _seed = new Optional<TEvent>( seed );
        _func = func;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _func, _seed );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Func<TEvent, TEvent, TEvent> _func;
        private Optional<TEvent> _result;

        internal EventListener(IEventListener<TEvent> next, Func<TEvent, TEvent, TEvent> func, Optional<TEvent> seed)
            : base( next )
        {
            _result = seed;
            _func = func;
            _result.TryForward( Next );
        }

        public override void React(TEvent @event)
        {
            _result = _result.HasValue
                ? new Optional<TEvent>( _func( _result.Event!, @event ) )
                : new Optional<TEvent>( @event );

            Next.React( _result.Event! );
        }

        public override void OnDispose(DisposalSource source)
        {
            _result = Optional<TEvent>.Empty;
            base.OnDispose( source );
        }
    }
}
