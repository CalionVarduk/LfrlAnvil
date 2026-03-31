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
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
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
    private readonly FrozenDictionary<TKey, TimerTaskContainer<TKey>> _taskContainersByKey;

    internal TimerTaskCollection(
        IEventStream<WithInterval<long>> stream,
        IEnumerable<ITimerTask<TKey>> tasks,
        Duration taskDisposalTimeout)
    {
        TaskDisposalTimeout = taskDisposalTimeout.Max( Duration.Zero );

        CancellationTokenSource = new CancellationTokenSource();
        var taskContainers = tasks.Select( t => new TimerTaskContainer<TKey>( this, t ) ).ToArray();

        if ( taskContainers.Length == 0 )
        {
            _taskContainersByKey = FrozenDictionary<TKey, TimerTaskContainer<TKey>>.Empty;
            CancellationTokenSource.Dispose();
        }
        else
        {
            _listener = new TimerTaskCollectionListener<TKey>( this, taskContainers );
            try
            {
                _taskContainersByKey = taskContainers.ToFrozenDictionary( static c => c.Key );
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
    /// Max time the disposal of timer tasks will wait for their invocations to complete.
    /// </summary>
    public Duration TaskDisposalTimeout { get; }

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
