using System.Collections.Generic;

namespace LfrlAnvil.Collections.Tests;

public abstract class GenericDictionaryTestsBase<TKey, TValue> : GenericCollectionTestsBase<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    [Fact]
    public void IDictionaryKeys_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( 3 );
        var sut = CreateEmptyDictionary();
        sut.Add( keys[0], values[0] );
        sut.Add( keys[1], values[1] );
        sut.Add( keys[2], values[2] );

        var result = sut.Keys;

        result.Should().BeEquivalentTo( keys );
    }

    [Fact]
    public void IDictionaryValues_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( 3 );
        var sut = CreateEmptyDictionary();
        sut.Add( keys[0], values[0] );
        sut.Add( keys[1], values[1] );
        sut.Add( keys[2], values[2] );

        var result = sut.Values;

        result.Should().BeEquivalentTo( values );
    }

    [Fact]
    public void IReadOnlyDictionaryKeys_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( 3 );
        var sut = CreateEmptyDictionary();
        sut.Add( keys[0], values[0] );
        sut.Add( keys[1], values[1] );
        sut.Add( keys[2], values[2] );

        var result = AsReadOnlyDictionary( sut ).Keys;

        result.Should().BeEquivalentTo( keys );
    }

    [Fact]
    public void IReadOnlyDictionaryValues_ShouldReturnCorrectResult()
    {
        var keys = Fixture.CreateDistinctCollection<TKey>( 3 );
        var values = Fixture.CreateDistinctCollection<TValue>( 3 );
        var sut = CreateEmptyDictionary();
        sut.Add( keys[0], values[0] );
        sut.Add( keys[1], values[1] );
        sut.Add( keys[2], values[2] );

        var result = AsReadOnlyDictionary( sut ).Values;

        result.Should().BeEquivalentTo( values );
    }

    [Fact]
    public void IDictionaryContainsKey_ShouldReturnTrue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();
        sut.Add( key, value );

        var result = sut.ContainsKey( key );

        result.Should().BeTrue();
    }

    [Fact]
    public void IDictionaryContainsKey_ShouldReturnFalse_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = CreateEmptyDictionary();

        var result = sut.ContainsKey( key );

        result.Should().BeFalse();
    }

    [Fact]
    public void IReadOnlyDictionaryContainsKey_ShouldReturnTrue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();
        sut.Add( key, value );

        var result = AsReadOnlyDictionary( sut ).ContainsKey( key );

        result.Should().BeTrue();
    }

    [Fact]
    public void IReadOnlyDictionaryContainsKey_ShouldReturnFalse_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = CreateEmptyDictionary();

        var result = AsReadOnlyDictionary( sut ).ContainsKey( key );

        result.Should().BeFalse();
    }

    [Fact]
    public void IDictionaryTryGetValue_ShouldReturnTrueAndCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();
        sut.Add( key, value );

        var result = sut.TryGetValue( key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( value );
        }
    }

    [Fact]
    public void IDictionaryTryGetValue_ShouldReturnFalseAndDefaultResult_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = CreateEmptyDictionary();

        var result = sut.TryGetValue( key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default( TValue ) );
        }
    }

    [Fact]
    public void IReadOnlyDictionaryTryGetValue_ShouldReturnTrueAndCorrectResult_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();
        sut.Add( key, value );

        var result = AsReadOnlyDictionary( sut ).TryGetValue( key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( value );
        }
    }

    [Fact]
    public void IReadOnlyDictionaryTryGetValue_ShouldReturnFalseAndDefaultResult_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = CreateEmptyDictionary();

        var result = AsReadOnlyDictionary( sut ).TryGetValue( key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default( TValue ) );
        }
    }

    [Fact]
    public void ICollectionAdd_ShouldAddItemCorrectly()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();

        sut.Add( KeyValuePair.Create( key, value ) );

        sut[key].Should().Be( value );
    }

    [Fact]
    public void ICollectionRemove_ShouldReturnFalse_WhenKeyExistsButValueDoesNot()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( 2 );
        var sut = CreateEmptyDictionary();
        sut.Add( key, values[0] );

        var result = sut.Remove( KeyValuePair.Create( key, values[1] ) );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 1 );
        }
    }

    [Fact]
    public void ICollectionRemove_ShouldReturnTrueAndRemoveItemCorrectly_WhenItemExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = CreateEmptyDictionary();
        sut.Add( key, value );

        var result = sut.Remove( key );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.ContainsKey( key ).Should().BeFalse();
        }
    }

    [Fact]
    public void ICollectionContains_ShouldReturnFalse_WhenKeyExistsButValueDoesNot()
    {
        var key = Fixture.Create<TKey>();
        var values = Fixture.CreateDistinctCollection<TValue>( 2 );
        var sut = CreateEmptyDictionary();
        sut.Add( key, values[0] );

        var result = sut.Contains( KeyValuePair.Create( key, values[1] ) );

        result.Should().BeFalse();
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
