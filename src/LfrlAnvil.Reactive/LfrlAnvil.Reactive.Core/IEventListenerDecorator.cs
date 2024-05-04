namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a decorator of <see cref="IEventListener{TEvent}"/> instances.
/// </summary>
/// <typeparam name="TSourceEvent">Source event type.</typeparam>
/// <typeparam name="TNextEvent">Next event type.</typeparam>
public interface IEventListenerDecorator<in TSourceEvent, out TNextEvent>
{
    /// <summary>
    /// Creates a new decorated <see cref="IEventListener{TEvent}"/> instance.
    /// </summary>
    /// <param name="listener">Source event listener.</param>
    /// <param name="subscriber">Event subscriber.</param>
    /// <returns>Decorated <see cref="IEventListener{TEvent}"/> instance.</returns>
    IEventListener<TSourceEvent> Decorate(IEventListener<TNextEvent> listener, IEventSubscriber subscriber);
}
