using System;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a reactive task that can be registered in <see cref="ReactiveScheduler{TKey}"/>.
/// </summary>
/// <typeparam name="TKey">Task key type.</typeparam>
public interface IScheduleTask<TKey> : IDisposable
    where TKey : notnull
{
    /// <summary>
    /// Underlying key.
    /// </summary>
    TKey Key { get; }

    /// <summary>
    /// Specifies the maximum number of invocations that can be enqueued due to maximum concurrency.
    /// </summary>
    int MaxEnqueuedInvocations { get; }

    /// <summary>
    /// Specifies the maximum number of concurrently running invocations.
    /// </summary>
    int MaxConcurrentInvocations { get; }

    /// <summary>
    /// Invokes the task.
    /// </summary>
    /// <param name="scheduler">Source scheduler.</param>
    /// <param name="parameters">Invocation parameters.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> instance.</param>
    /// <returns></returns>
    Task InvokeAsync(IReactiveScheduler<TKey> scheduler, ReactiveTaskInvocationParams parameters, CancellationToken cancellationToken);

    /// <summary>
    /// Callback for task invocation completion.
    /// </summary>
    /// <param name="scheduler">Source scheduler.</param>
    /// <param name="parameters">Completion parameters.</param>
    void OnCompleted(IReactiveScheduler<TKey> scheduler, ReactiveTaskCompletionParams parameters);

    /// <summary>
    /// Callback invoked before the task invocation gets enqueued due to maximum concurrency.
    /// </summary>
    /// <param name="scheduler">Source scheduler.</param>
    /// <param name="parameters">Invocation parameters.</param>
    /// <param name="positionInQueue">Invocation's position in queue.</param>
    /// <returns><b>true</b> to proceed with enqueueing, <b>false</b> to cancel the invocation.</returns>
    bool OnEnqueue(IReactiveScheduler<TKey> scheduler, ReactiveTaskInvocationParams parameters, int positionInQueue);
}
