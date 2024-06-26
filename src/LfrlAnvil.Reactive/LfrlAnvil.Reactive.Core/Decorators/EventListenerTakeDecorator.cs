﻿// Copyright 2024 Łukasz Furlepa
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

using System;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Notifies the decorated event listener only with the specified number of events emitted at the beginning of the sequence,
/// before disposing the subscriber.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventListenerTakeDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly int _count;

    /// <summary>
    /// Creates a new <see cref="EventListenerTakeDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="count">Number of events at the beginning of the sequence to take.</param>
    public EventListenerTakeDecorator(int count)
    {
        _count = Math.Max( count, 0 );
    }

    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, subscriber, _count );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly IEventSubscriber _subscriber;
        private readonly int _count;
        private int _taken;

        internal EventListener(IEventListener<TEvent> next, IEventSubscriber subscriber, int count)
            : base( next )
        {
            _subscriber = subscriber;
            _count = count;
            _taken = 0;

            if ( _count == 0 )
                _subscriber.Dispose();
        }

        public override void React(TEvent @event)
        {
            Next.React( @event );

            if ( ++_taken == _count )
                _subscriber.Dispose();
        }
    }
}
