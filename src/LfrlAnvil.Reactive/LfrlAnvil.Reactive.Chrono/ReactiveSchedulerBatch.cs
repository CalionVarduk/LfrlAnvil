// Copyright 2026 Łukasz Furlepa
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Exceptions;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a queue of elements, which are processed in batches, automatically or on demand,
/// with auto-flushing registered via a <see cref="ReactiveScheduler{TKey}"/>.
/// </summary>
/// <typeparam name="TKey">Scheduler task's key type.</typeparam>
/// <typeparam name="T">Element type.</typeparam>
public abstract class ReactiveSchedulerBatch<TKey, T> : Batch<T>
    where TKey : notnull
{
    private static Timestamp InactiveTimestamp => new Timestamp( long.MaxValue );
    private readonly ScheduleTask _task;

    /// <summary>
    /// Creates a new <see cref="ReactiveSchedulerBatch{TKey,T}"/> instance.
    /// </summary>
    /// <param name="scheduler">Scheduler in which to register auto-flush task.</param>
    /// <param name="key">Identifier of the auto-flush task in the <paramref name="scheduler"/>.</param>
    /// <param name="autoFlushDelay">
    /// Specifies the delay with which auto-flush should be scheduled after first item was added to an empty batch.
    /// </param>
    /// <param name="queueOverflowStrategy">
    /// Specifies the maximum number of enqueued elements that, when exceeded while adding new elements,
    /// will cause this batch to react according to its <see cref="Batch{T}.QueueOverflowStrategy"/>.
    /// Equal to <see cref="BatchQueueOverflowStrategy.DiscardLast"/> by default.
    /// </param>
    /// <param name="autoFlushCount">
    /// Specifies the number of enqueued elements, which acts as a threshold that, when reached while adding new elements,
    /// will cause this batch to automatically <see cref="Batch{T}.Flush()"/> itself.
    /// Equal to <b>1 000</b> by default.
    /// </param>
    /// <param name="queueSizeLimitHint">
    /// Specifies the maximum number of enqueued elements that, when exceeded while adding new elements,
    /// will cause this batch to react according to its <see cref="Batch{T}.QueueOverflowStrategy"/>.
    /// Equal to <b>100 000 000</b> by default.
    /// </param>
    /// <param name="minInitialCapacity">Specifies minimum initial capacity of the internal queue. Equal to <b>0</b> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="autoFlushDelay"/> is less than or equal to <see cref="Duration.Zero"/>
    /// or when <paramref name="autoFlushCount"/> is less than <b>1</b>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// When auto-flush task could not be registered in the provided <paramref name="scheduler"/>,
    /// either because the scheduler is disposed or a task with the provided <paramref name="key"/> already exists.
    /// </exception>
    protected ReactiveSchedulerBatch(
        IReactiveScheduler<TKey> scheduler,
        TKey key,
        Duration autoFlushDelay,
        BatchQueueOverflowStrategy queueOverflowStrategy = BatchQueueOverflowStrategy.DiscardLast,
        int autoFlushCount = 1000,
        int queueSizeLimitHint = 100000000,
        int minInitialCapacity = 0)
        : base( queueOverflowStrategy, autoFlushCount, queueSizeLimitHint, minInitialCapacity )
    {
        Ensure.IsGreaterThan( autoFlushDelay, Duration.Zero );

        AutoFlushDelay = autoFlushDelay;
        Scheduler = scheduler;
        _task = new ScheduleTask( this, key );
        if ( ! scheduler.Schedule( _task, InactiveTimestamp ) )
        {
            _task.DiscardOwner();
            throw new InvalidOperationException( Resources.FailedToScheduleAutoFlushTask );
        }
    }

    /// <summary>
    /// Scheduler in which auto-flush task is registered.
    /// </summary>
    public IReactiveScheduler<TKey> Scheduler { get; }

    /// <summary>
    /// Specifies the delay with which auto-flush should be scheduled after first item was added to an empty batch.
    /// </summary>
    public Duration AutoFlushDelay { get; }

    /// <summary>
    /// Identifier of the auto-flush task in the <see cref="Scheduler"/>.
    /// </summary>
    public TKey SchedulerKey => _task.Key;

    /// <inheritdoc/>
    protected override void OnDisposed()
    {
        base.OnDisposed();
        _task.RemoveIfActive( Scheduler );
    }

    /// <inheritdoc/>
    protected override void OnEnqueued(QueueSlimMemory<T> items, bool autoFlushing)
    {
        base.OnEnqueued( items, autoFlushing );
        _task.ScheduleIfActive( Scheduler, autoFlushing );
    }

    private sealed class ScheduleTask : ScheduleTask<TKey>
    {
        private readonly object _lock = new object();
        private ReactiveSchedulerBatch<TKey, T>? _owner;
        private bool _scheduled;

        public ScheduleTask(ReactiveSchedulerBatch<TKey, T> owner, TKey key)
            : base( key )
        {
            _owner = owner;
        }

        public override void Dispose()
        {
            base.Dispose();

            ReactiveSchedulerBatch<TKey, T>? owner;
            using ( AcquireLock() )
            {
                owner = _owner;
                _owner = null;
            }

            // NOTE:
            // fire-and-forget is acceptable, batch's DisposeAsync uses a TCS to halt subsequent batch disposers
            // until the first disposal finishes
            _ = owner?.DisposeAsync().AsTask();
        }

        public override Task InvokeAsync(
            IReactiveScheduler<TKey> scheduler,
            ReactiveTaskInvocationParams parameters,
            CancellationToken cancellationToken)
        {
            ReactiveSchedulerBatch<TKey, T>? owner;
            using ( AcquireLock() )
            {
                owner = _owner;
                if ( _owner is not null )
                {
                    _scheduled = false;
                    scheduler.Schedule( this, InactiveTimestamp );
                }
            }

            owner?.Flush();
            return Task.CompletedTask;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void RemoveIfActive(IReactiveScheduler<TKey> scheduler)
        {
            using ( AcquireLock() )
            {
                if ( _owner is not null )
                {
                    _owner = null;
                    _scheduled = false;
                    scheduler.Remove( Key );
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ScheduleIfActive(IReactiveScheduler<TKey> scheduler, bool autoFlushing)
        {
            using ( AcquireLock() )
            {
                if ( _owner is null )
                    return;

                if ( autoFlushing )
                {
                    if ( ! _scheduled )
                        return;

                    _scheduled = false;
                    scheduler.Schedule( this, InactiveTimestamp );
                }
                else if ( ! _scheduled )
                {
                    _scheduled = true;
                    scheduler.Schedule( this, scheduler.Timestamps.GetNow() + _owner.AutoFlushDelay );
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void DiscardOwner()
        {
            using ( AcquireLock() )
                _owner = null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ExclusiveLock AcquireLock()
        {
            return ExclusiveLock.Enter( _lock );
        }
    }
}
