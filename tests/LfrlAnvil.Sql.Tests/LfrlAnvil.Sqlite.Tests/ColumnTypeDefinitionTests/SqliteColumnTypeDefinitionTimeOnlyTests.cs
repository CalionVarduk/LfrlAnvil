using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionTimeOnlyTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( "00:00:00.0000000" )]
    [InlineData( "16:58:43.1234567" )]
    [InlineData( "07:06:05.0000001" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dt)
    {
        var value = TimeOnly.Parse( dt );
        var expected = $"'{dt}'";
        var sut = _provider.GetByType<TimeOnly>();
        var result = sut.TryToDbLiteral( value );
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfTimeOnlyType()
    {
        var sut = _provider.GetByType<TimeOnly>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( "00:00:00.0000000" )]
    [InlineData( "16:58:43.1234567" )]
    [InlineData( "07:06:05.0000001" )]
    public void TryToParameterValue_ShouldReturnCorrectResult(string dt)
    {
        var value = TimeOnly.Parse( dt );
        var sut = _provider.GetByType<TimeOnly>();
        var result = sut.TryToParameterValue( value );
        result.TestEquals( dt ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfTimeOnlyType()
    {
        var sut = _provider.GetByType<TimeOnly>();
        var result = sut.TryToParameterValue( string.Empty );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<TimeOnly>();

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
        var sut = _provider.GetByType<TimeOnly>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.TestEquals( sut.DataType.DbType ).Go();
    }
}
