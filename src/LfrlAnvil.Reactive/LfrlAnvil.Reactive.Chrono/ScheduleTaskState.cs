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

using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a snapshot of schedule task's state.
/// </summary>
/// <param name="State">Underlying state.</param>
/// <param name="NextTimestamp">Next <see cref="Timestamp"/> at which this task should be invoked.</param>
/// <param name="Interval"><see cref="Duration"/> between subsequent task invocations.</param>
/// <param name="Repetitions">Number of repetitions of this task.</param>
/// <param name="IsDisposed">Specifies whether or not the task has been disposed.</param>
/// <typeparam name="TKey">Scheduler's key type.</typeparam>
public sealed record ScheduleTaskState<TKey>(
    ReactiveTaskSnapshot<IScheduleTask<TKey>> State,
    Timestamp? NextTimestamp,
    Duration Interval,
    int? Repetitions,
    bool IsDisposed
)
    where TKey : notnull
{
    /// <summary>
    /// Specifies whether or not this task repeats infinitely.
    /// </summary>
    public bool IsInfinite => Repetitions is null && ! IsDisposed;
}
