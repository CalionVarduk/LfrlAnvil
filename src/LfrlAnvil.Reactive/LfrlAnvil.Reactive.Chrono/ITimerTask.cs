using System;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a reactive task that can be registered in an <see cref="IEventStream{TEvent}"/>
/// with <see cref="WithInterval{TEvent}"/> events.
/// </summary>
/// <typeparam name="TKey">Task key type.</typeparam>
public interface ITimerTask<TKey> : IDisposable
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
    /// Specifies the <see cref="Timestamp"/> of the next task invocation.
    /// Lack of value means that the task will be invoked on every event stream's event.
    /// </summary>
    Timestamp? NextInvocationTimestamp { get; }

    /// <summary>
    /// Invokes the task.
    /// </summary>
    /// <param name="source">Source timer task collection.</param>
    /// <param name="parameters">Invocation parameters.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> instance.</param>
    /// <returns></returns>
    Task InvokeAsync(TimerTaskCollection<TKey> source, ReactiveTaskInvocationParams parameters, CancellationToken cancellationToken);

    /// <summary>
    /// Callback for task invocation completion.
    /// </summary>
    /// <param name="source">Source timer task collection.</param>
    /// <param name="parameters">Completion parameters.</param>
    void OnCompleted(TimerTaskCollection<TKey> source, ReactiveTaskCompletionParams parameters);

    /// <summary>
    /// Callback invoked before the task invocation gets enqueued due to maximum concurrency.
    /// </summary>
    /// <param name="source">Source timer task collection.</param>
    /// <param name="parameters">Invocation parameters.</param>
    /// <param name="positionInQueue">Invocation's position in queue.</param>
    /// <returns><b>true</b> to proceed with enqueueing, <b>false</b> to cancel the invocation.</returns>
    bool OnEnqueue(TimerTaskCollection<TKey> source, ReactiveTaskInvocationParams parameters, int positionInQueue);
}
