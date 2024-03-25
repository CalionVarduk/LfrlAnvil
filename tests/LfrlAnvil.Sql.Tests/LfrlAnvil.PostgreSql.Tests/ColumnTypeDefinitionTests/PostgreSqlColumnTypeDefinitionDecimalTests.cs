using System.Data;
using System.Globalization;
using LfrlAnvil.Sql.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionDecimalTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( "123.625" )]
    [InlineData( "123.0" )]
    [InlineData( "0.0" )]
    [InlineData( "-123.625" )]
    [InlineData( "-123.0" )]
    [InlineData( "1.2345678901234567890123456789" )]
    [InlineData( "-1.2345678901234567890123456789" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dec)
    {
        var value = decimal.Parse( dec, CultureInfo.InvariantCulture );
        var sut = _provider.GetByType<decimal>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( dec );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfDecimalType()
    {
        var sut = _provider.GetByType<decimal>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( "123.625" )]
    [InlineData( "123.0" )]
    [InlineData( "0.0" )]
    [InlineData( "-123.625" )]
    [InlineData( "-123.0" )]
    [InlineData( "1.2345678901234567890123456789" )]
    [InlineData( "-1.2345678901234567890123456789" )]
    public void TryToParameterValue_ShouldReturnCorrectResult(string dec)
    {
        var value = decimal.Parse( dec, CultureInfo.InvariantCulture );
        var sut = _provider.GetByType<decimal>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( value );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfDecimalType()
    {
        var sut = _provider.GetByType<decimal>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties(bool isNullable)
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<decimal>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.NpgsqlDbType.Should().Be( NpgsqlDbType.Numeric );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonPostgreSqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<decimal>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
