using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.SegmentedSparseDictionaryTests;

public class SegmentedSparseDictionaryTests : TestsBase
{
    [Theory]
    [InlineData( -1, 8 )]
    [InlineData( 0, 8 )]
    [InlineData( 1, 8 )]
    [InlineData( 7, 8 )]
    [InlineData( 8, 8 )]
    [InlineData( 9, 16 )]
    [InlineData( 16, 16 )]
    [InlineData( 17, 32 )]
    [InlineData( 33, 64 )]
    [InlineData( 1023, 1024 )]
    public void Create_ShouldReturnCorrectDictionary(int minSegmentLength, int expectedSegmentLength)
    {
        var sut = SegmentedSparseDictionary<string>.Create( minSegmentLength );
        Assertion.All(
                sut.SegmentLength.TestEquals( expectedSegmentLength ),
                sut.Count.TestEquals( 0 ),
                sut.SegmentCount.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                TestSequence( sut, [ ] ) )
            .Go();
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 1, 1 )]
    [InlineData( 2, 1 )]
    [InlineData( 7, 1 )]
    [InlineData( 8, 1 )]
    [InlineData( 9, 1 )]
    [InlineData( 15, 1 )]
    [InlineData( 16, 2 )]
    [InlineData( 128, 9 )]
    public void TryAdd_ShouldAddFirstItemCorrectly(int key, int expectedSegmentCount)
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        var result = sut.TryAdd( key, "foo" );
        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 1 ),
                sut.SegmentCount.TestEquals( expectedSegmentCount ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (key, "foo") ] ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldAddSecondItemCorrectly_WhenItemIsAddedToInitializedSegment()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        var result = sut.TryAdd( 16, "bar" );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (16, "bar"), (17, "foo") ] ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldAddSecondItemCorrectly_WhenItemIsAddedToUninitializedSegment()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        var result = sut.TryAdd( 7, "bar" );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (7, "bar"), (17, "foo") ] ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldReturnFalse_WhenKeyAlreadyExists()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        var result = sut.TryAdd( 17, "bar" );

        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (17, "foo") ] ) )
            .Go();
    }

    [Fact]
    public void TryAdd_ShouldReturnFalse_WhenKeyIsNegative()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        var result = sut.TryAdd( -1, "foo" );
        Assertion.All(
                result.TestFalse(),
                sut.Count.TestEquals( 0 ),
                sut.SegmentCount.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                TestSequence( sut, [ ] ) )
            .Go();
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 1, 1 )]
    [InlineData( 2, 1 )]
    [InlineData( 7, 1 )]
    [InlineData( 8, 1 )]
    [InlineData( 9, 1 )]
    [InlineData( 15, 1 )]
    [InlineData( 16, 2 )]
    [InlineData( 128, 9 )]
    public void AddOrUpdate_ShouldAddFirstItemCorrectly(int key, int expectedSegmentCount)
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        var result = sut.AddOrUpdate( key, "foo" );
        Assertion.All(
                result.TestEquals( AddOrUpdateResult.Added ),
                sut.Count.TestEquals( 1 ),
                sut.SegmentCount.TestEquals( expectedSegmentCount ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (key, "foo") ] ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddSecondItemCorrectly_WhenItemIsAddedToInitializedSegment()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        var result = sut.AddOrUpdate( 16, "bar" );

        Assertion.All(
                result.TestEquals( AddOrUpdateResult.Added ),
                sut.Count.TestEquals( 2 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (16, "bar"), (17, "foo") ] ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldAddSecondItemCorrectly_WhenItemIsAddedToUninitializedSegment()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        var result = sut.AddOrUpdate( 7, "bar" );

        Assertion.All(
                result.TestEquals( AddOrUpdateResult.Added ),
                sut.Count.TestEquals( 2 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (7, "bar"), (17, "foo") ] ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldUpdateValue_WhenKeyAlreadyExists()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        var result = sut.AddOrUpdate( 17, "bar" );

        Assertion.All(
                result.TestEquals( AddOrUpdateResult.Updated ),
                sut.Count.TestEquals( 1 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (17, "bar") ] ) )
            .Go();
    }

    [Fact]
    public void AddOrUpdate_ShouldThrowArgumentOutOfRangeException_WhenKeyIsNegative()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        var action = Lambda.Of( () => sut.AddOrUpdate( -1, "foo" ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 1, 1 )]
    [InlineData( 2, 1 )]
    [InlineData( 7, 1 )]
    [InlineData( 8, 1 )]
    [InlineData( 9, 1 )]
    [InlineData( 15, 1 )]
    [InlineData( 16, 2 )]
    [InlineData( 128, 9 )]
    public void GetValueRefOrAddDefault_ShouldAddFirstItemCorrectly(int key, int expectedSegmentCount)
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );

        ref var result = ref sut.GetValueRefOrAddDefault( key, out var exists );
        result = "foo";

        Assertion.All(
                exists.TestFalse(),
                sut.Count.TestEquals( 1 ),
                sut.SegmentCount.TestEquals( expectedSegmentCount ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (key, "foo") ] ) )
            .Go();
    }

    [Fact]
    public void GetValueRefOrAddDefault_ShouldAddSecondItemCorrectly_WhenItemIsAddedToInitializedSegment()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        ref var result = ref sut.GetValueRefOrAddDefault( 16, out var exists );
        result = "bar";

        Assertion.All(
                exists.TestFalse(),
                sut.Count.TestEquals( 2 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (16, "bar"), (17, "foo") ] ) )
            .Go();
    }

    [Fact]
    public void GetValueRefOrAddDefault_ShouldAddSecondItemCorrectly_WhenItemIsAddedToUninitializedSegment()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        ref var result = ref sut.GetValueRefOrAddDefault( 7, out var exists );
        result = "bar";

        Assertion.All(
                exists.TestFalse(),
                sut.Count.TestEquals( 2 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (7, "bar"), (17, "foo") ] ) )
            .Go();
    }

    [Fact]
    public void GetValueRefOrAddDefault_ShouldReturnRefToValue_WhenKeyExists()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        ref var result = ref sut.GetValueRefOrAddDefault( 17, out var exists );
        var oldResult = result;
        result = "bar";

        Assertion.All(
                exists.TestTrue(),
                oldResult.TestEquals( "foo" ),
                sut.Count.TestEquals( 1 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (17, "bar") ] ) )
            .Go();
    }

    [Fact]
    public void GetValueRefOrAddDefault_ShouldThrowArgumentOutOfRangeException_WhenKeyIsNegative()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        var action = Lambda.Of( () => sut.GetValueRefOrAddDefault( -1, out _ ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void GetValueRefOrNullRef_ShouldReturnRefToValue_WhenKeyExists()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        ref var result = ref sut.GetValueRefOrNullRef( 17 );
        Unsafe.IsNullRef( ref result ).TestFalse().Go();
        var oldResult = result;
        result = "bar";

        Assertion.All(
                oldResult.TestEquals( "foo" ),
                sut.Count.TestEquals( 1 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (17, "bar") ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 16 )]
    [InlineData( 18 )]
    [InlineData( 32 )]
    public void GetValueRefOrNullRef_ShouldReturnNullRef_WhenKeyDoesNotExist(int key)
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 17, "foo" );

        ref var result = ref sut.GetValueRefOrNullRef( key );

        Unsafe.IsNullRef( ref result ).TestTrue().Go();
    }

    [Theory]
    [InlineData( -1, false )]
    [InlineData( 0, false )]
    [InlineData( 2, false )]
    [InlineData( 3, true )]
    [InlineData( 4, false )]
    [InlineData( 16, false )]
    [InlineData( 32, false )]
    [InlineData( 33, true )]
    [InlineData( 34, true )]
    [InlineData( 35, false )]
    [InlineData( 48, false )]
    public void ContainsKey_ShouldReturnTrue_WhenKeyExists(int key, bool expected)
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 3, "foo" );
        sut.TryAdd( 33, "bar" );
        sut.TryAdd( 34, "qux" );

        var result = sut.ContainsKey( key );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 3, "foo" )]
    [InlineData( 33, "bar" )]
    [InlineData( 34, "qux" )]
    public void TryGetValue_ShouldReturnExistingItem(int key, string expected)
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 3, "foo" );
        sut.TryAdd( 33, "bar" );
        sut.TryAdd( 34, "qux" );

        var result = sut.TryGetValue( key, out var outResult );

        Assertion.All( result.TestTrue(), outResult.TestEquals( expected ) ).Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 2 )]
    [InlineData( 4 )]
    [InlineData( 16 )]
    [InlineData( 32 )]
    [InlineData( 35 )]
    [InlineData( 48 )]
    public void TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist(int key)
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 3, "foo" );
        sut.TryAdd( 33, "bar" );
        sut.TryAdd( 34, "qux" );

        var result = sut.TryGetValue( key, out var outResult );

        Assertion.All( result.TestFalse(), outResult.TestNull() ).Go();
    }

    [Fact]
    public void Remove_ShouldRemoveExistingKey()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 3, "foo" );
        sut.TryAdd( 15, "bar" );
        sut.TryAdd( 16, "qux" );

        var result = sut.Remove( 16 );

        Assertion.All(
                result.TestTrue(),
                sut.Count.TestEquals( 2 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (3, "foo"), (15, "bar") ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 2 )]
    [InlineData( 4 )]
    [InlineData( 16 )]
    [InlineData( 32 )]
    [InlineData( 35 )]
    [InlineData( 48 )]
    public void Remove_ShouldReturnFalse_WhenKeyDoesNotExist(int key)
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 3, "foo" );
        sut.TryAdd( 33, "bar" );
        sut.TryAdd( 34, "qux" );

        var result = sut.Remove( key );

        Assertion.All( sut.Count.TestEquals( 3 ), result.TestFalse() ).Go();
    }

    [Fact]
    public void Remove_WithOutParameter_ShouldRemoveExistingKey()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 3, "foo" );
        sut.TryAdd( 15, "bar" );
        sut.TryAdd( 16, "qux" );

        var result = sut.Remove( 16, out var removed );

        Assertion.All(
                result.TestTrue(),
                removed.TestEquals( "qux" ),
                sut.Count.TestEquals( 2 ),
                sut.SegmentCount.TestEquals( 2 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (3, "foo"), (15, "bar") ] ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    [InlineData( 2 )]
    [InlineData( 4 )]
    [InlineData( 16 )]
    [InlineData( 32 )]
    [InlineData( 35 )]
    [InlineData( 48 )]
    public void Remove_WithOutParameter_ShouldReturnFalse_WhenKeyDoesNotExist(int key)
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 3, "foo" );
        sut.TryAdd( 33, "bar" );
        sut.TryAdd( 34, "qux" );

        var result = sut.Remove( key, out var removed );

        Assertion.All( sut.Count.TestEquals( 3 ), result.TestFalse(), removed.TestNull() ).Go();
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntriesAndSegments()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 3, "foo" );
        sut.TryAdd( 33, "bar" );
        sut.TryAdd( 34, "qux" );

        sut.Clear();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.SegmentCount.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                TestSequence( sut, [ ] ) )
            .Go();
    }

    [Fact]
    public void TrimExcess_ShouldRemoveAllSegments_WhenDictionaryIsEmpty()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 16, "foo" );
        sut.Remove( 16 );

        sut.TrimExcess();

        Assertion.All(
                sut.Count.TestEquals( 0 ),
                sut.SegmentCount.TestEquals( 0 ),
                sut.IsEmpty.TestTrue(),
                TestSequence( sut, [ ] ) )
            .Go();
    }

    [Fact]
    public void TrimExcess_ShouldDoNothing_WhenAllSegmentsAreInUse()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 4, "foo" );
        sut.TryAdd( 20, "bar" );
        sut.TryAdd( 36, "qux" );

        sut.TrimExcess();

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.SegmentCount.TestEquals( 3 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (4, "foo"), (20, "bar"), (36, "qux") ] ) )
            .Go();
    }

    [Fact]
    public void TrimExcess_ShouldRemoveUnusedTailSegmentsAndResetUnusedMiddleSegments()
    {
        var sut = SegmentedSparseDictionary<string>.Create( 16 );
        sut.TryAdd( 15, "lorem" );
        sut.TryAdd( 36, "foo" );
        sut.TryAdd( 37, "bar" );
        sut.TryAdd( 48, "ipsum" );
        sut.TryAdd( 64, "qux" );
        sut.TryAdd( 86, "dolor" );
        sut.Remove( 15 );
        sut.Remove( 48 );
        sut.Remove( 86 );

        sut.TrimExcess();

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.SegmentCount.TestEquals( 5 ),
                sut.IsEmpty.TestFalse(),
                TestSequence( sut, [ (36, "foo"), (37, "bar"), (64, "qux") ] ) )
            .Go();
    }

    [Pure]
    private static Assertion TestSequence<T>(SegmentedSparseDictionary<T> dictionary, (int Key, T Value)[] expected)
    {
        var list = new List<KeyValuePair<int, T>>();
        foreach ( var entry in dictionary )
            list.Add( entry );

        return list.TestSequence( expected.Select( static x => KeyValuePair.Create( x.Key, x.Value ) ) );
    }
}
