using System.Data;
using System.Globalization;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionDecimalTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( "123.625" )]
    [InlineData( "123.0" )]
    [InlineData( "0.0" )]
    [InlineData( "-123.625" )]
    [InlineData( "-123.0" )]
    [InlineData( "1.2345678901234567890123456789" )]
    [InlineData( "-1.2345678901234567890123456789" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dec)
    {
        var value = decimal.Parse( dec, CultureInfo.InvariantCulture );
        var expected = $"'{dec}'";
        var sut = _provider.GetByType<decimal>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfDecimalType()
    {
        var sut = _provider.GetByType<decimal>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( "123.625" )]
    [InlineData( "123.0" )]
    [InlineData( "0.0" )]
    [InlineData( "-123.625" )]
    [InlineData( "-123.0" )]
    [InlineData( "1.2345678901234567890123456789" )]
    [InlineData( "-1.2345678901234567890123456789" )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(string dec)
    {
        var value = decimal.Parse( dec, CultureInfo.InvariantCulture );
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<decimal>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.String );
            parameter.Value.Should().Be( dec );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfDecimalType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<decimal>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }
}
