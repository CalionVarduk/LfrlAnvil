using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Collections.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Collections.Tests.MultiDictionaryTests;

public abstract class GenericMultiDictionaryTests<TKey, TValue> : TestsBase
    where TKey : notnull
{
    [Fact]
    public void Ctor_ShouldCreateEmpty()
    {
        var sut = new MultiDictionary<TKey, TValue>();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Comparer.Should().Be( EqualityComparer<TKey>.Default );
        }
    }

    [Fact]
    public void Ctor_ShouldCreateEmpty_WithExplicitComparer()
    {
        var comparer = EqualityComparerFactory<TKey>.Create( (a, b) => a!.Equals( b ) );
        var sut = new MultiDictionary<TKey, TValue>( comparer );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Comparer.Should().Be( comparer );
        }
    }

    [Fact]
    public void Add_ShouldAddNewItemToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();

        sut.Add( key, value );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().BeSequentiallyEqualTo( value );
        }
    }

    [Fact]
    public void Add_ShouldAddNewItemToNonEmptyDictionaryCorrectly_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 2 );
        var values = Fixture.CreateDistinctCollection<TValue>( count: 2 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( keys[0], values[0] );

        sut.Add( keys[1], values[1] );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut[keys[1]].Should().BeSequentiallyEqualTo( values[1] );
        }
    }

    [Fact]
    public void Add_ShouldAddNewItemToNonEmptyDictionaryCorrectly_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 2 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, values[0] );

        sut.Add( key, values[1] );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void AddRange_ShouldAddNewItemsToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        sut.AddRange( key, values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void AddRange_ShouldAddNewItemsToNonEmptyDictionaryCorrectly_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 2 );
        var allValues = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( keys[0], allValues[0] );

        sut.AddRange( keys[1], values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut[keys[1]].Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void AddRange_ShouldAddNewItemsToNonEmptyDictionaryCorrectly_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var allValues = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, allValues[0] );

        sut.AddRange( key, values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().BeSequentiallyEqualTo( allValues );
        }
    }

    [Fact]
    public void AddRange_ShouldDoNothing_WhenValuesAreEmpty()
    {
        var key = Fixture.Create<TKey>();
        var values = Enumerable.Empty<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();

        sut.AddRange( key, values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.ContainsKey( key ).Should().BeFalse();
        }
    }

    [Fact]
    public void SetRange_ShouldAddNewItemsToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        sut.SetRange( key, values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void SetRange_ShouldAddNewItemsToNonEmptyDictionaryCorrectly_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 2 );
        var allValues = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( keys[0], allValues[0] );

        sut.SetRange( keys[1], values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut[keys[1]].Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void SetRange_ShouldReplaceExistingItemsInNonEmptyDictionaryCorrectly_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var allValues = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, allValues[0] );

        sut.SetRange( key, values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void SetRange_ShouldRemoveKey_WhenValuesAreEmpty()
    {
        var key = Fixture.Create<TKey>();
        var oldValue = Fixture.Create<TValue>();
        var values = Enumerable.Empty<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, oldValue );

        sut.SetRange( key, values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.ContainsKey( key ).Should().BeFalse();
        }
    }

    [Fact]
    public void Remove_ShouldReturnEmptyList_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.Remove( key );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveExistingItems_WhenDictionaryHasOneKey()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.Remove( key );

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( values );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveCorrectExistingItems()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 2 );
        var values = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( keys[0], values.Take( 2 ) );
        sut.AddRange( keys[1], values.Skip( 2 ) );

        var result = sut.Remove( keys[0] );

        using ( new AssertionScope() )
        {
            result.Should().BeSequentiallyEqualTo( values.Take( 2 ) );
            sut.Count.Should().Be( 1 );
            sut.ContainsKey( keys[1] ).Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_WithValue_ShouldReturnFalse_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.Remove( key, value );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_WithValue_ShouldReturnTrueAndRemoveExistingItem_WhenKeyAndItemExist()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.Remove( key, values[1] );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut[key].Should().BeSequentiallyEqualTo( values[0], values[2] );
        }
    }

    [Fact]
    public void Remove_WithValue_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values.Take( 2 ) );

        var result = sut.Remove( key, values[2] );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut[key].Should().BeSequentiallyEqualTo( values.Take( 2 ) );
        }
    }

    [Fact]
    public void Remove_WithValue_ShouldReturnTrueAndRemoveKey_WhenItemExistsAndListOnlyContainsOneItem()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, value );

        var result = sut.Remove( key, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void RemoveAt_ShouldReturnFalse_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.RemoveAt( key, index: 0 );

        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveAt_ShouldReturnTrueAndRemoveExistingItem_WhenKeyAndIndexExist()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveAt( key, index: 1 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut[key].Should().BeSequentiallyEqualTo( values[0], values[2] );
        }
    }

    [Fact]
    public void RemoveAt_ShouldReturnTrueAndRemoveKey_WhenIndexExistsAndListOnlyContainsOneItem()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, value );

        var result = sut.RemoveAt( key, index: 0 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void RemoveAt_ShouldThrowArgumentOutOfRangeException_WhenIndexDoesNotExist(int index)
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var action = Lambda.Of( () => sut.RemoveAt( key, index ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RemoveRange_ShouldReturnFalse_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.RemoveRange( key, index: 0, count: 0 );

        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveRange_ShouldReturnTrueAndRemoveExistingItems_WhenKeyAndRangeExist()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveRange( key, index: 1, count: 2 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut[key].Should().BeSequentiallyEqualTo( values[0], values[3] );
        }
    }

    [Fact]
    public void RemoveRange_ShouldReturnTrueAndRemoveKey_WhenRangeExistsAndAllListItemsAreRemoved()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveRange( key, index: 0, count: 3 );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
        }
    }

    [Theory]
    [InlineData( -1, 1 )]
    [InlineData( 0, -1 )]
    public void RemoveRange_ShouldThrowArgumentOutOfRangeException_WhenIndexOrCountIsNegative(int index, int count)
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var action = Lambda.Of( () => sut.RemoveRange( key, index, count ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 4 )]
    [InlineData( 2, 2 )]
    [InlineData( 3, 1 )]
    public void RemoveRange_ShouldThrowArgumentException_WhenIndexAndCountDenoteInvalidRange(int index, int count)
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var action = Lambda.Of( () => sut.RemoveRange( key, index, count ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void RemoveAll_ShouldReturnZero_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.RemoveAll( key, _ => true );

        result.Should().Be( 0 );
    }

    [Fact]
    public void RemoveAll_ShouldReturnAmountOfRemovedItems_WhenKeyExistsAndPredicateFindsItemsToRemove()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveAll( key, v => v!.Equals( values[0] ) || v.Equals( values[1] ) );

        using ( new AssertionScope() )
        {
            result.Should().Be( 2 );
            sut[key].Should().BeSequentiallyEqualTo( values[2] );
        }
    }

    [Fact]
    public void RemoveAll_ShouldReturnZero_WhenPredicateDoesNotFindItemsToRemove()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveAll( key, _ => false );

        using ( new AssertionScope() )
        {
            result.Should().Be( 0 );
            sut[key].Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void RemoveAll_ShouldReturnAmountOfRemovedItemsAndRemoveKey_WhenAllListItemsPassThePredicate()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveAll( key, _ => true );

        using ( new AssertionScope() )
        {
            result.Should().Be( 3 );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrueAndCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.TryGetValue( key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalseAndNullResult_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.TryGetValue( key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().BeNull();
        }
    }

    [Fact]
    public void GetCount_ShouldReturnCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.GetCount( key );

        result.Should().Be( 3 );
    }

    [Fact]
    public void GetCount_ShouldReturnZero_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.GetCount( key );

        result.Should().Be( 0 );
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        sut.Clear();

        sut.Count.Should().Be( 0 );
    }

    [Fact]
    public void IndexerGet_ShouldReturnEmptyCollection_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut[key];

        result.Should().BeEmpty();
    }

    [Fact]
    public void IndexerSet_ShouldAddNewItemsToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        sut[key] = values;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void IndexerSet_ShouldAddNewItemsToNonEmptyDictionaryCorrectly_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 2 );
        var allValues = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( keys[0], allValues[0] );

        sut[keys[1]] = values;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut[keys[1]].Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void IndexerSet_ShouldReplaceExistingItemsInNonEmptyDictionaryCorrectly_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var allValues = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, allValues[0] );

        sut[key] = values;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().BeSequentiallyEqualTo( values );
        }
    }

    [Fact]
    public void IndexerSet_ShouldRemoveKey_WhenValuesAreEmpty()
    {
        var key = Fixture.Create<TKey>();
        var oldValue = Fixture.Create<TValue>();
        var values = Array.Empty<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, oldValue );

        sut[key] = values;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.ContainsKey( key ).Should().BeFalse();
        }
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        var result = sut.AsEnumerable().ToList();

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 3 );
            result.SelectMany( kv => kv.Value.Select( v => KeyValuePair.Create( kv.Key, v ) ) )
                .Should()
                .BeEquivalentTo( keys.Zip( values ).Select( x => KeyValuePair.Create( x.First, x.Second ) ) );
        }
    }

    [Fact]
    public void Keys_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        var result = sut.Keys.ToList();

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 3 );
            result.Should().BeEquivalentTo( keys );
        }
    }

    [Fact]
    public void Values_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        var result = sut.Values.ToList();

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 3 );
            result.SelectMany( v => v ).Should().BeEquivalentTo( values );
        }
    }

    [Fact]
    public void ILookup_IndexerGet_ShouldReturnEmptyCollection_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<TKey>();
        ILookup<TKey, TValue> sut = new MultiDictionary<TKey, TValue>();

        var result = sut[key];

        result.Should().BeEmpty();
    }

    [Fact]
    public void ILookup_IndexerGet_ShouldReturnCorrectCollection_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var source = new MultiDictionary<TKey, TValue>();
        source.AddRange( key, values );
        ILookup<TKey, TValue> sut = source;

        var result = sut[key];

        result.Should().BeSequentiallyEqualTo( values );
    }

    [Fact]
    public void ILookup_Contains_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<TKey>();
        ILookup<TKey, TValue> sut = new MultiDictionary<TKey, TValue>();

        var result = sut.Contains( key );

        result.Should().BeFalse();
    }

    [Fact]
    public void ILookup_Contains_ShouldReturnTrue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var source = new MultiDictionary<TKey, TValue>();
        source.AddRange( key, values );
        ILookup<TKey, TValue> sut = source;

        var result = sut.Contains( key );

        result.Should().BeTrue();
    }

    [Fact]
    public void ILookup_GetEnumerator_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        var result = ((ILookup<TKey, TValue>)sut).ToList();

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 3 );
            result.SelectMany( g => g.Select( v => KeyValuePair.Create( g.Key, v ) ) )
                .Should()
                .BeEquivalentTo( keys.Zip( values ).Select( x => KeyValuePair.Create( x.First, x.Second ) ) );
        }
    }

    [Fact]
    public void IDictionaryKeys_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( 3 );
        var source = new MultiDictionary<TKey, TValue>();
        IDictionary<TKey, IReadOnlyList<TValue>> sut = source;
        source.Add( keys[0], values[0] );
        source.Add( keys[1], values[1] );
        source.Add( keys[2], values[2] );

        var result = sut.Keys;

        result.Should().BeEquivalentTo( keys );
    }

    [Fact]
    public void IDictionaryValues_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( 3 );
        var source = new MultiDictionary<TKey, TValue>();
        IDictionary<TKey, IReadOnlyList<TValue>> sut = source;
        source.Add( keys[0], values[0] );
        source.Add( keys[1], values[1] );
        source.Add( keys[2], values[2] );

        var result = sut.Values;

        result.SelectMany( v => v ).Should().BeEquivalentTo( values );
    }

    [Fact]
    public void IDictionaryAdd_ShouldBeEquivalentToAddRange()
    {
        var key = Fixture.Create<TKey>();
        var allValues = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var source = new MultiDictionary<TKey, TValue>();
        IDictionary<TKey, IReadOnlyList<TValue>> sut = source;
        source.Add( key, allValues[0] );

        sut.Add( key, values );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().BeSequentiallyEqualTo( allValues );
        }
    }

    [Fact]
    public void IDictionaryRemove_ShouldBeEquivalentToRemove()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 2 );
        var values = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var source = new MultiDictionary<TKey, TValue>();
        IDictionary<TKey, IReadOnlyList<TValue>> sut = source;
        source.AddRange( keys[0], values.Take( 2 ) );
        source.AddRange( keys[1], values.Skip( 2 ) );

        var result = sut.Remove( keys[0] );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.ContainsKey( keys[1] ).Should().BeTrue();
        }
    }

    [Fact]
    public void ICollectionAdd_ShouldBeEquivalentToAddRange()
    {
        var key = Fixture.Create<TKey>();
        var allValues = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var source = new MultiDictionary<TKey, TValue>();
        ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>> sut = source;
        source.Add( key, allValues[0] );

        sut.Add( KeyValuePair.Create( key, (IReadOnlyList<TValue>)values ) );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            source[key].Should().BeSequentiallyEqualTo( allValues );
        }
    }

    [Fact]
    public void ICollectionRemove_ShouldBeEquivalentToRemoveAll_AndReturnTrueIfAtLeastOneElementWasRemoved()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( count: 2 );
        var values = Fixture.CreateDistinctCollection<TValue>( count: 6 );
        var source = new MultiDictionary<TKey, TValue>();
        ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>> sut = source;
        source.AddRange( keys[0], values.Take( 3 ) );
        source.AddRange( keys[1], values.Skip( 3 ) );

        var result = sut.Remove( KeyValuePair.Create( keys[0], (IReadOnlyList<TValue>)new[] { values[0], values[2] } ) );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 2 );
            source[keys[0]].Should().BeSequentiallyEqualTo( values[1] );
        }
    }

    [Fact]
    public void ICollectionContains_ShouldReturnFalse_WhenAnyElementDoesNotExistInValuesUnderTheSpecifiedKey()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 4 );
        var source = new MultiDictionary<TKey, TValue>();
        ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>> sut = source;
        source.AddRange( key, values.Take( 3 ) );

        var result = sut.Contains( KeyValuePair.Create( key, values ) );

        result.Should().BeFalse();
    }

    [Fact]
    public void ICollectionContains_ShouldReturnTrue_WhenAllElementsExistInValuesUnderTheSpecifiedKey()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( count: 3 );
        var source = new MultiDictionary<TKey, TValue>();
        ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>> sut = source;
        source.AddRange( key, values );

        var result = sut.Contains( KeyValuePair.Create( key, (IReadOnlyList<TValue>)new[] { values[0], values[2] } ) );

        result.Should().BeTrue();
    }
}
