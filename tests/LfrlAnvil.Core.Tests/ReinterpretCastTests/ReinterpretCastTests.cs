using System.Collections.Generic;

namespace LfrlAnvil.Tests.ReinterpretCastTests;

public class ReinterpretCastTests : TestsBase
{
    [Fact]
    public void To_ShouldReturnParameter_WhenParameterIsNotNull()
    {
        var value = new List<int>();
        var result = ReinterpretCast.To<IEnumerable<int>>( value );
        result.Should().BeSameAs( value );
    }

    [Fact]
    public void To_ShouldReturnNull_WhenParameterIsNull()
    {
        var result = ReinterpretCast.To<string>( null );
        result.Should().BeNull();
    }
}
