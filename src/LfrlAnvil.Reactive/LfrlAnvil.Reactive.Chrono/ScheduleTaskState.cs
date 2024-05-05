using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a snapshot of schedule task's state.
/// </summary>
/// <param name="State">Underlying state.</param>
/// <param name="NextTimestamp">Next <see cref="Timestamp"/> at which this task should be invoked.</param>
/// <param name="Interval"><see cref="Duration"/> between subsequent task invocations.</param>
/// <param name="Repetitions">Number of repetitions of this task.</param>
/// <param name="IsDisposed">Specifies whether or not the task has been disposed.</param>
/// <typeparam name="TKey">Scheduler's key type.</typeparam>
public sealed record ScheduleTaskState<TKey>(
    ReactiveTaskSnapshot<IScheduleTask<TKey>> State,
    Timestamp? NextTimestamp,
    Duration Interval,
    int? Repetitions,
    bool IsDisposed
)
    where TKey : notnull
{
    /// <summary>
    /// Specifies whether or not this task repeats infinitely.
    /// </summary>
    public bool IsInfinite => Repetitions is null && ! IsDisposed;
}
