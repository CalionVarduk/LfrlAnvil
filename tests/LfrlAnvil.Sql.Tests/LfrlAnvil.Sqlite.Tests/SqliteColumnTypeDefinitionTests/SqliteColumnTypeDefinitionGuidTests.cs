using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionGuidTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( "00000000-0000-0000-0000-000000000000", "X'00000000000000000000000000000000'" )]
    [InlineData( "DE4E2141-D9C0-48E3-B3E1-B783C99CF921", "X'41214EDEC0D9E348B3E1B783C99CF921'" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string guid, string expected)
    {
        var value = Guid.Parse( guid );
        var sut = _provider.GetByType<Guid>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfGuidType()
    {
        var sut = _provider.GetByType<Guid>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( "00000000-0000-0000-0000-000000000000" )]
    [InlineData( "DE4E2141-D9C0-48E3-B3E1-B783C99CF921" )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(string guid)
    {
        var value = Guid.Parse( guid );
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<Guid>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Binary );
            parameter.Value.Should().BeEquivalentTo( value.ToByteArray() );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfGuidType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<Guid>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }
}
