using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionDoubleTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( 123.625, "123.625" )]
    [InlineData( 123, "123.0" )]
    [InlineData( 0, "0.0" )]
    [InlineData( -123.625, "-123.625" )]
    [InlineData( -123, "-123.0" )]
    [InlineData( double.Epsilon, "4.9406564584124654E-324" )]
    [InlineData( 1234567890987654321, "1.2345678909876544E+18" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(double value, string expected)
    {
        var sut = _provider.GetByType<double>();
        var result = sut.TryToDbLiteral( value );
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfDoubleType()
    {
        var sut = _provider.GetByType<double>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.TestNull().Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult()
    {
        var sut = _provider.GetByType<double>();
        var result = sut.TryToParameterValue( 123.625 );
        result.TestEquals( 123.625 ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfDoubleType()
    {
        var sut = _provider.GetByType<double>();
        var result = sut.TryToParameterValue( string.Empty );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<double>();

        sut.SetParameterInfo( parameter, isNullable );

        Assertion.All(
                parameter.DbType.TestEquals( sut.DataType.DbType ),
                parameter.SqliteType.TestEquals( SqliteType.Real ),
                parameter.IsNullable.TestEquals( isNullable ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonSqliteParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<double>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.TestEquals( sut.DataType.DbType ).Go();
    }
}
