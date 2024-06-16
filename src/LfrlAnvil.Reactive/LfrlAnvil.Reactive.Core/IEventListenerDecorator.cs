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

namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a decorator of <see cref="IEventListener{TEvent}"/> instances.
/// </summary>
/// <typeparam name="TSourceEvent">Source event type.</typeparam>
/// <typeparam name="TNextEvent">Next event type.</typeparam>
public interface IEventListenerDecorator<in TSourceEvent, out TNextEvent>
{
    /// <summary>
    /// Creates a new decorated <see cref="IEventListener{TEvent}"/> instance.
    /// </summary>
    /// <param name="listener">Source event listener.</param>
    /// <param name="subscriber">Event subscriber.</param>
    /// <returns>Decorated <see cref="IEventListener{TEvent}"/> instance.</returns>
    IEventListener<TSourceEvent> Decorate(IEventListener<TNextEvent> listener, IEventSubscriber subscriber);
}
