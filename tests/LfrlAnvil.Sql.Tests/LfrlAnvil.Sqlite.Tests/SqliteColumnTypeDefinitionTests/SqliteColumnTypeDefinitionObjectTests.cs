using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionObjectTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _provider = new SqliteColumnTypeDefinitionProvider();

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsInteger()
    {
        var value = 12345L;
        var sut = _provider.GetByType<object>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( "12345" );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsReal()
    {
        var value = 12345.0625;
        var sut = _provider.GetByType<object>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( "12345.0625" );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsText()
    {
        var value = "foo'bar";
        var sut = _provider.GetByType<object>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( "'foo''bar'" );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsBlob()
    {
        var value = new byte[] { 123, 45, 6 };
        var sut = _provider.GetByType<object>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( "X'7B2D06'" );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfSupportedType()
    {
        var sut = _provider.GetByType<object>();
        var result = sut.TryToDbLiteral( new object() );
        result.Should().BeNull();
    }

    [Fact]
    public void ToDbLiteral_ShouldReturnCorrectResult_WhenValueIsInteger()
    {
        var value = 12345L;
        var sut = _provider.GetByType<object>();
        var result = sut.ToDbLiteral( value );
        result.Should().Be( "12345" );
    }

    [Fact]
    public void ToDbLiteral_ShouldReturnCorrectResult_WhenValueIsReal()
    {
        var value = 12345.0625;
        var sut = _provider.GetByType<object>();
        var result = sut.ToDbLiteral( value );
        result.Should().Be( "12345.0625" );
    }

    [Fact]
    public void ToDbLiteral_ShouldReturnCorrectResult_WhenValueIsText()
    {
        var value = "foo'bar";
        var sut = _provider.GetByType<object>();
        var result = sut.ToDbLiteral( value );
        result.Should().Be( "'foo''bar'" );
    }

    [Fact]
    public void ToDbLiteral_ShouldReturnCorrectResult_WhenValueIsBlob()
    {
        var value = new byte[] { 123, 45, 6 };
        var sut = _provider.GetByType<object>();
        var result = sut.ToDbLiteral( value );
        result.Should().Be( "X'7B2D06'" );
    }

    [Fact]
    public void ToDbLiteral_ShouldThrowArgumentException_WhenValueIsNotOfSupportedType()
    {
        var sut = _provider.GetByType<object>();
        var action = Lambda.Of( () => sut.ToDbLiteral( new object() ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TrySetParameter_ShouldUpdateParameterCorrectly_WhenValueIsInteger()
    {
        var parameter = new SqliteParameter();
        var value = 12345L;
        var sut = _provider.GetByType<object>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldUpdateParameterCorrectly_WhenValueIsReal()
    {
        var parameter = new SqliteParameter();
        var value = 12345.0625;
        var sut = _provider.GetByType<object>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Double );
            parameter.Value.Should().Be( value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldUpdateParameterCorrectly_WhenValueIsText()
    {
        var parameter = new SqliteParameter();
        var value = "foo'bar";
        var sut = _provider.GetByType<object>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.String );
            parameter.Value.Should().Be( value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldUpdateParameterCorrectly_WhenValueIsBlob()
    {
        var parameter = new SqliteParameter();
        var value = new byte[] { 123, 45, 6 };
        var sut = _provider.GetByType<object>();

        var result = sut.TrySetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            parameter.DbType.Should().Be( DbType.Binary );
            parameter.Value.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void TrySetParameter_ShouldReturnFalse_WhenValueIsNotOfSupportedType()
    {
        var parameter = new SqliteParameter();
        var value = new object();
        var sut = _provider.GetByType<object>();

        var result = sut.TrySetParameter( parameter, value );

        result.Should().BeFalse();
    }

    [Fact]
    public void SetParameter_ShouldUpdateParameterCorrectly_WhenValueIsInteger()
    {
        var parameter = new SqliteParameter();
        var value = 12345L;
        var sut = _provider.GetByType<object>();

        sut.SetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.Int64 );
            parameter.Value.Should().Be( value );
        }
    }

    [Fact]
    public void SetParameter_ShouldUpdateParameterCorrectly_WhenValueIsReal()
    {
        var parameter = new SqliteParameter();
        var value = 12345.0625;
        var sut = _provider.GetByType<object>();

        sut.SetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.Double );
            parameter.Value.Should().Be( value );
        }
    }

    [Fact]
    public void SetParameter_ShouldUpdateParameterCorrectly_WhenValueIsText()
    {
        var parameter = new SqliteParameter();
        var value = "foo'bar";
        var sut = _provider.GetByType<object>();

        sut.SetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.String );
            parameter.Value.Should().Be( value );
        }
    }

    [Fact]
    public void SetParameter_ShouldUpdateParameterCorrectly_WhenValueIsBlob()
    {
        var parameter = new SqliteParameter();
        var value = new byte[] { 123, 45, 6 };
        var sut = _provider.GetByType<object>();

        sut.SetParameter( parameter, value );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.Binary );
            parameter.Value.Should().BeSameAs( value );
        }
    }

    [Fact]
    public void SetParameter_ShouldThrowArgumentException_WhenValueIsNotOfSupportedType()
    {
        var parameter = new SqliteParameter();
        var value = new object();
        var sut = _provider.GetByType<object>();

        var action = Lambda.Of( () => sut.SetParameter( parameter, value ) );

        action.Should().ThrowExactly<ArgumentException>();
    }
}
