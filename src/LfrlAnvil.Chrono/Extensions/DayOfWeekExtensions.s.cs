using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions
{
    public static class DayOfWeekExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IsoDayOfWeek ToIso(this DayOfWeek dayOfWeek)
        {
            return dayOfWeek == DayOfWeek.Sunday
                ? IsoDayOfWeek.Sunday
                : (IsoDayOfWeek)dayOfWeek;
        }
    }
}
