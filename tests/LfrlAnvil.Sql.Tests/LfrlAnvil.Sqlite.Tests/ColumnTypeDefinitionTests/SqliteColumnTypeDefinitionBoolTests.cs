using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionBoolTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

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
    public void TryToParameterValue_ShouldReturnCorrectResult(bool value, long expectedValue)
    {
        var sut = _provider.GetByType<bool>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( expectedValue );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfBoolType()
    {
        var sut = _provider.GetByType<bool>();
        var result = sut.TryToParameterValue( 0L );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<bool>();

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
        var sut = _provider.GetByType<bool>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
