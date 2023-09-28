using System.Data;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionEnumTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Theory]
    [InlineData( WithDefault.A, "-1" )]
    [InlineData( WithDefault.B, "0" )]
    [InlineData( WithDefault.C, "1" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(WithDefault value, string expected)
    {
        var sut = _provider.GetByType<WithDefault>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfEnumType()
    {
        var sut = _provider.GetByType<WithDefault>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( WithDefault.A )]
    [InlineData( WithDefault.B )]
    [InlineData( WithDefault.C )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(WithDefault value)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<WithDefault>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( (long)value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfEnumType()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<WithDefault>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }

    [Fact]
    public void ProviderShouldRegisterEnumTypeOnlyOnce()
    {
        var sut = _provider.GetByType<WithDefault>();
        var result = _provider.GetByType<WithDefault>();
        sut.Should().BeSameAs( result );
    }

    [Fact]
    public void ProviderShouldRegisterEnumTypeWithDefaultValue_WhenZeroLikeValueExists()
    {
        var sut = _provider.GetByType<WithDefault>();
        var result = sut.DefaultValue;
        result.Should().Be( WithDefault.B );
    }

    [Fact]
    public void ProviderShouldRegisterEnumTypeWithDefaultValue_WhenZeroValueDoesNotExist()
    {
        var sut = _provider.GetByType<WithoutDefault>();
        var result = sut.DefaultValue;
        result.Should().Be( WithoutDefault.A );
    }

    [Fact]
    public void ProviderShouldRegisterEnumTypeWithDefaultValue_WhenThereAreNotValues()
    {
        var sut = _provider.GetByType<Empty>();
        var result = sut.DefaultValue;
        result.Should().Be( default( Empty ) );
    }

    public enum Empty { }

    public enum WithoutDefault : short
    {
        A = 5,
        B = 10,
        C = 20
    }

    public enum WithDefault : sbyte
    {
        A = -1,
        B = 0,
        C = 1
    }
}
