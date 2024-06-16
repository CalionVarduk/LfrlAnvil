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
/// Represents a snapshot of reactive task's state.
/// </summary>
/// <param name="Task">Reactive task.</param>
/// <param name="FirstInvocationTimestamp"><see cref="Timestamp"/> of the first invocation.</param>
/// <param name="LastInvocationTimestamp"><see cref="Timestamp"/> of the last invocation.</param>
/// <param name="TotalInvocations">Number of total task invocations.</param>
/// <param name="ActiveInvocations">Number of currently active invocations.</param>
/// <param name="CompletedInvocations">Number of completed invocations.</param>
/// <param name="DelayedInvocations">Number of invocations that have been delayed due to maximum concurrency limit.</param>
/// <param name="FailedInvocations">Number of completed invocations that ended with an error.</param>
/// <param name="CancelledInvocations">Number of completed invocations that ended due to cancellation.</param>
/// <param name="QueuedInvocations">Number of currently queued invocations due to maximum concurrency limit.</param>
/// <param name="MaxQueuedInvocations">Number of maximum queued invocations at the same time.</param>
/// <param name="ActiveTasks">Number of currently running tasks.</param>
/// <param name="MaxActiveTasks">Number of maximum running tasks at the same time.</param>
/// <param name="MinElapsedTime">Minimum amount of time taken to complete a single invocation.</param>
/// <param name="MaxElapsedTime">Maximum amount of time taken to complete a single invocation.</param>
/// <param name="AverageElapsedTime">Average amount of time taken to complete a single invocation.</param>
/// <typeparam name="TTask">Reactive task type.</typeparam>
public sealed record ReactiveTaskSnapshot<TTask>(
    TTask Task,
    Timestamp? FirstInvocationTimestamp,
    Timestamp? LastInvocationTimestamp,
    long TotalInvocations,
    long ActiveInvocations,
    long CompletedInvocations,
    long DelayedInvocations,
    long FailedInvocations,
    long CancelledInvocations,
    long QueuedInvocations,
    long MaxQueuedInvocations,
    long ActiveTasks,
    long MaxActiveTasks,
    Duration MinElapsedTime,
    Duration MaxElapsedTime,
    FloatingDuration AverageElapsedTime
)
    where TTask : notnull
{
    /// <summary>
    /// Number of skipped invocations.
    /// </summary>
    public long SkippedInvocations => TotalInvocations - ActiveInvocations - CompletedInvocations - QueuedInvocations - ActiveTasks;
}
