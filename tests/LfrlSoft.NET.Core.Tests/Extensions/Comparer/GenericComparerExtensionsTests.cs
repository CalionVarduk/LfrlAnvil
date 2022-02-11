using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Comparer
{
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
                result.Compare( lo, hi ).Should().BeGreaterThan( 0 );
                result.Compare( hi, lo ).Should().BeLessThan( 0 );
                result.Compare( lo, lo ).Should().Be( 0 );
            }
        }
    }
}
