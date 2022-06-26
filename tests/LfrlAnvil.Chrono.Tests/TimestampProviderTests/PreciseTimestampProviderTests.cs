using System;
using System.Threading.Tasks;
using FluentAssertions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.TimestampProviderTests;

public class PreciseTimestampProviderTests : TestsBase
{
    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    [InlineData( -2 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMaxIdleTimeInTicksIsLessThanOne(long value)
    {
        var action = Lambda.Of( () => new PreciseTimestampProvider( value ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 100 )]
    [InlineData( 12345 )]
    public void Ctor_ShouldReturnCorrectResult(long value)
    {
        var sut = new PreciseTimestampProvider( value );
        sut.MaxIdleTimeInTicks.Should().Be( value );
    }

    [Fact]
    public void DefaultCtor_ShouldReturnCorrectResult()
    {
        var sut = new PreciseTimestampProvider();
        sut.MaxIdleTimeInTicks.Should().Be( ChronoConstants.TicksPerSecond );
    }

    [Fact]
    public void GetNow_ShouldReturnCorrectResult()
    {
        var expectedMin = DateTime.UtcNow;
        var sut = new PreciseTimestampProvider( ChronoConstants.TicksPerDay );

        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        result.UtcValue.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
    }

    [Fact]
    public void GetNow_ShouldReturnCorrectResult_WhenMaxIdleTimeInTicksIsExceeded()
    {
        var sut = new PreciseTimestampProvider( maxIdleTimeInTicks: 1 );

        Task.Delay( TimeSpan.FromMilliseconds( 1 ) ).Wait();

        var expectedMin = DateTime.UtcNow;
        var result = sut.GetNow();
        var expectedMax = DateTime.UtcNow;

        result.UtcValue.Should().BeOnOrAfter( expectedMin ).And.BeOnOrBefore( expectedMax );
    }
}