using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.EnumerableTests
{
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
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void EmptyIfNull_ShouldReturnNull_WhenSourceIsNull()
        {
            IEnumerable<T>? sut = null;
            var result = sut.EmptyIfNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenSourceIsNull()
        {
            IEnumerable<T>? sut = null;
            var result = sut.IsNullOrEmpty();
            result.Should().BeTrue();
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenSourceHasNoElements()
        {
            var sut = Enumerable.Empty<T>();
            var result = sut.IsNullOrEmpty();
            result.Should().BeTrue();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetIsEmptyData ) )]
        public void IsNullOrEmpty_ShouldReturnFalse_WhenSourceHasSomeElements(int count)
        {
            var sut = Fixture.CreateMany<T>( count );
            var result = sut.IsNullOrEmpty();
            result.Should().BeFalse();
        }

        [Fact]
        public void IsEmpty_ShouldReturnTrue_WhenSourceHasNoElements()
        {
            var sut = Enumerable.Empty<T>();
            var result = sut.IsEmpty();
            result.Should().BeTrue();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetIsEmptyData ) )]
        public void IsEmpty_ShouldReturnFalse_WhenSourceHasSomeElements(int count)
        {
            var sut = Fixture.CreateMany<T>( count );
            var result = sut.IsEmpty();
            result.Should().BeFalse();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsAtLeastData ) )]
        public void ContainsAtLeast_ShouldReturnCorrectResult(int sourceCount, int minCount, bool expected)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );
            var result = sut.ContainsAtLeast( minCount );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsAtMostData ) )]
        public void ContainsAtMost_ShouldReturnCorrectResult(int sourceCount, int maxCount, bool expected)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );
            var result = sut.ContainsAtMost( maxCount );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForMaxCountLessThanMinCountData ) )]
        public void ContainsInRange_ShouldReturnFalse_WhenMaxCountIsLessThanMinCount(int count)
        {
            var (max, min) = Fixture.CreateDistinctSortedCollection<int>( 2 );
            var sut = Fixture.CreateMany<T>( count );

            var result = sut.ContainsInRange( min, max );

            result.Should().BeFalse();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForZeroMinCountData ) )]
        public void ContainsInRange_ShouldReturnCorrectResult_WhenMinCountIsZero(int count, int maxCount, bool expected)
        {
            var sut = Fixture.CreateMany<T>( count );
            var result = sut.ContainsInRange( 0, maxCount );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForNegativeMinCountData ) )]
        public void ContainsInRange_ShouldReturnCorrectResult_WhenMinCountIsNegative(int count, int maxCount, bool expected)
        {
            var minCount = Fixture.CreateNegativeInt32();
            var sut = Fixture.CreateMany<T>( count );

            var result = sut.ContainsInRange( minCount, maxCount );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForCountLessThanMinCountData ) )]
        public void ContainsInRange_ShouldReturnFalse_WhenSourceCountIsLessThanMinCount(int count, int minCount)
        {
            var sut = Fixture.CreateMany<T>( count );
            var result = sut.ContainsInRange( minCount, minCount + 1 );
            result.Should().BeFalse();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForCountGreaterThanMaxCountData ) )]
        public void ContainsInRange_ShouldReturnFalse_WhenSourceCountIsGreaterThanMaxCount(int count, int maxCount)
        {
            var sut = Fixture.CreateMany<T>( count );
            var result = sut.ContainsInRange( maxCount - 1, maxCount );
            result.Should().BeFalse();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsInRangeForCountBetweenMinAndMaxData ) )]
        public void ContainsInRange_ShouldReturnTrue_WhenSourceCountIsBetweenMinAndMaxCount(int sourceCount, int minCount, int maxCount)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );
            var result = sut.ContainsInRange( minCount, maxCount );
            result.Should().BeTrue();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsExactlyForNegativeCountData ) )]
        public void ContainsExactly_ShouldReturnFalse_WhenCountIsNegative(int sourceCount)
        {
            var count = Fixture.CreateNegativeInt32();
            var sut = Fixture.CreateMany<T>( sourceCount );

            var result = sut.ContainsExactly( count );

            result.Should().BeFalse();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsExactlyForNonNegativeCountData ) )]
        public void ContainsExactly_ShouldReturnCorrectResult_WhenCountIsNotNegative(int sourceCount, int count, bool expected)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );
            var result = sut.ContainsExactly( count );
            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetFlattenData ) )]
        public void Flatten_ShouldReturnCorrectResult(IReadOnlyList<Pair<T, IEnumerable<T>>> data, IEnumerable<Pair<T, T>> expected)
        {
            var sut = data.Select( d => d.First );
            var result = sut.Flatten( x => data.First( y => x!.Equals( y.First ) ).Second );
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void TryMin_ShouldReturnFalseAndDefaultResult_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<T>();

            var result = sut.TryMin( out var min );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                min.Should().Be( default( T ) );
            }
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinData ) )]
        public void TryMin_ShouldReturnTrueAndCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expected)
        {
            var result = sut.TryMin( out var min );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                min.Should().Be( expected );
            }
        }

        [Fact]
        public void TryMax_ShouldReturnFalseAndDefaultResult_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<T>();

            var result = sut.TryMax( out var max );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                max.Should().Be( default( T ) );
            }
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMaxData ) )]
        public void TryMax_ShouldReturnTrueAndCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expected)
        {
            var result = sut.TryMax( out var max );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                max.Should().Be( expected );
            }
        }

        [Fact]
        public void MinMax_ShouldThrowInvalidOperationException_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<T>();
            var action = Lambda.Of( () => sut.MinMax() );
            action.Should().ThrowExactly<InvalidOperationException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinMaxData ) )]
        public void MinMax_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expectedMin, T expectedMax)
        {
            var result = sut.MinMax();

            using ( new AssertionScope() )
            {
                result.Min.Should().Be( expectedMin );
                result.Max.Should().Be( expectedMax );
            }
        }

        [Fact]
        public void TryMinMax_ShouldReturnNull_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<T>();
            var result = sut.TryMinMax();
            result.Should().BeNull();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinMaxData ) )]
        public void TryMinMax_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> sut, T expectedMin, T expectedMax)
        {
            var result = sut.TryMinMax();

            using ( new AssertionScope() )
            {
                result.Should().NotBeNull();
                result?.Min.Should().Be( expectedMin );
                result?.Max.Should().Be( expectedMax );
            }
        }

        [Fact]
        public void ContainsDuplicates_ShouldReturnFalse_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<T>();
            var result = sut.ContainsDuplicates();
            result.Should().BeFalse();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetContainsDuplicatesData ) )]
        public void ContainsDuplicates_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> sut, bool expected)
        {
            var result = sut.ContainsDuplicates();
            result.Should().Be( expected );
        }

        [Fact]
        public void Repeat_ShouldThrowArgumentOutOfRangeException_WhenCountIsNegative()
        {
            var count = Fixture.CreateNegativeInt32();
            var sut = Fixture.CreateMany<T>();

            var action = Lambda.Of( () => sut.Repeat( count ) );

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetRepeatForZeroOrOneCountData ) )]
        public void Repeat_ShouldReturnEmpty_WhenCountIsZero(int sourceCount)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );
            var result = sut.Repeat( 0 );
            result.Should().BeEmpty();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetRepeatForZeroOrOneCountData ) )]
        public void Repeat_ShouldReturnSource_WhenCountIsOne(int sourceCount)
        {
            var sut = Fixture.CreateMany<T>( sourceCount ).ToList();
            var result = sut.Repeat( 1 );
            result.Should().BeSameAs( sut );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetRepeatForCountGreaterThanOneData ) )]
        public void Repeat_ShouldReturnCorrectResult_WhenCountIsGreaterThanOne(IEnumerable<T> sut, int count, IEnumerable<T> expected)
        {
            var result = sut.Repeat( count );
            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetRepeatForMemoizationWithCountGreaterThanOneData ) )]
        public void Repeat_ShouldNotEvaluateSource_WhenCountIsGreaterThanOne_BeforeResultIsEvaluated(int count)
        {
            var @delegate = Substitute.For<Func<int, T>>().WithAnyArgs( _ => Fixture.Create<T>() );

            var sut = Enumerable.Range( 0, 1 ).Select( @delegate );

            var _ = sut.Repeat( count );

            @delegate.Verify().CallCount.Should().Be( 0 );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetRepeatForMemoizationWithCountGreaterThanOneData ) )]
        public void Repeat_ShouldMemoizeSource_WhenCountIsGreaterThanOne(int count)
        {
            var @delegate = Substitute.For<Func<int, T>>().WithAnyArgs( _ => Fixture.Create<T>() );

            var sut = Enumerable.Range( 0, 1 ).Select( @delegate );

            var _ = sut.Repeat( count ).ToList();

            @delegate.Verify().CallCount.Should().Be( 1 );
        }

        [Fact]
        public void Materialize_ShouldReturnSource_WhenSourceImplementsReadOnlyCollectionInterface()
        {
            var sut = new TestCollection<T>();
            var result = sut.Materialize();
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Materialize_ShouldMaterializeMemoizedCollection()
        {
            var @delegate = Substitute.For<Func<int, T>>().WithAnyArgs( _ => Fixture.Create<T>() );

            var sut = Enumerable.Range( 0, 3 )
                .Select( @delegate )
                .Memoize();

            var result = sut.Materialize();

            using ( new AssertionScope() )
            {
                @delegate.Verify().CallCount.Should().Be( result.Count );
                result.Should().NotBeSameAs( sut );
                result.Should().BeSequentiallyEqualTo( sut );
            }
        }

        [Fact]
        public void Materialize_ShouldReturnCorrectResult_WhenSourceIsNotYetMaterialized()
        {
            var sut = Fixture.CreateMany<T>().Select( x => x );

            var result = sut.Materialize();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Should().BeSequentiallyEqualTo( sut );
            }
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

            @delegate.Verify().CallCount.Should().Be( iterationCount == 0 ? 0 : sourceCount );
        }

        [Fact]
        public void Memoize_ShouldReturnSource_WhenSourceIsAlreadyMemoized()
        {
            var sut = Fixture.CreateMany<T>().Memoize();
            var result = sut.Memoize();
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void IsMaterialized_ShouldReturnTrue_WhenSourceIsReadOnlyCollection()
        {
            var sut = Fixture.CreateMany<T>().ToList();
            var result = sut.IsMaterialized();
            result.Should().BeTrue();
        }

        [Fact]
        public void IsMaterialized_ShouldReturnFalse_WhenSourceIsNotReadOnlyCollection()
        {
            var sut = Fixture.CreateMany<T>();
            var result = sut.IsMaterialized();
            result.Should().BeFalse();
        }

        [Fact]
        public void IsMaterialized_ShouldReturnFalse_WhenSourceIsMemoized()
        {
            var sut = Fixture.CreateMany<T>().Memoize();
            var result = sut.IsMaterialized();
            result.Should().BeFalse();
        }

        [Fact]
        public void IsMemoized_ShouldReturnTrue_WhenSourceIsMemoized()
        {
            var sut = Fixture.CreateMany<T>().Memoize();
            var result = sut.IsMemoized();
            result.Should().BeTrue();
        }

        [Fact]
        public void IsMemoized_ShouldReturnFalse_WhenSourceIsNotMemoizedAndNotMaterialized()
        {
            var sut = Fixture.CreateMany<T>();
            var result = sut.IsMemoized();
            result.Should().BeFalse();
        }

        [Fact]
        public void IsMemoized_ShouldReturnFalse_WhenSourceIsMaterialized()
        {
            var sut = Fixture.CreateMany<T>().ToList();
            var result = sut.IsMemoized();
            result.Should().BeFalse();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetSetEqualsData ) )]
        public void SetEquals_ShouldReturnCorrectResult(IEnumerable<T> sut, IEnumerable<T> other, bool expected)
        {
            var result = sut.SetEquals( other );
            result.Should().Be( expected );
        }

        [Fact]
        public void VisitMany_ShouldReturnEmptyCollection_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<VisitManyNode<T>>();
            var result = sut.VisitMany( n => n.Children );
            result.Should().BeEmpty();
        }

        [Fact]
        public void VisitMany_ShouldReturnResultAccordingToBreadthFirstTraversal()
        {
            var expected = Fixture.CreateMany<T>( 10 ).ToList();

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

            result.Should().BeSequentiallyEqualTo( expected.Select( x => (T?)x ) );
        }

        [Fact]
        public void VisitMany_WithStopPredicate_ShouldReturnEmptyCollection_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<VisitManyNode<T>>();
            var result = sut.VisitMany( n => n.Children, n => EqualityComparer.Equals( n.Value, default ) );
            result.Should().BeEmpty();
        }

        [Fact]
        public void VisitMany_WithStopPredicate_ShouldReturnResultAccordingToBreadthFirstTraversal()
        {
            var sourceOfValues = Fixture.CreateDistinctCollection<T>( 10 ).ToList();
            var valuesToStopAt = new HashSet<T> { sourceOfValues[0], sourceOfValues[5] };
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

            result.Should().BeSequentiallyEqualTo( expected.Select( x => (T?)x ) );
        }

        [Fact]
        public void TryAggregate_ShouldReturnFalseAndDefaultResult_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<T>();

            var result = sut.TryAggregate( (_, c) => c, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default( T ) );
            }
        }

        [Fact]
        public void TryAggregate_ShouldReturnTrueAndCorrectResult_WhenSourceIsNotEmpty()
        {
            var expected = Fixture.Create<T>();
            var sut = Fixture.CreateMany<T>().Append( expected );

            var result = sut.TryAggregate( (p, c) => c, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().Be( expected );
            }
        }

        [Fact]
        public void MaxBy_ShouldThrowInvalidOperationException_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<Contained<T>>();
            var action = Lambda.Of( () => sut.MaxBy( c => c.Value ) );
            action.Should().ThrowExactly<InvalidOperationException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMaxData ) )]
        public void MaxBy_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );
            var result = sut.MaxBy( c => c.Value );
            result.Value.Should().Be( expected );
        }

        [Fact]
        public void MinBy_ShouldThrowInvalidOperationException_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<Contained<T>>();
            var action = Lambda.Of( () => sut.MinBy( c => c.Value ) );
            action.Should().ThrowExactly<InvalidOperationException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinData ) )]
        public void MinBy_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );
            var result = sut.MinBy( c => c.Value );
            result.Value.Should().Be( expected );
        }

        [Fact]
        public void MinMaxBy_ShouldThrowInvalidOperationException_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<Contained<T>>();
            var action = Lambda.Of( () => sut.MinMaxBy( c => c.Value ) );
            action.Should().ThrowExactly<InvalidOperationException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinMaxData ) )]
        public void MinMaxBy_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> values, T expectedMin, T expectedMax)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );
            var result = sut.MinMaxBy( c => c.Value );

            using ( new AssertionScope() )
            {
                result.Min.Value.Should().Be( expectedMin );
                result.Max.Value.Should().Be( expectedMax );
            }
        }

        [Fact]
        public void TryMaxBy_ShouldReturnFalseAndDefaultResult_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<Contained<T>>();

            var result = sut.TryMaxBy( c => c.Value, out var max );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                max.Should().Be( default( Contained<T> ) );
            }
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMaxData ) )]
        public void TryMaxBy_ShouldReturnTrueAndCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );

            var result = sut.TryMaxBy( c => c.Value, out var max );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                max!.Value.Should().Be( expected );
            }
        }

        [Fact]
        public void TryMinBy_ShouldReturnFalseAndDefaultResult_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<Contained<T>>();

            var result = sut.TryMinBy( c => c.Value, out var min );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                min.Should().Be( default( Contained<T> ) );
            }
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinData ) )]
        public void TryMinBy_ShouldReturnTrueAndCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );

            var result = sut.TryMinBy( c => c.Value, out var min );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                min!.Value.Should().Be( expected );
            }
        }

        [Fact]
        public void TryMinMaxBy_ShouldReturnNull_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<Contained<T>>();
            var result = sut.TryMinMaxBy( c => c.Value );
            result.Should().BeNull();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetMinMaxData ) )]
        public void TryMinMaxBy_ShouldReturnCorrectResult_WhenSourceIsNotEmpty(IEnumerable<T> values, T expectedMin, T expectedMax)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );
            var result = sut.TryMinMaxBy( c => c.Value );

            using ( new AssertionScope() )
            {
                result.Should().NotBeNull();
                result?.Min.Value.Should().Be( expectedMin );
                result?.Max.Value.Should().Be( expectedMax );
            }
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetDistinctData ) )]
        public void DistinctBy_ShouldReturnCorrectResult(IEnumerable<T> values, IEnumerable<T> expected)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );
            var result = sut.DistinctBy( c => c.Value ).Select( c => c.Value );
            result.Should().BeSequentiallyEqualTo( expected.Select( x => (T?)x ) );
        }

        [Fact]
        public void LeftJoin_ShouldReturnCorrectResult()
        {
            var values = Fixture.CreateDistinctCollection<T>( 5 );

            var sut = new[]
                {
                    values[0],
                    values[1],
                    values[2],
                    values[1],
                    values[3],
                    values[0]
                }
                .Select( v => new Contained<T> { Value = v } )
                .ToList();

            var inner = new[]
                {
                    values[1],
                    values[2],
                    values[1],
                    values[3],
                    values[4]
                }
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

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void FullJoin_ShouldReturnCorrectResult()
        {
            var values = Fixture.CreateDistinctCollection<T>( 5 );

            var sut = new[]
                {
                    values[0],
                    values[1],
                    values[2],
                    values[1],
                    values[3],
                    values[0]
                }
                .Select( v => new Contained<T> { Value = v } )
                .ToList();

            var inner = new[]
                {
                    values[1],
                    values[2],
                    values[1],
                    values[3],
                    values[4]
                }
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

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void IsOrdered_ShouldReturnTrue_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<T>();
            var result = sut.IsOrdered();
            result.Should().BeTrue();
        }

        [Fact]
        public void IsOrdered_ShouldReturnTrue_WhenSourceContainsOnlyOneElement()
        {
            var sut = Fixture.CreateMany<T>( 1 );
            var result = sut.IsOrdered();
            result.Should().BeTrue();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetIsOrderedData ) )]
        public void IsOrdered_ShouldReturnCorrectResult_WhenSourceHasMoreThanOneElement(IEnumerable<T> sut, bool expected)
        {
            var result = sut.IsOrdered();
            result.Should().Be( expected );
        }

        [Fact]
        public void Partition_ShouldReturnResultWithPassedContainingElementsThatReturnedTrueInPredicate_AndFailedContainingOtherElements()
        {
            var sut = Fixture.CreateDistinctSortedCollection<T>( count: 10 );
            var pivot = sut[6];
            var expectedPassed = sut.Take( 7 );
            var expectedFailed = sut.Skip( 7 );

            var (passed, failed) = sut.Partition( e => Comparer.Compare( e, pivot ) <= 0 );

            using ( new AssertionScope() )
            {
                passed.Should().BeSequentiallyEqualTo( expectedPassed );
                failed.Should().BeSequentiallyEqualTo( expectedFailed );
            }
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetDivideForDivisibleSourceCountData ) )]
        public void Divide_ShouldReturnCorrectResult_WhenSourceCountIsDivisibleByPartLength(int partLength)
        {
            var sut = Fixture.CreateMany<T>( 12 ).ToList();
            var expectedCount = 12 / partLength;

            var result = sut.Divide( partLength ).ToList();

            using ( new AssertionScope() )
            {
                result.Count.Should().Be( expectedCount );
                for ( var i = 0; i < expectedCount; ++i )
                    result[i].Should().BeSequentiallyEqualTo( sut.Skip( i * partLength ).Take( partLength ) );
            }
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetDivideForNonDivisibleSourceCountData ) )]
        public void Divide_ShouldReturnCorrectResult_WhenSourceCountIsNotDivisibleByPartLength(int partLength, int expectedLastPartLength)
        {
            var sut = Fixture.CreateMany<T>( 12 ).ToList();
            var expectedFullCount = 12 / partLength;

            var result = sut.Divide( partLength ).ToList();

            using ( new AssertionScope() )
            {
                result.Count.Should().Be( expectedFullCount + 1 );
                for ( var i = 0; i < expectedFullCount; ++i )
                    result[i].Should().BeSequentiallyEqualTo( sut.Skip( i * partLength ).Take( partLength ) );

                result[^1].Should().BeSequentiallyEqualTo( sut.TakeLast( expectedLastPartLength ) );
            }
        }

        [Fact]
        public void Divide_ShouldReturnEmptyResult_WhenSourceIsEmpty()
        {
            var sut = Enumerable.Empty<T>();
            var result = sut.Divide( 1 );
            result.Should().BeEmpty();
        }

        [Theory]
        [GenericMethodData( nameof( GenericEnumerableExtensionsTestsData<T>.GetDivideThrowData ) )]
        public void Divide_ShouldThrowArgumentOutOfRangeException_WhenPartLengthIsLessThanOne(int partLength)
        {
            var sut = Fixture.CreateMany<T>();
            var action = Lambda.Of( () => sut.Divide( partLength ) );
            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }
    }
}
