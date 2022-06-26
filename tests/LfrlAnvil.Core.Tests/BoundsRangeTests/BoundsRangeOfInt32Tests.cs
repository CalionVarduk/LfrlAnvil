using FluentAssertions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Tests.BoundsRangeTests;

public class BoundsRangeOfInt32Tests : GenericBoundsRangeTests<int>
{
    [Fact]
    public void Normalize_ShouldReturnBoundsRangeWithLessItems_WhenNormalizePredicateReturnsTrueForAny()
    {
        var sut = new BoundsRange<int>(
            new[]
            {
                Bounds.Create( 0, 2 ),
                Bounds.Create( 4, 7 ),
                Bounds.Create( 8, 10 ),
                Bounds.Create( 13, 14 ),
                Bounds.Create( 15, 17 ),
                Bounds.Create( 18, 21 ),
                Bounds.Create( 24, 28 )
            } );

        var expected = new BoundsRange<int>(
            new[]
            {
                Bounds.Create( 0, 2 ),
                Bounds.Create( 4, 10 ),
                Bounds.Create( 13, 21 ),
                Bounds.Create( 24, 28 )
            } );

        var result = sut.Normalize( (a, b) => a + 1 == b );

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void Normalize_ShouldReturnTarget_WhenNormalizePredicateReturnsFalseForAll()
    {
        var sut = new BoundsRange<int>(
            new[]
            {
                Bounds.Create( 0, 2 ),
                Bounds.Create( 4, 7 ),
                Bounds.Create( 8, 10 ),
                Bounds.Create( 13, 14 ),
                Bounds.Create( 15, 17 ),
                Bounds.Create( 18, 21 ),
                Bounds.Create( 24, 28 )
            } );

        var result = sut.Normalize( (a, b) => a == b );

        result.Should().BeSequentiallyEqualTo( sut );
    }
}
