namespace LfrlAnvil.Functional.Tests.ErraticTests;

public class ErraticStaticTests : TestsBase
{
    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Erratic.GetUnderlyingType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = Erratic.GetUnderlyingType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Erratic<int> ), typeof( int ) )]
    [InlineData( typeof( Erratic<decimal> ), typeof( decimal ) )]
    [InlineData( typeof( Erratic<double> ), typeof( double ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Erratic.GetUnderlyingType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Erratic<> ).GetGenericArguments()[0];

        var result = Erratic.GetUnderlyingType( typeof( Erratic<> ) );

        result.TestEquals( expected ).Go();
    }
}
