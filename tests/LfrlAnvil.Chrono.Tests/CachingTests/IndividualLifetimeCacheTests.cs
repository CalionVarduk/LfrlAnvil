using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Caching;
using LfrlAnvil.Chrono.Caching;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Chrono.Tests.CachingTests;

public class IndividualIndividualLifetimeCacheTests : TestsBase
{
    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 2, 100 )]
    [InlineData( 3, 10000 )]
    public void Ctor_ShouldCreateEmptyWithCorrectLifetimeAndCapacity(int capacity, int lifetimeTicks)
    {
        var start = new Timestamp( 123 );
        var lifetime = Duration.FromTicks( lifetimeTicks );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime, capacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( capacity ),
                sut.Lifetime.TestEquals( lifetime ),
                sut.StartTimestamp.TestEquals( start ),
                sut.CurrentTimestamp.TestEquals( start ),
                sut.Comparer.TestRefEquals( EqualityComparer<string>.Default ),
                sut.Oldest.TestNull() )
            .Go();
    }

    [Theory]
    [InlineData( 1, 1 )]
    [InlineData( 2, 100 )]
    [InlineData( 3, 10000 )]
    public void Ctor_ShouldCreateEmptyWithCorrectLifetimeAndCapacity_WithExplicitComparer(int capacity, int lifetimeTicks)
    {
        var comparer = EqualityComparerFactory<string>.Create( (a, b) => a!.Equals( b ) );
        var start = new Timestamp( 123 );
        var lifetime = Duration.FromTicks( lifetimeTicks );
        var sut = new IndividualLifetimeCache<string, int>( comparer, start, lifetime, capacity );

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.Capacity.TestEquals( capacity ),
                sut.Lifetime.TestEquals( lifetime ),
                sut.StartTimestamp.TestEquals( start ),
                sut.CurrentTimestamp.TestEquals( start ),
                sut.Comparer.TestRefEquals( comparer ),
                sut.Oldest.TestNull() )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCapacityIsLessThanOne(int capacity)
    {
        var start = new Timestamp( 123 );
        var lifetime = Duration.FromTicks( 1 );

        var action = Lambda.Of( () => new IndividualLifetimeCache<string, int>( start, lifetime, capacity ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenLifetimeIsLessThanOneTick(int ticks)
    {
        var start = new Timestamp( 123 );
        var lifetime = Duration.FromTicks( ticks );

        var action = Lambda.Of( () => new IndividualLifetimeCache<string, int>( start, lifetime, capacity: 1 ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void TryAdd_ShouldAddFirstItemCorrectly()
    {
        var start = new Timestamp( 123 );
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var result = sut.TryAdd( entry.Key, entry.Value );

        Assertion.All(
                result.TestTrue(),
                sut.GetRemainingLifetime( entry.Key ).TestEquals( sut.Lifetime ),
                AssertCollection( sut, entry ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldReturnFalse_WhenKeyAlreadyExists()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
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
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );

        AssertCollection( sut, entry1, entry2, entry3 ).Go();
    }

    [Fact]
    public void TryAdd_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut.TryAdd( entry4.Key, entry4.Value );

        Assertion.All(
                AssertCollection( sut, entry2, entry4, entry3 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>(
            start,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut.TryAdd( entry4.Key, entry4.Value );

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) ] ),
                AssertCollection( sut, entry2, entry4, entry3 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldAddItemsWithCustomLifetime()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut.TryAdd( entry1.Key, entry1.Value, Duration.FromSeconds( 5 ) );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value, Duration.FromSeconds( 2 ) );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value, Duration.FromSeconds( 3 ) );

        Assertion.All(
                sut.GetRemainingLifetime( entry1.Key ).TestEquals( Duration.FromSeconds( 5 ) - Duration.FromTicks( 2 ) ),
                sut.GetRemainingLifetime( entry2.Key ).TestEquals( Duration.FromSeconds( 2 ) - Duration.FromTicks( 1 ) ),
                sut.GetRemainingLifetime( entry3.Key ).TestEquals( Duration.FromSeconds( 3 ) ),
                AssertCollection( sut, entry2, entry1, entry3 ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddFirstItemCorrectly()
    {
        var start = new Timestamp( 123 );
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var result = sut.AddOrUpdate( entry.Key, entry.Value );

        Assertion.All(
                result.TestEquals( AddOrUpdateResult.Added ),
                sut.GetRemainingLifetime( entry.Key ).TestEquals( sut.Lifetime ),
                AssertCollection( sut, entry ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateValueAndLifetime_WhenKeyAlreadyExists()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        var result = sut.AddOrUpdate( entry2.Key, entry2.Value );

        Assertion.All(
                result.TestEquals( AddOrUpdateResult.Updated ),
                sut.GetRemainingLifetime( entry2.Key ).TestEquals( sut.Lifetime ),
                AssertCollection( sut, entry2 ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateValueAndLifetime_WhenKeyAlreadyExists_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new IndividualLifetimeCache<string, int>(
            start,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        var result = sut.AddOrUpdate( entry2.Key, entry2.Value );

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateReplaced( entry1.Key, entry1.Value, entry2.Value ) ] ),
                result.TestEquals( AddOrUpdateResult.Updated ),
                sut.GetRemainingLifetime( entry2.Key ).TestEquals( sut.Lifetime ),
                AssertCollection( sut, entry2 ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddItemsToFullCapacityCorrectly()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut.AddOrUpdate( entry1.Key, entry1.Value );
        sut.AddOrUpdate( entry2.Key, entry2.Value );
        sut.AddOrUpdate( entry3.Key, entry3.Value );

        Assertion.All().Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut.AddOrUpdate( entry4.Key, entry4.Value );

        Assertion.All(
                AssertCollection( sut, entry2, entry4, entry3 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>(
            start,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut.AddOrUpdate( entry4.Key, entry4.Value );

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) ] ),
                AssertCollection( sut, entry2, entry4, entry3 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateExistingValueAndRestartEntry_WhenKeyAlreadyExists()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut.AddOrUpdate( entry4.Key, entry4.Value );

        AssertCollection( sut, entry2, entry4, entry3 ).Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddItemsWithCustomLifetime()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut.AddOrUpdate( entry1.Key, entry1.Value, Duration.FromSeconds( 5 ) );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.AddOrUpdate( entry2.Key, entry2.Value, Duration.FromSeconds( 2 ) );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.AddOrUpdate( entry3.Key, entry3.Value, Duration.FromSeconds( 3 ) );

        Assertion.All(
                sut.GetRemainingLifetime( entry1.Key ).TestEquals( Duration.FromSeconds( 5 ) - Duration.FromTicks( 2 ) ),
                sut.GetRemainingLifetime( entry2.Key ).TestEquals( Duration.FromSeconds( 2 ) - Duration.FromTicks( 1 ) ),
                sut.GetRemainingLifetime( entry3.Key ).TestEquals( Duration.FromSeconds( 3 ) ),
                AssertCollection( sut, entry2, entry1, entry3 ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_WithCustomShorterLifetime_ShouldUpdateExistingValueAndRestartEntryWithNewLifetime_WhenKeyAlreadyExists()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut.AddOrUpdate( entry4.Key, entry4.Value, Duration.FromMilliseconds( 500 ) );

        Assertion.All(
                sut.GetRemainingLifetime( entry4.Key ).TestEquals( Duration.FromMilliseconds( 500 ) ),
                AssertCollection( sut, entry4, entry2, entry3 ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_WithCustomLongerLifetime_ShouldUpdateExistingValueAndRestartEntryWithNewLifetime_WhenKeyAlreadyExists()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut.AddOrUpdate( entry4.Key, entry4.Value, Duration.FromSeconds( 2 ) );

        Assertion.All(
                sut.GetRemainingLifetime( entry4.Key ).TestEquals( Duration.FromSeconds( 2 ) ),
                AssertCollection( sut, entry2, entry4, entry3 ) )
            .Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldAddFirstItemCorrectly()
    {
        var start = new Timestamp( 123 );
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut[entry.Key] = entry.Value;

        Assertion.All(
                sut.GetRemainingLifetime( entry.Key ).TestEquals( sut.Lifetime ),
                AssertCollection( sut, entry ) )
            .Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldUpdateValueAndLifetime_WhenKeyAlreadyExists()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut[entry2.Key] = entry2.Value;

        Assertion.All(
                sut.GetRemainingLifetime( entry2.Key ).TestEquals( sut.Lifetime ),
                AssertCollection( sut, entry2 ) )
            .Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldUpdateValueAndLifetime_WhenKeyAlreadyExists_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 42 );
        var entry2 = KeyValuePair.Create( entry1.Key, 1 );
        var sut = new IndividualLifetimeCache<string, int>(
            start,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut[entry2.Key] = entry2.Value;

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateReplaced( entry1.Key, entry1.Value, entry2.Value ) ] ),
                sut.GetRemainingLifetime( entry2.Key ).TestEquals( sut.Lifetime ),
                AssertCollection( sut, entry2 ) )
            .Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldAddItemsToFullCapacityCorrectly()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut[entry1.Key] = entry1.Value;
        sut[entry2.Key] = entry2.Value;
        sut[entry3.Key] = entry3.Value;

        AssertCollection( sut, entry1, entry2, entry3 ).Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut[entry4.Key] = entry4.Value;

        Assertion.All(
                AssertCollection( sut, entry2, entry4, entry3 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldAddNewItemAndRemoveItemWithShortestLifetime_WhenCapacityIsExceeded_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( "lorem", 4 );
        var sut = new IndividualLifetimeCache<string, int>(
            start,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut[entry4.Key] = entry4.Value;

        Assertion.All(
                removed.TestSequence( [ CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ) ] ),
                AssertCollection( sut, entry2, entry4, entry3 ),
                sut.ContainsKey( entry1.Key ).TestFalse() )
            .Go();
    }

    [Fact]
    public void Indexer_Setter_ShouldUpdateExistingValueAndRestartEntry_WhenKeyAlreadyExists()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var entry4 = KeyValuePair.Create( entry1.Key, 4 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        sut[entry4.Key] = entry4.Value;

        AssertCollection( sut, entry2, entry4, entry3 ).Go();
    }

    [Fact]
    public void Indexer_Getter_ShouldRestartExistingEntry_WhenKeyExists()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        var result = sut[entry1.Key];

        Assertion.All(
                result.TestEquals( entry1.Value ),
                sut.GetRemainingLifetime( entry1.Key ).TestEquals( sut.Lifetime ),
                AssertCollection( sut, entry2, entry1, entry3 ) )
            .Go();
    }

    [Fact]
    public void Indexer_Getter_ShouldThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        var start = new Timestamp( 123 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var action = Lambda.Of( () => sut["foo"] );

        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void TryGetValue_ShouldRestartExistingEntry_WhenKeyExists()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        var result = sut.TryGetValue( entry1.Key, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( entry1.Value ),
                sut.GetRemainingLifetime( entry1.Key ).TestEquals( sut.Lifetime ),
                AssertCollection( sut, entry2, entry1, entry3 ) )
            .Go();
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var start = new Timestamp( 123 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
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
        var start = new Timestamp( 123 );
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry.Key, entry.Value );

        var result = sut.ContainsKey( key );

        result.TestEquals( expected ).Go();
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
    public void GetRemainingLifetime_ShouldReturnRemainingItemLifetime(long ticksToMoveForward, long expectedTicks)
    {
        var start = new Timestamp( 123 );
        var entry = KeyValuePair.Create( "foo", 1 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry.Key, entry.Value );
        sut.Move( Duration.FromTicks( ticksToMoveForward ) );

        var result = sut.GetRemainingLifetime( entry.Key );

        result.TestEquals( Duration.FromTicks( expectedTicks ) ).Go();
    }

    [Fact]
    public void Restart_ShouldRestartExistingEntry_WhenKeyExists()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        var result = sut.Restart( entry1.Key );

        Assertion.All(
                result.TestTrue(),
                sut.GetRemainingLifetime( entry1.Key ).TestEquals( sut.Lifetime ),
                AssertCollection( sut, entry2, entry1, entry3 ) )
            .Go();
    }

    [Fact]
    public void Restart_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var start = new Timestamp( 123 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var result = sut.Restart( "foo" );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var start = new Timestamp( 123 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var result = sut.Remove( "foo" );

        result.TestFalse().Go();
    }

    [Fact]
    public void Remove_ShouldRemoveOldestEntry()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        var result = sut.Remove( entry1.Key );

        Assertion.All(
                result.TestTrue(),
                AssertCollection( sut, entry2, entry3 ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveNewestEntry()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

        var result = sut.Remove( entry3.Key );

        Assertion.All(
                result.TestTrue(),
                AssertCollection( sut, entry1, entry2 ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveAnyEntry()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 100 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value, sut.Lifetime - Duration.FromTicks( 50 ) );
        sut.Move( Duration.FromTicks( 1 ) );

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
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>(
            start,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 100 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value, sut.Lifetime - Duration.FromTicks( 50 ) );
        sut.Move( Duration.FromTicks( 1 ) );

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
        var start = new Timestamp( 123 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        var result = sut.Remove( "foo", out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void Remove_WithReturnedRemoved_ShouldRemoveOldestEntry()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

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
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );
        sut.Move( Duration.FromTicks( 1 ) );

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
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 100 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value, sut.Lifetime - Duration.FromTicks( 50 ) );
        sut.Move( Duration.FromTicks( 1 ) );

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
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>(
            start,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.Move( Duration.FromTicks( 100 ) );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value, sut.Lifetime - Duration.FromTicks( 50 ) );
        sut.Move( Duration.FromTicks( 1 ) );

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
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.TestEmpty(),
                sut.Oldest.TestNull() )
            .Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntries_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>(
            start,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

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
                sut.Oldest.TestNull() )
            .Go();
    }

    [Fact]
    public void Move_ShouldRemoveAllExpiredEntriesAndMoveCurrentPointForward()
    {
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );
        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.Move( sut.Lifetime - Duration.FromTicks( 1 ) );

        AssertCollection( sut, entry3 ).Go();
    }

    [Fact]
    public void Move_ShouldRemoveAllExpiredEntriesAndMoveCurrentPointForward_WithRemoveCallback()
    {
        var removed = new List<CachedItemRemovalEvent<string, int>>();
        var start = new Timestamp( 123 );
        var entry1 = KeyValuePair.Create( "foo", 1 );
        var entry2 = KeyValuePair.Create( "bar", 2 );
        var entry3 = KeyValuePair.Create( "qux", 3 );
        var sut = new IndividualLifetimeCache<string, int>(
            start,
            lifetime: Duration.FromSeconds( 1 ),
            capacity: 3,
            removeCallback: removed.Add );

        sut.TryAdd( entry1.Key, entry1.Value );
        sut.TryAdd( entry2.Key, entry2.Value );
        sut.Move( Duration.FromTicks( 1 ) );
        sut.TryAdd( entry3.Key, entry3.Value );

        sut.Move( sut.Lifetime - Duration.FromTicks( 1 ) );

        Assertion.All(
                removed.TestSequence(
                [
                    CachedItemRemovalEvent<string, int>.CreateRemoved( entry1.Key, entry1.Value ),
                    CachedItemRemovalEvent<string, int>.CreateRemoved( entry2.Key, entry2.Value )
                ] ),
                AssertCollection( sut, entry3 ) )
            .Go();
    }

    [Fact]
    public void MoveTo_ShouldMoveCacheByCorrectAmount()
    {
        var start = new Timestamp( 123 );
        var sut = new LifetimeCache<string, int>( start, lifetime: Duration.FromSeconds( 1 ), capacity: 3 );

        sut.MoveTo( new Timestamp( 456 ) );

        sut.CurrentTimestamp.TestEquals( new Timestamp( 456 ) ).Go();
    }

    [Pure]
    private static Assertion AssertCollection(IndividualLifetimeCache<string, int> sut, params KeyValuePair<string, int>[] expected)
    {
        return Assertion.All(
            sut.Count.TestEquals( expected.Length ),
            sut.Oldest.TestEquals( expected[0] ),
            sut.Keys.TestSequence( expected.Select( kv => kv.Key ) ),
            sut.Values.TestSequence( expected.Select( kv => kv.Value ) ),
            sut.TestSequence( expected ),
            expected.TestAll( (kv, _) => sut.GetValueOrDefault( kv.Key ).TestEquals( kv.Value ) ) );
    }
}
