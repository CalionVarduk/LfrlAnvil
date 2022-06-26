using System;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.Decorators;

public class EventListenerTakeLastDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _count;

    public EventListenerTakeLastDecorator(int count)
    {
        _count = Math.Max( count, 0 );
    }

    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _count );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly LinkedList<TEvent> _last;
        private readonly int _count;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, int count)
            : base( next )
        {
            _last = new LinkedList<TEvent>();
            _count = count;

            if ( _count == 0 )
                subscriber.Dispose();
        }

        public override void React(TEvent @event)
        {
            if ( _last.Count == _count )
                _last.RemoveFirst();

            _last.AddLast( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            foreach ( var @event in _last )
                Next.React( @event );

            _last.Clear();
            base.OnDispose( source );
        }
    }
}