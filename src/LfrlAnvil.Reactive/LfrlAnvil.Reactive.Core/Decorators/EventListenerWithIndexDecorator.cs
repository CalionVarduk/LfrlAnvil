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
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener with <see cref="WithIndex{TEvent}"/> instances whose indexes are incremented by <b>1</b>
/// during each emitted event handling, starting from <b>0</b>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerWithIndexDecorator<TEvent> : IEventListenerDecorator<TEvent, WithIndex<TEvent>>
{
    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<WithIndex<TEvent>> listener, IEventSubscriber _)
    {
        return new EventListener( listener );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, WithIndex<TEvent>>
    {
        private int _index;

        internal EventListener(IEventListener<WithIndex<TEvent>> next)
            : base( next )
        {
            _index = -1;
        }

        public override void React(TEvent @event)
        {
            Next.React( new WithIndex<TEvent>( @event, ++_index ) );
        }
    }
}
