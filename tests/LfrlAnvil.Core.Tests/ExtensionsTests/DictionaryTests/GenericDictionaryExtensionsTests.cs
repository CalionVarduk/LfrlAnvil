using System.Collections.Generic;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.DictionaryTests;

public abstract class GenericDictionaryExtensionsTests<TKey, TValue> : TestsBase
    where TKey : notnull
{
    [Fact]
    public void GetOrAddDefault_ShouldReturnExistingValue()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new Dictionary<TKey, TValue?> { { key, value } };

        var result = sut.GetOrAddDefault( key );

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetOrAddDefault_ShouldAddAndReturnDefaultValue_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new Dictionary<TKey, TValue?>();

        var result = sut.GetOrAddDefault( key );

        Assertion.All(
                result.TestEquals( default ),
                sut[key].TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void GetOrAdd_ShouldReturnExistingValue()
    {
        var key = Fixture.Create<TKey>();
        var (value, providedValue) = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new Dictionary<TKey, TValue> { { key, value } };

        var result = sut.GetOrAdd( key, () => providedValue );

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetOrAdd_ShouldAddAndReturnProvidedValue_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var providedValue = Fixture.Create<TValue>();
        var sut = new Dictionary<TKey, TValue>();

        var result = sut.GetOrAdd( key, () => providedValue );

        Assertion.All(
                result.TestEquals( providedValue ),
                sut[key].TestEquals( providedValue ) )
            .Go();
    }

    [Fact]
    public void GetOrAdd_WithLazy_ShouldReturnExistingValue()
    {
        var key = Fixture.Create<TKey>();
        var (value, providedValue) = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new Dictionary<TKey, TValue> { { key, value } };

        var result = sut.GetOrAdd( key, new Lazy<TValue>( () => providedValue ) );

        result.TestEquals( value ).Go();
    }

    [Fact]
    public void GetOrAdd_WithLazy_ShouldAddAndReturnProvidedValue_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var providedValue = Fixture.Create<TValue>();
        var sut = new Dictionary<TKey, TValue>();

        var result = sut.GetOrAdd( key, new Lazy<TValue>( () => providedValue ) );

        Assertion.All(
                result.TestEquals( providedValue ),
                sut[key].TestEquals( providedValue ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddValue_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new Dictionary<TKey, TValue>();

        var result = sut.AddOrUpdate( key, value );

        Assertion.All(
                result.TestEquals( AddOrUpdateResult.Added ),
                sut[key].TestEquals( value ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateValue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var (oldValue, newValue) = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new Dictionary<TKey, TValue> { { key, oldValue } };

        var result = sut.AddOrUpdate( key, newValue );

        Assertion.All(
                result.TestEquals( AddOrUpdateResult.Updated ),
                sut[key].TestEquals( newValue ) )
            .Go();
    }

    [Fact]
    public void TryUpdate_ShouldReturnFalse_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new Dictionary<TKey, TValue>();

        var result = sut.TryUpdate( key, value );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void TryUpdate_ShouldReturnTrue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var (oldValue, newValue) = Fixture.CreateManyDistinct<TValue>( count: 2 );
        var sut = new Dictionary<TKey, TValue> { { key, oldValue } };

        var result = sut.TryUpdate( key, newValue );

        Assertion.All(
                result.TestTrue(),
                sut[key].TestEquals( newValue ) )
            .Go();
    }
}
