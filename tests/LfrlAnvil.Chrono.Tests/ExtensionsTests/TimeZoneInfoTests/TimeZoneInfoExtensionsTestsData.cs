using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.TimeZoneInfoTests
{
    public class TimeZoneInfoExtensionsTestsData
    {
        public static TheoryData<DateTime, IEnumerable<(DateTime Start, DateTime End)>>
            GetGetActiveAdjustmentRuleWithNullResultData(
                IFixture fixture)
        {
            var (a, b, c, d, e) = fixture.CreateDistinctSortedCollection<DateTime>( count: 5 );

            return new TheoryData<DateTime, IEnumerable<(DateTime, DateTime)>>
            {
                { a, Enumerable.Empty<(DateTime, DateTime)>() },
                { a.Date, new[] { (b.Date, c.Date) } },
                { c.Date, new[] { (a.Date, b.Date) } },
                { c.Date, new[] { (a.Date, b.Date), (d.Date, e.Date) } },
                { a.Date.AddTicks( -1 ), new[] { (a.Date, b.Date) } },
                { b.Date.AddDays( 1 ), new[] { (a.Date, b.Date) } }
            };
        }

        public static TheoryData<
                DateTime,
                IEnumerable<(DateTime Start, DateTime End)>,
                (DateTime Start, DateTime End)>
            GetGetActiveAdjustmentRuleData(
                IFixture fixture)
        {
            var baseDt = fixture.Create<DateTime>().Date;
            var (a, b, c, d, e) = (baseDt.AddDays( -2 ), baseDt.AddDays( -1 ), baseDt, baseDt.AddDays( 1 ), baseDt.AddDays( 2 ));

            return new TheoryData<DateTime, IEnumerable<(DateTime, DateTime)>, (DateTime, DateTime)>
            {
                { b, new[] { (a.Date, c.Date) }, (a.Date, c.Date) },
                { b, new[] { (a.Date, c.Date), (d.Date, e.Date) }, (a.Date, c.Date) },
                { d, new[] { (a.Date, b.Date), (c.Date, e.Date) }, (c.Date, e.Date) },
                { a.Date, new[] { (a.Date, c.Date) }, (a.Date, c.Date) },
                { c.Date.AddDays( 1 ).AddTicks( -1 ), new[] { (a.Date, c.Date) }, (a.Date, c.Date) }
            };
        }

        public static TheoryData<
                DateTime,
                IEnumerable<(DateTime Start, DateTime End)>,
                int>
            GetGetActiveAdjustmentRuleIndexData(
                IFixture fixture)
        {
            var baseDt = fixture.Create<DateTime>().Date;
            var (a, b, c, d, e) = (baseDt.AddDays( -2 ), baseDt.AddDays( -1 ), baseDt, baseDt.AddDays( 1 ), baseDt.AddDays( 2 ));

            return new TheoryData<DateTime, IEnumerable<(DateTime, DateTime)>, int>
            {
                { b, new[] { (a.Date, c.Date) }, 0 },
                { b, new[] { (a.Date, c.Date), (d.Date, e.Date) }, 0 },
                { d, new[] { (a.Date, b.Date), (c.Date, e.Date) }, 1 },
                { a.Date, new[] { (a.Date, c.Date) }, 0 },
                { c.Date.AddDays( 1 ).AddTicks( -1 ), new[] { (a.Date, c.Date) }, 0 }
            };
        }

        public static TheoryData<TimeZoneInfo, DateTime, (DateTime Start, DateTime End)?>
            GetGetContainingInvalidityRangeData(
                IFixture fixture)
        {
            var simpleTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1.0,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 2, 1, 3, 0, 0 ),
                    transitionEnd: new DateTime( 1, 11, 1, 3, 0, 0 ) ) );

            var reverseSimpleTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1.0,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 2, 1, 3, 0, 0 ),
                    transitionEnd: new DateTime( 1, 11, 1, 3, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            var yearOverlapTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1.0,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 12, 31, 23, 30, 0 ),
                    transitionEnd: new DateTime( 1, 2, 1, 3, 0, 0 ) ) );

            var reverseYearOverlapTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1.0,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 2, 1, 3, 0, 0 ),
                    transitionEnd: new DateTime( 1, 12, 31, 23, 30, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<TimeZoneInfo, DateTime, (DateTime, DateTime)?>
            {
                { TimeZoneInfo.Utc, new DateTime( 2021, 8, 26, 12, 30, 40, 500 ), null },
                { simpleTimeZone, new DateTime( 2021, 2, 1, 2, 59, 59, 999 ).AddTicks( 9999 ), null },
                { simpleTimeZone, new DateTime( 2021, 2, 1, 4, 0, 0 ), null },
                {
                    simpleTimeZone,
                    new DateTime( 2021, 2, 1, 3, 0, 0 ),
                    (new DateTime( 2021, 2, 1, 3, 0, 0 ), new DateTime( 2021, 2, 1, 3, 59, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    simpleTimeZone,
                    new DateTime( 2021, 2, 1, 3, 59, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2021, 2, 1, 3, 0, 0 ), new DateTime( 2021, 2, 1, 3, 59, 59, 999 ).AddTicks( 9999 ))
                },
                { reverseSimpleTimeZone, new DateTime( 2021, 11, 1, 2, 59, 59, 999 ).AddTicks( 9999 ), null },
                { reverseSimpleTimeZone, new DateTime( 2021, 11, 1, 4, 0, 0 ), null },
                {
                    reverseSimpleTimeZone,
                    new DateTime( 2021, 11, 1, 3, 0, 0 ),
                    (new DateTime( 2021, 11, 1, 3, 0, 0 ), new DateTime( 2021, 11, 1, 3, 59, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseSimpleTimeZone,
                    new DateTime( 2021, 11, 1, 3, 59, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2021, 11, 1, 3, 0, 0 ), new DateTime( 2021, 11, 1, 3, 59, 59, 999 ).AddTicks( 9999 ))
                },
                { yearOverlapTimeZone, new DateTime( 2021, 1, 1, 0, 30, 0, 0 ), null },
                { yearOverlapTimeZone, new DateTime( 2021, 12, 31, 23, 29, 59, 999 ).AddTicks( 9999 ), null },
                {
                    yearOverlapTimeZone,
                    new DateTime( 2021, 1, 1 ),
                    (new DateTime( 2020, 12, 31, 23, 30, 0 ), new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2020, 12, 31, 23, 30, 0 ), new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new DateTime( 2021, 12, 31, 23, 30, 0 ),
                    (new DateTime( 2021, 12, 31, 23, 30, 0 ), new DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2021, 12, 31, 23, 30, 0 ), new DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                { reverseYearOverlapTimeZone, new DateTime( 2021, 1, 1, 0, 30, 0, 0 ), null },
                { reverseYearOverlapTimeZone, new DateTime( 2021, 12, 31, 23, 29, 59, 999 ).AddTicks( 9999 ), null },
                {
                    reverseYearOverlapTimeZone,
                    new DateTime( 2021, 1, 1 ),
                    (new DateTime( 2020, 12, 31, 23, 30, 0 ), new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2020, 12, 31, 23, 30, 0 ), new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new DateTime( 2021, 12, 31, 23, 30, 0 ),
                    (new DateTime( 2021, 12, 31, 23, 30, 0 ), new DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2021, 12, 31, 23, 30, 0 ), new DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                }
            };
        }

        public static TheoryData<TimeZoneInfo, DateTime, (DateTime Start, DateTime End)?>
            GetGetContainingAmbiguityRangeData(
                IFixture fixture)
        {
            var simpleTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1.0,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 2, 1, 3, 0, 0 ),
                    transitionEnd: new DateTime( 1, 11, 1, 3, 0, 0 ) ) );

            var reverseSimpleTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1.0,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 2, 1, 3, 0, 0 ),
                    transitionEnd: new DateTime( 1, 11, 1, 3, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            var yearOverlapTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1.0,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 2, 1, 3, 0, 0 ),
                    transitionEnd: new DateTime( 1, 1, 1, 0, 30, 0 ) ) );

            var reverseYearOverlapTimeZone = TimeZoneFactory.Create(
                utcOffsetInHours: 1.0,
                TimeZoneFactory.CreateInfiniteRule(
                    transitionStart: new DateTime( 1, 1, 1, 0, 30, 0 ),
                    transitionEnd: new DateTime( 1, 2, 1, 3, 0, 0 ),
                    daylightDeltaInHours: -1 ) );

            return new TheoryData<TimeZoneInfo, DateTime, (DateTime, DateTime)?>
            {
                { TimeZoneInfo.Utc, new DateTime( 2021, 8, 26, 12, 30, 40, 500 ), null },
                { simpleTimeZone, new DateTime( 2021, 11, 1, 1, 59, 59, 999 ).AddTicks( 9999 ), null },
                { simpleTimeZone, new DateTime( 2021, 11, 1, 3, 0, 0 ), null },
                {
                    simpleTimeZone,
                    new DateTime( 2021, 11, 1, 2, 0, 0 ),
                    (new DateTime( 2021, 11, 1, 2, 0, 0 ), new DateTime( 2021, 11, 1, 2, 59, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    simpleTimeZone,
                    new DateTime( 2021, 11, 1, 2, 59, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2021, 11, 1, 2, 0, 0 ), new DateTime( 2021, 11, 1, 2, 59, 59, 999 ).AddTicks( 9999 ))
                },
                { reverseSimpleTimeZone, new DateTime( 2021, 2, 1, 1, 59, 59, 999 ).AddTicks( 9999 ), null },
                { reverseSimpleTimeZone, new DateTime( 2021, 2, 1, 3, 0, 0 ), null },
                {
                    reverseSimpleTimeZone,
                    new DateTime( 2021, 2, 1, 2, 0, 0 ),
                    (new DateTime( 2021, 2, 1, 2, 0, 0 ), new DateTime( 2021, 2, 1, 2, 59, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseSimpleTimeZone,
                    new DateTime( 2021, 2, 1, 2, 59, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2021, 2, 1, 2, 0, 0 ), new DateTime( 2021, 2, 1, 2, 59, 59, 999 ).AddTicks( 9999 ))
                },
                { yearOverlapTimeZone, new DateTime( 2021, 1, 1, 0, 30, 0, 0 ), null },
                { yearOverlapTimeZone, new DateTime( 2021, 12, 31, 23, 29, 59, 999 ).AddTicks( 9999 ), null },
                {
                    yearOverlapTimeZone,
                    new DateTime( 2021, 1, 1 ),
                    (new DateTime( 2020, 12, 31, 23, 30, 0 ), new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2020, 12, 31, 23, 30, 0 ), new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new DateTime( 2021, 12, 31, 23, 30, 0 ),
                    (new DateTime( 2021, 12, 31, 23, 30, 0 ), new DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    yearOverlapTimeZone,
                    new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2021, 12, 31, 23, 30, 0 ), new DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                { reverseYearOverlapTimeZone, new DateTime( 2021, 1, 1, 0, 30, 0, 0 ), null },
                { reverseYearOverlapTimeZone, new DateTime( 2021, 12, 31, 23, 29, 59, 999 ).AddTicks( 9999 ), null },
                {
                    reverseYearOverlapTimeZone,
                    new DateTime( 2021, 1, 1 ),
                    (new DateTime( 2020, 12, 31, 23, 30, 0 ), new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2020, 12, 31, 23, 30, 0 ), new DateTime( 2021, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new DateTime( 2021, 12, 31, 23, 30, 0 ),
                    (new DateTime( 2021, 12, 31, 23, 30, 0 ), new DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                },
                {
                    reverseYearOverlapTimeZone,
                    new DateTime( 2021, 12, 31, 23, 59, 59, 999 ).AddTicks( 9999 ),
                    (new DateTime( 2021, 12, 31, 23, 30, 0 ), new DateTime( 2022, 1, 1, 0, 29, 59, 999 ).AddTicks( 9999 ))
                }
            };
        }
    }
}
