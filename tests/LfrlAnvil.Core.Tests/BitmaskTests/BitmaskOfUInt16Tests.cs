using AutoFixture;
using Xunit;

namespace LfrlAnvil.Tests.BitmaskTests;

public class BitmaskOfUInt16Tests : GenericBitmaskTests<ushort>
{
    [Fact]
    public void BitCount_ShouldBe16()
    {
        BitCount_ShouldBeCorrect_Impl( 16 );
    }

    [Fact]
    public void BaseType_ShouldBeTypeOfUshort()
    {
        BaseType_ShouldBeCorrect_Impl( typeof( ushort ) );
    }

    [Fact]
    public void Sanitize_ShouldReturnUnderlyingValue()
    {
        var value = Fixture.Create<ushort>();
        Sanitize_ShouldReturnCorrectResult_Impl( value, value );
    }
}
