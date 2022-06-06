using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive
{
    public interface IEventStream
    {
        bool IsDisposed { get; }
        IEventSubscriber Listen(IEventListener listener);
    }

    public interface IEventStream<out TEvent> : IEventStream
    {
        IEventSubscriber Listen(IEventListener<TEvent> listener);

        [Pure]
        IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator);
    }
}
