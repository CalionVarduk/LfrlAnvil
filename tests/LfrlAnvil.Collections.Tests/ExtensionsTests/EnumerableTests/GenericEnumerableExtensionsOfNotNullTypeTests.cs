using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Collections.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Collections.Tests.ExtensionsTests.EnumerableTests;

public abstract class GenericEnumerableExtensionsOfNotNullTypeTests<T> : TestsBase
    where T : notnull
{
    [Fact]
    public void ToMultiHashSet_ShouldReturnCorrectResult()
    {
        var distinctItems = Fixture.CreateDistinctCollection<T>( 5 );
        var items = distinctItems.SelectMany( i => new[] { i, i, i, i } ).ToList();
        var expected = distinctItems.Select( i => Pair.Create( i, 4 ) ).ToList();

        var result = items.ToMultiHashSet();

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ToMultiDictionary_WithKeySelector_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateDistinctCollection<T>( 5 ).Select( i => new Value<T>( i ) ).ToList();
        items = items.Concat( items ).ToList();
        var expected = items.Select( i => KeyValuePair.Create( i.Val, i ) );

        var result = items.ToMultiDictionary( i => i.Val );

        result.AsEnumerable()
            .SelectMany( kv => kv.Value.Select( v => KeyValuePair.Create( kv.Key, v ) ) )
            .Should()
            .BeEquivalentTo( expected );
    }

    [Fact]
    public void ToMultiDictionary_WithKeyAndValueSelectors_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateDistinctCollection<T>( 5 ).Select( i => new Value<T>( i ) ).ToList();
        items = items.Concat( items ).ToList();
        var expected = items.Select( i => KeyValuePair.Create( i.Val, i ) );

        var result = items.ToMultiDictionary( i => i.Val, i => i );

        result.AsEnumerable()
            .SelectMany( kv => kv.Value.Select( v => KeyValuePair.Create( kv.Key, v ) ) )
            .Should()
            .BeEquivalentTo( expected );
    }

    [Fact]
    public void ToMultiDictionary_FromKeyValuePairs_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateDistinctCollection<T>( 5 ).Select( i => new Value<T>( i ) ).ToList();
        items = items.Concat( items ).ToList();
        var expected = items.Select( i => KeyValuePair.Create( i.Val, i ) ).ToList();

        var result = expected.ToMultiDictionary();

        result.AsEnumerable()
            .SelectMany( kv => kv.Value.Select( v => KeyValuePair.Create( kv.Key, v ) ) )
            .Should()
            .BeEquivalentTo( expected );
    }

    [Fact]
    public void ToMultiDictionary_FromGroupings_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateDistinctCollection<T>( 5 ).Select( i => new Value<T>( i ) ).ToList();
        items = items.Concat( items ).ToList();
        var expected = items.Select( i => KeyValuePair.Create( i.Val, i ) ).ToList();
        var groupings = items.ToLookup( i => i.Val );

        var result = groupings.ToMultiDictionary();

        result.AsEnumerable()
            .SelectMany( kv => kv.Value.Select( v => KeyValuePair.Create( kv.Key, v ) ) )
            .Should()
            .BeEquivalentTo( expected );
    }

    [Fact]
    public void ToSequentialHashSet_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateDistinctCollection<T>( 5 );
        var result = items.ToSequentialHashSet();
        result.Should().BeSequentiallyEqualTo( items );
    }

    [Fact]
    public void ToSequentialDictionary_ShouldReturnCorrectResult()
    {
        var items = Fixture.CreateDistinctCollection<T>( 5 ).Select( i => new Value<T>( i ) ).ToList();
        var expected = items.Select( i => KeyValuePair.Create( i.Val, i ) );

        var result = items.ToSequentialDictionary( i => i.Val );

        result.AsEnumerable().Should().BeSequentiallyEqualTo( expected );
    }

    [Fact]
    public void ToSequentialDictionary_ShouldReturnCorrectResult_WithValueSelector()
    {
        var items = Fixture.CreateDistinctCollection<T>( 5 );
        var expected = items.Select( i => KeyValuePair.Create( i, new Value<T>( i ) ) );

        var result = items.ToSequentialDictionary( i => i, i => new Value<T>( i ) );

        result.AsEnumerable().Should().BeSequentiallyEqualTo( expected );
    }
}

internal record Value<T>(T Val);
