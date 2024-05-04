using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Skips the specified number of events at the beginning of the sequence before starting to notify the decorated event listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerSkipDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _count;

    /// <summary>
    /// Creates a new <see cref="EventListenerSkipDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="count">Number of events at the beginning of the sequence to skip.</param>
    public EventListenerSkipDecorator(int count)
    {
        _count = count;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return _count <= 0 ? listener : new EventListener( listener, _count );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly int _count;
        private int _skipped;

        internal EventListener(IEventListener<TEvent> next, int count)
            : base( next )
        {
            _count = count;
            _skipped = 0;
        }

        public override void React(TEvent @event)
        {
            if ( _skipped == _count )
            {
                Next.React( @event );
                return;
            }

            ++_skipped;
        }
    }
}
