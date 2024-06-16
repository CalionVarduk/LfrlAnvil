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
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;

namespace LfrlAnvil.Reactive.Chrono.Internal;

internal sealed class TimerTaskCollectionListener<TKey> : EventListener<WithInterval<long>>
    where TKey : notnull
{
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
        using ( ExclusiveLock.Enter( this ) )
        {
            if ( _owner is null )
                return;

            _firstTimestamp ??= @event.Timestamp;
            _lastTimestamp = @event.Timestamp;
            ++_eventCount;

            foreach ( var container in _taskContainers )
                container.EnqueueInvocation( @event.Timestamp );
        }
    }

    public override void OnDispose(DisposalSource source)
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            if ( _owner is null )
                return;

            var errors = Chain<Exception>.Empty;
            var owner = _owner;
            _owner = null;
            owner.Subscriber = null;

            foreach ( var container in _taskContainers )
            {
                try
                {
                    container.Dispose();
                }
                catch ( Exception exc )
                {
                    errors = errors.Extend( exc );
                }
            }

            try
            {
                owner.CancellationTokenSource.Cancel();
            }
            catch ( Exception exc )
            {
                errors = errors.Extend( exc );
            }
            finally
            {
                owner.CancellationTokenSource.Dispose();
            }

            _taskContainers = Array.Empty<TimerTaskContainer<TKey>>();

            if ( errors.Count > 0 )
                throw new AggregateException( errors );
        }
    }
}
