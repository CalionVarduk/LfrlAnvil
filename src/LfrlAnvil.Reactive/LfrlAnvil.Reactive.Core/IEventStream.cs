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

namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a type-erased event stream that can be listened to.
/// </summary>
public interface IEventStream
{
    /// <summary>
    /// Specifies whether or not this event stream has been disposed.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Attaches the provided <paramref name="listener"/> to this event stream.
    /// </summary>
    /// <param name="listener">Event listener to attach.</param>
    /// <returns>New <see cref="IEventSubscriber"/> instance that can be used to detach the <paramref name="listener"/>.</returns>
    IEventSubscriber Listen(IEventListener listener);
}

/// <summary>
/// Represents a generic event stream that can be listened to.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public interface IEventStream<out TEvent> : IEventStream
{
    /// <summary>
    /// Attaches the provided <paramref name="listener"/> to this event stream.
    /// </summary>
    /// <param name="listener">Event listener to attach.</param>
    /// <returns>New <see cref="IEventSubscriber"/> instance that can be used to detach the <paramref name="listener"/>.</returns>
    IEventSubscriber Listen(IEventListener<TEvent> listener);

    /// <summary>
    /// Creates a new decorated <see cref="IEventStream{TEvent}"/> instance.
    /// </summary>
    /// <param name="decorator">Event listener decorator.</param>
    /// <typeparam name="TNextEvent">Next event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator);
}
