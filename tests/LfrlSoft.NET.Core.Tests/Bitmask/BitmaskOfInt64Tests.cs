using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Bitmask
{
    public class BitmaskOfInt64Tests : GenericBitmaskTests<long>
    {
        [Fact]
        public void BitCount_ShouldBeCorrect()
        {
            BitCount_ShouldBeCorrect_Impl( 64 );
        }

        [Fact]
        public void BaseType_ShouldBeCorrect()
        {
            BaseType_ShouldBeCorrect_Impl( typeof( long ) );
        }

        [Fact]
        public void Sanitize_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<long>();
            Sanitize_ShouldReturnCorrectResult_Impl( value, value );
        }
    }
}
