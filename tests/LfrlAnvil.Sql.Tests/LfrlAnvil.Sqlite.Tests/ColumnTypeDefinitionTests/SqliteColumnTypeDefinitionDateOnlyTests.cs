using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionDateOnlyTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( "1970-01-01" )]
    [InlineData( "2023-04-09" )]
    [InlineData( "2022-11-23" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dt)
    {
        var value = DateOnly.Parse( dt );
        var expected = $"'{dt}'";
        var sut = _provider.GetByType<DateOnly>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfDateOnlyType()
    {
        var sut = _provider.GetByType<DateOnly>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( "1970-01-01" )]
    [InlineData( "2023-04-09" )]
    [InlineData( "2022-11-23" )]
    public void TryToParameterValue_ShouldReturnCorrectResult(string dt)
    {
        var value = DateOnly.Parse( dt );
        var sut = _provider.GetByType<DateOnly>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( dt );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfDateOnlyType()
    {
        var sut = _provider.GetByType<DateOnly>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<DateOnly>();

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
        var sut = _provider.GetByType<DateOnly>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
