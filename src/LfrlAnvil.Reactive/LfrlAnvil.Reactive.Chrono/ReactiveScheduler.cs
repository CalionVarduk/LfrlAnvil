// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

/// <inheritdoc cref="IReactiveScheduler{TKey}" />
public class ReactiveScheduler<TKey> : IReactiveScheduler<TKey>
    where TKey : notnull
{
    private readonly DictionaryHeap<TKey, ReactiveSchedulerEntry<TKey>> _queue;
    private readonly ManualResetEventSlim _reset;
    private InterlockedEnum<ReactiveSchedulerState> _state;
    private Timestamp _nextEventTimestamp;

    /// <summary>
    /// Creates a new empty <see cref="ReactiveScheduler{TKey}"/> instance.
    /// </summary>
    /// <param name="timestamps"><see cref="ITimestampProvider"/> instance used for time tracking.</param>
    /// <param name="keyComparer">Key equality comparer. Equal to <see cref="EqualityComparer{T}.Default"/> by default.</param>
    /// <param name="defaultInterval">
    /// Maximum <see cref="Duration"/> to hang the underlying time tracking mechanism for. Equal to <b>1 hour</b> by default.
    /// </param>
    /// <param name="spinWaitDurationHint">
    /// <see cref="SpinWait"/> duration hint for the underlying time tracking mechanism. Equal to <b>1 microsecond</b> by default.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="defaultInterval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
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

    /// <inheritdoc />
    public Timestamp StartTimestamp { get; }

    /// <inheritdoc />
    public Duration DefaultInterval { get; }

    /// <inheritdoc />
    public Duration SpinWaitDurationHint { get; }

    /// <inheritdoc />
    public ITimestampProvider Timestamps { get; }

    /// <inheritdoc />
    public IEqualityComparer<TKey> KeyComparer => _queue.KeyComparer;

    /// <inheritdoc />
    public ReactiveSchedulerState State => _state.Value;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Dispose()
    {
        var toRemove = ListSlim<ReactiveSchedulerEntry<TKey>>.Create();
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Start()
    {
        if ( _state.Write( ReactiveSchedulerState.Running, ReactiveSchedulerState.Created ) )
            RunCore();
    }

    /// <inheritdoc />
    public Task StartAsync(TaskScheduler? scheduler = null)
    {
        if ( ! _state.Write( ReactiveSchedulerState.Running, ReactiveSchedulerState.Created ) )
            return Task.CompletedTask;

        var taskFactory = scheduler is null ? Task.Factory : new TaskFactory( scheduler );
        return taskFactory.StartNew( RunCore );
    }

    /// <inheritdoc />
    public bool Schedule(IScheduleTask<TKey> task, Timestamp timestamp)
    {
        return TrySchedule( task, timestamp, Duration.Zero, 1 );
    }

    /// <inheritdoc />
    public bool Schedule(IScheduleTask<TKey> task, Timestamp firstTimestamp, Duration interval, int repetitions)
    {
        Ensure.IsGreaterThan( interval, Duration.Zero );
        return repetitions > 0 && TrySchedule( task, firstTimestamp, interval, repetitions );
    }

    /// <inheritdoc />
    public bool ScheduleInfinite(IScheduleTask<TKey> task, Timestamp firstTimestamp, Duration interval)
    {
        Ensure.IsGreaterThan( interval, Duration.Zero );
        return TrySchedule( task, firstTimestamp, interval, -1 );
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Clear()
    {
        ListSlim<ReactiveSchedulerEntry<TKey>> toRemove = ListSlim<ReactiveSchedulerEntry<TKey>>.Create();
        using ( AcquireScheduleLock() )
            DisposeAll( ref toRemove );

        RemoveAll( toRemove );
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

    private void DisposeAll(ref ListSlim<ReactiveSchedulerEntry<TKey>> toRemove)
    {
        foreach ( var entry in _queue )
        {
            if ( entry.IsDisposed )
                continue;

            if ( toRemove.Capacity == 0 )
                toRemove.ResetCapacity( _queue.Count );

            toRemove.Add( entry );
        }

        if ( toRemove.IsEmpty )
            return;

        foreach ( var entry in toRemove )
            _queue.Replace( entry.Container.Key, entry.AsDisposed() );
    }

    private void RemoveAll(ListSlim<ReactiveSchedulerEntry<TKey>> toRemove)
    {
        if ( toRemove.IsEmpty )
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
        var eventBuffer = ListSlim<ReactiveSchedulerEntry<TKey>>.Create();

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
