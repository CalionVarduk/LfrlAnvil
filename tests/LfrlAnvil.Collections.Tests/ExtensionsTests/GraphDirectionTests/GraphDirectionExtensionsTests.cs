using LfrlAnvil.Collections.Extensions;

namespace LfrlAnvil.Collections.Tests.ExtensionsTests.GraphDirectionTests;

public class GraphDirectionExtensionsTests : TestsBase
{
    [Theory]
    [InlineData( GraphDirection.None, GraphDirection.None )]
    [InlineData( GraphDirection.In, GraphDirection.Out )]
    [InlineData( GraphDirection.Out, GraphDirection.In )]
    [InlineData( GraphDirection.Both, GraphDirection.Both )]
    public void Invert_ShouldReturnCorrectResult(GraphDirection sut, GraphDirection expected)
    {
        var result = sut.Invert();
        result.Should().Be( expected );
    }
}
