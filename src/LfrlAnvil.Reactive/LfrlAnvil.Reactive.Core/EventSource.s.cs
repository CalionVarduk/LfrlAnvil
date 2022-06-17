using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive
{
    public static class EventSource
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static EnumerableEventSource<TEvent> From<TEvent>(IEnumerable<TEvent> values)
        {
            return new EnumerableEventSource<TEvent>( values );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static EnumerableEventSource<TEvent> From<TEvent>(params TEvent[] values)
        {
            return From( values.AsEnumerable() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static EventHandlerSource<TEvent> FromEvent<TEvent>(
            Action<EventHandler<TEvent>> setup,
            Action<EventHandler<TEvent>> teardown)
        {
            return new EventHandlerSource<TEvent>( setup, teardown );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ConcurrentEventHandlerSource<TEvent> ConcurrentFromEvent<TEvent>(
            Action<EventHandler<TEvent>> setup,
            Action<EventHandler<TEvent>> teardown)
        {
            return new ConcurrentEventHandlerSource<TEvent>( setup, teardown );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static WhenAllEventSource<TEvent> WhenAll<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
        {
            return new WhenAllEventSource<TEvent>( streams );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static WhenAllEventSource<TEvent> WhenAll<TEvent>(params IEventStream<TEvent>[] streams)
        {
            return WhenAll( streams.AsEnumerable() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static WhenAnyEventSource<TEvent> WhenAny<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
        {
            return new WhenAnyEventSource<TEvent>( streams );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static WhenAnyEventSource<TEvent> WhenAny<TEvent>(params IEventStream<TEvent>[] streams)
        {
            return WhenAny( streams.AsEnumerable() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static CombineEventSource<TEvent> Combine<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
        {
            return new CombineEventSource<TEvent>( streams );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static CombineEventSource<TEvent> Combine<TEvent>(params IEventStream<TEvent>[] streams)
        {
            return Combine( streams.AsEnumerable() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static MergeEventSource<TEvent> Merge<TEvent>(IEnumerable<IEventStream<TEvent>> streams, int maxConcurrency = int.MaxValue)
        {
            return new MergeEventSource<TEvent>( streams, maxConcurrency );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static MergeEventSource<TEvent> Merge<TEvent>(params IEventStream<TEvent>[] streams)
        {
            return Merge( streams.AsEnumerable() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static MergeEventSource<TEvent> Concat<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
        {
            return Merge( streams, maxConcurrency: 1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static MergeEventSource<TEvent> Concat<TEvent>(params IEventStream<TEvent>[] streams)
        {
            return Concat( streams.AsEnumerable() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static TaskEventSource<TEvent> FromTask<TEvent>(
            Func<CancellationToken, Task<TEvent>> taskFactory,
            TaskSchedulerCapture schedulerCapture = default)
        {
            return new TaskEventSource<TEvent>( taskFactory, schedulerCapture );
        }
    }
}
