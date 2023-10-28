using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionFloatTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

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

    [Theory]
    [InlineData( 123.625 )]
    [InlineData( 123 )]
    [InlineData( 0 )]
    [InlineData( -123.625 )]
    [InlineData( -123 )]
    [InlineData( float.Epsilon )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(float value)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<float>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Double );
            parameter.Value.Should().Be( (double)value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfFloatType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<float>();

        var result = sut.TrySetParameter( parameter, 0.0 );

        result.Should().BeFalse();
    }

    [Fact]
    public void SetNullParameter_ShouldUpdateParameterCorrectly()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<float>();

        sut.SetNullParameter( parameter );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.Double );
            parameter.Value.Should().BeSameAs( DBNull.Value );
        }
    }
}
