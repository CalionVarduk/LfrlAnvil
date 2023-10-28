using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionDateOnlyTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( "1970-01-01" )]
    [InlineData( "2023-04-09" )]
    [InlineData( "2022-11-23" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dt)
    {
        var value = DateOnly.Parse( dt );
        var expected = $"'{dt}'";
        var sut = _provider.GetByType<DateOnly>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfDateOnlyType()
    {
        var sut = _provider.GetByType<DateOnly>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( "1970-01-01" )]
    [InlineData( "2023-04-09" )]
    [InlineData( "2022-11-23" )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(string dt)
    {
        var value = DateOnly.Parse( dt );
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<DateOnly>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.String );
            parameter.Value.Should().Be( dt );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfDateOnlyType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<DateOnly>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }

    [Fact]
    public void SetNullParameter_ShouldUpdateParameterCorrectly()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<DateOnly>();

        sut.SetNullParameter( parameter );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.String );
            parameter.Value.Should().BeSameAs( DBNull.Value );
        }
    }
}
