using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.Collections.Tests;

public abstract class GenericDictionaryTestsBase<TKey, TValue> : GenericCollectionTestsBase<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    protected GenericDictionaryTestsBase()
    {
        Fixture.Customize<KeyValuePair<TKey, TValue>>( (_, _) => f => KeyValuePair.Create( f.Create<TKey>(), f.Create<TValue>() ) );
    }

    [Fact]
    public void IDictionaryKeys_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = CreateEmptyDictionary();
        sut.Add( keys[0], values[0] );
        sut.Add( keys[1], values[1] );
        sut.Add( keys[2], values[2] );

        var result = sut.Keys;

        Assertion.All( result.Count.TestEquals( keys.Length ), result.TestSetEqual( keys ) ).Go();
    }

    [Fact]
    public void IDictionaryValues_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = CreateEmptyDictionary();
        sut.Add( keys[0], values[0] );
        sut.Add( keys[1], values[1] );
        sut.Add( keys[2], values[2] );

        var result = sut.Values;

        Assertion.All( result.Count.TestEquals( values.Length ), result.TestSetEqual( values ) ).Go();
    }

    [Fact]
    public void IReadOnlyDictionaryKeys_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = CreateEmptyDictionary();
        sut.Add( keys[0], values[0] );
        sut.Add( keys[1], values[1] );
        sut.Add( keys[2], values[2] );

        var result = AsReadOnlyDictionary( sut ).Keys.ToList();

        Assertion.All( result.Count.TestEquals( keys.Length ), result.TestSetEqual( keys ) ).Go();
    }

    [Fact]
    public void IReadOnlyDictionaryValues_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateManyDistinct<TKey>( count: 3 );
        var values = Fixture.CreateManyDistinct<TValue>( count: 3 );
        var sut = CreateEmptyDictionary();
        sut.Add( keys[0], values[0] );
        sut.Add( keys[1], values[1] );
        sut.Add( keys[2], values[2] );

        var result = AsReadOnlyDictionary( sut ).Values.ToList();

        Assertion.All( result.Count.TestEquals( values.Length ), result.TestSetEqual( values ) ).Go();
    }

    [Fact]
    public void IDictionaryContainsKey_ShouldReturnTrue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();
        sut.Add( key, value );

        var result = sut.ContainsKey( key );

        result.TestTrue().Go();
    }

    [Fact]
    public void IDictionaryContainsKey_ShouldReturnFalse_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = CreateEmptyDictionary();

        var result = sut.ContainsKey( key );

        result.TestFalse().Go();
    }

    [Fact]
    public void IReadOnlyDictionaryContainsKey_ShouldReturnTrue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();
        sut.Add( key, value );

        var result = AsReadOnlyDictionary( sut ).ContainsKey( key );

        result.TestTrue().Go();
    }

    [Fact]
    public void IReadOnlyDictionaryContainsKey_ShouldReturnFalse_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = CreateEmptyDictionary();

        var result = AsReadOnlyDictionary( sut ).ContainsKey( key );

        result.TestFalse().Go();
    }

    [Fact]
    public void IDictionaryTryGetValue_ShouldReturnTrueAndCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();
        sut.Add( key, value );

        var result = sut.TryGetValue( key, out var outResult );

        Assertion.All( result.TestTrue(), outResult.TestEquals( value ) ).Go();
    }

    [Fact]
    public void IDictionaryTryGetValue_ShouldReturnFalseAndDefaultResult_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = CreateEmptyDictionary();

        var result = sut.TryGetValue( key, out var outResult );

        Assertion.All( result.TestFalse(), outResult.TestEquals( default ) ).Go();
    }

    [Fact]
    public void IReadOnlyDictionaryTryGetValue_ShouldReturnTrueAndCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();
        sut.Add( key, value );

        var result = AsReadOnlyDictionary( sut ).TryGetValue( key, out var outResult );

        Assertion.All( result.TestTrue(), outResult.TestEquals( value ) ).Go();
    }

    [Fact]
    public void IReadOnlyDictionaryTryGetValue_ShouldReturnFalseAndDefaultResult_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = CreateEmptyDictionary();

        var result = AsReadOnlyDictionary( sut ).TryGetValue( key, out var outResult );

        Assertion.All( result.TestFalse(), outResult.TestEquals( default ) ).Go();
    }

    [Fact]
    public void ICollectionAdd_ShouldAddItemCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();

        sut.Add( KeyValuePair.Create( key, value ) );

        sut[key].TestEquals( value ).Go();
    }

    [Fact]
    public void ICollectionRemove_ShouldReturnFalse_WhenKeyExistsButValueDoesNot()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = CreateEmptyDictionary();
        sut.Add( key, values[0] );

        var result = sut.Remove( KeyValuePair.Create( key, values[1] ) );

        Assertion.All( result.TestFalse(), sut.Count.TestEquals( 1 ) ).Go();
    }

    [Fact]
    public void ICollectionRemove_ShouldReturnTrueAndRemoveItemCorrectly_WhenItemExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();
        sut.Add( key, value );

        var result = sut.Remove( key );

        Assertion.All( result.TestTrue(), sut.ContainsKey( key ).TestFalse() ).Go();
    }

    [Fact]
    public void ICollectionContains_ShouldReturnFalse_WhenKeyExistsButValueDoesNot()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = CreateEmptyDictionary();
        sut.Add( key, values[0] );

        var result = sut.Contains( KeyValuePair.Create( key, values[1] ) );

        result.TestFalse().Go();
    }

    protected abstract IDictionary<TKey, TValue> CreateEmptyDictionary();

    protected virtual IReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
    {
        return dictionary as IReadOnlyDictionary<TKey, TValue> ?? throw new NullReferenceException();
    }

    protected sealed override ICollection<KeyValuePair<TKey, TValue>> CreateEmptyCollection()
    {
        return CreateEmptyDictionary();
    }
}
