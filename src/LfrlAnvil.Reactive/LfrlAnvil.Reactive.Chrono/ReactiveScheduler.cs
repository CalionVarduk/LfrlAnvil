using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Collections;
using LfrlAnvil.Extensions;
using LfrlAnvil.Reactive.Chrono.Internal;

namespace LfrlAnvil.Reactive.Chrono;

public class ReactiveScheduler<TKey> : IReactiveScheduler<TKey>
    where TKey : notnull
{
    private readonly DictionaryHeap<TKey, ReactiveSchedulerEntry<TKey>> _queue;
    private readonly ManualResetEventSlim _reset;
    private InterlockedEnum<ReactiveSchedulerState> _state;
    private Timestamp _nextEventTimestamp;

    public ReactiveScheduler(
        ITimestampProvider timestamps,
        IEqualityComparer<TKey>? keyComparer = null,
        Duration? defaultInterval = null,
        Duration? spinWaitDurationHint = null)
    {
        if ( defaultInterval is not null )
            Ensure.IsInRange( defaultInterval.Value, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );

        if ( spinWaitDurationHint is not null )
            Ensure.IsGreaterThanOrEqualTo( spinWaitDurationHint.Value, Duration.Zero );

        _state = new InterlockedEnum<ReactiveSchedulerState>( ReactiveSchedulerState.Created );
        DefaultInterval = defaultInterval ?? Duration.FromHours( 1 );
        SpinWaitDurationHint = spinWaitDurationHint ?? ReactiveTimer.DefaultSpinWaitDurationHint;
        Timestamps = timestamps;

        _queue = new DictionaryHeap<TKey, ReactiveSchedulerEntry<TKey>>(
            keyComparer ?? EqualityComparer<TKey>.Default,
            Comparer<ReactiveSchedulerEntry<TKey>>.Default );

        _nextEventTimestamp = new Timestamp( long.MinValue );
        _reset = new ManualResetEventSlim( false );
        StartTimestamp = Timestamps.GetNow();
    }

    public Timestamp StartTimestamp { get; }
    public Duration DefaultInterval { get; }
    public Duration SpinWaitDurationHint { get; }
    public ITimestampProvider Timestamps { get; }
    public IEqualityComparer<TKey> KeyComparer => _queue.KeyComparer;
    public ReactiveSchedulerState State => _state.Value;

    public IReadOnlyCollection<TKey> TaskKeys
    {
        get
        {
            using ( AcquireScheduleLock() )
            {
                if ( _queue.Count == 0 )
                    return Array.Empty<TKey>();

                var i = 0;
                var result = new TKey[_queue.Count];
                foreach ( var e in _queue )
                    result[i++] = e.Container.Key;

                return result;
            }
        }
    }

    public void Dispose()
    {
        List<ReactiveSchedulerEntry<TKey>>? toRemove = null;
        if ( _state.Write( ReactiveSchedulerState.Disposed, ReactiveSchedulerState.Created ) )
        {
            using ( AcquireScheduleLock() )
            {
                _reset.Dispose();
                DisposeAll( ref toRemove );
            }

            RemoveAll( toRemove );
            return;
        }

        if ( ! _state.Write( ReactiveSchedulerState.Stopping, ReactiveSchedulerState.Running ) )
            return;

        using ( AcquireScheduleLock() )
        {
            _reset.Set();
            DisposeAll( ref toRemove );
        }

        RemoveAll( toRemove );
    }

    [Pure]
    public ScheduleTaskState<TKey>? TryGetTaskState(TKey key)
    {
        ReactiveSchedulerEntry<TKey> entry;
        using ( AcquireScheduleLock() )
        {
            if ( ! _queue.TryGetValue( key, out entry ) )
                return null;
        }

        return new ScheduleTaskState<TKey>(
            entry.Container.CreateStateSnapshot(),
            entry.IsDisposed || entry.IsFinished ? null : entry.Timestamp,
            entry.Interval,
            entry.Repetitions >= 0 ? entry.Repetitions : null,
            entry.IsDisposed );
    }

    public void Start()
    {
        if ( _state.Write( ReactiveSchedulerState.Running, ReactiveSchedulerState.Created ) )
            RunCore();
    }

    public Task StartAsync(TaskScheduler? scheduler = null)
    {
        if ( ! _state.Write( ReactiveSchedulerState.Running, ReactiveSchedulerState.Created ) )
            return Task.CompletedTask;

        var taskFactory = scheduler is null ? Task.Factory : new TaskFactory( scheduler );
        return taskFactory.StartNew( RunCore );
    }

    public bool Schedule(IScheduleTask<TKey> task, Timestamp timestamp)
    {
        return TrySchedule( task, timestamp, Duration.Zero, 1 );
    }

    public bool Schedule(IScheduleTask<TKey> task, Timestamp firstTimestamp, Duration interval, int repetitions)
    {
        Ensure.IsGreaterThan( interval, Duration.Zero );
        return repetitions > 0 && TrySchedule( task, firstTimestamp, interval, repetitions );
    }

    public bool ScheduleInfinite(IScheduleTask<TKey> task, Timestamp firstTimestamp, Duration interval)
    {
        Ensure.IsGreaterThan( interval, Duration.Zero );
        return TrySchedule( task, firstTimestamp, interval, -1 );
    }

    private bool TrySchedule(IScheduleTask<TKey> task, Timestamp timestamp, Duration interval, int repetitions)
    {
        Assume.IsGreaterThanOrEqualTo( interval, Duration.Zero );
        Assume.IsGreaterThanOrEqualTo( repetitions, -1 );
        Assume.NotEquals( repetitions, 0 );

        var key = task.Key;
        using ( AcquireScheduleLock() )
        {
            if ( _state.Value > ReactiveSchedulerState.Running )
                return false;

            if ( _queue.TryGetValue( key, out var entry ) )
            {
                if ( ! ReferenceEquals( entry.Container.Source, task ) || entry.IsDisposed )
                    return false;

                entry = new ReactiveSchedulerEntry<TKey>( entry.Container, timestamp, interval, repetitions );
                _queue.Replace( key, entry );
            }
            else
            {
                var container = new ScheduleTaskContainer<TKey>( this, task );
                entry = new ReactiveSchedulerEntry<TKey>( container, timestamp, interval, repetitions );
                _queue.Add( key, entry );
            }

            if ( _nextEventTimestamp > timestamp )
                _reset.Set();

            return true;
        }
    }

    public bool SetInterval(TKey key, Duration interval)
    {
        Ensure.IsGreaterThan( interval, Duration.Zero );
        using ( AcquireScheduleLock() )
        {
            if ( ! TryGetMutableEntry( key, out var entry ) )
                return false;

            if ( interval != entry.Interval )
            {
                entry = new ReactiveSchedulerEntry<TKey>( entry.Container, entry.Timestamp, interval, entry.Repetitions );
                _queue.Replace( key, entry );
            }

            return true;
        }
    }

    public bool SetRepetitions(TKey key, int repetitions)
    {
        Ensure.IsGreaterThan( repetitions, 0 );
        using ( AcquireScheduleLock() )
        {
            if ( ! TryGetMutableEntry( key, out var entry ) )
                return false;

            if ( repetitions != entry.Repetitions )
            {
                entry = new ReactiveSchedulerEntry<TKey>( entry.Container, entry.Timestamp, entry.Interval, repetitions );
                _queue.Replace( key, entry );
            }

            return true;
        }
    }

    public bool MakeInfinite(TKey key)
    {
        using ( AcquireScheduleLock() )
        {
            if ( ! TryGetMutableEntry( key, out var entry ) )
                return false;

            if ( ! entry.IsInfinite )
            {
                entry = new ReactiveSchedulerEntry<TKey>( entry.Container, entry.Timestamp, entry.Interval, -1 );
                _queue.Replace( key, entry );
            }

            return true;
        }
    }

    public bool SetNextTimestamp(TKey key, Timestamp timestamp)
    {
        using ( AcquireScheduleLock() )
        {
            if ( ! TryGetMutableEntry( key, out var entry ) )
                return false;

            if ( timestamp != entry.Timestamp )
            {
                entry = new ReactiveSchedulerEntry<TKey>( entry.Container, timestamp, entry.Interval, entry.Repetitions );
                _queue.Replace( key, entry );
                if ( _nextEventTimestamp > timestamp )
                    _reset.Set();
            }

            return true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool TryGetMutableEntry(TKey key, out ReactiveSchedulerEntry<TKey> result)
    {
        if ( _state.Value > ReactiveSchedulerState.Running
            || ! _queue.TryGetValue( key, out var entry )
            || entry.IsDisposed
            || entry.IsFinished )
        {
            result = default;
            return false;
        }

        result = entry;
        return true;
    }

    public bool Remove(TKey key)
    {
        ReactiveSchedulerEntry<TKey> entry;
        using ( AcquireScheduleLock() )
        {
            if ( ! _queue.TryGetValue( key, out entry ) )
                return false;

            if ( ! entry.IsDisposed )
                _queue.Replace( key, entry.AsDisposed() );
        }

        entry.Container.Dispose();
        return true;
    }

    public void Clear()
    {
        List<ReactiveSchedulerEntry<TKey>>? toRemove = null;
        using ( AcquireScheduleLock() )
            DisposeAll( ref toRemove );

        RemoveAll( toRemove );
    }

    private void DisposeAll(ref List<ReactiveSchedulerEntry<TKey>>? toRemove)
    {
        foreach ( var entry in _queue )
        {
            if ( entry.IsDisposed )
                continue;

            toRemove ??= new List<ReactiveSchedulerEntry<TKey>>( capacity: _queue.Count );
            toRemove.Add( entry );
        }

        if ( toRemove is null )
            return;

        foreach ( var entry in toRemove )
            _queue.Replace( entry.Container.Key, entry.AsDisposed() );
    }

    private void RemoveAll(List<ReactiveSchedulerEntry<TKey>>? toRemove)
    {
        if ( toRemove is null )
            return;

        var errors = Chain<Exception>.Empty;
        foreach ( var entry in toRemove )
        {
            try
            {
                entry.Container.Dispose();
            }
            catch ( Exception exc )
            {
                errors = errors.Extend( exc );
            }
        }

        if ( errors.Count > 0 )
            throw new AggregateException( errors );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryRemoveFinishedTask(ScheduleTaskContainer<TKey> container)
    {
        using ( AcquireScheduleLock() )
        {
            if ( ! _queue.TryGetValue( container.Key, out var entry )
                || ! ReferenceEquals( entry.Container, container )
                || (! entry.IsDisposed && ! entry.IsFinished) )
                return false;

            _queue.Remove( container.Key );
            return true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireScheduleLock()
    {
        return ExclusiveLock.Enter( _queue );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (Timestamp NextEventTimestamp, Duration Delay) UpdateNextEventTimestamp(Timestamp now)
    {
        var delay = DefaultInterval;
        if ( _queue.TryPeek( out var next ) )
        {
            var delayUntilNextEvent = next.Timestamp - now;
            delay = delayUntilNextEvent.Min( DefaultInterval ).Max( Duration.Zero );
        }

        _nextEventTimestamp = now + delay;
        delay = (delay - SpinWaitDurationHint).Max( Duration.Zero );
        return (_nextEventTimestamp, delay);
    }

    private void RunCore()
    {
        var eventBuffer = new List<ReactiveSchedulerEntry<TKey>>();

        Duration delay;
        Timestamp timestamp;
        Timestamp nextEventTimestamp;
        using ( AcquireScheduleLock() )
        {
            timestamp = Timestamps.GetNow();
            (nextEventTimestamp, delay) = UpdateNextEventTimestamp( timestamp );
        }

        while ( true )
        {
            var signaled = _reset.Wait( delay );
            if ( _state.Write( ReactiveSchedulerState.Disposed, ReactiveSchedulerState.Stopping ) )
                break;

            timestamp = Timestamps.GetNow();
            if ( ! signaled )
            {
                while ( timestamp < nextEventTimestamp )
                {
                    Thread.SpinWait( 1 );
                    timestamp = Timestamps.GetNow();
                }
            }

            using ( AcquireScheduleLock() )
            {
                while ( _queue.TryPeek( out var entry ) && entry.Timestamp <= timestamp )
                {
                    var next = entry.Next();
                    _queue.Replace( entry.Container.Key, next );
                    eventBuffer.Add( entry );
                }
            }

            foreach ( var e in eventBuffer )
                e.Container.EnqueueInvocation( Timestamps.GetNow(), e.Timestamp );

            eventBuffer.Clear();
            using ( AcquireScheduleLock() )
            {
                if ( _state.Write( ReactiveSchedulerState.Disposed, ReactiveSchedulerState.Stopping ) )
                    break;

                _reset.Reset();
                timestamp = Timestamps.GetNow();
                (nextEventTimestamp, delay) = UpdateNextEventTimestamp( timestamp );
            }
        }

        _reset.Dispose();
    }
}
