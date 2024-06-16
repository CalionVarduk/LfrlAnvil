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
using System.Linq;

namespace LfrlAnvil.Reactive.Internal;

/// <summary>
/// Represents a generic disposable event source that can be listened to,
/// that notifies its listeners immediately with all stored events sequentially, and then disposes the listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EnumerableEventSource<TEvent> : EventSource<TEvent>
{
    private readonly TEvent[] _values;

    internal EnumerableEventSource(IEnumerable<TEvent> values)
    {
        _values = values.ToArray();
    }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        base.OnDispose();
        Array.Clear( _values, 0, _values.Length );
    }

    /// <inheritdoc />
    protected override void OnSubscriberAdded(IEventSubscriber subscriber, IEventListener<TEvent> listener)
    {
        base.OnSubscriberAdded( subscriber, listener );

        foreach ( var value in _values )
        {
            if ( subscriber.IsDisposed )
                return;

            listener.React( value );
        }

        subscriber.Dispose();
    }
}
