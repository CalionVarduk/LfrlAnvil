using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionDoubleTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 123.625, "123.625" )]
    [InlineData( 123, "123.0" )]
    [InlineData( 0, "0.0" )]
    [InlineData( -123.625, "-123.625" )]
    [InlineData( -123, "-123.0" )]
    [InlineData( double.Epsilon, "4.9406564584124654E-324" )]
    [InlineData( 1234567890987654321, "1.2345678909876544E+18" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(double value, string expected)
    {
        var sut = _provider.GetByType<double>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfDoubleType()
    {
        var sut = _provider.GetByType<double>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( 123.625 )]
    [InlineData( 123 )]
    [InlineData( 0 )]
    [InlineData( -123.625 )]
    [InlineData( -123 )]
    [InlineData( double.Epsilon )]
    [InlineData( 1234567890987654321 )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(double value)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<double>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Double );
            parameter.Value.Should().Be( value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfDoubleType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<double>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }
}
