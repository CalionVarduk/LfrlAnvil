using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Internal;

namespace LfrlAnvil.Reactive.Chrono;

public sealed class TimerTaskCollection<TKey> : IDisposable
    where TKey : notnull
{
    private readonly IEventSubscriber? _subscriber;
    private readonly TimerTaskCollectionListener? _listener;
    private readonly Dictionary<TKey, TimerTaskContainer> _taskContainersByKey;

    internal TimerTaskCollection(IEventStream<WithInterval<long>> stream, IEnumerable<ITimerTask<TKey>> tasks)
    {
        var listener = new TimerTaskCollectionListener( tasks );
        if ( listener.ContainsTasks )
        {
            _listener = listener;
            _taskContainersByKey = listener.TaskContainers
                .ToDictionary( static c => ReinterpretCast.To<ITimerTask<TKey>>( c.Source ).Key );

            _subscriber = stream.Listen( listener );
        }
        else
        {
            _taskContainersByKey = new Dictionary<TKey, TimerTaskContainer>();
            listener.OnDispose( DisposalSource.Subscriber );
        }
    }

    public Timestamp? FirstTimestamp => _listener?.FirstTimestamp;
    public Timestamp? LastTimestamp => _listener?.LastTimestamp;
    public long EventCount => _listener?.EventCount ?? 0;
    public IReadOnlyCollection<TKey> TaskKeys => _taskContainersByKey.Keys;

    public void Dispose()
    {
        _subscriber?.Dispose();
    }

    [Pure]
    public TimerTaskStateSnapshot? TryGetTaskState(TKey key)
    {
        return _taskContainersByKey.GetValueOrDefault( key )?.CreateStateSnapshot();
    }
}
