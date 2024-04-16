using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

public sealed record TimerTaskStateSnapshot(
    ITimerTask Task,
    Timestamp? FirstInvocationTimestamp,
    Timestamp? LastInvocationTimestamp,
    long TotalInvocations,
    long DelayedInvocations,
    long SkippedInvocations,
    long FailedInvocations,
    long CancelledInvocations,
    long QueuedInvocations,
    long MaxQueuedInvocations,
    long ActiveTasks,
    long MaxActiveTasks,
    Duration MinElapsedTime,
    Duration MaxElapsedTime,
    FloatingDuration AverageElapsedTime
);
