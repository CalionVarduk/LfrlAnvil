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

        using ( new AssertionScope() )
        {
            sut.DataType.Should().BeSameAs( SqlDataTypeMock.Text );
            sut.DefaultValue.Value.Should().BeSameAs( defaultValue );
            sut.RuntimeType.Should().Be<string>();
            (( ISqlColumnTypeDefinition )sut).DataType.Should().BeSameAs( sut.DataType );
            (( ISqlColumnTypeDefinition )sut).DefaultValue.Should().BeSameAs( sut.DefaultValue );
            (( ISqlColumnTypeDefinition )sut).OutputMapping.Should().BeSameAs( sut.OutputMapping );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, "foo" );
        var result = sut.ToString();
        result.Should().Be( "System.String <=> STRING, DefaultValue: [\"foo\" : System.String]" );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnToDbLiteralResult_WhenValueIsOfCorrectType()
    {
        var value = "foo";
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.TryToDbLiteral( value );
        result.Should().BeSameAs( value );
    }

    [Fact]
    public void TryToDbLiteral_ShouldReturnNull_WhenValueIsNotOfCorrectType()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.TryToDbLiteral( new object() );
        result.Should().BeNull();
    }

    [Fact]
    public void SetParameterInfo_ShouldOnlySetDbType_WhenParameterTypeIsIncompatible()
    {
        var parameter = Substitute.For<IDbDataParameter>();
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );

        (( ISqlColumnTypeDefinition )sut).SetParameterInfo( parameter, Fixture.Create<bool>() );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.IsNullable.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void SetParameterInfo_ShouldInvokeTypedSetParameterInfo_WhenParameterTypeIsCompatible(bool isNullable)
    {
        var parameter = new DbParameterMock();
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );

        (( ISqlColumnTypeDefinition )sut).SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            parameter.DbType.Should().Be( sut.DataType.DbType );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Fact]
    public void TryToNullableParameterValue_ShouldReturnTryToParameterValueResult_WhenValueIsNotNullAndIsOfCorrectType()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.TryToNullableParameterValue( "foo" );
        result.Should().Be( "foo" );
    }

    [Fact]
    public void TryToNullableParameterValue_ShouldReturnTryToParameterValueNullResult_WhenIsNotNullAndNotOfCorrectType()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.TryToNullableParameterValue( new object() );
        result.Should().BeNull();
    }

    [Fact]
    public void TryToNullableParameterValue_ShouldReturnDbNull_WhenValueIsNull()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.TryToNullableParameterValue( null );
        result.Should().BeSameAs( DBNull.Value );
    }

    [Fact]
    public void ToNullableParameterValue_ForRefType_ShouldReturnToParameterValueResult_WhenValueIsNotNull()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.ToNullableParameterValue( "foo" );
        result.Should().Be( "foo" );
    }

    [Fact]
    public void ToNullableParameterValue_ForRefType_ShouldReturnDbNull_WhenValueIsNull()
    {
        var sut = new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty );
        var result = sut.ToNullableParameterValue( null );
        result.Should().BeSameAs( DBNull.Value );
    }

    [Fact]
    public void ToNullableParameterValue_ForValueType_ShouldReturnToParameterValueResult_WhenValueIsNotNull()
    {
        var sut = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var result = sut.ToNullableParameterValue( 1 );
        result.Should().Be( 1 );
    }

    [Fact]
    public void ToNullableParameterValue_ForValueType_ShouldReturnDbNull_WhenValueIsNull()
    {
        var sut = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var result = sut.ToNullableParameterValue( null );
        result.Should().BeSameAs( DBNull.Value );
    }

    [Fact]
    public void ToDbLiteral_ForEnumType_ShouldInvokeBaseTypeDefinition()
    {
        var @base = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var sut = new SqlColumnTypeEnumDefinitionMock<TestEnum, int>( @base );

        var result = sut.ToDbLiteral( TestEnum.B );

        result.Should().Be( "1" );
    }

    [Fact]
    public void ToParameterValue_ForEnumType_ShouldInvokeBaseTypeDefinition()
    {
        var @base = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var sut = new SqlColumnTypeEnumDefinitionMock<TestEnum, int>( @base );

        var result = sut.ToParameterValue( TestEnum.B );

        result.Should().Be( 1 );
    }

    public enum TestEnum
    {
        A = 0,
        B = 1
    };
}
