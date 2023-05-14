using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionInt64Tests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    [InlineData( -1234567, "-1234567" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(long value, string expected)
    {
        var sut = _provider.GetByType<long>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfInt64Type()
    {
        var sut = _provider.GetByType<long>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( 1234567 )]
    [InlineData( 0 )]
    [InlineData( -1234567 )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(long value)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<long>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfInt64Type()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<long>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }
}
