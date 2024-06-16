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
/// Notifies the decorated event listener with <see cref="WithInterval{TEvent}"/> whose intervals are calculated from the current
/// and last emitted event's timestamps. First event's interval is equal to <b>-1 tick</b>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerWithIntervalDecorator<TEvent> : IEventListenerDecorator<TEvent, WithInterval<TEvent>>
{
    private readonly ITimestampProvider _timestampProvider;

    /// <summary>
    /// Creates a new <see cref="EventListenerWithIntervalDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider to use for time tracking.</param>
    public EventListenerWithIntervalDecorator(ITimestampProvider timestampProvider)
    {
        _timestampProvider = timestampProvider;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<WithInterval<TEvent>> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _timestampProvider );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, WithInterval<TEvent>>
    {
        private readonly ITimestampProvider _timestampProvider;
        private Timestamp? _lastTimestamp;

        internal EventListener(IEventListener<WithInterval<TEvent>> next, ITimestampProvider timestampProvider)
            : base( next )
        {
            _timestampProvider = timestampProvider;
            _lastTimestamp = null;
        }

        public override void React(TEvent @event)
        {
            var timestamp = _timestampProvider.GetNow();
            var interval = _lastTimestamp is null ? Duration.FromTicks( -1 ) : timestamp - _lastTimestamp.Value;
            _lastTimestamp = timestamp;

            var nextEvent = new WithInterval<TEvent>( @event, timestamp, interval );
            Next.React( nextEvent );
        }
    }
}
