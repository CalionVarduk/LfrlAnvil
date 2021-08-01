using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Equality
{
    [GenericTestClass( typeof( GenericEqualityTestsData<> ) )]
    public abstract class GenericEqualityTests<T> : TestsBase
    {
        [Theory]
        [GenericMethodData( nameof( GenericEqualityTestsData<T>.CreateCtorTestData ) )]
        public void Create_ShouldCreateWithCorrectProperties(T first, T second, bool expected)
        {
            var sut = Core.Equality.Create( first, second );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        First = first,
                        Second = second,
                        Result = expected
                    } );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var value1 = Fixture.Create<T>();
            var value2 = Fixture.Create<T>();

            var sut = new Equality<T>( value1, value2 );
            var expected = Core.Hash.Default.Add( value1 ).Add( value2 ).Add( sut.Result ).Value;

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEqualityTestsData<T>.CreateCtorTestData ) )]
        public void Ctor_ShouldCreateWithCorrectResult(T first, T second, bool expected)
        {
            var sut = new Equality<T>( first, second );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        First = first,
                        Second = second,
                        Result = expected
                    } );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEqualityTestsData<T>.CreateEqualsTestData ) )]
        public void Equals_ShouldReturnCorrectResult(T first1, T second1, T first2, T second2, bool expected)
        {
            var a = new Equality<T>( first1, second1 );
            var b = new Equality<T>( first2, second2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEqualityTestsData<T>.CreateConversionOperatorTestData ) )]
        public void BoolConversionOperator_ShouldReturnUnderlyingResult(T first, T second)
        {
            var sut = new Equality<T>( first, second );

            var result = (bool) sut;

            result.Should().Be( sut.Result );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEqualityTestsData<T>.CreateConversionOperatorTestData ) )]
        public void NegateOperator_ShouldReturnNegatedUnderlyingResult(T first, T second)
        {
            var sut = new Equality<T>( first, second );

            var result = ! sut;

            result.Should().Be( ! sut.Result );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEqualityTestsData<T>.CreateEqualsTestData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(T first1, T second1, T first2, T second2, bool expected)
        {
            var a = new Equality<T>( first1, second1 );
            var b = new Equality<T>( first2, second2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericEqualityTestsData<T>.CreateNotEqualsTestData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(T first1, T second1, T first2, T second2, bool expected)
        {
            var a = new Equality<T>( first1, second1 );
            var b = new Equality<T>( first2, second2 );

            var result = a != b;

            result.Should().Be( expected );
        }
    }
}
