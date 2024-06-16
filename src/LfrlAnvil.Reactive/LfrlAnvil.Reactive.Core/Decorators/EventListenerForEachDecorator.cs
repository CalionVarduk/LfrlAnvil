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
/// Notifies the decorated event listener with all emitted events and invokes the provided delegate for each event.
/// Delegate invocation happens before the decorated event listener gets notified.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerForEachDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Action<TEvent> _action;

    /// <summary>
    /// Creates a new <see cref="EventListenerForEachDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="action">Delegate to invoke on each event.</param>
    public EventListenerForEachDecorator(Action<TEvent> action)
    {
        _action = action;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _action );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Action<TEvent> _action;

        internal EventListener(IEventListener<TEvent> next, Action<TEvent> action)
            : base( next )
        {
            _action = action;
        }

        public override void React(TEvent @event)
        {
            _action( @event );
            Next.React( @event );
        }
    }
}
