using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Collections;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Collections.TwoWayDictionary
{
    public abstract class TwoWayDictionaryTests<T1, T2> : TestsBase
        where T1 : notnull
        where T2 : notnull
    {
        [Fact]
        public void Ctor_ShouldCreateEmptyTwoWayDictionary()
        {
            var sut = new TwoWayDictionary<T1, T2>();

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 0 );
                sut.ForwardComparer.Should().Be( EqualityComparer<T1>.Default );
                sut.ReverseComparer.Should().Be( EqualityComparer<T2>.Default );
            }
        }

        [Fact]
        public void Ctor_ShouldCreateWithExplicitComparers()
        {
            var forwardComparer = EqualityComparerFactory<T1>.Create( (a, b) => a!.Equals( b ) );
            var reverseComparer = EqualityComparerFactory<T2>.Create( (a, b) => a!.Equals( b ) );

            var sut = new TwoWayDictionary<T1, T2>( forwardComparer, reverseComparer );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 0 );
                sut.ForwardComparer.Should().Be( forwardComparer );
                sut.ReverseComparer.Should().Be( reverseComparer );
            }
        }

        [Fact]
        public void TryAdd_ShouldReturnFalse_WhenFirstAlreadyExists()
        {
            var first = Fixture.Create<T1>();
            var (oldSecond, newSecond) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { first, oldSecond } };

            var result = sut.TryAdd( first, newSecond );

            result.Should().BeFalse();
        }

        [Fact]
        public void TryAdd_ShouldReturnFalse_WhenSecondAlreadyExists()
        {
            var (oldFirst, newFirst) = Fixture.CreateDistinctCollection<T1>( 2 );
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { oldFirst, second } };

            var result = sut.TryAdd( newFirst, second );

            result.Should().BeFalse();
        }

        [Fact]
        public void TryAdd_ShouldReturnTrueAndAddValues_WhenFirstAndSecondDontExist()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            var result = sut.TryAdd( first, second );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.Forward[first].Should().Be( second );
                sut.Reverse[second].Should().Be( first );
            }
        }

        [Fact]
        public void Add_ShouldThrow_WhenFirstAlreadyExists()
        {
            var first = Fixture.Create<T1>();
            var (oldSecond, newSecond) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { first, oldSecond } };

            Action action = () => sut.Add( first, newSecond );

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Add_ShouldThrow_WhenSecondAlreadyExists()
        {
            var (oldFirst, newFirst) = Fixture.CreateDistinctCollection<T1>( 2 );
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { oldFirst, second } };

            Action action = () => sut.Add( newFirst, second );

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Add_ShouldAddValues_WhenFirstAndSecondDontExist()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            sut.Add( first, second );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 1 );
                sut.Forward[first].Should().Be( second );
                sut.Reverse[second].Should().Be( first );
            }
        }

        [Fact]
        public void TryUpdateForward_ShouldReturnFalse_WhenSecondAlreadyExists()
        {
            var (first1, first2) = Fixture.CreateDistinctCollection<T1>( 2 );
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

            var result = sut.TryUpdateForward( first2, second );

            result.Should().BeFalse();
        }

        [Fact]
        public void TryUpdateForward_ShouldReturnFalse_WhenFirstDoesntExist()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            var result = sut.TryUpdateForward( first, second );

            result.Should().BeFalse();
        }

        [Fact]
        public void TryUpdateForward_ShouldReturnTrueAndUpdate_WhenFirstExistsAndSecondDoesntExist()
        {
            var first = Fixture.Create<T1>();
            var (second1, second2) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

            var result = sut.TryUpdateForward( first, second2 );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.Forward[first].Should().Be( second2 );
                sut.Reverse.ContainsKey( second1 ).Should().BeFalse();
                sut.Reverse[second2].Should().Be( first );
            }
        }

        [Fact]
        public void UpdateForward_ShouldThrow_WhenSecondAlreadyExists()
        {
            var (first1, first2) = Fixture.CreateDistinctCollection<T1>( 2 );
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

            Action action = () => sut.UpdateForward( first2, second );

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UpdateForward_ShouldThrow_WhenFirstDoesntExist()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            Action action = () => sut.UpdateForward( first, second );

            action.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void UpdateForward_ShouldUpdate_WhenFirstExistsAndSecondDoesntExist()
        {
            var first = Fixture.Create<T1>();
            var (second1, second2) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

            sut.UpdateForward( first, second2 );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 1 );
                sut.Forward[first].Should().Be( second2 );
                sut.Reverse.ContainsKey( second1 ).Should().BeFalse();
                sut.Reverse[second2].Should().Be( first );
            }
        }

        [Fact]
        public void TryUpdateReverse_ShouldReturnFalse_WhenFirstAlreadyExists()
        {
            var first = Fixture.Create<T1>();
            var (second1, second2) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

            var result = sut.TryUpdateReverse( second2, first );

            result.Should().BeFalse();
        }

        [Fact]
        public void TryUpdateReverse_ShouldReturnFalse_WhenSecondDoesntExist()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            var result = sut.TryUpdateReverse( second, first );

            result.Should().BeFalse();
        }

        [Fact]
        public void TryUpdateReverse_ShouldReturnTrueAndUpdate_WhenSecondExistsAndFirstDoesntExist()
        {
            var (first1, first2) = Fixture.CreateDistinctCollection<T1>( 2 );
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

            var result = sut.TryUpdateReverse( second, first2 );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 1 );
                sut.Reverse[second].Should().Be( first2 );
                sut.Forward.ContainsKey( first1 ).Should().BeFalse();
                sut.Forward[first2].Should().Be( second );
            }
        }

        [Fact]
        public void UpdateReverse_ShouldThrow_WhenFirstAlreadyExists()
        {
            var first = Fixture.Create<T1>();
            var (second1, second2) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

            Action action = () => sut.UpdateReverse( second2, first );

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UpdateReverse_ShouldThrow_WhenSecondDoesntExist()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            Action action = () => sut.UpdateReverse( second, first );

            action.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void UpdateReverse_ShouldUpdate_WhenSecondExistsAndFirstDoesntExist()
        {
            var (first1, first2) = Fixture.CreateDistinctCollection<T1>( 2 );
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

            sut.UpdateReverse( second, first2 );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 1 );
                sut.Reverse[second].Should().Be( first2 );
                sut.Forward.ContainsKey( first1 ).Should().BeFalse();
                sut.Forward[first2].Should().Be( second );
            }
        }

        [Fact]
        public void RemoveForward_ShouldReturnFalse_WhenValueDoesntExist()
        {
            var first = Fixture.Create<T1>();

            var sut = new TwoWayDictionary<T1, T2>();

            var result = sut.RemoveForward( first );

            result.Should().BeFalse();
        }

        [Fact]
        public void RemoveForward_ShouldReturnTrueAndRemove_WhenValueExists()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { first, second } };

            var result = sut.RemoveForward( first );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
            }
        }

        [Fact]
        public void RemoveReverse_ShouldReturnFalse_WhenValueDoesntExist()
        {
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            var result = sut.RemoveReverse( second );

            result.Should().BeFalse();
        }

        [Fact]
        public void RemoveReverse_ShouldReturnTrueAndRemove_WhenValueExists()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { first, second } };

            var result = sut.RemoveReverse( second );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
            }
        }

        [Fact]
        public void RemoveForward_ShouldReturnFalse_WhenValueDoesntExist_WithOutParam()
        {
            var first = Fixture.Create<T1>();

            var sut = new TwoWayDictionary<T1, T2>();

            var result = sut.RemoveForward( first, out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                removed.Should().Be( default( T2 ) );
            }
        }

        [Fact]
        public void RemoveForward_ShouldReturnTrueAndRemove_WhenValueExists_WithOutParam()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { first, second } };

            var result = sut.RemoveForward( first, out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                removed.Should().Be( second );
            }
        }

        [Fact]
        public void RemoveReverse_ShouldReturnFalse_WhenValueDoesntExist_WithOutParam()
        {
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            var result = sut.RemoveReverse( second, out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                removed.Should().Be( default( T1 ) );
            }
        }

        [Fact]
        public void RemoveReverse_ShouldReturnTrueAndRemove_WhenValueExists_WithOutParam()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { first, second } };

            var result = sut.RemoveReverse( second, out var removed );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 0 );
                removed.Should().Be( first );
            }
        }

        [Fact]
        public void Clear_ShouldRemoveAll()
        {
            var (first1, first2, first3) = Fixture.CreateDistinctCollection<T1>( 3 );
            var (second1, second2, second3) = Fixture.CreateDistinctCollection<T2>( 3 );

            var sut = new TwoWayDictionary<T1, T2>
            {
                { first1, second1 },
                { first2, second2 },
                { first3, second3 }
            };

            sut.Clear();

            sut.Count.Should().Be( 0 );
        }

        [Fact]
        public void GetEnumerator_ShouldReturnCorrectResult()
        {
            var (first1, first2, first3) = Fixture.CreateDistinctCollection<T1>( 3 );
            var (second1, second2, second3) = Fixture.CreateDistinctCollection<T2>( 3 );

            var expected = new[]
            {
                Core.Pair.Create( first1, second1 ),
                Core.Pair.Create( first2, second2 ),
                Core.Pair.Create( first3, second3 )
            }.AsEnumerable();

            var sut = new TwoWayDictionary<T1, T2>
            {
                { first1, second1 },
                { first2, second2 },
                { first3, second3 }
            };

            sut.Should().BeEquivalentTo( expected );
        }
    }
}
