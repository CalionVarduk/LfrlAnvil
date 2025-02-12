using System.Threading.Tasks;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Chrono.Tests.TimestampProviderTests;

public class PreciseTimestampProviderTests : TestsBase
{
    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    [InlineData( -2 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenPrecisionResetTimeoutIsInvalid(long value)
    {
        var action = Lambda.Of( () => new PreciseTimestampProvider( Duration.FromTicks( value ) ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 100 )]
    [InlineData( 12345 )]
    public void Ctor_ShouldReturnCorrectResult(long value)
    {
        var sut = new PreciseTimestampProvider( Duration.FromTicks( value ) );
        sut.PrecisionResetTimeout.TestEquals( Duration.FromTicks( value ) ).Go();
    }

    [Fact]
    public void DefaultCtor_ShouldReturnCorrectResult()
    {
        var sut = new PreciseTimestampProvider();
        sut.PrecisionResetTimeout.TestEquals( Duration.FromMinutes( 1 ) ).Go();
    }

    [Fact]
    public void GetNow_ShouldReturnCorrectResult()
    {
        var expectedMin = DateTime.UtcNow;
        var sut = new PreciseTimestampProvider( Duration.FromHours( 24 ) );

        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        result.UtcValue.TestInRange( expectedMin, expectedMax ).Go();
    }

    [Fact]
    public async Task GetNow_ShouldReturnCorrectResult_WhenPrecisionResetTimeoutIsExceeded()
    {
        var sut = new PreciseTimestampProvider( Duration.FromTicks( 1 ) );

        await Task.Delay( TimeSpan.FromMilliseconds( 1 ) );

        var expectedMin = DateTime.UtcNow;
        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        result.UtcValue.TestInRange( expectedMin, expectedMax ).Go();
    }
}
