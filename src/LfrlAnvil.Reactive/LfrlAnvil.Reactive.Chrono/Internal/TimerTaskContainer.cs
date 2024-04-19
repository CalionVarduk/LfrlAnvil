using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Reactive.Chrono.Internal;

internal sealed class TimerTaskContainer<TKey> : IDisposable
    where TKey : notnull
{
    private readonly Action<Task> _onActiveTaskCompleted;
    private readonly ITimerTask<TKey> _source;
    private TimerTaskCollection<TKey>? _owner;
    private Queue<KeyValuePair<long, Timestamp>>? _awaitingInvocations;
    private Dictionary<int, TaskEntry>? _activeTasks;
    private long _totalInvocations;
    private long _completedInvocations;
    private long _delayedInvocations;
    private long _failedInvocations;
    private long _cancelledInvocations;
    private long _maxQueuedInvocations;
    private long _maxActiveTasks;
    private Timestamp? _firstInvocationTimestamp;
    private Timestamp? _lastInvocationTimestamp;
    private Timestamp? _lastOwnerTimestamp;
    private Duration _minElapsedTime;
    private Duration _maxElapsedTime;
    private FloatingDuration _averageElapsedTime;
    private bool _disposed;

    internal TimerTaskContainer(TimerTaskCollection<TKey> owner, ITimerTask<TKey> source)
    {
        _owner = owner;
        _source = source;
        _awaitingInvocations = null;
        _activeTasks = null;
        _totalInvocations = 0;
        _completedInvocations = 0;
        _delayedInvocations = 0;
        _failedInvocations = 0;
        _cancelledInvocations = 0;
        _maxQueuedInvocations = 0;
        _maxActiveTasks = 0;
        _firstInvocationTimestamp = null;
        _lastInvocationTimestamp = null;
        _lastOwnerTimestamp = null;
        _minElapsedTime = Duration.MaxValue;
        _maxElapsedTime = Duration.MinValue;
        _averageElapsedTime = FloatingDuration.Zero;
        _disposed = false;

        _onActiveTaskCompleted = t =>
        {
            using ( ExclusiveLock.Enter( this ) )
            {
                Assume.IsNotNull( _activeTasks );
                if ( _activeTasks.Remove( t.Id, out var entry ) )
                    FinishCompletedInvocation(
                        entry.Invocation,
                        entry.StartTimestamp,
                        t.Exception,
                        t.IsCanceled ? TaskCancellationReason.CancellationRequested : null );

                var maxConcurrentInvocations = _source.MaxConcurrentInvocations;
                while ( _activeTasks is not null
                    && _awaitingInvocations is not null
                    && _activeTasks.Count < maxConcurrentInvocations
                    && _awaitingInvocations.TryDequeue( out var invocation ) )
                {
                    ++_delayedInvocations;
                    if ( _disposed )
                        FinishSkippedInvocation( invocation.Key, invocation.Value, TaskCancellationReason.TaskDisposed );
                    else
                    {
                        ActivateTask(
                            new ReactiveTaskInvocationParams(
                                invocation.Key,
                                invocation.Value,
                                _lastOwnerTimestamp ?? invocation.Value ) );
                    }
                }
            }
        };
    }

    internal TKey Key => _source.Key;

    private bool HasActiveInvocations => (_activeTasks is not null && _activeTasks.Count > 0)
        || (_awaitingInvocations is not null && _awaitingInvocations.Count > 0);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            if ( _disposed )
                return;

            _disposed = true;
            if ( ! HasActiveInvocations )
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

            _firstInvocationTimestamp ??= timestamp;
            _lastInvocationTimestamp = timestamp;
            var invocationId = _totalInvocations++;

            _activeTasks ??= new Dictionary<int, TaskEntry>();
            if ( _activeTasks.Count < maxConcurrentInvocations )
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

            _awaitingInvocations ??= new Queue<KeyValuePair<long, Timestamp>>();
            _awaitingInvocations.Enqueue( KeyValuePair.Create( invocationId, timestamp ) );
            while ( _awaitingInvocations.Count > maxEnqueuedInvocations )
            {
                var skipped = _awaitingInvocations.Dequeue();
                FinishSkippedInvocation( skipped.Key, skipped.Value, TaskCancellationReason.MaxQueueSizeLimit );
            }

            _maxQueuedInvocations = Math.Max( _maxQueuedInvocations, _awaitingInvocations.Count );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal TimerTaskStateSnapshot<TKey> CreateStateSnapshot()
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            return new TimerTaskStateSnapshot<TKey>(
                _source,
                _firstInvocationTimestamp,
                _lastInvocationTimestamp,
                _totalInvocations,
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
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ActivateTask(ReactiveTaskInvocationParams parameters)
    {
        Assume.False( _disposed );
        Assume.IsNotNull( _owner );
        Assume.IsNotNull( _source );
        Assume.IsNotNull( _activeTasks );

        Task task;
        var startTimestamp = Stopwatch.GetTimestamp();
        try
        {
            task = _source.InvokeAsync( _owner, parameters, _owner.CancellationTokenSource.Token );
            if ( task.Status == TaskStatus.Created )
                task.Start( TaskScheduler.Default );
        }
        catch ( Exception exc )
        {
            FinishCompletedInvocation( parameters, startTimestamp, exc, null );
            return;
        }

        if ( task.IsCompleted )
        {
            FinishCompletedInvocation(
                parameters,
                startTimestamp,
                task.Exception,
                task.IsCanceled ? TaskCancellationReason.CancellationRequested : null );

            return;
        }

        _activeTasks[task.Id] = new TaskEntry( task, parameters, startTimestamp );
        _maxActiveTasks = Math.Max( _maxActiveTasks, _activeTasks.Count );
        task.ContinueWith( _onActiveTaskCompleted, TaskContinuationOptions.ExecuteSynchronously );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FinishCompletedInvocation(
        ReactiveTaskInvocationParams invocation,
        long startTimestamp,
        Exception? exception,
        TaskCancellationReason? cancellationReason)
    {
        var elapsedTime = new Duration( StopwatchTimestamp.GetTimeSpan( startTimestamp, Stopwatch.GetTimestamp() ) );
        _minElapsedTime = _minElapsedTime.Min( elapsedTime );
        _maxElapsedTime = _maxElapsedTime.Max( elapsedTime );
        _averageElapsedTime += (elapsedTime - _averageElapsedTime) / ++_completedInvocations;
        var parameters = new ReactiveTaskCompletionParams( invocation, elapsedTime, exception, cancellationReason );

        if ( exception is not null )
            ++_failedInvocations;

        if ( cancellationReason is not null )
            ++_cancelledInvocations;

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
        Assume.IsNotNull( _owner );
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

        if ( _disposed && ! HasActiveInvocations )
            FinalizeDisposal();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FinalizeDisposal()
    {
        _owner = null;
        _awaitingInvocations = null;
        _activeTasks = null;
        _source.Dispose();
    }

    private readonly record struct TaskEntry(Task Task, ReactiveTaskInvocationParams Invocation, long StartTimestamp);
}
