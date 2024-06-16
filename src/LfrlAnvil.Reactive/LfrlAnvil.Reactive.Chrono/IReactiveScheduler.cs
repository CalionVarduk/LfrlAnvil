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
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a scheduler of <see cref="IScheduleTask{TKey}"/> instances.
/// </summary>
/// <typeparam name="TKey">Task key type.</typeparam>
public interface IReactiveScheduler<TKey> : IDisposable
    where TKey : notnull
{
    /// <summary>
    /// Specifies the current state of this scheduler.
    /// </summary>
    ReactiveSchedulerState State { get; }

    /// <summary>
    /// Maximum <see cref="Duration"/> to hang the underlying time tracking mechanism for.
    /// </summary>
    Duration DefaultInterval { get; }

    /// <summary>
    /// <see cref="SpinWait"/> duration hint for the underlying time tracking mechanism.
    /// </summary>
    Duration SpinWaitDurationHint { get; }

    /// <summary>
    /// <see cref="Timestamp"/> of creation of this scheduler.
    /// </summary>
    Timestamp StartTimestamp { get; }

    /// <summary>
    /// <see cref="ITimestampProvider"/> instance used for time tracking.
    /// </summary>
    ITimestampProvider Timestamps { get; }

    /// <summary>
    /// Key equality comparer.
    /// </summary>
    IEqualityComparer<TKey> KeyComparer { get; }

    /// <summary>
    /// Collection of registered task keys.
    /// </summary>
    IReadOnlyCollection<TKey> TaskKeys { get; }

    /// <summary>
    /// Attempts to create a <see cref="ScheduleTaskState{TTask}"/> instance for the given task <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Task key to create a snapshot for.</param>
    /// <returns>New <see cref="ScheduleTaskState{TTask}"/> instance or null when task does not exist.</returns>
    [Pure]
    ScheduleTaskState<TKey>? TryGetTaskState(TKey key);

    /// <summary>
    /// Starts this scheduler synchronously. Does nothing when this scheduler has already been started.
    /// </summary>
    void Start();

    /// <summary>
    /// Starts this scheduler asynchronously.
    /// </summary>
    /// <param name="scheduler">Optional task scheduler.</param>
    /// <returns>
    /// New <see cref="Task"/> instance that completes when this scheduler is done
    /// or <see cref="Task.CompletedTask"/> when this scheduler has already been started.
    /// </returns>
    Task StartAsync(TaskScheduler? scheduler = null);

    /// <summary>
    /// Attempts to schedule the provided <paramref name="task"/>.
    /// </summary>
    /// <param name="task">Task to schedule.</param>
    /// <param name="timestamp"><see cref="Timestamp"/> that specifies when the scheduled task should be invoked.</param>
    /// <returns><b>true</b> when task was scheduled, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// When task key already exists, then the entry will be updated only if the key is associated exactly
    /// with the provided <paramref name="task"/> and it is not in the process of being disposed.
    /// </remarks>
    bool Schedule(IScheduleTask<TKey> task, Timestamp timestamp);

    /// <summary>
    /// Attempts to schedule the provided <paramref name="task"/> that repeats the specified number of times.
    /// </summary>
    /// <param name="task">Task to schedule.</param>
    /// <param name="firstTimestamp">
    /// <see cref="Timestamp"/> that specifies when the scheduled task should be invoked for the first time.
    /// </param>
    /// <param name="interval">Interval between subsequent task invocations.</param>
    /// <param name="repetitions">Number of repetitions.</param>
    /// <returns><b>true</b> when task was scheduled, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// When task key already exists, then the entry will be updated only if the key is associated exactly
    /// with the provided <paramref name="task"/> and it is not in the process of being disposed.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="interval"/> is less than <b>1 tick</b>.</exception>
    bool Schedule(IScheduleTask<TKey> task, Timestamp firstTimestamp, Duration interval, int repetitions);

    /// <summary>
    /// Attempts to schedule the provided <paramref name="task"/> that repeats infinitely.
    /// </summary>
    /// <param name="task">Task to schedule.</param>
    /// <param name="firstTimestamp">
    /// <see cref="Timestamp"/> that specifies when the scheduled task should be invoked for the first time.
    /// </param>
    /// <param name="interval">Interval between subsequent task invocations.</param>
    /// <returns><b>true</b> when task was scheduled, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// When task key already exists, then the entry will be updated only if the key is associated exactly
    /// with the provided <paramref name="task"/> and it is not in the process of being disposed.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="interval"/> is less than <b>1 tick</b>.</exception>
    bool ScheduleInfinite(IScheduleTask<TKey> task, Timestamp firstTimestamp, Duration interval);

    /// <summary>
    /// Attempts to change the interval of the task associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Task key.</param>
    /// <param name="interval">Interval between subsequent task invocations.</param>
    /// <returns><b>true</b> when task was updated, otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="interval"/> is less than <b>1 tick</b>.</exception>
    bool SetInterval(TKey key, Duration interval);

    /// <summary>
    /// Attempts to change the number of repetitions of the task associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Task key.</param>
    /// <param name="repetitions">Number of repetitions.</param>
    /// <returns><b>true</b> when task was updated, otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="repetitions"/> is less than <b>1</b>.</exception>
    bool SetRepetitions(TKey key, int repetitions);

    /// <summary>
    /// Attempts to make the task associated with the specified <paramref name="key"/> repeat infinitely.
    /// </summary>
    /// <param name="key">Task key.</param>
    /// <returns><b>true</b> when task was updated, otherwise <b>false</b>.</returns>
    bool MakeInfinite(TKey key);

    /// <summary>
    /// Attempts to change the next timestamp of the task associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Task key.</param>
    /// <param name="timestamp"><see cref="Timestamp"/> that specifies when the scheduled task should be invoked next.</param>
    /// <returns><b>true</b> when task was updated, otherwise <b>false</b>.</returns>
    bool SetNextTimestamp(TKey key, Timestamp timestamp);

    /// <summary>
    /// Attempts to remove and dispose a task with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    /// <returns><b>true</b> when task was removed, otherwise <b>false</b>.</returns>
    bool Remove(TKey key);

    /// <summary>
    /// Removes and disposes all currently registered tasks.
    /// </summary>
    void Clear();
}
