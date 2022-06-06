namespace LfrlAnvil.Reactive
{
    public interface IEventListenerDecorator<in TSourceEvent, out TNextEvent>
    {
        IEventListener<TSourceEvent> Decorate(IEventListener<TNextEvent> listener, IEventSubscriber subscriber);
    }
}
