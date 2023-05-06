using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionDateOnlyTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( "1970-01-01" )]
    [InlineData( "2023-04-09" )]
    [InlineData( "2022-11-23" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dt)
    {
        var value = DateOnly.Parse( dt );
        var expected = $"'{dt}'";
        var sut = _provider.GetByType<DateOnly>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfDateOnlyType()
    {
        var sut = _provider.GetByType<DateOnly>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }
}
