using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Caching;
using LfrlAnvil.Chrono.Caching;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Chrono.Tests.CachingTests;

public class IndividualIndividualLifetimeCacheTests : TestsBase
{
    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 2, 100 )]
    [InlineData( 3, 10000 )]
    public void Ctor_ShouldCreateEmptyWithCorrectLifetimeAndCapacity(int capacity, int lifetimeTicks)
    {
        var timestamps = new Timestamps();
        var lifetime = Duration.FromTicks( lifetimeTicks );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime, capacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( capacity );
            sut.Lifetime.Should().Be( lifetime );
            sut.Timestamps.Should().BeSameAs( timestamps );
            sut.Comparer.Should().BeSameAs( EqualityComparer<string>.Default );
            sut.Oldest.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 2, 100 )]
    [InlineData( 3, 10000 )]
    public void Ctor_ShouldCreateEmptyWithCorrectLifetimeAndCapacity_WithExplicitComparer(int capacity, int lifetimeTicks)
    {
        var comparer = EqualityComparerFactory<string>.Create( (a, b) => a!.Equals( b ) );
        var timestamps = new Timestamps();
        var lifetime = Duration.FromTicks( lifetimeTicks );
        var sut = new IndividualLifetimeCache<string, int>( comparer, timestamps, lifetime, capacity );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Capacity.Should().Be( capacity );
            sut.Lifetime.Should().Be( lifetime );
            sut.Timestamps.Should().BeSameAs( timestamps );
            sut.Comparer.Should().BeSameAs( comparer );
            sut.Oldest.Should().BeNull();
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCapacityIsLessThanOne(int capacity)
    {
        var timestamps = new Timestamps();
        var lifetime = Duration.FromTicks( 1 );

        var action = Lambda.Of( () => new IndividualLifetimeCache<string, int>( timestamps, lifetime, capacity ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenLifetimeIsLessThanOneTick(int ticks)
    {
        var timestamps = new Timestamps();
        var lifetime = Duration.FromTicks( ticks );

        var action = Lambda.Of( () => new IndividualLifetimeCache<string, int>( timestamps, lifetime, capacity: 1 ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TryAdd_ShouldAddFirstItemCorrectly()
    {
        var timestamps = new Timestamps();
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var result = sut.TryAdd( entry.Key, entry.Value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.GetRemainingLifetime( entry.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry );
        }
    }

    [Fact]
    public void TryAdd_ShouldReturnFalse_WhenKeyAlreadyExists()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
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
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );

        using ( new AssertionScope() )
            AssertCollection( sut, entry1, entry2, entry3 );
    }

    [Fact]
    public void TryAdd_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut.TryAdd( entry4.Key, entry4.Value );

        using ( new AssertionScope() )
        {
            AssertCollection( sut, entry2, entry4, entry3 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
    }

    [Fact]
    public void TryAdd_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>(
            timestamps,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut.TryAdd( entry4.Key, entry4.Value );

        using ( new AssertionScope() )
        {
            removed.Should().BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) );
            AssertCollection( sut, entry2, entry4, entry3 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
    }

    [Fact]
    public void TryAdd_ShouldRefreshCacheBeforeAttemptingToAddItem()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += sut.Lifetime - Duration.FromTicks( 1 );

        var result = sut.TryAdd( entry1.Key, entry1.Value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.GetRemainingLifetime( entry3.Key ).Should().Be( Duration.FromTicks( 1 ) );
            sut.GetRemainingLifetime( entry1.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry3, entry1 );
        }
    }

    [Fact]
    public void TryAdd_ShouldAddItemsWithCustomLifetime()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut.TryAdd( entry1.Key, entry1.Value, Duration.FromSeconds( 5 ) );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value, Duration.FromSeconds( 2 ) );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value, Duration.FromSeconds( 3 ) );

        using ( new AssertionScope() )
        {
            sut.GetRemainingLifetime( entry1.Key ).Should().Be( Duration.FromSeconds( 5 ) - Duration.FromTicks( 2 ) );
            sut.GetRemainingLifetime( entry2.Key ).Should().Be( Duration.FromSeconds( 2 ) - Duration.FromTicks( 1 ) );
            sut.GetRemainingLifetime( entry3.Key ).Should().Be( Duration.FromSeconds( 3 ) );
            AssertCollection( sut, entry2, entry1, entry3 );
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldAddFirstItemCorrectly()
    {
        var timestamps = new Timestamps();
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var result = sut.AddOrUpdate( entry.Key, entry.Value );

        using ( new AssertionScope() )
        {
            result.Should().Be( AddOrUpdateResult.Added );
            sut.GetRemainingLifetime( entry.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry );
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateValueAndLifetime_WhenKeyAlreadyExists()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        var result = sut.AddOrUpdate( entry2.Key, entry2.Value );

        using ( new AssertionScope() )
        {
            result.Should().Be( AddOrUpdateResult.Updated );
            sut.GetRemainingLifetime( entry2.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry2 );
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateValueAndLifetime_WhenKeyAlreadyExists_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new IndividualLifetimeCache<string, int>(
            timestamps,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        var result = sut.AddOrUpdate( entry2.Key, entry2.Value );

        using ( new AssertionScope() )
        {
            removed.Should()
                .BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateReplaced( entry1.Key, entry1.Value, entry2.Value ) );

            result.Should().Be( AddOrUpdateResult.Updated );
            sut.GetRemainingLifetime( entry2.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry2 );
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldAddItemsToFullCapacityCorrectly()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut.AddOrUpdate( entry1.Key, entry1.Value );
        sut.AddOrUpdate( entry2.Key, entry2.Value );
        sut.AddOrUpdate( entry3.Key, entry3.Value );

        using ( new AssertionScope() )
            AssertCollection( sut, entry1, entry2, entry3 );
    }

    [Fact]
    public void AddOrUpdate_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut.AddOrUpdate( entry4.Key, entry4.Value );

        using ( new AssertionScope() )
        {
            AssertCollection( sut, entry2, entry4, entry3 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>(
            timestamps,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut.AddOrUpdate( entry4.Key, entry4.Value );

        using ( new AssertionScope() )
        {
            removed.Should().BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) );
            AssertCollection( sut, entry2, entry4, entry3 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateExistingValueAndRestartEntry_WhenKeyAlreadyExists()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut.AddOrUpdate( entry4.Key, entry4.Value );

        using ( new AssertionScope() )
            AssertCollection( sut, entry2, entry4, entry3 );
    }

    [Fact]
    public void AddOrUpdate_ShouldRefreshCacheBeforeAddingOrUpdatingItem()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += sut.Lifetime - Duration.FromTicks( 1 );

        var result = sut.AddOrUpdate( entry1.Key, entry1.Value );

        using ( new AssertionScope() )
        {
            result.Should().Be( AddOrUpdateResult.Added );
            sut.GetRemainingLifetime( entry3.Key ).Should().Be( Duration.FromTicks( 1 ) );
            sut.GetRemainingLifetime( entry1.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry3, entry1 );
        }
    }

    [Fact]
    public void AddOrUpdate_ShouldAddItemsWithCustomLifetime()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut.AddOrUpdate( entry1.Key, entry1.Value, Duration.FromSeconds( 5 ) );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.AddOrUpdate( entry2.Key, entry2.Value, Duration.FromSeconds( 2 ) );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.AddOrUpdate( entry3.Key, entry3.Value, Duration.FromSeconds( 3 ) );

        using ( new AssertionScope() )
        {
            sut.GetRemainingLifetime( entry1.Key ).Should().Be( Duration.FromSeconds( 5 ) - Duration.FromTicks( 2 ) );
            sut.GetRemainingLifetime( entry2.Key ).Should().Be( Duration.FromSeconds( 2 ) - Duration.FromTicks( 1 ) );
            sut.GetRemainingLifetime( entry3.Key ).Should().Be( Duration.FromSeconds( 3 ) );
            AssertCollection( sut, entry2, entry1, entry3 );
        }
    }

    [Fact]
    public void AddOrUpdate_WithCustomShorterLifetime_ShouldUpdateExistingValueAndRestartEntryWithNewLifetime_WhenKeyAlreadyExists()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut.AddOrUpdate( entry4.Key, entry4.Value, Duration.FromMilliseconds( 500 ) );

        using ( new AssertionScope() )
        {
            sut.GetRemainingLifetime( entry4.Key ).Should().Be( Duration.FromMilliseconds( 500 ) );
            AssertCollection( sut, entry4, entry2, entry3 );
        }
    }

    [Fact]
    public void AddOrUpdate_WithCustomLongerLifetime_ShouldUpdateExistingValueAndRestartEntryWithNewLifetime_WhenKeyAlreadyExists()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut.AddOrUpdate( entry4.Key, entry4.Value, Duration.FromSeconds( 2 ) );

        using ( new AssertionScope() )
        {
            sut.GetRemainingLifetime( entry4.Key ).Should().Be( Duration.FromSeconds( 2 ) );
            AssertCollection( sut, entry2, entry4, entry3 );
        }
    }

    [Fact]
    public void Indexer_Setter_ShouldAddFirstItemCorrectly()
    {
        var timestamps = new Timestamps();
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut[entry.Key] = entry.Value;

        using ( new AssertionScope() )
        {
            sut.GetRemainingLifetime( entry.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry );
        }
    }

    [Fact]
    public void Indexer_Setter_ShouldUpdateValueAndLifetime_WhenKeyAlreadyExists()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut[entry2.Key] = entry2.Value;

        using ( new AssertionScope() )
        {
            sut.GetRemainingLifetime( entry2.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry2 );
        }
    }

    [Fact]
    public void Indexer_Setter_ShouldUpdateValueAndLifetime_WhenKeyAlreadyExists_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new IndividualLifetimeCache<string, int>(
            timestamps,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut[entry2.Key] = entry2.Value;

        using ( new AssertionScope() )
        {
            removed.Should()
                .BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateReplaced( entry1.Key, entry1.Value, entry2.Value ) );

            sut.GetRemainingLifetime( entry2.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry2 );
        }
    }

    [Fact]
    public void Indexer_Setter_ShouldAddItemsToFullCapacityCorrectly()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut[entry1.Key] = entry1.Value;
        sut[entry2.Key] = entry2.Value;
        sut[entry3.Key] = entry3.Value;

        using ( new AssertionScope() )
            AssertCollection( sut, entry1, entry2, entry3 );
    }

    [Fact]
    public void Indexer_Setter_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut[entry4.Key] = entry4.Value;

        using ( new AssertionScope() )
        {
            AssertCollection( sut, entry2, entry4, entry3 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
    }

    [Fact]
    public void Indexer_Setter_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>(
            timestamps,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut[entry4.Key] = entry4.Value;

        using ( new AssertionScope() )
        {
            removed.Should().BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) );
            AssertCollection( sut, entry2, entry4, entry3 );
            sut.ContainsKey( entry1.Key ).Should().BeFalse();
        }
    }

    [Fact]
    public void Indexer_Setter_ShouldUpdateExistingValueAndRestartEntry_WhenKeyAlreadyExists()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        sut[entry4.Key] = entry4.Value;

        using ( new AssertionScope() )
            AssertCollection( sut, entry2, entry4, entry3 );
    }

    [Fact]
    public void Indexer_Setter_ShouldRefreshCacheBeforeAddingOrUpdatingItem()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += sut.Lifetime - Duration.FromTicks( 1 );

        sut[entry1.Key] = entry1.Value;

        using ( new AssertionScope() )
        {
            sut.GetRemainingLifetime( entry3.Key ).Should().Be( Duration.FromTicks( 1 ) );
            sut.GetRemainingLifetime( entry1.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry3, entry1 );
        }
    }

    [Fact]
    public void Indexer_Getter_ShouldRestartExistingEntry_WhenKeyExists()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        var result = sut[entry1.Key];

        using ( new AssertionScope() )
        {
            result.Should().Be( entry1.Value );
            sut.GetRemainingLifetime( entry1.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry2, entry1, entry3 );
        }
    }

    [Fact]
    public void Indexer_Getter_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        var timestamps = new Timestamps();
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var action = Lambda.Of( () => sut["foo"] );

        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Indexer_Getter_ShouldRefreshCacheBeforeFetchingItem()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += sut.Lifetime - Duration.FromTicks( 1 );

        var result = sut[entry3.Key];

        using ( new AssertionScope() )
        {
            result.Should().Be( entry3.Value );
            sut.GetRemainingLifetime( entry3.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry3 );
        }
    }

    [Fact]
    public void TryGetValue_ShouldRestartExistingEntry_WhenKeyExists()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        var result = sut.TryGetValue( entry1.Key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( entry1.Value );
            sut.GetRemainingLifetime( entry1.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry2, entry1, entry3 );
        }
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var timestamps = new Timestamps();
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        var result = sut.TryGetValue( "foo", out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void TryGetValue_ShouldRefreshCacheBeforeFetchingItem()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += sut.Lifetime - Duration.FromTicks( 1 );

        var result = sut.TryGetValue( entry3.Key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( entry3.Value );
            sut.GetRemainingLifetime( entry3.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry3 );
        }
    }

    [Theory]
    [InlineData( "foo", true )]
    [InlineData( "bar", false )]
    public void ContainsKey_ShouldReturnTrue_WhenKeyExists(string key, bool expected)
    {
        var timestamps = new Timestamps();
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry.Key, entry.Value );

        var result = sut.ContainsKey( key );

        result.Should().Be( expected );
    }

    [Fact]
    public void ContainsKey_ShouldRefreshCacheBeforeCheckingForKeyExistence()
    {
        var timestamps = new Timestamps();
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry.Key, entry.Value );
        timestamps.Next += sut.Lifetime;

        var result = sut.ContainsKey( entry.Key );

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 0, 10000000 )]
    [InlineData( 1, 9999999 )]
    [InlineData( 10, 9999990 )]
    [InlineData( 5000000, 5000000 )]
    [InlineData( 9000000, 1000000 )]
    [InlineData( 9999990, 10 )]
    [InlineData( 9999999, 1 )]
    [InlineData( 10000000, 0 )]
    [InlineData( 10000001, 0 )]
    public void GetRemainingLifetime_ShouldRefreshCacheBeforeFetchingLifetime_AndReturnRemainingItemLifetime(
        long ticksToMoveForward,
        long expectedTicks)
    {
        var timestamps = new Timestamps();
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry.Key, entry.Value );
        timestamps.Next += Duration.FromTicks( ticksToMoveForward );

        var result = sut.GetRemainingLifetime( entry.Key );

        result.Should().Be( Duration.FromTicks( expectedTicks ) );
    }

    [Fact]
    public void Restart_ShouldRestartExistingEntry_WhenKeyExists()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

        var result = sut.Restart( entry1.Key );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.GetRemainingLifetime( entry1.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry2, entry1, entry3 );
        }
    }

    [Fact]
    public void Restart_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var timestamps = new Timestamps();
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var result = sut.Restart( "foo" );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void Restart_ShouldRefreshCacheBeforeFetchingItem()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += sut.Lifetime - Duration.FromTicks( 1 );

        var result = sut.Restart( entry3.Key );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            sut.GetRemainingLifetime( entry3.Key ).Should().Be( sut.Lifetime );
            AssertCollection( sut, entry3 );
        }
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var timestamps = new Timestamps();
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var result = sut.Remove( "foo" );

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_ShouldRemoveOldestEntry()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

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
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

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
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 100 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value, sut.Lifetime - Duration.FromTicks( 50 ) );
        timestamps.Next += Duration.FromTicks( 1 );

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
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>(
            timestamps,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 100 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value, sut.Lifetime - Duration.FromTicks( 50 ) );
        timestamps.Next += Duration.FromTicks( 1 );

        var result = sut.Remove( entry2.Key );

        using ( new AssertionScope() )
        {
            removed.Should().BeSequentiallyEqualTo( CachedItemRemovalEvent<string, int>.CreateRemoved( entry2.Key, entry2.Value ) );
            result.Should().BeTrue();
            AssertCollection( sut, entry1, entry3 );
        }
    }

    [Fact]
    public void Remove_ShouldRefreshCacheBeforeRemovingItem()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += sut.Lifetime;

        var result = sut.Remove( entry1.Key );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void Remove_WithReturnedRemoved_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var timestamps = new Timestamps();
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

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
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

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
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += Duration.FromTicks( 1 );

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
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 100 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value, sut.Lifetime - Duration.FromTicks( 50 ) );
        timestamps.Next += Duration.FromTicks( 1 );

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
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>(
            timestamps,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        timestamps.Next += Duration.FromTicks( 100 );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value, sut.Lifetime - Duration.FromTicks( 50 ) );
        timestamps.Next += Duration.FromTicks( 1 );

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
    public void Remove_WithReturnedRemoved_ShouldRefreshCacheBeforeRemovingItem()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += sut.Lifetime;

        var result = sut.Remove( entry1.Key, out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
            sut.Count.Should().Be( 0 );
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntries()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.Clear();

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 0 );
            sut.Should().BeEmpty();
            sut.Oldest.Should().BeNull();
        }
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntries_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>(
            timestamps,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

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
        }
    }

    [Fact]
    public void Refresh_ShouldRemoveAllExpiredEntries()
    {
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( timestamps, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += sut.Lifetime - Duration.FromTicks( 1 );

        sut.Refresh();

        using ( new AssertionScope() )
            AssertCollection( sut, entry3 );
    }

    [Fact]
    public void Refresh_ShouldRemoveAllExpiredEntries_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var timestamps = new Timestamps();
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>(
            timestamps,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        timestamps.Next += Duration.FromTicks( 1 );
        sut.TryAdd( entry3.Key, entry3.Value );
        timestamps.Next += sut.Lifetime - Duration.FromTicks( 1 );

        sut.Refresh();

        using ( new AssertionScope() )
        {
            removed.Should()
                .BeSequentiallyEqualTo(
                    CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ),
                    CachedItemRemovalEvent<string, int>.CreateRemoved( entry2.Key, entry2.Value ) );

            AssertCollection( sut, entry3 );
        }
    }

    private static void AssertCollection(IndividualLifetimeCache<string, int> sut, params KeyValuePair<string, int>[] expected)
    {
        sut.Count.Should().Be( expected.Length );
        sut.Oldest.Should().Be( expected[0] );
        sut.Keys.Should().BeSequentiallyEqualTo( expected.Select( kv => kv.Key ) );
        sut.Values.Should().BeSequentiallyEqualTo( expected.Select( kv => kv.Value ) );
        sut.Should().BeSequentiallyEqualTo( expected );

        foreach ( var (key, value) in expected )
            sut.GetValueOrDefault( key ).Should().Be( value );
    }

    private sealed class Timestamps : TimestampProviderBase
    {
        public Timestamps(Timestamp? next = null)
        {
            Next = next ?? Timestamp.Zero;
        }

        public Timestamp Next { get; set; }

        [Pure]
        public override Timestamp GetNow()
        {
            return Next;
        }
    }
}
