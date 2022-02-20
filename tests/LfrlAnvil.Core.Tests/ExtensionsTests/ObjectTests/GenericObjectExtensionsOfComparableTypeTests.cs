using System;
using FluentAssertions;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.ObjectTests
{
    public abstract class GenericObjectExtensionsOfComparableTypeTests<T> : GenericObjectExtensionsTests<T>
        where T : IComparable<T>
    {
        [Fact]
        public void Min_ShouldReturnSource_WhenBothValuesAreEqual()
        {
            var sut = Fixture.CreateNotDefault<T>();
            var result = sut.Min( sut );
            result.Should().Be( sut );
        }

        [Fact]
        public void Min_ShouldReturnSource_WhenSourceIsLesser()
        {
            var (sut, other) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            var result = sut.Min( other );
            result.Should().Be( sut );
        }

        [Fact]
        public void Min_ShouldReturnOther_WhenSourceIsGreater()
        {
            var (other, sut) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            var result = sut.Min( other );
            result.Should().Be( other );
        }

        [Fact]
        public void Max_ShouldReturnSource_WhenBothValuesAreEqual()
        {
            var sut = Fixture.CreateNotDefault<T>();
            var result = sut.Max( sut );
            result.Should().Be( sut );
        }

        [Fact]
        public void Max_ShouldReturnSource_WhenSourceIsGreater()
        {
            var (other, sut) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            var result = sut.Max( other );
            result.Should().Be( sut );
        }

        [Fact]
        public void Max_ShouldReturnOther_WhenSourceIsLesser()
        {
            var (sut, other) = Fixture.CreateDistinctSortedCollection<T>( 2 );
            var result = sut.Max( other );
            result.Should().Be( other );
        }
    }
}
