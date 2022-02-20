using System;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using Xunit;

namespace LfrlAnvil.Tests.HashTests
{
    [TestClass( typeof( HashTestsData ) )]
    public class HashTests : TestsBase
    {
        [Fact]
        public void Ctor_ShouldCreateWithCorrectValue()
        {
            var value = Fixture.Create<int>();
            var sut = new Hash( value );
            sut.Value.Should().Be( value );
        }

        [Fact]
        public void Add_ShouldCreateCorrectHashInstance_WhenParameterIsNull()
        {
            var value = Fixture.CreateDefault<string>();
            var sut = Hash.Default;

            var result = sut.Add( value );

            result.Value.Should().Be( 84696351 );
        }

        [Fact]
        public void Add_ShouldCreateCorrectHashInstance_WhenParameterIsNotNull()
        {
            var value = 1234567890;
            var sut = Hash.Default;

            var result = sut.Add( value );

            result.Value.Should().Be( -919047883 );
        }

        [Fact]
        public void AddRange_ShouldCreateCorrectHashInstance()
        {
            var range = new[] { 1234567890, 987654321, 1010101010 };
            var sut = Hash.Default;

            var result = sut.AddRange( range );

            result.Value.Should().Be( 104542330 );
        }

        [Fact]
        public void GetHashCode_ShouldReturnValue()
        {
            var value = Fixture.Create<int>();
            var sut = new Hash( value );

            var result = sut.GetHashCode();

            result.Should().Be( value );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.GetEqualsData ) )]
        public void Equals_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.GetCompareToData ) )]
        public void CompareTo_ShouldReturnCorrectResult(int value1, int value2, int expectedSign)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a.CompareTo( b );

            Math.Sign( result ).Should().Be( expectedSign );
        }

        [Fact]
        public void IntConversionOperator_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<int>();
            var sut = new Hash( value );

            var result = (int)sut;

            result.Should().Be( value );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.GetEqualsData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.GetNotEqualsData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a != b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.GetGreaterThanComparisonData ) )]
        public void GreaterThanOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a > b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.GetLessThanOrEqualToComparisonData ) )]
        public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a <= b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.GetLessThanComparisonData ) )]
        public void LessThanOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a < b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( HashTestsData.GetGreaterThanOrEqualToComparisonData ) )]
        public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(int value1, int value2, bool expected)
        {
            var a = new Hash( value1 );
            var b = new Hash( value2 );

            var result = a >= b;

            result.Should().Be( expected );
        }
    }
}
