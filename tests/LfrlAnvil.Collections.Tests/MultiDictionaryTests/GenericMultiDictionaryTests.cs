using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Collections.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Collections.Tests.MultiDictionaryTests;

public abstract class GenericMultiDictionaryTests<TKey, TValue> : TestsBase
    where TKey : notnull
{
    [Fact]
    public void Ctor_ShouldCreateEmpty()
    {
        var sut = new MultiDictionary<TKey, TValue>();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestEquals( EqualityComparer<TKey>.Default ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateEmpty_WithExplicitComparer()
    {
        var comparer = EqualityComparerFactory<TKey>.Create( (a, b) => a!.Equals( b ) );
        var sut = new MultiDictionary<TKey, TValue>( comparer );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Comparer.TestEquals( comparer ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();

        sut.Add( key, value );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestSequence( [ value ] ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemToNonEmptyDictionaryCorrectly_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( keys[0], values[0] );

        sut.Add( keys[1], values[1] );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut[keys[1]].TestSequence( [ values[1] ] ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldAddNewItemToNonEmptyDictionaryCorrectly_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, values[0] );

        sut.Add( key, values[1] );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void AddRange_ShouldAddNewItemsToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        sut.AddRange( key, values );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void AddRange_ShouldAddNewItemsToNonEmptyDictionaryCorrectly_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var allValues = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( keys[0], allValues[0] );

        sut.AddRange( keys[1], values );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut[keys[1]].TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void AddRange_ShouldAddNewItemsToNonEmptyDictionaryCorrectly_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var allValues = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, allValues[0] );

        sut.AddRange( key, values );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestSequence( allValues ) )
            .Go();
    }

    [Fact]
    public void AddRange_ShouldDoNothing_WhenValuesAreEmpty()
    {
        var key = Fixture.Create<TKey>();
        var values = Enumerable.Empty<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();

        sut.AddRange( key, values );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.ContainsKey( key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void SetRange_ShouldAddNewItemsToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        sut.SetRange( key, values );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void SetRange_ShouldAddNewItemsToNonEmptyDictionaryCorrectly_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var allValues = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( keys[0], allValues[0] );

        sut.SetRange( keys[1], values );

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut[keys[1]].TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void SetRange_ShouldReplaceExistingItemsInNonEmptyDictionaryCorrectly_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var allValues = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, allValues[0] );

        sut.SetRange( key, values );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestSequence( values ) )
            .Go();
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

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.ContainsKey( key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnEmptyList_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.Remove( key );

        result.TestEmpty().Go();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveExistingItems_WhenDictionaryHasOneKey()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.Remove( key );

        Assertion.All(
                result.TestSequence( values ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnTrueAndRemoveCorrectExistingItems()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( keys[0], values.Take( 2 ) );
        sut.AddRange( keys[1], values.Skip( 2 ) );

        var result = sut.Remove( keys[0] );

        Assertion.All(
                result.TestSequence( values.Take( 2 ) ),
                sut.Count.TestEquals( 1 ),
                sut.ContainsKey( keys[1] ).TestTrue() )
            .Go();
    }

    [Fact]
    public void Remove_WithValue_ShouldReturnFalse_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.Remove( key, value );

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_WithValue_ShouldReturnTrueAndRemoveExistingItem_WhenKeyAndItemExist()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.Remove( key, values[1] );

        Assertion.All(
                result.TestTrue(),
                sut[key].TestSequence( [ values[0], values[2] ] ) )
            .Go();
    }

    [Fact]
    public void Remove_WithValue_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values.Take( 2 ) );

        var result = sut.Remove( key, values[2] );

        Assertion.All(
                result.TestFalse(),
                sut[key].TestSequence( values.Take( 2 ) ) )
            .Go();
    }

    [Fact]
    public void Remove_WithValue_ShouldReturnTrueAndRemoveKey_WhenItemExistsAndListOnlyContainsOneItem()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, value );

        var result = sut.Remove( key, value );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void RemoveAt_ShouldReturnFalse_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.RemoveAt( key, index: 0 );

        result.TestFalse().Go();
    }

    [Fact]
    public void RemoveAt_ShouldReturnTrueAndRemoveExistingItem_WhenKeyAndIndexExist()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveAt( key, index: 1 );

        Assertion.All(
                result.TestTrue(),
                sut[key].TestSequence( [ values[0], values[2] ] ) )
            .Go();
    }

    [Fact]
    public void RemoveAt_ShouldReturnTrueAndRemoveKey_WhenIndexExistsAndListOnlyContainsOneItem()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, value );

        var result = sut.RemoveAt( key, index: 0 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 3 )]
    public void RemoveAt_ShouldThrowArgumentOutOfRangeException_WhenIndexDoesNotExist(int index)
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var action = Lambda.Of( () => sut.RemoveAt( key, index ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void RemoveRange_ShouldReturnFalse_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.RemoveRange( key, index: 0, count: 0 );

        result.TestFalse().Go();
    }

    [Fact]
    public void RemoveRange_ShouldReturnTrueAndRemoveExistingItems_WhenKeyAndRangeExist()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveRange( key, index: 1, count: 2 );

        Assertion.All(
                result.TestTrue(),
                sut[key].TestSequence( [ values[0], values[3] ] ) )
            .Go();
    }

    [Fact]
    public void RemoveRange_ShouldReturnTrueAndRemoveKey_WhenRangeExistsAndAllListItemsAreRemoved()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveRange( key, index: 0, count: 3 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Theory]
    [InlineData( -1, 1 )]
    [InlineData( 0, -1 )]
    public void RemoveRange_ShouldThrowArgumentOutOfRangeException_WhenIndexOrCountIsNegative(int index, int count)
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var action = Lambda.Of( () => sut.RemoveRange( key, index, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0, 4 )]
    [InlineData( 2, 2 )]
    [InlineData( 3, 1 )]
    public void RemoveRange_ShouldThrowArgumentException_WhenIndexAndCountDenoteInvalidRange(int index, int count)
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var action = Lambda.Of( () => sut.RemoveRange( key, index, count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void RemoveAll_ShouldReturnZero_WhenDictionaryIsEmpty()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.RemoveAll( key, _ => true );

        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void RemoveAll_ShouldReturnAmountOfRemovedItems_WhenKeyExistsAndPredicateFindsItemsToRemove()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveAll( key, v => v!.Equals( values[0] ) || v.Equals( values[1] ) );

        Assertion.All(
                result.TestEquals( 2 ),
                sut[key].TestSequence( [ values[2] ] ) )
            .Go();
    }

    [Fact]
    public void RemoveAll_ShouldReturnZero_WhenPredicateDoesNotFindItemsToRemove()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveAll( key, _ => false );

        Assertion.All(
                result.TestEquals( 0 ),
                sut[key].TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void RemoveAll_ShouldReturnAmountOfRemovedItemsAndRemoveKey_WhenAllListItemsPassThePredicate()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.RemoveAll( key, _ => true );

        Assertion.All(
                result.TestEquals( 3 ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrueAndCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.TryGetValue( key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                (outResult ?? Array.Empty<TValue>()).TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalseAndNullResult_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.TryGetValue( key, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void GetCount_ShouldReturnCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();
        sut.AddRange( key, values );

        var result = sut.GetCount( key );

        result.TestEquals( 3 ).Go();
    }

    [Fact]
    public void GetCount_ShouldReturnZero_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut.GetCount( key );

        result.TestEquals( 0 ).Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        sut.Clear();

        sut.Count.TestEquals( 0 ).Go();
    }

    [Fact]
    public void IndexerGet_ShouldReturnEmptyCollection_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new MultiDictionary<TKey, TValue>();

        var result = sut[key];

        result.TestEmpty().Go();
    }

    [Fact]
    public void IndexerSet_ShouldAddNewItemsToEmptyDictionaryCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        sut[key] = values;

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void IndexerSet_ShouldAddNewItemsToNonEmptyDictionaryCorrectly_WhenKeyDoesntExist()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var allValues = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( keys[0], allValues[0] );

        sut[keys[1]] = values;

        Assertion.All(
                sut.Count.TestEquals( 2 ),
                sut[keys[1]].TestSequence( values ) )
            .Go();
    }

    [Fact]
    public void IndexerSet_ShouldReplaceExistingItemsInNonEmptyDictionaryCorrectly_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var allValues = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var sut = new MultiDictionary<TKey, TValue>();
        sut.Add( key, allValues[0] );

        sut[key] = values;

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestSequence( values ) )
            .Go();
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

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.ContainsKey( key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void GetEnumerator_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        var result = sut.AsEnumerable().ToList();

        Assertion.All(
                result.Count.TestEquals( 3 ),
                result.SelectMany( kv => kv.Value.Select( v => KeyValuePair.Create( kv.Key, v ) ) )
                    .TestSetEqual( keys.Zip( values ).Select( x => KeyValuePair.Create( x.First, x.Second ) ) ) )
            .Go();
    }

    [Fact]
    public void Keys_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        var result = sut.Keys.ToList();

        Assertion.All(
                result.Count.TestEquals( 3 ),
                result.TestSetEqual( keys ) )
            .Go();
    }

    [Fact]
    public void Values_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        var result = sut.Values.ToList();

        Assertion.All(
                result.Count.TestEquals( 3 ),
                result.SelectMany( v => v ).TestSetEqual( values ) )
            .Go();
    }

    [Fact]
    public void ILookup_IndexerGet_ShouldReturnEmptyCollection_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<TKey>();
        ILookup<TKey, TValue> sut = new MultiDictionary<TKey, TValue>();

        var result = sut[key];

        result.TestEmpty().Go();
    }

    [Fact]
    public void ILookup_IndexerGet_ShouldReturnCorrectCollection_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var source = new MultiDictionary<TKey, TValue>();
        source.AddRange( key, values );
        ILookup<TKey, TValue> sut = source;

        var result = sut[key];

        result.TestSequence( values ).Go();
    }

    [Fact]
    public void ILookup_Contains_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var key = Fixture.Create<TKey>();
        ILookup<TKey, TValue> sut = new MultiDictionary<TKey, TValue>();

        var result = sut.Contains( key );

        result.TestFalse().Go();
    }

    [Fact]
    public void ILookup_Contains_ShouldReturnTrue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var source = new MultiDictionary<TKey, TValue>();
        source.AddRange( key, values );
        ILookup<TKey, TValue> sut = source;

        var result = sut.Contains( key );

        result.TestTrue().Go();
    }

    [Fact]
    public void ILookup_GetEnumerator_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = new MultiDictionary<TKey, TValue>();

        foreach ( var (k, v) in keys.Zip( values ) )
            sut.Add( k, v );

        var result = (( ILookup<TKey, TValue> )sut).ToList();

        Assertion.All(
                result.Count.TestEquals( 3 ),
                result.SelectMany( g => g.Select( v => KeyValuePair.Create( g.Key, v ) ) )
                    .TestSetEqual( keys.Zip( values ).Select( x => KeyValuePair.Create( x.First, x.Second ) ) ) )
            .Go();
    }

    [Fact]
    public void IDictionaryKeys_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var source = new MultiDictionary<TKey, TValue>();
        IDictionary<TKey, IReadOnlyList<TValue>> sut = source;
        source.Add( keys[0], values[0] );
        source.Add( keys[1], values[1] );
        source.Add( keys[2], values[2] );

        var result = sut.Keys;

        result.TestSetEqual( keys ).Go();
    }

    [Fact]
    public void IDictionaryValues_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var source = new MultiDictionary<TKey, TValue>();
        IDictionary<TKey, IReadOnlyList<TValue>> sut = source;
        source.Add( keys[0], values[0] );
        source.Add( keys[1], values[1] );
        source.Add( keys[2], values[2] );

        var result = sut.Values;

        result.SelectMany( v => v ).TestSetEqual( values ).Go();
    }

    [Fact]
    public void IReadOnlyDictionaryKeys_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var source = new MultiDictionary<TKey, TValue>();
        IReadOnlyDictionary<TKey, IReadOnlyList<TValue>> sut = source;
        source.Add( keys[0], values[0] );
        source.Add( keys[1], values[1] );
        source.Add( keys[2], values[2] );

        var result = sut.Keys;

        result.TestSetEqual( keys ).Go();
    }

    [Fact]
    public void IReadOnlyDictionaryValues_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var source = new MultiDictionary<TKey, TValue>();
        IReadOnlyDictionary<TKey, IReadOnlyList<TValue>> sut = source;
        source.Add( keys[0], values[0] );
        source.Add( keys[1], values[1] );
        source.Add( keys[2], values[2] );

        var result = sut.Values;

        result.SelectMany( v => v ).TestSetEqual( values ).Go();
    }

    [Fact]
    public void IDictionaryAdd_ShouldBeEquivalentToAddRange()
    {
        var key = Fixture.Create<TKey>();
        var allValues = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var source = new MultiDictionary<TKey, TValue>();
        IDictionary<TKey, IReadOnlyList<TValue>> sut = source;
        source.Add( key, allValues[0] );

        sut.Add( key, values );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                sut[key].TestSequence( allValues ) )
            .Go();
    }

    [Fact]
    public void IDictionaryRemove_ShouldBeEquivalentToRemove()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var source = new MultiDictionary<TKey, TValue>();
        IDictionary<TKey, IReadOnlyList<TValue>> sut = source;
        source.AddRange( keys[0], values.Take( 2 ) );
        source.AddRange( keys[1], values.Skip( 2 ) );

        var result = sut.Remove( keys[0] );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.ContainsKey( keys[1] ).TestTrue() )
            .Go();
    }

    [Fact]
    public void ICollectionAdd_ShouldBeEquivalentToAddRange()
    {
        var key = Fixture.Create<TKey>();
        var allValues = Fixture.CreateManyDistinct<TValue>( count: 4 );
        var values = allValues.Skip( 1 ).ToList();
        var source = new MultiDictionary<TKey, TValue>();
        ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>> sut = source;
        source.Add( key, allValues[0] );

        sut.Add( KeyValuePair.Create( key, ( IReadOnlyList<TValue> )values ) );

        Assertion.All(
                sut.Count.TestEquals( 1 ),
                source[key].TestSequence( allValues ) )
            .Go();
    }

    [Fact]
    public void ICollectionRemove_ShouldBeEquivalentToRemoveAll_AndReturnTrueIfAtLeastOneElementWasRemoved()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 2 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 6 );
        var source = new MultiDictionary<TKey, TValue>();
        ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>> sut = source;
        source.AddRange( keys[0], values.Take( 3 ) );
        source.AddRange( keys[1], values.Skip( 3 ) );

        var result = sut.Remove( KeyValuePair.Create( keys[0], ( IReadOnlyList<TValue> )new[] { values[0], values[2] } ) );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                source[keys[0]].TestSequence( [ values[1] ] ) )
            .Go();
    }

    [Fact]
    public void ICollectionContains_ShouldReturnFalse_WhenAnyElementDoesNotExistInValuesUnderTheSpecifiedKey()
    {
        var key = Fixture.Create<TKey>();
        var values = ( IReadOnlyList<TValue> )Fixture.CreateManyDistinct<TValue>( count: 4 );
        var source = new MultiDictionary<TKey, TValue>();
        ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>> sut = source;
        source.AddRange( key, values.Take( 3 ) );

        var result = sut.Contains( KeyValuePair.Create( key, values ) );

        result.TestFalse().Go();
    }

    [Fact]
    public void ICollectionContains_ShouldReturnTrue_WhenAllElementsExistInValuesUnderTheSpecifiedKey()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var source = new MultiDictionary<TKey, TValue>();
        ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>> sut = source;
        source.AddRange( key, values );

        var result = sut.Contains( KeyValuePair.Create( key, ( IReadOnlyList<TValue> )new[] { values[0], values[2] } ) );

        result.TestTrue().Go();
    }
}
