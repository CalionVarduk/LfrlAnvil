namespace LfrlAnvil.Dependencies.Tests;

public class InjectedTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldAssignInstance()
    {
        var instance = Fixture.Create<string>();
        var sut = new Injected<string>( instance );
        sut.Instance.TestRefEquals( instance ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var instance = Fixture.Create<string>();
        var expected = $"Injected({instance})";
        var sut = new Injected<string>( instance );

        var result = sut.ToString();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Create_ShouldAssignInstance()
    {
        var instance = Fixture.Create<string>();
        var sut = Injected.Create( instance );
        sut.Instance.TestRefEquals( instance ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Injected.GetUnderlyingType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = Injected.GetUnderlyingType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( Injected<int> ), typeof( int ) )]
    [InlineData( typeof( Injected<decimal> ), typeof( decimal ) )]
    [InlineData( typeof( Injected<double> ), typeof( double ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Injected.GetUnderlyingType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Injected<> ).GetGenericArguments()[0];

        var result = Injected.GetUnderlyingType( typeof( Injected<> ) );

        result.TestEquals( expected ).Go();
    }
}
