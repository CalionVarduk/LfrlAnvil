namespace LfrlAnvil.Reactive.Events
{
    public interface IEventListenerDecorator<in TSourceEvent, out TNextEvent>
    {
        IEventListener<TSourceEvent> Decorate(IEventListener<TNextEvent> listener, IEventSubscriber subscriber);
    }
}
