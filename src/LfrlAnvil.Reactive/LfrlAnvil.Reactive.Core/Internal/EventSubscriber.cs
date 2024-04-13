using System;
using LfrlAnvil.Async;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class EventSubscriber<TEvent> : IEventSubscriber
{
    private Action<EventSubscriber<TEvent>>? _disposer;
    private InterlockedBoolean _isDisposed;

    internal EventSubscriber(Action<EventSubscriber<TEvent>> disposer, IEventListener<TEvent> listener)
    {
        _disposer = disposer;
        Listener = listener;
        _isDisposed = new InterlockedBoolean( false );
    }

    internal IEventListener<TEvent> Listener { get; set; }
    public bool IsDisposed => _isDisposed.Value;

    public void Dispose()
    {
        if ( ! _isDisposed.WriteTrue() )
            return;

        _disposer?.Invoke( this );
        _disposer = null;

        Listener.OnDispose( DisposalSource.Subscriber );
    }

    internal bool MarkAsDisposed()
    {
        if ( ! _isDisposed.WriteTrue() )
            return false;

        _disposer = null;
        return true;

    }
}
