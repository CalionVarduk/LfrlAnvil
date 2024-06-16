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
/// Does not notify the decorated event listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerIgnoreDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        internal EventListener(IEventListener<TEvent> next)
            : base( next ) { }

        public override void React(TEvent _) { }
    }
}
