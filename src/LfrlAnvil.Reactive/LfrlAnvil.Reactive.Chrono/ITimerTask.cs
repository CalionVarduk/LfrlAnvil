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
    Task InvokeAsync(long invocationId, Timestamp invocationTimestamp, Timestamp currentTimestamp, CancellationToken cancellationToken);
    void OnCompleted(long invocationId, Duration elapsedTime, Exception? exception, bool isCancelled);
}

public interface ITimerTask<out TKey> : ITimerTask
    where TKey : notnull
{
    TKey Key { get; }
}
