using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.Reactive.Decorators;

public sealed class EventListenerPrependDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly TEvent[] _values;

    public EventListenerPrependDecorator(IEnumerable<TEvent> values)
    {
        _values = values.ToArray();
    }

    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        foreach ( var value in _values )
            listener.React( value );

        return listener;
    }
}