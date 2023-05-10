﻿using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionDateTimeOffsetTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( "1970-01-01 00:00:00.0000000+00:00" )]
    [InlineData( "2023-04-09 16:58:43.1234567+11:30" )]
    [InlineData( "2022-11-23 07:06:05.0000001-06:15" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(string dt)
    {
        var value = DateTimeOffset.Parse( dt );
        var expected = $"'{dt}'";
        var sut = _provider.GetByType<DateTimeOffset>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfDateTimeOffsetType()
    {
        var sut = _provider.GetByType<DateTimeOffset>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }
}