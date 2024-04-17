using System;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

public interface ITimerTask : IDisposable
{
    int MaxEnqueuedInvocations { get; }
    int MaxConcurrentInvocations { get; }
    Timestamp? NextInvocationTimestamp { get; }
    Task InvokeAsync(ReactiveTaskInvocationParams parameters, CancellationToken cancellationToken);
    void OnCompleted(ReactiveTaskCompletionParams parameters);
}

public interface ITimerTask<out TKey> : ITimerTask
    where TKey : notnull
{
    TKey Key { get; }
}
