using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Collections.Tests.MultiHashSetTests
{
    public abstract class GenericMultiHashSetTests<T> : TestsBase
        where T : notnull
    {
        [Fact]
        public void Ctor_ShouldCreateEmptySet()
        {
            var sut = new MultiHashSet<T>();

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
            var sut = new MultiHashSet<T>( comparer );
            sut.Comparer.Should().Be( comparer );
        }

        [Fact]
        public void Add_ShouldAddNewItemWithMultiplicityEqualToOne()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();

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

            var sut = new MultiHashSet<T> { other };

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

            var sut = new MultiHashSet<T> { item };

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

            var sut = new MultiHashSet<T> { other, item };

            var result = sut.Add( item );

            using ( new AssertionScope() )
            {
                result.Should().Be( 2 );
                sut.FullCount.Should().Be( 3 );
                sut.Count.Should().Be( 2 );
            }
        }

        [Fact]
        public void Add_ShouldThrowOverflowException_WhenItemMultiplicityIsTooLarge()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();
            sut.AddMany( item, int.MaxValue );

            var action = Lambda.Of( () => sut.Add( item ) );

            using ( new AssertionScope() )
            {
                action.Should().ThrowExactly<OverflowException>();
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

            var sut = new MultiHashSet<T>();

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
        public void AddMany_ShouldAddNewItemWithMultiplicityEqualToCount_WhenOtherItemExists(int count)
        {
            var (other, item) = Fixture.CreateDistinctCollection<T>( 2 );

            var sut = new MultiHashSet<T> { other };

            var result = sut.AddMany( item, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( count );
                sut.FullCount.Should().Be( 1 + count );
                sut.Count.Should().Be( 2 );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void AddMany_ShouldIncreaseMultiplicityOfExistingItemByCount(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T> { item };

            var result = sut.AddMany( item, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( 1 + count );
                sut.FullCount.Should().Be( 1 + count );
                sut.Count.Should().Be( 1 );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void AddMany_ShouldIncreaseMultiplicityOfExistingItemByCount_WhenOtherItemExists(int count)
        {
            var (other, item) = Fixture.CreateDistinctCollection<T>( 2 );

            var sut = new MultiHashSet<T> { other, item };

            var result = sut.AddMany( item, count );

            using ( new AssertionScope() )
            {
                result.Should().Be( 1 + count );
                sut.FullCount.Should().Be( 2 + count );
                sut.Count.Should().Be( 2 );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( -1 )]
        public void AddMany_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();

            var action = Lambda.Of( () => sut.AddMany( item, count ) );

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void AddMany_ShouldThrowOverflowException_WhenItemMultiplicityIsTooLarge()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();
            sut.AddMany( item, int.MaxValue );

            var action = Lambda.Of( () => sut.AddMany( item, 1 ) );

            action.Should().ThrowExactly<OverflowException>();
        }

        [Fact]
        public void Remove_ShouldReturnMinusOneWhenItemDoesntExist()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();

            var result = sut.Remove( item );

            result.Should().Be( -1 );
        }

        [Fact]
        public void Remove_ShouldDecreaseMultiplicityOfExistingItemByOne()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();
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

            var sut = new MultiHashSet<T> { item };

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

            var sut = new MultiHashSet<T>();

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

            var sut = new MultiHashSet<T>();
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

            var sut = new MultiHashSet<T>();
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
        public void RemoveMany_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanOne(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();

            var action = Lambda.Of( () => sut.RemoveMany( item, count ) );

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void RemoveAll_ShouldRemoveAnItemAndReturnItsOldMultiplicity(int count)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();
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

            var sut = new MultiHashSet<T>();

            var result = sut.RemoveAll( item );

            result.Should().Be( 0 );
        }

        [Fact]
        public void Clear_ShouldRemoveAllItems()
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );

            var sut = new MultiHashSet<T>();

            foreach ( var item in items )
                sut.Add( item );

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

            var sut = new MultiHashSet<T> { item };

            var result = sut.Contains( item );

            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_ShouldReturnFalseWhenItemDoesntExist()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();

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

            var sut = new MultiHashSet<T>();
            sut.AddMany( item, count );

            var result = sut.GetMultiplicity( item );

            result.Should().Be( count );
        }

        [Fact]
        public void GetMultiplicity_ShouldReturnZeroWhenItemDoesntExist()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();

            var result = sut.GetMultiplicity( item );

            result.Should().Be( 0 );
        }

        [Fact]
        public void SetMultiplicity_ShouldReturnZeroAndDoNothing_WhenItemDoesntExistAndValueIsZero()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();

            var result = sut.SetMultiplicity( item, 0 );

            using ( new AssertionScope() )
            {
                result.Should().Be( 0 );
                sut.FullCount.Should().Be( 0 );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void SetMultiplicity_ShouldReturnZeroAndAddNewItem_WhenItemDoesntExist(int value)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();

            var result = sut.SetMultiplicity( item, value );

            using ( new AssertionScope() )
            {
                result.Should().Be( 0 );
                sut.Count.Should().Be( 1 );
                sut.FullCount.Should().Be( value );
                sut.GetMultiplicity( item ).Should().Be( value );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void SetMultiplicity_ShouldReturnMultiplicityOfExistingItemAndDoNothing_WhenNewMultiplicityIsTheSame(int value)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();
            sut.AddMany( item, value );

            var result = sut.SetMultiplicity( item, value );

            using ( new AssertionScope() )
            {
                result.Should().Be( value );
                sut.Count.Should().Be( 1 );
                sut.FullCount.Should().Be( value );
                sut.GetMultiplicity( item ).Should().Be( value );
            }
        }

        [Theory]
        [InlineData( 2, 3 )]
        [InlineData( 2, 4 )]
        [InlineData( 2, 5 )]
        [InlineData( 6, 3 )]
        [InlineData( 6, 4 )]
        [InlineData( 6, 5 )]
        public void SetMultiplicity_ShouldReturnOldMultiplicityOfExistingItemAndUpdateMultiplicity(int oldMultiplicity, int newMultiplicity)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();
            sut.AddMany( item, oldMultiplicity );

            var result = sut.SetMultiplicity( item, newMultiplicity );

            using ( new AssertionScope() )
            {
                result.Should().Be( oldMultiplicity );
                sut.Count.Should().Be( 1 );
                sut.FullCount.Should().Be( newMultiplicity );
                sut.GetMultiplicity( item ).Should().Be( newMultiplicity );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void SetMultiplicity_ShouldReturnMultiplicityOfExistingItemRemoveIt_WhenNewMultiplicityIsZero(int oldMultiplicity)
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();
            sut.AddMany( item, oldMultiplicity );

            var result = sut.SetMultiplicity( item, 0 );

            using ( new AssertionScope() )
            {
                result.Should().Be( oldMultiplicity );
                sut.Count.Should().Be( 0 );
                sut.FullCount.Should().Be( 0 );
                sut.GetMultiplicity( item ).Should().Be( 0 );
            }
        }

        [Fact]
        public void DistinctItems_ShouldReturnCorrectResult()
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );

            var sut = new MultiHashSet<T>();

            for ( var i = 0; i < items.Count; ++i )
                sut.AddMany( items[i], i + 1 );

            sut.DistinctItems.Should().BeEquivalentTo( items );
        }

        [Fact]
        public void Items_ShouldReturnCorrectResult()
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );

            var expected = new[]
            {
                items[0],
                items[1],
                items[1],
                items[2],
                items[2],
                items[2]
            };

            var sut = new MultiHashSet<T>();

            for ( var i = 0; i < items.Count; ++i )
                sut.AddMany( items[i], i + 1 );

            sut.Items.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectResult()
        {
            var items = Fixture.CreateDistinctCollection<T>( 3 );
            var expected = new[]
                {
                    Pair.Create( items[0], 1 ),
                    Pair.Create( items[1], 2 ),
                    Pair.Create( items[2], 3 )
                }
                .AsEnumerable();

            var sut = new MultiHashSet<T>();

            for ( var i = 0; i < items.Count; ++i )
                sut.AddMany( items[i], i + 1 );

            sut.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void ICollectionAdd_ShouldAddItemCorrectly()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T> { item };
            ICollection<Pair<T, int>> collection = sut;

            collection.Add( Pair.Create( item, 3 ) );

            using ( new AssertionScope() )
            {
                sut.FullCount.Should().Be( 4 );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void ICollectionRemove_ShouldRemoveItemCorrectly()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();
            sut.AddMany( item, 4 );
            ICollection<Pair<T, int>> collection = sut;

            var result = collection.Remove( Pair.Create( item, 3 ) );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.FullCount.Should().Be( 1 );
                sut.Count.Should().Be( 1 );
            }
        }

        [Fact]
        public void ICollectionRemove_ShouldReturnFalse_WhenItemDoesntExist()
        {
            var item = Fixture.Create<T>();

            var sut = new MultiHashSet<T>();
            ICollection<Pair<T, int>> collection = sut;

            var result = collection.Remove( Pair.Create( item, 1 ) );

            result.Should().BeFalse();
        }

        [Fact]
        public void ICollectionContains_ShouldReturnTrue_WhenItemExistsWithExactMultiplicity()
        {
            var item = Fixture.Create<T>();
            var multiplicity = 3;

            var sut = new MultiHashSet<T>();
            sut.AddMany( item, multiplicity );
            ICollection<Pair<T, int>> collection = sut;

            var result = collection.Contains( Pair.Create( item, multiplicity ) );

            result.Should().BeTrue();
        }

        [Fact]
        public void ICollectionContains_ShouldReturnFalse_WhenItemDoesntExistWithExactMultiplicity()
        {
            var item = Fixture.Create<T>();
            var multiplicity = 3;

            var sut = new MultiHashSet<T>();
            sut.AddMany( item, multiplicity );
            ICollection<Pair<T, int>> collection = sut;

            var result = collection.Contains( Pair.Create( item, multiplicity + 1 ) );

            result.Should().BeFalse();
        }
    }
}
