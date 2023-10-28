using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionTimeOnlyTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( "00:00:00.0000000" )]
    [InlineData( "16:58:43.1234567" )]
    [InlineData( "07:06:05.0000001" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dt)
    {
        var value = TimeOnly.Parse( dt );
        var expected = $"'{dt}'";
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
    public void TrySetParameter_ShouldUpdateParameterCorrectly(string dt)
    {
        var value = TimeOnly.Parse( dt );
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<TimeOnly>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.String );
            parameter.Value.Should().Be( dt );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfTimeOnlyType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<TimeOnly>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }

    [Fact]
    public void SetNullParameter_ShouldUpdateParameterCorrectly()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<TimeOnly>();

        sut.SetNullParameter( parameter );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.String );
            parameter.Value.Should().BeSameAs( DBNull.Value );
        }
    }
}
