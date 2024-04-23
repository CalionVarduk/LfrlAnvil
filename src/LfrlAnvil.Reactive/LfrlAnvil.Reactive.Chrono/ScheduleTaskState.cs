using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

public sealed record ScheduleTaskState<TKey>(
    ReactiveTaskSnapshot<IScheduleTask<TKey>> State,
    Timestamp? NextTimestamp,
    Duration Interval,
    int? Repetitions,
    bool IsDisposed
)
    where TKey : notnull
{
    public bool IsInfinite => Repetitions is null && ! IsDisposed;
}
