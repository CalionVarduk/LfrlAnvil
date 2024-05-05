using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Internal;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents an attached collection of <see cref="ITimerTask{TKey}"/> instances.
/// </summary>
/// <typeparam name="TKey">Task key type.</typeparam>
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

    /// <summary>
    /// First <see cref="Timestamp"/> of an event emitted by the source event stream.
    /// </summary>
    public Timestamp? FirstTimestamp => _listener?.FirstTimestamp;

    /// <summary>
    /// Last <see cref="Timestamp"/> of an event emitted by the source event stream.
    /// </summary>
    public Timestamp? LastTimestamp => _listener?.LastTimestamp;

    /// <summary>
    /// Number of events emitted by the source event stream.
    /// </summary>
    public long EventCount => _listener?.EventCount ?? 0;

    /// <summary>
    /// Collection of registered task keys.
    /// </summary>
    public IReadOnlyCollection<TKey> TaskKeys => _taskContainersByKey.Keys;

    /// <inheritdoc />
    public void Dispose()
    {
        Subscriber?.Dispose();
    }

    /// <summary>
    /// Attempts to create a <see cref="ReactiveTaskSnapshot{TTask}"/> instance for the given task <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Task key to create a snapshot for.</param>
    /// <returns>New <see cref="ReactiveTaskSnapshot{TTask}"/> instance or null when task does not exist.</returns>
    [Pure]
    public ReactiveTaskSnapshot<ITimerTask<TKey>>? TryGetTaskSnapshot(TKey key)
    {
        return _taskContainersByKey.GetValueOrDefault( key )?.CreateStateSnapshot();
    }
}
