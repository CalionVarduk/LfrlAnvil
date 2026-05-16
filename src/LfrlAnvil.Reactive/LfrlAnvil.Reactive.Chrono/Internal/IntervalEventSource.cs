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
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Extensions;
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
    private readonly Duration _spinWaitDurationHint;
    private readonly long _count;
    private DelaySource _delaySource;

    internal IntervalEventSource(
        ITimestampProvider? timestampProvider,
        Duration interval,
        Duration spinWaitDurationHint,
        ValueTaskDelaySource? delaySource,
        long count)
    {
        Ensure.IsGreaterThan( count, 0 );
        Ensure.IsInRange( interval, Duration.FromTicks( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        Ensure.IsGreaterThanOrEqualTo( spinWaitDurationHint, Duration.Zero );

        _timestampProvider = timestampProvider ?? TimestampProvider.Shared;
        _interval = interval;
        _spinWaitDurationHint = spinWaitDurationHint;
        _delaySource = delaySource is null ? DelaySource.Owned() : DelaySource.External( delaySource );
        _count = count;
    }

    /// <inheritdoc />
    protected override IEventListener<WithInterval<long>> OverrideListenerUnsafe(
        IEventSubscriber subscriber,
        IEventListener<WithInterval<long>> listener)
    {
        ValueTaskDelaySource delaySource;
        using ( AcquireLock() )
        {
            if ( IsDisposedUnsafe() )
                return listener;

            delaySource = _delaySource.GetSource();
        }

        var timer = new ReactiveTimer( _interval, _spinWaitDurationHint, _timestampProvider, delaySource, _count );
        return new EventListener( listener, subscriber, timer );
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        if ( DisposeCore( out var exceptions ) )
        {
            ValueTaskDelaySource? delaySource;
            using ( AcquireLock() )
                delaySource = _delaySource.DiscardOwnedSource();

            try
            {
                delaySource?.Dispose();
            }
            catch ( Exception exc )
            {
                exceptions = exceptions.Extend( exc );
            }
        }

        if ( exceptions.Count > 0 )
            exceptions.Consolidate()?.Rethrow();
    }

    private sealed class EventListener : DecoratedEventListener<WithInterval<long>, WithInterval<long>>
    {
        private readonly ReactiveTimer _timer;

        internal EventListener(IEventListener<WithInterval<long>> next, IEventSubscriber subscriber, ReactiveTimer timer)
            : base( next )
        {
            _timer = timer;
            var timerListener = new TimerListener( this, subscriber );
            _timer.Listen( timerListener );
            timer.Start();
        }

        public override void React(WithInterval<long> @event)
        {
            Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _timer.Dispose();
            base.OnDispose( source );
        }
    }

    private sealed class TimerListener : EventListener<WithInterval<long>>
    {
        private readonly EventListener _mainListener;
        private readonly IEventSubscriber _mainSubscriber;

        internal TimerListener(EventListener mainListener, IEventSubscriber mainSubscriber)
        {
            _mainListener = mainListener;
            _mainSubscriber = mainSubscriber;
        }

        public override void React(WithInterval<long> @event)
        {
            _mainListener.React( @event );
        }

        public override void OnDispose(DisposalSource _)
        {
            _mainSubscriber.Dispose();
        }
    }
}
