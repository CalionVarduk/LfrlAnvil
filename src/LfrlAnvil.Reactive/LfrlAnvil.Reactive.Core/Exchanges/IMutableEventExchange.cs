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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Exchanges;

/// <summary>
///  Represents a mutable collection of event publishers identifiable by their event types.
/// </summary>
public interface IMutableEventExchange : IEventExchange, IDisposable
{
    /// <summary>
    /// Returns an event publisher associated with the provided <paramref name="eventType"/>.
    /// </summary>
    /// <param name="eventType">Event type.</param>
    /// <returns>Registered <see cref="IEventPublisher"/> instance.</returns>
    /// <exception cref="EventPublisherNotFoundException">When event publisher does not exist.</exception>
    [Pure]
    IEventPublisher GetPublisher(Type eventType);

    /// <summary>
    /// Attempts to return an event publisher associated with the provided <paramref name="eventType"/>.
    /// </summary>
    /// <param name="eventType">Event type.</param>
    /// <param name="result"><b>out</b> parameter that returns the registered <see cref="IEventPublisher"/> instance.</param>
    /// <returns><b>true</b> when event publisher exists, otherwise <b>false</b>.</returns>
    bool TryGetPublisher(Type eventType, [MaybeNullWhen( false )] out IEventPublisher result);

    /// <summary>
    /// Registers the provided event publisher.
    /// </summary>
    /// <param name="publisher">Event publisher to register.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Registered event publisher.</returns>
    /// <exception cref="EventPublisherAlreadyExistsException">When event publisher for the given event type already exists.</exception>
    IEventPublisher<TEvent> RegisterPublisher<TEvent>(IEventPublisher<TEvent> publisher);
}
