using System;
using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Chrono.Extensions
{
    public static class TimeZoneInfoExtensions
    {
        [Pure]
        public static DateTimeKind GetDateTimeKind(this TimeZoneInfo timeZone)
        {
            if ( ReferenceEquals( timeZone, TimeZoneInfo.Utc ) )
                return DateTimeKind.Utc;

            if ( ReferenceEquals( timeZone, TimeZoneInfo.Local ) )
                return DateTimeKind.Local;

            return DateTimeKind.Unspecified;
        }

        [Pure]
        public static TimeZoneInfo.AdjustmentRule? GetActiveAdjustmentRule(this TimeZoneInfo timeZone, DateTime dateTime)
        {
            var date = dateTime.Date;
            var rules = timeZone.GetAdjustmentRules();

            for ( var i = 0; i < rules.Length; ++i )
            {
                var rule = rules[i];
                if ( rule.DateStart <= date && rule.DateEnd >= date )
                    return rule;
            }

            return null;
        }

        [Pure]
        public static int GetActiveAdjustmentRuleIndex(this TimeZoneInfo timeZone, DateTime dateTime)
        {
            var date = dateTime.Date;
            var rules = timeZone.GetAdjustmentRules();

            for ( var i = 0; i < rules.Length; ++i )
            {
                var rule = rules[i];
                if ( rule.DateStart <= date && rule.DateEnd >= date )
                    return i;
            }

            return -1;
        }
    }
}
