using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Validation.Tests.ValidationMessageTests;

public class ValidationMessageTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var resource = Fixture.Create<string>();
        var parameter = Fixture.Create<int>();

        var sut = new ValidationMessage<string>( resource, parameter );

        using ( new AssertionScope() )
        {
            sut.Resource.Should().BeSameAs( resource );
            sut.Parameters.Should().BeSequentiallyEqualTo( parameter );
        }
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var resource = Fixture.Create<string>();
        var parameter = Fixture.Create<int>();

        var sut = ValidationMessage.Create( resource, parameter );

        using ( new AssertionScope() )
        {
            sut.Resource.Should().BeSameAs( resource );
            sut.Parameters.Should().BeSequentiallyEqualTo( parameter );
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var resource = Fixture.Create<string>();
        var parameter = Fixture.Create<int>();
        var expected = $"Resource: '{resource}', Parameters: 1";
        var sut = ValidationMessage.Create( resource, parameter );

        var result = sut.ToString();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = ValidationMessage.GetUnderlyingType( null );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = ValidationMessage.GetUnderlyingType( type );

        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( ValidationMessage<int> ), typeof( int ) )]
    [InlineData( typeof( ValidationMessage<decimal> ), typeof( decimal ) )]
    [InlineData( typeof( ValidationMessage<double> ), typeof( double ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = ValidationMessage.GetUnderlyingType( type );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( ValidationMessage<> ).GetGenericArguments()[0];

        var result = ValidationMessage.GetUnderlyingType( typeof( ValidationMessage<> ) );

        result.Should().Be( expected );
    }
}
