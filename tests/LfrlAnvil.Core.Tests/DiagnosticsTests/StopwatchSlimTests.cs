using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Tests.DiagnosticsTests;

public class StopwatchSlimTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnCorrectStopwatch()
    {
        var sut = StopwatchSlim.Create();

        Assertion.All(
                sut.Start.TestGreaterThanOrEqualTo( 0 ),
                sut.ElapsedTime.TestGreaterThanOrEqualTo( TimeSpan.Zero ) )
            .Go();
    }
}
