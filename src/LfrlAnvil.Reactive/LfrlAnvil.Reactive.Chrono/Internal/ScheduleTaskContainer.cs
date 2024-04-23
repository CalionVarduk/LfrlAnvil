using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Reactive.Chrono.Internal;

internal sealed class ScheduleTaskContainer<TKey> : IDisposable
    where TKey : notnull
{
    private readonly Action<Task> _onActiveTaskCompleted;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private ReactiveScheduler<TKey>? _owner;
    private ReactiveTaskInfo _info;
    private bool _disposed;

    internal ScheduleTaskContainer(ReactiveScheduler<TKey> owner, IScheduleTask<TKey> source)
    {
        _owner = owner;
        _info = ReactiveTaskInfo.Create();
        Source = source;
        Key = Source.Key;
        _cancellationTokenSource = new CancellationTokenSource();
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

                var maxConcurrentInvocations = Source.MaxConcurrentInvocations;
                while ( _info.TryDequeueDelayedInvocation( maxConcurrentInvocations, out var invocationId, out var invocationTimestamp ) )
                {
                    Assume.IsNotNull( _owner );
                    var now = _owner.Timestamps.GetNow();
                    if ( _disposed )
                        FinishSkippedInvocation( invocationId, invocationTimestamp, now, TaskCancellationReason.TaskDisposed );
                    else
                        ActivateTask( new ReactiveTaskInvocationParams( invocationId, invocationTimestamp, now ) );
                }
            }
        };
    }

    internal TKey Key { get; }
    internal IScheduleTask<TKey> Source { get; }

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

            if ( ! _info.HasActiveInvocations && _owner is not null && _owner.TryRemoveFinishedTask( this ) )
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

            var invocationId = _info.GetNextInvocationId( timestamp );
            if ( _info.CanActivateTask( maxConcurrentInvocations ) )
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

            var enqueue = OnEnqueue(
                new ReactiveTaskInvocationParams( invocationId, expectedTimestamp, timestamp ),
                _info.GetNextPositionInQueue() );

            if ( ! enqueue )
            {
                FinishSkippedInvocation( invocationId, expectedTimestamp, timestamp, TaskCancellationReason.CancellationRequested );
                return;
            }

            if ( ! _info.TryEnqueueInvocation( invocationId, expectedTimestamp ) )
                return;

            while ( _info.TryDequeueSkippedInvocation(
                maxEnqueuedInvocations,
                out var skippedInvocationId,
                out var skippedInvocationTimestamp ) )
                FinishSkippedInvocation(
                    skippedInvocationId,
                    skippedInvocationTimestamp,
                    timestamp,
                    TaskCancellationReason.MaxQueueSizeLimit );

            _info.UpdateMaxQueuedInvocations();
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReactiveTaskSnapshot<IScheduleTask<TKey>> CreateStateSnapshot()
    {
        using ( ExclusiveLock.Enter( this ) )
            return _info.CreateSnapshot( Source );
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
                task = Source.InvokeAsync( _owner, parameters, _cancellationTokenSource.Token );
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

        if ( ! _info.HasActiveInvocations && _owner is not null && _owner.TryRemoveFinishedTask( this ) )
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
        _info.Clear();

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
}
