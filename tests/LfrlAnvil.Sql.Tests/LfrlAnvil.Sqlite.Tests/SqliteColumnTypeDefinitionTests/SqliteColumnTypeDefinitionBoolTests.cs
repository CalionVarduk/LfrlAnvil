using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionBoolTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( true, "1" )]
    [InlineData( false, "0" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(bool value, string expected)
    {
        var sut = _provider.GetByType<bool>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfBoolType()
    {
        var sut = _provider.GetByType<bool>();
        var result = sut.TryToDbLiteral( 0L );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true, 1L )]
    [InlineData( false, 0L )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(bool value, long expectedValue)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<bool>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( expectedValue );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfBoolType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<bool>();

        var result = sut.TrySetParameter( parameter, 0L );

        result.Should().BeFalse();
    }
}
