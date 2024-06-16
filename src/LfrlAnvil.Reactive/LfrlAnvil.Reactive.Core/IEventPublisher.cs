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
/// Represents a type-erased disposable event publisher that can be listened to.
/// </summary>
public interface IEventPublisher : IEventSource
{
    /// <summary>
    /// Publishes an event that notifies all current event listeners.
    /// </summary>
    /// <param name="event">Event to publish.</param>
    void Publish(object? @event);
}

/// <summary>
/// Represents a generic disposable event source that can be listened to.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public interface IEventPublisher<TEvent> : IEventSource<TEvent>, IEventPublisher
{
    /// <summary>
    /// Publishes an event that notifies all current event listeners.
    /// </summary>
    /// <param name="event">Event to publish.</param>
    void Publish(TEvent @event);
}
