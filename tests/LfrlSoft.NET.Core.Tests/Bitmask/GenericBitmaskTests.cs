using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using LfrlSoft.NET.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Bitmask
{
    [GenericTestClass( typeof( GenericBitmaskTestsData<> ) )]
    public abstract class GenericBitmaskTests<T> : TestsBase
        where T : struct, IConvertible, IComparable
    {
        [Fact]
        public void Create_ShouldCreateCorrectBitmask()
        {
            var value = Fixture.Create<T>();

            var sut = Core.Bitmask.Create( value );

            sut.Value.Should().Be( value );
        }

        [Fact]
        public void Empty_ShouldHaveDefaultValue()
        {
            var sut = Bitmask<T>.Empty;

            sut.Value.Should().Be( default( T ) );
        }

        [Fact]
        public void All_ShouldHaveSanitizedValueContainingAllPossibleFlags()
        {
            var maxBitIndex = Bitmask<T>.BitCount - 1;
            var maxValue = ((1UL << maxBitIndex) - 1) | (1UL << maxBitIndex);

            var sut = Bitmask<T>.All;
            var expected = new Bitmask<T>( GenericBitmaskTestsData<T>.Convert( maxValue ) ).Sanitize().Value;

            sut.Value.Should().Be( expected );
        }

        [Fact]
        public void Ctor_ShouldCreateWithCorrectValue()
        {
            var value = Fixture.Create<T>();

            var sut = new Bitmask<T>( value );

            sut.Value.Should().Be( value );
        }

        [Fact]
        public void GetHashCode_ShouldReturnValue()
        {
            var value = Fixture.Create<T>();

            var sut = new Bitmask<T>( value );
            var expected = value.GetHashCode();

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Fact]
        public void Clear_ShouldReturnBitmaskWithZeroValue()
        {
            var value = Fixture.Create<T>();
            var sut = new Bitmask<T>( value );

            var result = sut.Clear();

            result.Value.Should().Be( default( T ) );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetContainsAnyData ) )]
        public void ContainsAny_ShouldReturnCorrectResult(T value1, T value2, bool expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a.ContainsAny( b );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetContainsAllData ) )]
        public void ContainsAll_ShouldReturnCorrectResult(T value1, T value2, bool expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a.ContainsAll( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void ContainsBit_ShouldThrowWhenBitIndexIsLessThanZero()
        {
            var sut = new Bitmask<T>();

            Action action = () =>
            {
                var _ = sut.ContainsBit( -1 );
            };

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ContainsBit_ShouldThrowWhenBitIndexIsTooLarge()
        {
            var sut = new Bitmask<T>();

            Action action = () =>
            {
                var _ = sut.ContainsBit( Bitmask<T>.BitCount );
            };

            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetContainsBitData ) )]
        public void ContainsBit_ShouldReturnCorrectResult(T value, int bitIndex, bool expected)
        {
            var sut = new Bitmask<T>( value );

            var result = sut.ContainsBit( bitIndex );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetSetData ) )]
        public void Set_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a.Set( b );

            result.Value.Should().Be( expected );
        }

        [Fact]
        public void SetBit_ShouldThrowWhenBitIndexIsLessThanZero()
        {
            var sut = new Bitmask<T>();

            Action action = () =>
            {
                var _ = sut.SetBit( -1 );
            };

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetBit_ShouldThrowWhenBitIndexIsTooLarge()
        {
            var sut = new Bitmask<T>();

            Action action = () =>
            {
                var _ = sut.SetBit( Bitmask<T>.BitCount );
            };

            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetSetBitData ) )]
        public void SetBit_ShouldReturnCorrectResult(T value, int bitIndex, T expected)
        {
            var sut = new Bitmask<T>( value );

            var result = sut.SetBit( bitIndex );

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetUnsetData ) )]
        public void Unset_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a.Unset( b );

            result.Value.Should().Be( expected );
        }

        [Fact]
        public void UnsetBit_ShouldThrowWhenBitIndexIsLessThanZero()
        {
            var sut = new Bitmask<T>();

            Action action = () =>
            {
                var _ = sut.UnsetBit( -1 );
            };

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UnsetBit_ShouldThrowWhenBitIndexIsTooLarge()
        {
            var sut = new Bitmask<T>();

            Action action = () =>
            {
                var _ = sut.UnsetBit( Bitmask<T>.BitCount );
            };

            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetUnsetBitData ) )]
        public void UnsetBit_ShouldReturnCorrectResult(T value, int bitIndex, T expected)
        {
            var sut = new Bitmask<T>( value );

            var result = sut.UnsetBit( bitIndex );

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetIntersectData ) )]
        public void Intersect_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a.Intersect( b );

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetAlternateData ) )]
        public void Alternate_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a.Alternate( b );

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetNegateData ) )]
        public void Negate_ShouldReturnCorrectResult(T value, T expected)
        {
            var sut = new Bitmask<T>( value );

            var result = sut.Negate();

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetEqualsData ) )]
        public void Equals_ShouldReturnCorrectResult(T value1, T value2, bool expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetCompareToData ) )]
        public void CompareTo_ShouldReturnCorrectResult(T value1, T value2, int expectedSign)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a.CompareTo( b );

            Math.Sign( result ).Should().Be( expectedSign );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetEnumeratorData ) )]
        public void GetEnumerator_ShouldReturnCorrectResult(T value, IEnumerable<T> expected)
        {
            var sut = new Bitmask<T>( value );

            var result = sut.AsEnumerable();

            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetCountData ) )]
        public void Count_ShouldReturnCorrectResult(T value, int expected)
        {
            var sut = new Bitmask<T>( value );

            var result = sut.Count;

            result.Should().Be( expected );
        }

        [Fact]
        public void TConversionOperator_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<T>();
            var sut = new Bitmask<T>( value );

            var result = (T)sut;

            result.Should().Be( value );
        }

        [Fact]
        public void BitmaskConversionOperator_ShouldCreateProperBitmask()
        {
            var value = Fixture.Create<T>();

            var result = (Bitmask<T>)value;

            result.Value.Should().Be( value );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetEqualsData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(T value1, T value2, bool expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetNotEqualsData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(T value1, T value2, bool expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a != b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.CreateGreaterThanComparisonTestData ) )]
        public void GreaterThanOperator_ShouldReturnCorrectResult(T value1, T value2, bool expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a > b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.CreateGreaterThanOrEqualToComparisonTestData ) )]
        public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(T value1, T value2, bool expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a >= b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.CreateLessThanComparisonTestData ) )]
        public void LessThanOperator_ShouldReturnCorrectResult(T value1, T value2, bool expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a < b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.CreateLessThanOrEqualToComparisonTestData ) )]
        public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(T value1, T value2, bool expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a <= b;

            result.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetSetData ) )]
        public void BitwiseOrOperator_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a | b;

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetSetData ) )]
        public void BitwiseOrOperator_WithSecondAsT_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var a = new Bitmask<T>( value1 );

            var result = a | value2;

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetSetData ) )]
        public void BitwiseOrOperator_WithFirstAsT_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var b = new Bitmask<T>( value2 );

            var result = value1 | b;

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetIntersectData ) )]
        public void BitwiseAndOperator_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a & b;

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetIntersectData ) )]
        public void BitwiseAndOperator_WithSecondAsT_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var a = new Bitmask<T>( value1 );

            var result = a & value2;

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetIntersectData ) )]
        public void BitwiseAndOperator_WithFirstAsT_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var b = new Bitmask<T>( value2 );

            var result = value1 & b;

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetAlternateData ) )]
        public void BitwiseXorOperator_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var a = new Bitmask<T>( value1 );
            var b = new Bitmask<T>( value2 );

            var result = a ^ b;

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetAlternateData ) )]
        public void BitwiseXorOperator_WithSecondAsT_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var a = new Bitmask<T>( value1 );

            var result = a ^ value2;

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetAlternateData ) )]
        public void BitwiseXorOperator_WithFirstAsT_ShouldReturnCorrectResult(T value1, T value2, T expected)
        {
            var b = new Bitmask<T>( value2 );

            var result = value1 ^ b;

            result.Value.Should().Be( expected );
        }

        [Theory]
        [GenericMethodData( nameof( GenericBitmaskTestsData<T>.GetNegateData ) )]
        public void BitwiseNegateOperator_ShouldReturnCorrectResult(T value, T expected)
        {
            var sut = new Bitmask<T>( value );

            var result = ~sut;

            result.Value.Should().Be( expected );
        }

        protected void BitCount_ShouldBeCorrect_Impl(int expected)
        {
            var sut = Bitmask<T>.BitCount;

            sut.Should().Be( expected );
        }

        protected void BaseType_ShouldBeCorrect_Impl(Type expected)
        {
            var sut = Bitmask<T>.BaseType;

            sut.Should().Be( expected );
        }

        protected void Sanitize_ShouldReturnCorrectResult_Impl(T value, T expected)
        {
            var sut = new Bitmask<T>( value );

            var result = sut.Sanitize();

            result.Value.Should().Be( expected );
        }
    }
}
