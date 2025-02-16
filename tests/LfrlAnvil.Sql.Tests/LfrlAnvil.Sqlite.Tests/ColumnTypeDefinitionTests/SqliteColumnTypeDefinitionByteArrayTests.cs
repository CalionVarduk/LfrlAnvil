using System.Data;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionByteArrayTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsEmpty()
    {
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToDbLiteral( Array.Empty<byte>() );
        result.TestEquals( "X''" ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsNotEmpty()
    {
        var value = new byte[] { 0, 10, 21, 31, 42, 58, 73, 89, 104, 129, 155, 181, 206, 233, 255 };
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToDbLiteral( value );
        result.TestEquals( "X'000A151F2A3A495968819BB5CEE9FF'" ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfByteArrayType()
    {
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToDbLiteral( string.Empty );
        result.TestNull().Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsEmpty()
    {
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToParameterValue( Array.Empty<byte>() );
        result.TestRefEquals( Array.Empty<byte>() ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsNotEmpty()
    {
        var value = new byte[] { 0, 10, 21, 31, 42, 58, 73, 89, 104, 129, 155, 181, 206, 233, 255 };
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToParameterValue( value );
        result.TestRefEquals( value ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnNull_WhenValueIsNotOfByteArrayType()
    {
        var sut = _provider.GetByType<byte[]>();
        var result = sut.TryToParameterValue( string.Empty );
        result.TestNull().Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties(bool isNullable)
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<byte[]>();

        sut.SetParameterInfo( parameter, isNullable );

        Assertion.All(
                parameter.DbType.TestEquals( sut.DataType.DbType ),
                parameter.SqliteType.TestEquals( SqliteType.Blob ),
                parameter.IsNullable.TestEquals( isNullable ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonSqliteParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<byte[]>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.TestEquals( sut.DataType.DbType ).Go();
    }
}
