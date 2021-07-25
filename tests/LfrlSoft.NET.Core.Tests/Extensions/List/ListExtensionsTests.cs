using System.Collections.Generic;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.List
{
    [GenericTestClass( typeof( ListExtensionsTestsData<> ) )]
    public abstract class ListExtensionsTests<T> : TestsBase
    {
        [Theory]
        [GenericMethodData( nameof( ListExtensionsTestsData<T>.CreateSwapItemsTestData ) )]
        public void SwapItems_ShouldDoNothing_WhenIndexesAreEqual(IList<T> source, int index1, int index2, IReadOnlyList<T> expected)
        {
            source.SwapItems( index1, index2 );

            source.Should().BeEquivalentTo( expected );
        }
    }
}
