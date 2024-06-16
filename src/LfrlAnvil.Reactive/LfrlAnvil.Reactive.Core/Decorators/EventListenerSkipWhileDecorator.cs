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
/// Skips events at the beginning of the sequence until an event passes the provided predicate,
/// before starting to notify the decorated event listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerSkipWhileDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Func<TEvent, bool> _predicate;

    /// <summary>
    /// Creates a new <see cref="EventListenerSkipWhileDecorator{TEvent}"/> instance,
    /// </summary>
    /// <param name="predicate">Predicate that skips events until the first event that passes it (returns <b>true</b>).</param>
    public EventListenerSkipWhileDecorator(Func<TEvent, bool> predicate)
    {
        _predicate = predicate;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _predicate );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Func<TEvent, bool> _predicate;
        private bool _isDone;

        internal EventListener(IEventListener<TEvent> next, Func<TEvent, bool> predicate)
            : base( next )
        {
            _isDone = false;
            _predicate = predicate;
        }

        public override void React(TEvent @event)
        {
            if ( _isDone )
            {
                Next.React( @event );
                return;
            }

            if ( _predicate( @event ) )
                return;

            _isDone = true;
            Next.React( @event );
        }
    }
}
