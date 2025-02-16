using System.Data;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests;

public class SqlColumnTypeDefinitionTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateCorrectTypeDefinition()
    {
        var defaultValue = Fixture.Create<string>();
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, defaultValue );

        Assertion.All(
                sut.DataType.TestRefEquals( SqlDataTypeMock.Text ),
                sut.DefaultValue.Value.TestRefEquals( defaultValue ),
                sut.RuntimeType.TestEquals( typeof( string ) ),
                (( ISqlColumnTypeDefinition )sut).DataType.TestRefEquals( sut.DataType ),
                (( ISqlColumnTypeDefinition )sut).DefaultValue.TestRefEquals( sut.DefaultValue ),
                (( ISqlColumnTypeDefinition )sut).OutputMapping.TestRefEquals( sut.OutputMapping ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, "foo" );
        var result = sut.ToString();
        result.TestEquals( "System.String <=> STRING, DefaultValue: [\"foo\" : System.String]" ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnToDbLiteralResult_WhenValueIsOfCorrectType()
    {
        var value = "foo";
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.TryToDbLiteral( value );
        result.TestRefEquals( value ).Go();
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfCorrectType()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.TryToDbLiteral( new object() );
        result.TestNull().Go();
    }

    [Fact]
    public void SetParameterInfo_ShouldOnlySetDbType_WhenParameterTypeIsIncompatible()
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );

        (( ISqlColumnTypeDefinition )sut).SetParameterInfo( parameter, Fixture.Create<bool>() );

        Assertion.All(
                parameter.DbType.TestEquals( sut.DataType.DbType ),
                parameter.IsNullable.TestFalse() )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldInvokeTypedSetParameterInfo_WhenParameterTypeIsCompatible(bool isNullable)
    {
        var parameter = new DbParameterMock();
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );

        (( ISqlColumnTypeDefinition )sut).SetParameterInfo( parameter, isNullable );

        Assertion.All(
                parameter.DbType.TestEquals( sut.DataType.DbType ),
                parameter.IsNullable.TestEquals( isNullable ) )
            .Go();
    }

    [Fact]
    public void TryToNullableParameterValue_ShouldReturnTryToParameterValueResult_WhenValueIsNotNullAndIsOfCorrectType()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.TryToNullableParameterValue( "foo" );
        result.TestEquals( "foo" ).Go();
    }

    [Fact]
    public void TryToNullableParameterValue_ShouldReturnTryToParameterValueNullResult_WhenIsNotNullAndNotOfCorrectType()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.TryToNullableParameterValue( new object() );
        result.TestNull().Go();
    }

    [Fact]
    public void TryToNullableParameterValue_ShouldReturnDbNull_WhenValueIsNull()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.TryToNullableParameterValue( null );
        result.TestRefEquals( DBNull.Value ).Go();
    }

    [Fact]
    public void ToNullableParameterValue_ForRefType_ShouldReturnToParameterValueResult_WhenValueIsNotNull()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.ToNullableParameterValue( "foo" );
        result.TestEquals( "foo" ).Go();
    }

    [Fact]
    public void ToNullableParameterValue_ForRefType_ShouldReturnDbNull_WhenValueIsNull()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.ToNullableParameterValue( null );
        result.TestRefEquals( DBNull.Value ).Go();
    }

    [Fact]
    public void ToNullableParameterValue_ForValueType_ShouldReturnToParameterValueResult_WhenValueIsNotNull()
    {
        var sut = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var result = sut.ToNullableParameterValue( 1 );
        result.TestEquals( 1 ).Go();
    }

    [Fact]
    public void ToNullableParameterValue_ForValueType_ShouldReturnDbNull_WhenValueIsNull()
    {
        var sut = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var result = sut.ToNullableParameterValue( null );
        result.TestRefEquals( DBNull.Value ).Go();
    }

    [Fact]
    public void ToDbLiteral_ForEnumType_ShouldInvokeBaseTypeDefinition()
    {
        var @base = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var sut = new SqlColumnTypeEnumDefinitionMock<TestEnum, int>( @base );

        var result = sut.ToDbLiteral( TestEnum.B );

        result.TestEquals( "1" ).Go();
    }

    [Fact]
    public void ToParameterValue_ForEnumType_ShouldInvokeBaseTypeDefinition()
    {
        var @base = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var sut = new SqlColumnTypeEnumDefinitionMock<TestEnum, int>( @base );

        var result = sut.ToParameterValue( TestEnum.B );

        result.TestEquals( 1 ).Go();
    }

    public enum TestEnum
    {
        A = 0,
        B = 1
    };
}
