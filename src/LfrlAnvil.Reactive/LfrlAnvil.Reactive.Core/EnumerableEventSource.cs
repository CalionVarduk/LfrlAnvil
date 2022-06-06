using System;
using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.Reactive
{
    public sealed class EnumerableEventSource<TEvent> : EventSource<TEvent>
    {
        private readonly TEvent[] _values;

        public EnumerableEventSource(IEnumerable<TEvent> values)
        {
            _values = values.ToArray();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            Array.Clear( _values, 0, _values.Length );
        }

        protected override void OnSubscriberAdded(IEventSubscriber subscriber, IEventListener<TEvent> listener)
        {
            base.OnSubscriberAdded( subscriber, listener );

            foreach ( var value in _values )
            {
                if ( subscriber.IsDisposed )
                    return;

                listener.React( value );
            }

            subscriber.Dispose();
        }
    }
}
