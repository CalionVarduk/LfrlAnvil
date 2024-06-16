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

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Maps emitted event to another event and notifies the decorated event listener with the result of that mapping.
/// </summary>
/// <typeparam name="TSourceEvent">Source event type.</typeparam>
/// <typeparam name="TNextEvent">Next event type.</typeparam>
public sealed class EventListenerSelectDecorator<TSourceEvent, TNextEvent> : IEventListenerDecorator<TSourceEvent, TNextEvent>
{
    private readonly Func<TSourceEvent, TNextEvent> _selector;

    /// <summary>
    /// Creates a new <see cref="EventListenerSelectDecorator{TSourceEvent,TNextEvent}"/> instance.
    /// </summary>
    /// <param name="selector">Next event selector.</param>
    public EventListenerSelectDecorator(Func<TSourceEvent, TNextEvent> selector)
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
        private readonly Func<TSourceEvent, TNextEvent> _selector;

        internal EventListener(IEventListener<TNextEvent> next, Func<TSourceEvent, TNextEvent> selector)
            : base( next )
        {
            _selector = selector;
        }

        public override void React(TSourceEvent @event)
        {
            Next.React( _selector( @event ) );
        }
    }
}
