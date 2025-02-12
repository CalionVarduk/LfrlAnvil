using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Collections.Tests.SequentialDictionaryTests;

public abstract class GenericSequentialDictionaryTests<TKey, TValue> : GenericDictionaryTestsBase<TKey, TValue>
    where TKey : notnull
{
    [Fact]
    public void Ctor_ShouldCreateEmpty()
    {
        var sut = new SequentialDictionary<TKey, TValue>();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestEquals( EqualityComparer<TKey>.Default ),
                sut.First.TestNull(),
                sut.Last.TestNull() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateEmpty_WithExplicitComparer()
    {
        var comparer = EqualityComparerFactory<TKey>.Create( (a, b) => a!.Equals( b ) );
        var sut = new SequentialDictionary<TKey, TValue>( comparer );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestRefEquals( comparer ),
                sut.First.TestNull(),
                sut.Last.TestNull() )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new SequentialDictionary<TKey, TValue>();

        sut.Add( key, value );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestEquals( value ),
                sut.First.TestEquals( KeyValuePair.Create( key, value ) ),
                sut.Last.TestEquals( KeyValuePair.Create( key, value ) ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemToNonEmptyDictionaryCorrectly()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new SequentialDictionary<TKey, TValue> { { keys[0], values[0] } };

        sut.Add( keys[1], values[1] );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut[keys[1]].TestEquals( values[1] ),
                sut.First.TestEquals( KeyValuePair.Create( keys[0], values[0] ) ),
                sut.Last.TestEquals( KeyValuePair.Create( keys[1], values[1] ) ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldThrowArgumentException_WhenKeyAlreadyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new SequentialDictionary<TKey, TValue> { { key, values[0] } };

        var action = Lambda.Of( () => sut.Add( key, values[1] ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new SequentialDictionary<TKey, TValue>();

        var result = sut.Remove( key );

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveExistingItem_WhenDictionaryHasOneItem()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new SequentialDictionary<TKey, TValue> { { key, value } };

        var result = sut.Remove( key );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                sut.First.TestNull(),
                sut.Last.TestNull() )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveCorrectExistingItem()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new SequentialDictionary<TKey, TValue>
        {
            { keys[0], values[0] },
            { keys[1], values[1] }
        };

        var result = sut.Remove( keys[0] );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.ContainsKey( keys[1] ).TestTrue(),
                sut.First.TestEquals( KeyValuePair.Create( keys[1], values[1] ) ),
                sut.Last.TestEquals( KeyValuePair.Create( keys[1], values[1] ) ) )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldReturnFalseAndDefaultRemoved_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new SequentialDictionary<TKey, TValue>();

        var result = sut.Remove( key, out var removed );

        Assertion.All(
                result.TestFalse(),
                removed.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldReturnTrueAndRemoveExistingItem_WhenDictionaryHasOneItem()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new SequentialDictionary<TKey, TValue> { { key, value } };

        var result = sut.Remove( key, out var removed );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ),
                removed.TestEquals( value ),
                sut.First.TestNull(),
                sut.Last.TestNull() )
            .Go();
    }

    [Fact]
    public void Remove_WithRemoved_ShouldReturnTrueAndRemoveCorrectExistingItem()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new SequentialDictionary<TKey, TValue>
        {
            { keys[0], values[0] },
            { keys[1], values[1] }
        };

        var result = sut.Remove( keys[0], out var removed );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.ContainsKey( keys[1] ).TestTrue(),
                removed.TestEquals( values[0] ),
                sut.First.TestEquals( KeyValuePair.Create( keys[1], values[1] ) ),
                sut.Last.TestEquals( KeyValuePair.Create( keys[1], values[1] ) ) )
            .Go();
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrueAndCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new SequentialDictionary<TKey, TValue> { { key, value } };

        var result = sut.TryGetValue( key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalseAndDefaultResult_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new SequentialDictionary<TKey, TValue>();

        var result = sut.TryGetValue( key, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new SequentialDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.First.TestNull(),
                sut.Last.TestNull() )
            .Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectAndOrderedResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 10 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 10 );

        var initialItems = keys.Zip( values, KeyValuePair.Create ).Take( 7 );
        var keysToRemove = new[] { keys[0], keys[2], keys[6] };
        var itemsToAddLast = keys.Zip( values, KeyValuePair.Create ).Skip( 7 );

        var expected = new[] { keys[1], keys[3], keys[4], keys[5], keys[7], keys[8], keys[9] }.Zip(
            new[] { values[1], values[3], values[4], values[5], values[7], values[8], values[9] },
            KeyValuePair.Create );

        var sut = GetDictionaryForOrderVerification( initialItems, keysToRemove, itemsToAddLast ).AsEnumerable();

        sut.TestSequence( expected ).Go();
    }

    [Fact]
    public void Keys_ShouldReturnCorrectAndOrderedResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 10 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 10 );

        var initialItems = keys.Zip( values, KeyValuePair.Create ).Take( 7 );
        var keysToRemove = new[] { keys[0], keys[2], keys[6] };
        var itemsToAddLast = keys.Zip( values, KeyValuePair.Create ).Skip( 7 );

        var expected = new[] { keys[1], keys[3], keys[4], keys[5], keys[7], keys[8], keys[9] };

        var sut = GetDictionaryForOrderVerification( initialItems, keysToRemove, itemsToAddLast );

        var result = sut.Keys;

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void Values_ShouldReturnCorrectAndOrderedResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 10 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 10 );

        var initialItems = keys.Zip( values, KeyValuePair.Create ).Take( 7 );
        var keysToRemove = new[] { keys[0], keys[2], keys[6] };
        var itemsToAddLast = keys.Zip( values, KeyValuePair.Create ).Skip( 7 );

        var expected = new[] { values[1], values[3], values[4], values[5], values[7], values[8], values[9] };

        var sut = GetDictionaryForOrderVerification( initialItems, keysToRemove, itemsToAddLast );

        var result = sut.Values;

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void IndexerSet_ShouldAddNewItemToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new SequentialDictionary<TKey, TValue>();

        sut[key] = value;

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestEquals( value ),
                sut.First.TestEquals( KeyValuePair.Create( key, value ) ),
                sut.Last.TestEquals( KeyValuePair.Create( key, value ) ) )
            .Go();
    }

    [Fact]
    public void IndexerSet_ShouldAddNewItemToNonEmptyDictionaryCorrectly()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new SequentialDictionary<TKey, TValue> { { keys[0], values[0] } };

        sut[keys[1]] = values[1];

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut[keys[1]].TestEquals( values[1] ),
                sut.First.TestEquals( KeyValuePair.Create( keys[0], values[0] ) ),
                sut.Last.TestEquals( KeyValuePair.Create( keys[1], values[1] ) ) )
            .Go();
    }

    [Fact]
    public void IndexerSet_ShouldReplaceExistingItemCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new SequentialDictionary<TKey, TValue> { { key, values[0] } };

        sut[key] = values[1];

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestEquals( values[1] ),
                sut.First.TestEquals( KeyValuePair.Create( key, values[1] ) ),
                sut.Last.TestEquals( KeyValuePair.Create( key, values[1] ) ) )
            .Go();
    }

    [Fact]
    public void IndexerSet_ShouldNotChangeOrderOfItems_WhenReplacing()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 10 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 11 );

        var initialItems = keys.Zip( values, KeyValuePair.Create ).Take( 7 );
        var keysToRemove = new[] { keys[0], keys[2], keys[6] };
        var itemsToAddLast = keys.Zip( values, KeyValuePair.Create ).Skip( 7 ).Take( 3 );
        var keyToReplace = keys[4];

        var expected = new[] { keys[1], keys[3], keys[4], keys[5], keys[7], keys[8], keys[9] }.Zip(
            new[] { values[1], values[3], values[10], values[5], values[7], values[8], values[9] },
            KeyValuePair.Create );

        var sut = GetDictionaryForOrderVerification( initialItems, keysToRemove, itemsToAddLast );

        sut[keyToReplace] = values[10];

        sut.AsEnumerable().TestSequence( expected ).Go();
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
