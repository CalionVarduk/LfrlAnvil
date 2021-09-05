using Xunit;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Extensions.Pair
{
    public class PairExtensionsOfInt32AndInt32Tests : GenericPairExtensionsTests<int, int>
    {
        [Fact]
        public void AsEnumerable_ShouldReturnCorrectResult_WhenNonNullable()
        {
            var (first, second) = Fixture.CreateDistinctCollection<int>( 2 );
            var sut = Core.Pair.Create( first, second );

            var result = sut.AsEnumerable();

            result.Should().ContainInOrder( first, second );
        }

        [Fact]
        public void AsEnumerable_ShouldReturnCorrectResult_WhenSecondIsNullableWithValue()
        {
            var (first, second) = Fixture.CreateDistinctCollection<int>( 2 );
            var sut = Core.Pair.Create( first, (int?) second );

            var result = sut.AsEnumerable();

            result.Should().ContainInOrder( first, second );
        }

        [Fact]
        public void AsEnumerable_ShouldReturnCorrectResult_WhenSecondIsNullableWithoutValue()
        {
            var first = Fixture.Create<int>();
            var sut = Core.Pair.Create( first, (int?) null );

            var result = sut.AsEnumerable();

            result.Should().ContainInOrder( first );
        }

        [Fact]
        public void AsEnumerable_ShouldReturnCorrectResult_WhenFirstIsNullableWithValue()
        {
            var (first, second) = Fixture.CreateDistinctCollection<int>( 2 );
            var sut = Core.Pair.Create( (int?) first, second );

            var result = sut.AsEnumerable();

            result.Should().ContainInOrder( first, second );
        }

        [Fact]
        public void AsEnumerable_ShouldReturnCorrectResult_WhenFirstIsNullableWithoutValue()
        {
            var second = Fixture.Create<int>();
            var sut = Core.Pair.Create( (int?) null, second );

            var result = sut.AsEnumerable();

            result.Should().ContainInOrder( second );
        }

        [Fact]
        public void AsEnumerable_ShouldReturnCorrectResult_WhenNullableWithBothValues()
        {
            var (first, second) = Fixture.CreateDistinctCollection<int>( 2 );
            var sut = Core.Pair.Create( (int?) first, (int?) second );

            var result = sut.AsEnumerable();

            result.Should().ContainInOrder( first, second );
        }

        [Fact]
        public void AsEnumerable_ShouldReturnCorrectResult_WhenNullableWithOnlyFirstValue()
        {
            var first = Fixture.Create<int>();
            var sut = Core.Pair.Create( (int?) first, (int?) null );

            var result = sut.AsEnumerable();

            result.Should().ContainInOrder( first );
        }

        [Fact]
        public void AsEnumerable_ShouldReturnCorrectResult_WhenNullableWithOnlySecondValue()
        {
            var second = Fixture.Create<int>();
            var sut = Core.Pair.Create( (int?) null, (int?) second );

            var result = sut.AsEnumerable();

            result.Should().ContainInOrder( second );
        }

        [Fact]
        public void AsEnumerable_ShouldReturnCorrectResult_WhenNullableWithNoValues()
        {
            var sut = Core.Pair.Create( (int?) null, (int?) null );

            var result = sut.AsEnumerable();

            result.Should().BeEmpty();
        }
    }
}
