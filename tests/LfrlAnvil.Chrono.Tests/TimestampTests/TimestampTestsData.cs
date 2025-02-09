using System.Collections.Generic;

namespace LfrlAnvil.Chrono.Tests.TimestampTests;

public class TimestampTestsData
{
    public static TheoryData<long, DateTime> GetTicksCtorData(Fixture fixture)
    {
        return new TheoryData<long, DateTime>
        {
            { 0, DateTime.UnixEpoch },
            { 1, DateTime.UnixEpoch.AddTicks( 1 ) },
            { -1, DateTime.UnixEpoch.AddTicks( -1 ) },
            { 6121840050006, DateTime.UnixEpoch.AddTicks( 6121840050006 ) },
            { -6121840050006, DateTime.UnixEpoch.AddTicks( -6121840050006 ) }
        };
    }

    public static TheoryData<DateTime, long> GetUtcDateTimeCtorData(Fixture fixture)
    {
        return new TheoryData<DateTime, long>
        {
            { DateTime.UnixEpoch, 0 },
            { DateTime.UnixEpoch.AddTicks( 1 ), 1 },
            { DateTime.UnixEpoch.AddTicks( -1 ), -1 },
            { DateTime.UnixEpoch.AddTicks( 6121840050006 ), 6121840050006 },
            { DateTime.UnixEpoch.AddTicks( -6121840050006 ), -6121840050006 }
        };
    }

    public static TheoryData<long, long, bool> GetEqualsData(Fixture fixture)
    {
        return new TheoryData<long, long, bool>
        {
            { 5, 5, true },
            { 5, -5, false }
        };
    }

    public static TheoryData<long, long, int> GetCompareToData(Fixture fixture)
    {
        return new TheoryData<long, long, int>
        {
            { 5, 5, 0 },
            { -5, 5, -1 },
            { 5, -5, 1 }
        };
    }

    public static TheoryData<long, long, long> GetAddData(Fixture fixture)
    {
        return new TheoryData<long, long, long>
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

    public static TheoryData<long, long, long> GetSubtractData(Fixture fixture)
    {
        return new TheoryData<long, long, long>
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

    public static IEnumerable<object?[]> GetNotEqualsData(Fixture fixture)
    {
        return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
    }

    public static IEnumerable<object?[]> GetGreaterThanComparisonData(Fixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r > 0 );
    }

    public static IEnumerable<object?[]> GetGreaterThanOrEqualToComparisonData(Fixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r >= 0 );
    }

    public static IEnumerable<object?[]> GetLessThanComparisonData(Fixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r < 0 );
    }

    public static IEnumerable<object?[]> GetLessThanOrEqualToComparisonData(Fixture fixture)
    {
        return GetCompareToData( fixture ).ConvertResult( (int r) => r <= 0 );
    }
}
