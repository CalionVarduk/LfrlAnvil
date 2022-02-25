using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions
{
    public static class BoundsExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static TimeSpan GetTimeSpan(this Bounds<DateTime> source)
        {
            return source.Max - source.Min + TimeSpan.FromTicks( 1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period GetPeriod(this Bounds<DateTime> source, PeriodUnits units)
        {
            return (source.Max + TimeSpan.FromTicks( 1 )).GetPeriodOffset( source.Min, units );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period GetGreedyPeriod(this Bounds<DateTime> source, PeriodUnits units)
        {
            return (source.Max + TimeSpan.FromTicks( 1 )).GetGreedyPeriodOffset( source.Min, units );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration GetDuration(this Bounds<ZonedDateTime> source)
        {
            return source.Max.GetDurationOffset( source.Min ).AddTicks( 1 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period GetPeriod(this Bounds<ZonedDateTime> source, PeriodUnits units)
        {
            return source.Max.Add( Duration.FromTicks( 1 ) ).GetPeriodOffset( source.Min, units );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period GetGreedyPeriod(this Bounds<ZonedDateTime> source, PeriodUnits units)
        {
            return source.Max.Add( Duration.FromTicks( 1 ) ).GetGreedyPeriodOffset( source.Min, units );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period GetPeriod(this Bounds<ZonedDay> source, PeriodUnits units)
        {
            return source.Max.End.Add( Duration.FromTicks( 1 ) ).GetPeriodOffset( source.Min.Start, units );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Period GetGreedyPeriod(this Bounds<ZonedDay> source, PeriodUnits units)
        {
            return source.Max.End.Add( Duration.FromTicks( 1 ) ).GetGreedyPeriodOffset( source.Min.Start, units );
        }
    }
}
