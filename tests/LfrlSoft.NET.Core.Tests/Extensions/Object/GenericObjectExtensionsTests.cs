using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.FluentAssertions;
using LfrlSoft.NET.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Object
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
        public void ToMaybe_ShouldReturnCorrectResult_WhenNotNull()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = value.ToMaybe();

            using ( new AssertionScope() )
            {
                sut.HasValue.Should().BeTrue();
                sut.Value.Should().Be( value );
            }
        }

        [Fact]
        public void ToEither_ShouldReturnCorrectResult()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = value.ToEither();

            sut.Value.Should().Be( value );
        }

        [Fact]
        public void ToUnsafe_WithValue_ShouldReturnCorrectResult()
        {
            var value = Fixture.CreateNotDefault<T>();

            var sut = value.ToUnsafe();

            using ( new AssertionScope() )
            {
                sut.IsOk.Should().BeTrue();
                sut.Value.Should().Be( value );
            }
        }

        [Fact]
        public void ToUnsafe_WithException_ShouldReturnCorrectResult()
        {
            var error = new Exception();

            var sut = error.ToUnsafe();

            using ( new AssertionScope() )
            {
                sut.HasError.Should().BeTrue();
                sut.Error.Should().Be( error );
            }
        }

        [Fact]
        public void Memoize_ShouldMaterializeSourceAfterFirstEnumeration()
        {
            var iterationCount = 5;
            var sourceCount = 3;
            var @delegate = Substitute.For<Func<int, T>>().WithAnyArgs( _ => Fixture.Create<T>() );

            var sut = new
            {
                Values = System.Linq.Enumerable.Range( 0, sourceCount ).Select( @delegate )
            };

            var result = sut.Memoize( o => o.Values );

            var materialized = new List<IEnumerable<T>>();
            for ( var i = 0; i < iterationCount; ++i )
                materialized.Add( result.ToList() );

            @delegate.Verify().CallCount.Should().Be( sourceCount );
        }

        [Fact]
        public void Visit_ShouldReturnEmptyCollectionWhenSourceDoesntPassTheBreakPredicate()
        {
            VisitNode? sut = null;

            var result = sut.Visit( r => r.Next );

            result.Should().BeEmpty();
        }

        [Fact]
        public void Visit_ShouldReturnCorrectResult()
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

            var result = sut.Visit( r => r.Next ).Select( r => r.Value );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void VisitMany_ShouldReturnCorrectResult()
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
                        new VisitManyNode { Value = values[4] },
                        new VisitManyNode
                        {
                            Value = values[5],
                            Children = new List<VisitManyNode>
                            {
                                new VisitManyNode { Value = values[7] },
                                new VisitManyNode { Value = values[8] }
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
                        new VisitManyNode
                        {
                            Value = values[6],
                            Children = new List<VisitManyNode>
                            {
                                new VisitManyNode { Value = values[9] },
                                new VisitManyNode { Value = values[10] }
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

            var result = sut.VisitMany( n => n.Children ).Select( n => n.Value );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void VisitWithSelf_ShouldReturnEmptyCollectionWhenSourceDoesntPassTheBreakPredicate()
        {
            VisitNode? sut = null;

            var result = sut.VisitWithSelf( r => r.Next );

            result.Should().BeEmpty();
        }

        [Fact]
        public void VisitWithSelf_ShouldReturnCorrectResult()
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

            var result = sut.VisitWithSelf( r => r.Next ).Select( r => r.Value );

            result.Should().BeEquivalentTo( values );
        }

        [Fact]
        public void VisitManyWithSelf_ShouldReturnCorrectResult()
        {
            var values = Fixture.CreateMany<T>( 11 ).ToList();

            var children = new[]
            {
                new VisitManyNode
                {
                    Value = values[1],
                    Children = new List<VisitManyNode>
                    {
                        new VisitManyNode { Value = values[4] },
                        new VisitManyNode
                        {
                            Value = values[5],
                            Children = new List<VisitManyNode>
                            {
                                new VisitManyNode { Value = values[7] },
                                new VisitManyNode { Value = values[8] }
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
                        new VisitManyNode
                        {
                            Value = values[6],
                            Children = new List<VisitManyNode>
                            {
                                new VisitManyNode { Value = values[9] },
                                new VisitManyNode { Value = values[10] }
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

            var result = sut.VisitManyWithSelf( n => n.Children ).Select( n => n.Value );

            result.Should().BeEquivalentTo( values );
        }

        public sealed class VisitNode
        {
            public T? Value { get; init; }
            public VisitNode? Next { get; init; }
        }

        public sealed class VisitManyNode
        {
            public T? Value { get; init; }
            public List<VisitManyNode> Children { get; init; } = new List<VisitManyNode>();
        }
    }
}
