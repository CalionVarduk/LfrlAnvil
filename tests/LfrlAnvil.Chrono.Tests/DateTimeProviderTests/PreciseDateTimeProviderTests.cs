using System.Threading.Tasks;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Chrono.Tests.DateTimeProviderTests;

public class PreciseDateTimeProviderTests : TestsBase
{
    [Fact]
    public void Utc_ShouldReturnCorrectResult()
    {
        var sut = PreciseDateTimeProvider.Utc;
        sut.Kind.TestEquals( DateTimeKind.Utc ).Go();
    }

    [Fact]
    public void Local_ShouldReturnCorrectResult()
    {
        var sut = PreciseDateTimeProvider.Local;
        sut.Kind.TestEquals( DateTimeKind.Local ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    [InlineData( -2 )]
    public void Ctor_ForUtc_ShouldThrowArgumentOutOfRangeException_WhenPrecisionResetTimeoutIsInvalid(long value)
    {
        var action = Lambda.Of( () => new PreciseUtcDateTimeProvider( Duration.FromTicks( value ) ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    [InlineData( -2 )]
    public void Ctor_ForLocal_ShouldThrowArgumentOutOfRangeException_WhenPrecisionResetTimeoutIsInvalid(long value)
    {
        var action = Lambda.Of( () => new PreciseLocalDateTimeProvider( Duration.FromTicks( value ) ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 100 )]
    [InlineData( 12345 )]
    public void Ctor_ForUtc_ShouldReturnCorrectResult(long value)
    {
        var sut = new PreciseUtcDateTimeProvider( Duration.FromTicks( value ) );

        Assertion.All(
                sut.Kind.TestEquals( DateTimeKind.Utc ),
                sut.PrecisionResetTimeout.TestEquals( Duration.FromTicks( value ) ) )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 100 )]
    [InlineData( 12345 )]
    public void Ctor_ForLocal_ShouldReturnCorrectResult(long value)
    {
        var sut = new PreciseLocalDateTimeProvider( Duration.FromTicks( value ) );

        Assertion.All(
                sut.Kind.TestEquals( DateTimeKind.Local ),
                sut.PrecisionResetTimeout.TestEquals( Duration.FromTicks( value ) ) )
            .Go();
    }

    [Fact]
    public void DefaultCtor_ForUtc_ShouldReturnCorrectResult()
    {
        var sut = new PreciseUtcDateTimeProvider();

        Assertion.All(
                sut.Kind.TestEquals( DateTimeKind.Utc ),
                sut.PrecisionResetTimeout.TestEquals( Duration.FromMinutes( 1 ) ) )
            .Go();
    }

    [Fact]
    public void DefaultCtor_ForLocal_ShouldReturnCorrectResult()
    {
        var sut = new PreciseLocalDateTimeProvider();

        Assertion.All(
                sut.Kind.TestEquals( DateTimeKind.Local ),
                sut.PrecisionResetTimeout.TestEquals( Duration.FromMinutes( 1 ) ) )
            .Go();
    }

    [Fact]
    public void GetNow_ForUtc_ShouldReturnCorrectResult()
    {
        var expectedMin = DateTime.UtcNow;
        var sut = new PreciseUtcDateTimeProvider( Duration.FromHours( 24 ) );

        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        Assertion.All(
                result.Kind.TestEquals( DateTimeKind.Utc ),
                result.TestInRange( expectedMin, expectedMax ) )
            .Go();
    }

    [Fact]
    public void GetNow_ForLocal_ShouldReturnCorrectResult()
    {
        var expectedMin = DateTime.Now;
        var sut = new PreciseLocalDateTimeProvider( Duration.FromHours( 24 ) );

        var result = sut.GetNow();
        var expectedMax = DateTime.Now;

        Assertion.All(
                result.Kind.TestEquals( DateTimeKind.Local ),
                result.TestInRange( expectedMin, expectedMax ) )
            .Go();
    }

    [Fact]
    public async Task GetNow_ForUtc_ShouldReturnCorrectResult_WhenPrecisionResetTimeoutIsExceeded()
    {
        var sut = new PreciseUtcDateTimeProvider( Duration.FromTicks( 1 ) );

        await Task.Delay( TimeSpan.FromMilliseconds( 1 ) );

        var expectedMin = DateTime.UtcNow;
        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        Assertion.All(
                result.Kind.TestEquals( DateTimeKind.Utc ),
                result.TestInRange( expectedMin, expectedMax ) )
            .Go();
    }

    [Fact]
    public async Task GetNow_ForLocal_ShouldReturnCorrectResult_WhenPrecisionResetTimeoutIsExceeded()
    {
        var sut = new PreciseLocalDateTimeProvider( Duration.FromTicks( 1 ) );

        await Task.Delay( TimeSpan.FromMilliseconds( 1 ) );

        var expectedMin = DateTime.Now;
        var result = sut.GetNow();
        var expectedMax = DateTime.Now;

        Assertion.All(
                result.Kind.TestEquals( DateTimeKind.Local ),
                result.TestInRange( expectedMin, expectedMax ) )
            .Go();
    }
}
