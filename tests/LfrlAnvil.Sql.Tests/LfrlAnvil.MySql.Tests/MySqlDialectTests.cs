namespace LfrlAnvil.MySql.Tests;

public class MySqlDialectTests : TestsBase
{
    [Fact]
    public void Instance_ShouldHaveCorrectName()
    {
        var sut = MySqlDialect.Instance;
        sut.Name.Should().Be( "MySql" );
    }
}
