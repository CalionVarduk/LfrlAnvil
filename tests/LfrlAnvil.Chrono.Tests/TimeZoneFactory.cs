using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlAnvil.Chrono.Extensions;

namespace LfrlAnvil.Chrono.Tests
{
    internal static class TimeZoneFactory
    {
        internal static TimeZoneInfo Create(double utcOffsetInHours, string? idSuffix = null)
        {
            var utcOffset = CreateOffsetTimeSpan( utcOffsetInHours );
            var id = StringifyTimeZone( utcOffset, Array.Empty<TimeZoneInfo.AdjustmentRule>(), idSuffix );

            return TimeZoneInfo.CreateCustomTimeZone(
                id: id,
                baseUtcOffset: utcOffset,
                displayName: id,
                standardDisplayName: id );
        }

        internal static TimeZoneInfo CreateRandom(IFixture fixture, string? idSuffix = null)
        {
            return Create( CreateRandomOffset( fixture ), idSuffix );
        }

        internal static TimeZoneInfo Create(double utcOffsetInHours, params TimeZoneInfo.AdjustmentRule[] rules)
        {
            var utcOffset = CreateOffsetTimeSpan( utcOffsetInHours );
            var id = StringifyTimeZone( utcOffset, rules, idSuffix: null );

            return TimeZoneInfo.CreateCustomTimeZone(
                id: id,
                baseUtcOffset: utcOffset,
                displayName: id,
                standardDisplayName: $"S {id}",
                daylightDisplayName: $"D {id}",
                adjustmentRules: rules );
        }

        internal static TimeZoneInfo Create(double utcOffsetInHours, string idSuffix, params TimeZoneInfo.AdjustmentRule[] rules)
        {
            var utcOffset = CreateOffsetTimeSpan( utcOffsetInHours );
            var id = StringifyTimeZone( utcOffset, rules, idSuffix );

            return TimeZoneInfo.CreateCustomTimeZone(
                id: id,
                baseUtcOffset: utcOffset,
                displayName: id,
                standardDisplayName: $"S {id}",
                daylightDisplayName: $"D {id}",
                adjustmentRules: rules );
        }

        internal static TimeZoneInfo.AdjustmentRule CreateRule(
            DateTime start,
            DateTime end,
            TimeZoneInfo.TransitionTime transitionStart,
            TimeZoneInfo.TransitionTime transitionEnd,
            double daylightDeltaInHours = 1.0)
        {
            return TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: start,
                dateEnd: end,
                daylightDelta: CreateOffsetTimeSpan( daylightDeltaInHours ),
                daylightTransitionStart: transitionStart,
                daylightTransitionEnd: transitionEnd );
        }

        internal static TimeZoneInfo.AdjustmentRule CreateRule(
            DateTime start,
            DateTime end,
            DateTime transitionStart,
            DateTime transitionEnd,
            double daylightDeltaInHours = 1.0)
        {
            return TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: start,
                dateEnd: end,
                daylightDelta: CreateOffsetTimeSpan( daylightDeltaInHours ),
                daylightTransitionStart: CreateFixedTime( transitionStart ),
                daylightTransitionEnd: CreateFixedTime( transitionEnd ) );
        }

        internal static TimeZoneInfo.AdjustmentRule CreateInfiniteRule(
            TimeZoneInfo.TransitionTime transitionStart,
            TimeZoneInfo.TransitionTime transitionEnd,
            double daylightDeltaInHours = 1.0)
        {
            return CreateRule( DateTime.MinValue, DateTime.MaxValue, transitionStart, transitionEnd, daylightDeltaInHours );
        }

        internal static TimeZoneInfo.AdjustmentRule CreateInfiniteRule(
            DateTime transitionStart,
            DateTime transitionEnd,
            double daylightDeltaInHours = 1.0)
        {
            return CreateRule( DateTime.MinValue, DateTime.MaxValue, transitionStart, transitionEnd, daylightDeltaInHours );
        }

        internal static TimeZoneInfo.TransitionTime CreateFixedTime(DateTime instant)
        {
            return TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                timeOfDay: DateTime.MinValue + instant.TimeOfDay,
                month: instant.Month,
                day: instant.Day );
        }

        internal static TimeZoneInfo.TransitionTime CreateFloatingTime(DateTime monthAndTime, int week, DayOfWeek day)
        {
            return TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                timeOfDay: DateTime.MinValue + monthAndTime.TimeOfDay,
                month: monthAndTime.Month,
                week: week,
                dayOfWeek: day );
        }

        internal static double CreateRandomOffset(IFixture fixture, double absMax = 14.0)
        {
            var utcOffsetInHours = fixture.Create<int>() % ((int)absMax * 4) / 4.0;
            var negate = fixture.Create<bool>();
            return utcOffsetInHours * (negate ? -1 : 1);
        }

        private static TimeSpan CreateOffsetTimeSpan(double offsetInHours)
        {
            var result = TimeSpan.FromHours( offsetInHours );
            result = TimeSpan.FromTicks( result.Ticks - result.Ticks % TimeSpan.TicksPerMinute );
            return result;
        }

        private static string StringifyTimeZone(
            TimeSpan utcOffset,
            IReadOnlyCollection<TimeZoneInfo.AdjustmentRule> rules,
            string? idSuffix)
        {
            var utcOffsetText = StringifyOffset( utcOffset );
            idSuffix = string.IsNullOrWhiteSpace( idSuffix ) ? string.Empty : $"{idSuffix} ";

            var result = $"TZ {idSuffix}{utcOffsetText}";

            if ( rules.Count == 0 )
                return result;

            var adjustmentRuleIds = string.Join( '&', rules.Select( StringifyAdjustmentRule ) );
            result = $"{result} [{adjustmentRuleIds}]";

            return result;
        }

        private static string StringifyAdjustmentRule(TimeZoneInfo.AdjustmentRule rule, int index)
        {
            var daylightDeltaText = StringifyOffset( rule.DaylightDelta );
            var ruleStartText = StringifyDate( rule.DateStart );
            var ruleEndText = StringifyDate( rule.DateEnd );
            var timeStartText = StringifyTransitionTime( rule.DaylightTransitionStart );
            var timeEndText = StringifyTransitionTime( rule.DaylightTransitionEnd );

            var result = $"({index}|{daylightDeltaText}|s{ruleStartText}e{ruleEndText}|{timeStartText}>{timeEndText})";
            return result;
        }

        private static string StringifyOffset(TimeSpan offset)
        {
            var absOffset = offset.Abs();
            var signText = offset < TimeSpan.Zero ? '-' : '+';
            var result = $"{signText}{absOffset.Hours:00}:{absOffset.Minutes:00}";
            return result;
        }

        private static string StringifyDate(DateTime dt)
        {
            return $"{dt.Year:0000}{dt.Month:00}{dt.Day:00}";
        }

        private static string StringifyTransitionTime(TimeZoneInfo.TransitionTime time)
        {
            var timeOfDayText = $"{time.TimeOfDay.Hour:00}{time.TimeOfDay.Minute:00}";
            if ( time.IsFixedDateRule )
            {
                var dayOfMonthText = $"{time.Month:00}{time.Day:00}";
                return $"{dayOfMonthText}t{timeOfDayText}";
            }

            var dayOfWeekText = $"{time.Month:00}w{time.Week:0}{(int)time.DayOfWeek:0}";
            return $"{dayOfWeekText}t{timeOfDayText}";
        }
    }
}
