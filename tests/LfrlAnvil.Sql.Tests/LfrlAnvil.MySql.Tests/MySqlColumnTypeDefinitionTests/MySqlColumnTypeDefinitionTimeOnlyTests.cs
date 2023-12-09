using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.MySqlColumnTypeDefinitionTests;

public class MySqlColumnTypeDefinitionTimeOnlyTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new MySqlColumnTypeDefinitionProvider( new MySqlDataTypeProvider() );

    [Theory]
    [InlineData( "00:00:00.0000000" )]
    [InlineData( "16:58:43.1234567" )]
    [InlineData( "07:06:05.0000001" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dt)
    {
        var value = TimeOnly.Parse( dt );
        var expected = $"TIME'{dt[..^1]}'";
        var sut = _provider.GetByType<TimeOnly>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfTimeOnlyType()
    {
        var sut = _provider.GetByType<TimeOnly>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
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
        result.Should().Be( value );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfTimeOnlyType()
    {
        var sut = _provider.GetByType<TimeOnly>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateMySqlParameterProperties(bool isNullable)
    {
        var parameter = new MySqlParameter();
        var sut = _provider.GetByType<TimeOnly>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.MySqlDbType.Should().Be( MySqlDbType.Time );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonMySqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<TimeOnly>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
