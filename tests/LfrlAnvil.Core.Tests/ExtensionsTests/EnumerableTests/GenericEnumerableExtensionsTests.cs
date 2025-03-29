using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Tests.ExtensionsTests.EnumerableTests;

[GenericTestClass( typeof( GenericEnumerableExtensionsTestsData<> ) )]
public abstract class GenericEnumerableExtensionsTests<T> : TestsBase
{
    protected readonly IEqualityComparer<T> EqualityComparer = EqualityComparer<T>.Default;
    protected readonly IComparer<T> Comparer = Comparer<T>.Default;

    [Fact]
    public void EmptyIfNull_ShouldReturnSource_WhenNotNull()
    {
        var sut = Fixture.CreateMany<T>().ToList();
        var result = sut.EmptyIfNull();
        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void EmptyIfNull_ShouldReturnNull_WhenSourceIsNull()
    {
        IEnumerable<T>? sut = null;
        var result = sut.EmptyIfNull();
        result.TestEmpty().Go();
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrue_WhenSourceIsNull()
    {
        IEnumerable<T>? sut = null;
        var result = sut.IsNullOrEmpty();
        result.TestTrue().Go();
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrue_WhenSourceHasNoElements()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.IsNullOrEmpty();
        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetIsEmptyData ) )]
    public void IsNullOrEmpty_ShouldReturnFalse_WhenSourceHasSomeElements(int count)
    {
        var sut = Fixture.CreateMany<T>( count );
        var result = sut.IsNullOrEmpty();
        result.TestFalse().Go();
    }

    [Fact]
    public void IsEmpty_ShouldReturnTrue_WhenSourceHasNoElements()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.IsEmpty();
        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetIsEmptyData ) )]
    public void IsEmpty_ShouldReturnFalse_WhenSourceHasSomeElements(int count)
    {
        var sut = Fixture.CreateMany<T>( count );
        var result = sut.IsEmpty();
        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsAtLeastData ) )]
    public void ContainsAtLeast_ShouldReturnCorrectResult(int sourceCount, int minCount, bool expected)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).Where( _ => true );
        var result = sut.ContainsAtLeast( minCount );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsAtLeastData ) )]
    public void ContainsAtLeast_WithMaterializedSource_ShouldReturnCorrectResult(int sourceCount, int minCount, bool expected)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList().AsEnumerable();
        var result = sut.ContainsAtLeast( minCount );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsAtMostData ) )]
    public void ContainsAtMost_ShouldReturnCorrectResult(int sourceCount, int maxCount, bool expected)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).Where( _ => true );
        var result = sut.ContainsAtMost( maxCount );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsAtMostData ) )]
    public void ContainsAtMost_WithMaterializedSource_ShouldReturnCorrectResult(int sourceCount, int maxCount, bool expected)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList().AsEnumerable();
        var result = sut.ContainsAtMost( maxCount );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForMaxCountLessThanMinCountData ) )]
    public void ContainsInRange_ShouldReturnFalse_WhenMaxCountIsLessThanMinCount(int count)
    {
        var (max, min) = Fixture.CreateManyDistinctSorted<int>( count: 2 );
        var sut = Fixture.CreateMany<T>( count ).Where( _ => true );

        var result = sut.ContainsInRange( min, max );

        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForMaxCountLessThanMinCountData ) )]
    public void ContainsInRange_WithMaterializedSource_ShouldReturnFalse_WhenMaxCountIsLessThanMinCount(int count)
    {
        var (max, min) = Fixture.CreateManyDistinctSorted<int>( count: 2 );
        var sut = Fixture.CreateMany<T>( count ).ToList().AsEnumerable();

        var result = sut.ContainsInRange( min, max );

        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForZeroMinCountData ) )]
    public void ContainsInRange_ShouldReturnCorrectResult_WhenMinCountIsZero(int count, int maxCount, bool expected)
    {
        var sut = Fixture.CreateMany<T>( count ).Where( _ => true );
        var result = sut.ContainsInRange( 0, maxCount );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForZeroMinCountData ) )]
    public void ContainsInRange_WithMaterializedSource_ShouldReturnCorrectResult_WhenMinCountIsZero(int count, int maxCount, bool expected)
    {
        var sut = Fixture.CreateMany<T>( count ).ToList().AsEnumerable();
        var result = sut.ContainsInRange( 0, maxCount );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForNegativeMinCountData ) )]
    public void ContainsInRange_ShouldReturnCorrectResult_WhenMinCountIsNegative(int count, int maxCount, bool expected)
    {
        var minCount = -Fixture.Create<int>( x => x > 0 );
        var sut = Fixture.CreateMany<T>( count ).Where( _ => true );

        var result = sut.ContainsInRange( minCount, maxCount );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForNegativeMinCountData ) )]
    public void ContainsInRange_WithMaterializedSource_ShouldReturnCorrectResult_WhenMinCountIsNegative(
        int count,
        int maxCount,
        bool expected)
    {
        var minCount = -Fixture.Create<int>( x => x > 0 );
        var sut = Fixture.CreateMany<T>( count ).ToList().AsEnumerable();

        var result = sut.ContainsInRange( minCount, maxCount );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForCountLessThanMinCountData ) )]
    public void ContainsInRange_ShouldReturnFalse_WhenSourceCountIsLessThanMinCount(int count, int minCount)
    {
        var sut = Fixture.CreateMany<T>( count ).Where( _ => true );
        var result = sut.ContainsInRange( minCount, minCount + 1 );
        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForCountLessThanMinCountData ) )]
    public void ContainsInRange_WithMaterializedSource_ShouldReturnFalse_WhenSourceCountIsLessThanMinCount(int count, int minCount)
    {
        var sut = Fixture.CreateMany<T>( count ).ToList().AsEnumerable();
        var result = sut.ContainsInRange( minCount, minCount + 1 );
        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForCountGreaterThanMaxCountData ) )]
    public void ContainsInRange_ShouldReturnFalse_WhenSourceCountIsGreaterThanMaxCount(int count, int maxCount)
    {
        var sut = Fixture.CreateMany<T>( count ).Where( _ => true );
        var result = sut.ContainsInRange( maxCount - 1, maxCount );
        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForCountGreaterThanMaxCountData ) )]
    public void ContainsInRange_WithMaterializedSource_ShouldReturnFalse_WhenSourceCountIsGreaterThanMaxCount(int count, int maxCount)
    {
        var sut = Fixture.CreateMany<T>( count ).ToList().AsEnumerable();
        var result = sut.ContainsInRange( maxCount - 1, maxCount );
        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForCountBetweenMinAndMaxData ) )]
    public void ContainsInRange_ShouldReturnTrue_WhenSourceCountIsBetweenMinAndMaxCount(int sourceCount, int minCount, int maxCount)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).Where( _ => true );
        var result = sut.ContainsInRange( minCount, maxCount );
        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForCountBetweenMinAndMaxData ) )]
    public void ContainsInRange_WithMaterializedSource_ShouldReturnTrue_WhenSourceCountIsBetweenMinAndMaxCount(
        int sourceCount,
        int minCount,
        int maxCount)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList().AsEnumerable();
        var result = sut.ContainsInRange( minCount, maxCount );
        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsExactlyForNegativeCountData ) )]
    public void ContainsExactly_ShouldReturnFalse_WhenCountIsNegative(int sourceCount)
    {
        var count = -Fixture.Create<int>( x => x > 0 );
        var sut = Fixture.CreateMany<T>( sourceCount ).Where( _ => true );

        var result = sut.ContainsExactly( count );

        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsExactlyForNegativeCountData ) )]
    public void ContainsExactly_WithMaterializedSource_ShouldReturnFalse_WhenCountIsNegative(int sourceCount)
    {
        var count = -Fixture.Create<int>( x => x > 0 );
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList().AsEnumerable();

        var result = sut.ContainsExactly( count );

        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsExactlyForNonNegativeCountData ) )]
    public void ContainsExactly_ShouldReturnCorrectResult_WhenCountIsNotNegative(int sourceCount, int count, bool expected)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).Where( _ => true );
        var result = sut.ContainsExactly( count );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsExactlyForNonNegativeCountData ) )]
    public void ContainsExactly_WithMaterializedSource_ShouldReturnCorrectResult_WhenCountIsNotNegative(
        int sourceCount,
        int count,
        bool expected)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList().AsEnumerable();
        var result = sut.ContainsExactly( count );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetFlattenData ) )]
    public void Flatten_ShouldReturnCorrectResult(IReadOnlyList<Pair<T, IEnumerable<T>>> data, IEnumerable<Pair<T, T>> expected)
    {
        var sut = data.Select( d => d.First );
        var result = sut.Flatten( x => data.First( y => x!.Equals( y.First ) ).Second );
        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void Flatten_WithoutParameters_ShouldReturnCorrectResult()
    {
        var expected = Fixture.CreateManyDistinct<T>( count: 9 );
        var sut = new[] { expected.Take( 3 ), expected.Skip( 3 ).Take( 3 ), expected.Skip( 6 ) };
        var result = sut.Flatten();
        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void TryMin_ShouldReturnFalseAndDefaultResult_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();

        var result = sut.TryMin( out var min );

        Assertion.All(
                result.TestFalse(),
                min.TestEquals( default ) )
            .Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinData ) )]
    public void TryMin_ShouldReturnTrueAndCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expected)
    {
        var result = sut.TryMin( out var min );

        Assertion.All(
                result.TestTrue(),
                min.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void TryMax_ShouldReturnFalseAndDefaultResult_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();

        var result = sut.TryMax( out var max );

        Assertion.All(
                result.TestFalse(),
                max.TestEquals( default ) )
            .Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMaxData ) )]
    public void TryMax_ShouldReturnTrueAndCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expected)
    {
        var result = sut.TryMax( out var max );

        Assertion.All(
                result.TestTrue(),
                max.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void MinMax_ShouldThrowInvalidOperationException_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var action = Lambda.Of( () => sut.MinMax() );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinMaxData ) )]
    public void MinMax_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expectedMin, T expectedMax)
    {
        var result = sut.MinMax();

        Assertion.All(
                result.Min.TestEquals( expectedMin ),
                result.Max.TestEquals( expectedMax ) )
            .Go();
    }

    [Fact]
    public void TryMinMax_ShouldReturnNull_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.TryMinMax();
        result.TestNull().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinMaxData ) )]
    public void TryMinMax_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expectedMin, T expectedMax)
    {
        var result = sut.TryMinMax();
        result.TestNotNull( r => Assertion.All( r.Min.TestEquals( expectedMin ), r.Max.TestEquals( expectedMax ) ) ).Go();
    }

    [Fact]
    public void ContainsDuplicates_ShouldReturnFalse_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.ContainsDuplicates();
        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsDuplicatesData ) )]
    public void ContainsDuplicates_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> sut, bool expected)
    {
        var result = sut.ContainsDuplicates();
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Repeat_ShouldThrowArgumentOutOfRangeException_WhenCountIsNegative()
    {
        var count = -Fixture.Create<int>( x => x > 0 );
        var sut = Fixture.CreateMany<T>();

        var action = Lambda.Of( () => sut.Repeat( count ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetRepeatForZeroOrOneCountData ) )]
    public void Repeat_ShouldReturnEmpty_WhenCountIsZero(int sourceCount)
    {
        var sut = Fixture.CreateMany<T>( sourceCount );
        var result = sut.Repeat( 0 );
        result.TestEmpty().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetRepeatForZeroOrOneCountData ) )]
    public void Repeat_ShouldReturnSource_WhenCountIsOne(int sourceCount)
    {
        var sut = Fixture.CreateMany<T>( sourceCount ).ToList();
        var result = sut.Repeat( 1 );
        result.TestRefEquals( sut ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetRepeatForCountGreaterThanOneData ) )]
    public void Repeat_ShouldReturnCorrectResult_WhenCountIsGreaterThanOne(IEnumerable<T> sut, int count, IEnumerable<T> expected)
    {
        var result = sut.Repeat( count );
        result.TestSequence( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetRepeatForMemoizationWithCountGreaterThanOneData ) )]
    public void Repeat_ShouldNotEvaluateSource_WhenCountIsGreaterThanOne_BeforeResultIsEvaluated(int count)
    {
        var @delegate = Substitute.For<Func<int, T>>().WithAnyArgs( _ => Fixture.Create<T>() );

        var sut = Enumerable.Range( 0, 1 ).Select( @delegate );

        _ = sut.Repeat( count );

        @delegate.CallCount().TestEquals( 0 ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetRepeatForMemoizationWithCountGreaterThanOneData ) )]
    public void Repeat_ShouldMemoizeSource_WhenCountIsGreaterThanOne(int count)
    {
        var @delegate = Substitute.For<Func<int, T>>().WithAnyArgs( _ => Fixture.Create<T>() );

        var sut = Enumerable.Range( 0, 1 ).Select( @delegate );

        _ = sut.Repeat( count ).ToList();

        @delegate.CallCount().TestEquals( 1 ).Go();
    }

    [Fact]
    public void Materialize_ShouldReturnSource_WhenSourceImplementsReadOnlyCollectionInterface()
    {
        var sut = new TestCollection<T>();
        var result = sut.Materialize();
        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void Materialize_ShouldMaterializeMemoizedCollection()
    {
        var @delegate = Substitute.For<Func<int, T>>().WithAnyArgs( _ => Fixture.Create<T>() );

        var sut = Enumerable.Range( 0, 3 )
            .Select( @delegate )
            .Memoize();

        var result = sut.Materialize();

        Assertion.All(
                @delegate.CallCount().TestEquals( result.Count ),
                result.TestNotRefEquals( sut ),
                result.TestSequence( sut ) )
            .Go();
    }

    [Fact]
    public void Materialize_ShouldReturnCorrectResult_WhenSourceIsNotYetMaterialized()
    {
        var sut = Fixture.CreateMany<T>().Select( x => x );

        var result = sut.Materialize();

        Assertion.All(
                result.TestNotRefEquals( sut ),
                result.TestSequence( sut ) )
            .Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMemoizeData ) )]
    public void Memoize_ShouldMaterializeSourceAfterFirstEnumeration(int sourceCount, int iterationCount)
    {
        var @delegate = Substitute.For<Func<int, T>>().WithAnyArgs( _ => Fixture.Create<T>() );

        var sut = Enumerable.Range( 0, sourceCount )
            .Select( @delegate )
            .Memoize();

        var materialized = new List<IEnumerable<T>>();
        for ( var i = 0; i < iterationCount; ++i )
            materialized.Add( sut.ToList() );

        @delegate.CallCount().TestEquals( iterationCount == 0 ? 0 : sourceCount ).Go();
    }

    [Fact]
    public void Memoize_ShouldReturnSource_WhenSourceIsAlreadyMemoized()
    {
        var sut = Fixture.CreateMany<T>().Memoize();
        var result = sut.Memoize();
        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void IsMaterialized_ShouldReturnTrue_WhenSourceIsReadOnlyCollection()
    {
        var sut = Fixture.CreateMany<T>().ToList();
        var result = sut.IsMaterialized();
        result.TestTrue().Go();
    }

    [Fact]
    public void IsMaterialized_ShouldReturnFalse_WhenSourceIsNotReadOnlyCollection()
    {
        var sut = Fixture.CreateMany<T>().Select( x => x );
        var result = sut.IsMaterialized();
        result.TestFalse().Go();
    }

    [Fact]
    public void IsMaterialized_ShouldReturnFalse_WhenSourceIsMemoizedAndNotMaterialized()
    {
        var sut = Fixture.CreateMany<T>().Memoize();
        var result = sut.IsMaterialized();
        result.TestFalse().Go();
    }

    [Fact]
    public void IsMaterialized_ShouldReturnTrue_WhenSourceIsMemoizedAndMaterialized()
    {
        var sut = Fixture.CreateMany<T>().Memoize();
        _ = sut.Materialize();
        var result = sut.IsMaterialized();
        result.TestTrue().Go();
    }

    [Fact]
    public void IsMemoized_ShouldReturnTrue_WhenSourceIsMemoized()
    {
        var sut = Fixture.CreateMany<T>().Memoize();
        var result = sut.IsMemoized();
        result.TestTrue().Go();
    }

    [Fact]
    public void IsMemoized_ShouldReturnFalse_WhenSourceIsNotMemoizedAndNotMaterialized()
    {
        var sut = Fixture.CreateMany<T>();
        var result = sut.IsMemoized();
        result.TestFalse().Go();
    }

    [Fact]
    public void IsMemoized_ShouldReturnFalse_WhenSourceIsMaterialized()
    {
        var sut = Fixture.CreateMany<T>().ToList();
        var result = sut.IsMemoized();
        result.TestFalse().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult(IEnumerable<T> sut, IEnumerable<T> other, bool expected)
    {
        var result = sut.SetEquals( other );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult_WhenSourceIsHashSet(IEnumerable<T> sut, IEnumerable<T> other, bool expected)
    {
        sut = new HashSet<T>( sut );
        var result = sut.SetEquals( other );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult_WhenOtherIsHashSet(IEnumerable<T> sut, IEnumerable<T> other, bool expected)
    {
        other = new HashSet<T>( other );
        var result = sut.SetEquals( other );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetSetEqualsData ) )]
    public void SetEquals_ShouldReturnCorrectResult_WhenSourceAndOtherAreHashSet(
        IEnumerable<T> sut,
        IEnumerable<T> other,
        bool expected)
    {
        var sutSet = new HashSet<T>( sut );
        other = new HashSet<T>( other, sutSet.Comparer );
        var result = sutSet.AsEnumerable().SetEquals( other );
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void VisitMany_ShouldReturnEmptyCollection_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<VisitManyNode<T>>();
        var result = sut.VisitMany( n => n.Children );
        result.TestEmpty().Go();
    }

    [Fact]
    public void VisitMany_ShouldReturnResultAccordingToBreadthFirstTraversal()
    {
        var expected = Fixture.CreateMany<T>( count: 10 ).ToList();

        var sut = new[]
        {
            new VisitManyNode<T>
            {
                Value = expected[0],
                Children = new List<VisitManyNode<T>>
                {
                    new() { Value = expected[3] },
                    new()
                    {
                        Value = expected[4],
                        Children = new List<VisitManyNode<T>>
                        {
                            new() { Value = expected[6] },
                            new() { Value = expected[7] }
                        }
                    }
                }
            },
            new VisitManyNode<T> { Value = expected[1] },
            new VisitManyNode<T>
            {
                Value = expected[2],
                Children = new List<VisitManyNode<T>>
                {
                    new()
                    {
                        Value = expected[5],
                        Children = new List<VisitManyNode<T>>
                        {
                            new() { Value = expected[8] },
                            new() { Value = expected[9] }
                        }
                    }
                }
            }
        };

        var result = sut.VisitMany( n => n.Children ).Select( n => n.Value );

        result.TestSequence( expected.Select( x => ( T? )x ) ).Go();
    }

    [Fact]
    public void VisitMany_WithStopPredicate_ShouldReturnEmptyCollection_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<VisitManyNode<T>>();
        var result = sut.VisitMany( n => n.Children, n => EqualityComparer.Equals( n.Value, default ) );
        result.TestEmpty().Go();
    }

    [Fact]
    public void VisitMany_WithStopPredicate_ShouldReturnResultAccordingToBreadthFirstTraversal()
    {
        var sourceOfValues = Fixture.CreateManyDistinct<T>( count: 10 ).ToList();
        var valuesToStopAt = new HashSet<T>
        {
            sourceOfValues[0],
            sourceOfValues[5]
        };

        var expected = new[] { sourceOfValues[0], sourceOfValues[1], sourceOfValues[2], sourceOfValues[5] };

        var sut = new[]
        {
            new VisitManyNode<T>
            {
                Value = sourceOfValues[0],
                Children = new List<VisitManyNode<T>>
                {
                    new() { Value = sourceOfValues[3] },
                    new()
                    {
                        Value = sourceOfValues[4],
                        Children = new List<VisitManyNode<T>>
                        {
                            new() { Value = sourceOfValues[6] },
                            new() { Value = sourceOfValues[7] }
                        }
                    }
                }
            },
            new VisitManyNode<T> { Value = sourceOfValues[1] },
            new VisitManyNode<T>
            {
                Value = sourceOfValues[2],
                Children = new List<VisitManyNode<T>>
                {
                    new()
                    {
                        Value = sourceOfValues[5],
                        Children = new List<VisitManyNode<T>>
                        {
                            new() { Value = sourceOfValues[8] },
                            new() { Value = sourceOfValues[9] }
                        }
                    }
                }
            }
        };

        var result = sut.VisitMany( n => n.Children, n => valuesToStopAt.Contains( n.Value! ) ).Select( n => n.Value );

        result.TestSequence( expected.Select( x => ( T? )x ) ).Go();
    }

    [Fact]
    public void TryAggregate_ShouldReturnFalseAndDefaultResult_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();

        var result = sut.TryAggregate( (_, c) => c, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void TryAggregate_ShouldReturnTrueAndCorrectResult_WhenSourceIsNotEmpty()
    {
        var expected = Fixture.Create<T>();
        var sut = Fixture.CreateMany<T>().Append( expected );

        var result = sut.TryAggregate( (_, c) => c, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void MinMaxBy_ShouldThrowInvalidOperationException_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<Contained<T>>();
        var action = Lambda.Of( () => sut.MinMaxBy( c => c.Value ) );
        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinMaxData ) )]
    public void MinMaxBy_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> values, T expectedMin, T expectedMax)
    {
        var sut = values.Select( v => new Contained<T> { Value = v } );
        var result = sut.MinMaxBy( c => c.Value );

        Assertion.All(
                result.Min.Value.TestEquals( expectedMin ),
                result.Max.Value.TestEquals( expectedMax ) )
            .Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinMaxData ) )]
    public void MinMaxBy_WithCustomComparer_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(
        IEnumerable<T> values,
        T expectedMin,
        T expectedMax)
    {
        var sut = values.Select( v => new Contained<T> { Value = v } );
        var result = sut.MinMaxBy( c => c.Value, Comparer<T?>.Default );

        Assertion.All(
                result.Min.Value.TestEquals( expectedMin ),
                result.Max.Value.TestEquals( expectedMax ) )
            .Go();
    }

    [Fact]
    public void TryMaxBy_ShouldReturnFalseAndDefaultResult_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<Contained<T>>();

        var result = sut.TryMaxBy( c => c.Value, out var max );

        Assertion.All(
                result.TestFalse(),
                max.TestEquals( default ) )
            .Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMaxData ) )]
    public void TryMaxBy_ShouldReturnTrueAndCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
    {
        var sut = values.Select( v => new Contained<T> { Value = v } );

        var result = sut.TryMaxBy( c => c.Value, out var max );

        Assertion.All(
                result.TestTrue(),
                max!.Value.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void TryMinBy_ShouldReturnFalseAndDefaultResult_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<Contained<T>>();

        var result = sut.TryMinBy( c => c.Value, out var min );

        Assertion.All(
                result.TestFalse(),
                min.TestEquals( default ) )
            .Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinData ) )]
    public void TryMinBy_ShouldReturnTrueAndCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
    {
        var sut = values.Select( v => new Contained<T> { Value = v } );

        var result = sut.TryMinBy( c => c.Value, out var min );

        Assertion.All(
                result.TestTrue(),
                min!.Value.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void TryMinMaxBy_ShouldReturnNull_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<Contained<T>>();
        var result = sut.TryMinMaxBy( c => c.Value );
        result.TestNull().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinMaxData ) )]
    public void TryMinMaxBy_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> values, T expectedMin, T expectedMax)
    {
        var sut = values.Select( v => new Contained<T> { Value = v } );
        var result = sut.TryMinMaxBy( c => c.Value );
        result.TestNotNull( r => Assertion.All( r.Min.Value.TestEquals( expectedMin ), r.Max.Value.TestEquals( expectedMax ) ) ).Go();
    }

    [Fact]
    public void TryMinMaxBy_WithDifferentSelectors_ShouldReturnNull_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<Contained<T>>();
        var result = sut.TryMinMaxBy( c => c.Value, c => c.Value );
        result.TestNull().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinMaxData ) )]
    public void TryMinMaxBy_WithDifferentSelectors_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(
        IEnumerable<T> values,
        T expectedMin,
        T expectedMax)
    {
        var sut = values.Select( v => new Contained<T> { Value = v } );
        var result = sut.TryMinMaxBy( c => c.Value, c => c.Value );
        result.TestNotNull( r => Assertion.All( r.Min.Value.TestEquals( expectedMin ), r.Max.Value.TestEquals( expectedMax ) ) ).Go();
    }

    [Fact]
    public void LeftJoin_ShouldReturnCorrectResult()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 5 );

        var sut = new[] { values[0], values[1], values[2], values[1], values[3], values[0] }
            .Select( v => new Contained<T> { Value = v } )
            .ToList();

        var inner = new[] { values[1], values[2], values[1], values[3], values[4] }
            .Select( v => new Contained<T> { Value = v } )
            .ToList();

        var expected = new (Contained<T> o, Contained<T>? i )[]
        {
            (sut[0], null),
            (sut[1], inner[0]),
            (sut[1], inner[2]),
            (sut[2], inner[1]),
            (sut[3], inner[0]),
            (sut[3], inner[2]),
            (sut[4], inner[3]),
            (sut[0], null)
        };

        var result = sut.LeftJoin(
            inner,
            o => o.Value,
            i => i.Value,
            (o, i) => (o, i) );

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void FullJoin_ShouldReturnCorrectResult()
    {
        var values = Fixture.CreateManyDistinct<T>( count: 5 );

        var sut = new[] { values[0], values[1], values[2], values[1], values[3], values[0] }
            .Select( v => new Contained<T> { Value = v } )
            .ToList();

        var inner = new[] { values[1], values[2], values[1], values[3], values[4] }
            .Select( v => new Contained<T> { Value = v } )
            .ToList();

        var expected = new (Contained<T>? o, Contained<T>? i )[]
        {
            (sut[0], null),
            (sut[1], inner[0]),
            (sut[1], inner[2]),
            (sut[2], inner[1]),
            (sut[3], inner[0]),
            (sut[3], inner[2]),
            (sut[4], inner[3]),
            (sut[0], null),
            (null, inner[4])
        };

        var result = sut.FullJoin(
            inner,
            o => o.Value,
            i => i.Value,
            (o, i) => (o, i) );

        result.TestSequence( expected ).Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetSliceData ) )]
    public void Slice_ShouldReturnCorrectResult(IEnumerable<T> values, int startIndex, int length, T[] expected)
    {
        var result = values.Slice( startIndex, length );
        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void IsOrdered_ShouldReturnTrue_WhenSourceIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.IsOrdered();
        result.TestTrue().Go();
    }

    [Fact]
    public void IsOrdered_ShouldReturnTrue_WhenSourceContainsOnlyOneElement()
    {
        var sut = Fixture.CreateMany<T>( count: 1 );
        var result = sut.IsOrdered();
        result.TestTrue().Go();
    }

    [Theory]
    [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetIsOrderedData ) )]
    public void IsOrdered_ShouldReturnCorrectResult_WhenSourceHasMoreThanOneElement(IEnumerable<T> sut, bool expected)
    {
        var result = sut.IsOrdered();
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Partition_ShouldReturnEmptyResult_WhenSourceIsEmpty()
    {
        var sut = Array.Empty<T>();
        var result = sut.Partition( _ => true );

        Assertion.All(
                result.Items.TestEmpty(),
                result.PassedItems.TestEmpty(),
                result.PassedItemsSpan.TestEmpty(),
                result.FailedItems.TestEmpty(),
                result.FailedItemsSpan.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Partition_ShouldReturnResultWithPassedContainingElementsThatReturnedTrueInPredicate_AndFailedContainingOtherElements()
    {
        var sut = Fixture.CreateManyDistinct<T>( count: 10 );
        var expectedPassed = new[] { sut[0], sut[2], sut[4], sut[6], sut[8] };
        var expectedFailed = new[] { sut[1], sut[3], sut[5], sut[7], sut[9] };

        var index = 0;
        var result = sut.Partition( _ => (index++).IsEven() );

        Assertion.All(
                result.Items.Count.TestEquals( sut.Length ),
                result.Items.TestSetEqual( sut ),
                result.PassedItems.TestSequence( expectedPassed ),
                result.PassedItemsSpan.TestSequence( result.PassedItems ),
                result.FailedItems.Count().TestEquals( expectedFailed.Length ),
                result.FailedItems.TestSetEqual( expectedFailed ),
                result.FailedItemsSpan.TestSequence( result.FailedItems ) )
            .Go();
    }

    [Fact]
    public void BufferUntil_ShouldReturnEmptyResult_WhenSourceIsEmpty()
    {
        var sut = Array.Empty<T>();
        var result = sut.BufferUntil( (_, _) => true );
        result.TestEmpty().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void BufferUntil_ShouldReturnSingleResult_WhenSourceContainsOneElement(bool predicateResult)
    {
        var sut = Fixture.CreateManyDistinct<T>( count: 1 ).ToList();
        var result = sut.BufferUntil( (_, _) => predicateResult ).Select( b => b.ToArray() );
        result.TestSequence( [ (b, _) => b.TestSequence( sut ) ] ).Go();
    }

    [Fact]
    public void BufferUntil_ShouldReturnSingleResult_WhenSourceContainsElementsThatAllPass()
    {
        var sut = Fixture.CreateManyDistinct<T>( count: 3 ).ToList();
        var result = sut.BufferUntil( (_, _) => true ).Select( b => b.ToArray() );
        result.TestSequence( [ (b, _) => b.TestSequence( sut ) ] ).Go();
    }

    [Fact]
    public void BufferUntil_ShouldReturnManyResults_WhenSourceContainsElementsThatAllFail()
    {
        var sut = Fixture.CreateManyDistinct<T>( count: 3 ).ToList();
        var result = sut.BufferUntil( (_, _) => false ).Select( b => b.ToArray() );
        result.TestAll( (b, i) => b.TestSequence( [ sut[i] ] ) ).Go();
    }

    [Fact]
    public void BufferUntil_ShouldReturnManyResults_WhenSourceContainsElementsThatConditionallyPass()
    {
        var items = Fixture.CreateManyDistinctSorted<T>( count: 11 ).ToList();
        var expected = new[]
        {
            new[] { items[8], items[9], items[10] },
            new[] { items[7] },
            new[] { items[2], items[3], items[4], items[5], items[6] },
            new[] { items[0], items[1] }
        };

        var sut = expected.SelectMany( x => x ).ToList();

        var result = sut.BufferUntil( (a, b) => Comparer<T>.Default.Compare( a, b ) <= 0 ).Select( b => b.ToArray() );
        result.TestAll( (b, i) => b.TestSequence( expected[i] ) ).Go();
    }

    [Fact]
    public void TemporaryBuffer_ShouldBeCorrect_WhenEmpty()
    {
        var sut = default( TemporaryBuffer<T> );

        var result = new List<T>();
        foreach ( var e in sut )
            result.Add( e );

        Assertion.All(
                result.TestEmpty(),
                sut.Length.TestEquals( 0 ),
                sut.ToArray().TestEmpty(),
                sut.AsMemory().TestEmpty(),
                sut.AsSpan().TestEmpty(),
                sut.AsEnumerable().TestEmpty() )
            .Go();
    }

    [Fact]
    public void TemporaryBuffer_ShouldBeCorrect_WhenNotEmpty()
    {
        var source = Fixture.CreateManyDistinct<T>( count: 3 ).ToList();
        var sut = source.BufferUntil( (_, _) => true ).First();

        var result = new List<T>();
        foreach ( var e in sut )
            result.Add( e );

        Assertion.All(
                result.TestSequence( source ),
                sut.Length.TestEquals( source.Count ),
                sut.ToArray().TestSequence( source ),
                sut.AsMemory().TestSequence( source ),
                sut.AsSpan().TestSequence( source ),
                sut.AsEnumerable().TestSequence( source ) )
            .Go();
    }

    [Fact]
    public void ToMemory_ShouldReturnArrayAsMemory_WhenUnderlyingCollectionIsOfArrayType()
    {
        var sut = Fixture.CreateMany<T>( count: 3 ).ToArray();
        var result = sut.ToMemory();
        result.TestEquals( sut.AsMemory() ).Go();
    }

    [Fact]
    public void ToMemory_ShouldCreateNewArrayAndReturnItAsMemory_WhenUnderlyingCollectionIsNotEmptyAndNotOfArrayType()
    {
        var sut = Fixture.CreateMany<T>( count: 3 ).ToList();
        var result = sut.ToMemory();
        result.ToArray().TestSequence( sut ).Go();
    }

    [Fact]
    public void ToMemory_ShouldReturnEmptyMemory_WhenUnderlyingCollectionIsEmpty()
    {
        var sut = Enumerable.Empty<T>();
        var result = sut.ToMemory();
        result.TestEquals( ReadOnlyMemory<T>.Empty ).Go();
    }
}
