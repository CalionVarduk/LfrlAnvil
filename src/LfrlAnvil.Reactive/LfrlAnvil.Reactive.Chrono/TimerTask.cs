using System;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

public abstract class TimerTask<TKey> : ITimerTask<TKey>
    where TKey : notnull
{
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

    public TKey Key { get; }
    public int MaxEnqueuedInvocations { get; }
    public int MaxConcurrentInvocations { get; }
    public Timestamp? NextInvocationTimestamp { get; protected set; }

    public virtual void Dispose() { }

    public abstract Task InvokeAsync(
        long invocationId,
        Timestamp invocationTimestamp,
        Timestamp currentTimestamp,
        CancellationToken cancellationToken);

    public virtual void OnCompleted(long invocationId, Duration elapsedTime, Exception? exception, bool isCancelled) { }
}
