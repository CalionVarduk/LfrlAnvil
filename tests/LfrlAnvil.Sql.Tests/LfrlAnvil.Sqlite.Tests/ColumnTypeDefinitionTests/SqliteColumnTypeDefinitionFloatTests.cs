using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionFloatTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( 123.625, "123.625" )]
    [InlineData( 123, "123.0" )]
    [InlineData( 0, "0.0" )]
    [InlineData( -123.625, "-123.625" )]
    [InlineData( -123, "-123.0" )]
    [InlineData( float.Epsilon, "1.4012984643248171E-45" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(float value, string expected)
    {
        var sut = _provider.GetByType<float>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfFloatType()
    {
        var sut = _provider.GetByType<float>();
        var result = sut.TryToDbLiteral( 0.0 );
        result.Should().BeNull();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult()
    {
        var sut = _provider.GetByType<float>();
        var result = sut.TryToParameterValue( 123.625f );
        result.Should().Be( 123.625 );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfFloatType()
    {
        var sut = _provider.GetByType<float>();
        var result = sut.TryToParameterValue( 0.0 );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<float>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.SqliteType.Should().Be( SqliteType.Real );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonSqliteParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<float>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
