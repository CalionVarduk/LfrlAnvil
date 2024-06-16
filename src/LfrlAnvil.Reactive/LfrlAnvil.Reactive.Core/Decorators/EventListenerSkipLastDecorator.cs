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
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Skips the specified number of events at the end of the sequence. The decorated event listener will be notified with
/// a sequence of non-skipped events during its disposal.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerSkipLastDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _count;

    /// <summary>
    /// Creates a new <see cref="EventListenerSkipLastDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="count">Number of events at the end of the sequence to skip.</param>
    public EventListenerSkipLastDecorator(int count)
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
