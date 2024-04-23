using System;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Chrono;

public interface IScheduleTask<TKey> : IDisposable
    where TKey : notnull
{
    TKey Key { get; }
    int MaxEnqueuedInvocations { get; }
    int MaxConcurrentInvocations { get; }
    Task InvokeAsync(IReactiveScheduler<TKey> scheduler, ReactiveTaskInvocationParams parameters, CancellationToken cancellationToken);
    void OnCompleted(IReactiveScheduler<TKey> scheduler, ReactiveTaskCompletionParams parameters);
    bool OnEnqueue(IReactiveScheduler<TKey> scheduler, ReactiveTaskInvocationParams parameters, int positionInQueue);
}
