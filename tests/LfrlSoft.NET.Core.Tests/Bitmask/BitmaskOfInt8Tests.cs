using AutoFixture;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Bitmask
{
    public class BitmaskOfInt8Tests : GenericBitmaskTests<sbyte>
    {
        [Fact]
        public void BitCount_ShouldBe8()
        {
            BitCount_ShouldBeCorrect_Impl( 8 );
        }

        [Fact]
        public void BaseType_ShouldBeTypeOfSbyte()
        {
            BaseType_ShouldBeCorrect_Impl( typeof( sbyte ) );
        }

        [Fact]
        public void Sanitize_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<sbyte>();
            Sanitize_ShouldReturnCorrectResult_Impl( value, value );
        }
    }
}
