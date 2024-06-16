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

using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Represents a generic mutable event queue.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public interface IMutableEventQueue<TEvent, TPoint, TPointDelta> : IEventQueue<TEvent, TPoint, TPointDelta>
{
    /// <summary>
    /// Moves the <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/> forward.
    /// </summary>
    /// <param name="delta">
    /// Point delta to move the <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/> forward by.
    /// </param>
    void Move(TPointDelta delta);

    /// <summary>
    /// Removes all events from this queue.
    /// </summary>
    void Clear();

    /// <summary>
    /// Attempts to dequeue the next event that should be processed.
    /// </summary>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance or null when no events are waiting for processing.
    /// </returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? Dequeue();
}
