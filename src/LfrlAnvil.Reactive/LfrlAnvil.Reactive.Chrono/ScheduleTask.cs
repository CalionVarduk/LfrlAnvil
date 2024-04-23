using System;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Chrono;

public abstract class ScheduleTask<TKey> : IScheduleTask<TKey>
    where TKey : notnull
{
    protected ScheduleTask(TKey key, int maxEnqueuedInvocations = 0, int maxConcurrentInvocations = 1)
    {
        Key = key;
        MaxEnqueuedInvocations = Math.Max( maxEnqueuedInvocations, 0 );
        MaxConcurrentInvocations = Math.Max( maxConcurrentInvocations, 1 );
    }

    public TKey Key { get; }
    public int MaxEnqueuedInvocations { get; }
    public int MaxConcurrentInvocations { get; }

    public virtual void Dispose() { }

    public abstract Task InvokeAsync(
        IReactiveScheduler<TKey> scheduler,
        ReactiveTaskInvocationParams parameters,
        CancellationToken cancellationToken);

    public virtual void OnCompleted(IReactiveScheduler<TKey> scheduler, ReactiveTaskCompletionParams parameters) { }

    public virtual bool OnEnqueue(IReactiveScheduler<TKey> scheduler, ReactiveTaskInvocationParams parameters, int positionInQueue)
    {
        return true;
    }
}
