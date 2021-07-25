﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Extensions;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Extensions.Enumerable
{
    [GenericTestClass( typeof( EnumerableExtensionsTestsData<> ) )]
    public abstract class EnumerableExtensionsTests<T> : TestsBase
    {
        protected readonly IEqualityComparer<T> EqualityComparer = EqualityComparer<T>.Default;
        protected readonly IComparer<T> Comparer = Comparer<T>.Default;

        [Fact]
        public void EmptyIfNull_ShouldReturnSourceWhenNotNull()
        {
            var sut = Fixture.CreateMany<T>();

            var result = sut.EmptyIfNull();

            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void EmptyIfNull_ShouldReturnNullWhenSourceIsNull()
        {
            IEnumerable<T>? sut = null;

            var result = sut.EmptyIfNull();

            result.Should().BeEmpty();
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnTrueWhenSourceIsNull()
        {
            IEnumerable<T>? sut = null;

            var result = sut.IsNullOrEmpty();

            result.Should().BeTrue();
        }

        [Fact]
        public void IsNullOrEmpty_ShouldReturnTrueWhenSourceHasNoElements()
        {
            var sut = System.Linq.Enumerable.Empty<T>();

            var result = sut.IsNullOrEmpty();

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 3 )]
        public void IsNullOrEmpty_ShouldReturnFalseWhenSourceHasSomeElements(int count)
        {
            var sut = Fixture.CreateMany<T>( count );

            var result = sut.IsNullOrEmpty();

            result.Should().BeFalse();
        }

        [Fact]
        public void IsEmpty_ShouldReturnTrueWhenSourceHasNoElements()
        {
            var sut = System.Linq.Enumerable.Empty<T>();

            var result = sut.IsEmpty();

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 3 )]
        public void IsEmpty_ShouldReturnFalseWhenSourceHasSomeElements(int count)
        {
            var sut = Fixture.CreateMany<T>( count );

            var result = sut.IsEmpty();

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( 0, -1, true )]
        [InlineData( 0, 0, true )]
        [InlineData( 0, 1, false )]
        [InlineData( 1, -1, true )]
        [InlineData( 1, 0, true )]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, false )]
        [InlineData( 3, -1, true )]
        [InlineData( 3, 0, true )]
        [InlineData( 3, 1, true )]
        [InlineData( 3, 2, true )]
        [InlineData( 3, 3, true )]
        [InlineData( 3, 4, false )]
        public void ContainsAtLeast_ShouldReturnCorrectResult(int sourceCount, int minCount, bool expected)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );

            var result = sut.ContainsAtLeast( minCount );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0, -1, false )]
        [InlineData( 0, 0, true )]
        [InlineData( 0, 1, true )]
        [InlineData( 1, -1, false )]
        [InlineData( 1, 0, false )]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, true )]
        [InlineData( 3, -1, false )]
        [InlineData( 3, 0, false )]
        [InlineData( 3, 1, false )]
        [InlineData( 3, 2, false )]
        [InlineData( 3, 3, true )]
        [InlineData( 3, 4, true )]
        public void ContainsAtMost_ShouldReturnCorrectResult(int sourceCount, int maxCount, bool expected)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );

            var result = sut.ContainsAtMost( maxCount );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 3 )]
        public void ContainsBetween_ShouldReturnFalseWhenMaxCountIsLessThanMinCount(int count)
        {
            var (max, min) = Fixture.CreateDistinctSortedCollection<int>( 2 );
            var sut = Fixture.CreateMany<T>( count );

            var result = sut.ContainsBetween( min, max );

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( 0, -1, false )]
        [InlineData( 0, 0, true )]
        [InlineData( 0, 1, true )]
        [InlineData( 1, -1, false )]
        [InlineData( 1, 0, false )]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, true )]
        [InlineData( 3, -1, false )]
        [InlineData( 3, 0, false )]
        [InlineData( 3, 1, false )]
        [InlineData( 3, 2, false )]
        [InlineData( 3, 3, true )]
        [InlineData( 3, 4, true )]
        public void ContainsBetween_ShouldReturnCorrectResultWhenMinCountIsZero(int count, int maxCount, bool expected)
        {
            var sut = Fixture.CreateMany<T>( count );

            var result = sut.ContainsBetween( 0, maxCount );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0, -1, false )]
        [InlineData( 0, 0, true )]
        [InlineData( 0, 1, true )]
        [InlineData( 1, -1, false )]
        [InlineData( 1, 0, false )]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, true )]
        [InlineData( 3, -1, false )]
        [InlineData( 3, 0, false )]
        [InlineData( 3, 1, false )]
        [InlineData( 3, 2, false )]
        [InlineData( 3, 3, true )]
        [InlineData( 3, 4, true )]
        public void ContainsBetween_ShouldReturnCorrectResultWhenMinCountIsNegative(int count, int maxCount, bool expected)
        {
            var minCount = Fixture.CreateNegativeInt32();
            var sut = Fixture.CreateMany<T>( count );

            var result = sut.ContainsBetween( minCount, maxCount );

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( 0, 1 )]
        [InlineData( 0, 2 )]
        [InlineData( 1, 2 )]
        [InlineData( 1, 3 )]
        [InlineData( 3, 4 )]
        [InlineData( 3, 5 )]
        public void ContainsBetween_ShouldReturnFalseWhenSourceCountIsLessThanMinCount(int count, int minCount)
        {
            var sut = Fixture.CreateMany<T>( count );

            var result = sut.ContainsBetween( minCount, minCount + 1 );

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( 3, 2 )]
        [InlineData( 4, 3 )]
        [InlineData( 4, 2 )]
        [InlineData( 5, 4 )]
        [InlineData( 5, 3 )]
        public void ContainsBetween_ShouldReturnFalseWhenSourceCountIsGreaterThanMaxCount(int count, int maxCount)
        {
            var sut = Fixture.CreateMany<T>( count );

            var result = sut.ContainsBetween( maxCount - 1, maxCount );

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( 1, 1, 1 )]
        [InlineData( 1, 1, 2 )]
        [InlineData( 1, 1, 3 )]
        [InlineData( 3, 1, 3 )]
        [InlineData( 3, 1, 4 )]
        [InlineData( 3, 1, 5 )]
        [InlineData( 3, 2, 3 )]
        [InlineData( 3, 2, 4 )]
        [InlineData( 3, 2, 5 )]
        [InlineData( 3, 3, 3 )]
        [InlineData( 3, 3, 4 )]
        [InlineData( 3, 3, 5 )]
        public void ContainsBetween_ShouldReturnTrueWhenSourceCountIsBetweenMinAndMaxCount(int sourceCount, int minCount, int maxCount)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );

            var result = sut.ContainsBetween( minCount, maxCount );

            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 3 )]
        public void ContainsExactly_ShouldReturnFalseWhenCountIsNegative(int sourceCount)
        {
            var count = Fixture.CreateNegativeInt32();
            var sut = Fixture.CreateMany<T>( sourceCount );

            var result = sut.ContainsExactly( count );

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( 0, 0, true )]
        [InlineData( 0, 1, false )]
        [InlineData( 1, 0, false )]
        [InlineData( 1, 1, true )]
        [InlineData( 1, 2, false )]
        [InlineData( 3, 2, false )]
        [InlineData( 3, 3, true )]
        [InlineData( 3, 4, false )]
        public void ContainsExactly_ShouldReturnCorrectResultWhenCountIsNotNegative(int sourceCount, int count, bool expected)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );

            var result = sut.ContainsExactly( count );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( EnumerableExtensionsTestsData<T>.CreateFlattenTestData ) )]
        public void Flatten_ShouldReturnCorrectResult(IEnumerable<Pair<T, IEnumerable<T>>> data, IEnumerable<Pair<T, T>> expected)
        {
            var sut = data.Select( d => d.First );

            var result = sut.Flatten( x => data.First( y => x!.Equals( y.First ) ).Second );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void TryMin_ShouldReturnFalseAndDefaultResultWhenSourceIsEmpty()
        {
            var sut = System.Linq.Enumerable.Empty<T>();

            var result = sut.TryMin( out var min );

            result.Should().BeFalse();
            min.Should().Be( default( T ) );
        }

        [Theory]
        [GenericMethodData( nameof( EnumerableExtensionsTestsData<T>.CreateMinTestData ) )]
        public void TryMin_ShouldReturnTrueAndCorrectResultWhenSourceIsNotEmpty(IEnumerable<T> sut, T expected)
        {
            var result = sut.TryMin( out var min );

            result.Should().BeTrue();
            min.Should().Be( expected );
        }

        [Fact]
        public void TryMax_ShouldReturnFalseAndDefaultResultWhenSourceIsEmpty()
        {
            var sut = System.Linq.Enumerable.Empty<T>();

            var result = sut.TryMax( out var max );

            result.Should().BeFalse();
            max.Should().Be( default( T ) );
        }

        [Theory]
        [GenericMethodData( nameof( EnumerableExtensionsTestsData<T>.CreateMaxTestData ) )]
        public void TryMax_ShouldReturnTrueAndCorrectResultWhenSourceIsNotEmpty(IEnumerable<T> sut, T expected)
        {
            var result = sut.TryMax( out var max );

            result.Should().BeTrue();
            max.Should().Be( expected );
        }

        [Fact]
        public void ContainsDuplicates_ShouldReturnFalseWhenSourceIsEmpty()
        {
            var sut = System.Linq.Enumerable.Empty<T>();

            var result = sut.ContainsDuplicates();

            result.Should().BeFalse();
        }

        [Theory]
        [GenericMethodData( nameof( EnumerableExtensionsTestsData<T>.CreateContainsDuplicatesTestData ) )]
        public void ContainsDuplicates_ShouldReturnCorrectResultWhenSourceIsNotEmpty(IEnumerable<T> sut, bool expected)
        {
            var result = sut.ContainsDuplicates();

            result.Should().Be( expected );
        }

        [Fact]
        public void Repeat_ShouldThrowWhenCountIsNegative()
        {
            var count = Fixture.CreateNegativeInt32();
            var sut = Fixture.CreateMany<T>();

            Action action = () => sut.Repeat( count );

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 3 )]
        public void Repeat_ShouldReturnEmptyWhenCountIsZero(int sourceCount)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );

            var result = sut.Repeat( 0 );

            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData( 0 )]
        [InlineData( 1 )]
        [InlineData( 3 )]
        public void Repeat_ShouldReturnSourceWhenCountIsOne(int sourceCount)
        {
            var sut = Fixture.CreateMany<T>( sourceCount );

            var result = sut.Repeat( 1 );

            result.Should().BeSameAs( sut );
        }

        [Theory]
        [GenericMethodData( nameof( EnumerableExtensionsTestsData<T>.CreateRepeatTestData ) )]
        public void Repeat_ShouldReturnCorrectResultWhenCountIsGreaterThanOne(IEnumerable<T> sut, int count, IEnumerable<T> expected)
        {
            var result = sut.Repeat( count );

            result.Should().BeEquivalentTo( expected );
        }

        [Theory]
        [InlineData( 2 )]
        [InlineData( 3 )]
        [InlineData( 5 )]
        public void Repeat_ShouldNotEvaluateSourceWhenCountIsGreaterThanOne_BeforeResultIsEvaluated(int count)
        {
            var callCount = 0;

            var sut = System.Linq.Enumerable.Range( 0, 1 )
                .Select(
                    _ =>
                    {
                        ++callCount;
                        return Fixture.Create<T>();
                    } );

            var result = sut.Repeat( count );

            callCount.Should().Be( 0 );
        }

        [Theory]
        [InlineData( 2 )]
        [InlineData( 3 )]
        [InlineData( 5 )]
        public void Repeat_ShouldMemoizeSourceWhenCountIsGreaterThanOne(int count)
        {
            var callCount = 0;

            var sut = System.Linq.Enumerable.Range( 0, 1 )
                .Select(
                    _ =>
                    {
                        ++callCount;
                        return Fixture.Create<T>();
                    } );

            var result = sut.Repeat( count ).ToList();

            callCount.Should().Be( 1 );
        }

        [Fact]
        public void Materialize_ShouldReturnSourceWhenSourceIsArray()
        {
            var sut = Array.Empty<T>();

            var result = sut.Materialize();

            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Materialize_ShouldReturnSourceWhenSourceIsList()
        {
            var sut = new List<T>();

            var result = sut.Materialize();

            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void Materialize_ShouldReturnCorrectResultWhenSourceIsNotList()
        {
            var sut = Fixture.CreateMany<T>().Select( x => x );

            var result = sut.Materialize();

            result.Should().BeEquivalentTo( sut );
        }

        [Fact]
        public void Materialize_ShouldReturnListWhenSourceIsNotList()
        {
            var sut = Fixture.CreateMany<T>().Select( x => x );

            var result = sut.Materialize();

            result.Should().BeAssignableTo<IReadOnlyList<T>>();
        }

        [Theory]
        [InlineData( 0, 0 )]
        [InlineData( 0, 1 )]
        [InlineData( 0, 3 )]
        [InlineData( 0, 5 )]
        [InlineData( 1, 0 )]
        [InlineData( 1, 1 )]
        [InlineData( 1, 3 )]
        [InlineData( 1, 5 )]
        [InlineData( 3, 0 )]
        [InlineData( 3, 1 )]
        [InlineData( 3, 3 )]
        [InlineData( 3, 5 )]
        public void Memoize_ShouldMaterializeSourceAfterFirstEnumeration(int sourceCount, int iterationCount)
        {
            var callCount = 0;

            var sut = System.Linq.Enumerable.Range( 0, sourceCount )
                .Select(
                    _ =>
                    {
                        ++callCount;
                        return Fixture.Create<T>();
                    } )
                .Memoize();

            var materialized = new List<IEnumerable<T>>();
            for ( var i = 0; i < iterationCount; ++i )
                materialized.Add( sut.ToList() );

            callCount.Should().Be( iterationCount == 0 ? 0 : sourceCount );
        }

        [Theory]
        [GenericMethodData( nameof( EnumerableExtensionsTestsData<T>.CreateSetEqualsTestData ) )]
        public void SetEquals_ShouldReturnCorrectResult(IEnumerable<T> sut, IEnumerable<T> other, bool expected)
        {
            var result = sut.SetEquals( other );

            result.Should().Be( expected );
        }

        [Fact]
        public void VisitMany_ShouldReturnCorrectResultWhenSourceIsEmpty()
        {
            var sut = System.Linq.Enumerable.Empty<VisitManyNode<T>>();

            var result = sut.VisitMany( n => n.Children );

            result.Should().BeEmpty();
        }

        [Fact]
        public void VisitMany_ShouldReturnCorrectResult()
        {
            var expected = Fixture.CreateMany<T>( 10 ).ToList();

            var sut = new[]
            {
                new VisitManyNode<T>
                {
                    Value = expected[0],
                    Children = new List<VisitManyNode<T>>
                    {
                        new VisitManyNode<T> { Value = expected[3] },
                        new VisitManyNode<T>
                        {
                            Value = expected[4],
                            Children = new List<VisitManyNode<T>>
                            {
                                new VisitManyNode<T> { Value = expected[6] },
                                new VisitManyNode<T> { Value = expected[7] }
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
                        new VisitManyNode<T>
                        {
                            Value = expected[5],
                            Children = new List<VisitManyNode<T>>
                            {
                                new VisitManyNode<T> { Value = expected[8] },
                                new VisitManyNode<T> { Value = expected[9] }
                            }
                        }
                    }
                }
            };

            var result = sut.VisitMany( n => n.Children ).Select( n => n.Value );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void TryAggregate_ShouldReturnFalseAndDefaultResultWhenSourceIsEmpty()
        {
            var sut = System.Linq.Enumerable.Empty<T>();

            var result = sut.TryAggregate( (p, c) => c, out var outResult );

            result.Should().BeFalse();
            outResult.Should().Be( default( T ) );
        }

        [Fact]
        public void TryAggregate_ShouldReturnTrueAndCorrectResultWhenSourceIsNotEmpty()
        {
            var expected = Fixture.Create<T>();
            var sut = Fixture.CreateMany<T>().Append( expected );

            var result = sut.TryAggregate( (p, c) => c, out var outResult );

            result.Should().BeTrue();
            outResult.Should().Be( expected );
        }

        [Fact]
        public void MaxBy_ShouldThrowWhenSourceIsEmpty()
        {
            var sut = System.Linq.Enumerable.Empty<Contained<T>>();

            Action action = () => sut.MaxBy( c => c.Value );

            action.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [GenericMethodData( nameof( EnumerableExtensionsTestsData<T>.CreateMaxTestData ) )]
        public void MaxBy_ShouldReturnCorrectResultWhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );

            var result = sut.MaxBy( c => c.Value );

            result.Value.Should().Be( expected );
        }

        [Fact]
        public void MinBy_ShouldThrowWhenSourceIsEmpty()
        {
            var sut = System.Linq.Enumerable.Empty<Contained<T>>();

            Action action = () => sut.MinBy( c => c.Value );

            action.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [GenericMethodData( nameof( EnumerableExtensionsTestsData<T>.CreateMinTestData ) )]
        public void MinBy_ShouldReturnCorrectResultWhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );

            var result = sut.MinBy( c => c.Value );

            result.Value.Should().Be( expected );
        }

        [Fact]
        public void TryMaxBy_ShouldReturnFalseAndDefaultResultWhenSourceIsEmpty()
        {
            var sut = System.Linq.Enumerable.Empty<Contained<T>>();

            var result = sut.TryMaxBy( c => c.Value, out var max );

            result.Should().BeFalse();
            max.Should().Be( default( Contained<T> ) );
        }

        [Theory]
        [GenericMethodData( nameof( EnumerableExtensionsTestsData<T>.CreateMaxTestData ) )]
        public void TryMaxBy_ShouldReturnTrueAndCorrectResultWhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );

            var result = sut.TryMaxBy( c => c.Value, out var max );

            result.Should().BeTrue();
            max!.Value.Should().Be( expected );
        }

        [Fact]
        public void TryMinBy_ShouldReturnFalseAndDefaultResultWhenSourceIsEmpty()
        {
            var sut = System.Linq.Enumerable.Empty<Contained<T>>();

            var result = sut.TryMinBy( c => c.Value, out var min );

            result.Should().BeFalse();
            min.Should().Be( default( Contained<T> ) );
        }

        [Theory]
        [GenericMethodData( nameof( EnumerableExtensionsTestsData<T>.CreateMinTestData ) )]
        public void TryMinBy_ShouldReturnTrueAndCorrectResultWhenSourceIsNotEmpty(IEnumerable<T> values, T expected)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );

            var result = sut.TryMinBy( c => c.Value, out var min );

            result.Should().BeTrue();
            min!.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( (nameof( EnumerableExtensionsTestsData<T>.CreateDistinctTestData )) )]
        public void DistinctBy_ShouldReturnCorrectResult(IEnumerable<T> values, IEnumerable<T> expected)
        {
            var sut = values.Select( v => new Contained<T> { Value = v } );

            var result = sut.DistinctBy( c => c.Value ).Select( c => c.Value );

            result.Should().BeEquivalentTo( expected );
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
    }
}
