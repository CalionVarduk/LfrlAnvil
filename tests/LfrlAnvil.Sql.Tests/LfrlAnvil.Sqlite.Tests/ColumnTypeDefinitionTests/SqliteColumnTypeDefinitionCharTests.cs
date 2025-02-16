using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionCharTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( 'a', "'a'" )]
    [InlineData( '0', "'0'" )]
    [InlineData( 'Z', "'Z'" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(char value, string expected)
    {
        var sut = _provider.GetByType<char>();
        var result = sut.TryToDbLiteral( value );
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfCharType()
    {
        var sut = _provider.GetByType<char>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( 'a', "a" )]
    [InlineData( '0', "0" )]
    [InlineData( 'Z', "Z" )]
    public void TryToParameterValue_ShouldReturnCorrectResult(char value, string expected)
    {
        var sut = _provider.GetByType<char>();
        var result = sut.TryToParameterValue( value );
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfCharType()
    {
        var sut = _provider.GetByType<char>();
        var result = sut.TryToParameterValue( string.Empty );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<char>();

        sut.SetParameterInfo( parameter, isNullable );

        Assertion.All(
                parameter.DbType.TestEquals( sut.DataType.DbType ),
                parameter.SqliteType.TestEquals( SqliteType.Text ),
                parameter.IsNullable.TestEquals( isNullable ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonSqliteParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<char>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.TestEquals( sut.DataType.DbType ).Go();
    }
}
