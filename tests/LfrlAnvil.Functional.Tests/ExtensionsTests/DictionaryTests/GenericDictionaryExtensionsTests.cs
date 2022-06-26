using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

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

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryGetValue_ShouldReturnWithValue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new Dictionary<TKey, TValue> { { key, value } };

        var result = sut.TryGetValue( key );

        result.Value.Should().Be( value );
    }

    [Fact]
    public void TryRemove_ShouldReturnNone_WhenKeyDoesntExist()
    {
        var key = Fixture.Create<TKey>();
        var sut = new Dictionary<TKey, TValue>();

        var result = sut.TryRemove( key );

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryRemove_ShouldReturnWithValue_WhenKeyExists()
    {
        var key = Fixture.Create<TKey>();
        var value = Fixture.Create<TValue>();
        var sut = new Dictionary<TKey, TValue> { { key, value } };

        var result = sut.TryRemove( key );

        using ( new AssertionScope() )
        {
            result.Value.Should().Be( value );
            sut.Should().HaveCount( 0 );
        }
    }
}