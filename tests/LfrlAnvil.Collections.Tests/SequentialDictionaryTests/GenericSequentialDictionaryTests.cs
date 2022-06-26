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

namespace LfrlAnvil.Collections.Tests.SequentialDictionaryTests;

public abstract class GenericSequentialDictionaryTests<TKey, TValue> : GenericDictionaryTestsBase<TKey, TValue>
    where TKey : notnull
{
    [Fact]
    public void Ctor_ShouldCreateEmpty()
    {
        var sut = new SequentialDictionary<TKey, TValue>();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Comparer.Should().Be( EqualityComparer<TKey>.Default );
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
        }
    }

    [Fact]
    public void Ctor_ShouldCreateEmpty_WithExplicitComparer()
    {
        var comparer = EqualityComparerFactory<TKey>.Create( (a, b) => a!.Equals( b ) );
        var sut = new SequentialDictionary<TKey, TValue>( comparer );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Comparer.Should().Be( comparer );
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
        }
    }

    [Fact]
    public void Add_ShouldAddNewItemToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new SequentialDictionary<TKey, TValue>();

        sut.Add( key, value );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().Be( value );
            sut.First.Should().Be( KeyValuePair.Create( key, value ) );
            sut.Last.Should().Be( KeyValuePair.Create( key, value ) );
        }
    }

    [Fact]
    public void Add_ShouldAddNewItemToNonEmptyDictionaryCorrectly()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
        var values = Fixture.CreateDistinctCollection<TValue>( 2 );
        var sut = new SequentialDictionary<TKey, TValue> { { keys[0], values[0] } };

        sut.Add( keys[1], values[1] );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut[keys[1]].Should().Be( values[1] );
            sut.First.Should().Be( KeyValuePair.Create( keys[0], values[0] ) );
            sut.Last.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
        }
    }

    [Fact]
    public void Add_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( 2 );
        var sut = new SequentialDictionary<TKey, TValue> { { key, values[0] } };

        var action = Lambda.Of( () => sut.Add( key, values[1] ) );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new SequentialDictionary<TKey, TValue>();

        var result = sut.Remove( key );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveExistingItem_WhenDictionaryHasOneItem()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new SequentialDictionary<TKey, TValue> { { key, value } };

        var result = sut.Remove( key );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
        }
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveCorrectExistingItem()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
        var values = Fixture.CreateDistinctCollection<TValue>( 2 );
        var sut = new SequentialDictionary<TKey, TValue> { { keys[0], values[0] }, { keys[1], values[1] } };

        var result = sut.Remove( keys[0] );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.ContainsKey( keys[1] ).Should().BeTrue();
            sut.First.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
            sut.Last.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
        }
    }

    [Fact]
    public void Remove_WithRemoved_ShouldReturnFalseAndDefaultRemoved_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new SequentialDictionary<TKey, TValue>();

        var result = sut.Remove( key, out var removed );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            removed.Should().Be( default( TValue ) );
        }
    }

    [Fact]
    public void Remove_WithRemoved_ShouldReturnTrueAndRemoveExistingItem_WhenDictionaryHasOneItem()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new SequentialDictionary<TKey, TValue> { { key, value } };

        var result = sut.Remove( key, out var removed );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 0 );
            removed.Should().Be( value );
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
        }
    }

    [Fact]
    public void Remove_WithRemoved_ShouldReturnTrueAndRemoveCorrectExistingItem()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
        var values = Fixture.CreateDistinctCollection<TValue>( 2 );
        var sut = new SequentialDictionary<TKey, TValue> { { keys[0], values[0] }, { keys[1], values[1] } };

        var result = sut.Remove( keys[0], out var removed );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.Count.Should().Be( 1 );
            sut.ContainsKey( keys[1] ).Should().BeTrue();
            removed.Should().Be( values[0] );
            sut.First.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
            sut.Last.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrueAndCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new SequentialDictionary<TKey, TValue> { { key, value } };

        var result = sut.TryGetValue( key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( value );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalseAndDefaultResult_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new SequentialDictionary<TKey, TValue>();

        var result = sut.TryGetValue( key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default( TValue ) );
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( 3 );
        var sut = new SequentialDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.First.Should().BeNull();
            sut.Last.Should().BeNull();
        }
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectAndOrderedResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 10 );
        var values = Fixture.CreateDistinctCollection<TValue>( 10 );

        var initialItems = keys.Zip( values, KeyValuePair.Create ).Take( 7 );
        var keysToRemove = new[] { keys[0], keys[2], keys[6] };
        var itemsToAddLast = keys.Zip( values, KeyValuePair.Create ).Skip( 7 );

        var expected = new[] { keys[1], keys[3], keys[4], keys[5], keys[7], keys[8], keys[9] }
            .Zip( new[] { values[1], values[3], values[4], values[5], values[7], values[8], values[9] }, KeyValuePair.Create );

        var sut = GetDictionaryForOrderVerification( initialItems, keysToRemove, itemsToAddLast ).AsEnumerable();

        sut.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void Keys_ShouldReturnCorrectAndOrderedResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 10 );
        var values = Fixture.CreateDistinctCollection<TValue>( 10 );

        var initialItems = keys.Zip( values, KeyValuePair.Create ).Take( 7 );
        var keysToRemove = new[] { keys[0], keys[2], keys[6] };
        var itemsToAddLast = keys.Zip( values, KeyValuePair.Create ).Skip( 7 );

        var expected = new[] { keys[1], keys[3], keys[4], keys[5], keys[7], keys[8], keys[9] };

        var sut = GetDictionaryForOrderVerification( initialItems, keysToRemove, itemsToAddLast );

        var result = sut.Keys;

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void Values_ShouldReturnCorrectAndOrderedResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 10 );
        var values = Fixture.CreateDistinctCollection<TValue>( 10 );

        var initialItems = keys.Zip( values, KeyValuePair.Create ).Take( 7 );
        var keysToRemove = new[] { keys[0], keys[2], keys[6] };
        var itemsToAddLast = keys.Zip( values, KeyValuePair.Create ).Skip( 7 );

        var expected = new[] { values[1], values[3], values[4], values[5], values[7], values[8], values[9] };

        var sut = GetDictionaryForOrderVerification( initialItems, keysToRemove, itemsToAddLast );

        var result = sut.Values;

        result.Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void IndexerSet_ShouldAddNewItemToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new SequentialDictionary<TKey, TValue>();

        sut[key] = value;

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().Be( value );
            sut.First.Should().Be( KeyValuePair.Create( key, value ) );
            sut.Last.Should().Be( KeyValuePair.Create( key, value ) );
        }
    }

    [Fact]
    public void IndexerSet_ShouldAddNewItemToNonEmptyDictionaryCorrectly()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 2 );
        var values = Fixture.CreateDistinctCollection<TValue>( 2 );
        var sut = new SequentialDictionary<TKey, TValue> { { keys[0], values[0] } };

        sut[keys[1]] = values[1];

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 2 );
            sut[keys[1]].Should().Be( values[1] );
            sut.First.Should().Be( KeyValuePair.Create( keys[0], values[0] ) );
            sut.Last.Should().Be( KeyValuePair.Create( keys[1], values[1] ) );
        }
    }

    [Fact]
    public void IndexerSet_ShouldReplaceExistingItemCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( 2 );
        var sut = new SequentialDictionary<TKey, TValue> { { key, values[0] } };

        sut[key] = values[1];

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 1 );
            sut[key].Should().Be( values[1] );
            sut.First.Should().Be( KeyValuePair.Create( key, values[1] ) );
            sut.Last.Should().Be( KeyValuePair.Create( key, values[1] ) );
        }
    }

    [Fact]
    public void IndexerSet_ShouldNotChangeOrderOfItems_WhenReplacing()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 10 );
        var values = Fixture.CreateDistinctCollection<TValue>( 11 );

        var initialItems = keys.Zip( values, KeyValuePair.Create ).Take( 7 );
        var keysToRemove = new[] { keys[0], keys[2], keys[6] };
        var itemsToAddLast = keys.Zip( values, KeyValuePair.Create ).Skip( 7 ).Take( 3 );
        var keyToReplace = keys[4];

        var expected = new[] { keys[1], keys[3], keys[4], keys[5], keys[7], keys[8], keys[9] }
            .Zip( new[] { values[1], values[3], values[10], values[5], values[7], values[8], values[9] }, KeyValuePair.Create );

        var sut = GetDictionaryForOrderVerification( initialItems, keysToRemove, itemsToAddLast );

        sut[keyToReplace] = values[10];

        sut.AsEnumerable().Should().BeSequentiallyEqualTo( expected );
    }

    protected sealed override IDictionary<TKey, TValue> CreateEmptyDictionary()
    {
        return new SequentialDictionary<TKey, TValue>();
    }

    private static SequentialDictionary<TKey, TValue> GetDictionaryForOrderVerification(
        IEnumerable<KeyValuePair<TKey, TValue>> initialItems,
        IEnumerable<TKey> keysToRemove,
        IEnumerable<KeyValuePair<TKey, TValue>> itemsToAddLast)
    {
        var result = new SequentialDictionary<TKey, TValue>();

        foreach ( var (k, v) in initialItems )
            result.Add( k, v );

        foreach ( var k in keysToRemove )
            result.Remove( k );

        foreach ( var (k, v) in itemsToAddLast )
            result.Add( k, v );

        return result;
    }
}