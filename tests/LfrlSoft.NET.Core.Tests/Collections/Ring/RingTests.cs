using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Collections;
using LfrlSoft.NET.Core.Internal;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Collections.Ring
{
    public abstract class RingTests<T> : TestsBase
    {
        [Fact]
        public void Ctor_ShouldThrowWhenNoParametersHaveBeenPassed()
        {
            Action action = () =>
            {
                var _ = new Ring<T>();
            };

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( -1 )]
        public void Ctor_ShouldThrowWhenCountIsLessThanOne(int count)
        {
            Action action = () =>
            {
                var _ = new Ring<T>( count );
            };

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void Ctor_ShouldCreateWithCorrectCount(int count)
        {
            var sut = new Ring<T>( count );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( count );
                sut.StartIndex.Should().Be( 0 );
            }
        }

        [Fact]
        public void Ctor_ShouldCreateWithCorrectItems()
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );

            var sut = new Ring<T>( items[0], items[1], items[2] );

            sut.Should().BeEquivalentTo( items );
        }

        [Theory]
        [InlineData( 0, 0 )]
        [InlineData( 1, 1 )]
        [InlineData( 2, 2 )]
        [InlineData( 3, 0 )]
        [InlineData( 4, 1 )]
        [InlineData( -1, 2 )]
        [InlineData( -2, 1 )]
        [InlineData( -3, 0 )]
        public void StartIndexSet_ShouldUpdateStartIndexCorrectly(int startIndex, int expected)
        {
            var sut = new Ring<T>( 3 );

            sut.StartIndex = startIndex;
            var result = sut.StartIndex;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0, 0 )]
        [InlineData( 0, 1 )]
        [InlineData( 0, 2 )]
        [InlineData( 0, 3 )]
        [InlineData( 0, 4 )]
        [InlineData( 0, -1 )]
        [InlineData( 0, -2 )]
        [InlineData( 0, -3 )]
        [InlineData( 1, 0 )]
        [InlineData( 1, 1 )]
        [InlineData( 1, 2 )]
        [InlineData( 1, 3 )]
        [InlineData( 1, 4 )]
        [InlineData( 1, -1 )]
        [InlineData( 1, -2 )]
        [InlineData( 1, -3 )]
        [InlineData( 2, 0 )]
        [InlineData( 2, 1 )]
        [InlineData( 2, 2 )]
        [InlineData( 2, 3 )]
        [InlineData( 2, 4 )]
        [InlineData( 2, -1 )]
        [InlineData( 2, -2 )]
        [InlineData( 2, -3 )]
        public void IndexerSet_ShouldChangeCorrectItem(int startIndex, int setIndex)
        {
            var item = Fixture.Create<T>();

            var sut = new Ring<T>( 3 ) { StartIndex = startIndex };

            sut[setIndex] = item;
            var result = sut[setIndex];

            result.Should().Be( item );
        }

        [Theory]
        [InlineData( 0, 0, 0 )]
        [InlineData( 1, 0, 1 )]
        [InlineData( 2, 0, 2 )]
        [InlineData( 0, 1, 1 )]
        [InlineData( 1, 1, 2 )]
        [InlineData( 2, 1, 0 )]
        [InlineData( 0, 2, 2 )]
        [InlineData( 1, 2, 0 )]
        [InlineData( 2, 2, 1 )]
        [InlineData( 0, 3, 0 )]
        [InlineData( 1, 3, 1 )]
        [InlineData( 2, 3, 2 )]
        [InlineData( 0, 4, 1 )]
        [InlineData( 1, 4, 2 )]
        [InlineData( 2, 4, 0 )]
        [InlineData( 0, -1, 2 )]
        [InlineData( 1, -1, 0 )]
        [InlineData( 2, -1, 1 )]
        [InlineData( 0, -2, 1 )]
        [InlineData( 1, -2, 2 )]
        [InlineData( 2, -2, 0 )]
        [InlineData( 0, -3, 0 )]
        [InlineData( 1, -3, 1 )]
        [InlineData( 2, -3, 2 )]
        public void GetUnderlyingIndex_ShouldReturnCorrectResult(int startIndex, int index, int expected)
        {
            var sut = new Ring<T>( 3 ) { StartIndex = startIndex };

            var result = sut.GetUnderlyingIndex( index );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0, 1 )]
        [InlineData( 1, 2 )]
        [InlineData( 2, 0 )]
        public void SetNext_ShouldChangeItemAtStartIndexAndIncrementStartIndex(int startIndex, int expectedStartIndex)
        {
            var item = Fixture.Create<T>();

            var sut = new Ring<T>( 3 ) { StartIndex = startIndex };

            sut.SetNext( item );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 3 );
                sut.StartIndex.Should().Be( expectedStartIndex );
                sut[-1].Should().Be( item );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void Clear_ShouldResetItemsAndStartIndex(int startIndex)
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );

            var sut = new Ring<T>( items ) { StartIndex = startIndex };

            sut.Clear();

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 3 );
                sut.StartIndex.Should().Be( 0 );
                sut.Should().OnlyContain( i => Generic<T>.AreEqual( i, default ) );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void GetEnumerator_ShouldReturnCorrectResult(int startIndex)
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );
            var expected = new[]
            {
                items[(0 + startIndex) % 3],
                items[(1 + startIndex) % 3],
                items[(2 + startIndex) % 3]
            }.AsEnumerable();

            var sut = new Ring<T>( items ) { StartIndex = startIndex };

            sut.Should().BeEquivalentTo( expected );
        }
    }
}
