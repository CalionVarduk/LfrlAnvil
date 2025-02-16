using System.Data;
using LfrlAnvil.Sql.Extensions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.ColumnTypeDefinitionTests;

public class MySqlColumnTypeDefinitionEnumTests : TestsBase
{
    private readonly MySqlColumnTypeDefinitionProvider _provider =
        new MySqlColumnTypeDefinitionProvider( new MySqlColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( Values.A, "-10" )]
    [InlineData( Values.B, "0" )]
    [InlineData( Values.C, "123" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(Values value, string expected)
    {
        var sut = _provider.GetByType<Values>();
        var result = sut.TryToDbLiteral( value );
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfEnumType()
    {
        var sut = _provider.GetByType<Values>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( Values.A, -10 )]
    [InlineData( Values.B, 0 )]
    [InlineData( Values.C, 123 )]
    public void TryToParameterValue_ShouldReturnCorrectResult(Values value, long expected)
    {
        var sut = _provider.GetByType<Values>();
        var result = sut.TryToParameterValue( value );
        result.TestEquals( ( sbyte )expected ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfEnumType()
    {
        var sut = _provider.GetByType<Values>();
        var result = sut.TryToParameterValue( string.Empty );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateMySqlParameterProperties(bool isNullable)
    {
        var parameter = new MySqlParameter();
        var sut = _provider.GetByType<Values>();

        sut.SetParameterInfo( parameter, isNullable );

        Assertion.All(
                parameter.DbType.TestEquals( sut.DataType.DbType ),
                parameter.MySqlDbType.TestEquals( MySqlDbType.Byte ),
                parameter.IsNullable.TestEquals( isNullable ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonMySqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<Values>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.TestEquals( sut.DataType.DbType ).Go();
    }

    public enum Values : sbyte
    {
        A = -10,
        B = 0,
        C = 123
    }
}
