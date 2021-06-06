using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Common.Tests.Bitmask
{
    public class UInt32 : BitmaskTests<uint>
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

        [Fact]
        public void Sanitize_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<uint>();
            Sanitize_ShouldReturnCorrectResult_Impl( value, value );
        }
    }
}
