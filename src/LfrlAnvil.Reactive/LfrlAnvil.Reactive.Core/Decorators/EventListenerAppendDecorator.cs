// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Appends a collection of values to notify the decorated event listener sequentially with once the listener gets disposed.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerAppendDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly TEvent[] _values;

    /// <summary>
    /// Creates a new <see cref="EventListenerAppendDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="values">Collection of values to append.</param>
    public EventListenerAppendDecorator(IEnumerable<TEvent> values)
    {
        _values = values.ToArray();
    }

    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return _values.Length == 0 ? listener : new EventListener( listener, _values );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private TEvent[]? _values;

        internal EventListener(IEventListener<TEvent> next, TEvent[] values)
            : base( next )
        {
            _values = values;
        }

        public override void React(TEvent @event)
        {
            Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            Assume.IsNotNull( _values );
            foreach ( var value in _values )
                Next.React( value );

            _values = null;

            base.OnDispose( source );
        }
    }
}
