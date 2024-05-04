using System;
using System.Threading;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Dispatches decorated event listener reactions to a <see cref="SynchronizationContext"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerUseSynchronizationContextDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly SynchronizationContext _context;

    /// <summary>
    /// Creates a new <see cref="EventListenerUseSynchronizationContextDecorator{TEvent}"/> instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">When <see cref="SynchronizationContext.Current"/> is null.</exception>
    public EventListenerUseSynchronizationContextDecorator()
    {
        var context = SynchronizationContext.Current;
        _context = context ?? throw new InvalidOperationException( Resources.CurrentSynchronizationContextCannotBeNull );
    }

    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _context );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly SynchronizationContext _context;

        internal EventListener(IEventListener<TEvent> next, SynchronizationContext context)
            : base( next )
        {
            _context = context;
        }

        public override void React(TEvent @event)
        {
            _context.Post( _ => Next.React( @event ), null );
        }

        public override void OnDispose(DisposalSource source)
        {
            _context.Post( _ => Next.OnDispose( source ), null );
        }
    }
}
