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

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Skips the specified number of events at the beginning of the sequence before starting to notify the decorated event listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerSkipDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _count;

    /// <summary>
    /// Creates a new <see cref="EventListenerSkipDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="count">Number of events at the beginning of the sequence to skip.</param>
    public EventListenerSkipDecorator(int count)
    {
        _count = count;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return _count <= 0 ? listener : new EventListener( listener, _count );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly int _count;
        private int _skipped;

        internal EventListener(IEventListener<TEvent> next, int count)
            : base( next )
        {
            _count = count;
            _skipped = 0;
        }

        public override void React(TEvent @event)
        {
            if ( _skipped == _count )
            {
                Next.React( @event );
                return;
            }

            ++_skipped;
        }
    }
}
