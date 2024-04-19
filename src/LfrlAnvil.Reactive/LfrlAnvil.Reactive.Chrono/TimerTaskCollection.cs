using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Internal;

namespace LfrlAnvil.Reactive.Chrono;

public sealed class TimerTaskCollection<TKey> : IDisposable
    where TKey : notnull
{
    internal readonly CancellationTokenSource CancellationTokenSource;
    internal IEventSubscriber? Subscriber;
    private readonly TimerTaskCollectionListener<TKey>? _listener;
    private readonly Dictionary<TKey, TimerTaskContainer<TKey>> _taskContainersByKey;

    internal TimerTaskCollection(IEventStream<WithInterval<long>> stream, IEnumerable<ITimerTask<TKey>> tasks)
    {
        CancellationTokenSource = new CancellationTokenSource();
        var taskContainers = tasks.Select( t => new TimerTaskContainer<TKey>( this, t ) ).ToArray();

        if ( taskContainers.Length == 0 )
        {
            _taskContainersByKey = new Dictionary<TKey, TimerTaskContainer<TKey>>();
            CancellationTokenSource.Dispose();
        }
        else
        {
            _listener = new TimerTaskCollectionListener<TKey>( this, taskContainers );
            try
            {
                _taskContainersByKey = taskContainers.ToDictionary( static c => c.Key );
                Subscriber = stream.Listen( _listener );
            }
            catch
            {
                _listener.OnDispose( DisposalSource.Subscriber );
                _listener = null;
                throw;
            }
        }
    }

    public Timestamp? FirstTimestamp => _listener?.FirstTimestamp;
    public Timestamp? LastTimestamp => _listener?.LastTimestamp;
    public long EventCount => _listener?.EventCount ?? 0;
    public IReadOnlyCollection<TKey> TaskKeys => _taskContainersByKey.Keys;

    public void Dispose()
    {
        Subscriber?.Dispose();
    }

    [Pure]
    public TimerTaskStateSnapshot<TKey>? TryGetTaskState(TKey key)
    {
        return _taskContainersByKey.GetValueOrDefault( key )?.CreateStateSnapshot();
    }
}
