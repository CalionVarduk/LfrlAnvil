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
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Chrono;

/// <inheritdoc cref="IScheduleTask{TKey}" />
public abstract class ScheduleTask<TKey> : IScheduleTask<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Creates a new <see cref="ScheduleTask{TKey}"/> instance.
    /// </summary>
    /// <param name="key">Underlying key.</param>
    /// <param name="maxEnqueuedInvocations">
    /// Specifies the maximum number of invocations that can be enqueued due to maximum concurrency. Equal to <b>0</b> by default.
    /// </param>
    /// <param name="maxConcurrentInvocations">
    /// Specifies the maximum number of concurrently running invocations. Equal to <b>1</b> by default.
    /// </param>
    protected ScheduleTask(TKey key, int maxEnqueuedInvocations = 0, int maxConcurrentInvocations = 1)
    {
        Key = key;
        MaxEnqueuedInvocations = Math.Max( maxEnqueuedInvocations, 0 );
        MaxConcurrentInvocations = Math.Max( maxConcurrentInvocations, 1 );
    }

    /// <inheritdoc />
    public TKey Key { get; }

    /// <inheritdoc />
    public int MaxEnqueuedInvocations { get; }

    /// <inheritdoc />
    public int MaxConcurrentInvocations { get; }

    /// <inheritdoc />
    public virtual void Dispose() { }

    /// <inheritdoc />
    public abstract Task InvokeAsync(
        IReactiveScheduler<TKey> scheduler,
        ReactiveTaskInvocationParams parameters,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual void OnCompleted(IReactiveScheduler<TKey> scheduler, ReactiveTaskCompletionParams parameters) { }

    /// <inheritdoc />
    public virtual bool OnEnqueue(IReactiveScheduler<TKey> scheduler, ReactiveTaskInvocationParams parameters, int positionInQueue)
    {
        return true;
    }
}
