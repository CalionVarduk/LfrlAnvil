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

namespace LfrlAnvil.Collections.Tests.DictionaryHeapTests
{
    public abstract class GenericDictionaryHeapTests<TKey, TValue> : TestsBase
    {
        [Fact]
        public void Ctor_ShouldCreateEmptyHeap()
        {
            var sut = new DictionaryHeap<TKey, TValue>();

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 0 );
                sut.Comparer.Should().Be( Comparer<TValue>.Default );
                sut.KeyComparer.Should().Be( EqualityComparer<TKey>.Default );
            }
        }

        [Fact]
        public void Ctor_ShouldCreateEmptyHeap_WithExplicitComparer()
        {
            var keyComparer = EqualityComparerFactory<TKey>.Create( (a, b) => a!.GetHashCode() == b!.GetHashCode() );
            var comparer = Comparer<TValue>.Create( (a, b) => a!.GetHashCode().CompareTo( b!.GetHashCode() ) );
            var sut = new DictionaryHeap<TKey, TValue>( keyComparer, comparer );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 0 );
                sut.Comparer.Should().Be( comparer );
                sut.KeyComparer.Should().Be( keyComparer );
            }
        }

        [Fact]
        public void Ctor_ShouldCreateCorrectHeapWithDistinctItems()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var items = Fixture.CreateDistinctCollection<TValue>( 20 );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            using ( new AssertionScope() )
            {
                sut.Should().BeEquivalentTo( items );
                sut.Comparer.Should().Be( Comparer<TValue>.Default );
                sut.KeyComparer.Should().Be( EqualityComparer<TKey>.Default );
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void Ctor_ShouldCreateCorrectHeapWithRepeatingItems()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var distinctItems = Fixture.CreateDistinctCollection<TValue>( 5 );
            var items = distinctItems.SelectMany( i => new[] { i, i, i, i } ).ToList();

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            using ( new AssertionScope() )
            {
                sut.Should().BeEquivalentTo( items );
                sut.Comparer.Should().Be( Comparer<TValue>.Default );
                sut.KeyComparer.Should().Be( EqualityComparer<TKey>.Default );
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void Ctor_ShouldCreateCorrectHeapWithDistinctItems_WithExplicitComparer()
        {
            var keyComparer = EqualityComparerFactory<TKey>.Create( (a, b) => a is null ? b is null : a.Equals( b ) );
            var comparer = Comparer<TValue>.Create( (a, b) => a!.GetHashCode().CompareTo( b!.GetHashCode() ) );
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var items = Fixture.CreateDistinctCollection<TValue>( 20 );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ), keyComparer, comparer );

            using ( new AssertionScope() )
            {
                sut.Should().BeEquivalentTo( items );
                sut.Comparer.Should().Be( comparer );
                sut.KeyComparer.Should().Be( keyComparer );
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void Ctor_ShouldThrowArgumentException_WhenKeysAreNotDistinct()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 19 );
            keys = keys.Append( keys[0] ).ToList();
            var items = Fixture.CreateDistinctCollection<TValue>( 20 );

            var action = Lambda.Of( () => new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) ) );

            action.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Add_ShouldAddNewItemToEmptyHeapOnTop()
        {
            var key = Fixture.Create<TKey>();
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>();

            sut.Add( key, item );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 1 );
                sut[0].Should().Be( item );
                sut.GetKey( 0 ).Should().Be( key );
            }
        }

        [Fact]
        public void Add_ShouldAddNewItemOnTop_WhenNewItemIsLessThanExistingItem()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var (item, other) = Fixture.CreateDistinctSortedCollection<TValue>( 2 );

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], other ) } );

            sut.Add( keys[1], item );

            sut.Should().BeSequentiallyEqualTo( item, other );
        }

        [Fact]
        public void Add_ShouldAddNewItemAsLeftChild_WhenNewItemIsGreaterThanSoleExistingItem()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var (other, item) = Fixture.CreateDistinctSortedCollection<TValue>( 2 );

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], other ) } );

            sut.Add( keys[1], item );

            sut.Should().BeSequentiallyEqualTo( other, item );
        }

        [Fact]
        public void Add_ShouldAddNewItemAsRightChild_WhenNewItemIsGreaterThanExistingItem_AndLeftChildExists()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var (other, item, left) = Fixture.CreateDistinctSortedCollection<TValue>( 3 );

            var sut = new DictionaryHeap<TKey, TValue>(
                new[]
                {
                    KeyValuePair.Create( keys[0], other ),
                    KeyValuePair.Create( keys[1], left )
                } );

            sut.Add( keys[2], item );

            sut.Should().BeSequentiallyEqualTo( other, left, item );
        }

        [Fact]
        public void Add_ShouldThrowArgumentException_WhenKeyAlreadyExists()
        {
            var key = Fixture.Create<TKey>();
            var (item, other) = Fixture.CreateDistinctCollection<TValue>( 2 );
            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( key, other ) } );

            var action = Lambda.Of( () => sut.Add( key, item ) );

            action.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Add_ShouldSatisfyHeapInvariant()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var items = Fixture.CreateMany<TValue>( 20 ).ToList();

            var sut = new DictionaryHeap<TKey, TValue>();

            foreach ( var (key, item) in keys.Zip( items ) )
                sut.Add( key, item );

            using ( new AssertionScope() )
            {
                sut.Should().BeEquivalentTo( items );
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void TryAdd_ShouldAddNewItemToEmptyHeapOnTop()
        {
            var key = Fixture.Create<TKey>();
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>();

            var result = sut.TryAdd( key, item );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut[0].Should().Be( item );
                sut.GetKey( 0 ).Should().Be( key );
            }
        }

        [Fact]
        public void TryAdd_ShouldAddNewItemOnTop_WhenNewItemIsLessThanExistingItem()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var (item, other) = Fixture.CreateDistinctSortedCollection<TValue>( 2 );

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], other ) } );

            var result = sut.TryAdd( keys[1], item );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Should().BeSequentiallyEqualTo( item, other );
            }
        }

        [Fact]
        public void TryAdd_ShouldAddNewItemAsLeftChild_WhenNewItemIsGreaterThanSoleExistingItem()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var (other, item) = Fixture.CreateDistinctSortedCollection<TValue>( 2 );

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], other ) } );

            var result = sut.TryAdd( keys[1], item );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Should().BeSequentiallyEqualTo( other, item );
            }
        }

        [Fact]
        public void TryAdd_ShouldAddNewItemAsRightChild_WhenNewItemIsGreaterThanExistingItem_AndLeftChildExists()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var (other, item, left) = Fixture.CreateDistinctSortedCollection<TValue>( 3 );

            var sut = new DictionaryHeap<TKey, TValue>(
                new[]
                {
                    KeyValuePair.Create( keys[0], other ),
                    KeyValuePair.Create( keys[1], left )
                } );

            var result = sut.TryAdd( keys[2], item );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Should().BeSequentiallyEqualTo( other, left, item );
            }
        }

        [Fact]
        public void TryAdd_ReturnFalse_WhenKeyAlreadyExists()
        {
            var key = Fixture.Create<TKey>();
            var (item, other) = Fixture.CreateDistinctCollection<TValue>( 2 );
            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( key, other ) } );

            var result = sut.TryAdd( key, item );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Should().BeSequentiallyEqualTo( other );
            }
        }

        [Fact]
        public void TryAdd_ShouldSatisfyHeapInvariant()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var items = Fixture.CreateMany<TValue>( 20 ).ToList();

            var sut = new DictionaryHeap<TKey, TValue>();

            foreach ( var (key, item) in keys.Zip( items ) )
                sut.TryAdd( key, item );

            using ( new AssertionScope() )
            {
                sut.Should().BeEquivalentTo( items );
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void Peek_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
        {
            var sut = new DictionaryHeap<TKey, TValue>();
            var action = Lambda.Of( () => sut.Peek() );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void TryPeek_ShouldReturnFalse_WhenHeapIsEmpty()
        {
            var sut = new DictionaryHeap<TKey, TValue>();

            var result = sut.TryPeek( out var peeked );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                peeked.Should().Be( default( TValue ) );
            }
        }

        [Fact]
        public void TryPeek_ShouldReturnTrueAndReturnTopItem()
        {
            var key = Fixture.Create<TKey>();
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( key, item ) } );

            var result = sut.TryPeek( out var peeked );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                peeked.Should().Be( item );
            }
        }

        [Fact]
        public void TryPeek_ShouldReturnCorrectResultAndNotModifyHeap()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 10 );
            var items = Fixture.CreateDistinctSortedCollection<TValue>( 10 );
            var expected = items[0];

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );
            var heapifiedItems = sut.ToList();

            var result = sut.TryPeek( out var peeked );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                peeked.Should().Be( expected );
                sut.Should().BeSequentiallyEqualTo( heapifiedItems );
            }
        }

        [Fact]
        public void Extract_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
        {
            var sut = new DictionaryHeap<TKey, TValue>();
            var action = Lambda.Of( () => sut.Extract() );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void TryExtract_ShouldReturnFalse_WhenHeapIsEmpty()
        {
            var sut = new DictionaryHeap<TKey, TValue>();

            var result = sut.TryExtract( out var extracted );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                extracted.Should().Be( default( TValue ) );
            }
        }

        [Fact]
        public void TryExtract_ShouldReturnTrueAndRemoveTopItem()
        {
            var key = Fixture.Create<TKey>();
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( key, item ) } );

            var result = sut.TryExtract( out var extracted );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                extracted.Should().Be( item );
                sut.Count.Should().Be( 0 );
                sut.ContainsKey( key ).Should().BeFalse();
            }
        }

        [Fact]
        public void TryExtract_ShouldSatisfyHeapInvariant()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var items = Fixture.CreateDistinctCollection<TValue>( 20 );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );
            var expectedKey = sut.GetKey( 0 );
            var expectedExtracted = sut[0];

            var result = sut.TryExtract( out var extracted );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                extracted.Should().Be( expectedExtracted );
                sut.Should().NotContain( expectedExtracted );
                sut.Should().BeEquivalentTo( items.Where( i => ! i!.Equals( expectedExtracted ) ) );
                sut.ContainsKey( expectedKey ).Should().BeFalse();
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void Remove_ShouldThrowKeyNotFoundException_WhenItemWithKeyDoesNotExist()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], item ) } );

            var action = Lambda.Of( () => sut.Remove( keys[1] ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Remove_ShouldRemoveMiddleItemAndReturnIt_WhenRemovedItemIsLessThanLastItem()
        {
            var index = 2;
            var keys = Fixture.CreateDistinctCollection<TKey>( 7 );
            var items = Fixture.CreateDistinctSortedCollection<TValue>( 7 );
            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.Remove( keys[index] );

            using ( new AssertionScope() )
            {
                result.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeFalse();
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void Remove_ShouldRemoveMiddleItemAndReturnIt_WhenRemovedItemIsGreaterThanLastItem()
        {
            var index = 3;
            var keys = Fixture.CreateDistinctCollection<TKey>( 7 );
            var sortedItems = Fixture.CreateDistinctSortedCollection<TValue>( 7 );
            var items = new List<TValue>
            {
                sortedItems[0],
                sortedItems[4],
                sortedItems[1],
                sortedItems[5],
                sortedItems[6],
                sortedItems[2],
                sortedItems[3]
            };

            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.Remove( keys[index] );

            using ( new AssertionScope() )
            {
                result.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeFalse();
                AssertHeapInvariant( sut );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 5 )]
        [InlineData( 10 )]
        [InlineData( 15 )]
        [InlineData( 18 )]
        [InlineData( 19 )]
        public void Remove_ShouldSatisfyHeapInvariant(int index)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var items = Fixture.CreateDistinctCollection<TValue>( 20 );
            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.Remove( keys[index] );

            using ( new AssertionScope() )
            {
                result.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeFalse();
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void TryRemove_ShouldReturnFalse_WhenItemWithKeyDoesNotExist()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], item ) } );

            var result = sut.TryRemove( keys[1], out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                removed.Should().Be( default( TValue ) );
            }
        }

        [Fact]
        public void TryRemove_ShouldRemoveMiddleItemAndReturnIt_WhenRemovedItemIsLessThanLastItem()
        {
            var index = 2;
            var keys = Fixture.CreateDistinctCollection<TKey>( 7 );
            var items = Fixture.CreateDistinctSortedCollection<TValue>( 7 );
            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.TryRemove( keys[index], out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                removed.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeFalse();
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void TryRemove_ShouldRemoveMiddleItemAndReturnIt_WhenRemovedItemIsGreaterThanLastItem()
        {
            var index = 3;
            var keys = Fixture.CreateDistinctCollection<TKey>( 7 );
            var sortedItems = Fixture.CreateDistinctSortedCollection<TValue>( 7 );
            var items = new List<TValue>
            {
                sortedItems[0],
                sortedItems[4],
                sortedItems[1],
                sortedItems[5],
                sortedItems[6],
                sortedItems[2],
                sortedItems[3]
            };

            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.TryRemove( keys[index], out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                removed.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeFalse();
                AssertHeapInvariant( sut );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 5 )]
        [InlineData( 10 )]
        [InlineData( 15 )]
        [InlineData( 18 )]
        [InlineData( 19 )]
        public void TryRemove_ShouldSatisfyHeapInvariant(int index)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var items = Fixture.CreateDistinctCollection<TValue>( 20 );
            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.TryRemove( keys[index], out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                removed.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeFalse();
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void Pop_ShouldThrowArgumentOutOfRangeException_WhenHeapIsEmpty()
        {
            var sut = new DictionaryHeap<TKey, TValue>();
            var action = Lambda.Of( () => sut.Pop() );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void TryPop_ShouldReturnFalse_WhenHeapIsEmpty()
        {
            var sut = new DictionaryHeap<TKey, TValue>();

            var result = sut.TryPop();

            result.Should().BeFalse();
        }

        [Fact]
        public void TryPop_ShouldReturnTrueAndRemoveTopItem()
        {
            var key = Fixture.Create<TKey>();
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( key, item ) } );

            var result = sut.TryPop();

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                sut.ContainsKey( key ).Should().BeFalse();
            }
        }

        [Fact]
        public void TryPop_ShouldSatisfyHeapInvariant()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var items = Fixture.CreateDistinctCollection<TValue>( 20 );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );
            var expectedKey = sut.GetKey( 0 );
            var expectedExtracted = sut[0];

            var result = sut.TryPop();

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Should().NotContain( expectedExtracted );
                sut.Should().BeEquivalentTo( items.Where( i => ! i!.Equals( expectedExtracted ) ) );
                sut.ContainsKey( expectedKey ).Should().BeFalse();
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void Replace_ShouldThrowKeyNotFoundException_WhenItemWithKeyDoesNotExist()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], item ) } );

            var action = Lambda.Of( () => sut.Replace( keys[1], item ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Replace_ShouldReplaceMiddleItemAndReturnIt_WhenOldValueIsLessThanNewValue()
        {
            var index = 3;
            var keys = Fixture.CreateDistinctCollection<TKey>( 7 );
            var allItems = Fixture.CreateDistinctSortedCollection<TValue>( 8 );
            var items = allItems.SkipLast( 1 ).ToList();
            var newValue = allItems[^1];
            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) ).Append( newValue );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.Replace( keys[index], newValue );

            using ( new AssertionScope() )
            {
                result.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeTrue();
                sut.GetValue( keys[index] ).Should().Be( newValue );
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void Replace_ShouldRemoveMiddleItemAndReturnIt_WhenRemovedItemIsLessThanLastItem()
        {
            var index = 3;
            var keys = Fixture.CreateDistinctCollection<TKey>( 7 );
            var allItems = Fixture.CreateDistinctSortedCollection<TValue>( 8 );
            var items = allItems.Skip( 1 ).ToList();
            var newValue = allItems[0];
            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) ).Append( newValue );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.Replace( keys[index], newValue );

            using ( new AssertionScope() )
            {
                result.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeTrue();
                sut.GetValue( keys[index] ).Should().Be( newValue );
                AssertHeapInvariant( sut );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 5 )]
        [InlineData( 10 )]
        [InlineData( 15 )]
        [InlineData( 18 )]
        [InlineData( 19 )]
        public void Replace_ShouldSatisfyHeapInvariant(int index)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var allItems = Fixture.CreateDistinctCollection<TValue>( 21 );
            var items = allItems.SkipLast( 1 ).ToList();
            var newValue = allItems[^1];
            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) ).Append( newValue );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.Replace( keys[index], newValue );

            using ( new AssertionScope() )
            {
                result.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeTrue();
                sut.GetValue( keys[index] ).Should().Be( newValue );
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void TryReplace_ShouldReturnFalse_WhenItemWithKeyDoesNotExist()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], item ) } );

            var result = sut.TryReplace( keys[1], item, out var replaced );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                replaced.Should().Be( default( TValue ) );
            }
        }

        [Fact]
        public void TryReplace_ShouldReplaceMiddleItemAndReturnIt_WhenOldValueIsLessThanNewValue()
        {
            var index = 3;
            var keys = Fixture.CreateDistinctCollection<TKey>( 7 );
            var allItems = Fixture.CreateDistinctSortedCollection<TValue>( 8 );
            var items = allItems.SkipLast( 1 ).ToList();
            var newValue = allItems[^1];
            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) ).Append( newValue );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.TryReplace( keys[index], newValue, out var replaced );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                replaced.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeTrue();
                sut.GetValue( keys[index] ).Should().Be( newValue );
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void TryReplace_ShouldRemoveMiddleItemAndReturnIt_WhenRemovedItemIsLessThanLastItem()
        {
            var index = 3;
            var keys = Fixture.CreateDistinctCollection<TKey>( 7 );
            var allItems = Fixture.CreateDistinctSortedCollection<TValue>( 8 );
            var items = allItems.Skip( 1 ).ToList();
            var newValue = allItems[0];
            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) ).Append( newValue );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.TryReplace( keys[index], newValue, out var replaced );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                replaced.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeTrue();
                sut.GetValue( keys[index] ).Should().Be( newValue );
                AssertHeapInvariant( sut );
            }
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        [InlineData( 5 )]
        [InlineData( 10 )]
        [InlineData( 15 )]
        [InlineData( 18 )]
        [InlineData( 19 )]
        public void TryReplace_ShouldSatisfyHeapInvariant(int index)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 20 );
            var allItems = Fixture.CreateDistinctCollection<TValue>( 21 );
            var items = allItems.SkipLast( 1 ).ToList();
            var newValue = allItems[^1];
            var expected = items.Take( index ).Concat( items.Skip( index + 1 ) ).Append( newValue );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.TryReplace( keys[index], newValue, out var replaced );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                replaced.Should().Be( items[index] );
                sut.Should().BeEquivalentTo( expected );
                sut.ContainsKey( keys[index] ).Should().BeTrue();
                sut.GetValue( keys[index] ).Should().Be( newValue );
                AssertHeapInvariant( sut );
            }
        }

        [Fact]
        public void AddOrReplace_ShouldAddNewItemAndReturnIt_WhenKeyDoesNotExist()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var (value, other) = Fixture.CreateDistinctCollection<TValue>( 2 );

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], other ) } );

            var result = sut.AddOrReplace( keys[1], value );

            using ( new AssertionScope() )
            {
                result.Should().Be( value );
                sut.Should().BeEquivalentTo( value, other );
            }
        }

        [Fact]
        public void AddOrReplace_ShouldReplaceExistingItemAndReturnOldValue_WhenKeyExists()
        {
            var key = Fixture.Create<TKey>();
            var (value, other) = Fixture.CreateDistinctCollection<TValue>( 2 );

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( key, other ) } );

            var result = sut.AddOrReplace( key, value );

            using ( new AssertionScope() )
            {
                result.Should().Be( other );
                sut.Should().BeEquivalentTo( value );
            }
        }

        [Fact]
        public void Clear_ShouldRemoveAllItems()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 10 );
            var items = Fixture.CreateMany<TValue>( 10 );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            sut.Clear();

            sut.Count.Should().Be( 0 );
        }

        [Theory]
        [InlineData( -1 )]
        [InlineData( 3 )]
        public void IndexerGet_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfBounds(int index)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var items = Fixture.CreateMany<TValue>( 3 );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var action = Lambda.Of( () => sut[index] );

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void IndexerGet_ShouldReturnCorrectResult(int index)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var items = Fixture.CreateDistinctSortedCollection<TValue>( 3 );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut[index];

            result.Should().Be( items[index] );
        }

        [Theory]
        [InlineData( -1 )]
        [InlineData( 3 )]
        public void GetKey_ShouldThrowArgumentOutOfRangeException_WhenIndexIsOutOfBounds(int index)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var items = Fixture.CreateMany<TValue>( 3 );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var action = Lambda.Of( () => sut.GetKey( index ) );

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void GetKey_ShouldReturnCorrectResult(int index)
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
            var items = Fixture.CreateDistinctSortedCollection<TValue>( 3 );

            var sut = new DictionaryHeap<TKey, TValue>( keys.Zip( items, KeyValuePair.Create ) );

            var result = sut.GetKey( index );

            result.Should().Be( keys[index] );
        }

        [Fact]
        public void ContainsKey_ShouldReturnTrue_WhenItemWithKeyExists()
        {
            var key = Fixture.Create<TKey>();
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( key, item ) } );

            var result = sut.ContainsKey( key );

            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsKey_ShouldReturnFalse_WhenItemWithKeyDoesNotExist()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], item ) } );

            var result = sut.ContainsKey( keys[1] );

            result.Should().BeFalse();
        }

        [Fact]
        public void GetValue_ShouldReturnCorrectResult_WhenItemWithKeyExists()
        {
            var key = Fixture.Create<TKey>();
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( key, item ) } );

            var result = sut.GetValue( key );

            result.Should().Be( item );
        }

        [Fact]
        public void GetValue_ShouldThrowKeyNotFoundException_WhenItemWithKeyDoesNotExist()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], item ) } );

            var action = Lambda.Of( () => sut.GetValue( keys[1] ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void TryGetValue_ShouldReturnTrueAndCorrectResult_WhenItemWithKeyExists()
        {
            var key = Fixture.Create<TKey>();
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( key, item ) } );

            var result = sut.TryGetValue( key, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().Be( item );
            }
        }

        [Fact]
        public void TryGetValue_ShouldReturnFalse_WhenItemWithKeyDoesNotExist()
        {
            var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
            var item = Fixture.Create<TValue>();

            var sut = new DictionaryHeap<TKey, TValue>( new[] { KeyValuePair.Create( keys[0], item ) } );

            var result = sut.TryGetValue( keys[1], out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default( TValue ) );
            }
        }

        private static void AssertHeapInvariant(DictionaryHeap<TKey, TValue> heap)
        {
            var comparer = heap.Comparer;
            var maxParentIndex = (heap.Count >> 1) - 1;

            for ( var parentIndex = 0; parentIndex <= maxParentIndex; ++parentIndex )
            {
                var parent = heap[parentIndex];

                var leftChildIndex = Heap.GetLeftChildIndex( parentIndex );
                var rightChildIndex = Heap.GetRightChildIndex( parentIndex );

                var leftChild = heap[leftChildIndex];
                var leftChildComparisonResult = comparer.Compare( parent, leftChild );

                leftChildComparisonResult.Should()
                    .BeLessOrEqualTo(
                        0,
                        "min heap invariant must be satisfied, which means that parent {0} must be less than or equal to its left child {1}",
                        parent,
                        leftChild );

                if ( rightChildIndex >= heap.Count )
                    continue;

                var rightChild = heap[rightChildIndex];
                var rightChildComparisonResult = comparer.Compare( parent, rightChild );

                rightChildComparisonResult.Should()
                    .BeLessOrEqualTo(
                        0,
                        "min heap invariant must be satisfied, which means that parent {0} must be less than or equal to its right child {1}",
                        parent,
                        rightChild );
            }
        }
    }
}
