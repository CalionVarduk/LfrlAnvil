using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Collections.Tests.TwoWayDictionaryTests
{
    public abstract class GenericTwoWayDictionaryTests<T1, T2> : TestsBase
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
        public void TryAdd_ShouldReturnFalseAndDoNothing_WhenFirstAlreadyExists()
        {
            var first = Fixture.Create<T1>();
            var (oldSecond, newSecond) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { first, oldSecond } };

            var result = sut.TryAdd( first, newSecond );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
                sut.Forward[first].Should().Be( oldSecond );
                sut.Reverse[oldSecond].Should().Be( first );
            }
        }

        [Fact]
        public void TryAdd_ShouldReturnFalseAndDoNothing_WhenSecondAlreadyExists()
        {
            var (oldFirst, newFirst) = Fixture.CreateDistinctCollection<T1>( 2 );
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { oldFirst, second } };

            var result = sut.TryAdd( newFirst, second );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
                sut.Forward[oldFirst].Should().Be( second );
                sut.Reverse[second].Should().Be( oldFirst );
            }
        }

        [Fact]
        public void TryAdd_ShouldReturnTrueAndAddNewValues_WhenDictionaryIsEmpty()
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
        public void TryAdd_ShouldReturnTrueAndAddNewValues_WhenFirstAndSecondDontExist()
        {
            var (first, otherFirst) = Fixture.CreateDistinctCollection<T1>( 2 );
            var (second, otherSecond) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { otherFirst, otherSecond } };

            var result = sut.TryAdd( first, second );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                sut.Count.Should().Be( 2 );
                sut.Forward[first].Should().Be( second );
                sut.Reverse[second].Should().Be( first );
                sut.Forward[otherFirst].Should().Be( otherSecond );
                sut.Reverse[otherSecond].Should().Be( otherFirst );
            }
        }

        [Fact]
        public void Add_ShouldThrowArgumentException_WhenFirstAlreadyExists()
        {
            var first = Fixture.Create<T1>();
            var (oldSecond, newSecond) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { first, oldSecond } };

            var action = Lambda.Of( () => sut.Add( first, newSecond ) );

            using ( new AssertionScope() )
            {
                action.Should().ThrowExactly<ArgumentException>();
                sut.Count.Should().Be( 1 );
                sut.Forward[first].Should().Be( oldSecond );
                sut.Reverse[oldSecond].Should().Be( first );
            }
        }

        [Fact]
        public void Add_ShouldThrowArgumentException_WhenSecondAlreadyExists()
        {
            var (oldFirst, newFirst) = Fixture.CreateDistinctCollection<T1>( 2 );
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { oldFirst, second } };

            var action = Lambda.Of( () => sut.Add( newFirst, second ) );

            using ( new AssertionScope() )
            {
                action.Should().ThrowExactly<ArgumentException>();
                sut.Count.Should().Be( 1 );
                sut.Forward[oldFirst].Should().Be( second );
                sut.Reverse[second].Should().Be( oldFirst );
            }
        }

        [Fact]
        public void Add_ShouldAddNewValues_WhenDictionaryIsEmpty()
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
        public void Add_ShouldAddNewValues_WhenFirstAndSecondDontExist()
        {
            var (first, otherFirst) = Fixture.CreateDistinctCollection<T1>( 2 );
            var (second, otherSecond) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { otherFirst, otherSecond } };

            sut.Add( first, second );

            using ( new AssertionScope() )
            {
                sut.Count.Should().Be( 2 );
                sut.Forward[first].Should().Be( second );
                sut.Reverse[second].Should().Be( first );
                sut.Forward[otherFirst].Should().Be( otherSecond );
                sut.Reverse[otherSecond].Should().Be( otherFirst );
            }
        }

        [Fact]
        public void TryUpdateForward_ShouldReturnFalseAndDoNothing_WhenSecondAlreadyExists()
        {
            var (first1, first2) = Fixture.CreateDistinctCollection<T1>( 2 );
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

            var result = sut.TryUpdateForward( first2, second );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
                sut.Forward[first1].Should().Be( second );
                sut.Reverse[second].Should().Be( first1 );
            }
        }

        [Fact]
        public void TryUpdateForward_ShouldReturnFalseAndDoNothing_WhenFirstDoesntExist()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            var result = sut.TryUpdateForward( first, second );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 0 );
            }
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
                sut.Reverse[second2].Should().Be( first );
                sut.Reverse.ContainsKey( second1 ).Should().BeFalse();
            }
        }

        [Fact]
        public void UpdateForward_ShouldThrowArgumentException_WhenSecondAlreadyExists()
        {
            var (first1, first2) = Fixture.CreateDistinctCollection<T1>( 2 );
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2> { { first1, second } };

            var action = Lambda.Of( () => sut.UpdateForward( first2, second ) );

            using ( new AssertionScope() )
            {
                action.Should().ThrowExactly<ArgumentException>();
                sut.Count.Should().Be( 1 );
                sut.Forward[first1].Should().Be( second );
                sut.Reverse[second].Should().Be( first1 );
            }
        }

        [Fact]
        public void UpdateForward_ShouldThrowKeyNotFoundException_WhenFirstDoesntExist()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            var action = Lambda.Of( () => sut.UpdateForward( first, second ) );

            using ( new AssertionScope() )
            {
                action.Should().ThrowExactly<KeyNotFoundException>();
                sut.Count.Should().Be( 0 );
            }
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
                sut.Reverse[second2].Should().Be( first );
                sut.Reverse.ContainsKey( second1 ).Should().BeFalse();
            }
        }

        [Fact]
        public void TryUpdateReverse_ShouldReturnFalseAndDoNothing_WhenFirstAlreadyExists()
        {
            var first = Fixture.Create<T1>();
            var (second1, second2) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

            var result = sut.TryUpdateReverse( second2, first );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 1 );
                sut.Forward[first].Should().Be( second1 );
                sut.Reverse[second1].Should().Be( first );
            }
        }

        [Fact]
        public void TryUpdateReverse_ShouldReturnFalseAndDoNothing_WhenSecondDoesntExist()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            var result = sut.TryUpdateReverse( second, first );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                sut.Count.Should().Be( 0 );
            }
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
                sut.Forward[first2].Should().Be( second );
                sut.Forward.ContainsKey( first1 ).Should().BeFalse();
            }
        }

        [Fact]
        public void UpdateReverse_ShouldThrowArgumentException_WhenFirstAlreadyExists()
        {
            var first = Fixture.Create<T1>();
            var (second1, second2) = Fixture.CreateDistinctCollection<T2>( 2 );

            var sut = new TwoWayDictionary<T1, T2> { { first, second1 } };

            var action = Lambda.Of( () => sut.UpdateReverse( second2, first ) );

            using ( new AssertionScope() )
            {
                action.Should().ThrowExactly<ArgumentException>();
                sut.Count.Should().Be( 1 );
                sut.Forward[first].Should().Be( second1 );
                sut.Reverse[second1].Should().Be( first );
            }
        }

        [Fact]
        public void UpdateReverse_ShouldThrowKeyNotFoundException_WhenSecondDoesntExist()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new TwoWayDictionary<T1, T2>();

            var action = Lambda.Of( () => sut.UpdateReverse( second, first ) );

            using ( new AssertionScope() )
            {
                action.Should().ThrowExactly<KeyNotFoundException>();
                sut.Count.Should().Be( 0 );
            }
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
                sut.Forward[first2].Should().Be( second );
                sut.Forward.ContainsKey( first1 ).Should().BeFalse();
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
                Pair.Create( first1, second1 ),
                Pair.Create( first2, second2 ),
                Pair.Create( first3, second3 )
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
