using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Events.Decorators
{
    public class EventListenerSkipLastDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
    {
        private readonly int _count;

        public EventListenerSkipLastDecorator(int count)
        {
            _count = count;
        }

        [Pure]
        public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
        {
            return _count <= 0 ? listener : new EventListener( listener, _count );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
        {
            private readonly List<TEvent> _buffer;
            private readonly int _count;

            internal EventListener(IEventListener<TEvent> next, int count)
                : base( next )
            {
                _buffer = new List<TEvent>();
                _count = count;
            }

            public override void React(TEvent @event)
            {
                _buffer.Add( @event );
            }

            public override void OnDispose(DisposalSource source)
            {
                var count = _buffer.Count - _count;
                for ( var i = 0; i < count; ++i )
                    Next.React( _buffer[i] );

                _buffer.Clear();
                _buffer.TrimExcess();

                base.OnDispose( source );
            }
        }
    }
}
