using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

public sealed record ReactiveTaskSnapshot<TTask>(
    TTask Task,
    Timestamp? FirstInvocationTimestamp,
    Timestamp? LastInvocationTimestamp,
    long TotalInvocations,
    long ActiveInvocations,
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
    where TTask : notnull
{
    public long SkippedInvocations => TotalInvocations - ActiveInvocations - CompletedInvocations - QueuedInvocations - ActiveTasks;
}
