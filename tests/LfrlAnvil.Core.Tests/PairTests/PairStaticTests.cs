namespace LfrlAnvil.Tests.PairTests;

public class PairStaticTests
{
    [Fact]
    public void GetUnderlyingFirstType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Pair.GetUnderlyingFirstType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingFirstType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = Pair.GetUnderlyingFirstType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Pair<int, string> ), typeof( int ) )]
    [InlineData( typeof( Pair<decimal, bool> ), typeof( decimal ) )]
    [InlineData( typeof( Pair<double, byte> ), typeof( double ) )]
    public void GetUnderlyingFirstType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Pair.GetUnderlyingFirstType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingFirstType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Pair<,> ).GetGenericArguments()[0];

        var result = Pair.GetUnderlyingFirstType( typeof( Pair<,> ) );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingSecondType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Pair.GetUnderlyingSecondType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingSecondType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = Pair.GetUnderlyingSecondType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Pair<int, string> ), typeof( string ) )]
    [InlineData( typeof( Pair<decimal, bool> ), typeof( bool ) )]
    [InlineData( typeof( Pair<double, byte> ), typeof( byte ) )]
    public void GetUnderlyingSecondType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Pair.GetUnderlyingSecondType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingSecondType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Pair<,> ).GetGenericArguments()[1];

        var result = Pair.GetUnderlyingSecondType( typeof( Pair<,> ) );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Pair.GetUnderlyingTypes( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingTypes_ShouldReturnNull_WhenTypeIsNotCorrect(Type type)
    {
        var result = Pair.GetUnderlyingTypes( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Pair<int, string> ), typeof( int ), typeof( string ) )]
    [InlineData( typeof( Pair<decimal, bool> ), typeof( decimal ), typeof( bool ) )]
    [InlineData( typeof( Pair<double, byte> ), typeof( double ), typeof( byte ) )]
    public void GetUnderlyingTypes_ShouldReturnCorrectTypes_WhenTypeIsCorrect(Type type, Type expectedFirst, Type expectedSecond)
    {
        var result = Pair.GetUnderlyingTypes( type );

        result.TestEquals( new Pair<Type, Type>( expectedFirst, expectedSecond ) ).Go();
    }

    [Fact]
    public void GetUnderlyingTypes_ShouldReturnCorrectTypes_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Pair<,> ).GetGenericArguments();

        var result = Pair.GetUnderlyingTypes( typeof( Pair<,> ) );

        result.TestEquals( new Pair<Type, Type>( expected[0], expected[1] ) ).Go();
    }
}
