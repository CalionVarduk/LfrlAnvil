using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.ObjectTests
{
    public abstract class GenericObjectExtensionsTests<T> : TestsBase
        where T : notnull
    {
        [Fact]
        public void ToOne_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<T>();
            var sut = value.ToOne();
            sut.Value.Should().Be( value );
        }

        [Fact]
        public void Memoize_ShouldMaterializeSourceAfterFirstEnumeration()
        {
            var iterationCount = 5;
            var sourceCount = 3;
            var @delegate = Substitute.For<Func<int, T>>().WithAnyArgs( _ => Fixture.Create<T>() );

            var sut = new
            {
                Values = Enumerable.Range( 0, sourceCount ).Select( @delegate )
            };

            var result = sut.Memoize( o => o.Values );

            var materialized = new List<IEnumerable<T>>();
            for ( var i = 0; i < iterationCount; ++i )
                materialized.Add( result.ToList() );

            @delegate.Verify().CallCount.Should().Be( sourceCount );
        }

        [Fact]
        public void Memoize_ShouldReturnSource_WhenSourceIsAlreadyMemoized()
        {
            var sut = new
            {
                Values = Fixture.CreateMany<T>().Memoize()
            };

            var result = sut.Memoize( o => o.Values );

            result.Should().BeSameAs( sut.Values );
        }

        [Fact]
        public void Visit_ShouldReturnEmptyCollection_WhenSourceDoesntPassTheBreakPredicate()
        {
            VisitNode? sut = null;
            var result = sut.Visit( r => r.Next );
            result.Should().BeEmpty();
        }

        [Fact]
        public void Visit_ShouldReturnResultFromTopToBottom()
        {
            var values = Fixture.CreateMany<T>( 3 ).ToList();
            var expected = values.Skip( 1 );

            var sut = new VisitNode
            {
                Value = values[0],
                Next = new VisitNode
                {
                    Value = values[1],
                    Next = new VisitNode { Value = values[2] }
                }
            };

            var result = sut.Visit( r => r.Next ).Select( r => r.Value! );

            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void VisitMany_ShouldReturnResultAccordingToBreadthFirstTraversal()
        {
            var values = Fixture.CreateMany<T>( 11 ).ToList();
            var expected = values.Skip( 1 );

            var children = new[]
            {
                new VisitManyNode
                {
                    Value = values[1],
                    Children = new List<VisitManyNode>
                    {
                        new() { Value = values[4] },
                        new()
                        {
                            Value = values[5],
                            Children = new List<VisitManyNode>
                            {
                                new() { Value = values[7] },
                                new() { Value = values[8] }
                            }
                        }
                    }
                },
                new VisitManyNode { Value = values[2] },
                new VisitManyNode
                {
                    Value = values[3],
                    Children = new List<VisitManyNode>
                    {
                        new()
                        {
                            Value = values[6],
                            Children = new List<VisitManyNode>
                            {
                                new() { Value = values[9] },
                                new() { Value = values[10] }
                            }
                        }
                    }
                }
            };

            var sut = new VisitManyNode
            {
                Value = values[0],
                Children = children.ToList()
            };

            var result = sut.VisitMany( n => n.Children ).Select( n => n.Value! );

            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void VisitMany_WithStopPredicate_ShouldReturnResultAccordingToBreadthFirstTraversal()
        {
            var values = Fixture.CreateDistinctCollection<T>( 11 ).ToList();
            var valuesToStopAt = new HashSet<T> { values[1], values[6] };
            T?[] expected = { values[1], values[2], values[3], values[6] };

            var children = new[]
            {
                new VisitManyNode
                {
                    Value = values[1],
                    Children = new List<VisitManyNode>
                    {
                        new() { Value = values[4] },
                        new()
                        {
                            Value = values[5],
                            Children = new List<VisitManyNode>
                            {
                                new() { Value = values[7] },
                                new() { Value = values[8] }
                            }
                        }
                    }
                },
                new VisitManyNode { Value = values[2] },
                new VisitManyNode
                {
                    Value = values[3],
                    Children = new List<VisitManyNode>
                    {
                        new()
                        {
                            Value = values[6],
                            Children = new List<VisitManyNode>
                            {
                                new() { Value = values[9] },
                                new() { Value = values[10] }
                            }
                        }
                    }
                }
            };

            var sut = new VisitManyNode
            {
                Value = values[0],
                Children = children.ToList()
            };

            var result = sut.VisitMany( n => n.Children, n => valuesToStopAt.Contains( n.Value! ) ).Select( n => n.Value );

            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void VisitMany_WithStopPredicate_ShouldReturnEmpty_WhenPredicateStopsImmediately()
        {
            var values = Fixture.CreateDistinctCollection<T>( 4 ).ToList();
            var children = values.Skip( 1 ).Select( v => new VisitManyNode { Value = v } );

            var sut = new VisitManyNode
            {
                Value = values[0],
                Children = children.ToList()
            };

            var result = sut.VisitMany( n => n.Children, n => n.Value!.Equals( sut.Value ) ).Select( n => n.Value );

            result.Should().BeEmpty();
        }

        [Fact]
        public void VisitWithSelf_ShouldReturnEmptyCollection_WhenSourceDoesntPassTheBreakPredicate()
        {
            VisitNode? sut = null;
            var result = sut.VisitWithSelf( r => r.Next );
            result.Should().BeEmpty();
        }

        [Fact]
        public void VisitWithSelf_ShouldReturnResultFromTopToBottom_IncludingTheTargetAsRoot()
        {
            var values = Fixture.CreateMany<T>( 3 ).ToList();

            var sut = new VisitNode
            {
                Value = values[0],
                Next = new VisitNode
                {
                    Value = values[1],
                    Next = new VisitNode { Value = values[2] }
                }
            };

            var result = sut.VisitWithSelf( r => r.Next ).Select( r => r.Value! );

            result.Should().BeSequentiallyEqualTo( values );
        }

        [Fact]
        public void VisitManyWithSelf_ShouldReturnResultAccordingToBreadthFirstTraversal_IncludingTheTargetAsRoot()
        {
            var values = Fixture.CreateMany<T>( 11 ).ToList();

            var children = new[]
            {
                new VisitManyNode
                {
                    Value = values[1],
                    Children = new List<VisitManyNode>
                    {
                        new() { Value = values[4] },
                        new()
                        {
                            Value = values[5],
                            Children = new List<VisitManyNode>
                            {
                                new() { Value = values[7] },
                                new() { Value = values[8] }
                            }
                        }
                    }
                },
                new VisitManyNode { Value = values[2] },
                new VisitManyNode
                {
                    Value = values[3],
                    Children = new List<VisitManyNode>
                    {
                        new()
                        {
                            Value = values[6],
                            Children = new List<VisitManyNode>
                            {
                                new() { Value = values[9] },
                                new() { Value = values[10] }
                            }
                        }
                    }
                }
            };

            var sut = new VisitManyNode
            {
                Value = values[0],
                Children = children.ToList()
            };

            var result = sut.VisitManyWithSelf( n => n.Children ).Select( n => n.Value! );

            result.Should().BeSequentiallyEqualTo( values );
        }

        [Fact]
        public void VisitManyWithSelf_WithStopPredicate_ShouldReturnResultAccordingToBreadthFirstTraversal()
        {
            var values = Fixture.CreateDistinctCollection<T>( 11 ).ToList();
            var valuesToStopAt = new HashSet<T> { values[1], values[6] };
            T?[] expected = { values[0], values[1], values[2], values[3], values[6] };

            var children = new[]
            {
                new VisitManyNode
                {
                    Value = values[1],
                    Children = new List<VisitManyNode>
                    {
                        new() { Value = values[4] },
                        new()
                        {
                            Value = values[5],
                            Children = new List<VisitManyNode>
                            {
                                new() { Value = values[7] },
                                new() { Value = values[8] }
                            }
                        }
                    }
                },
                new VisitManyNode { Value = values[2] },
                new VisitManyNode
                {
                    Value = values[3],
                    Children = new List<VisitManyNode>
                    {
                        new()
                        {
                            Value = values[6],
                            Children = new List<VisitManyNode>
                            {
                                new() { Value = values[9] },
                                new() { Value = values[10] }
                            }
                        }
                    }
                }
            };

            var sut = new VisitManyNode
            {
                Value = values[0],
                Children = children.ToList()
            };

            var result = sut.VisitManyWithSelf( n => n.Children, n => valuesToStopAt.Contains( n.Value! ) ).Select( n => n.Value );

            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void VisitManyWithSelf_WithStopPredicate_ShouldReturnOnlySource_WhenPredicateStopsImmediately()
        {
            var values = Fixture.CreateDistinctCollection<T>( 4 ).ToList();
            var children = values.Skip( 1 ).Select( v => new VisitManyNode { Value = v } );

            var sut = new VisitManyNode
            {
                Value = values[0],
                Children = children.ToList()
            };

            var result = sut.VisitManyWithSelf( n => n.Children, n => n.Value!.Equals( sut.Value ) ).Select( n => n.Value );

            result.Should().BeSequentiallyEqualTo( sut.Value );
        }

        public sealed class VisitNode
        {
            public T? Value { get; init; }
            public VisitNode? Next { get; init; }
        }

        public sealed class VisitManyNode
        {
            public T? Value { get; init; }
            public List<VisitManyNode> Children { get; init; } = new();
        }
    }
}
