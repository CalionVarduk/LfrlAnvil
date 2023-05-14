using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionUInt16Tests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 12345, "12345" )]
    [InlineData( 0, "0" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(ushort value, string expected)
    {
        var sut = _provider.GetByType<ushort>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfUInt16Type()
    {
        var sut = _provider.GetByType<ushort>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( 12345 )]
    [InlineData( 0 )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(ushort value)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<ushort>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( (long)value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfUInt16Type()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<ushort>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }
}
