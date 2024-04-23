using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Reactive.Chrono.Internal;

internal sealed class ScheduleTaskContainer<TKey> : IDisposable
    where TKey : notnull
{
    private readonly Action<Task> _onActiveTaskCompleted;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private ReactiveScheduler<TKey>? _owner;
    private Queue<KeyValuePair<long, Timestamp>>? _awaitingInvocations;
    private Dictionary<int, TaskEntry>? _activeTasks;
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
    private bool _disposed;

    internal ScheduleTaskContainer(ReactiveScheduler<TKey> owner, IScheduleTask<TKey> source)
    {
        _owner = owner;
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
        _minElapsedTime = Duration.MaxValue;
        _maxElapsedTime = Duration.MinValue;
        _averageElapsedTime = FloatingDuration.Zero;
        Source = source;
        Key = Source.Key;
        _cancellationTokenSource = new CancellationTokenSource();
        _disposed = false;

        _onActiveTaskCompleted = t =>
        {
            using ( ExclusiveLock.Enter( this ) )
            {
                if ( _activeTasks is null )
                    return;

                if ( _activeTasks.Remove( t.Id, out var entry ) )
                    FinishCompletedInvocation(
                        entry.Invocation,
                        entry.StartTimestamp,
                        t.Exception,
                        t.IsCanceled ? TaskCancellationReason.CancellationRequested : null );

                var maxConcurrentInvocations = Source.MaxConcurrentInvocations;
                while ( _activeTasks is not null
                    && _awaitingInvocations is not null
                    && _activeTasks.Count < maxConcurrentInvocations
                    && _awaitingInvocations.TryDequeue( out var invocation ) )
                {
                    Assume.IsNotNull( _owner );

                    ++_delayedInvocations;
                    var now = _owner.Timestamps.GetNow();
                    if ( _disposed )
                        FinishSkippedInvocation( invocation.Key, invocation.Value, now, TaskCancellationReason.TaskDisposed );
                    else
                        ActivateTask( new ReactiveTaskInvocationParams( invocation.Key, invocation.Value, now ) );
                }
            }
        };
    }

    internal TKey Key { get; }
    internal IScheduleTask<TKey> Source { get; }

    private bool HasActiveInvocations => _activeInvocations > 0
        || (_activeTasks is not null && _activeTasks.Count > 0)
        || (_awaitingInvocations is not null && _awaitingInvocations.Count > 0);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            if ( _disposed )
                return;

            _disposed = true;
            var errors = Chain<Exception>.Empty;
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch ( Exception exc )
            {
                errors = errors.Extend( exc );
            }

            if ( ! HasActiveInvocations && _owner is not null && _owner.TryRemoveFinishedTask( this ) )
            {
                var exc = FinalizeDisposal();
                if ( exc is not null )
                    errors = errors.Extend( exc );
            }

            if ( errors.Count > 0 )
                throw new AggregateException( errors );
        }
    }

    internal void EnqueueInvocation(Timestamp timestamp, Timestamp expectedTimestamp)
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            if ( _disposed )
                return;

            var maxConcurrentInvocations = Source.MaxConcurrentInvocations;
            if ( maxConcurrentInvocations <= 0 )
                return;

            _firstInvocationTimestamp ??= timestamp;
            _lastInvocationTimestamp = timestamp;
            var invocationId = _totalInvocations++;

            _activeTasks ??= new Dictionary<int, TaskEntry>();
            if ( _activeTasks.Count < maxConcurrentInvocations )
            {
                ActivateTask( new ReactiveTaskInvocationParams( invocationId, expectedTimestamp, timestamp ) );
                return;
            }

            var maxEnqueuedInvocations = Source.MaxEnqueuedInvocations;
            if ( maxEnqueuedInvocations <= 0 )
            {
                FinishSkippedInvocation( invocationId, expectedTimestamp, timestamp, TaskCancellationReason.MaxQueueSizeLimit );
                return;
            }

            _awaitingInvocations ??= new Queue<KeyValuePair<long, Timestamp>>();
            var enqueue = OnEnqueue(
                new ReactiveTaskInvocationParams( invocationId, expectedTimestamp, timestamp ),
                _awaitingInvocations.Count );

            if ( ! enqueue )
            {
                FinishSkippedInvocation( invocationId, expectedTimestamp, timestamp, TaskCancellationReason.CancellationRequested );
                return;
            }

            if ( _awaitingInvocations is null )
                return;

            _awaitingInvocations.Enqueue( KeyValuePair.Create( invocationId, expectedTimestamp ) );
            while ( _awaitingInvocations.Count > maxEnqueuedInvocations )
            {
                var skipped = _awaitingInvocations.Dequeue();
                FinishSkippedInvocation( skipped.Key, skipped.Value, timestamp, TaskCancellationReason.MaxQueueSizeLimit );
            }

            _maxQueuedInvocations = Math.Max( _maxQueuedInvocations, _awaitingInvocations.Count );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReactiveTaskSnapshot<IScheduleTask<TKey>> CreateStateSnapshot()
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            return new ReactiveTaskSnapshot<IScheduleTask<TKey>>(
                Source,
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
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ActivateTask(ReactiveTaskInvocationParams parameters)
    {
        Assume.False( _disposed );
        Assume.IsNotNull( _owner );
        Assume.IsNotNull( _activeTasks );

        Task task;
        var startTimestamp = Stopwatch.GetTimestamp();
        try
        {
            ++_activeInvocations;
            try
            {
                task = Source.InvokeAsync( _owner, parameters, _cancellationTokenSource.Token );
            }
            finally
            {
                --_activeInvocations;
            }

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

        if ( _activeTasks is null )
            return;

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
    private void FinishSkippedInvocation(
        long invocationId,
        Timestamp originalTimestamp,
        Timestamp invocationTimestamp,
        TaskCancellationReason cancellationReason)
    {
        var parameters = new ReactiveTaskCompletionParams(
            new ReactiveTaskInvocationParams( invocationId, originalTimestamp, invocationTimestamp ),
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
            Source.OnCompleted( _owner, parameters );
        }
        catch
        {
            // NOTE:
            // silently ignore all exceptions, otherwise the whole scheduler would get derailed
            // this is by design, no exceptions should be thrown by the OnCompleted invocation
        }

        if ( ! HasActiveInvocations && _owner is not null && _owner.TryRemoveFinishedTask( this ) )
            FinalizeDisposal();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool OnEnqueue(ReactiveTaskInvocationParams parameters, int positionInQueue)
    {
        if ( _owner is null )
            return false;

        try
        {
            return Source.OnEnqueue( _owner, parameters, positionInQueue );
        }
        catch
        {
            // NOTE:
            // silently ignore all exceptions, otherwise the whole scheduler would get derailed
            // this is by design, no exceptions should be thrown by the OnEnqueue invocation
        }

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Exception? FinalizeDisposal()
    {
        _disposed = true;
        _owner = null;
        _awaitingInvocations = null;
        _activeTasks = null;

        Exception? exception = null;
        try
        {
            Source.Dispose();
        }
        catch ( Exception exc )
        {
            exception = exc;
        }
        finally
        {
            _cancellationTokenSource.Dispose();
        }

        return exception;
    }

    private readonly record struct TaskEntry(Task Task, ReactiveTaskInvocationParams Invocation, long StartTimestamp);
}
