using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionUInt64Tests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(ulong value, string expected)
    {
        var sut = _provider.GetByType<ulong>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfUInt64Type()
    {
        var sut = _provider.GetByType<ulong>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Fact]
    public void TryToDbLiteral_ShouldThrowOverflowException_WhenValueIsGreaterThanInt64MaxValue()
    {
        var sut = _provider.GetByType<ulong>();
        var action = Lambda.Of( () => sut.TryToDbLiteral( (ulong)long.MaxValue + 1 ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult()
    {
        var sut = _provider.GetByType<ulong>();
        var result = sut.TryToParameterValue( (ulong)1234567 );
        result.Should().Be( 1234567L );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfUInt64Type()
    {
        var sut = _provider.GetByType<ulong>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Fact]
    public void TryToParameterValue_ShouldThrowOverflowException_WhenValueIsGreaterThanInt64MaxValue()
    {
        var sut = _provider.GetByType<ulong>();
        var action = Lambda.Of( () => sut.TryToParameterValue( (ulong)long.MaxValue + 1 ) );
        action.Should().ThrowExactly<OverflowException>();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties(bool isNullable)
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<ulong>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.NpgsqlDbType.Should().Be( NpgsqlDbType.Bigint );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonPostgreSqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<ulong>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
