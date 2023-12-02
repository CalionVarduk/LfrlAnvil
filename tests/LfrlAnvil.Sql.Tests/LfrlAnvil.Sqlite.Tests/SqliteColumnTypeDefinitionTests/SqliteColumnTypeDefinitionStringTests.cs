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

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult()
    {
        var value = "foo";
        var sut = _provider.GetByType<string>();
        var result = sut.TryToParameterValue( value );
        result.Should().BeSameAs( value );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfStringType()
    {
        var sut = _provider.GetByType<string>();
        var result = sut.TryToParameterValue( '0' );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<string>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.SqliteType.Should().Be( SqliteType.Text );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonSqliteParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<string>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
