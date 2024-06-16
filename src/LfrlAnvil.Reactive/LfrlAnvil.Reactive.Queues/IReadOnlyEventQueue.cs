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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Represents a generic read-only event queue.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public interface IReadOnlyEventQueue<TEvent, TPoint, TPointDelta> : IReadOnlyCollection<EnqueuedEvent<TEvent, TPoint, TPointDelta>>
{
    /// <summary>
    /// Specifies the starting point of this queue.
    /// </summary>
    TPoint StartPoint { get; }

    /// <summary>
    /// Specifies the current point that this queue is in.
    /// </summary>
    TPoint CurrentPoint { get; }

    /// <summary>
    /// Queue point comparer.
    /// </summary>
    IComparer<TPoint> Comparer { get; }

    /// <summary>
    /// Attempts to return information about the next event to happen.
    /// </summary>
    /// <returns><see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance or null when this queue is empty.</returns>
    [Pure]
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? GetNext();

    /// <summary>
    /// Returns information about all currently registered events in this queue,
    /// from <see cref="CurrentPoint"/> to the specified <paramref name="endPoint"/>.
    /// </summary>
    /// <param name="endPoint">Largest event point to include in the result.</param>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    IEnumerable<EnqueuedEvent<TEvent, TPoint, TPointDelta>> GetEvents(TPoint endPoint);
}
