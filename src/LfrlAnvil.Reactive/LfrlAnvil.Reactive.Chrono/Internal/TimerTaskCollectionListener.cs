using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;

namespace LfrlAnvil.Reactive.Chrono.Internal;

internal sealed class TimerTaskCollectionListener : EventListener<WithInterval<long>>
{
    internal readonly CancellationTokenSource CancellationTokenSource;
    internal TimerTaskContainer[] TaskContainers;
    private Timestamp? _firstTimestamp;
    private Timestamp? _lastTimestamp;
    private long _eventCount;

    internal TimerTaskCollectionListener(IEnumerable<ITimerTask> tasks)
    {
        _firstTimestamp = null;
        _lastTimestamp = null;
        _eventCount = 0;
        CancellationTokenSource = new CancellationTokenSource();
        TaskContainers = tasks.Select( t => new TimerTaskContainer( this, t ) ).ToArray();
    }

    internal bool ContainsTasks => TaskContainers.Length > 0;

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
            if ( TaskContainers.Length == 0 )
                return;

            _firstTimestamp ??= @event.Timestamp;
            _lastTimestamp = @event.Timestamp;
            ++_eventCount;

            foreach ( var container in TaskContainers )
                container.EnqueueInvocation( @event.Timestamp );
        }
    }

    public override void OnDispose(DisposalSource source)
    {
        using ( ExclusiveLock.Enter( this ) )
        {
            foreach ( var container in TaskContainers )
                container.Dispose();

            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();

            TaskContainers = Array.Empty<TimerTaskContainer>();
        }
    }
}
