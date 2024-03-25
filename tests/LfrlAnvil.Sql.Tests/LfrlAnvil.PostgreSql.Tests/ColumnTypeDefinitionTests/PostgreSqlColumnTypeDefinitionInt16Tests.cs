﻿using System.Data;
using LfrlAnvil.Sql.Extensions;
using Npgsql;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionInt16Tests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

    [Theory]
    [InlineData( 12345, "12345" )]
    [InlineData( 0, "0" )]
    [InlineData( -12345, "-12345" )]
    public void TryToDbLiteral_ShouldReturnCorrectResult(short value, string expected)
    {
        var sut = _provider.GetByType<short>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( expected );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfInt16Type()
    {
        var sut = _provider.GetByType<short>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult()
    {
        var sut = _provider.GetByType<short>();
        var result = sut.TryToParameterValue( (short)12345 );
        result.Should().Be( 12345 );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfInt16Type()
    {
        var sut = _provider.GetByType<short>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdatePostgreSqlParameterProperties(bool isNullable)
    {
        var parameter = new NpgsqlParameter();
        var sut = _provider.GetByType<short>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.NpgsqlDbType.Should().Be( NpgsqlDbType.Smallint );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonPostgreSqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<short>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
