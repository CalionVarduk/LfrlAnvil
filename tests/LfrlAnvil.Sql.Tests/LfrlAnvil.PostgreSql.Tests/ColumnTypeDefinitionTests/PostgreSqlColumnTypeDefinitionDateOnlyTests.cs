using System.Data;
using LfrlAnvil.Sql.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionDateOnlyTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( "1970-01-01" )]
    [InlineData( "2023-04-09" )]
    [InlineData( "2022-11-23" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dt)
    {
        var value = DateOnly.Parse( dt );
        var expected = $"DATE '{dt}'";
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
        result.Should().Be( value );
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
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties(bool isNullable)
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<DateOnly>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.NpgsqlDbType.Should().Be( NpgsqlDbType.Date );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonPostgreSqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<DateOnly>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
