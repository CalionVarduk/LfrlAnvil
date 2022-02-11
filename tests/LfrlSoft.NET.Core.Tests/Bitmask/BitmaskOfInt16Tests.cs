using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Bitmask
{
    public class BitmaskOfInt16Tests : GenericBitmaskTests<short>
    {
        [Fact]
        public void BitCount_ShouldBe16()
        {
            BitCount_ShouldBeCorrect_Impl( 16 );
        }

        [Fact]
        public void BaseType_ShouldBeTypeOfShort()
        {
            BaseType_ShouldBeCorrect_Impl( typeof( short ) );
        }

        [Fact]
        public void Sanitize_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<short>();
            Sanitize_ShouldReturnCorrectResult_Impl( value, value );
        }
    }
}
