using System.Data;
using LfrlAnvil.Sql.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionDateTimeTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( "1970-01-01 00:00:00.0000000" )]
    [InlineData( "2023-04-09 16:58:43.1234567" )]
    [InlineData( "2022-11-23 07:06:05.0000001" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dt)
    {
        var value = DateTime.Parse( dt );
        var expected = $"TIMESTAMP '{dt[..^1]}'";
        var sut = _provider.GetByType<DateTime>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfDateTimeType()
    {
        var sut = _provider.GetByType<DateTime>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( "1970-01-01 00:00:00.0000000" )]
    [InlineData( "2023-04-09 16:58:43.1234567" )]
    [InlineData( "2022-11-23 07:06:05.0000001" )]
    public void TryToParameterValue_ShouldReturnCorrectResult(string dt)
    {
        var value = DateTime.Parse( dt );
        var sut = _provider.GetByType<DateTime>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( value );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfDateTimeType()
    {
        var sut = _provider.GetByType<DateTime>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties(bool isNullable)
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<DateTime>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.NpgsqlDbType.Should().Be( NpgsqlDbType.Timestamp );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonPostgreSqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<DateTime>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
