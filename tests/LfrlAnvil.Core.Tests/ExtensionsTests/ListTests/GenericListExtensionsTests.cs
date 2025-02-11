using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Tests.ExtensionsTests.ListTests;

[GenericTestClass( typeof( GenericListExtensionsTestsData<> ) )]
public abstract class GenericListExtensionsTests<T> : TestsBase
{
    [Theory]
    [GenericMethodData( nameof( GenericListExtensionsTestsData<T>.CreateSwapItemsTestData ) )]
    public void SwapItems_ShouldSwapTwoItemsCorrectly(IList<T> source, int index1, int index2, IReadOnlyList<T> expected)
    {
        source.SwapItems( index1, index2 );
        source.TestSequence( expected ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 5 )]
    public void RemoveLast_ShouldRemoveLastItem(int count)
    {
        IList<T> sut = Fixture.CreateManyDistinct<T>( count ).ToList();
        var expected = sut.Take( count - 1 );

        sut.RemoveLast();

        sut.TestSequence( expected ).Go();
    }
}
