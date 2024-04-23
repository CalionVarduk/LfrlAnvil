using System;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

public interface ITimerTask<TKey> : IDisposable
    where TKey : notnull
{
    TKey Key { get; }
    int MaxEnqueuedInvocations { get; }
    int MaxConcurrentInvocations { get; }
    Timestamp? NextInvocationTimestamp { get; }
    Task InvokeAsync(TimerTaskCollection<TKey> source, ReactiveTaskInvocationParams parameters, CancellationToken cancellationToken);
    void OnCompleted(TimerTaskCollection<TKey> source, ReactiveTaskCompletionParams parameters);
    bool OnEnqueue(TimerTaskCollection<TKey> source, ReactiveTaskInvocationParams parameters, int positionInQueue);
}
