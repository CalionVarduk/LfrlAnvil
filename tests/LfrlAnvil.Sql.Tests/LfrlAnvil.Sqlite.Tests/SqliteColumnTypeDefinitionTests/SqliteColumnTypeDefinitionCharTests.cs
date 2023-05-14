using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

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

    [Theory]
    [InlineData( 'a', "a" )]
    [InlineData( '0', "0" )]
    [InlineData( 'Z', "Z" )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(char value, string expectedValue)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<char>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.String );
            parameter.Value.Should().Be( expectedValue );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfCharType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<char>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }
}
