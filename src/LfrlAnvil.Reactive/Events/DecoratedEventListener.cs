namespace LfrlAnvil.Reactive.Events
{
    public abstract class DecoratedEventListener<TSourceEvent, TNextEvent> : EventListener<TSourceEvent>
    {
        protected DecoratedEventListener(IEventListener<TNextEvent> next)
        {
            Next = next;
        }

        protected IEventListener<TNextEvent> Next { get; }

        public override void OnDispose()
        {
            Next.OnDispose();
        }
    }
}
