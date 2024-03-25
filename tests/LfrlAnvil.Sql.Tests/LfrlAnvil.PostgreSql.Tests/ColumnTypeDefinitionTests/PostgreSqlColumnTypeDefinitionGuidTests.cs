using System.Data;
using LfrlAnvil.Sql.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionGuidTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( "00000000-0000-0000-0000-000000000000" )]
    [InlineData( "DE4E2141-D9C0-48E3-B3E1-B783C99CF921" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string guid)
    {
        var expected = $"'{guid}'".ToLower();
        var value = Guid.Parse( guid );
        var sut = _provider.GetByType<Guid>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfGuidType()
    {
        var sut = _provider.GetByType<Guid>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult()
    {
        var value = Guid.NewGuid();
        var sut = _provider.GetByType<Guid>();
        var result = sut.TryToParameterValue( value );
        result.Should().BeEquivalentTo( value );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfGuidType()
    {
        var sut = _provider.GetByType<Guid>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties(bool isNullable)
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<Guid>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.NpgsqlDbType.Should().Be( NpgsqlDbType.Uuid );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonPostgreSqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<Guid>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
