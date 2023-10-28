using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionInt32Tests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    [InlineData( -1234567, "-1234567" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(int value, string expected)
    {
        var sut = _provider.GetByType<int>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfInt32Type()
    {
        var sut = _provider.GetByType<int>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( 1234567 )]
    [InlineData( 0 )]
    [InlineData( -1234567 )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(int value)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<int>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( (long)value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfInt32Type()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<int>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }

    [Fact]
    public void SetNullParameter_ShouldUpdateParameterCorrectly()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<int>();

        sut.SetNullParameter( parameter );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().BeSameAs( DBNull.Value );
        }
    }
}
