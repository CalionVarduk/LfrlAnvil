using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono.Extensions
{
    public static class IsoDayOfWeekExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static DayOfWeek ToBcl(this IsoDayOfWeek dayOfWeek)
        {
            return (DayOfWeek)((int)dayOfWeek % 7);
        }
    }
}
