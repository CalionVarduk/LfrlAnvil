using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionInt16Tests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( 12345, "12345" )]
    [InlineData( 0, "0" )]
    [InlineData( -12345, "-12345" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(short value, string expected)
    {
        var sut = _provider.GetByType<short>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfInt16Type()
    {
        var sut = _provider.GetByType<short>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult()
    {
        var sut = _provider.GetByType<short>();
        var result = sut.TryToParameterValue( (short)12345 );
        result.Should().Be( 12345L );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfInt16Type()
    {
        var sut = _provider.GetByType<short>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<short>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.SqliteType.Should().Be( SqliteType.Integer );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonSqliteParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<short>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
