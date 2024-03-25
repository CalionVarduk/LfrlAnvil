using System.Data;
using LfrlAnvil.Sql.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionBoolTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

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
    [InlineData( true )]
    [InlineData( false )]
    public void TryToParameterValue_ShouldReturnCorrectResult(bool value)
    {
        var sut = _provider.GetByType<bool>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( value );
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
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties(bool isNullable)
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<bool>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.NpgsqlDbType.Should().Be( NpgsqlDbType.Boolean );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonPostgreSqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<bool>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
