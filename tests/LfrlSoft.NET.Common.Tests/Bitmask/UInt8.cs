using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Bitmask
{
    public class UInt8 : BitmaskTests<byte>
    {
        [Fact]
        public void BitCount_ShouldBeCorrect()
        {
            BitCount_ShouldBeCorrect_Impl( 8 );
        }

        [Fact]
        public void BaseType_ShouldBeCorrect()
        {
            BaseType_ShouldBeCorrect_Impl( typeof( byte ) );
        }

        [Fact]
        public void Sanitize_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<byte>();
            Sanitize_ShouldReturnCorrectResult_Impl( value, value );
        }
    }
}
