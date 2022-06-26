using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using Xunit;

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
    public void Ctor_ForUtc_ShouldThrowArgumentOutOfRangeException_WhenMaxIdleTimeInTicksIsLessThanOne(long value)
    {
        var action = Lambda.Of( () => new PreciseUtcDateTimeProvider( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    [InlineData( -2 )]
    public void Ctor_ForLocal_ShouldThrowArgumentOutOfRangeException_WhenMaxIdleTimeInTicksIsLessThanOne(long value)
    {
        var action = Lambda.Of( () => new PreciseLocalDateTimeProvider( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 100 )]
    [InlineData( 12345 )]
    public void Ctor_ForUtc_ShouldReturnCorrectResult(long value)
    {
        var sut = new PreciseUtcDateTimeProvider( value );

        using ( new AssertionScope() )
        {
            sut.Kind.Should().Be( DateTimeKind.Utc );
            sut.MaxIdleTimeInTicks.Should().Be( value );
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 100 )]
    [InlineData( 12345 )]
    public void Ctor_ForLocal_ShouldReturnCorrectResult(long value)
    {
        var sut = new PreciseLocalDateTimeProvider( value );

        using ( new AssertionScope() )
        {
            sut.Kind.Should().Be( DateTimeKind.Local );
            sut.MaxIdleTimeInTicks.Should().Be( value );
        }
    }

    [Fact]
    public void DefaultCtor_ForUtc_ShouldReturnCorrectResult()
    {
        var sut = new PreciseUtcDateTimeProvider();

        using ( new AssertionScope() )
        {
            sut.Kind.Should().Be( DateTimeKind.Utc );
            sut.MaxIdleTimeInTicks.Should().Be( ChronoConstants.TicksPerSecond );
        }
    }

    [Fact]
    public void DefaultCtor_ForLocal_ShouldReturnCorrectResult()
    {
        var sut = new PreciseLocalDateTimeProvider();

        using ( new AssertionScope() )
        {
            sut.Kind.Should().Be( DateTimeKind.Local );
            sut.MaxIdleTimeInTicks.Should().Be( ChronoConstants.TicksPerSecond );
        }
    }

    [Fact]
    public void GetNow_ForUtc_ShouldReturnCorrectResult()
    {
        var expectedMin = DateTime.UtcNow;
        var sut = new PreciseUtcDateTimeProvider( ChronoConstants.TicksPerDay );

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
        var sut = new PreciseLocalDateTimeProvider( ChronoConstants.TicksPerDay );

        var result = sut.GetNow();
        var expectedMax = DateTime.Now;

        using ( new AssertionScope() )
        {
            result.Kind.Should().Be( DateTimeKind.Local );
            result.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
        }
    }

    [Fact]
    public void GetNow_ForUtc_ShouldReturnCorrectResult_WhenMaxIdleTimeInTicksIsExceeded()
    {
        var sut = new PreciseUtcDateTimeProvider( maxIdleTimeInTicks: 1 );

        Task.Delay( TimeSpan.FromMilliseconds( 1 ) ).Wait();

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
    public void GetNow_ForLocal_ShouldReturnCorrectResult_WhenMaxIdleTimeInTicksIsExceeded()
    {
        var sut = new PreciseLocalDateTimeProvider( maxIdleTimeInTicks: 1 );

        Task.Delay( TimeSpan.FromMilliseconds( 1 ) ).Wait();

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
