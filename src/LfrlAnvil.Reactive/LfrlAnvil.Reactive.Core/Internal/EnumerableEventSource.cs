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
