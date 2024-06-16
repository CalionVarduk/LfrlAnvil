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

using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with the first emitted event, unless more events have been emitted.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerSingleDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly IEventSubscriber _subscriber;
        private Optional<TEvent> _value;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber)
            : base( next )
        {
            _subscriber = subscriber;
            _value = Optional<TEvent>.Empty;
        }

        public override void React(TEvent @event)
        {
            if ( _value.HasValue )
            {
                _value = Optional<TEvent>.Empty;
                _subscriber.Dispose();
                return;
            }

            _value = new Optional<TEvent>( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _value.TryForward( Next );
            _value = Optional<TEvent>.Empty;

            base.OnDispose( source );
        }
    }
}
