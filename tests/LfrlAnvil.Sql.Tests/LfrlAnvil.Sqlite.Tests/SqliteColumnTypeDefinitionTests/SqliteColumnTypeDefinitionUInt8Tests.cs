using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionUInt8Tests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 123, "123" )]
    [InlineData( 0, "0" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(byte value, string expected)
    {
        var sut = _provider.GetByType<byte>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfUInt8Type()
    {
        var sut = _provider.GetByType<byte>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( 123 )]
    [InlineData( 0 )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(byte value)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<byte>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( (long)value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfUInt8Type()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<byte>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }
}
