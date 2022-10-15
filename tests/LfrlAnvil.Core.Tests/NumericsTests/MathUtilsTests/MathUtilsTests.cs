using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Numerics;

namespace LfrlAnvil.Tests.NumericsTests.MathUtilsTests;

public class MathUtilsTests : TestsBase
{
    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( uint.MaxValue )]
    [InlineData( long.MaxValue )]
    public void ToUnsigned_ShouldReturnCorrectResult_WhenValueIsNotNegative(long value)
    {
        var sign = Fixture.Create<int>();
        var refSign = sign;
        var result = MathUtils.ToUnsigned( value, ref refSign );

        using ( new AssertionScope() )
        {
            result.Should().Be( (ulong)value );
            refSign.Should().Be( sign );
        }
    }

    [Theory]
    [InlineData( -1, 1 )]
    [InlineData( int.MinValue, (ulong)int.MaxValue + 1 )]
    [InlineData( long.MinValue, (ulong)long.MaxValue + 1 )]
    public void ToUnsigned_ShouldReturnCorrectResult_WhenValueIsNegative(long value, ulong expected)
    {
        var sign = Fixture.Create<int>();
        var refSign = sign;
        var result = MathUtils.ToUnsigned( value, ref refSign );

        using ( new AssertionScope() )
        {
            result.Should().Be( expected );
            refSign.Should().Be( -sign );
        }
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1, 1 )]
    [InlineData( uint.MaxValue, uint.MaxValue )]
    [InlineData( long.MaxValue, long.MaxValue )]
    public void ToSigned_ShouldReturnCorrectResult_WhenValueIsNotNegative(ulong value, long expected)
    {
        var result = MathUtils.ToSigned( value, sign: 1 );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 1, -1 )]
    [InlineData( uint.MaxValue, -uint.MaxValue )]
    [InlineData( long.MaxValue, -long.MaxValue )]
    [InlineData( (ulong)long.MaxValue + 1, long.MinValue )]
    public void ToSigned_ShouldReturnCorrectResult_WhenValueIsNegative(ulong value, long expected)
    {
        var result = MathUtils.ToSigned( value, sign: -1 );
        result.Should().Be( expected );
    }

    [Fact]
    public void ToSigned_ShouldThrowOverflowException_WhenValueIsNotNegativeAndExceedsInt64MaxValue()
    {
        var action = Lambda.Of( () => MathUtils.ToSigned( (ulong)long.MaxValue + 1, sign: 1 ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void ToSigned_ShouldThrowOverflowException_WhenValueIsNegativeAndExceedsInt64MinValue()
    {
        var action = Lambda.Of( () => MathUtils.ToSigned( (ulong)long.MaxValue + 2, sign: -1 ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( 0, 0, 0, 0 )]
    [InlineData( 100, 200, 0, 20000 )]
    [InlineData( long.MaxValue, 1, 0, (ulong)long.MaxValue )]
    [InlineData( 1, long.MaxValue, 0, (ulong)long.MaxValue )]
    [InlineData( 3, 6148914691236517205, 0, ulong.MaxValue )]
    [InlineData( 1L << 62, 4, 1, 0 )]
    [InlineData( long.MaxValue, long.MaxValue, 4611686018427387903UL, 1 )]
    public void BigMulU128_ShouldReturnCorrectResult(ulong a, ulong b, ulong expectedHigh, ulong expectedLow)
    {
        var (high, low) = MathUtils.BigMulU128( a, b );

        using ( new AssertionScope() )
        {
            high.Should().Be( expectedHigh );
            low.Should().Be( expectedLow );
        }
    }

    [Theory]
    [InlineData( 0, 0, 1, 0, 0, 0 )]
    [InlineData( 0, 0, ulong.MaxValue, 0, 0, 0 )]
    [InlineData( 0, 6, 2, 0, 3, 0 )]
    [InlineData( 0, 7, 3, 0, 2, 1 )]
    [InlineData( 0, 123456789054321, ulong.MaxValue, 0, 0, 123456789054321 )]
    [InlineData( ulong.MaxValue, ulong.MaxValue, 1, ulong.MaxValue, ulong.MaxValue, 0 )]
    [InlineData( 64, 123456789, 456321, 0, 2587195462662324, 410209 )]
    [InlineData( 4241943008571542805, 1432135365720863893, 1481763723909, 2862766, 2486493130135359103, 639595366042 )]
    [InlineData( 3833045038591339946, 573693638711787792, 6350190974633306954, 0, 11134657387840150926, 701016773156101124 )]
    [InlineData( 4605921407466066877, 7887564637810862666, 4648446336110425948, 0, 18277989522460577627, 2421975627461220502 )]
    [InlineData( 5304349795661473109, 2867999000909509222, 219012612294601820, 24, 4046856291975950627, 213928629415675346 )]
    [InlineData( 829678622093003025, 1975874835207407424, 4555612114624923477, 0, 3359563724937039266, 3558444750694797942 )]
    [InlineData( 0, 6149394939174668089, 2208266555358409424, 0, 2, 1732861828457849241 )]
    [InlineData( 123456789, 9876543210, 1UL << 63, 0, 246913578, 9876543210 )]
    public void BigDivU128_ShouldReturnCorrectResult(
        ulong aHigh,
        ulong aLow,
        ulong b,
        ulong expectedHigh,
        ulong expectedLow,
        ulong expectedRemainder)
    {
        var (high, low, remainder) = MathUtils.BigDivU128( aHigh, aLow, b );

        using ( new AssertionScope() )
        {
            high.Should().Be( expectedHigh );
            low.Should().Be( expectedLow );
            remainder.Should().Be( expectedRemainder );
        }
    }

    [Fact]
    public void BigDivU128_ShouldThrowDivideByZeroException_WhenRightIsEqualToZero()
    {
        var action = Lambda.Of( () => MathUtils.BigDivU128( Fixture.Create<ulong>(), Fixture.Create<ulong>(), 0 ) );
        action.Should().ThrowExactly<DivideByZeroException>();
    }
}
