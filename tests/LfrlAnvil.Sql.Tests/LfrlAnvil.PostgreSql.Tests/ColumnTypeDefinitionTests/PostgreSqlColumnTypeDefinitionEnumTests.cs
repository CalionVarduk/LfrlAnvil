using System.Data;
using LfrlAnvil.Sql.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionEnumTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( Values.A, "-10" )]
    [InlineData( Values.B, "0" )]
    [InlineData( Values.C, "123" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(Values value, string expected)
    {
        var sut = _provider.GetByType<Values>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfEnumType()
    {
        var sut = _provider.GetByType<Values>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( Values.A, -10 )]
    [InlineData( Values.B, 0 )]
    [InlineData( Values.C, 123 )]
    public void TryToParameterValue_ShouldReturnCorrectResult(Values value, long expected)
    {
        var sut = _provider.GetByType<Values>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfEnumType()
    {
        var sut = _provider.GetByType<Values>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties(bool isNullable)
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<Values>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.NpgsqlDbType.Should().Be( NpgsqlDbType.Smallint );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonPostgreSqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<Values>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }

    public enum Values : sbyte
    {
        A = -10,
        B = 0,
        C = 123
    }
}
