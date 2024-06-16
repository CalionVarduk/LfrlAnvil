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
using LfrlAnvil.Reactive.Queues.Composites;

namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Represents a generic event queue that allows to modify registered events.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPoint">Queue point type.</typeparam>
/// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
public interface IReorderableEventQueue<TEvent, TPoint, TPointDelta>
    : IEventQueue<TEvent, TPoint, TPointDelta>, IReadOnlyReorderableEventQueue<TEvent, TPoint, TPointDelta>
{
    /// <summary>
    /// Modifies the existing <paramref name="event"/> by changing the <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/>.
    /// </summary>
    /// <param name="event">Event to modify.</param>
    /// <param name="dequeuePoint">Next queue point at which this event should be dequeued.</param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the modified <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetDequeuePoint(TEvent @event, TPoint dequeuePoint);

    /// <summary>
    /// Modifies the existing <paramref name="event"/> by moving the <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/>
    /// forward by the specified <paramref name="delta"/>.
    /// </summary>
    /// <param name="event">Event to modify.</param>
    /// <param name="delta">
    /// Point delta to move the current <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/> forward by.
    /// </param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the modified <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? DelayDequeuePoint(TEvent @event, TPointDelta delta);

    /// <summary>
    /// Modifies the existing <paramref name="event"/> by moving the <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/>
    /// backward by the specified <paramref name="delta"/>.
    /// </summary>
    /// <param name="event">Event to modify.</param>
    /// <param name="delta">
    /// Point delta to move the current <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/> backward by.
    /// </param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the modified <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? AdvanceDequeuePoint(TEvent @event, TPointDelta delta);

    /// <summary>
    /// Modifies the existing <paramref name="event"/> by changing the <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.Repetitions"/>.
    /// </summary>
    /// <param name="event">Event to modify.</param>
    /// <param name="repetitions">Number of repetitions.</param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the modified <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="repetitions"/> is less than <b>1</b>.</exception>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetRepetitions(TEvent @event, int repetitions);

    /// <summary>
    /// Modifies the existing <paramref name="event"/> by adding <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.Repetitions"/>.
    /// </summary>
    /// <param name="event">Event to modify.</param>
    /// <param name="count">Number of repetitions to add.</param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the modified <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">When new number of repetitions is less than <b>1</b>.</exception>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? IncreaseRepetitions(TEvent @event, int count);

    /// <summary>
    /// Modifies the existing <paramref name="event"/> by subtracting <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.Repetitions"/>.
    /// </summary>
    /// <param name="event">Event to modify.</param>
    /// <param name="count">Number of repetitions to subtract.</param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the modified <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">When new number of repetitions is less than <b>1</b>.</exception>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? DecreaseRepetitions(TEvent @event, int count);

    /// <summary>
    /// Modifies the existing <paramref name="event"/> by making it repeat infinitely.
    /// </summary>
    /// <param name="event">Event to modify.</param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the modified <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? MakeInfinite(TEvent @event);

    /// <summary>
    /// Modifies the existing <paramref name="event"/> by changing the <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.Delta"/>.
    /// </summary>
    /// <param name="event">Event to modify.</param>
    /// <param name="delta">
    /// Point delta used for moving <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.DequeuePoint"/>
    /// forward on each repetition of this event.
    /// </param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the modified <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetDelta(TEvent @event, TPointDelta delta);

    /// <summary>
    /// Modifies the existing <paramref name="event"/> by adding a value to <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.Delta"/>.
    /// </summary>
    /// <param name="event">Event to modify.</param>
    /// <param name="delta">Point delta to add.</param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the modified <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? IncreaseDelta(TEvent @event, TPointDelta delta);

    /// <summary>
    /// Modifies the existing <paramref name="event"/> by subtracting a value from
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}.Delta"/>.
    /// </summary>
    /// <param name="event">Event to modify.</param>
    /// <param name="delta">Point delta to subtract.</param>
    /// <returns>
    /// <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance associated with the modified <paramref name="event"/>
    /// or null when event does not exist.
    /// </returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? DecreaseDelta(TEvent @event, TPointDelta delta);

    /// <summary>
    /// Attempts to remove the specified <paramref name="event"/>.
    /// </summary>
    /// <param name="event">Event to remove.</param>
    /// <returns>
    /// Removed <see cref="EnqueuedEvent{TEvent,TPoint,TPointDelta}"/> instance or null when <paramref name="event"/> does not exist.
    /// </returns>
    EnqueuedEvent<TEvent, TPoint, TPointDelta>? Remove(TEvent @event);
}
