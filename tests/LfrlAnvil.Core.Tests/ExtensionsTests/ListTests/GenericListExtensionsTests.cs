﻿using System.Collections.Generic;
using FluentAssertions;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.ListTests
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