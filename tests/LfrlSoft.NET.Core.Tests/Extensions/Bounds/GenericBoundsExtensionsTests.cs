using System;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Bounds
{
    public abstract class GenericBoundsExtensionsTests<T> : TestsBase
        where T : IComparable<T>
    {
        [Fact]
        public void AsEnumerable_ShouldReturnCorrectResult()
        {
            var (min, max) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            var sut = Core.Bounds.Create( min, max );

            var result = sut.AsEnumerable();

            result.Should().ContainInOrder( min, max );
        }
    }
}
