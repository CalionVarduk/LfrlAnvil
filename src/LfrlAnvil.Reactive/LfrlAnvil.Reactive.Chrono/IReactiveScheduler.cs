using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono;

public interface IReactiveScheduler<TKey> : IDisposable
    where TKey : notnull
{
    ReactiveSchedulerState State { get; }
    Duration DefaultInterval { get; }
    Duration SpinWaitDurationHint { get; }
    Timestamp StartTimestamp { get; }
    ITimestampProvider Timestamps { get; }
    IEqualityComparer<TKey> KeyComparer { get; }
    IReadOnlyCollection<TKey> TaskKeys { get; }

    [Pure]
    ScheduleTaskState<TKey>? TryGetTaskState(TKey key);

    void Start();
    Task StartAsync(TaskScheduler? scheduler = null);
    bool Schedule(IScheduleTask<TKey> task, Timestamp timestamp);
    bool Schedule(IScheduleTask<TKey> task, Timestamp firstTimestamp, Duration interval, int repetitions);
    bool ScheduleInfinite(IScheduleTask<TKey> task, Timestamp firstTimestamp, Duration interval);
    bool SetInterval(TKey key, Duration interval);
    bool SetRepetitions(TKey key, int repetitions);
    bool MakeInfinite(TKey key);
    bool SetNextTimestamp(TKey key, Timestamp timestamp);
    bool Remove(TKey key);
    void Clear();
}
