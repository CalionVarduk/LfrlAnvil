using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionCharTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 'a', "'a'" )]
    [InlineData( '0', "'0'" )]
    [InlineData( 'Z', "'Z'" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(char value, string expected)
    {
        var sut = _provider.GetByType<char>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfCharType()
    {
        var sut = _provider.GetByType<char>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }
}
