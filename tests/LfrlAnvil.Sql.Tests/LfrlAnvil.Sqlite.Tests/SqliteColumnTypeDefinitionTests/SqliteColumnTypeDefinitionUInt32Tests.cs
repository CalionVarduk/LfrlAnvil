﻿using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionUInt32Tests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(uint value, string expected)
    {
        var sut = _provider.GetByType<uint>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfUInt32Type()
    {
        var sut = _provider.GetByType<uint>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }
}
