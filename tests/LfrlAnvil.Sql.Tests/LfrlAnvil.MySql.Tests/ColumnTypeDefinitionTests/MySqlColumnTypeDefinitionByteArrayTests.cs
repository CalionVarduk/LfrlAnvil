﻿using System.Data;
using LfrlAnvil.Sql.Extensions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.ColumnTypeDefinitionTests;

public class MySqlColumnTypeDefinitionByteArrayTests : TestsBase
{
    private readonly MySqlColumnTypeDefinitionProvider _provider =
        new MySqlColumnTypeDefinitionProvider( new MySqlColumnTypeDefinitionProviderBuilder() );

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsEmpty()
    {
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToDbLiteral( Array.Empty<byte>() );
        result.Should().Be( "X''" );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsNotEmpty()
    {
        var value = new byte[] { 0, 10, 21, 31, 42, 58, 73, 89, 104, 129, 155, 181, 206, 233, 255 };
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToDbLiteral( value );
        result.Should().Be( "X'000A151F2A3A495968819BB5CEE9FF'" );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfByteArrayType()
    {
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.Should().BeNull();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsEmpty()
    {
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToParameterValue( Array.Empty<byte>() );
        result.Should().BeSameAs( Array.Empty<byte>() );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsNotEmpty()
    {
        var value = new byte[] { 0, 10, 21, 31, 42, 58, 73, 89, 104, 129, 155, 181, 206, 233, 255 };
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToParameterValue( value );
        result.Should().BeSameAs( value );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfByteArrayType()
    {
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateMySqlParameterProperties(bool isNullable)
    {
        var parameter = new MySqlParameter();
        var sut = _provider.GetByType<byte[]>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.MySqlDbType.Should().Be( MySqlDbType.LongBlob );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonMySqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<byte[]>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}
