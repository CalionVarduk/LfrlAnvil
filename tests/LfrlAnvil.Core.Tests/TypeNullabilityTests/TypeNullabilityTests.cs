namespace LfrlAnvil.Tests.TypeNullabilityTests;

public class TypeNullabilityTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeAnObjectType()
    {
        var sut = default( TypeNullability );

        Assertion.All(
                sut.UnderlyingType.TestEquals( typeof( object ) ),
                sut.ActualType.TestEquals( typeof( object ) ),
                sut.IsNullable.TestFalse() )
            .Go();
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

        Assertion.All(
                sut.UnderlyingType.TestEquals( expectedBaseType ),
                sut.ActualType.TestEquals( expectedFullType ),
                sut.IsNullable.TestEquals( expectedIsNullable ) )
            .Go();
    }

    [Theory]
    [InlineData( false, typeof( int ) )]
    [InlineData( true, typeof( int? ) )]
    public void Create_Generic_ShouldReturnCorrectTypeForValueType(bool isNullable, Type expectedFullType)
    {
        var sut = TypeNullability.Create<int>( isNullable );

        Assertion.All(
                sut.UnderlyingType.TestEquals( typeof( int ) ),
                sut.ActualType.TestEquals( expectedFullType ),
                sut.IsNullable.TestEquals( isNullable ) )
            .Go();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void Create_Generic_ShouldReturnCorrectTypeForRefType(bool isNullable)
    {
        var sut = TypeNullability.Create<string>( isNullable );

        Assertion.All(
                sut.UnderlyingType.TestEquals( typeof( string ) ),
                sut.ActualType.TestEquals( typeof( string ) ),
                sut.IsNullable.TestEquals( isNullable ) )
            .Go();
    }

    [Fact]
    public void GetCommonType_ShouldReturnNull_WhenBothTypesAreNull()
    {
        var a = ( TypeNullability? )null;
        var b = ( TypeNullability? )null;

        var result = TypeNullability.GetCommonType( a, b );

        result.TestNull().Go();
    }

    [Fact]
    public void GetCommonType_ShouldReturnNull_WhenFirstTypeIsNull()
    {
        var a = ( TypeNullability? )null;
        var b = TypeNullability.Create<int>();

        var result = TypeNullability.GetCommonType( a, b );

        result.TestNull().Go();
    }

    [Fact]
    public void GetCommonType_ShouldReturnNull_WhenSecondTypeIsNull()
    {
        var a = TypeNullability.Create<int>();
        var b = ( TypeNullability? )null;

        var result = TypeNullability.GetCommonType( a, b );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void GetCommonType_ShouldReturnOneOfTheParameters_WhenBothTypesAreTheSame(bool isNullable)
    {
        var a = TypeNullability.Create<int>( isNullable );
        var b = TypeNullability.Create<int>( isNullable );

        var result = TypeNullability.GetCommonType( a, b );

        result.TestEquals( TypeNullability.Create<int>( isNullable ) ).Go();
    }

    [Fact]
    public void GetCommonType_ShouldReturnNullableParameter_WhenBaseTypesAreTheSameAndFirstIsRequiredAndSecondIsNullable()
    {
        var a = TypeNullability.Create<int>();
        var b = TypeNullability.Create<int>( isNullable: true );

        var result = TypeNullability.GetCommonType( a, b );

        result.TestEquals( TypeNullability.Create<int>( isNullable: true ) ).Go();
    }

    [Fact]
    public void GetCommonType_ShouldReturnNullableParameter_WhenBaseTypesAreTheSameAndFirstIsNullableAndSecondIsRequired()
    {
        var a = TypeNullability.Create<int>( isNullable: true );
        var b = TypeNullability.Create<int>();

        var result = TypeNullability.GetCommonType( a, b );

        result.TestEquals( TypeNullability.Create<int>( isNullable: true ) ).Go();
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

        result.TestNull().Go();
    }

    [Fact]
    public void MakeNullable_ShouldReturnSelf_WhenTypeIsAlreadyNullable()
    {
        var sut = TypeNullability.Create<int>( isNullable: true );
        var result = sut.MakeNullable();
        result.TestEquals( sut ).Go();
    }

    [Fact]
    public void MakeNullable_ShouldReturnNullableType_WhenTypeIsNotNullable()
    {
        var sut = TypeNullability.Create<int>();
        var result = sut.MakeNullable();
        result.TestEquals( TypeNullability.Create<int>( isNullable: true ) ).Go();
    }

    [Fact]
    public void MakeRequired_ShouldReturnSelf_WhenTypeIsAlreadyNotNullable()
    {
        var sut = TypeNullability.Create<int>();
        var result = sut.MakeRequired();
        result.TestEquals( sut ).Go();
    }

    [Fact]
    public void MakeRequired_ShouldReturnNonNullableType_WhenTypeIsNullable()
    {
        var sut = TypeNullability.Create<int>( isNullable: true );
        var result = sut.MakeRequired();
        result.TestEquals( TypeNullability.Create<int>() ).Go();
    }

    [Theory]
    [InlineData( false, "System.String" )]
    [InlineData( true, "Nullable<System.String>" )]
    public void ToString_ShouldReturnCorrectResult(bool isNullable, string expected)
    {
        var sut = TypeNullability.Create<string>( isNullable );
        var result = sut.ToString();
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = TypeNullability.Create<int>( isNullable: true );
        var expected = HashCode.Combine( typeof( int ), true );
        var result = sut.GetHashCode();
        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
    }
}
