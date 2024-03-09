using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionTimeSpanTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

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

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult()
    {
        var value = TimeSpan.FromTicks( 1234567 );
        var sut = _provider.GetByType<TimeSpan>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( 1234567L );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfTimeSpanType()
    {
        var sut = _provider.GetByType<TimeSpan>();
        var result = sut.TryToParameterValue( 0L );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<TimeSpan>();

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
        var sut = _provider.GetByType<TimeSpan>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
