using System.Diagnostics;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Tests.DiagnosticsTests;

public class StopwatchTimestampTests : TestsBase
{
    [Fact]
    public void GetTicks_ShouldConvertStopwatchTicksToDateTimeTicks()
    {
        var result = StopwatchTimestamp.GetTicks( 10, 50 );
        var expected = ( long )(40 * ( double )TimeSpan.TicksPerSecond / Stopwatch.Frequency);
        result.Should().Be( expected );
    }

    [Fact]
    public void GetTimeSpan_ShouldConvertStopwatchTicksToTimeSpan()
    {
        var result = StopwatchTimestamp.GetTimeSpan( 10, 50 );
        var expected = ( long )(40 * ( double )TimeSpan.TicksPerSecond / Stopwatch.Frequency);
        result.Ticks.Should().Be( expected );
    }

    [Fact]
    public void GetStopwatchTicks_WithInt64_ShouldConvertDateTimeTicksToStopwatchTicks()
    {
        var result = StopwatchTimestamp.GetStopwatchTicks( 50 );
        var expected = ( long )(50 * Stopwatch.Frequency / ( double )TimeSpan.TicksPerSecond);
        result.Should().Be( expected );
    }

    [Fact]
    public void GetStopwatchTicks_WithTimeSpan_ShouldConvertTimeSpanToStopwatchTicks()
    {
        var result = StopwatchTimestamp.GetStopwatchTicks( TimeSpan.FromTicks( 50 ) );
        var expected = ( long )(50 * Stopwatch.Frequency / ( double )TimeSpan.TicksPerSecond);
        result.Should().Be( expected );
    }
}
