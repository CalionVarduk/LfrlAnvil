using System;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Chrono;

/// <inheritdoc />
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
