using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionInt8Tests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 123, "123" )]
    [InlineData( 0, "0" )]
    [InlineData( -123, "-123" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(sbyte value, string expected)
    {
        var sut = _provider.GetByType<sbyte>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfInt8Type()
    {
        var sut = _provider.GetByType<sbyte>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }
}
