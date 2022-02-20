using AutoFixture;
using Xunit;

namespace LfrlAnvil.Tests.BitmaskTests
{
    public class BitmaskOfUInt8Tests : GenericBitmaskTests<byte>
    {
        [Fact]
        public void BitCount_ShouldBe8()
        {
            BitCount_ShouldBeCorrect_Impl( 8 );
        }

        [Fact]
        public void BaseType_ShouldBeTypeOfByte()
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
