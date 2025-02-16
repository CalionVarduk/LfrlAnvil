using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionInt8Tests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( 123, "123" )]
    [InlineData( 0, "0" )]
    [InlineData( -123, "-123" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(sbyte value, string expected)
    {
        var sut = _provider.GetByType<sbyte>();
        var result = sut.TryToDbLiteral( value );
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfInt8Type()
    {
        var sut = _provider.GetByType<sbyte>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.TestNull().Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult()
    {
        var sut = _provider.GetByType<sbyte>();
        var result = sut.TryToParameterValue( ( sbyte )123 );
        result.TestEquals( 123L ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfInt8Type()
    {
        var sut = _provider.GetByType<sbyte>();
        var result = sut.TryToParameterValue( string.Empty );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<sbyte>();

        sut.SetParameterInfo( parameter, isNullable );

        Assertion.All(
                parameter.DbType.TestEquals( sut.DataType.DbType ),
                parameter.SqliteType.TestEquals( SqliteType.Integer ),
                parameter.IsNullable.TestEquals( isNullable ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonSqliteParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<sbyte>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.TestEquals( sut.DataType.DbType ).Go();
    }
}
