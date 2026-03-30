// Copyright 2024-2026 Łukasz Furlepa
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
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Internal;

namespace LfrlAnvil.Reactive.Chrono.Decorators;

/// <summary>
/// Notifies decorated event listener with delayed emitted events.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventListenerDelayDecorator<TEvent> : IEventListenerDecorator<TEvent, WithInterval<TEvent>>
{
    private readonly ValueTaskDelaySource _delaySource;
    private readonly ITimestampProvider? _timestampProvider;
    private readonly Duration _delay;
    private readonly Duration _spinWaitDurationHint;

    /// <summary>
    /// Creates a new <see cref="EventListenerDelayDecorator{TEvent}"/> instance,
    /// </summary>
    /// <param name="delaySource">Value task delay source to use for scheduling delays.</param>
    /// <param name="delay">Event delay.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for the underlying timer.</param>
    /// <param name="timestampProvider">Optional timestamp provider to use for time tracking.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    public EventListenerDelayDecorator(
        ValueTaskDelaySource delaySource,
        Duration delay,
        Duration spinWaitDurationHint,
        ITimestampProvider? timestampProvider)
    {
        Ensure.IsInRange( delay, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        Ensure.IsGreaterThanOrEqualTo( spinWaitDurationHint, Duration.Zero );

        _delaySource = delaySource;
        _timestampProvider = timestampProvider;
        _delay = delay;
        _spinWaitDurationHint = spinWaitDurationHint;
    }

    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<WithInterval<TEvent>> listener, IEventSubscriber subscriber)
    {
        var timeout = new IntervalEventSource( _timestampProvider, _delay, _spinWaitDurationHint, _delaySource, count: 1 );
        return new EventListener( listener, timeout );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, WithInterval<TEvent>>
    {
        private IntervalEventSource? _timeout;

        internal EventListener(IEventListener<WithInterval<TEvent>> next, IntervalEventSource timeout)
            : base( next )
        {
            _timeout = timeout;
        }

        public override void React(TEvent @event)
        {
            Assume.IsNotNull( _timeout );
            var timerListener = new TimerListener( this, @event );
            _timeout.Listen( timerListener );
        }

        public override void OnDispose(DisposalSource source)
        {
            Assume.IsNotNull( _timeout );
            _timeout.Dispose();
            _timeout = null;
            base.OnDispose( source );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void OnTimerReact(TEvent @event, WithInterval<long> timerEvent)
        {
            var nextEvent = new WithInterval<TEvent>( @event, timerEvent.Timestamp, timerEvent.Interval );
            Next.React( nextEvent );
        }
    }

    private sealed class TimerListener : EventListener<WithInterval<long>>
    {
        private EventListener? _mainListener;
        private TEvent? _event;

        internal TimerListener(EventListener mainListener, TEvent @event)
        {
            _mainListener = mainListener;
            _event = @event;
        }

        public override void React(WithInterval<long> @event)
        {
            Assume.IsNotNull( _mainListener );
            _mainListener.OnTimerReact( _event!, @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _mainListener = null;
            _event = default;
        }
    }
}
