using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Tests.DiagnosticsTests.TimeMeasurementTests;

public class TimeMeasurementTests : TestsBase
{
    [Fact]
    public void Zero_ShouldReturnCorrectResult()
    {
        var sut = TimeMeasurement.Zero;

        using ( new AssertionScope() )
        {
            sut.Preparation.Should().Be( TimeSpan.Zero );
            sut.Invocation.Should().Be( TimeSpan.Zero );
            sut.Teardown.Should().Be( TimeSpan.Zero );
            sut.Total.Should().Be( TimeSpan.Zero );
        }
    }

    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = default( TimeMeasurement );

        using ( new AssertionScope() )
        {
            sut.Preparation.Should().Be( TimeSpan.Zero );
            sut.Invocation.Should().Be( TimeSpan.Zero );
            sut.Teardown.Should().Be( TimeSpan.Zero );
            sut.Total.Should().Be( TimeSpan.Zero );
        }
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var preparation = Fixture.Create<TimeSpan>();
        var invocation = Fixture.Create<TimeSpan>();
        var teardown = Fixture.Create<TimeSpan>();
        var sut = new TimeMeasurement( preparation, invocation, teardown );

        using ( new AssertionScope() )
        {
            sut.Preparation.Should().Be( preparation );
            sut.Invocation.Should().Be( invocation );
            sut.Teardown.Should().Be( teardown );
            sut.Total.Should().Be( preparation + invocation + teardown );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var preparation = TimeSpan.FromSeconds( 0.1234567 );
        var invocation = TimeSpan.FromSeconds( 1.2345678 );
        var teardown = TimeSpan.FromSeconds( 123.456789 );
        var sut = new TimeMeasurement( preparation, invocation, teardown );

        var result = sut.ToString();

        result.Should().Be( "Preparation: 0.1234567s, Invocation: 1.2345678s, Teardown: 123.4567890s (Total: 124.8148135s)" );
    }
}
