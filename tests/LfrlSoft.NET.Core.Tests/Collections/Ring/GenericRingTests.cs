using System;
using System.Collections;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Collections;
using LfrlSoft.NET.Core.Collections.Internal;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.Core.Internal;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Collections.Ring
{
    public abstract class GenericRingTests<T> : TestsBase
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
                sut.WriteIndex.Should().Be( 0 );
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
        public void WriteIndexSet_ShouldUpdateWriteIndexCorrectly(int writeIndex, int expected)
        {
            var sut = new Ring<T>( count: 3 );

            sut.WriteIndex = writeIndex;
            var result = sut.WriteIndex;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( -1 )]
        [InlineData( 3 )]
        public void IndexerGet_ShouldThrow_WhenIndexIsOutOfBounds(int index)
        {
            var sut = new Ring<T>( count: 3 );

            Action action = () =>
            {
                var _ = sut[index];
            };

            action.Should().Throw<IndexOutOfRangeException>();
        }

        [Theory]
        [InlineData( -1 )]
        [InlineData( 3 )]
        public void IndexerSet_ShouldThrow_WhenIndexIsOutOfBounds(int index)
        {
            var item = Fixture.Create<T>();

            var sut = new Ring<T>( count: 3 );

            Action action = () => sut[index] = item;

            action.Should().Throw<IndexOutOfRangeException>();
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void IndexerSet_ShouldChangeCorrectItem(int index)
        {
            var item = Fixture.Create<T>();

            var sut = new Ring<T>( count: 3 );

            sut[index] = item;
            var result = sut[index];

            result.Should().Be( item );
        }

        [Theory]
        [InlineData( -3, 0 )]
        [InlineData( -2, 1 )]
        [InlineData( -1, 2 )]
        [InlineData( 0, 0 )]
        [InlineData( 1, 1 )]
        [InlineData( 2, 2 )]
        [InlineData( 3, 0 )]
        [InlineData( 4, 1 )]
        [InlineData( 5, 2 )]
        public void GetWrappedIndex_ShouldReturnCorrectResult(int index, int expected)
        {
            var sut = new Ring<T>( count: 3 );

            var result = sut.GetWrappedIndex( index );

            result.Should().Be( expected );
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
        public void GetWriteIndex_ShouldReturnCorrectResult(int writeIndex, int offset, int expected)
        {
            var sut = new Ring<T>( count: 3 ) { WriteIndex = writeIndex };

            var result = sut.GetWriteIndex( offset );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0, 1 )]
        [InlineData( 1, 2 )]
        [InlineData( 2, 0 )]
        public void SetNext_ShouldChangeItemAtWriteIndexAndIncrementWriteIndex(int writeIndex, int expectedWriteIndex)
        {
            var item = Fixture.Create<T>();

            var sut = new Ring<T>( count: 3 ) { WriteIndex = writeIndex };

            sut.SetNext( item );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 3 );
                sut.WriteIndex.Should().Be( expectedWriteIndex );
                sut[writeIndex].Should().Be( item );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void Clear_ShouldResetItemsAndWriteIndex(int writeIndex)
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );

            var sut = new Ring<T>( items ) { WriteIndex = writeIndex };

            sut.Clear();

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 3 );
                sut.WriteIndex.Should().Be( 0 );
                sut.Should().OnlyContain( i => Generic<T>.AreEqual( i, default ) );
            }
        }

        [Theory]
        [InlineData( -3 )]
        [InlineData( -2 )]
        [InlineData( -1 )]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        [InlineData( 4 )]
        [InlineData( 5 )]
        [InlineData( 6 )]
        public void Read_ShouldReturnCorrectResult(int readIndex)
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );
            var expected = new[]
            {
                items[(0 + readIndex).EuclidModulo( 3 )],
                items[(1 + readIndex).EuclidModulo( 3 )],
                items[(2 + readIndex).EuclidModulo( 3 )]
            }.AsEnumerable();

            var sut = new Ring<T>( items );
            var result = sut.Read( readIndex );

            result.Should().ContainInOrder( expected );
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void GetEnumerator_ShouldReturnCorrectResult(int writeIndex)
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );
            var expected = new[]
            {
                items[(1 + writeIndex) % 3],
                items[(2 + writeIndex) % 3],
                items[(0 + writeIndex) % 3]
            }.AsEnumerable();

            var sut = new Ring<T>( items ) { WriteIndex = writeIndex };

            sut.Should().ContainInOrder( expected );
        }

        [Theory]
        [InlineData( 0, 0 )]
        [InlineData( 1, 0 )]
        [InlineData( 2, 0 )]
        [InlineData( 0, 1 )]
        [InlineData( 1, 1 )]
        [InlineData( 2, 1 )]
        [InlineData( 0, 2 )]
        [InlineData( 1, 2 )]
        [InlineData( 2, 2 )]
        [InlineData( 0, 3 )]
        [InlineData( 1, 3 )]
        [InlineData( 2, 3 )]
        public void RingEnumeratorReset_ShouldResetEnumeratorCorrectly(int startIndex, int iterationCount)
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 ).ToArray();

            using var sut = new RingEnumerator<T>( items, startIndex );
            IEnumerator enumerator = sut;

            for ( var i = 0; i < iterationCount; ++i )
                sut.MoveNext();

            enumerator.Reset();
            sut.MoveNext();

            var firstItemAfterReset = enumerator.Current;

            var availableSteps = 1;
            while ( enumerator.MoveNext() )
                ++availableSteps;

            using ( new AssertionScope() )
            {
                firstItemAfterReset.Should().Be( items[startIndex] );
                availableSteps.Should().Be( items.Length );
            }
        }
    }
}
