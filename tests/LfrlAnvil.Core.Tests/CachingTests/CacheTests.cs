using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Caching;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.CachingTests;

public class CacheTests : TestsBase
{
    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void Ctor_ShouldCreateEmptyWithCorrectCapacity(int capacity)
    {
        var sut = new Cache<string, int>( capacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( capacity ),
                sut.Comparer.TestRefEquals( EqualityComparer<string>.Default ),
                sut.Oldest.TestNull(),
                sut.Newest.TestNull() )
            .Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void Ctor_ShouldCreateEmptyWithCorrectCapacity_WithExplicitComparer(int capacity)
    {
        var comparer = EqualityComparerFactory<string>.Create( (a, b) => a!.Equals( b ) );
        var sut = new Cache<string, int>( comparer, capacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( capacity ),
                sut.Comparer.TestRefEquals( comparer ),
                sut.Oldest.TestNull(),
                sut.Newest.TestNull() )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCapacityIsLessThanOne(int capacity)
    {
        var action = Lambda.Of( () => new Cache<string, int>( capacity ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void TryAdd_ShouldAddFirstItemCorrectly()
    {
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new Cache<string, int>( capacity: 3 );

        var result = sut.TryAdd( entry.Key, entry.Value );

        Assertion.All(
                result.TestTrue(),
                AssertCollection( sut, entry ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldReturnFalse_WhenKeyAlreadyExists()
    {
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );

        var result = sut.TryAdd( entry2.Key, entry2.Value );

        Assertion.All(
                result.TestFalse(),
                AssertCollection( sut, entry1 ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldAddItemsToFullCapacityCorrectly()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        AssertCollection( sut, entry1, entry2, entry3 ).Go();
    }

    [Fact]
    public void TryAdd_ShouldAddNewItemAndRemoveOldestItem_WhenCapacityIsExceeded()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.TryAdd( entry4.Key, entry4.Value );

        Assertion.All(
                AssertCollection( sut, entry2, entry3, entry4 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldAddNewItemAndRemoveOldestItem_WhenCapacityIsExceeded_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new Cache<string, int>( capacity: 3, removeCallback: removed.Add );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.TryAdd( entry4.Key, entry4.Value );

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) ] ),
                AssertCollection( sut, entry2, entry3, entry4 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddFirstItemCorrectly()
    {
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new Cache<string, int>( capacity: 3 );

        var result = sut.AddOrUpdate( entry.Key, entry.Value );

        Assertion.All(
                result.TestEquals( AddOrUpdateResult.Added ),
                AssertCollection( sut, entry ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateValue_WhenKeyAlreadyExists()
    {
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );

        var result = sut.AddOrUpdate( entry2.Key, entry2.Value );

        Assertion.All(
                result.TestEquals( AddOrUpdateResult.Updated ),
                AssertCollection( sut, entry2 ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateValue_WhenKeyAlreadyExists_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new Cache<string, int>( capacity: 3, removeCallback: removed.Add );
        sut.TryAdd( entry1.Key, entry1.Value );

        var result = sut.AddOrUpdate( entry2.Key, entry2.Value );

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateReplaced( entry1.Key, entry1.Value, entry2.Value ) ] ),
                result.TestEquals( AddOrUpdateResult.Updated ),
                AssertCollection( sut, entry2 ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddItemsToFullCapacityCorrectly()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );

        sut.AddOrUpdate( entry1.Key, entry1.Value );
        sut.AddOrUpdate( entry2.Key, entry2.Value );
        sut.AddOrUpdate( entry3.Key, entry3.Value );

        AssertCollection( sut, entry1, entry2, entry3 ).Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddNewItemAndRemoveOldestItem_WhenCapacityIsExceeded()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.AddOrUpdate( entry4.Key, entry4.Value );

        Assertion.All(
                AssertCollection( sut, entry2, entry3, entry4 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddNewItemAndRemoveOldestItem_WhenCapacityIsExceeded_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new Cache<string, int>( capacity: 3, removeCallback: removed.Add );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.AddOrUpdate( entry4.Key, entry4.Value );

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) ] ),
                AssertCollection( sut, entry2, entry3, entry4 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateExistingValueAndSetEntryAsNewest_WhenKeyAlreadyExistsAndIsNotNewest()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.AddOrUpdate( entry4.Key, entry4.Value );

        AssertCollection( sut, entry2, entry3, entry4 ).Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldAddFirstItemCorrectly()
    {
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new Cache<string, int>( capacity: 3 );

        sut[entry.Key] = entry.Value;

        AssertCollection( sut, entry ).Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldUpdateValue_WhenKeyAlreadyExists()
    {
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );

        sut[entry2.Key] = entry2.Value;

        AssertCollection( sut, entry2 ).Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldUpdateValue_WhenKeyAlreadyExists_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new Cache<string, int>( capacity: 3, removeCallback: removed.Add );
        sut.TryAdd( entry1.Key, entry1.Value );

        sut[entry2.Key] = entry2.Value;

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateReplaced( entry1.Key, entry1.Value, entry2.Value ) ] ),
                AssertCollection( sut, entry2 ) )
            .Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldAddItemsToFullCapacityCorrectly()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );

        sut[entry1.Key] = entry1.Value;
        sut[entry2.Key] = entry2.Value;
        sut[entry3.Key] = entry3.Value;

        AssertCollection( sut, entry1, entry2, entry3 ).Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldAddNewItemAndRemoveOldestItem_WhenCapacityIsExceeded()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut[entry4.Key] = entry4.Value;

        Assertion.All(
                AssertCollection( sut, entry2, entry3, entry4 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldAddNewItemAndRemoveOldestItem_WhenCapacityIsExceeded_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new Cache<string, int>( capacity: 3, removeCallback: removed.Add );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut[entry4.Key] = entry4.Value;

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) ] ),
                AssertCollection( sut, entry2, entry3, entry4 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldUpdateExistingValueAndSetEntryAsNewest_WhenKeyAlreadyExistsAndIsNotNewest()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut[entry4.Key] = entry4.Value;

        AssertCollection( sut, entry2, entry3, entry4 ).Go();
    }

    [Fact]
    public void Indexer_Getter_ShouldSetExistingEntryAsNewest_WhenKeyExistsAndIsNotNewest()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut[entry1.Key];

        Assertion.All(
                result.TestEquals( entry1.Value ),
                AssertCollection( sut, entry2, entry3, entry1 ) )
            .Go();
    }

    [Fact]
    public void Indexer_Getter_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        var sut = new Cache<string, int>( capacity: 3 );
        var action = Lambda.Of( () => sut["foo"] );
        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void TryGetValue_ShouldSetExistingEntryAsNewest_WhenKeyExistsAndIsNotNewest()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.TryGetValue( entry1.Key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( entry1.Value ),
                AssertCollection( sut, entry2, entry3, entry1 ) )
            .Go();
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new Cache<string, int>( capacity: 3 );
        var result = sut.TryGetValue( "foo", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Theory]
    [InlineData( "foo", true )]
    [InlineData( "bar", false )]
    public void ContainsKey_ShouldReturnTrue_WhenKeyExists(string key, bool expected)
    {
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry.Key, entry.Value );

        var result = sut.ContainsKey( key );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Restart_ShouldSetExistingEntryAsNewest_WhenKeyExistsAndIsNotNewest()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.Restart( entry1.Key );

        Assertion.All(
                result.TestTrue(),
                AssertCollection( sut, entry2, entry3, entry1 ) )
            .Go();
    }

    [Fact]
    public void Restart_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new Cache<string, int>( capacity: 3 );
        var result = sut.Restart( "foo" );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new Cache<string, int>( capacity: 3 );
        var result = sut.Remove( "foo" );
        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_ShouldRemoveOldestEntry()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.Remove( entry1.Key );

        Assertion.All(
                result.TestTrue(),
                AssertCollection( sut, entry2, entry3 ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveNewestEntry()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.Remove( entry3.Key );

        Assertion.All(
                result.TestTrue(),
                AssertCollection( sut, entry1, entry2 ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveAnyEntry()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.Remove( entry2.Key );

        Assertion.All(
                result.TestTrue(),
                AssertCollection( sut, entry1, entry3 ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveAnyEntry_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3, removeCallback: removed.Add );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.Remove( entry2.Key );

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateRemoved( entry2.Key, entry2.Value ) ] ),
                result.TestTrue(),
                AssertCollection( sut, entry1, entry3 ) )
            .Go();
    }

    [Fact]
    public void Remove_WithReturnedRemoved_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new Cache<string, int>( capacity: 3 );

        var result = sut.Remove( "foo", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void Remove_WithReturnedRemoved_ShouldRemoveOldestEntry()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.Remove( entry1.Key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( entry1.Value ),
                AssertCollection( sut, entry2, entry3 ) )
            .Go();
    }

    [Fact]
    public void Remove_WithReturnedRemoved_ShouldRemoveNewestEntry()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.Remove( entry3.Key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( entry3.Value ),
                AssertCollection( sut, entry1, entry2 ) )
            .Go();
    }

    [Fact]
    public void Remove_WithReturnedRemoved_ShouldRemoveAnyEntry()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.Remove( entry2.Key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( entry2.Value ),
                AssertCollection( sut, entry1, entry3 ) )
            .Go();
    }

    [Fact]
    public void Remove_WithReturnedRemoved_ShouldRemoveAnyEntry_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3, removeCallback: removed.Add );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.Remove( entry2.Key, out var outResult );

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateRemoved( entry2.Key, entry2.Value ) ] ),
                result.TestTrue(),
                outResult.TestEquals( entry2.Value ),
                AssertCollection( sut, entry1, entry3 ) )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntries()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.TestEmpty(),
                sut.Oldest.TestNull(),
                sut.Newest.TestNull() )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntries_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new Cache<string, int>( capacity: 3, removeCallback: removed.Add );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.Clear();

        Assertion.All(
                removed.TestSequence(
                [
                    CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ),
                    CachedItemRemovalEvent<string, int>.CreateRemoved( entry2.Key, entry2.Value ),
                    CachedItemRemovalEvent<string, int>.CreateRemoved( entry3.Key, entry3.Value )
                ] ),
                sut.Count.TestEquals( 0 ),
                sut.TestEmpty(),
                sut.Oldest.TestNull(),
                sut.Newest.TestNull() )
            .Go();
    }

    [Fact]
    public void GetOrAdd_ShouldAddNewItemAndRemoveOldestItem_WhenCapacityIsExceeded()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.GetOrAdd( entry4.Key, _ => entry4.Value );

        Assertion.All(
                result.TestEquals( entry4.Value ),
                AssertCollection( sut, entry2, entry3, entry4 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void GetOrAdd_ShouldReturnExistingValueAndSetEntryAsNewest_WhenKeyAlreadyExistsAndIsNotNewest()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = sut.GetOrAdd( entry4.Key, _ => entry4.Value );

        Assertion.All(
                result.TestEquals( entry1.Value ),
                AssertCollection( sut, entry2, entry3, entry1 ) )
            .Go();
    }

    [Fact]
    public async Task GetOrAddAsync_WithValueTask_ShouldAddNewItemAndRemoveOldestItem_WhenCapacityIsExceeded()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = await sut.GetOrAddAsync( entry4.Key, (_, _) => ValueTask.FromResult( entry4.Value ) );

        Assertion.All(
                result.TestEquals( entry4.Value ),
                AssertCollection( sut, entry2, entry3, entry4 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public async Task GetOrAdd_WithValueTask_ShouldReturnExistingValueAndSetEntryAsNewest_WhenKeyAlreadyExistsAndIsNotNewest()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = await sut.GetOrAddAsync( entry4.Key, (_, _) => ValueTask.FromResult( entry4.Value ) );

        Assertion.All(
                result.TestEquals( entry1.Value ),
                AssertCollection( sut, entry2, entry3, entry1 ) )
            .Go();
    }

    [Fact]
    public async Task GetOrAddAsync_WithTask_ShouldAddNewItemAndRemoveOldestItem_WhenCapacityIsExceeded()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = await sut.GetOrAddAsync( entry4.Key, (_, _) => Task.FromResult( entry4.Value ) );

        Assertion.All(
                result.TestEquals( entry4.Value ),
                AssertCollection( sut, entry2, entry3, entry4 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public async Task GetOrAdd_WithTask_ShouldReturnExistingValueAndSetEntryAsNewest_WhenKeyAlreadyExistsAndIsNotNewest()
    {
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        var result = await sut.GetOrAddAsync( entry4.Key, (_, _) => Task.FromResult( entry4.Value ) );

        Assertion.All(
                result.TestEquals( entry1.Value ),
                AssertCollection( sut, entry2, entry3, entry1 ) )
            .Go();
    }

    [Pure]
    private static Assertion AssertCollection(Cache<string, int> sut, params KeyValuePair<string, int>[] expected)
    {
        return Assertion.All(
            sut.Count.TestEquals( expected.Length ),
            sut.Oldest.TestEquals( expected[0] ),
            sut.Newest.TestEquals( expected[^1] ),
            sut.Keys.TestSequence( expected.Select( kv => kv.Key ) ),
            sut.Values.TestSequence( expected.Select( kv => kv.Value ) ),
            sut.TestSequence( expected ),
            expected.TestAll( (kv, _) => sut.GetValueOrDefault( kv.Key ).TestEquals( kv.Value ) ) );
    }
}
