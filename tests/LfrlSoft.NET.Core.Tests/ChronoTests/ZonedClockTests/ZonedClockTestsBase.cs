using System;
using AutoFixture;
using LfrlSoft.NET.Core.Tests.ChronoTests.ZonedDateTimeTests;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.ChronoTests.ZonedClockTests
{
    public abstract class ZonedClockTestsBase : TestsBase
    {
        protected TimeZoneInfo CreateTimeZone()
        {
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            return timeZone;
        }
    }
}
