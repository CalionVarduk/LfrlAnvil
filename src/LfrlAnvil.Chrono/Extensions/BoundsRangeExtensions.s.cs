using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions
{
    public static class BoundsRangeExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static BoundsRange<DateTime> Normalize(this BoundsRange<DateTime> source)
        {
            return source.Normalize( (a, b) => a + TimeSpan.FromTicks( 1 ) == b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static TimeSpan GetTimeSpan(this BoundsRange<DateTime> source)
        {
            return source.Aggregate( TimeSpan.Zero, (a, b) => a + b.GetTimeSpan() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static BoundsRange<ZonedDateTime> Normalize(this BoundsRange<ZonedDateTime> source)
        {
            return source.Normalize( (a, b) => a.Timestamp.Add( Duration.FromTicks( 1 ) ) == b.Timestamp );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration GetDuration(this BoundsRange<ZonedDateTime> source)
        {
            return source.Aggregate( Duration.Zero, (a, b) => a + b.GetDuration() );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static BoundsRange<ZonedDay> Normalize(this BoundsRange<ZonedDay> source)
        {
            return source.Normalize( (a, b) => a.End.Timestamp.Add( Duration.FromTicks( 1 ) ) == b.Start.Timestamp );
        }
    }
}
