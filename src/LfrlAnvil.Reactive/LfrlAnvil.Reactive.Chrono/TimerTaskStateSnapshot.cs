using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

public sealed record TimerTaskStateSnapshot<TKey>(
    ITimerTask<TKey> Task,
    Timestamp? FirstInvocationTimestamp,
    Timestamp? LastInvocationTimestamp,
    long TotalInvocations,
    long CompletedInvocations,
    long DelayedInvocations,
    long FailedInvocations,
    long CancelledInvocations,
    long QueuedInvocations,
    long MaxQueuedInvocations,
    long ActiveTasks,
    long MaxActiveTasks,
    Duration MinElapsedTime,
    Duration MaxElapsedTime,
    FloatingDuration AverageElapsedTime
)
    where TKey : notnull
{
    public long SkippedInvocations => TotalInvocations - CompletedInvocations - QueuedInvocations - ActiveTasks;
}
