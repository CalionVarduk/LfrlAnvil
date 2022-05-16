namespace LfrlAnvil.Reactive.Events.Internal
{
    internal sealed class EventSubscriber<TEvent> : IEventSubscriber
    {
        private EventSource<TEvent>? _source;

        internal EventSubscriber(EventSource<TEvent> source, IEventListener<TEvent> listener)
        {
            _source = source;
            Listener = listener;
        }

        internal IEventListener<TEvent> Listener { get; set; }
        public bool IsDisposed => _source is null;

        public void Dispose()
        {
            if ( _source is null )
                return;

            _source.RemoveSubscriber( this );
            MarkAsDisposed();

            Listener.OnDispose( DisposalSource.Subscriber );
        }

        internal void MarkAsDisposed()
        {
            _source = null;
        }
    }
}
