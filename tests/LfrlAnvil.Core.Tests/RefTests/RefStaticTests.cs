namespace LfrlAnvil.Tests.RefTests;

public class RefStaticTests
{
    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Ref.GetUnderlyingType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = Ref.GetUnderlyingType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Ref<int> ), typeof( int ) )]
    [InlineData( typeof( Ref<decimal> ), typeof( decimal ) )]
    [InlineData( typeof( Ref<double> ), typeof( double ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Ref.GetUnderlyingType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Ref<> ).GetGenericArguments()[0];

        var result = Ref.GetUnderlyingType( typeof( Ref<> ) );

        result.TestEquals( expected ).Go();
    }
}
