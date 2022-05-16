using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Events.Decorators;

namespace LfrlAnvil.Reactive.Events.Extensions
{
    public static class EventStreamExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventStream<TEvent> Where<TEvent>(this IEventStream<TEvent> source, Func<TEvent, bool> predicate)
        {
            var decorator = new EventListenerWhereDecorator<TEvent>( predicate );
            return source.Decorate( decorator );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventStream<TNextEvent> Select<TSourceEvent, TNextEvent>(
            this IEventStream<TSourceEvent> source,
            Func<TSourceEvent, TNextEvent> selector)
        {
            var decorator = new EventListenerSelectDecorator<TSourceEvent, TNextEvent>( selector );
            return source.Decorate( decorator );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventStream<WithIndex<TEvent>> WithIndex<TEvent>(this IEventStream<TEvent> source)
        {
            var decorator = new EventListenerWithIndexDecorator<TEvent>();
            return source.Decorate( decorator );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventStream<TEvent> ForEach<TEvent>(this IEventStream<TEvent> source, Action<TEvent> action)
        {
            var decorator = new EventListenerForEachDecorator<TEvent>( action );
            return source.Decorate( decorator );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventStream<TEvent> TakeUntil<TEvent, TTargetEvent>(
            this IEventStream<TEvent> source,
            IEventStream<TTargetEvent> target)
        {
            var decorator = new EventListenerTakeUntilDecorator<TEvent, TTargetEvent>( target );
            return source.Decorate( decorator );
        }
    }
}
