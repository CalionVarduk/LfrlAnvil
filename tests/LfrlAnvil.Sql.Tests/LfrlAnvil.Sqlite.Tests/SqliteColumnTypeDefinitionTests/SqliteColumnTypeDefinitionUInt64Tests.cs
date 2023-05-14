using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

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

    [Theory]
    [InlineData( 1234567 )]
    [InlineData( 0 )]
    public void TrySetParameter_ShouldUpdateParameterCorrectly(ulong value)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<ulong>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( (long)value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfUInt64Type()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<ulong>();

        var result = sut.TrySetParameter( parameter, string.Empty );

        result.Should().BeFalse();
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsGreaterThanInt64MaxValue()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<ulong>();

        var result = sut.TrySetParameter( parameter, (ulong)long.MaxValue + 1 );

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData( 1234567 )]
    [InlineData( 0 )]
    public void SetParameter_ShouldUpdateParameterCorrectly(ulong value)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<ulong>();

        sut.SetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( (long)value );
        }
    }

    [Fact]
    public void SetParameter_ShouldThrowOverflowException_WhenValueIsGreaterThanInt64MaxValue()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<ulong>();

        var action = Lambda.Of( () => sut.SetParameter( parameter, (ulong)long.MaxValue + 1 ) );

        action.Should().ThrowExactly<OverflowException>();
    }
}
