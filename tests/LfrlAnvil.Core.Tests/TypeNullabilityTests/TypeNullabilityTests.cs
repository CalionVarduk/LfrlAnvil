namespace LfrlAnvil.Tests.TypeNullabilityTests;

public class TypeNullabilityTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeAnObjectType()
    {
        var sut = default( TypeNullability );

        using ( new AssertionScope() )
        {
            sut.UnderlyingType.Should().Be( typeof( object ) );
            sut.ActualType.Should().Be( typeof( object ) );
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
    public void Create_ShouldReturnCorrectType(
        Type type,
        bool isNullable,
        Type expectedBaseType,
        Type expectedFullType,
        bool expectedIsNullable)
    {
        var sut = TypeNullability.Create( type, isNullable );

        using ( new AssertionScope() )
        {
            sut.UnderlyingType.Should().Be( expectedBaseType );
            sut.ActualType.Should().Be( expectedFullType );
            sut.IsNullable.Should().Be( expectedIsNullable );
        }
    }

    [Theory]
    [InlineData( false, typeof( int ) )]
    [InlineData( true, typeof( int? ) )]
    public void Create_Generic_ShouldReturnCorrectTypeForValueType(bool isNullable, Type expectedFullType)
    {
        var sut = TypeNullability.Create<int>( isNullable );

        using ( new AssertionScope() )
        {
            sut.UnderlyingType.Should().Be( typeof( int ) );
            sut.ActualType.Should().Be( expectedFullType );
            sut.IsNullable.Should().Be( isNullable );
        }
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void Create_Generic_ShouldReturnCorrectTypeForRefType(bool isNullable)
    {
        var sut = TypeNullability.Create<string>( isNullable );

        using ( new AssertionScope() )
        {
            sut.UnderlyingType.Should().Be( typeof( string ) );
            sut.ActualType.Should().Be( typeof( string ) );
            sut.IsNullable.Should().Be( isNullable );
        }
    }

    [Fact]
    public void GetCommonType_ShouldReturnNull_WhenBothTypesAreNull()
    {
        var a = ( TypeNullability? )null;
        var b = ( TypeNullability? )null;

        var result = TypeNullability.GetCommonType( a, b );

        result.Should().BeNull();
    }

    [Fact]
    public void GetCommonType_ShouldReturnNull_WhenFirstTypeIsNull()
    {
        var a = ( TypeNullability? )null;
        var b = TypeNullability.Create<int>();

        var result = TypeNullability.GetCommonType( a, b );

        result.Should().BeNull();
    }

    [Fact]
    public void GetCommonType_ShouldReturnNull_WhenSecondTypeIsNull()
    {
        var a = TypeNullability.Create<int>();
        var b = ( TypeNullability? )null;

        var result = TypeNullability.GetCommonType( a, b );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void GetCommonType_ShouldReturnOneOfTheParameters_WhenBothTypesAreTheSame(bool isNullable)
    {
        var a = TypeNullability.Create<int>( isNullable );
        var b = TypeNullability.Create<int>( isNullable );

        var result = TypeNullability.GetCommonType( a, b );

        result.Should().Be( TypeNullability.Create<int>( isNullable ) );
    }

    [Fact]
    public void GetCommonType_ShouldReturnNullableParameter_WhenBaseTypesAreTheSameAndFirstIsRequiredAndSecondIsNullable()
    {
        var a = TypeNullability.Create<int>();
        var b = TypeNullability.Create<int>( isNullable: true );

        var result = TypeNullability.GetCommonType( a, b );

        result.Should().Be( TypeNullability.Create<int>( isNullable: true ) );
    }

    [Fact]
    public void GetCommonType_ShouldReturnNullableParameter_WhenBaseTypesAreTheSameAndFirstIsNullableAndSecondIsRequired()
    {
        var a = TypeNullability.Create<int>( isNullable: true );
        var b = TypeNullability.Create<int>();

        var result = TypeNullability.GetCommonType( a, b );

        result.Should().Be( TypeNullability.Create<int>( isNullable: true ) );
    }

    [Theory]
    [InlineData( typeof( string ), false, typeof( int ), false )]
    [InlineData( typeof( string ), false, typeof( int ), true )]
    [InlineData( typeof( string ), true, typeof( int ), false )]
    [InlineData( typeof( string ), true, typeof( int ), true )]
    public void GetCommonType_ReturnNull_WhenTypesAreIncompatible(Type aBaseType, bool aIsNullable, Type bBaseType, bool bIsNullable)
    {
        var a = TypeNullability.Create( aBaseType, aIsNullable );
        var b = TypeNullability.Create( bBaseType, bIsNullable );

        var result = TypeNullability.GetCommonType( a, b );

        result.Should().BeNull();
    }

    [Fact]
    public void MakeNullable_ShouldReturnSelf_WhenTypeIsAlreadyNullable()
    {
        var sut = TypeNullability.Create<int>( isNullable: true );
        var result = sut.MakeNullable();
        result.Should().Be( sut );
    }

    [Fact]
    public void MakeNullable_ShouldReturnNullableType_WhenTypeIsNotNullable()
    {
        var sut = TypeNullability.Create<int>();
        var result = sut.MakeNullable();
        result.Should().Be( TypeNullability.Create<int>( isNullable: true ) );
    }

    [Fact]
    public void MakeRequired_ShouldReturnSelf_WhenTypeIsAlreadyNotNullable()
    {
        var sut = TypeNullability.Create<int>();
        var result = sut.MakeRequired();
        result.Should().Be( sut );
    }

    [Fact]
    public void MakeRequired_ShouldReturnNonNullableType_WhenTypeIsNullable()
    {
        var sut = TypeNullability.Create<int>( isNullable: true );
        var result = sut.MakeRequired();
        result.Should().Be( TypeNullability.Create<int>() );
    }

    [Theory]
    [InlineData( false, "System.String" )]
    [InlineData( true, "Nullable<System.String>" )]
    public void ToString_ShouldReturnCorrectResult(bool isNullable, string expected)
    {
        var sut = TypeNullability.Create<string>( isNullable );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = TypeNullability.Create<int>( isNullable: true );
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
        var a = TypeNullability.Create( aBaseType, aIsNullable );
        var b = TypeNullability.Create( bBaseType, bIsNullable );

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
        var a = TypeNullability.Create( aBaseType, aIsNullable );
        var b = TypeNullability.Create( bBaseType, bIsNullable );

        var result = a != b;

        result.Should().Be( expected );
    }
}
