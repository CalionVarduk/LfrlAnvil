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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.Reactive.Chrono.Composites;

namespace LfrlAnvil.Reactive.Chrono.Internal;

internal sealed class TimerTaskCollectionListener<TKey> : EventListener<WithInterval<long>>
    where TKey : notnull
{
    private readonly TaskCompletionSource _disposed;
    private TimerTaskContainer<TKey>[] _taskContainers;
    private TimerTaskCollection<TKey>? _owner;
    private Timestamp? _firstTimestamp;
    private Timestamp? _lastTimestamp;
    private long _eventCount;

    internal TimerTaskCollectionListener(TimerTaskCollection<TKey> owner, TimerTaskContainer<TKey>[] taskContainers)
    {
        Assume.IsNotEmpty( taskContainers );
        _firstTimestamp = null;
        _lastTimestamp = null;
        _eventCount = 0;
        _owner = owner;
        _taskContainers = taskContainers;
        _disposed = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
    }

    internal Timestamp? FirstTimestamp
    {
        get
        {
            using ( ExclusiveLock.Enter( this ) )
                return _firstTimestamp;
        }
    }

    internal Timestamp? LastTimestamp
    {
        get
        {
            using ( ExclusiveLock.Enter( this ) )
                return _lastTimestamp;
        }
    }

    internal long EventCount
    {
        get
        {
            using ( ExclusiveLock.Enter( this ) )
                return _eventCount;
        }
    }

    public override void React(WithInterval<long> @event)
    {
        TimerTaskContainer<TKey>[] taskContainers;
        using ( ExclusiveLock.Enter( this ) )
        {
            if ( _owner is null )
                return;

            _firstTimestamp ??= @event.Timestamp;
            _lastTimestamp = @event.Timestamp;
            ++_eventCount;
            taskContainers = _taskContainers;
        }

        foreach ( var container in taskContainers )
            container.EnqueueInvocation( @event.Timestamp );
    }

    public override void OnDispose(DisposalSource source)
    {
        var errors = Chain<Exception>.Empty;
        TimerTaskContainer<TKey>[] taskContainers;
        CancellationTokenSource cancellationTokenSource;
        TaskCompletionSource disposed;
        Duration taskDisposalTimeout;

        using ( ExclusiveLock.Enter( this ) )
        {
            if ( _owner is null )
                return;

            disposed = _disposed;
            taskContainers = _taskContainers;
            _taskContainers = Array.Empty<TimerTaskContainer<TKey>>();
            cancellationTokenSource = _owner.CancellationTokenSource;
            taskDisposalTimeout = _owner.TaskDisposalTimeout;
            _owner.Subscriber = null;
            _owner = null;
        }

        try
        {
            BeginDispose( taskContainers, cancellationTokenSource, taskDisposalTimeout, ref errors );
            if ( taskDisposalTimeout > Duration.Zero )
                errors = WaitForDisposalAsync( taskContainers, taskDisposalTimeout, errors ).GetAwaiter().GetResult();

            if ( errors.Count > 0 )
                throw errors.Consolidate()!;
        }
        finally
        {
            disposed.TrySetResult();
        }
    }

    internal async ValueTask DisposeAsyncCore()
    {
        var errors = Chain<Exception>.Empty;
        TimerTaskContainer<TKey>[]? taskContainers;
        TimerTaskCollection<TKey>? owner;
        IEventSubscriber? subscriber;
        TaskCompletionSource disposed;

        using ( ExclusiveLock.Enter( this ) )
        {
            disposed = _disposed;
            if ( _owner is null )
            {
                taskContainers = null;
                owner = null;
                subscriber = null;
            }
            else
            {
                taskContainers = _taskContainers;
                _taskContainers = Array.Empty<TimerTaskContainer<TKey>>();
                owner = _owner;
                subscriber = owner.Subscriber;
                _owner.Subscriber = null;
                _owner = null;
            }
        }

        if ( owner is null )
        {
            await disposed.Task.ConfigureAwait( false );
            return;
        }

        Assume.IsNotNull( taskContainers );

        try
        {
            subscriber?.Dispose();
            BeginDispose( taskContainers, owner.CancellationTokenSource, owner.TaskDisposalTimeout, ref errors );

            if ( owner.TaskDisposalTimeout > Duration.Zero )
                errors = await WaitForDisposalAsync( taskContainers, owner.TaskDisposalTimeout, errors ).ConfigureAwait( false );

            if ( errors.Count > 0 )
                throw errors.Consolidate()!;
        }
        finally
        {
            disposed.TrySetResult();
        }
    }

    private static void BeginDispose(
        TimerTaskContainer<TKey>[] containers,
        CancellationTokenSource cts,
        Duration taskDisposalTimeout,
        ref Chain<Exception> errors)
    {
        foreach ( var container in containers )
        {
            try
            {
                container.BeginDispose( taskDisposalTimeout );
            }
            catch ( Exception exc )
            {
                errors = errors.Extend( exc );
            }
        }

        try
        {
            cts.Cancel();
        }
        catch ( Exception exc )
        {
            errors = errors.Extend( exc );
        }

        try
        {
            cts.Dispose();
        }
        catch ( Exception exc )
        {
            errors = errors.Extend( exc );
        }
    }

    private static async Task<Chain<Exception>> WaitForDisposalAsync(
        TimerTaskContainer<TKey>[] containers,
        Duration taskDisposalTimeout,
        Chain<Exception> errors)
    {
        Assume.IsGreaterThan( taskDisposalTimeout, Duration.Zero );
        try
        {
            await Task.WhenAll( containers.Select( c => c.WaitForDisposalAsync( taskDisposalTimeout ) ) ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            errors = errors.Extend( exc );
        }

        return errors;
    }
}
