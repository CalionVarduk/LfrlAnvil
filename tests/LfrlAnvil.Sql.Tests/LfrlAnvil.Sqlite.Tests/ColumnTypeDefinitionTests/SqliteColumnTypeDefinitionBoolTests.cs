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
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfBoolType()
    {
        var sut = _provider.GetByType<bool>();
        var result = sut.TryToDbLiteral( 0L );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( true, 1L )]
    [InlineData( false, 0L )]
    public void TryToParameterValue_ShouldReturnCorrectResult(bool value, long expectedValue)
    {
        var sut = _provider.GetByType<bool>();
        var result = sut.TryToParameterValue( value );
        result.TestEquals( expectedValue ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfBoolType()
    {
        var sut = _provider.GetByType<bool>();
        var result = sut.TryToParameterValue( 0L );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<bool>();

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
        var sut = _provider.GetByType<bool>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.TestEquals( sut.DataType.DbType ).Go();
    }
}
