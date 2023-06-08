using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class SqlExpressionTypeTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeAnObjectType()
    {
        var sut = default( SqlExpressionType );

        using ( new AssertionScope() )
        {
            sut.BaseType.Should().Be( typeof( object ) );
            sut.FullType.Should().Be( typeof( object ) );
            sut.IsNullable.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( typeof( int ), false, typeof( int ), typeof( int ), false )]
    [InlineData( typeof( int ), true, typeof( int ), typeof( int? ), true )]
    [InlineData( typeof( int? ), false, typeof( int ), typeof( int? ), true )]
    [InlineData( typeof( int? ), true, typeof( int ), typeof( int? ), true )]
    [InlineData( typeof( string ), false, typeof( string ), typeof( string ), false )]
    [InlineData( typeof( string ), true, typeof( string ), typeof( string ), true )]
    [InlineData( typeof( DBNull ), false, typeof( DBNull ), typeof( DBNull ), false )]
    [InlineData( typeof( DBNull ), true, typeof( DBNull ), typeof( DBNull ), false )]
    public void Create_ShouldReturnCorrectType(
        Type type,
        bool isNullable,
        Type expectedBaseType,
        Type expectedFullType,
        bool expectedIsNullable)
    {
        var sut = SqlExpressionType.Create( type, isNullable );

        using ( new AssertionScope() )
        {
            sut.BaseType.Should().Be( expectedBaseType );
            sut.FullType.Should().Be( expectedFullType );
            sut.IsNullable.Should().Be( expectedIsNullable );
        }
    }

    [Theory]
    [InlineData( false, typeof( int ) )]
    [InlineData( true, typeof( int? ) )]
    public void Create_Generic_ShouldReturnCorrectTypeForValueType(bool isNullable, Type expectedFullType)
    {
        var sut = SqlExpressionType.Create<int>( isNullable );

        using ( new AssertionScope() )
        {
            sut.BaseType.Should().Be( typeof( int ) );
            sut.FullType.Should().Be( expectedFullType );
            sut.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void Create_Generic_ShouldReturnCorrectTypeForRefType(bool isNullable)
    {
        var sut = SqlExpressionType.Create<string>( isNullable );

        using ( new AssertionScope() )
        {
            sut.BaseType.Should().Be( typeof( string ) );
            sut.FullType.Should().Be( typeof( string ) );
            sut.IsNullable.Should().Be( isNullable );
        }
    }

    [Fact]
    public void GetCommonType_ShouldReturnNull_WhenBothTypesAreNull()
    {
        var a = (SqlExpressionType?)null;
        var b = (SqlExpressionType?)null;

        var result = SqlExpressionType.GetCommonType( a, b );

        result.Should().BeNull();
    }

    [Fact]
    public void GetCommonType_ShouldReturnNull_WhenFirstTypeIsNull()
    {
        var a = (SqlExpressionType?)null;
        var b = SqlExpressionType.Create<int>();

        var result = SqlExpressionType.GetCommonType( a, b );

        result.Should().BeNull();
    }

    [Fact]
    public void GetCommonType_ShouldReturnNull_WhenSecondTypeIsNull()
    {
        var a = SqlExpressionType.Create<int>();
        var b = (SqlExpressionType?)null;

        var result = SqlExpressionType.GetCommonType( a, b );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void GetCommonType_ShouldReturnOneOfTheParameters_WhenBothTypesAreTheSame(bool isNullable)
    {
        var a = SqlExpressionType.Create<int>( isNullable );
        var b = SqlExpressionType.Create<int>( isNullable );

        var result = SqlExpressionType.GetCommonType( a, b );

        result.Should().Be( SqlExpressionType.Create<int>( isNullable ) );
    }

    [Fact]
    public void GetCommonType_ShouldReturnNullableParameter_WhenBaseTypesAreTheSameAndFirstIsRequiredAndSecondIsNullable()
    {
        var a = SqlExpressionType.Create<int>();
        var b = SqlExpressionType.Create<int>( isNullable: true );

        var result = SqlExpressionType.GetCommonType( a, b );

        result.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
    }

    [Fact]
    public void GetCommonType_ShouldReturnNullableParameter_WhenBaseTypesAreTheSameAndFirstIsNullableAndSecondIsRequired()
    {
        var a = SqlExpressionType.Create<int>( isNullable: true );
        var b = SqlExpressionType.Create<int>();

        var result = SqlExpressionType.GetCommonType( a, b );

        result.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void GetCommonType_ShouldReturnNullableSecondParameter_WhenFirstIsDBNull(bool isSecondNullable)
    {
        var a = SqlExpressionType.Create<DBNull>();
        var b = SqlExpressionType.Create<int>( isNullable: isSecondNullable );

        var result = SqlExpressionType.GetCommonType( a, b );

        result.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void GetCommonType_ShouldReturnNullableFirstParameter_WhenSecondIsDBNull(bool isFirstNullable)
    {
        var a = SqlExpressionType.Create<int>( isNullable: isFirstNullable );
        var b = SqlExpressionType.Create<DBNull>();

        var result = SqlExpressionType.GetCommonType( a, b );

        result.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
    }

    [Theory]
    [InlineData( typeof( string ), false, typeof( int ), false )]
    [InlineData( typeof( string ), false, typeof( int ), true )]
    [InlineData( typeof( string ), true, typeof( int ), false )]
    [InlineData( typeof( string ), true, typeof( int ), true )]
    public void GetCommonType_ShouldThrowSqlNodeException_WhenTypesAreIncompatible(
        Type aBaseType,
        bool aIsNullable,
        Type bBaseType,
        bool bIsNullable)
    {
        var a = SqlExpressionType.Create( aBaseType, aIsNullable );
        var b = SqlExpressionType.Create( bBaseType, bIsNullable );

        var action = Lambda.Of( () => SqlExpressionType.GetCommonType( a, b ) );

        action.Should().ThrowExactly<SqlNodeException>();
    }

    [Fact]
    public void HaveCommonType_ShouldReturnTrue_WhenBothTypesAreNull()
    {
        var a = (SqlExpressionType?)null;
        var b = (SqlExpressionType?)null;

        var result = SqlExpressionType.HaveCommonType( a, b );

        result.Should().BeTrue();
    }

    [Fact]
    public void HaveCommonType_ShouldReturnTrue_WhenFirstTypeIsNull()
    {
        var a = (SqlExpressionType?)null;
        var b = SqlExpressionType.Create<int>();

        var result = SqlExpressionType.HaveCommonType( a, b );

        result.Should().BeTrue();
    }

    [Fact]
    public void HaveCommonType_ShouldReturnTrue_WhenSecondTypeIsNull()
    {
        var a = SqlExpressionType.Create<int>();
        var b = (SqlExpressionType?)null;

        var result = SqlExpressionType.HaveCommonType( a, b );

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData( typeof( int ), false, typeof( int ), false, true )]
    [InlineData( typeof( int ), false, typeof( int ), true, true )]
    [InlineData( typeof( int ), true, typeof( int ), false, true )]
    [InlineData( typeof( int ), true, typeof( int ), true, true )]
    [InlineData( typeof( int ), false, typeof( DBNull ), false, true )]
    [InlineData( typeof( int ), true, typeof( DBNull ), false, true )]
    [InlineData( typeof( DBNull ), false, typeof( int ), false, true )]
    [InlineData( typeof( DBNull ), false, typeof( int ), true, true )]
    [InlineData( typeof( string ), false, typeof( int ), false, false )]
    [InlineData( typeof( string ), false, typeof( int ), true, false )]
    [InlineData( typeof( string ), true, typeof( int ), false, false )]
    [InlineData( typeof( string ), true, typeof( int ), true, false )]
    public void HaveCommonType_ShouldReturnTrue_WhenBothTypesHaveTheSameBaseTypeOrAreDBNull(
        Type aBaseType,
        bool aIsNullable,
        Type bBaseType,
        bool bIsNullable,
        bool expected)
    {
        var a = SqlExpressionType.Create( aBaseType, aIsNullable );
        var b = SqlExpressionType.Create( bBaseType, bIsNullable );

        var result = SqlExpressionType.HaveCommonType( a, b );

        result.Should().Be( expected );
    }

    [Fact]
    public void MakeNullable_ShouldReturnSelf_WhenTypeIsAlreadyNullable()
    {
        var sut = SqlExpressionType.Create<int>( isNullable: true );
        var result = sut.MakeNullable();
        result.Should().Be( sut );
    }

    [Fact]
    public void MakeNullable_ShouldReturnNullableType_WhenTypeIsNotNullable()
    {
        var sut = SqlExpressionType.Create<int>();
        var result = sut.MakeNullable();
        result.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
    }

    [Fact]
    public void MakeRequired_ShouldReturnSelf_WhenTypeIsAlreadyNotNullable()
    {
        var sut = SqlExpressionType.Create<int>();
        var result = sut.MakeRequired();
        result.Should().Be( sut );
    }

    [Fact]
    public void MakeRequired_ShouldReturnNonNullableType_WhenTypeIsNullable()
    {
        var sut = SqlExpressionType.Create<int>( isNullable: true );
        var result = sut.MakeRequired();
        result.Should().Be( SqlExpressionType.Create<int>() );
    }

    [Theory]
    [InlineData( false, "System.String" )]
    [InlineData( true, "Nullable<System.String>" )]
    public void ToString_ShouldReturnCorrectResult(bool isNullable, string expected)
    {
        var sut = SqlExpressionType.Create<string>( isNullable );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = SqlExpressionType.Create<int>( isNullable: true );
        var expected = HashCode.Combine( typeof( int ), true );
        var result = sut.GetHashCode();
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( typeof( int ), false, typeof( int ), false, true )]
    [InlineData( typeof( int ), true, typeof( int ), true, true )]
    [InlineData( typeof( int ), false, typeof( int ), true, false )]
    [InlineData( typeof( int ), true, typeof( int ), false, false )]
    [InlineData( typeof( string ), false, typeof( int ), false, false )]
    [InlineData( typeof( string ), true, typeof( int ), false, false )]
    public void EqualityOperator_ShouldReturnCorrectResult(
        Type aBaseType,
        bool aIsNullable,
        Type bBaseType,
        bool bIsNullable,
        bool expected)
    {
        var a = SqlExpressionType.Create( aBaseType, aIsNullable );
        var b = SqlExpressionType.Create( bBaseType, bIsNullable );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( typeof( int ), false, typeof( int ), false, false )]
    [InlineData( typeof( int ), true, typeof( int ), true, false )]
    [InlineData( typeof( int ), false, typeof( int ), true, true )]
    [InlineData( typeof( int ), true, typeof( int ), false, true )]
    [InlineData( typeof( string ), false, typeof( int ), false, true )]
    [InlineData( typeof( string ), true, typeof( int ), false, true )]
    public void InequalityOperator_ShouldReturnCorrectResult(
        Type aBaseType,
        bool aIsNullable,
        Type bBaseType,
        bool bIsNullable,
        bool expected)
    {
        var a = SqlExpressionType.Create( aBaseType, aIsNullable );
        var b = SqlExpressionType.Create( bBaseType, bIsNullable );

        var result = a != b;

        result.Should().Be( expected );
    }
}
