namespace LfrlAnvil.Tests.EqualityTests;

public class EqualityStaticTests
{
    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Equality.GetUnderlyingType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNotCorrect(Type type)
    {
        var result = Equality.GetUnderlyingType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Equality<int> ), typeof( int ) )]
    [InlineData( typeof( Equality<decimal> ), typeof( decimal ) )]
    [InlineData( typeof( Equality<double> ), typeof( double ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Equality.GetUnderlyingType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Equality<> ).GetGenericArguments()[0];

        var result = Equality.GetUnderlyingType( typeof( Equality<> ) );

        result.TestEquals( expected ).Go();
    }
}
