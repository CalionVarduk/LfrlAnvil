using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono.Extensions
{
    public static class ZonedClockExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static ZonedDateTime Create(this IZonedClock clock, DateTime dateTime)
        {
            return ZonedDateTime.Create( dateTime, clock.TimeZone );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsInPast(this IZonedClock clock, ZonedDateTime dateTime)
        {
            return dateTime.Timestamp < clock.GetNow().Timestamp;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNow(this IZonedClock clock, ZonedDateTime dateTime)
        {
            return dateTime.Timestamp == clock.GetNow().Timestamp;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsInFuture(this IZonedClock clock, ZonedDateTime dateTime)
        {
            return dateTime.Timestamp > clock.GetNow().Timestamp;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration GetDurationOffset(this IZonedClock clock, ZonedDateTime dateTime)
        {
            return dateTime.GetDurationOffset( clock.GetNow() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration GetDurationOffset(this IZonedClock clock, IZonedClock other)
        {
            return clock.GetDurationOffset( other.GetNow() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IZonedClock Freeze(this IZonedClock clock)
        {
            return new FrozenZonedClock( clock.GetNow() );
        }
    }
}
