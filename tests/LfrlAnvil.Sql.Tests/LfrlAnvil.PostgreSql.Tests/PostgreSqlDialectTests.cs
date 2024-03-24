namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDialectTests : TestsBase
{
    [Fact]
    public void Instance_ShouldHaveCorrectName()
    {
        var sut = PostgreSqlDialect.Instance;
        sut.Name.Should().Be( "PostgreSql" );
    }
}
