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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Decorators;

namespace LfrlAnvil.Reactive.Chrono.Extensions;

/// <summary>
/// Contains <see cref="IEventStream{TEvent}"/> extension methods.
/// </summary>
public static class EventStreamExtensions
{
    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerWithTimestampDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="timestampProvider">Timestamp provider to use for time tracking.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<WithTimestamp<TEvent>> WithTimestamp<TEvent>(
        this IEventStream<TEvent> source,
        ITimestampProvider timestampProvider)
    {
        var decorator = new EventListenerWithTimestampDecorator<TEvent>( timestampProvider );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerWithIntervalDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="timestampProvider">Timestamp provider to use for time tracking.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<WithInterval<TEvent>> WithInterval<TEvent>(
        this IEventStream<TEvent> source,
        ITimestampProvider timestampProvider)
    {
        var decorator = new EventListenerWithIntervalDecorator<TEvent>( timestampProvider );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerWithZonedDateTimeDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="clock">Clock to use for time tracking.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<WithZonedDateTime<TEvent>> WithZonedDateTime<TEvent>(this IEventStream<TEvent> source, IZonedClock clock)
    {
        var decorator = new EventListenerWithZonedDateTimeDecorator<TEvent>( clock );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDelayDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="timestampProvider">Timestamp provider to use for time tracking.</param>
    /// <param name="delay">Event delay.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<WithInterval<TEvent>> Delay<TEvent>(
        this IEventStream<TEvent> source,
        ITimestampProvider timestampProvider,
        Duration delay)
    {
        return source.Delay( timestampProvider, delay, ReactiveTimer.DefaultSpinWaitDurationHint );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDelayDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="timestampProvider">Timestamp provider to use for time tracking.</param>
    /// <param name="delay">Event delay.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for the underlying timer.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<WithInterval<TEvent>> Delay<TEvent>(
        this IEventStream<TEvent> source,
        ITimestampProvider timestampProvider,
        Duration delay,
        Duration spinWaitDurationHint)
    {
        var decorator = new EventListenerDelayDecorator<TEvent>( timestampProvider, delay, scheduler: null, spinWaitDurationHint );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDelayDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="timestampProvider">Timestamp provider to use for time tracking.</param>
    /// <param name="delay">Event delay.</param>
    /// <param name="scheduler">Task scheduler.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<WithInterval<TEvent>> Delay<TEvent>(
        this IEventStream<TEvent> source,
        ITimestampProvider timestampProvider,
        Duration delay,
        TaskScheduler scheduler)
    {
        return source.Delay( timestampProvider, delay, scheduler, ReactiveTimer.DefaultSpinWaitDurationHint );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDelayDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="timestampProvider">Timestamp provider to use for time tracking.</param>
    /// <param name="delay">Event delay.</param>
    /// <param name="scheduler">Task scheduler.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for the underlying timer.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<WithInterval<TEvent>> Delay<TEvent>(
        this IEventStream<TEvent> source,
        ITimestampProvider timestampProvider,
        Duration delay,
        TaskScheduler scheduler,
        Duration spinWaitDurationHint)
    {
        var decorator = new EventListenerDelayDecorator<TEvent>( timestampProvider, delay, scheduler, spinWaitDurationHint );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Creates a new <see cref="TimerTaskCollection{TKey}"/> instance by registering a collection
    /// of <see cref="ITimerTask{TKey}"/> instances in the provided <see cref="IEventStream"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="tasks">Collection of <see cref="ITimerTask{TKey}"/> instances to register.</param>
    /// <typeparam name="TKey">Task key type.</typeparam>
    /// <returns>New <see cref="TimerTaskCollection{TKey}"/> instance.</returns>
    /// <exception cref="ArgumentException">When task keys are not unique.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimerTaskCollection<TKey> RegisterTasks<TKey>(
        this IEventStream<WithInterval<long>> source,
        IEnumerable<ITimerTask<TKey>> tasks)
        where TKey : notnull
    {
        return new TimerTaskCollection<TKey>( source, tasks );
    }
}
