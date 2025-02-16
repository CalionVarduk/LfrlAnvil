using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.ColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionObjectTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProvider _provider =
        new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsInteger()
    {
        var value = 12345L;
        var sut = _provider.GetByType<object>();
        var result = sut.TryToDbLiteral( value );
        result.TestEquals( "12345" ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsReal()
    {
        var value = 12345.0625;
        var sut = _provider.GetByType<object>();
        var result = sut.TryToDbLiteral( value );
        result.TestEquals( "12345.0625" ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsText()
    {
        var value = "foo'bar";
        var sut = _provider.GetByType<object>();
        var result = sut.TryToDbLiteral( value );
        result.TestEquals( "'foo''bar'" ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnCorrectResult_WhenValueIsBlob()
    {
        var value = new byte[] { 123, 45, 6 };
        var sut = _provider.GetByType<object>();
        var result = sut.TryToDbLiteral( value );
        result.TestEquals( "X'7B2D06'" ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldThrowArgumentException_WhenValueIsNotOfSupportedType()
    {
        var sut = _provider.GetByType<object>();
        var action = Lambda.Of( () => sut.TryToDbLiteral( new object() ) );
        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsInteger()
    {
        var value = 12345L;
        var sut = _provider.GetByType<object>();
        var result = sut.TryToParameterValue( value );
        result.TestEquals( value ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsReal()
    {
        var value = 12345.0625;
        var sut = _provider.GetByType<object>();
        var result = sut.TryToParameterValue( value );
        result.TestEquals( value ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsText()
    {
        var value = "foo'bar";
        var sut = _provider.GetByType<object>();
        var result = sut.TryToParameterValue( value );
        result.TestRefEquals( value ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnCorrectResult_WhenValueIsBlob()
    {
        var value = new byte[] { 123, 45, 6 };
        var sut = _provider.GetByType<object>();
        var result = sut.TryToParameterValue( value );
        result.TestRefEquals( value ).Go();
    }

    [Fact]
    public void TryToParameterValue_ShouldReturnValue_WhenValueIsNotOfSupportedType()
    {
        var value = new object();
        var sut = _provider.GetByType<object>();
        var result = sut.TryToParameterValue( value );
        result.TestRefEquals( value ).Go();
    }

    [Fact]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties_WhenIsNullable()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<object>();

        sut.SetParameterInfo( parameter, isNullable: true );

        Assertion.All(
                parameter.DbType.TestEquals( sut.DataType.DbType ),
                parameter.SqliteType.TestEquals( SqliteType.Text ),
                parameter.IsNullable.TestTrue() )
            .Go();
    }

    [Fact]
    public void SetParameterInfo_ShouldUpdateSqliteParameterProperties_WhenIsNotNullable()
    {
        var parameter = new SqliteParameter();
        var sut = _provider.GetByType<object>();

        sut.SetParameterInfo( parameter, isNullable: false );

        Assertion.All(
                parameter.DbType.TestEquals( DbType.String ),
                parameter.SqliteType.TestEquals( SqliteType.Text ),
                parameter.IsNullable.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldUpdateNonSqliteParameterDbTypeProperty(bool isNullable)
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = _provider.GetByType<object>();

        sut.SetParameterInfo( parameter, isNullable );

        parameter.DbType.TestEquals( sut.DataType.DbType ).Go();
    }
}
