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

using System.Diagnostics.Contracts;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;

namespace LfrlAnvil.Reactive.Chrono.Decorators;

/// <summary>
/// Notifies the decorated event listener with <see cref="WithZonedDateTime{TEvent}"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerWithZonedDateTimeDecorator<TEvent> : IEventListenerDecorator<TEvent, WithZonedDateTime<TEvent>>
{
    private readonly IZonedClock _clock;

    /// <summary>
    /// Creates a new <see cref="EventListenerWithTimestampDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="clock">Clock to use for time tracking.</param>
    public EventListenerWithZonedDateTimeDecorator(IZonedClock clock)
    {
        _clock = clock;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<WithZonedDateTime<TEvent>> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _clock );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, WithZonedDateTime<TEvent>>
    {
        private readonly IZonedClock _clock;

        internal EventListener(IEventListener<WithZonedDateTime<TEvent>> next, IZonedClock clock)
            : base( next )
        {
            _clock = clock;
        }

        public override void React(TEvent @event)
        {
            var dateTime = _clock.GetNow();
            var nextEvent = new WithZonedDateTime<TEvent>( @event, dateTime );
            Next.React( nextEvent );
        }
    }
}
