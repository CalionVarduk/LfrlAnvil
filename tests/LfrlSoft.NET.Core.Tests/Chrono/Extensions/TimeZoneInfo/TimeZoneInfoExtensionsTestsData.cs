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
            var (a, b, c, d, e) = fixture.CreateDistinctSortedCollection<System.DateTime>( count: 5 );

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
            var (a, b, c, d, e) = fixture.CreateDistinctSortedCollection<System.DateTime>( count: 5 );

            return new TheoryData<System.DateTime, IEnumerable<(System.DateTime, System.DateTime)>, int>
            {
                { b, new[] { (a.Date, c.Date) }, 0 },
                { b, new[] { (a.Date, c.Date), (d.Date, e.Date) }, 0 },
                { d, new[] { (a.Date, b.Date), (c.Date, e.Date) }, 1 },
                { a.Date, new[] { (a.Date, c.Date) }, 0 },
                { c.Date.AddDays( 1 ).AddTicks( -1 ), new[] { (a.Date, c.Date) }, 0 }
            };
        }
    }
}
