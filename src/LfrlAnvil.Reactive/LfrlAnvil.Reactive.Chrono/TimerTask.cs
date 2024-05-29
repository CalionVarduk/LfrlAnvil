using System;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

/// <inheritdoc cref="ITimerTask{TKey}" />
public abstract class TimerTask<TKey> : ITimerTask<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Creates a new <see cref="TimerTask{TKey}"/> instance.
    /// </summary>
    /// <param name="key">Underlying key.</param>
    /// <param name="nextInvocationTimestamp">
    /// Specifies the <see cref="Timestamp"/> of the next task invocation.
    /// Lack of value means that the task will be invoked on every event stream's event. Equal to null by default.
    /// </param>
    /// <param name="maxEnqueuedInvocations">
    /// Specifies the maximum number of invocations that can be enqueued due to maximum concurrency. Equal to <b>0</b> by default.
    /// </param>
    /// <param name="maxConcurrentInvocations">
    /// Specifies the maximum number of concurrently running invocations. Equal to <b>1</b> by default.
    /// </param>
    protected TimerTask(
        TKey key,
        Timestamp? nextInvocationTimestamp = null,
        int maxEnqueuedInvocations = 0,
        int maxConcurrentInvocations = 1)
    {
        Key = key;
        MaxEnqueuedInvocations = Math.Max( maxEnqueuedInvocations, 0 );
        MaxConcurrentInvocations = Math.Max( maxConcurrentInvocations, 1 );
        NextInvocationTimestamp = nextInvocationTimestamp;
    }

    /// <inheritdoc />
    public TKey Key { get; }

    /// <inheritdoc />
    public int MaxEnqueuedInvocations { get; }

    /// <inheritdoc />
    public int MaxConcurrentInvocations { get; }

    /// <inheritdoc />
    public Timestamp? NextInvocationTimestamp { get; protected set; }

    /// <inheritdoc />
    public virtual void Dispose() { }

    /// <inheritdoc />
    public abstract Task InvokeAsync(
        TimerTaskCollection<TKey> source,
        ReactiveTaskInvocationParams parameters,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public virtual void OnCompleted(TimerTaskCollection<TKey> source, ReactiveTaskCompletionParams parameters) { }

    /// <inheritdoc />
    public virtual bool OnEnqueue(TimerTaskCollection<TKey> source, ReactiveTaskInvocationParams parameters, int positionInQueue)
    {
        return true;
    }
}
