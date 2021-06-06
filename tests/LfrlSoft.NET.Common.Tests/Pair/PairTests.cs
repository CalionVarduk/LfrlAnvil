using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Tests.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Pair
{
    [GenericTestClass( typeof( PairTestsData<,> ) )]
    public abstract class PairTests<T1, T2>
    {
        protected readonly IFixture Fixture = new Fixture();

        [Fact]
        public void Create_ShouldCreateCorrectPair()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = Common.Pair.Create( first, second );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        First = first,
                        Second = second
                    } );
        }

        [Fact]
        public void CtorWithValue_ShouldCreateWithCorrectValues()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new Pair<T1, T2>( first, second );

            sut.Should()
                .BeEquivalentTo(
                    new
                    {
                        First = first,
                        Second = second
                    } );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();

            var sut = new Pair<T1, T2>( first, second );
            var expected = Common.Hash.Default.Add( first ).Add( second ).Value;

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Fact]
        public void SetFirst_ShouldReturnCorrectResult()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();
            var other = Fixture.Create<double>();

            var sut = new Pair<T1, T2>( first, second );

            var result = sut.SetFirst( other );

            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        First = other,
                        Second = second
                    } );
        }

        [Fact]
        public void SetSecond_ShouldReturnCorrectResult()
        {
            var first = Fixture.Create<T1>();
            var second = Fixture.Create<T2>();
            var other = Fixture.Create<double>();

            var sut = new Pair<T1, T2>( first, second );

            var result = sut.SetSecond( other );

            result.Should()
                .BeEquivalentTo(
                    new
                    {
                        First = first,
                        Second = other
                    } );
        }

        [Theory]
        [GenericMethodData( nameof( PairTestsData<T1, T2>.CreateEqualsTestData ) )]
        public void Equals_ShouldReturnCorrectResult(T1 first1, T2 second1, T1 first2, T2 second2, bool expected)
        {
            var a = new Pair<T1, T2>( first1, second1 );
            var b = new Pair<T1, T2>( first2, second2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( PairTestsData<T1, T2>.CreateEqualsTestData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(T1 first1, T2 second1, T1 first2, T2 second2, bool expected)
        {
            var a = new Pair<T1, T2>( first1, second1 );
            var b = new Pair<T1, T2>( first2, second2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( PairTestsData<T1, T2>.CreateNotEqualsTestData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(T1 first1, T2 second1, T1 first2, T2 second2, bool expected)
        {
            var a = new Pair<T1, T2>( first1, second1 );
            var b = new Pair<T1, T2>( first2, second2 );

            var result = a != b;

            result.Should().Be( expected );
        }
    }
}
