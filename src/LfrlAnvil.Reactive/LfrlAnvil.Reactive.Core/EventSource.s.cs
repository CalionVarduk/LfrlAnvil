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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive;

/// <summary>
/// Creates instances of <see cref="IEventSource{TEvent}"/> type.
/// </summary>
public static class EventSource
{
    /// <summary>
    /// Creates a new <see cref="EnumerableEventSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="values">Collection of events.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="EnumerableEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EnumerableEventSource<TEvent> From<TEvent>(IEnumerable<TEvent> values)
    {
        return new EnumerableEventSource<TEvent>( values );
    }

    /// <summary>
    /// Creates a new <see cref="EnumerableEventSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="values">Collection of events.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="EnumerableEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EnumerableEventSource<TEvent> From<TEvent>(params TEvent[] values)
    {
        return From( values.AsEnumerable() );
    }

    /// <summary>
    /// Creates a new <see cref="EventHandlerSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="setup">Delegate that handles initialization of this event source.</param>
    /// <param name="teardown">Delegate that handles disposal of this event source.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="EventHandlerSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EventHandlerSource<TEvent> FromEvent<TEvent>(
        Action<EventHandler<TEvent>> setup,
        Action<EventHandler<TEvent>> teardown)
    {
        return new EventHandlerSource<TEvent>( setup, teardown );
    }

    /// <summary>
    /// Creates a new <see cref="ConcurrentEventHandlerSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="setup">Delegate that handles initialization of this event source.</param>
    /// <param name="teardown">Delegate that handles disposal of this event source.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="ConcurrentEventHandlerSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ConcurrentEventHandlerSource<TEvent> ConcurrentFromEvent<TEvent>(
        Action<EventHandler<TEvent>> setup,
        Action<EventHandler<TEvent>> teardown)
    {
        return new ConcurrentEventHandlerSource<TEvent>( setup, teardown );
    }

    /// <summary>
    /// Creates a new <see cref="WhenAllEventSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="streams">Collection of event streams.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="WhenAllEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static WhenAllEventSource<TEvent> WhenAll<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
    {
        return new WhenAllEventSource<TEvent>( streams );
    }

    /// <summary>
    /// Creates a new <see cref="WhenAllEventSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="streams">Collection of event streams.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="WhenAllEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static WhenAllEventSource<TEvent> WhenAll<TEvent>(params IEventStream<TEvent>[] streams)
    {
        return WhenAll( streams.AsEnumerable() );
    }

    /// <summary>
    /// Creates a new <see cref="WhenAnyEventSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="streams">Collection of event streams.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="WhenAnyEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static WhenAnyEventSource<TEvent> WhenAny<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
    {
        return new WhenAnyEventSource<TEvent>( streams );
    }

    /// <summary>
    /// Creates a new <see cref="WhenAnyEventSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="streams">Collection of event streams.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="WhenAnyEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static WhenAnyEventSource<TEvent> WhenAny<TEvent>(params IEventStream<TEvent>[] streams)
    {
        return WhenAny( streams.AsEnumerable() );
    }

    /// <summary>
    /// Creates a new <see cref="CombineEventSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="streams">Collection of event streams.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="CombineEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CombineEventSource<TEvent> Combine<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
    {
        return new CombineEventSource<TEvent>( streams );
    }

    /// <summary>
    /// Creates a new <see cref="CombineEventSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="streams">Collection of event streams.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="CombineEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CombineEventSource<TEvent> Combine<TEvent>(params IEventStream<TEvent>[] streams)
    {
        return Combine( streams.AsEnumerable() );
    }

    /// <summary>
    /// Creates a new <see cref="MergeEventSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="streams">Collection of event streams.</param>
    /// <param name="maxConcurrency">
    /// Maximum number of concurrent active inner event streams. Equal to <see cref="Int32.MaxValue"/> by default.
    /// </param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="MergeEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MergeEventSource<TEvent> Merge<TEvent>(IEnumerable<IEventStream<TEvent>> streams, int maxConcurrency = int.MaxValue)
    {
        return new MergeEventSource<TEvent>( streams, maxConcurrency );
    }

    /// <summary>
    /// Creates a new <see cref="MergeEventSource{TEvent}"/> instance with maximum concurrency equal to <see cref="Int32.MaxValue"/>.
    /// </summary>
    /// <param name="streams">Collection of event streams.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="MergeEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MergeEventSource<TEvent> Merge<TEvent>(params IEventStream<TEvent>[] streams)
    {
        return Merge( streams.AsEnumerable() );
    }

    /// <summary>
    /// Creates a new <see cref="MergeEventSource{TEvent}"/> instance with maximum concurrency equal to <b>1</b>.
    /// </summary>
    /// <param name="streams">Collection of event streams.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="MergeEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MergeEventSource<TEvent> Concat<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
    {
        return Merge( streams, maxConcurrency: 1 );
    }

    /// <summary>
    /// Creates a new <see cref="MergeEventSource{TEvent}"/> instance with maximum concurrency equal to <b>1</b>.
    /// </summary>
    /// <param name="streams">Collection of event streams.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="MergeEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MergeEventSource<TEvent> Concat<TEvent>(params IEventStream<TEvent>[] streams)
    {
        return Concat( streams.AsEnumerable() );
    }

    /// <summary>
    /// Creates a new <see cref="TaskEventSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="taskFactory"><see cref="Task{TResult}"/> factory.</param>
    /// <param name="schedulerCapture">Optional <see cref="TaskSchedulerCapture"/> instance.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="TaskEventSource{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TaskEventSource<TEvent> FromTask<TEvent>(
        Func<CancellationToken, Task<TEvent>> taskFactory,
        TaskSchedulerCapture schedulerCapture = default)
    {
        return new TaskEventSource<TEvent>( taskFactory, schedulerCapture );
    }

    /// <summary>
    /// Returns an <see cref="IEventSource{TEvent}"/> instance that is disposed.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Disposed <see cref="IEventSource{TEvent}"/> instance.</returns>
    [Pure]
    public static IEventSource<TEvent> Disposed<TEvent>()
    {
        return DisposedStore<TEvent>.Instance;
    }

    private static class DisposedStore<TEvent>
    {
        internal static readonly IEventSource<TEvent> Instance = CreateDisposed();

        [Pure]
        private static IEventSource<TEvent> CreateDisposed()
        {
            var result = new EventPublisher<TEvent>();
            result.Dispose();
            return result;
        }
    }
}
