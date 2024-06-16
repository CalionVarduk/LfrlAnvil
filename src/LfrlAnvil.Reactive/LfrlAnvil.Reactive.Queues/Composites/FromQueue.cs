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

namespace LfrlAnvil.Reactive.Queues.Composites;

/// <summary>
/// Represents an <see cref="IEventQueue{TEvent,TPoint,TPointDelta}"/> event.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public readonly struct FromQueue<TEvent, TPoint, TPointDelta>
{
    /// <summary>
    /// Creates a new <see cref="FromQueue{TEvent,TPoint,TPointDelta}"/> instance.
    /// </summary>
    /// <param name="enqueued">Underlying dequeued event.</param>
    /// <param name="currentQueuePoint">Queue's <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/>.</param>
    /// <param name="delta">Point delta that the queue was moved by.</param>
    public FromQueue(EnqueuedEvent<TEvent, TPoint, TPointDelta> enqueued, TPoint currentQueuePoint, TPointDelta delta)
    {
        Enqueued = enqueued;
        CurrentQueuePoint = currentQueuePoint;
        Delta = delta;
    }

    /// <summary>
    /// Underlying dequeued event.
    /// </summary>
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> Enqueued { get; }

    /// <summary>
    /// Queue's <see cref="IReadOnlyEventQueue{TEvent,TPoint,TPointDelta}.CurrentPoint"/>.
    /// </summary>
    public TPoint CurrentQueuePoint { get; }

    /// <summary>
    /// Point delta that the queue was moved by.
    /// </summary>
    public TPointDelta Delta { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="FromQueue{TEvent,TPoint,TPointDelta}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{Enqueued} [queue: {CurrentQueuePoint} ({Delta} dt)]";
    }
}
