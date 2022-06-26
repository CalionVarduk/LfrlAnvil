using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Chrono.Tests.ExtensionsTests.TimestampProviderTests;

public class TimestampProviderExtensionsTests : TestsBase
{
    [Theory]
    [InlineData( 100, 99, true )]
    [InlineData( 100, 100, false )]
    [InlineData( 100, 101, false )]
    public void IsInPast_ShouldReturnCorrectResult(long providerTicks, long ticksToTest, bool expected)
    {
        var timestamp = new Timestamp( ticksToTest );
        var sut = GetMockedProvider( providerTicks );

        var result = sut.IsInPast( timestamp );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 100, 99, false )]
    [InlineData( 100, 100, true )]
    [InlineData( 100, 101, false )]
    public void IsNow_ShouldReturnCorrectResult(long providerTicks, long ticksToTest, bool expected)
    {
        var timestamp = new Timestamp( ticksToTest );
        var sut = GetMockedProvider( providerTicks );

        var result = sut.IsNow( timestamp );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 100, 99, false )]
    [InlineData( 100, 100, false )]
    [InlineData( 100, 101, true )]
    public void IsInFuture_ShouldReturnCorrectResult(long providerTicks, long ticksToTest, bool expected)
    {
        var timestamp = new Timestamp( ticksToTest );
        var sut = GetMockedProvider( providerTicks );

        var result = sut.IsInFuture( timestamp );

        result.Should().Be( expected );
    }

    [Fact]
    public void IsFrozen_ShouldReturnTrue_WhenProviderIsOfFrozenTimestampProviderType()
    {
        var sut = new FrozenTimestampProvider( default );
        var result = sut.IsFrozen();
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFrozen_ShouldReturnFalse_WhenProviderIsNotOfFrozenTimestampProviderType()
    {
        var sut = new TimestampProvider();
        var result = sut.IsFrozen();
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 100, 90, -10 )]
    [InlineData( 100, 100, 0 )]
    [InlineData( 100, 110, 10 )]
    public void GetDifference_ShouldReturnCorrectResult(long providerTicks, long ticksToTest, long expectedTicks)
    {
        var expected = new Duration( expectedTicks );
        var timestamp = new Timestamp( ticksToTest );
        var sut = GetMockedProvider( providerTicks );

        var result = sut.GetDifference( timestamp );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 100, 90, -10 )]
    [InlineData( 100, 100, 0 )]
    [InlineData( 100, 110, 10 )]
    public void GetDifference_WithOtherProvider_ShouldReturnCorrectResult(long providerTicks, long ticksToTest, long expectedTicks)
    {
        var expected = new Duration( expectedTicks );
        var other = GetMockedProvider( ticksToTest );
        var sut = GetMockedProvider( providerTicks );

        var result = sut.GetDifference( other );

        result.Should().Be( expected );
    }

    [Fact]
    public void Freeze_ShouldReturnCorrectResult()
    {
        var (first, second) = Fixture.CreateDistinctCollection<int>( count: 2 );
        var sut = GetMockedProvider( first, second );

        var frozen = sut.Freeze();
        var firstResult = frozen.GetNow();
        var secondResult = frozen.GetNow();

        using ( new AssertionScope() )
        {
            firstResult.UnixEpochTicks.Should().Be( first );
            secondResult.UnixEpochTicks.Should().Be( first );
        }
    }

    private static ITimestampProvider GetMockedProvider(long timestampTicks, params long[] additionalTimestampTicks)
    {
        var result = Substitute.For<ITimestampProvider>();
        result
            .GetNow()
            .Returns(
                new Timestamp( timestampTicks ),
                additionalTimestampTicks.Select( t => new Timestamp( t ) ).ToArray() );

        return result;
    }
}
