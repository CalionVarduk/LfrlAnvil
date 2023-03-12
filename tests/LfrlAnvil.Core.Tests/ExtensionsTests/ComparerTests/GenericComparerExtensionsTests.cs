using System.Collections.Generic;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.ComparerTests;

public abstract class GenericComparerExtensionsTests<T> : TestsBase
    where T : IComparable<T>
{
    [Fact]
    public void Invert_ShouldReturnComparerThatReturnsNegatedBaseComparisonResult()
    {
        var (lo, hi) = Fixture.CreateDistinctSortedCollection<T>( 2 );
        var sut = Comparer<T>.Default;

        var result = sut.Invert();

        using ( new AssertionScope() )
        {
            result.Compare( lo, hi ).Should().Be( 1 );
            result.Compare( hi, lo ).Should().Be( -1 );
            result.Compare( lo, lo ).Should().Be( 0 );
        }
    }
}
