using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionEnumTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

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
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<Values>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.SqliteType.Should().Be( SqliteType.Integer );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonSqliteParameterDbTypeProperty(bool isNullable)
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
