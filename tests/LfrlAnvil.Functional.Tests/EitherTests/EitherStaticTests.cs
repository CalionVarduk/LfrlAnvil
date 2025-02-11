namespace LfrlAnvil.Functional.Tests.EitherTests;

public class EitherStaticTests : TestsBase
{
    [Fact]
    public void GetUnderlyingFirstType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Either.GetUnderlyingFirstType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingFirstType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = Either.GetUnderlyingFirstType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Either<int, string> ), typeof( int ) )]
    [InlineData( typeof( Either<decimal, bool> ), typeof( decimal ) )]
    [InlineData( typeof( Either<double, byte> ), typeof( double ) )]
    public void GetUnderlyingFirstType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Either.GetUnderlyingFirstType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingFirstType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Either<,> ).GetGenericArguments()[0];

        var result = Either.GetUnderlyingFirstType( typeof( Either<,> ) );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingSecondType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Either.GetUnderlyingSecondType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingSecondType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = Either.GetUnderlyingSecondType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Either<int, string> ), typeof( string ) )]
    [InlineData( typeof( Either<decimal, bool> ), typeof( bool ) )]
    [InlineData( typeof( Either<double, byte> ), typeof( byte ) )]
    public void GetUnderlyingSecondType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Either.GetUnderlyingSecondType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingSecondType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Either<,> ).GetGenericArguments()[1];

        var result = Either.GetUnderlyingSecondType( typeof( Either<,> ) );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Either.GetUnderlyingTypes( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNotCorrect(Type type)
    {
        var result = Either.GetUnderlyingTypes( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Either<int, string> ), typeof( int ), typeof( string ) )]
    [InlineData( typeof( Either<decimal, bool> ), typeof( decimal ), typeof( bool ) )]
    [InlineData( typeof( Either<double, byte> ), typeof( double ), typeof( byte ) )]
    public void GetUnderlyingTypes_ShouldReturnCorrectTypes_WhenTypeIsCorrect(Type type, Type expectedFirst, Type expectedSecond)
    {
        var result = Either.GetUnderlyingTypes( type );

        result.TestEquals( new Pair<Type, Type>( expectedFirst, expectedSecond ) ).Go();
    }

    [Fact]
    public void GetUnderlyingTypes_ShouldReturnCorrectTypes_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Either<,> ).GetGenericArguments();

        var result = Either.GetUnderlyingTypes( typeof( Either<,> ) );

        result.TestEquals( new Pair<Type, Type>( expected[0], expected[1] ) ).Go();
    }
}
