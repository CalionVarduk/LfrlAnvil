using System.Collections.Generic;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.BoundsRangeTests;

public class BoundsRangeExtensionsTestsData
{
    public static TheoryData<IEnumerable<(DateTime Min, DateTime Max)>, TimeSpan> GetGetTimeSpanData(IFixture fixture)
    {
        var dt1 = new DateTime( 2021, 8, 26 );
        var dt2 = new DateTime( 2021, 8, 27, 12, 34, 56, 789 ).AddTicks( 9876 );
        var dt3 = new DateTime( 2021, 8, 27, 13, 34, 56, 789 ).AddTicks( 9876 );
        var dt4 = new DateTime( 2021, 8, 27, 14, 34, 56, 789 ).AddTicks( 9875 );

        return new TheoryData<IEnumerable<(DateTime Min, DateTime Max)>, TimeSpan>
        {
            { new[] { (dt1, dt1) }, TimeSpan.FromTicks( 1 ) },
            { new[] { (dt1, dt1), (dt2, dt2), (dt3, dt3) }, TimeSpan.FromTicks( 3 ) },
            { new[] { (dt1, dt2), (dt3, dt4) }, new TimeSpan( 1, 13, 34, 56, 789 ) + TimeSpan.FromTicks( 9877 ) }
        };
    }

    public static TheoryData<IEnumerable<(DateTime Min, DateTime Max)>, Duration> GetGetDurationData(IFixture fixture)
    {
        var dt1 = new DateTime( 2021, 8, 26 );
        var dt2 = new DateTime( 2021, 8, 27, 12, 34, 56, 789 ).AddTicks( 9876 );
        var dt3 = new DateTime( 2021, 8, 27, 13, 34, 56, 789 ).AddTicks( 9876 );
        var dt4 = new DateTime( 2021, 8, 27, 14, 34, 56, 789 ).AddTicks( 9875 );

        return new TheoryData<IEnumerable<(DateTime Min, DateTime Max)>, Duration>
        {
            { new[] { (dt1, dt1) }, Duration.FromTicks( 1 ) },
            { new[] { (dt1, dt1), (dt2, dt2), (dt3, dt3) }, Duration.FromTicks( 3 ) },
            { new[] { (dt1, dt2), (dt3, dt4) }, new Duration( 37, 34, 56, 789, 9877 ) }
        };
    }
}
