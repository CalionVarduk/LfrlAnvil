using System;

namespace LfrlSoft.NET.Core.Chrono.Extensions
{
    public static class IsoDayOfWeekExtensions
    {
        public static DayOfWeek ToBcl(this IsoDayOfWeek dayOfWeek)
        {
            return (DayOfWeek)((int)dayOfWeek % 7);
        }
    }
}
