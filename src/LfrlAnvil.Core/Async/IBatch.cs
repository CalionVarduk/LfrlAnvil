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
using System.Collections.Generic;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents a queue of elements, which are processed in batches, automatically or on demand.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IBatch<T>
{
    /// <summary>
    /// Specifies the number of elements currently waiting for processing in this batch.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Specifies the strategy to use when handling overflow of enqueued elements.
    /// </summary>
    /// <remarks>
    /// See <see cref="QueueSizeLimitHint"/> and <see cref="BatchQueueOverflowStrategy"/> for more details.
    /// </remarks>
    BatchQueueOverflowStrategy QueueOverflowStrategy { get; }

    /// <summary>
    /// Specifies the number of enqueued elements, which acts as a threshold that, when reached while adding new elements,
    /// will cause this batch to automatically <see cref="Flush()"/> itself.
    /// </summary>
    int AutoFlushCount { get; }

    /// <summary>
    /// Specifies the maximum number of enqueued elements that, when exceeded while adding new elements,
    /// will cause this batch to react according to its <see cref="QueueOverflowStrategy"/>.
    /// </summary>
    /// <remarks>
    /// See <see cref="BatchQueueOverflowStrategy"/> for more details.
    /// </remarks>
    int QueueSizeLimitHint { get; }

    /// <summary>
    /// Adds the provided element to this batch's queue.
    /// </summary>
    /// <param name="item">Element to enqueue.</param>
    /// <returns><b>true</b> when element was enqueued successfully, otherwise <b>false</b>.</returns>
    bool Add(T item);

    /// <summary>
    /// Adds the provided range of elements to this batch's queue.
    /// </summary>
    /// <param name="items">Range of elements to enqueue.</param>
    /// <returns><b>true</b> when elements were enqueued successfully, otherwise <b>false</b>.</returns>
    bool AddRange(ReadOnlySpan<T> items);

    /// <summary>
    /// Adds the provided range of elements to this batch's queue.
    /// </summary>
    /// <param name="items">Range of elements to enqueue.</param>
    /// <returns><b>true</b> when elements were enqueued successfully, otherwise <b>false</b>.</returns>
    bool AddRange(IEnumerable<T> items);

    /// <summary>
    /// Signals this batch to dequeue all of its elements, in an attempt to process them.
    /// </summary>
    /// <returns><b>true</b> when operation was initiated successfully, otherwise <b>false</b>.</returns>
    bool Flush();
}
