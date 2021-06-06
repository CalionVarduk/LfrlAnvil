using System;
using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Bitmask
{
    [Flags]
    public enum TestEnumUInt32 : uint
    {
        A = 0,
        B = 1,
        C = 2,
        D = 4,
        E = 8,
        F = 1U << 31
    }

    public class EnumUInt32 : BitmaskTests<TestEnumUInt32>
    {
        [Fact]
        public void BitCount_ShouldBeCorrect()
        {
            BitCount_ShouldBeCorrect_Impl( 32 );
        }

        [Fact]
        public void BaseType_ShouldBeCorrect()
        {
            BaseType_ShouldBeCorrect_Impl( typeof( uint ) );
        }

        [Theory]
        [InlineData( 0U, TestEnumUInt32.A )]
        [InlineData( 1U, TestEnumUInt32.B )]
        [InlineData( 2U, TestEnumUInt32.C )]
        [InlineData( 4U, TestEnumUInt32.D )]
        [InlineData( 8U, TestEnumUInt32.E )]
        [InlineData( 16U, TestEnumUInt32.A )]
        [InlineData( 17U, TestEnumUInt32.B )]
        [InlineData( 18U, TestEnumUInt32.C )]
        [InlineData( 20U, TestEnumUInt32.D )]
        [InlineData( 24U, TestEnumUInt32.E )]
        [InlineData( 16U | (1U << 31), TestEnumUInt32.F )]
        [InlineData( 17U | (1U << 31), TestEnumUInt32.B | TestEnumUInt32.F )]
        [InlineData( 18U | (1U << 31), TestEnumUInt32.C | TestEnumUInt32.F )]
        [InlineData( 20U | (1U << 31), TestEnumUInt32.D | TestEnumUInt32.F )]
        [InlineData( 24U | (1U << 31), TestEnumUInt32.E | TestEnumUInt32.F )]
        public void Sanitize_ShouldReturnCorrectResult(uint value, TestEnumUInt32 expected)
        {
            Sanitize_ShouldReturnCorrectResult_Impl( (TestEnumUInt32) value, expected );
        }
    }
}
