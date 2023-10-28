using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionByteArrayTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsEmpty()
    {
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToDbLiteral( Array.Empty<byte>() );
        result.Should().Be( "X''" );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsNotEmpty()
    {
        var value = new byte[] { 0, 10, 21, 31, 42, 58, 73, 89, 104, 129, 155, 181, 206, 233, 255 };
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( "X'000A151F2A3A495968819BB5CEE9FF'" );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfByteArrayType()
    {
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Fact]
    public void TrySetParameter_ShouldUpdateParameterCorrectly()
    {
        var value = new byte[] { 0, 10, 21, 31, 42, 58, 73, 89, 104, 129, 155, 181, 206, 233, 255 };
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<byte[]>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Binary );
            parameter.Value.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfByteArrayType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<byte[]>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }

    [Fact]
    public void SetNullParameter_ShouldUpdateParameterCorrectly()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<byte[]>();

        sut.SetNullParameter( parameter );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.Binary );
            parameter.Value.Should().BeSameAs( DBNull.Value );
        }
    }
}
