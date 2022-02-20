using System;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using Xunit;

namespace LfrlAnvil.Tests.ExtensionsTests.ArithmeticTests
{
    [TestClass( typeof( ArithmeticExtensionsTestsData ) )]
    public class ArithmeticExtensionsTests : TestsBase
    {
        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloFloatData ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForFloat(float a, float b, float expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForFloat()
        {
            var dividend = Fixture.Create<float>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloDoubleData ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForDouble(double a, double b, double expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForDouble()
        {
            var dividend = Fixture.Create<double>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloDecimalData ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForDecimal(decimal a, decimal b, decimal expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForDecimal()
        {
            var dividend = Fixture.Create<decimal>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloUint64Data ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForUint64(ulong a, ulong b, ulong expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForUint64()
        {
            var dividend = Fixture.Create<ulong>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloInt64Data ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForInt64(long a, long b, long expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForInt64()
        {
            var dividend = Fixture.Create<long>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloUint32Data ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForUint32(uint a, uint b, uint expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForUint32()
        {
            var dividend = Fixture.Create<uint>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloInt32Data ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForInt32(int a, int b, int expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForInt32()
        {
            var dividend = Fixture.Create<int>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloUint16Data ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForUint16(ushort a, ushort b, ushort expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForUint16()
        {
            var dividend = Fixture.Create<ushort>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloInt16Data ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForInt16(short a, short b, short expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForInt16()
        {
            var dividend = Fixture.Create<short>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloUint8Data ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForUint8(byte a, byte b, byte expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForUint8()
        {
            var dividend = Fixture.Create<byte>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetEuclidModuloInt8Data ) )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForInt8(sbyte a, sbyte b, sbyte expected)
        {
            var result = a.EuclidModulo( b );
            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrowDivideByZeroException_WhenDivisorIsZero_ForInt8()
        {
            var dividend = Fixture.Create<sbyte>();
            var action = Lambda.Of( () => dividend.EuclidModulo( 0 ) );
            action.Should().ThrowExactly<DivideByZeroException>();
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsEvenUint64Data ) )]
        public void IsEven_ShouldReturnCorrectResult_ForUint64(ulong value, bool expected)
        {
            var result = value.IsEven();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsOddUint64Data ) )]
        public void IsOdd_ShouldReturnCorrectResult_ForUint64(ulong value, bool expected)
        {
            var result = value.IsOdd();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsEvenInt64Data ) )]
        public void IsEven_ShouldReturnCorrectResult_ForInt64(long value, bool expected)
        {
            var result = value.IsEven();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsOddInt64Data ) )]
        public void IsOdd_ShouldReturnCorrectResult_ForInt64(long value, bool expected)
        {
            var result = value.IsOdd();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsEvenUint32Data ) )]
        public void IsEven_ShouldReturnCorrectResult_ForUint32(uint value, bool expected)
        {
            var result = value.IsEven();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsOddUint32Data ) )]
        public void IsOdd_ShouldReturnCorrectResult_ForUint32(uint value, bool expected)
        {
            var result = value.IsOdd();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsEvenInt32Data ) )]
        public void IsEven_ShouldReturnCorrectResult_ForInt32(int value, bool expected)
        {
            var result = value.IsEven();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsOddInt32Data ) )]
        public void IsOdd_ShouldReturnCorrectResult_ForInt32(int value, bool expected)
        {
            var result = value.IsOdd();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsEvenUint16Data ) )]
        public void IsEven_ShouldReturnCorrectResult_ForUint16(ushort value, bool expected)
        {
            var result = value.IsEven();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsOddUint16Data ) )]
        public void IsOdd_ShouldReturnCorrectResult_ForUint16(ushort value, bool expected)
        {
            var result = value.IsOdd();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsEvenInt16Data ) )]
        public void IsEven_ShouldReturnCorrectResult_ForInt16(short value, bool expected)
        {
            var result = value.IsEven();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsOddInt16Data ) )]
        public void IsOdd_ShouldReturnCorrectResult_ForInt16(short value, bool expected)
        {
            var result = value.IsOdd();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsEvenUint8Data ) )]
        public void IsEven_ShouldReturnCorrectResult_ForUint8(byte value, bool expected)
        {
            var result = value.IsEven();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsOddUint8Data ) )]
        public void IsOdd_ShouldReturnCorrectResult_ForUint8(byte value, bool expected)
        {
            var result = value.IsOdd();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsEvenInt8Data ) )]
        public void IsEven_ShouldReturnCorrectResult_ForInt8(sbyte value, bool expected)
        {
            var result = value.IsEven();
            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ArithmeticExtensionsTestsData.GetIsOddInt8Data ) )]
        public void IsOdd_ShouldReturnCorrectResult_ForInt8(sbyte value, bool expected)
        {
            var result = value.IsOdd();
            result.Should().Be( expected );
        }
    }
}
