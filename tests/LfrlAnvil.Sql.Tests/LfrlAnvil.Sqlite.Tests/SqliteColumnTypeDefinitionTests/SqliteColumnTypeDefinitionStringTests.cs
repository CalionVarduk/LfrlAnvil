using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionStringTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( "foo", "'foo'" )]
    [InlineData( "", "''" )]
    [InlineData( "FOOBAR", "'FOOBAR'" )]
    [InlineData( "'", "''''" )]
    [InlineData( "f'oo'bar'", "'f''oo''bar'''" )]
    [InlineData( "'FOO'BAR", "'''FOO''BAR'" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string value, string expected)
    {
        var sut = _provider.GetByType<string>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfStringType()
    {
        var sut = _provider.GetByType<string>();
        var result = sut.TryToDbLiteral( '0' );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( "foo" )]
    [InlineData( "" )]
    [InlineData( "FOOBAR" )]
    [InlineData( "'FOO'BAR" )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(string value)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<string>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.String );
            parameter.Value.Should().Be( value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfStringType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<string>();

        var result = sut.TrySetParameter( parameter, '0' );

        result.Should().BeFalse();
    }
}
