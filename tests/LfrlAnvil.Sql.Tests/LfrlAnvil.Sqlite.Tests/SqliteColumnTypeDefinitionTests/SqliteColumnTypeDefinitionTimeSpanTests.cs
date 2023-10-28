using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionTimeSpanTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    [InlineData( -1234567, "-1234567" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(long ticks, string expected)
    {
        var value = TimeSpan.FromTicks( ticks );
        var sut = _provider.GetByType<TimeSpan>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfTimeSpanType()
    {
        var sut = _provider.GetByType<TimeSpan>();
        var result = sut.TryToDbLiteral( 0L );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( 1234567 )]
    [InlineData( 0 )]
    [InlineData( -1234567 )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(long ticks)
    {
        var value = TimeSpan.FromTicks( ticks );
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<TimeSpan>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( ticks );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfTimeSpanType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<TimeSpan>();

        var result = sut.TrySetParameter( parameter, 0L );

        result.Should().BeFalse();
    }

    [Fact]
    public void SetNullParameter_ShouldUpdateParameterCorrectly()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<TimeSpan>();

        sut.SetNullParameter( parameter );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().BeSameAs( DBNull.Value );
        }
    }
}
