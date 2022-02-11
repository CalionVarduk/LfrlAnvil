using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Bitmask
{
    public class BitmaskOfUInt64Tests : GenericBitmaskTests<ulong>
    {
        [Fact]
        public void BitCount_ShouldBe64()
        {
            BitCount_ShouldBeCorrect_Impl( 64 );
        }

        [Fact]
        public void BaseType_ShouldBeTypeOfUlong()
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
