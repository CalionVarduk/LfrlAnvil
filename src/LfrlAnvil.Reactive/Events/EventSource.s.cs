﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Events.Composites;
using LfrlAnvil.Reactive.Events.Internal;

namespace LfrlAnvil.Reactive.Events
{
    public static class EventSource
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventSource<TEvent> From<TEvent>(IEnumerable<TEvent> values)
        {
            return new EnumerableEventSource<TEvent>( values );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventSource<TEvent> From<TEvent>(params TEvent[] values)
        {
            return From( values.AsEnumerable() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventSource<WithSender<TEvent>> FromEvent<TEvent>(
            Action<EventHandler<TEvent>> setup,
            Action<EventHandler<TEvent>> teardown)
        {
            return new EventHandlerSource<TEvent>( setup, teardown );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventSource<ReadOnlyMemory<TEvent?>> WhenAll<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
        {
            return new WhenAllEventSource<TEvent>( streams );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventSource<ReadOnlyMemory<TEvent?>> WhenAll<TEvent>(params IEventStream<TEvent>[] streams)
        {
            return WhenAll( streams.AsEnumerable() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventSource<WithIndex<TEvent>> WhenAny<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
        {
            return new WhenAnyEventSource<TEvent>( streams );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventSource<WithIndex<TEvent>> WhenAny<TEvent>(params IEventStream<TEvent>[] streams)
        {
            return WhenAny( streams.AsEnumerable() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventSource<ReadOnlyMemory<TEvent>> Combine<TEvent>(IEnumerable<IEventStream<TEvent>> streams)
        {
            return new CombineEventSource<TEvent>( streams );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventSource<ReadOnlyMemory<TEvent>> Combine<TEvent>(params IEventStream<TEvent>[] streams)
        {
            return Combine( streams.AsEnumerable() );
        }
    }
}
