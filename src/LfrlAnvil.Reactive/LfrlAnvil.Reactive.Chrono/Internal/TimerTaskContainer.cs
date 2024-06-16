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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Reactive.Chrono.Internal;

internal sealed class TimerTaskContainer<TKey> : IDisposable
    where TKey : notnull
{
    private readonly Action<Task> _onActiveTaskCompleted;
    private readonly ITimerTask<TKey> _source;
    private TimerTaskCollection<TKey>? _owner;
    private ReactiveTaskInfo _info;
    private Timestamp? _lastOwnerTimestamp;
    private bool _disposed;

    internal TimerTaskContainer(TimerTaskCollection<TKey> owner, ITimerTask<TKey> source)
    {
        _owner = owner;
        _source = source;
        _info = ReactiveTaskInfo.Create();
        _lastOwnerTimestamp = null;
        _disposed = false;

        _onActiveTaskCompleted = t =>
        {
            using ( ExclusiveLock.Enter( this ) )
            {
                if ( ! _info.AreActiveTasksInitialized )
                    return;

                if ( _info.TryRemoveActiveTask( t, out var entry ) )
                    FinishCompletedInvocation(
                        entry.Invocation,
                        entry.Stopwatch,
                        t.Exception,
                        t.IsCanceled ? TaskCancellationReason.CancellationRequested : null );

                var maxConcurrentInvocations = _source.MaxConcurrentInvocations;
                while ( _info.TryDequeueDelayedInvocation( maxConcurrentInvocations, out var invocationId, out var invocationTimestamp ) )
                {
                    if ( _disposed )
                        FinishSkippedInvocation( invocationId, invocationTimestamp, TaskCancellationReason.TaskDisposed );
                    else
                    {
                        ActivateTask(
                            new ReactiveTaskInvocationParams(
                                invocationId,
                                invocationTimestamp,
                                _lastOwnerTimestamp ?? invocationTimestamp ) );
                    }
                }
            }
        };
    }

    internal TKey Key => _source.Key;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            if ( _disposed )
                return;

            _disposed = true;
            if ( ! _info.HasActiveInvocations )
                FinalizeDisposal();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void EnqueueInvocation(Timestamp timestamp)
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            if ( _disposed )
                return;

            _lastOwnerTimestamp = timestamp;
            var nextInvocationTimestamp = _source.NextInvocationTimestamp;
            if ( nextInvocationTimestamp is not null && nextInvocationTimestamp.Value > timestamp )
                return;

            var maxConcurrentInvocations = _source.MaxConcurrentInvocations;
            if ( maxConcurrentInvocations <= 0 )
                return;

            var invocationId = _info.GetNextInvocationId( timestamp );
            if ( _info.CanActivateTask( maxConcurrentInvocations ) )
            {
                ActivateTask( new ReactiveTaskInvocationParams( invocationId, timestamp, timestamp ) );
                return;
            }

            var maxEnqueuedInvocations = _source.MaxEnqueuedInvocations;
            if ( maxEnqueuedInvocations <= 0 )
            {
                FinishSkippedInvocation( invocationId, timestamp, TaskCancellationReason.MaxQueueSizeLimit );
                return;
            }

            var enqueue = OnEnqueue(
                new ReactiveTaskInvocationParams( invocationId, timestamp, timestamp ),
                _info.GetNextPositionInQueue() );

            if ( ! enqueue )
            {
                FinishSkippedInvocation( invocationId, timestamp, TaskCancellationReason.CancellationRequested );
                return;
            }

            if ( ! _info.TryEnqueueInvocation( invocationId, timestamp ) )
                return;

            while ( _info.TryDequeueSkippedInvocation(
                maxEnqueuedInvocations,
                out var skippedInvocationId,
                out var skippedInvocationTimestamp ) )
                FinishSkippedInvocation( skippedInvocationId, skippedInvocationTimestamp, TaskCancellationReason.MaxQueueSizeLimit );

            _info.UpdateMaxQueuedInvocations();
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReactiveTaskSnapshot<ITimerTask<TKey>> CreateStateSnapshot()
    {
        using ( ExclusiveLock.Enter( this ) )
            return _info.CreateSnapshot( _source );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ActivateTask(ReactiveTaskInvocationParams parameters)
    {
        Assume.False( _disposed );
        Assume.IsNotNull( _owner );

        Task task;
        var stopwatch = StopwatchSlim.Create();
        try
        {
            _info.OffsetActiveInvocations( 1 );
            try
            {
                task = _source.InvokeAsync( _owner, parameters, _owner.CancellationTokenSource.Token );
            }
            finally
            {
                _info.OffsetActiveInvocations( -1 );
            }

            if ( task.Status == TaskStatus.Created )
                task.Start( TaskScheduler.Default );
        }
        catch ( Exception exc )
        {
            FinishCompletedInvocation( parameters, stopwatch, exc, null );
            return;
        }

        if ( task.IsCompleted )
        {
            FinishCompletedInvocation(
                parameters,
                stopwatch,
                task.Exception,
                task.IsCanceled ? TaskCancellationReason.CancellationRequested : null );

            return;
        }

        if ( _info.TryRegisterActiveTask( task, parameters, stopwatch ) )
            task.ContinueWith( _onActiveTaskCompleted, TaskContinuationOptions.ExecuteSynchronously );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FinishCompletedInvocation(
        ReactiveTaskInvocationParams invocation,
        StopwatchSlim stopwatch,
        Exception? exception,
        TaskCancellationReason? cancellationReason)
    {
        var elapsedTime = _info.UpdateTaskCompletion( stopwatch, exception, cancellationReason );
        var parameters = new ReactiveTaskCompletionParams( invocation, elapsedTime, exception, cancellationReason );
        OnCompleted( parameters );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FinishSkippedInvocation(long invocationId, Timestamp originalTimestamp, TaskCancellationReason cancellationReason)
    {
        var parameters = new ReactiveTaskCompletionParams(
            new ReactiveTaskInvocationParams( invocationId, originalTimestamp, _lastOwnerTimestamp ?? originalTimestamp ),
            Duration.Zero,
            null,
            cancellationReason );

        OnCompleted( parameters );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void OnCompleted(ReactiveTaskCompletionParams parameters)
    {
        if ( _owner is null )
            return;

        try
        {
            _source.OnCompleted( _owner, parameters );
        }
        catch
        {
            // NOTE:
            // silently ignore all exceptions, otherwise the whole timer task collection would get derailed
            // this is by design, no exceptions should be thrown by the OnCompleted invocation
        }

        if ( _disposed && ! _info.HasActiveInvocations )
            FinalizeDisposal();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool OnEnqueue(ReactiveTaskInvocationParams parameters, int positionInQueue)
    {
        if ( _owner is null )
            return false;

        try
        {
            return _source.OnEnqueue( _owner, parameters, positionInQueue );
        }
        catch
        {
            // NOTE:
            // silently ignore all exceptions, otherwise the whole timer task collection would get derailed
            // this is by design, no exceptions should be thrown by the OnEnqueue invocation
        }

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FinalizeDisposal()
    {
        _owner = null;
        _info.Clear();
        _source.Dispose();
    }
}
