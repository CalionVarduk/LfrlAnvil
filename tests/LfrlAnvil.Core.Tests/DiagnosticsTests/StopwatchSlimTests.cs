using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Tests.DiagnosticsTests;

public class StopwatchSlimTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnCorrectStopwatch()
    {
        var sut = StopwatchSlim.Create();

        using ( new AssertionScope() )
        {
            sut.Start.Should().BeGreaterOrEqualTo( 0 );
            sut.ElapsedTime.Should().BeGreaterOrEqualTo( TimeSpan.Zero );
        }
    }
}
