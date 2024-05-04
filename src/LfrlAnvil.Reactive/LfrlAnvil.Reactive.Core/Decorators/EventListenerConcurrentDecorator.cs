using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Encapsulates decorated event listener's reactions to events and disposal in an exclusive lock.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerConcurrentDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly object? _sync;

    /// <summary>
    /// Creates a new <see cref="EventListenerConcurrentDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="sync">Optional synchronization object.</param>
    public EventListenerConcurrentDecorator(object? sync)
    {
        _sync = sync;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _sync );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly object _sync;
        private bool _disposed;

        internal EventListener(IEventListener<TEvent> next, object? sync)
            : base( next )
        {
            _sync = sync ?? new object();
            _disposed = false;
        }

        public override void React(TEvent @event)
        {
            lock ( _sync )
            {
                if ( _disposed )
                    return;

                Next.React( @event );
            }
        }

        public override void OnDispose(DisposalSource source)
        {
            lock ( _sync )
            {
                if ( _disposed )
                    return;

                _disposed = true;
                base.OnDispose( source );
            }
        }
    }
}
