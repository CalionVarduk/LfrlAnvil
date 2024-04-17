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

internal sealed class TimerTaskContainer : IDisposable
{
    private readonly Action<Task> _onActiveTaskCompleted;
    private TimerTaskCollectionListener? _owner;
    private Queue<Timestamp>? _awaitingInvocations;
    private Dictionary<int, TaskEntry>? _activeTasks;
    private long _totalInvocations;
    private long _delayedInvocations;
    private long _skippedInvocations;
    private long _failedInvocations;
    private long _cancelledInvocations;
    private long _maxQueuedInvocations;
    private long _maxActiveTasks;
    private Timestamp? _firstInvocationTimestamp;
    private Timestamp? _lastInvocationTimestamp;
    private Duration _minElapsedTime;
    private Duration _maxElapsedTime;
    private FloatingDuration _averageElapsedTime;

    internal TimerTaskContainer(TimerTaskCollectionListener owner, ITimerTask source)
    {
        _owner = owner;
        _awaitingInvocations = null;
        _activeTasks = null;
        _totalInvocations = 0;
        _delayedInvocations = 0;
        _skippedInvocations = 0;
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

        _onActiveTaskCompleted = t =>
        {
            using ( ExclusiveLock.Enter( this ) )
            {
                Assume.IsNotNull( _activeTasks );
                if ( _activeTasks.Remove( t.Id, out var entry ) )
                    FinishInvocation( entry.Invocation, entry.StartTimestamp, t.Exception, t.IsCanceled );

                if ( _awaitingInvocations is null )
                    return;

                var maxConcurrentInvocations = Source.MaxConcurrentInvocations;
                while ( _activeTasks.Count < maxConcurrentInvocations && _awaitingInvocations.TryDequeue( out var timestamp ) )
                {
                    ++_delayedInvocations;
                    Assume.IsNotNull( _owner.LastTimestamp );
                    ActivateTask( timestamp, _owner.LastTimestamp.Value );
                }
            }
        };
    }

    internal ITimerTask Source { get; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            if ( _owner is null )
                return;

            _owner = null;
            _skippedInvocations += _awaitingInvocations?.Count ?? 0;
            _awaitingInvocations = null;
            Source.Dispose();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void EnqueueInvocation(Timestamp timestamp)
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            var nextInvocationTimestamp = Source.NextInvocationTimestamp;
            if ( nextInvocationTimestamp is not null && nextInvocationTimestamp.Value > timestamp )
                return;

            var maxConcurrentInvocations = Source.MaxConcurrentInvocations;
            if ( maxConcurrentInvocations <= 0 )
            {
                ++_skippedInvocations;
                return;
            }

            _activeTasks ??= new Dictionary<int, TaskEntry>();
            if ( _activeTasks.Count < maxConcurrentInvocations )
            {
                ActivateTask( timestamp, timestamp );
                return;
            }

            var maxEnqueuedInvocations = Source.MaxEnqueuedInvocations;
            if ( maxEnqueuedInvocations <= 0 )
            {
                ++_skippedInvocations;
                return;
            }

            _awaitingInvocations ??= new Queue<Timestamp>();
            _awaitingInvocations.Enqueue( timestamp );
            while ( _awaitingInvocations.Count > maxEnqueuedInvocations )
            {
                _awaitingInvocations.Dequeue();
                ++_skippedInvocations;
            }

            _maxQueuedInvocations = Math.Max( _maxQueuedInvocations, _awaitingInvocations.Count );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal TimerTaskStateSnapshot CreateStateSnapshot()
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            return new TimerTaskStateSnapshot(
                Source,
                _firstInvocationTimestamp,
                _lastInvocationTimestamp,
                _totalInvocations,
                _delayedInvocations,
                _skippedInvocations,
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
    private void ActivateTask(Timestamp originalTimestamp, Timestamp invocationTimestamp)
    {
        Assume.IsNotNull( _owner );
        Assume.IsNotNull( _activeTasks );

        _firstInvocationTimestamp ??= originalTimestamp;
        _lastInvocationTimestamp = originalTimestamp;
        var parameters = new ReactiveTaskInvocationParams( _totalInvocations++, originalTimestamp, invocationTimestamp );

        Task task;
        var startTimestamp = Stopwatch.GetTimestamp();
        try
        {
            task = Source.InvokeAsync( parameters, _owner.CancellationTokenSource.Token );
            if ( task.Status == TaskStatus.Created )
                task.Start( TaskScheduler.Default );
        }
        catch ( Exception exc )
        {
            FinishInvocation( parameters, startTimestamp, exc, false );
            return;
        }

        if ( task.IsCompleted )
        {
            FinishInvocation( parameters, startTimestamp, null, false );
            return;
        }

        _activeTasks[task.Id] = new TaskEntry( task, parameters, startTimestamp );
        _maxActiveTasks = Math.Max( _maxActiveTasks, _activeTasks.Count );
        task.ContinueWith( _onActiveTaskCompleted, TaskContinuationOptions.ExecuteSynchronously );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FinishInvocation(ReactiveTaskInvocationParams invocation, long startTimestamp, Exception? exception, bool isCancelled)
    {
        Assume.IsNotNull( _activeTasks );
        var elapsedTime = new Duration( StopwatchTimestamp.GetTimeSpan( startTimestamp, Stopwatch.GetTimestamp() ) );
        _minElapsedTime = _minElapsedTime.Min( elapsedTime );
        _maxElapsedTime = _maxElapsedTime.Max( elapsedTime );
        _averageElapsedTime += (elapsedTime - _averageElapsedTime) / (_totalInvocations - _activeTasks.Count);
        var parameters = new ReactiveTaskCompletionParams( invocation, elapsedTime, exception, isCancelled );

        if ( exception is not null )
            ++_failedInvocations;

        if ( isCancelled )
            ++_cancelledInvocations;

        try
        {
            Source.OnCompleted( parameters );
        }
        catch
        {
            // NOTE:
            // swallow silently, otherwise the whole timer task collection would get derailed
            // by design, no exceptions should be thrown by the OnCompleted invocation
        }
    }

    private readonly record struct TaskEntry(Task Task, ReactiveTaskInvocationParams Invocation, long StartTimestamp);
}
