namespace LfrlAnvil.Tests.BoundsTests;

public class BoundsStaticTests
{
    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Bounds.GetUnderlyingType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = Bounds.GetUnderlyingType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Bounds<int> ), typeof( int ) )]
    [InlineData( typeof( Bounds<decimal> ), typeof( decimal ) )]
    [InlineData( typeof( Bounds<double> ), typeof( double ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Bounds.GetUnderlyingType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Bounds<> ).GetGenericArguments()[0];

        var result = Bounds.GetUnderlyingType( typeof( Bounds<> ) );

        result.TestEquals( expected ).Go();
    }
}
