using AutoFixture;
using Xunit;

namespace LfrlAnvil.Tests.BitmaskTests
{
    public class BitmaskOfInt32Tests : GenericBitmaskTests<int>
    {
        [Fact]
        public void BitCount_ShouldBe32()
        {
            BitCount_ShouldBeCorrect_Impl( 32 );
        }

        [Fact]
        public void BaseType_ShouldBeTypeOfInt()
        {
            BaseType_ShouldBeCorrect_Impl( typeof( int ) );
        }

        [Fact]
        public void Sanitize_ShouldReturnUnderlyingValue()
        {
            var value = Fixture.Create<int>();
            Sanitize_ShouldReturnCorrectResult_Impl( value, value );
        }
    }
}
