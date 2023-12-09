using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.MySqlColumnTypeDefinitionTests;

public class MySqlColumnTypeDefinitionGuidTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new MySqlColumnTypeDefinitionProvider( new MySqlDataTypeProvider() );

    [Theory]
    [InlineData( "00000000-0000-0000-0000-000000000000", "X'00000000000000000000000000000000'" )]
    [InlineData( "DE4E2141-D9C0-48E3-B3E1-B783C99CF921", "X'41214EDEC0D9E348B3E1B783C99CF921'" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string guid, string expected)
    {
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
        result.Should().BeEquivalentTo( value.ToByteArray() );
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
    public void SetParameterInfo_ShouldUpdateMySqlParameterProperties(bool isNullable)
    {
        var parameter = new MySqlParameter();
        var sut = _provider.GetByType<Guid>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.MySqlDbType.Should().Be( MySqlDbType.Binary );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonMySqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<Guid>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
