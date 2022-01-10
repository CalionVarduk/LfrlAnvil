using System;

namespace LfrlSoft.NET.Core.Chrono.Extensions
{
    public static class DateTimeExtensions
    {
        public static IsoMonthOfYear GetMonthOfYear(this DateTime dt)
        {
            return (IsoMonthOfYear)dt.Month;
        }

        public static IsoDayOfWeek GetDayOfWeek(this DateTime dt)
        {
            return dt.DayOfWeek.ToIso();
        }
    }
}
