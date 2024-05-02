using System.Threading.Tasks;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Chrono.Tests.DateTimeProviderTests;

public class PreciseDateTimeProviderTests : TestsBase
{
    [Fact]
    public void Utc_ShouldReturnCorrectResult()
    {
        var sut = PreciseDateTimeProvider.Utc;
        sut.Kind.Should().Be( DateTimeKind.Utc );
    }

    [Fact]
    public void Local_ShouldReturnCorrectResult()
    {
        var sut = PreciseDateTimeProvider.Local;
        sut.Kind.Should().Be( DateTimeKind.Local );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    [InlineData( -2 )]
    public void Ctor_ForUtc_ShouldThrowArgumentOutOfRangeException_WhenPrecisionResetTimeoutIsInvalid(long value)
    {
        var action = Lambda.Of( () => new PreciseUtcDateTimeProvider( Duration.FromTicks( value ) ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    [InlineData( -2 )]
    public void Ctor_ForLocal_ShouldThrowArgumentOutOfRangeException_WhenPrecisionResetTimeoutIsInvalid(long value)
    {
        var action = Lambda.Of( () => new PreciseLocalDateTimeProvider( Duration.FromTicks( value ) ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 100 )]
    [InlineData( 12345 )]
    public void Ctor_ForUtc_ShouldReturnCorrectResult(long value)
    {
        var sut = new PreciseUtcDateTimeProvider( Duration.FromTicks( value ) );

        using ( new AssertionScope() )
        {
            sut.Kind.Should().Be( DateTimeKind.Utc );
            sut.PrecisionResetTimeout.Should().Be( Duration.FromTicks( value ) );
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 100 )]
    [InlineData( 12345 )]
    public void Ctor_ForLocal_ShouldReturnCorrectResult(long value)
    {
        var sut = new PreciseLocalDateTimeProvider( Duration.FromTicks( value ) );

        using ( new AssertionScope() )
        {
            sut.Kind.Should().Be( DateTimeKind.Local );
            sut.PrecisionResetTimeout.Should().Be( Duration.FromTicks( value ) );
        }
    }

    [Fact]
    public void DefaultCtor_ForUtc_ShouldReturnCorrectResult()
    {
        var sut = new PreciseUtcDateTimeProvider();

        using ( new AssertionScope() )
        {
            sut.Kind.Should().Be( DateTimeKind.Utc );
            sut.PrecisionResetTimeout.Should().Be( Duration.FromMinutes( 1 ) );
        }
    }

    [Fact]
    public void DefaultCtor_ForLocal_ShouldReturnCorrectResult()
    {
        var sut = new PreciseLocalDateTimeProvider();

        using ( new AssertionScope() )
        {
            sut.Kind.Should().Be( DateTimeKind.Local );
            sut.PrecisionResetTimeout.Should().Be( Duration.FromMinutes( 1 ) );
        }
    }

    [Fact]
    public void GetNow_ForUtc_ShouldReturnCorrectResult()
    {
        var expectedMin = DateTime.UtcNow;
        var sut = new PreciseUtcDateTimeProvider( Duration.FromHours( 24 ) );

        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        using ( new AssertionScope() )
        {
            result.Kind.Should().Be( DateTimeKind.Utc );
            result.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
        }
    }

    [Fact]
    public void GetNow_ForLocal_ShouldReturnCorrectResult()
    {
        var expectedMin = DateTime.Now;
        var sut = new PreciseLocalDateTimeProvider( Duration.FromHours( 24 ) );

        var result = sut.GetNow();
        var expectedMax = DateTime.Now;

        using ( new AssertionScope() )
        {
            result.Kind.Should().Be( DateTimeKind.Local );
            result.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
        }
    }

    [Fact]
    public async Task GetNow_ForUtc_ShouldReturnCorrectResult_WhenPrecisionResetTimeoutIsExceeded()
    {
        var sut = new PreciseUtcDateTimeProvider( Duration.FromTicks( 1 ) );

        await Task.Delay( TimeSpan.FromMilliseconds( 1 ) );

        var expectedMin = DateTime.UtcNow;
        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        using ( new AssertionScope() )
        {
            result.Kind.Should().Be( DateTimeKind.Utc );
            result.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
        }
    }

    [Fact]
    public async Task GetNow_ForLocal_ShouldReturnCorrectResult_WhenPrecisionResetTimeoutIsExceeded()
    {
        var sut = new PreciseLocalDateTimeProvider( Duration.FromTicks( 1 ) );

        await Task.Delay( TimeSpan.FromMilliseconds( 1 ) );

        var expectedMin = DateTime.Now;
        var result = sut.GetNow();
        var expectedMax = DateTime.Now;

        using ( new AssertionScope() )
        {
            result.Kind.Should().Be( DateTimeKind.Local );
            result.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
        }
    }
}
