using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Extensions.TimeZoneInfo
{
    public class TimeZoneInfoExtensionsTestsData
    {
        public static System.TimeZoneInfo GetTimeZone(
            string id,
            double offsetInHours,
            params System.TimeZoneInfo.AdjustmentRule[] rules)
        {
            var fullName = $"Test Time Zone [{id}]";

            return System.TimeZoneInfo.CreateCustomTimeZone(
                id: fullName,
                baseUtcOffset: TimeSpan.FromHours( offsetInHours ),
                displayName: fullName,
                standardDisplayName: fullName,
                daylightDisplayName: fullName,
                adjustmentRules: rules );
        }

        public static System.TimeZoneInfo.AdjustmentRule CreateAdjustmentRule(
            System.DateTime start,
            System.DateTime end,
            System.DateTime transitionStart,
            System.DateTime transitionEnd,
            double daylightDeltaInHours = 1)
        {
            return System.TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: start,
                dateEnd: end,
                daylightDelta: TimeSpan.FromHours( daylightDeltaInHours ),
                daylightTransitionStart: System.TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                    timeOfDay: new System.DateTime( transitionStart.TimeOfDay.Ticks ),
                    month: transitionStart.Month,
                    day: transitionStart.Day ),
                daylightTransitionEnd: System.TimeZoneInfo.TransitionTime.CreateFixedDateRule(
                    timeOfDay: new System.DateTime( transitionEnd.TimeOfDay.Ticks ),
                    month: transitionEnd.Month,
                    day: transitionEnd.Day ) );
        }

        public static TheoryData<System.DateTime, IEnumerable<(System.DateTime Start, System.DateTime End)>>
            GetGetActiveAdjustmentRuleWithNullResultData(
                IFixture fixture)
        {
            var (a, b, c, d, e) = fixture.CreateDistinctSortedCollection<System.DateTime>( count: 5 );

            return new TheoryData<System.DateTime, IEnumerable<(System.DateTime, System.DateTime)>>
            {
                { a, Enumerable.Empty<(System.DateTime, System.DateTime)>() },
                { a, new[] { (b.Date, c.Date) } },
                { c, new[] { (a.Date, b.Date) } },
                { c, new[] { (a.Date, b.Date), (d.Date, e.Date) } },
                { a.Date.AddTicks( -1 ), new[] { (a.Date, b.Date) } },
                { b.Date.AddDays( 1 ), new[] { (a.Date, b.Date) } }
            };
        }

        public static TheoryData<
                System.DateTime,
                IEnumerable<(System.DateTime Start, System.DateTime End)>,
                (System.DateTime Start, System.DateTime End)>
            GetGetActiveAdjustmentRuleData(
                IFixture fixture)
        {
            var baseDt = fixture.Create<System.DateTime>().Date;
            var (a, b, c, d, e) = (baseDt.AddDays( -2 ), baseDt.AddDays( -1 ), baseDt, baseDt.AddDays( 1 ), baseDt.AddDays( 2 ));

            return new TheoryData<System.DateTime, IEnumerable<(System.DateTime, System.DateTime)>, (System.DateTime, System.DateTime)>
            {
                { b, new[] { (a.Date, c.Date) }, (a.Date, c.Date) },
                { b, new[] { (a.Date, c.Date), (d.Date, e.Date) }, (a.Date, c.Date) },
                { d, new[] { (a.Date, b.Date), (c.Date, e.Date) }, (c.Date, e.Date) },
                { a.Date, new[] { (a.Date, c.Date) }, (a.Date, c.Date) },
                { c.Date.AddDays( 1 ).AddTicks( -1 ), new[] { (a.Date, c.Date) }, (a.Date, c.Date) }
            };
        }

        public static TheoryData<
                System.DateTime,
                IEnumerable<(System.DateTime Start, System.DateTime End)>,
                int>
            GetGetActiveAdjustmentRuleIndexData(
                IFixture fixture)
        {
            var baseDt = fixture.Create<System.DateTime>().Date;
            var (a, b, c, d, e) = (baseDt.AddDays( -2 ), baseDt.AddDays( -1 ), baseDt, baseDt.AddDays( 1 ), baseDt.AddDays( 2 ));

            return new TheoryData<System.DateTime, IEnumerable<(System.DateTime, System.DateTime)>, int>
            {
                { b, new[] { (a.Date, c.Date) }, 0 },
                { b, new[] { (a.Date, c.Date), (d.Date, e.Date) }, 0 },
                { d, new[] { (a.Date, b.Date), (c.Date, e.Date) }, 1 },
                { a.Date, new[] { (a.Date, c.Date) }, 0 },
                { c.Date.AddDays( 1 ).AddTicks( -1 ), new[] { (a.Date, c.Date) }, 0 }
            };
        }

        public static TheoryData<System.TimeZoneInfo, System.DateTime, (System.DateTime Start, System.DateTime End)?>
            GetGetContainingInvalidityRangeData(
                IFixture fixture)
        {
            var simpleTimeZone = GetTimeZone(
                "1 (simple +DS)",
                1,
                CreateAdjustmentRule(
                    System.DateTime.MinValue,
                    System.DateTime.MaxValue,
                    new System.DateTime( 1, 2, 1, 3, 0, 0 ),
                    new System.DateTime( 1, 11, 1, 3, 0, 0 ) ) );

            var reverseSimpleTimeZone = GetTimeZone(
                "1 (simple -DS)",
                1,
                CreateAdjustmentRule(
                    System.DateTime.MinValue,
                    System.DateTime.MaxValue,
                    new System.DateTime( 1, 2, 1, 3, 0, 0 ),
                    new System.DateTime( 1, 11, 1, 3, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            var yearOverlapTimeZone = GetTimeZone(
                "1 (year overlap +DS)",
                1,
                CreateAdjustmentRule(
                    System.DateTime.MinValue,
                    System.DateTime.MaxValue,
                    new System.DateTime( 1, 12, 31, 23, 30, 0 ),
                    new System.DateTime( 1, 2, 1, 3, 0, 0 ) ) );

            var reverseYearOverlapTimeZone = GetTimeZone(
                "1 (year overlap -DS)",
                1,
                CreateAdjustmentRule(
                    System.DateTime.MinValue,
                    System.DateTime.MaxValue,
                    new System.DateTime( 1, 2, 1, 3, 0, 0 ),
                    new System.DateTime( 1, 12, 31, 23, 30, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<System.TimeZoneInfo, System.DateTime, (System.DateTime, System.DateTime)?>
            {
                { System.TimeZoneInfo.Utc, new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ), null },
                { simpleTimeZone, new System.DateTime( 2021, 2, 1, 2, 59, 59, 999 ).AddTicks( 9999 ), null },
                { simpleTimeZone, new System.DateTime( 2021, 2, 1, 4, 0, 0 ), null },
                {
                    simpleTimeZone,
                    new System.DateTime( 2021, 2, 1, 3, 0, 0 ),
                    (new System.DateTime( 2021, 2, 1, 3, 0, 0 ), new System.DateTime( 2021, 2, 1, 3, 59, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    simpleTimeZone,
                    new System.DateTime( 2021, 2, 1, 3, 59, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2021, 2, 1, 3, 0, 0 ), new System.DateTime( 2021, 2, 1, 3, 59, 59, 999 ).AddTicks( 9999 ))
                },
                { reverseSimpleTimeZone, new System.DateTime( 2021, 11, 1, 2, 59, 59, 999 ).AddTicks( 9999 ), null },
                { reverseSimpleTimeZone, new System.DateTime( 2021, 11, 1, 4, 0, 0 ), null },
                {
                    reverseSimpleTimeZone,
                    new System.DateTime( 2021, 11, 1, 3, 0, 0 ),
                    (new System.DateTime( 2021, 11, 1, 3, 0, 0 ), new System.DateTime( 2021, 11, 1, 3, 59, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseSimpleTimeZone,
                    new System.DateTime( 2021, 11, 1, 3, 59, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2021, 11, 1, 3, 0, 0 ), new System.DateTime( 2021, 11, 1, 3, 59, 59, 999 ).AddTicks( 9999 ))
                },
                { yearOverlapTimeZone, new System.DateTime( 2021, 1, 1, 0, 30, 0, 0 ), null },
                { yearOverlapTimeZone, new System.DateTime( 2021, 12, 31, 23, 29, 59, 999 ).AddTicks( 9999 ), null },
                {
                    yearOverlapTimeZone,
                    new System.DateTime( 2021, 1, 1 ),
                    (new System.DateTime( 2020, 12, 31, 23, 30, 0 ), new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2020, 12, 31, 23, 30, 0 ), new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new System.DateTime( 2021, 12, 31, 23, 30, 0 ),
                    (new System.DateTime( 2021, 12, 31, 23, 30, 0 ), new System.DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new System.DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2021, 12, 31, 23, 30, 0 ), new System.DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                { reverseYearOverlapTimeZone, new System.DateTime( 2021, 1, 1, 0, 30, 0, 0 ), null },
                { reverseYearOverlapTimeZone, new System.DateTime( 2021, 12, 31, 23, 29, 59, 999 ).AddTicks( 9999 ), null },
                {
                    reverseYearOverlapTimeZone,
                    new System.DateTime( 2021, 1, 1 ),
                    (new System.DateTime( 2020, 12, 31, 23, 30, 0 ), new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2020, 12, 31, 23, 30, 0 ), new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new System.DateTime( 2021, 12, 31, 23, 30, 0 ),
                    (new System.DateTime( 2021, 12, 31, 23, 30, 0 ), new System.DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new System.DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2021, 12, 31, 23, 30, 0 ), new System.DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                }
            };
        }

        public static TheoryData<System.TimeZoneInfo, System.DateTime, (System.DateTime Start, System.DateTime End)?>
            GetGetContainingAmbiguityRangeData(
                IFixture fixture)
        {
            var simpleTimeZone = GetTimeZone(
                "1 (simple +DS)",
                1,
                CreateAdjustmentRule(
                    System.DateTime.MinValue,
                    System.DateTime.MaxValue,
                    new System.DateTime( 1, 2, 1, 3, 0, 0 ),
                    new System.DateTime( 1, 11, 1, 3, 0, 0 ) ) );

            var reverseSimpleTimeZone = GetTimeZone(
                "1 (simple -DS)",
                1,
                CreateAdjustmentRule(
                    System.DateTime.MinValue,
                    System.DateTime.MaxValue,
                    new System.DateTime( 1, 2, 1, 3, 0, 0 ),
                    new System.DateTime( 1, 11, 1, 3, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            var yearOverlapTimeZone = GetTimeZone(
                "1 (year overlap +DS)",
                1,
                CreateAdjustmentRule(
                    System.DateTime.MinValue,
                    System.DateTime.MaxValue,
                    new System.DateTime( 1, 2, 1, 3, 0, 0 ),
                    new System.DateTime( 1, 1, 1, 0, 30, 0 ) ) );

            var reverseYearOverlapTimeZone = GetTimeZone(
                "1 (year overlap -DS)",
                1,
                CreateAdjustmentRule(
                    System.DateTime.MinValue,
                    System.DateTime.MaxValue,
                    new System.DateTime( 1, 1, 1, 0, 30, 0 ),
                    new System.DateTime( 1, 2, 1, 3, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<System.TimeZoneInfo, System.DateTime, (System.DateTime, System.DateTime)?>
            {
                { System.TimeZoneInfo.Utc, new System.DateTime( 2021, 8, 26, 12, 30, 40, 500 ), null },
                { simpleTimeZone, new System.DateTime( 2021, 11, 1, 1, 59, 59, 999 ).AddTicks( 9999 ), null },
                { simpleTimeZone, new System.DateTime( 2021, 11, 1, 3, 0, 0 ), null },
                {
                    simpleTimeZone,
                    new System.DateTime( 2021, 11, 1, 2, 0, 0 ),
                    (new System.DateTime( 2021, 11, 1, 2, 0, 0 ), new System.DateTime( 2021, 11, 1, 2, 59, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    simpleTimeZone,
                    new System.DateTime( 2021, 11, 1, 2, 59, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2021, 11, 1, 2, 0, 0 ), new System.DateTime( 2021, 11, 1, 2, 59, 59, 999 ).AddTicks( 9999 ))
                },
                { reverseSimpleTimeZone, new System.DateTime( 2021, 2, 1, 1, 59, 59, 999 ).AddTicks( 9999 ), null },
                { reverseSimpleTimeZone, new System.DateTime( 2021, 2, 1, 3, 0, 0 ), null },
                {
                    reverseSimpleTimeZone,
                    new System.DateTime( 2021, 2, 1, 2, 0, 0 ),
                    (new System.DateTime( 2021, 2, 1, 2, 0, 0 ), new System.DateTime( 2021, 2, 1, 2, 59, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseSimpleTimeZone,
                    new System.DateTime( 2021, 2, 1, 2, 59, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2021, 2, 1, 2, 0, 0 ), new System.DateTime( 2021, 2, 1, 2, 59, 59, 999 ).AddTicks( 9999 ))
                },
                { yearOverlapTimeZone, new System.DateTime( 2021, 1, 1, 0, 30, 0, 0 ), null },
                { yearOverlapTimeZone, new System.DateTime( 2021, 12, 31, 23, 29, 59, 999 ).AddTicks( 9999 ), null },
                {
                    yearOverlapTimeZone,
                    new System.DateTime( 2021, 1, 1 ),
                    (new System.DateTime( 2020, 12, 31, 23, 30, 0 ), new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2020, 12, 31, 23, 30, 0 ), new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new System.DateTime( 2021, 12, 31, 23, 30, 0 ),
                    (new System.DateTime( 2021, 12, 31, 23, 30, 0 ), new System.DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new System.DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2021, 12, 31, 23, 30, 0 ), new System.DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                { reverseYearOverlapTimeZone, new System.DateTime( 2021, 1, 1, 0, 30, 0, 0 ), null },
                { reverseYearOverlapTimeZone, new System.DateTime( 2021, 12, 31, 23, 29, 59, 999 ).AddTicks( 9999 ), null },
                {
                    reverseYearOverlapTimeZone,
                    new System.DateTime( 2021, 1, 1 ),
                    (new System.DateTime( 2020, 12, 31, 23, 30, 0 ), new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2020, 12, 31, 23, 30, 0 ), new System.DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new System.DateTime( 2021, 12, 31, 23, 30, 0 ),
                    (new System.DateTime( 2021, 12, 31, 23, 30, 0 ), new System.DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new System.DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    (new System.DateTime( 2021, 12, 31, 23, 30, 0 ), new System.DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                }
            };
        }
    }
}
