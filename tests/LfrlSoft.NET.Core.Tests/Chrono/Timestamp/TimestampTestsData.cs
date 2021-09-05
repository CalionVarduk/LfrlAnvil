using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Timestamp
{
    public class TimestampTestsData
    {
        public static TheoryData<long, DateTime> GetTicksCtorData(IFixture fixture)
        {
            return new()
            {
                { 0, DateTime.UnixEpoch },
                { 1, DateTime.UnixEpoch.AddTicks( 1 ) },
                { -1, DateTime.UnixEpoch.AddTicks( -1 ) },
                { 6121840050006, DateTime.UnixEpoch.AddTicks( 6121840050006 ) },
                { -6121840050006, DateTime.UnixEpoch.AddTicks( -6121840050006 ) }
            };
        }

        public static TheoryData<DateTime, long> GetUtcDateTimeCtorData(IFixture fixture)
        {
            return new()
            {
                { DateTime.UnixEpoch, 0 },
                { DateTime.UnixEpoch.AddTicks( 1 ), 1 },
                { DateTime.UnixEpoch.AddTicks( -1 ), -1 },
                { DateTime.UnixEpoch.AddTicks( 6121840050006 ), 6121840050006 },
                { DateTime.UnixEpoch.AddTicks( -6121840050006 ), -6121840050006 }
            };
        }

        public static TheoryData<long, long, bool> GetEqualsData(IFixture fixture)
        {
            return new()
            {
                { 5, 5, true },
                { 5, -5, false }
            };
        }

        public static TheoryData<long, long, int> GetCompareToData(IFixture fixture)
        {
            return new()
            {
                { 5, 5, 0 },
                { -5, 5, -1 },
                { 5, -5, 1 }
            };
        }

        public static TheoryData<long, long, long> GetAddData(IFixture fixture)
        {
            return new()
            {
                { 0, 0, 0 },
                { 5, 0, 5 },
                { 5, 3, 8 },
                { 5, -3, 2 },
                { -5, 0, -5 },
                { -5, 3, -2 },
                { -5, -3, -8 }
            };
        }

        public static TheoryData<long, long, long> GetSubtractData(IFixture fixture)
        {
            return new()
            {
                { 0, 0, 0 },
                { 5, 0, 5 },
                { 5, 3, 2 },
                { 5, -3, 8 },
                { -5, 0, -5 },
                { -5, 3, -8 },
                { -5, -3, -2 }
            };
        }

        public static IEnumerable<object?[]> GetNotEqualsData(IFixture fixture)
        {
            return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
        }

        public static IEnumerable<object?[]> GetGreaterThanComparisonData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r > 0 );
        }

        public static IEnumerable<object?[]> GetGreaterThanOrEqualToComparisonData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r >= 0 );
        }

        public static IEnumerable<object?[]> GetLessThanComparisonData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r < 0 );
        }

        public static IEnumerable<object?[]> GetLessThanOrEqualToComparisonData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r <= 0 );
        }
    }
}
