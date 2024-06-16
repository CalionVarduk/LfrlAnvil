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
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Reactive.Chrono.Internal;

internal struct ReactiveTaskInfo
{
    private Queue<KeyValuePair<long, Timestamp>>? _awaitingInvocations;
    private Dictionary<int, ActiveTaskEntry>? _activeTasks;
    private long _totalInvocations;
    private long _activeInvocations;
    private long _completedInvocations;
    private long _delayedInvocations;
    private long _failedInvocations;
    private long _cancelledInvocations;
    private long _maxQueuedInvocations;
    private long _maxActiveTasks;
    private Timestamp? _firstInvocationTimestamp;
    private Timestamp? _lastInvocationTimestamp;
    private Duration _minElapsedTime;
    private Duration _maxElapsedTime;
    private FloatingDuration _averageElapsedTime;

    private ReactiveTaskInfo(Duration minElapsedTime, Duration maxElapsedTime)
    {
        _awaitingInvocations = null;
        _activeTasks = null;
        _totalInvocations = 0;
        _activeInvocations = 0;
        _completedInvocations = 0;
        _delayedInvocations = 0;
        _failedInvocations = 0;
        _cancelledInvocations = 0;
        _maxQueuedInvocations = 0;
        _maxActiveTasks = 0;
        _firstInvocationTimestamp = null;
        _lastInvocationTimestamp = null;
        _minElapsedTime = minElapsedTime;
        _maxElapsedTime = maxElapsedTime;
        _averageElapsedTime = FloatingDuration.Zero;
    }

    public bool AreActiveTasksInitialized => _activeTasks is not null;

    public bool HasActiveInvocations => _activeInvocations > 0
        || (_activeTasks is not null && _activeTasks.Count > 0)
        || (_awaitingInvocations is not null && _awaitingInvocations.Count > 0);

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReactiveTaskInfo Create()
    {
        return new ReactiveTaskInfo( Duration.MaxValue, Duration.MinValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public long GetNextInvocationId(Timestamp timestamp)
    {
        _firstInvocationTimestamp ??= timestamp;
        _lastInvocationTimestamp = timestamp;
        return _totalInvocations++;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool CanActivateTask(int maxConcurrentInvocations)
    {
        _activeTasks ??= new Dictionary<int, ActiveTaskEntry>();
        return _activeTasks.Count < maxConcurrentInvocations;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int GetNextPositionInQueue()
    {
        _awaitingInvocations ??= new Queue<KeyValuePair<long, Timestamp>>();
        return _awaitingInvocations.Count;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryEnqueueInvocation(long invocationId, Timestamp invocationTimestamp)
    {
        if ( _awaitingInvocations is null )
            return false;

        _awaitingInvocations.Enqueue( KeyValuePair.Create( invocationId, invocationTimestamp ) );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryDequeueSkippedInvocation(int maxEnqueuedInvocations, out long invocationId, out Timestamp invocationTimestamp)
    {
        Assume.IsNotNull( _awaitingInvocations );
        if ( _awaitingInvocations.Count > maxEnqueuedInvocations )
        {
            (invocationId, invocationTimestamp) = _awaitingInvocations.Dequeue();
            return true;
        }

        invocationId = default;
        invocationTimestamp = default;
        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void UpdateMaxQueuedInvocations()
    {
        Assume.IsNotNull( _awaitingInvocations );
        _maxQueuedInvocations = Math.Max( _maxQueuedInvocations, _awaitingInvocations.Count );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void OffsetActiveInvocations(int offset)
    {
        _activeInvocations += offset;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryRegisterActiveTask(Task task, ReactiveTaskInvocationParams parameters, StopwatchSlim stopwatch)
    {
        if ( _activeTasks is null )
            return false;

        _activeTasks[task.Id] = new ActiveTaskEntry( task, parameters, stopwatch );
        _maxActiveTasks = Math.Max( _maxActiveTasks, _activeTasks.Count );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration UpdateTaskCompletion(StopwatchSlim stopwatch, Exception? exception, TaskCancellationReason? cancellationReason)
    {
        var elapsedTime = new Duration( stopwatch.ElapsedTime );
        _minElapsedTime = _minElapsedTime.Min( elapsedTime );
        _maxElapsedTime = _maxElapsedTime.Max( elapsedTime );
        _averageElapsedTime += (elapsedTime - _averageElapsedTime) / ++_completedInvocations;

        if ( exception is not null )
            ++_failedInvocations;

        if ( cancellationReason is not null )
            ++_cancelledInvocations;

        return elapsedTime;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryRemoveActiveTask(Task task, out ActiveTaskEntry entry)
    {
        Assume.IsNotNull( _activeTasks );
        return _activeTasks.Remove( task.Id, out entry );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryDequeueDelayedInvocation(int maxConcurrentInvocations, out long invocationId, out Timestamp invocationTimestamp)
    {
        if ( _activeTasks is not null
            && _awaitingInvocations is not null
            && _activeTasks.Count < maxConcurrentInvocations
            && _awaitingInvocations.TryDequeue( out var invocation ) )
        {
            invocationId = invocation.Key;
            invocationTimestamp = invocation.Value;
            ++_delayedInvocations;
            return true;
        }

        invocationId = default;
        invocationTimestamp = default;
        return false;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReactiveTaskSnapshot<TTask> CreateSnapshot<TTask>(TTask task)
        where TTask : notnull
    {
        return new ReactiveTaskSnapshot<TTask>(
            task,
            _firstInvocationTimestamp,
            _lastInvocationTimestamp,
            _totalInvocations,
            _activeInvocations,
            _completedInvocations,
            _delayedInvocations,
            _failedInvocations,
            _cancelledInvocations,
            _awaitingInvocations?.Count ?? 0,
            _maxQueuedInvocations,
            _activeTasks?.Count ?? 0,
            _maxActiveTasks,
            _minElapsedTime,
            _maxElapsedTime,
            _averageElapsedTime );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Clear()
    {
        _awaitingInvocations = null;
        _activeTasks = null;
    }
}
