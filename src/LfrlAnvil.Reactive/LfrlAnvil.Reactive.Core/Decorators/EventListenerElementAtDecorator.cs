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

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with a single emitted event at the specified 0-based index in a sequence of emitted events.
/// Does not notify the decorated event listener if no such event was emitted.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerElementAtDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _index;

    /// <summary>
    /// Creates a new <see cref="EventListenerElementAtDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="index">0-based position of the desired event.</param>
    public EventListenerElementAtDecorator(int index)
    {
        _index = index;
    }

    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _index );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly IEventSubscriber _subscriber;
        private readonly int _index;
        private int _currentIndex;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, int index)
            : base( next )
        {
            _subscriber = subscriber;
            _index = index;
            _currentIndex = -1;

            if ( _index < 0 )
                _subscriber.Dispose();
        }

        public override void React(TEvent @event)
        {
            if ( ++_currentIndex < _index )
                return;

            Next.React( @event );
            _subscriber.Dispose();
        }
    }
}
