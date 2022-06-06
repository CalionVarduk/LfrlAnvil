using System.Threading;

namespace LfrlAnvil.Reactive.Internal
{
    internal sealed class EventSubscriber<TEvent> : IEventSubscriber
    {
        private EventSource<TEvent>? _source;
        private int _state;

        internal EventSubscriber(EventSource<TEvent> source, IEventListener<TEvent> listener)
        {
            _source = source;
            Listener = listener;
            _state = 0;
        }

        internal IEventListener<TEvent> Listener { get; set; }
        public bool IsDisposed => _state == 1;

        public void Dispose()
        {
            if ( Interlocked.Exchange( ref _state, 1 ) == 1 )
                return;

            _source!.RemoveSubscriber( this );
            _source = null;

            Listener.OnDispose( DisposalSource.Subscriber );
        }

        internal void MarkAsDisposed()
        {
            Interlocked.Exchange( ref _state, 1 );
            _source = null;
        }
    }
}
