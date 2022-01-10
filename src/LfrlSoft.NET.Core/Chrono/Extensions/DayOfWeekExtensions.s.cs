using System;

namespace LfrlSoft.NET.Core.Chrono.Extensions
{
    public static class DayOfWeekExtensions
    {
        public static IsoDayOfWeek ToIso(this DayOfWeek dayOfWeek)
        {
            return dayOfWeek == DayOfWeek.Sunday
                ? IsoDayOfWeek.Sunday
                : (IsoDayOfWeek)dayOfWeek;
        }
    }
}
