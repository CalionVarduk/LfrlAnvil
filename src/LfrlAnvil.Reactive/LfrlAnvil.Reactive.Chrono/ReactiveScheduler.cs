// Copyright 2024-2026 Łukasz Furlepa
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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Collections;
using LfrlAnvil.Extensions;
using LfrlAnvil.Reactive.Chrono.Internal;

namespace LfrlAnvil.Reactive.Chrono;

/// <inheritdoc cref="IReactiveScheduler{TKey}" />
public sealed class ReactiveScheduler<TKey> : IReactiveScheduler<TKey>
    where TKey : notnull
{
    private static Duration MaxDelay => Duration.FromMilliseconds( int.MaxValue );

    private readonly DictionaryHeap<TKey, ReactiveSchedulerEntry<TKey>> _queue;
    private readonly TaskCompletionSource _disposed;
    private DelaySource _delaySource;
    private AsyncManualResetEvent _reset;
    private Timestamp _nextEventTimestamp;
    private Task? _task;
    private ReactiveSchedulerState _state;

    /// <summary>
    /// Creates a new empty <see cref="ReactiveScheduler{TKey}"/> instance.
    /// </summary>
    /// <param name="timestamps">Optional <see cref="ITimestampProvider"/> instance used for time tracking.</param>
    /// <param name="delaySource">Optional value task delay source to use for scheduling delays.</param>
    /// <param name="keyComparer">Key equality comparer. Equal to <see cref="EqualityComparer{T}.Default"/> by default.</param>
    /// <param name="spinWaitDurationHint">
    /// <see cref="SpinWait"/> duration hint for the underlying time tracking mechanism. Equal to <b>1 microsecond</b> by default.
    /// </param>
    /// <param name="taskDisposalTimeout">
    /// Max time the disposal of scheduled tasks will wait for their invocations to complete.
    /// Equal to <see cref="Duration.Zero"/> by default.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"> When <paramref name="spinWaitDurationHint"/> is less than <b>0</b>. </exception>
    public ReactiveScheduler(
        ITimestampProvider? timestamps = null,
        ValueTaskDelaySource? delaySource = null,
        IEqualityComparer<TKey>? keyComparer = null,
        Duration? spinWaitDurationHint = null,
        Duration taskDisposalTimeout = default)
    {
        if ( spinWaitDurationHint is not null )
            Ensure.IsGreaterThanOrEqualTo( spinWaitDurationHint.Value, Duration.Zero );

        _state = ReactiveSchedulerState.Created;
        TaskDisposalTimeout = taskDisposalTimeout.Max( Duration.Zero );
        SpinWaitDurationHint = spinWaitDurationHint ?? ReactiveTimer.DefaultSpinWaitDurationHint;
        Timestamps = timestamps ?? TimestampProvider.Shared;

        _queue = new DictionaryHeap<TKey, ReactiveSchedulerEntry<TKey>>(
            keyComparer ?? EqualityComparer<TKey>.Default,
            Comparer<ReactiveSchedulerEntry<TKey>>.Default );

        _nextEventTimestamp = new Timestamp( long.MinValue );
        _delaySource = delaySource is null ? DelaySource.Owned() : DelaySource.External( delaySource );
        _disposed = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        StartTimestamp = Timestamps.GetNow();
    }

    /// <inheritdoc />
    public Duration TaskDisposalTimeout { get; }

    /// <inheritdoc />
    public Timestamp StartTimestamp { get; }

    /// <inheritdoc />
    public Duration SpinWaitDurationHint { get; }

    /// <inheritdoc />
    public ITimestampProvider Timestamps { get; }

    /// <inheritdoc />
    public IEqualityComparer<TKey> KeyComparer => _queue.KeyComparer;

    /// <inheritdoc />
    public ReactiveSchedulerState State
    {
        get
        {
            using ( AcquireScheduleLock() )
                return _state;
        }
    }

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
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        TaskCompletionSource? disposed = null;
        using ( AcquireScheduleLock() )
        {
            if ( _state >= ReactiveSchedulerState.Disposing )
                disposed = _disposed;
            else
                _state = ReactiveSchedulerState.Disposing;
        }

        if ( disposed is not null )
        {
            await disposed.Task.ConfigureAwait( false );
            return;
        }

        await DisposeAsyncCore().ConfigureAwait( false );
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
    public bool Start()
    {
        using ( AcquireScheduleLock() )
        {
            if ( _state != ReactiveSchedulerState.Created )
                return false;

            Assume.IsNull( _task );
            Assume.Equals( _reset, default );

            _state = ReactiveSchedulerState.Running;
            var delaySource = _delaySource.GetSource();
            _reset = delaySource.GetResetEvent();
        }

        var task = RunCore();
        using ( AcquireScheduleLock() )
        {
            if ( _state == ReactiveSchedulerState.Running )
                _task = task;
        }

        return true;
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

        entry.Container.BeginDispose( TaskDisposalTimeout );
        return true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        ReactiveSchedulerEntry<TKey>[] entries;
        using ( AcquireScheduleLock() )
        {
            if ( _state >= ReactiveSchedulerState.Disposing )
                return;

            entries = _queue.ToArray();
            MarkEntriesAsDisposed( entries );
        }

        var errors = Chain<Exception>.Empty;
        BeginDisposing( entries, ref errors );
        if ( errors.Count > 0 )
            throw errors.Consolidate()!;
    }

    private bool TrySchedule(IScheduleTask<TKey> task, Timestamp timestamp, Duration interval, int repetitions)
    {
        Assume.IsGreaterThanOrEqualTo( interval, Duration.Zero );
        Assume.IsGreaterThanOrEqualTo( repetitions, -1 );
        Assume.NotEquals( repetitions, 0 );

        var key = task.Key;
        using ( AcquireScheduleLock() )
        {
            if ( _state >= ReactiveSchedulerState.Disposing )
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
        if ( _state >= ReactiveSchedulerState.Disposing
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

    private async ValueTask DisposeAsyncCore(bool ignoreTask = false)
    {
        try
        {
            Task? task;
            ValueTaskDelaySource? ownedDelaySource;
            ReactiveSchedulerEntry<TKey>[] entries;

            using ( AcquireScheduleLock() )
            {
                Assume.Equals( _state, ReactiveSchedulerState.Disposing );
                ownedDelaySource = _delaySource.DiscardOwnedSource();
                task = ignoreTask ? null : _task;
                _task = null;

                if ( ! ignoreTask )
                    _reset.Set();

                entries = _queue.ToArray();
                MarkEntriesAsDisposed( entries );
            }

            var errors = Chain<Exception>.Empty;
            BeginDisposing( entries, ref errors );

            try
            {
                if ( task is not null )
                    await task.ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                errors = errors.Extend( exc );
            }

            if ( TaskDisposalTimeout > Duration.Zero )
            {
                try
                {
                    await Task.WhenAll( entries.Select( e => e.Container.WaitForDisposalAsync( TaskDisposalTimeout ) ) )
                        .ConfigureAwait( false );
                }
                catch ( Exception exc )
                {
                    errors = errors.Extend( exc );
                }
            }

            using ( AcquireScheduleLock() )
            {
                _state = ReactiveSchedulerState.Disposed;
                _reset.Dispose();
                _reset = default;
                _queue.Clear();
            }

            try
            {
                if ( ownedDelaySource is not null )
                    await ownedDelaySource.DisposeAsync().ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                errors = errors.Extend( exc );
            }

            if ( errors.Count > 0 )
                throw errors.Consolidate()!;
        }
        finally
        {
            _disposed.TrySetResult();
        }
    }

    private void MarkEntriesAsDisposed(ReadOnlyArray<ReactiveSchedulerEntry<TKey>> entries)
    {
        foreach ( var entry in entries )
        {
            if ( ! entry.IsDisposed )
                _queue.Replace( entry.Container.Key, entry.AsDisposed() );
        }
    }

    private void BeginDisposing(ReadOnlyArray<ReactiveSchedulerEntry<TKey>> entries, ref Chain<Exception> errors)
    {
        foreach ( var entry in entries )
        {
            try
            {
                entry.Container.BeginDispose( TaskDisposalTimeout );
            }
            catch ( Exception exc )
            {
                errors = errors.Extend( exc );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryRemoveAndDisposeFinishedTask(ScheduleTaskContainer<TKey> container, out Exception? exception)
    {
        bool exists;
        exception = null;
        using ( AcquireScheduleLock() )
        {
            if ( _state >= ReactiveSchedulerState.Disposing
                || ! _queue.TryGetValue( container.Key, out var entry )
                || ! ReferenceEquals( entry.Container, container ) )
                exists = false;
            else
            {
                if ( entry.IsFinished )
                    _queue.Replace( container.Key, entry.AsDisposed() );
                else if ( ! entry.IsDisposed )
                    return false;

                exists = true;
            }
        }

        try
        {
            container.Source.Dispose();
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        if ( exists )
        {
            using ( AcquireScheduleLock() )
                _queue.Remove( container.Key );
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireScheduleLock()
    {
        return ExclusiveLock.Enter( _queue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (Timestamp NextEventTimestamp, Duration Delay) UpdateNextEventTimestamp(Timestamp now)
    {
        var delay = MaxDelay;
        if ( _queue.TryPeek( out var entry ) )
        {
            var next = entry.Timestamp;
            var delayUntilNextEvent = next - now;
            delay = delayUntilNextEvent.Clamp( Duration.Zero, delay );
        }

        _nextEventTimestamp = now + delay;
        delay = (delay - SpinWaitDurationHint).Max( Duration.Zero );
        return (_nextEventTimestamp, delay);
    }

    private bool TrySpinUntilTimestampReached(Timestamp nextEventTimestamp, out Timestamp timestamp)
    {
        timestamp = Timestamps.GetNow();
        if ( timestamp >= nextEventTimestamp )
            return true;

        var spinWait = new SpinWait();
        var lockAcquiredAt = timestamp;
        var lockAcquisitionInterval = Duration.FromMicroseconds( 500 );
        do
        {
            if ( timestamp - lockAcquiredAt > lockAcquisitionInterval )
            {
                using ( AcquireScheduleLock() )
                {
                    if ( _state >= ReactiveSchedulerState.Disposing )
                        return false;

                    Assume.Equals( _state, ReactiveSchedulerState.Running );
                }

                lockAcquiredAt = timestamp;
            }

            spinWait.SpinOnce();
            timestamp = Timestamps.GetNow();
        }
        while ( timestamp < nextEventTimestamp );

        return true;
    }

    private async Task RunCore()
    {
        var eventBuffer = ListSlim<ReactiveSchedulerEntry<TKey>>.Create();

        Duration delay;
        Timestamp timestamp;
        Timestamp nextEventTimestamp;
        using ( AcquireScheduleLock() )
        {
            if ( _state >= ReactiveSchedulerState.Disposing )
                return;

            timestamp = Timestamps.GetNow();
            (nextEventTimestamp, delay) = UpdateNextEventTimestamp( timestamp );
        }

        while ( true )
        {
            var result = await _reset.WaitAsync( delay ).ConfigureAwait( false );
            if ( result == AsyncManualResetEventResult.Disposed )
            {
                using ( AcquireScheduleLock() )
                {
                    if ( _state >= ReactiveSchedulerState.Disposing )
                        return;

                    _state = ReactiveSchedulerState.Disposing;
                }

                await DisposeAsyncCore( ignoreTask: true ).ConfigureAwait( false );
                return;
            }

            if ( result == AsyncManualResetEventResult.Signaled )
                timestamp = Timestamps.GetNow();
            else if ( ! TrySpinUntilTimestampReached( nextEventTimestamp, out timestamp ) )
                return;

            using ( AcquireScheduleLock() )
            {
                if ( _state >= ReactiveSchedulerState.Disposing )
                    return;

                while ( _queue.TryPeek( out var entry ) && entry.Timestamp <= timestamp )
                {
                    var next = entry.Next();
                    _queue.Replace( entry.Container.Key, next );
                    eventBuffer.Add( entry );
                }
            }

            EnqueueInvocations( ref eventBuffer );
            eventBuffer.Clear();

            using ( AcquireScheduleLock() )
            {
                if ( _state >= ReactiveSchedulerState.Disposing )
                    return;

                _reset.Reset();
                timestamp = Timestamps.GetNow();
                (nextEventTimestamp, delay) = UpdateNextEventTimestamp( timestamp );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EnqueueInvocations(ref ListSlim<ReactiveSchedulerEntry<TKey>> events)
    {
        foreach ( var e in events )
            e.Container.EnqueueInvocation( Timestamps.GetNow(), e.Timestamp );
    }
}
