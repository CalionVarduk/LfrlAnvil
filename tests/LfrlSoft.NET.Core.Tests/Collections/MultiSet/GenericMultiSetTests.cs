using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Collections;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Collections.MultiSet
{
    public abstract class GenericMultiSetTests<T> : TestsBase
        where T : notnull
    {
        [Fact]
        public void Ctor_ShouldCreateEmptySet()
        {
            var sut = new MultiSet<T>();

            using ( new AssertionScope() )
            {
                sut.FullCount.Should().Be( 0 );
                sut.Comparer.Should().Be( EqualityComparer<T>.Default );
            }
        }

        [Fact]
        public void Ctor_ShouldCreateWithExplicitComparer()
        {
            var comparer = EqualityComparerFactory<T>.Create( (a, b) => a!.Equals( b ) );

            var sut = new MultiSet<T>( comparer );

            sut.Comparer.Should().Be( comparer );
        }

        [Fact]
        public void Add_ShouldAddNewItemWithMultiplicityEqualToOne()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();

            var result = sut.Add( item );

            using ( new AssertionScope() )
            {
                result.Should().Be( 1 );
                sut.FullCount.Should().Be( 1 );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Add_ShouldAddNewItemWithMultiplicityEqualToOne_WhenOtherItemExists()
        {
            var (other, item) = Fixture.CreateDistinctCollection<T>( 2 );

            var sut = new MultiSet<T> { other };

            var result = sut.Add( item );

            using ( new AssertionScope() )
            {
                result.Should().Be( 1 );
                sut.FullCount.Should().Be( 2 );
                sut.Count.Should().Be( 2 );
            }
        }

        [Fact]
        public void Add_ShouldIncreaseMultiplicityOfExistingItemByOne()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T> { item };

            var result = sut.Add( item );

            using ( new AssertionScope() )
            {
                result.Should().Be( 2 );
                sut.FullCount.Should().Be( 2 );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Add_ShouldIncreaseMultiplicityOfExistingItemByOne_WhenOtherItemExists()
        {
            var (other, item) = Fixture.CreateDistinctCollection<T>( 2 );

            var sut = new MultiSet<T> { other, item };

            var result = sut.Add( item );

            using ( new AssertionScope() )
            {
                result.Should().Be( 2 );
                sut.FullCount.Should().Be( 3 );
                sut.Count.Should().Be( 2 );
            }
        }

        [Fact]
        public void Add_ShouldThrowWhenItemMultiplicityIsTooLarge()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();
            sut.AddMany( item, int.MaxValue );

            Action action = () => sut.Add( item );

            using ( new AssertionScope() )
            {
                action.Should().Throw<OverflowException>();
                sut.Count.Should().Be( 1 );
                sut.FullCount.Should().Be( int.MaxValue );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void AddMany_ShouldAddNewItemWithMultiplicityEqualToCount(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();

            var result = sut.AddMany( item, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( count );
                sut.FullCount.Should().Be( count );
                sut.Count.Should().Be( 1 );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void AddMany_ShouldIncreaseMultiplicityOfExistingItemByCount(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T> { item };

            var result = sut.AddMany( item, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( 1 + count );
                sut.FullCount.Should().Be( 1 + count );
                sut.Count.Should().Be( 1 );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( -1 )]
        public void AddMany_ShouldThrowWhenCountIsLessThanOne(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();

            Action action = () => sut.AddMany( item, count );

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void AddMany_ShouldThrowWhenItemMultiplicityIsTooLarge()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();
            sut.AddMany( item, int.MaxValue );

            Action action = () => sut.AddMany( item, 1 );

            action.Should().Throw<OverflowException>();
        }

        [Fact]
        public void Remove_ShouldReturnMinusOneWhenItemDoesntExist()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();

            var result = sut.Remove( item );

            result.Should().Be( -1 );
        }

        [Fact]
        public void Remove_ShouldDecreaseMultiplicityOfExistingItemByOne()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();
            sut.AddMany( item, 2 );

            var result = sut.Remove( item );

            using ( new AssertionScope() )
            {
                result.Should().Be( 1 );
                sut.FullCount.Should().Be( 1 );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void Remove_ShouldRemoveExistingItemWhenItsMultiplicityIsEqualToOne()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T> { item };

            var result = sut.Remove( item );

            using ( new AssertionScope() )
            {
                result.Should().Be( 0 );
                sut.FullCount.Should().Be( 0 );
                sut.Count.Should().Be( 0 );
            }
        }

        [Fact]
        public void RemoveMany_ShouldReturnMinusOneWhenItemDoesntExist()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();

            var result = sut.RemoveMany( item, 1 );

            result.Should().Be( -1 );
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void RemoveMany_ShouldDecreaseMultiplicityOfExistingItemByCount(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();
            sut.AddMany( item, 4 );

            var result = sut.RemoveMany( item, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( 4 - count );
                sut.FullCount.Should().Be( 4 - count );
                sut.Count.Should().Be( 1 );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void RemoveMany_ShouldRemoveExistingItemWhenItsMultiplicityIsLessThanOrEqualToRemoveCount(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();
            sut.AddMany( item, 1 );

            var result = sut.RemoveMany( item, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( 0 );
                sut.FullCount.Should().Be( 0 );
                sut.Count.Should().Be( 0 );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( -1 )]
        public void RemoveMany_ShouldThrowWhenCountIsLessThanOne(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();

            Action action = () => sut.RemoveMany( item, count );

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void RemoveAll_ShouldRemoveAnItemAndReturnItsOldMultiplicity(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();
            sut.AddMany( item, count );

            var result = sut.RemoveAll( item );

            using ( new AssertionScope() )
            {
                result.Should().Be( count );
                sut.FullCount.Should().Be( 0 );
                sut.Count.Should().Be( 0 );
            }
        }

        [Fact]
        public void RemoveAll_ShouldReturnZeroWhenItemDoesntExist()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();

            var result = sut.RemoveAll( item );

            result.Should().Be( 0 );
        }

        [Fact]
        public void Clear_ShouldRemoveAllItems()
        {
            var keys = Fixture.CreateDistinctCollection<T>( 3 );

            var sut = new MultiSet<T>();

            foreach ( var key in keys )
                sut.Add( key );

            sut.Clear();

            using ( new AssertionScope() )
            {
                sut.FullCount.Should().Be( 0 );
                sut.Count.Should().Be( 0 );
            }
        }

        [Fact]
        public void Contains_ShouldReturnTrueWhenItemExists()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T> { item };

            var result = sut.Contains( item );

            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_ShouldReturnFalseWhenItemDoesntExist()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();

            var result = sut.Contains( item );

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void GetMultiplicity_ShouldReturnCorrectResultWhenItemExists(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();
            sut.AddMany( item, count );

            var result = sut.GetMultiplicity( item );

            result.Should().Be( count );
        }

        [Fact]
        public void GetMultiplicity_ShouldReturnZeroWhenItemDoesntExist()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();

            var result = sut.GetMultiplicity( item );

            result.Should().Be( 0 );
        }

        [Fact]
        public void SetMultiplicity_ShouldReturnZeroAndDoNothing_WhenItemDoesntExistAndValueIsZero()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiSet<T>();

            var result = sut.SetMultiplicity( item, 0 );

            using ( new AssertionScope() )
            {
                result.Should().Be( 0 );
                sut.FullCount.Should().Be( 0 );
            }
        }

        [Fact]
        public void DistinctItems_ShouldReturnCorrectResult()
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );

            var sut = new MultiSet<T>();

            for ( var i = 0; i < items.Count; ++i )
                sut.AddMany( items[i], i + 1 );

            sut.DistinctItems.Should().BeEquivalentTo( items );
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectResult()
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );
            var expected = new[]
                {
                    Core.Pair.Create( items[0], 1 ),
                    Core.Pair.Create( items[1], 2 ),
                    Core.Pair.Create( items[2], 3 )
                }
                .AsEnumerable();

            var sut = new MultiSet<T>();

            for ( var i = 0; i < items.Count; ++i )
                sut.AddMany( items[i], i + 1 );

            sut.Should().BeEquivalentTo( expected );
        }
    }
}
