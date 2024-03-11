﻿using System.Data;
using LfrlAnvil.Sql.Extensions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.ColumnTypeDefinitionTests;

public class MySqlColumnTypeDefinitionDateTimeOffsetTests : TestsBase
{
    private readonly MySqlColumnTypeDefinitionProvider _provider =
        new MySqlColumnTypeDefinitionProvider( new MySqlColumnTypeDefinitionProviderBuilder() );

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

    [Theory]
    [InlineData( "1970-01-01 00:00:00.0000000+00:00" )]
    [InlineData( "2023-04-09 16:58:43.1234567+11:30" )]
    [InlineData( "2022-11-23 07:06:05.0000001-06:15" )]
    public void TryToParameterValue_ShouldReturnCorrectResult(string dt)
    {
        var value = DateTimeOffset.Parse( dt );
        var sut = _provider.GetByType<DateTimeOffset>();
        var result = sut.TryToParameterValue( value );
        result.Should().Be( dt );
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfDateTimeOffsetType()
    {
        var sut = _provider.GetByType<DateTimeOffset>();
        var result = sut.TryToParameterValue( string.Empty );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateMySqlParameterProperties(bool isNullable)
    {
        var parameter = new MySqlParameter();
        var sut = _provider.GetByType<DateTimeOffset>();

        sut.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.MySqlDbType.Should().Be( MySqlDbType.String );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonMySqlParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<DateTimeOffset>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.Should().Be( sut.DataType.DbType );
    }
}