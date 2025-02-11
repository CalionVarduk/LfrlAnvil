using System.Collections.Generic;
using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.DictionaryTests;

public abstract class GenericDictionaryExtensionsTests<TKey, TValue> : TestsBase
    where TKey : notnull
    where TValue : notnull
{
    [Fact]
    public void TryGetValue_ShouldReturnNone_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new Dictionary<TKey, TValue>();

        var result = sut.TryGetValue( key );

        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TryGetValue_ShouldReturnWithValue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new Dictionary<TKey, TValue> { { key, value } };

        var result = sut.TryGetValue( key );

        result.Value.TestEquals( value ).Go();
    }

    [Fact]
    public void TryRemove_ShouldReturnNone_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new Dictionary<TKey, TValue>();

        var result = sut.TryRemove( key );

        result.HasValue.TestFalse().Go();
    }

    [Fact]
    public void TryRemove_ShouldReturnWithValue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new Dictionary<TKey, TValue> { { key, value } };

        var result = sut.TryRemove( key );

        Assertion.All(
                result.Value.TestEquals( value ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }
}
