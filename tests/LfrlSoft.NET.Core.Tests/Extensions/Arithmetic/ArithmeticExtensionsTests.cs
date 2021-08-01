using System;
using AutoFixture;
using FluentAssertions;
using LfrlSoft.NET.Core.Extensions;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Extensions.Arithmetic
{
    public class ArithmeticExtensionsTests : TestsBase
    {
        [Theory]
        [InlineData( -3L, 3L, 0L )]
        [InlineData( -2L, 3L, 1L )]
        [InlineData( -1L, 3L, 2L )]
        [InlineData( 0L, 3L, 0L )]
        [InlineData( 1L, 3L, 1L )]
        [InlineData( 2L, 3L, 2L )]
        [InlineData( 3L, 3L, 0L )]
        [InlineData( 4L, 3L, 1L )]
        [InlineData( 5L, 3L, 2L )]
        [InlineData( 6L, 3L, 0L )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForInt64(long a, long b, long expected)
        {
            var result = a.EuclidModulo( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrow_WhenDivisorIsZero_ForInt64()
        {
            var dividend = Fixture.Create<long>();

            Action action = () =>
            {
                var _ = dividend.EuclidModulo( 0 );
            };

            action.Should().Throw<DivideByZeroException>();
        }

        [Theory]
        [InlineData( -3, 3, 0 )]
        [InlineData( -2, 3, 1 )]
        [InlineData( -1, 3, 2 )]
        [InlineData( 0, 3, 0 )]
        [InlineData( 1, 3, 1 )]
        [InlineData( 2, 3, 2 )]
        [InlineData( 3, 3, 0 )]
        [InlineData( 4, 3, 1 )]
        [InlineData( 5, 3, 2 )]
        [InlineData( 6, 3, 0 )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForInt32(int a, int b, int expected)
        {
            var result = a.EuclidModulo( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrow_WhenDivisorIsZero_ForInt32()
        {
            var dividend = Fixture.Create<int>();

            Action action = () =>
            {
                var _ = dividend.EuclidModulo( 0 );
            };

            action.Should().Throw<DivideByZeroException>();
        }

        [Theory]
        [InlineData( -3, 3, 0 )]
        [InlineData( -2, 3, 1 )]
        [InlineData( -1, 3, 2 )]
        [InlineData( 0, 3, 0 )]
        [InlineData( 1, 3, 1 )]
        [InlineData( 2, 3, 2 )]
        [InlineData( 3, 3, 0 )]
        [InlineData( 4, 3, 1 )]
        [InlineData( 5, 3, 2 )]
        [InlineData( 6, 3, 0 )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForInt16(short a, short b, short expected)
        {
            var result = a.EuclidModulo( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrow_WhenDivisorIsZero_ForInt16()
        {
            var dividend = Fixture.Create<short>();

            Action action = () =>
            {
                var _ = dividend.EuclidModulo( 0 );
            };

            action.Should().Throw<DivideByZeroException>();
        }

        [Theory]
        [InlineData( -3, 3, 0 )]
        [InlineData( -2, 3, 1 )]
        [InlineData( -1, 3, 2 )]
        [InlineData( 0, 3, 0 )]
        [InlineData( 1, 3, 1 )]
        [InlineData( 2, 3, 2 )]
        [InlineData( 3, 3, 0 )]
        [InlineData( 4, 3, 1 )]
        [InlineData( 5, 3, 2 )]
        [InlineData( 6, 3, 0 )]
        public void EuclidModulo_ShouldReturnCorrectResult_ForInt8(sbyte a, sbyte b, sbyte expected)
        {
            var result = a.EuclidModulo( b );

            result.Should().Be( expected );
        }

        [Fact]
        public void EuclidModulo_ShouldThrow_WhenDivisorIsZero_ForInt8()
        {
            var dividend = Fixture.Create<sbyte>();

            Action action = () =>
            {
                var _ = dividend.EuclidModulo( 0 );
            };

            action.Should().Throw<DivideByZeroException>();
        }
    }
}
