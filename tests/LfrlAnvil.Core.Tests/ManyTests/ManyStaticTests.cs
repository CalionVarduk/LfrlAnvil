namespace LfrlAnvil.Tests.ManyTests;

public class ManyStaticTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnParamsArray()
    {
        var expected = new[] { 10, 20, 30 };
        var result = Many.Create( expected );
        result.Should().BeSameAs( expected );
    }
}
