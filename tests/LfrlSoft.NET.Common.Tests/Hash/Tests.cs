using System;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Common.Tests.Extensions;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Hash
{
    [TestClass( typeof( HashTestsData ) )]
    public class Tests
    {
        protected readonly IFixture Fixture = new Fixture();

        [Fact]
        public void Ctor_ShouldCreateWithCorrectValue()
        {
            var value = Fixture.Create<int>();

            var sut = new Common.Hash( value );

            sut.Value.Should().Be( value );
        }

        [Fact]
        public void Add_ShouldCreateCorrectHashInstance_WhenParameterIsNull()
        {
            var value = Fixture.CreateDefault<string>();
            var sut = Common.Hash.Default;

            var result = sut.Add( value );

            result.Value.Should().Be( 84696351 );
        }

        [Fact]
        public void Add_ShouldCreateCorrectHashInstance_WhenParameterIsNotNull()
        {
            var value = 1234567890;
            var sut = Common.Hash.Default;

            var result = sut.Add( value );

            result.Value.Should().Be( -919047883 );
        }

        [Fact]
        public void AddRange_ShouldCreateCorrectHashInstance()
        {
            var range = new[] { 1234567890, 987654321, 1010101010 };
            var sut = Common.Hash.Default;

            var result = sut.AddRange( range );

            result.Value.Should().Be( 104542330 );
        }

        [Fact]
        public void GetHashCode_ShouldReturnValue()
        {
            var value = Fixture.Create<int>();
            var sut = new Common.Hash( value );

            var result = sut.GetHashCode();

            result.Should().Be( value );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.CreateEqualsTestData ) )]
        public void Equals_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Common.Hash( value1 );
            var b = new Common.Hash( value2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.CreateCompareToTestData ) )]
        public void CompareTo_ShouldReturnCorrectResult(int value1, int value2, int expectedSign)
        {
            var a = new Common.Hash( value1 );
            var b = new Common.Hash( value2 );

            var result = a.CompareTo( b );

            Math.Sign( result ).Should().Be( expectedSign );
        }

        [Fact]
        public void IntConversionOperator_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<int>();
            var sut = new Common.Hash( value );

            var result = (int) sut;

            result.Should().Be( value );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.CreateEqualsTestData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Common.Hash( value1 );
            var b = new Common.Hash( value2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.CreateNotEqualsTestData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Common.Hash( value1 );
            var b = new Common.Hash( value2 );

            var result = a != b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.CreateGreaterThanComparisonTestData ) )]
        public void GreaterThanOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Common.Hash( value1 );
            var b = new Common.Hash( value2 );

            var result = a > b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.CreateLessThanOrEqualToComparisonTestData ) )]
        public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Common.Hash( value1 );
            var b = new Common.Hash( value2 );

            var result = a <= b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.CreateLessThanComparisonTestData ) )]
        public void LessThanOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Common.Hash( value1 );
            var b = new Common.Hash( value2 );

            var result = a < b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.CreateGreaterThanOrEqualToComparisonTestData ) )]
        public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Common.Hash( value1 );
            var b = new Common.Hash( value2 );

            var result = a >= b;

            result.Should().Be( expected );
        }
    }
}
