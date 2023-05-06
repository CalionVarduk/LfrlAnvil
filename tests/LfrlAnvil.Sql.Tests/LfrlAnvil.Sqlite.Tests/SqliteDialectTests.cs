namespace LfrlAnvil.Sqlite.Tests;

public class SqliteDialectTests : TestsBase
{
    [Fact]
    public void Instance_ShouldHaveCorrectName()
    {
        var sut = SqliteDialect.Instance;
        sut.Name.Should().Be( "SQLite" );
    }
}
