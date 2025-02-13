namespace LfrlAnvil.Validation.Tests;

public class ValidationMessageTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var resource = Fixture.Create<string>();
        var parameter = Fixture.Create<int>();

        var sut = new ValidationMessage<string>( resource, parameter );

        Assertion.All(
                sut.Resource.TestRefEquals( resource ),
                sut.Parameters.TestNotNull( p => p.TestSequence( [ parameter ] ) ) )
            .Go();
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var resource = Fixture.Create<string>();
        var parameter = Fixture.Create<int>();

        var sut = ValidationMessage.Create( resource, parameter );

        Assertion.All(
                sut.Resource.TestRefEquals( resource ),
                sut.Parameters.TestNotNull( p => p.TestSequence( [ parameter ] ) ) )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var resource = Fixture.Create<string>();
        var parameter = Fixture.Create<int>();
        var expected = $"Resource: '{resource}', Parameters: 1";
        var sut = ValidationMessage.Create( resource, parameter );

        var result = sut.ToString();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = ValidationMessage.GetUnderlyingType( null );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = ValidationMessage.GetUnderlyingType( type );

        result.TestNull().Go();
    }

    [Theory]
    [InlineData( typeof( ValidationMessage<int> ), typeof( int ) )]
    [InlineData( typeof( ValidationMessage<decimal> ), typeof( decimal ) )]
    [InlineData( typeof( ValidationMessage<double> ), typeof( double ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = ValidationMessage.GetUnderlyingType( type );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( ValidationMessage<> ).GetGenericArguments()[0];

        var result = ValidationMessage.GetUnderlyingType( typeof( ValidationMessage<> ) );

        result.TestEquals( expected ).Go();
    }
}
