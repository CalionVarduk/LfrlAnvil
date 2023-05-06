using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionFloatTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( 123.625, "123.625" )]
    [InlineData( 123, "123.0" )]
    [InlineData( 0, "0.0" )]
    [InlineData( -123.625, "-123.625" )]
    [InlineData( -123, "-123.0" )]
    [InlineData( float.Epsilon, "1.4012984643248171E-45" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(float value, string expected)
    {
        var sut = _provider.GetByType<float>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfFloatType()
    {
        var sut = _provider.GetByType<float>();
        var result = sut.TryToDbLiteral( 0.0 );
        result.Should().BeNull();
    }
}
