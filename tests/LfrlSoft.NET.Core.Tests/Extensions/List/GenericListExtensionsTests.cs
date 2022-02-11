using System.Collections.Generic;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.List
{
    [GenericTestClass( typeof( GenericListExtensionsTestsData<> ) )]
    public abstract class GenericListExtensionsTests<T> : TestsBase
    {
        [Theory]
        [GenericMethodData( nameof( GenericListExtensionsTestsData<T>.CreateSwapItemsTestData ) )]
        public void SwapItems_ShouldSwapTwoItemsCorrectly(IList<T> source, int index1, int index2, IReadOnlyList<T> expected)
        {
            source.SwapItems( index1, index2 );
            source.Should().BeEquivalentTo( expected );
        }
    }
}
