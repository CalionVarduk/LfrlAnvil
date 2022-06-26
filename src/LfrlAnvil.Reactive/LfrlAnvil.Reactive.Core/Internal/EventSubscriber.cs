using System;
using System.Threading;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class EventSubscriber<TEvent> : IEventSubscriber
{
    private Action<EventSubscriber<TEvent>>? _disposer;
    private int _state;

    internal EventSubscriber(Action<EventSubscriber<TEvent>> disposer, IEventListener<TEvent> listener)
    {
        _disposer = disposer;
        Listener = listener;
        _state = 0;
    }

    internal IEventListener<TEvent> Listener { get; set; }
    public bool IsDisposed => _state == 1;

    public void Dispose()
    {
        if ( Interlocked.Exchange( ref _state, 1 ) == 1 )
            return;

        _disposer!( this );
        _disposer = null;

        Listener.OnDispose( DisposalSource.Subscriber );
    }

    internal void MarkAsDisposed()
    {
        Interlocked.Exchange( ref _state, 1 );
        _disposer = null;
    }
}