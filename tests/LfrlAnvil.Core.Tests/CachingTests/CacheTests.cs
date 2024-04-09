using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Caching;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

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

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( capacity );
            sut.Comparer.Should().BeSameAs( EqualityComparer<string>.Default );
            sut.Oldest.Should().BeNull();
            sut.Newest.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    [InlineData( 3 )]
    public void Ctor_ShouldCreateEmptyWithCorrectCapacity_WithExplicitComparer(int capacity)
    {
        var comparer = EqualityComparerFactory<string>.Create( (a, b) => a!.Equals( b ) );
        var sut = new Cache<string, int>( comparer, capacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( capacity );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Oldest.Should().BeNull();
            sut.Newest.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCapacityIsLessThanOne(int capacity)
    {
        var action = Lambda.Of( () => new Cache<string, int>( capacity ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TryAdd_ShouldAddFirstItemCorrectly()
    {
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new Cache<string, int>( capacity: 3 );

        var result = sut.TryAdd( entry.Key, entry.Value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            AssertCollection( sut, entry );
        }
    }

    [Fact]
    public void TryAdd_ShouldReturnFalse_WhenKeyAlreadyExists()
    {
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );

        var result = sut.TryAdd( entry2.Key, entry2.Value );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            AssertCollection( sut, entry1 );
        }
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

        using ( new AssertionScope() )
            AssertCollection( sut, entry1, entry2, entry3 );
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

        using ( new AssertionScope() )
        {
            AssertCollection( sut, entry2, entry3, entry4 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
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

        using ( new AssertionScope() )
        {
            removed.Should().BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) );
            AssertCollection( sut, entry2, entry3, entry4 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldAddFirstItemCorrectly()
    {
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new Cache<string, int>( capacity: 3 );

        var result = sut.AddOrUpdate( entry.Key, entry.Value );

        using ( new AssertionScope() )
        {
            result.Should().Be( AddOrUpdateResult.Added );
            AssertCollection( sut, entry );
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateValue_WhenKeyAlreadyExists()
    {
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );

        var result = sut.AddOrUpdate( entry2.Key, entry2.Value );

        using ( new AssertionScope() )
        {
            result.Should().Be( AddOrUpdateResult.Updated );
            AssertCollection( sut, entry2 );
        }
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

        using ( new AssertionScope() )
        {
            removed.Should()
                .BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateReplaced( entry1.Key, entry1.Value, entry2.Value ) );

            result.Should().Be( AddOrUpdateResult.Updated );
            AssertCollection( sut, entry2 );
        }
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

        using ( new AssertionScope() )
            AssertCollection( sut, entry1, entry2, entry3 );
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

        using ( new AssertionScope() )
        {
            AssertCollection( sut, entry2, entry3, entry4 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
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

        using ( new AssertionScope() )
        {
            removed.Should().BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) );
            AssertCollection( sut, entry2, entry3, entry4 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
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

        using ( new AssertionScope() )
            AssertCollection( sut, entry2, entry3, entry4 );
    }

    [Fact]
    public void Indexer_Setter_ShouldAddFirstItemCorrectly()
    {
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new Cache<string, int>( capacity: 3 );

        sut[entry.Key] = entry.Value;

        using ( new AssertionScope() )
            AssertCollection( sut, entry );
    }

    [Fact]
    public void Indexer_Setter_ShouldUpdateValue_WhenKeyAlreadyExists()
    {
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new Cache<string, int>( capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );

        sut[entry2.Key] = entry2.Value;

        using ( new AssertionScope() )
            AssertCollection( sut, entry2 );
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

        using ( new AssertionScope() )
        {
            removed.Should()
                .BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateReplaced( entry1.Key, entry1.Value, entry2.Value ) );

            AssertCollection( sut, entry2 );
        }
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

        using ( new AssertionScope() )
            AssertCollection( sut, entry1, entry2, entry3 );
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

        using ( new AssertionScope() )
        {
            AssertCollection( sut, entry2, entry3, entry4 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
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

        using ( new AssertionScope() )
        {
            removed.Should().BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) );
            AssertCollection( sut, entry2, entry3, entry4 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
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

        using ( new AssertionScope() )
            AssertCollection( sut, entry2, entry3, entry4 );
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

        using ( new AssertionScope() )
        {
            result.Should().Be( entry1.Value );
            AssertCollection( sut, entry2, entry3, entry1 );
        }
    }

    [Fact]
    public void Indexer_Getter_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        var sut = new Cache<string, int>( capacity: 3 );
        var action = Lambda.Of( () => sut["foo"] );
        action.Should().ThrowExactly<KeyNotFoundException>();
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

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( entry1.Value );
            AssertCollection( sut, entry2, entry3, entry1 );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new Cache<string, int>( capacity: 3 );
        var result = sut.TryGetValue( "foo", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
            sut.Count.Should().Be( 0 );
        }
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

        result.Should().Be( expected );
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

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            AssertCollection( sut, entry2, entry3, entry1 );
        }
    }

    [Fact]
    public void Restart_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new Cache<string, int>( capacity: 3 );
        var result = sut.Restart( "foo" );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new Cache<string, int>( capacity: 3 );
        var result = sut.Remove( "foo" );
        result.Should().BeFalse();
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

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            AssertCollection( sut, entry2, entry3 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            AssertCollection( sut, entry1, entry2 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            AssertCollection( sut, entry1, entry3 );
        }
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

        using ( new AssertionScope() )
        {
            removed.Should().BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateRemoved( entry2.Key, entry2.Value ) );
            result.Should().BeTrue();
            AssertCollection( sut, entry1, entry3 );
        }
    }

    [Fact]
    public void Remove_WithReturnedRemoved_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var sut = new Cache<string, int>( capacity: 3 );

        var result = sut.Remove( "foo", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( entry1.Value );
            AssertCollection( sut, entry2, entry3 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( entry3.Value );
            AssertCollection( sut, entry1, entry2 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( entry2.Value );
            AssertCollection( sut, entry1, entry3 );
        }
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

        using ( new AssertionScope() )
        {
            removed.Should().BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateRemoved( entry2.Key, entry2.Value ) );
            result.Should().BeTrue();
            outResult.Should().Be( entry2.Value );
            AssertCollection( sut, entry1, entry3 );
        }
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

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Should().BeEmpty();
            sut.Oldest.Should().BeNull();
            sut.Newest.Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            removed.Should()
                .BeSequentiallyEqualTo(
                    CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ),
                    CachedItemRemovalEvent<string, int>.CreateRemoved( entry2.Key, entry2.Value ),
                    CachedItemRemovalEvent<string, int>.CreateRemoved( entry3.Key, entry3.Value ) );

            sut.Count.Should().Be( 0 );
            sut.Should().BeEmpty();
            sut.Oldest.Should().BeNull();
            sut.Newest.Should().BeNull();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( entry4.Value );
            AssertCollection( sut, entry2, entry3, entry4 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( entry1.Value );
            AssertCollection( sut, entry2, entry3, entry1 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( entry4.Value );
            AssertCollection( sut, entry2, entry3, entry4 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( entry1.Value );
            AssertCollection( sut, entry2, entry3, entry1 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( entry4.Value );
            AssertCollection( sut, entry2, entry3, entry4 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
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

        using ( new AssertionScope() )
        {
            result.Should().Be( entry1.Value );
            AssertCollection( sut, entry2, entry3, entry1 );
        }
    }

    private static void AssertCollection(Cache<string, int> sut, params KeyValuePair<string, int>[] expected)
    {
        sut.Count.Should().Be( expected.Length );
        sut.Oldest.Should().Be( expected[0] );
        sut.Newest.Should().Be( expected[^1] );
        sut.Keys.Should().BeSequentiallyEqualTo( expected.Select( kv => kv.Key ) );
        sut.Values.Should().BeSequentiallyEqualTo( expected.Select( kv => kv.Value ) );
        sut.Should().BeSequentiallyEqualTo( expected );

        foreach ( var (key, value) in expected )
            sut.GetValueOrDefault( key ).Should().Be( value );
    }
}
