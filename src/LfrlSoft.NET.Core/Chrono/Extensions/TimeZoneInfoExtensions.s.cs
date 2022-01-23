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

        [Pure]
        public static Bounds<DateTime>? GetContainingInvalidityRange(this TimeZoneInfo timeZone, DateTime dateTime)
        {
            // NOTE: this can't really be safely optimized away, since .NET doesn't provide any public properties to handle DST disabling changes
            // https://techcommunity.microsoft.com/t5/daylight-saving-time-time-zone/how-does-microsoft-handle-dst-servicing-when-a-country-decides/ba-p/1175108
            if ( ! timeZone.IsInvalidTime( dateTime ) )
                return null;

            var activeRule = timeZone.GetActiveAdjustmentRule( dateTime )!;
            var transitionDateTime = GetInvalidityDateTimeRange( activeRule, dateTime.Year );

            if ( dateTime >= transitionDateTime.Start && dateTime < transitionDateTime.End )
                return Bounds.Create( transitionDateTime.Start, transitionDateTime.End.AddTicks( -1 ) );

            // NOTE: this doesn't seem to  be correct, since activeRule may actually no longer be active in the previous year (or the rule may be floating)
            // however, this implementation corresponds to .NET's private GetIsInvalidTime method's body:
            // https://github.com/microsoft/referencesource/blob/5697c29004a34d80acdaf5742d7e699022c64ecd/mscorlib/system/timezoneinfo.cs#L1745
            // the main goal of GetContainingInvalidityRange extension is to correspond 1-to-1 to TimeZoneInfo.IsInvalidTime method's result
            // so for now, it must stay like this
            transitionDateTime = (transitionDateTime.Start.AddYears( -1 ), transitionDateTime.End.AddYears( -1 ));
            return Bounds.Create( transitionDateTime.Start, transitionDateTime.End.AddTicks( -1 ) );
        }

        [Pure]
        public static Bounds<DateTime>? GetContainingAmbiguityRange(this TimeZoneInfo timeZone, DateTime dateTime)
        {
            // NOTE: this can't really be safely optimized away, since .NET doesn't provide any public properties to handle DST disabling changes
            // https://techcommunity.microsoft.com/t5/daylight-saving-time-time-zone/how-does-microsoft-handle-dst-servicing-when-a-country-decides/ba-p/1175108
            if ( ! timeZone.IsAmbiguousTime( dateTime ) )
                return null;

            var activeRule = timeZone.GetActiveAdjustmentRule( dateTime )!;
            var transitionDateTime = GetAmbiguityDateTimeRange( activeRule, dateTime.Year );

            if ( dateTime >= transitionDateTime.Start && dateTime < transitionDateTime.End )
                return Bounds.Create( transitionDateTime.Start, transitionDateTime.End.AddTicks( -1 ) );

            // NOTE: this doesn't seem to  be correct, since activeRule may actually no longer be active in the next year (or the rule may be floating)
            // however, this implementation corresponds to .NET's private GetIsAmbiguousTime method's body:
            // https://github.com/microsoft/referencesource/blob/5697c29004a34d80acdaf5742d7e699022c64ecd/mscorlib/system/timezoneinfo.cs#L1681
            // the main goal of GetContainingAmbiguityRange extension is to correspond 1-to-1 to TimeZoneInfo.IsAmbiguousTime method's result
            // so for now, it must stay like this
            transitionDateTime = (transitionDateTime.Start.AddYears( 1 ), transitionDateTime.End.AddYears( 1 ));
            return Bounds.Create( transitionDateTime.Start, transitionDateTime.End.AddTicks( -1 ) );
        }

        private static (DateTime Start, DateTime End) GetInvalidityDateTimeRange(TimeZoneInfo.AdjustmentRule rule, int year)
        {
            if ( rule.DaylightDelta > TimeSpan.Zero )
            {
                var start = rule.DaylightTransitionStart.ToDateTime( year );
                return (start, start + rule.DaylightDelta);
            }
            else
            {
                var start = rule.DaylightTransitionEnd.ToDateTime( year );
                return (start, start - rule.DaylightDelta);
            }
        }

        private static (DateTime Start, DateTime End) GetAmbiguityDateTimeRange(TimeZoneInfo.AdjustmentRule rule, int year)
        {
            if ( rule.DaylightDelta > TimeSpan.Zero )
            {
                var end = rule.DaylightTransitionEnd.ToDateTime( year );
                return (end - rule.DaylightDelta, end);
            }
            else
            {
                var end = rule.DaylightTransitionStart.ToDateTime( year );
                return (end + rule.DaylightDelta, end);
            }
        }
    }
}
