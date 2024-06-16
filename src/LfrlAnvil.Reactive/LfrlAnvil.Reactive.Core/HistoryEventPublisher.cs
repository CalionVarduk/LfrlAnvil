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
using System.Collections.Generic;

namespace LfrlAnvil.Reactive;

/// <inheritdoc cref="IHistoryEventPublisher{TEvent}" />
public class HistoryEventPublisher<TEvent> : EventPublisher<TEvent>, IHistoryEventPublisher<TEvent>
{
    private readonly Queue<TEvent> _history;

    /// <summary>
    /// Creates a new <see cref="HistoryEventPublisher{TEvent}"/> instance.
    /// </summary>
    /// <param name="capacity">Specifies the maximum number of events this event publisher can record.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="capacity"/> is less than <b>1</b>.</exception>
    public HistoryEventPublisher(int capacity)
    {
        Ensure.IsGreaterThan( capacity, 0 );
        Capacity = capacity;
        _history = new Queue<TEvent>();
    }

    /// <inheritdoc />
    public int Capacity { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<TEvent> History => _history;

    /// <inheritdoc />
    public void ClearHistory()
    {
        _history.Clear();
    }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        base.OnDispose();
        ClearHistory();
        _history.TrimExcess();
    }

    /// <inheritdoc />
    protected override void OnPublish(TEvent @event)
    {
        if ( _history.Count == Capacity )
            _history.Dequeue();

        _history.Enqueue( @event );
        base.OnPublish( @event );
    }

    /// <inheritdoc />
    protected override void OnSubscriberAdded(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        base.OnSubscriberAdded( subscriber, listener );

        foreach ( var @event in _history )
        {
            if ( subscriber.IsDisposed )
                return;

            listener.React( @event );
        }
    }
}
