using System.Data;
using LfrlAnvil.Sql.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionTimeSpanTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    [InlineData( -1234567, "-1234567" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(long ticks, string expected)
    {
        var value = TimeSpan.FromTicks( ticks );
        var sut = _provider.GetByType<TimeSpan>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfTimeSpanType()
    {
        var sut = _provider.GetByType<TimeSpan>();
        var result = sut.TryToDbLiteral( 0L );
        result.Should().BeNull();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult()
    {
        var value = TimeSpan.FromTicks( 1234567 );
        var sut = _provider.GetByType<TimeSpan>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( 1234567L );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfTimeSpanType()
    {
        var sut = _provider.GetByType<TimeSpan>();
        var result = sut.TryToParameterValue( 0L );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties(bool isNullable)
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<TimeSpan>();

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
        var sut = _provider.GetByType<TimeSpan>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
