using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Collections.Tests.FiniteCacheTests
{
    public abstract class GenericFiniteCacheTests<TKey, TValue> : GenericDictionaryTestsBase<TKey, TValue>
        where TKey : notnull
    {
        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void Ctor_ShouldCreateEmptyWithCorrectCapacity(int capacity)
        {
            var sut = new FiniteCache<TKey, TValue>( capacity );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 0 );
                sut.Capacity.Should().Be( capacity );
                sut.Comparer.Should().Be( EqualityComparer<TKey>.Default );
                sut.Oldest.Should().BeNull();
                sut.Newest.Should().BeNull();
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 3 )]
        public void Ctor_ShouldCreateEmptyWithCorrectCapacity_WithExplicitComparer(int capacity)
        {
            var comparer = EqualityComparerFactory<TKey>.Create( (a, b) => a!.Equals( b ) );

            var sut = new FiniteCache<TKey, TValue>( capacity, comparer );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 0 );
                sut.Capacity.Should().Be( capacity );
                sut.Comparer.Should().Be( comparer );
                sut.Oldest.Should().BeNull();
                sut.Newest.Should().BeNull();
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( -1 )]
        public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCapacityIsLessThanOne(int capacity)
        {
            var action = Lambda.Of( () => new FiniteCache<TKey, TValue>( capacity ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Add_ShouldAddNewItemToEmptyCacheCorrectly()
        {
            var key = Fixture.Create<TKey>();
            var value = Fixture.Create<TValue>();

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );

            sut.Add( key, value );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 1 );
                sut[key].Should().Be( value );
                sut.Oldest.Should().Be( KeyValuePair.Create( key, value ) );
                sut.Newest.Should().Be( KeyValuePair.Create( key, value ) );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void Add_ShouldAddNextItemToCacheWithExistingItemsCorrectly_WithoutExceedingCapacity(int existingItemCount)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( existingItemCount + 1 );
            var values = Fixture.CreateDistinctCollection<TValue>( existingItemCount + 1 );

            var newKey = keys[^1];
            var newValue = values[^1];

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ).Take( existingItemCount ) )
                sut.Add( key, value );

            sut.Add( newKey, newValue );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( existingItemCount + 1 );
                sut[newKey].Should().Be( newValue );
                sut.Oldest.Should().BeEquivalentTo( KeyValuePair.Create( keys[0], values[0] ) );
                sut.Newest.Should().BeEquivalentTo( KeyValuePair.Create( newKey, newValue ) );
            }
        }

        [Theory]
        [InlineData( 4 )]
        [InlineData( 5 )]
        [InlineData( 6 )]
        [InlineData( 7 )]
        public void Add_ShouldAddNextItemToFullCacheAndRemoveTheOldestItem(int itemCount)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( itemCount );
            var values = Fixture.CreateDistinctCollection<TValue>( itemCount );

            var newKey = keys[^1];
            var newValue = values[^1];

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ).Take( itemCount - 1 ) )
                sut.Add( key, value );

            sut.Add( newKey, newValue );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( sut.Capacity );
                sut[newKey].Should().Be( newValue );
                sut.ContainsKey( keys[^4] ).Should().BeFalse();
                sut.Oldest.Should().BeEquivalentTo( KeyValuePair.Create( keys[^3], values[^3] ) );
                sut.Newest.Should().BeEquivalentTo( KeyValuePair.Create( newKey, newValue ) );
            }
        }

        [Fact]
        public void Add_ShouldThrowArgumentException_WhenKeyAlreadyExists()
        {
            var key = Fixture.Create<TKey>();
            var values = Fixture.CreateDistinctCollection<TValue>( 2 );
            var sut = new FiniteCache<TKey, TValue>( capacity: 3 ) { { key, values[0] } };

            var action = Lambda.Of( () => sut.Add( key, values[1] ) );

            action.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Remove_ShouldReturnFalse_WhenCacheIsEmpty()
        {
            var key = Fixture.Create<TKey>();
            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );

            var result = sut.Remove( key );

            result.Should().BeFalse();
        }

        [Fact]
        public void Remove_ShouldReturnTrueAndRemoveExistingItem_WhenCacheHasOneItem()
        {
            var key = Fixture.Create<TKey>();
            var value = Fixture.Create<TValue>();
            var sut = new FiniteCache<TKey, TValue>( capacity: 3 ) { { key, value } };

            var result = sut.Remove( key );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                sut.Oldest.Should().BeNull();
                sut.Newest.Should().BeNull();
            }
        }

        [Fact]
        public void Remove_ShouldReturnTrueAndRemoveCorrectExistingItem()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var values = Fixture.CreateDistinctCollection<TValue>( 2 );
            var sut = new FiniteCache<TKey, TValue>( capacity: 3 ) { { keys[0], values[0] }, { keys[1], values[1] } };

            var result = sut.Remove( keys[0] );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.ContainsKey( keys[1] ).Should().BeTrue();
                sut.Oldest.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
                sut.Newest.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
            }
        }

        [Fact]
        public void Remove_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenCountEqualsCapacity()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var values = Fixture.CreateDistinctCollection<TValue>( 3 );
            var sut = new FiniteCache<TKey, TValue>( capacity: 3 )
            {
                { keys[0], values[0] },
                { keys[1], values[1] },
                { keys[2], values[2] }
            };

            var result = sut.Remove( keys[0] );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 2 );
                sut.ContainsKey( keys[1] ).Should().BeTrue();
                sut.ContainsKey( keys[2] ).Should().BeTrue();
                sut.Oldest.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
                sut.Newest.Should().Be( KeyValuePair.Create( keys[2], values[2] ) );
            }
        }

        [Fact]
        public void Remove_WithRemoved_ShouldReturnFalseAndDefaultRemoved_WhenCacheIsEmpty()
        {
            var key = Fixture.Create<TKey>();
            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );

            var result = sut.Remove( key, out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                removed.Should().Be( default( TValue ) );
            }
        }

        [Fact]
        public void Remove_WithRemoved_ShouldReturnTrueAndRemoveExistingItem_WhenCacheHasOneItem()
        {
            var key = Fixture.Create<TKey>();
            var value = Fixture.Create<TValue>();
            var sut = new FiniteCache<TKey, TValue>( capacity: 3 ) { { key, value } };

            var result = sut.Remove( key, out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                removed.Should().Be( value );
                sut.Oldest.Should().BeNull();
                sut.Newest.Should().BeNull();
            }
        }

        [Fact]
        public void Remove_WithRemoved_ShouldReturnTrueAndRemoveCorrectExistingItem()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var values = Fixture.CreateDistinctCollection<TValue>( 2 );
            var sut = new FiniteCache<TKey, TValue>( capacity: 3 ) { { keys[0], values[0] }, { keys[1], values[1] } };

            var result = sut.Remove( keys[0], out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.ContainsKey( keys[1] ).Should().BeTrue();
                removed.Should().Be( values[0] );
                sut.Oldest.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
                sut.Newest.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
            }
        }

        [Fact]
        public void Remove_WithRemoved_ShouldReturnTrueAndRemoveCorrectExistingItem_WhenCountEqualsCapacity()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var values = Fixture.CreateDistinctCollection<TValue>( 3 );
            var sut = new FiniteCache<TKey, TValue>( capacity: 3 )
            {
                { keys[0], values[0] },
                { keys[1], values[1] },
                { keys[2], values[2] }
            };

            var result = sut.Remove( keys[0], out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 2 );
                sut.ContainsKey( keys[1] ).Should().BeTrue();
                sut.ContainsKey( keys[2] ).Should().BeTrue();
                removed.Should().Be( values[0] );
                sut.Oldest.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
                sut.Newest.Should().Be( KeyValuePair.Create( keys[2], values[2] ) );
            }
        }

        [Fact]
        public void IndexerSet_ShouldAddNewItemToEmptyCacheCorrectly_WhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();
            var value = Fixture.Create<TValue>();

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );

            sut[key] = value;

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 1 );
                sut[key].Should().Be( value );
                sut.Oldest.Should().BeEquivalentTo( KeyValuePair.Create( key, value ) );
                sut.Newest.Should().BeEquivalentTo( KeyValuePair.Create( key, value ) );
            }
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void IndexerSet_ShouldAddNextItemToCacheWithExistingItemsCorrectly_WhenKeyDoesntExistAndWithoutExceedingCapacity(
            int existingItemCount)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( existingItemCount + 1 );
            var values = Fixture.CreateDistinctCollection<TValue>( existingItemCount + 1 );

            var newKey = keys[^1];
            var newValue = values[^1];

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ).Take( existingItemCount ) )
                sut.Add( key, value );

            sut[newKey] = newValue;

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( existingItemCount + 1 );
                sut[newKey].Should().Be( newValue );
                sut.Oldest.Should().BeEquivalentTo( KeyValuePair.Create( keys[0], values[0] ) );
                sut.Newest.Should().BeEquivalentTo( KeyValuePair.Create( newKey, newValue ) );
            }
        }

        [Theory]
        [InlineData( 4 )]
        [InlineData( 5 )]
        [InlineData( 6 )]
        [InlineData( 7 )]
        public void IndexerSet_ShouldAddNextItemToFullCacheAndRemoveTheOldestItem_WhenKeyDoesntExist(int itemCount)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( itemCount );
            var values = Fixture.CreateDistinctCollection<TValue>( itemCount );

            var newKey = keys[^1];
            var newValue = values[^1];

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ).Take( itemCount - 1 ) )
                sut.Add( key, value );

            sut[newKey] = newValue;

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( sut.Capacity );
                sut[newKey].Should().Be( newValue );
                sut.ContainsKey( keys[^4] ).Should().BeFalse();
                sut.Oldest.Should().BeEquivalentTo( KeyValuePair.Create( keys[^3], values[^3] ) );
                sut.Newest.Should().BeEquivalentTo( KeyValuePair.Create( newKey, newValue ) );
            }
        }

        [Fact]
        public void IndexerSet_ShouldUpdateValue_WhenKeyExists()
        {
            var key = Fixture.Create<TKey>();
            var values = Fixture.CreateDistinctCollection<TValue>( 2 );

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 ) { { key, values[0] } };

            sut[key] = values[1];

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 1 );
                sut[key].Should().Be( values[1] );
                sut.Oldest.Should().BeEquivalentTo( KeyValuePair.Create( key, values[1] ) );
                sut.Newest.Should().BeEquivalentTo( KeyValuePair.Create( key, values[1] ) );
            }
        }

        [Fact]
        public void TryGetValue_ShouldReturnTrueAndCorrectResult_WhenKeyExists()
        {
            var key = Fixture.Create<TKey>();
            var value = Fixture.Create<TValue>();

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 ) { { key, value } };

            var result = sut.TryGetValue( key, out var existingValue );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                existingValue.Should().Be( value );
            }
        }

        [Fact]
        public void TryGetValue_ShouldReturnFalseAndDefaultResult_WhenKeyDoesntExist()
        {
            var key = Fixture.Create<TKey>();

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );

            var result = sut.TryGetValue( key, out var existingValue );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                existingValue.Should().Be( default( TValue ) );
            }
        }

        [Fact]
        public void Clear_ShouldRemoveAllItems()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var values = Fixture.CreateDistinctCollection<TValue>( 3 );

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ) )
                sut.Add( key, value );

            sut.Clear();

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 0 );
                sut.Oldest.Should().BeNull();
                sut.Newest.Should().BeNull();
            }
        }

        [Fact]
        public void Keys_ShouldReturnCorrectResult_WhenCountIsLessThanCapacity()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var values = Fixture.CreateDistinctCollection<TValue>( 2 );

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ) )
                sut.Add( key, value );

            var result = sut.Keys;

            result.Should().BeSequentiallyEqualTo( keys[0], keys[1] );
        }

        [Fact]
        public void Keys_ShouldReturnCorrectResult_WhenCapacityHasBeenExceeded()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 6 );
            var values = Fixture.CreateDistinctCollection<TValue>( 6 );

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ) )
                sut.Add( key, value );

            var result = sut.Keys;

            result.Should().BeSequentiallyEqualTo( keys[^3], keys[^2], keys[^1] );
        }

        [Fact]
        public void Values_ShouldReturnCorrectResult_WhenCountIsLessThanCapacity()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var values = Fixture.CreateDistinctCollection<TValue>( 2 );

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ) )
                sut.Add( key, value );

            var result = sut.Values;

            result.Should().BeSequentiallyEqualTo( values[0], values[1] );
        }

        [Fact]
        public void Values_ShouldReturnCorrectResult_WhenCapacityHasBeenExceeded()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 6 );
            var values = Fixture.CreateDistinctCollection<TValue>( 6 );

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ) )
                sut.Add( key, value );

            var result = sut.Values;

            result.Should().BeSequentiallyEqualTo( values[^3], values[^2], values[^1] );
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectResult_WhenCountIsLessThanCapacity()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var values = Fixture.CreateDistinctCollection<TValue>( 2 );

            var expected = new[]
            {
                KeyValuePair.Create( keys[0], values[0] ),
                KeyValuePair.Create( keys[1], values[1] )
            };

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ) )
                sut.Add( key, value );

            sut.AsEnumerable().Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectResult_WhenCapacityHasBeenExceeded()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 6 );
            var values = Fixture.CreateDistinctCollection<TValue>( 6 );

            var expected = new[]
            {
                KeyValuePair.Create( keys[^3], values[^3] ),
                KeyValuePair.Create( keys[^2], values[^2] ),
                KeyValuePair.Create( keys[^1], values[^1] )
            };

            var sut = new FiniteCache<TKey, TValue>( capacity: 3 );
            foreach ( var (key, value) in keys.Zip( values ) )
                sut.Add( key, value );

            sut.AsEnumerable().Should().BeSequentiallyEqualTo( expected );
        }

        protected sealed override IDictionary<TKey, TValue> CreateEmptyDictionary()
        {
            return new FiniteCache<TKey, TValue>( capacity: 10 );
        }
    }
}
