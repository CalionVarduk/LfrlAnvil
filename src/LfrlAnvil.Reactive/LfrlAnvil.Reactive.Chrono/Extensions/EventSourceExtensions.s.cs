using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Composites;
using LfrlAnvil.Reactive.Chrono.Decorators;

namespace LfrlAnvil.Reactive.Chrono.Extensions
{
    public static class EventSourceExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventStream<WithTimestamp<TEvent>> WithTimestamp<TEvent>(
            this IEventStream<TEvent> source,
            ITimestampProvider timestampProvider)
        {
            var decorator = new EventListenerWithTimestampDecorator<TEvent>( timestampProvider );
            return source.Decorate( decorator );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventStream<WithInterval<TEvent>> WithInterval<TEvent>(
            this IEventStream<TEvent> source,
            ITimestampProvider timestampProvider)
        {
            var decorator = new EventListenerWithIntervalDecorator<TEvent>( timestampProvider );
            return source.Decorate( decorator );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventStream<WithZonedDateTime<TEvent>> WithZonedDateTime<TEvent>(this IEventStream<TEvent> source, IZonedClock clock)
        {
            var decorator = new EventListenerWithZonedDateTimeDecorator<TEvent>( clock );
            return source.Decorate( decorator );
        }
    }
}
