﻿using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionObjectTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

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
        result.Should().Be( "'\\x7B2D06'::BYTEA" );
    }

    [Fact]
    public void TryToDbLiteral_ShouldThrowArgumentException_WhenValueIsNotOfSupportedType()
    {
        var sut = _provider.GetByType<object>();
        var action = Lambda.Of( () => sut.TryToDbLiteral( new object() ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsInteger()
    {
        var value = 12345L;
        var sut = _provider.GetByType<object>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( value );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsReal()
    {
        var value = 12345.0625;
        var sut = _provider.GetByType<object>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( value );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsText()
    {
        var value = "foo'bar";
        var sut = _provider.GetByType<object>();
        var result = sut.TryToParameterValue( value );
        result.Should().BeSameAs( value );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsBlob()
    {
        var value = new byte[] { 123, 45, 6 };
        var sut = _provider.GetByType<object>();
        var result = sut.TryToParameterValue( value );
        result.Should().BeSameAs( value );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnValue_WhenValueIsNotOfSupportedType()
    {
        var value = new object();
        var sut = _provider.GetByType<object>();
        var result = sut.TryToParameterValue( value );
        result.Should().BeSameAs( value );
    }

    [Fact]
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties_WhenIsNullable()
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<object>();

        sut.SetParameterInfo( parameter, isNullable: true );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.Object );
            parameter.NpgsqlDbType.Should().Be( NpgsqlDbType.Unknown );
            parameter.IsNullable.Should().BeTrue();
        }
    }

    [Fact]
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties_WhenIsNotNullable()
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<object>();

        sut.SetParameterInfo( parameter, isNullable: false );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( DbType.Object );
            parameter.NpgsqlDbType.Should().Be( NpgsqlDbType.Unknown );
            parameter.IsNullable.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonPostgreSqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<object>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
