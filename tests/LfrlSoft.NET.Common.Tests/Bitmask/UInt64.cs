using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Bitmask
{
    public class UInt64 : BitmaskTests<ulong>
    {
        [Fact]
        public void BitCount_ShouldBeCorrect()
        {
            BitCount_ShouldBeCorrect_Impl( 64 );
        }

        [Fact]
        public void BaseType_ShouldBeCorrect()
        {
            BaseType_ShouldBeCorrect_Impl( typeof( ulong ) );
        }

        [Fact]
        public void Sanitize_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<ulong>();
            Sanitize_ShouldReturnCorrectResult_Impl( value, value );
        }
    }
}
