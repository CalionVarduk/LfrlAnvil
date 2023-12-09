using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.MySqlColumnTypeDefinitionTests;

public class MySqlColumnTypeDefinitionBoolTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new MySqlColumnTypeDefinitionProvider( new MySqlDataTypeProvider() );

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
    public void SetParameterInfo_ShouldUpdateMySqlParameterProperties(bool isNullable)
    {
        var parameter = new MySqlParameter();
        var sut = _provider.GetByType<bool>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.MySqlDbType.Should().Be( MySqlDbType.Bool );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonMySqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<bool>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
