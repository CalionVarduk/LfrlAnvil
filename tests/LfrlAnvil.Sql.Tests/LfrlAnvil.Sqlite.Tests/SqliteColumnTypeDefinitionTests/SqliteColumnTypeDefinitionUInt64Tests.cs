using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionUInt64Tests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(ulong value, string expected)
    {
        var sut = _provider.GetByType<ulong>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfUInt64Type()
    {
        var sut = _provider.GetByType<ulong>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsGreaterThanInt64MaxValue()
    {
        var sut = _provider.GetByType<ulong>();
        var result = sut.TryToDbLiteral( (ulong)long.MaxValue + 1 );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( 1234567, "1234567" )]
    [InlineData( 0, "0" )]
    public void ToDbLiteral_ShouldReturnCorrectResult(ulong value, string expected)
    {
        var sut = _provider.GetByType<ulong>();
        var result = sut.ToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void ToDbLiteral_ShouldThrowOverflowException_WhenValueIsGreaterThanInt64MaxValue()
    {
        var sut = _provider.GetByType<ulong>();
        var action = Lambda.Of( () => sut.ToDbLiteral( (ulong)long.MaxValue + 1 ) );
        action.Should().ThrowExactly<OverflowException>();
    }
}
