using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Tests.DiagnosticsTests;

public class BenchmarkOptionsTests : TestsBase
{
    [Theory]
    [InlineData( 1, 0.0 )]
    [InlineData( 2, 0.23334523779156066 )]
    [InlineData( 5, 0.5903219460599445 )]
    [InlineData( 10, 0.9391964650700086 )]
    [InlineData( 15, 1.1928788706318845 )]
    [InlineData( 20, 1.4020146218923681 )]
    [InlineData( 25, 1.5 )]
    [InlineData( 30, 1.5 )]
    public void GetDefaultStandardScoreCutoff_ShouldReturnCorrectResult(int sampleCount, double expected)
    {
        var result = BenchmarkOptions.GetDefaultStandardScoreCutoff( sampleCount );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, 0.0 )]
    [InlineData( 2, 0.23334523779156066 )]
    [InlineData( 5, 0.5903219460599445 )]
    [InlineData( 10, 0.9391964650700086 )]
    [InlineData( 15, 1.1928788706318845 )]
    [InlineData( 20, 1.4020146218923681 )]
    [InlineData( 25, 1.5 )]
    [InlineData( 30, 1.5 )]
    public void Create_ShouldReturnCorrectResult(int sampleCount, double expectedStandardCutoff)
    {
        var collectGarbage = Fixture.Create<bool>();
        var samples = new BenchmarkSampleOptions( sampleCount, Fixture.CreatePositiveInt32(), collectGarbage );
        var result = BenchmarkOptions.Create( samples );

        using ( new AssertionScope() )
        {
            result.Samples.Should().BeEquivalentTo( samples );
            result.StandardScoreCutoff.Should().Be( expectedStandardCutoff );
        }
    }

    [Fact]
    public void BenchmarkSampleOptions_ImplicitConversion_ShouldBeEquivalentToCreate()
    {
        var collectGarbage = Fixture.Create<bool>();
        var samples = new BenchmarkSampleOptions( 20, Fixture.CreatePositiveInt32(), collectGarbage );
        var result = (BenchmarkOptions)samples;

        using ( new AssertionScope() )
        {
            result.Samples.Should().BeEquivalentTo( samples );
            result.StandardScoreCutoff.Should().Be( 1.4020146218923681 );
        }
    }
}
