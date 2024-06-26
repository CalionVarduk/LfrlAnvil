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

using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;

namespace LfrlAnvil.Reactive.Chrono.Internal;

/// <summary>
/// Represents a disposable event source that can be listened to,
/// that notifies its listeners with events published by an underlying <see cref="ReactiveTimer"/> created per listener.
/// </summary>
public sealed class IntervalEventSource : EventSource<WithInterval<long>>
{
    private readonly ITimestampProvider _timestampProvider;
    private readonly Duration _interval;
    private readonly TaskScheduler? _scheduler;
    private readonly Duration _spinWaitDurationHint;
    private readonly long _count;

    internal IntervalEventSource(
        ITimestampProvider timestampProvider,
        Duration interval,
        TaskScheduler? scheduler,
        Duration spinWaitDurationHint,
        long count)
    {
        Ensure.IsGreaterThan( count, 0 );
        Ensure.IsInRange( interval, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        Ensure.IsGreaterThanOrEqualTo( spinWaitDurationHint, Duration.Zero );

        _timestampProvider = timestampProvider;
        _interval = interval;
        _scheduler = scheduler;
        _spinWaitDurationHint = spinWaitDurationHint;
        _count = count;
    }

    /// <inheritdoc />
    protected override IEventListener<WithInterval<long>> OverrideListener(
        IEventSubscriber subscriber,
        IEventListener<WithInterval<long>> listener)
    {
        if ( IsDisposed )
            return listener;

        var timer = new ReactiveTimer( _timestampProvider, _interval, _spinWaitDurationHint, _count );
        return new EventListener( listener, subscriber, timer, _scheduler );
    }

    private sealed class EventListener : DecoratedEventListener<WithInterval<long>, WithInterval<long>>
    {
        private ReactiveTimer? _timer;

        internal EventListener(
            IEventListener<WithInterval<long>> next,
            IEventSubscriber subscriber,
            ReactiveTimer timer,
            TaskScheduler? scheduler)
            : base( next )
        {
            _timer = timer;
            var timerListener = new TimerListener( this, subscriber );
            _timer.Listen( timerListener );

            if ( scheduler is null )
            {
                _timer.StartAsync();
                return;
            }

            _timer.StartAsync( scheduler );
        }

        public override void React(WithInterval<long> @event)
        {
            Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            Assume.IsNotNull( _timer );
            _timer.Dispose();
            _timer = null;
            base.OnDispose( source );
        }
    }

    private sealed class TimerListener : EventListener<WithInterval<long>>
    {
        private EventListener? _mainListener;
        private IEventSubscriber? _mainSubscriber;

        internal TimerListener(EventListener mainListener, IEventSubscriber mainSubscriber)
        {
            _mainListener = mainListener;
            _mainSubscriber = mainSubscriber;
        }

        public override void React(WithInterval<long> @event)
        {
            Assume.IsNotNull( _mainListener );
            _mainListener.React( @event );
        }

        public override void OnDispose(DisposalSource _)
        {
            Assume.IsNotNull( _mainSubscriber );
            _mainSubscriber.Dispose();
            _mainSubscriber = null;
            _mainListener = null;
        }
    }
}
