using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class TypeCastValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsOfProvidedTypeAndIfIsOfTypeValidatorReturnsEmptyChain()
    {
        var value = Fixture.Create<string>();
        var ifIsOfType = Validators<string>.Pass<string>();
        var ifIsNotOfType = Validators<string>.Fail<object>( Fixture.Create<string>() );
        var sut = Validators<string>.TypeCast( ifIsOfType, ifIsNotOfType );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsOfProvidedTypeAndIfIsOfTypeValidatorReturnsChainWithFailure()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<string>();
        var ifIsOfType = Validators<string>.Fail<string>( failure );
        var ifIsNotOfType = Validators<string>.Pass<object>();
        var sut = Validators<string>.TypeCast( ifIsOfType, ifIsNotOfType );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNotOfProvidedTypeAndIfIsNotOfTypeValidatorReturnsEmptyChain()
    {
        var value = Fixture.Create<int[]>();
        var ifIsOfType = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var ifIsNotOfType = Validators<string>.Pass<object>();
        var sut = Validators<string>.TypeCast( ifIsOfType, ifIsNotOfType );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotOfProvidedTypeAndIfIsNotOfTypeValidatorReturnsChainWithFailure()
    {
        var value = Fixture.Create<int[]>();
        var failure = Fixture.Create<string>();
        var ifIsOfType = Validators<string>.Pass<string>();
        var ifIsNotOfType = Validators<string>.Fail<object>( failure );
        var sut = Validators<string>.TypeCast( ifIsOfType, ifIsNotOfType );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }

    [Fact]
    public void Validate_WhenCreatedWithIfIsOfType_ShouldReturnIfIsOfTypeResult_WhenObjectIsOfProvidedType()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<string>();
        var ifIsOfType = Validators<string>.Fail<string>( failure );
        var sut = Validators<string>.IfIsOfType<object, string>( ifIsOfType );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }

    [Fact]
    public void Validate_WhenCreatedWithIfIsOfType_ShouldReturnEmptyChain_WhenObjectIsNotOfProvidedType()
    {
        var value = Fixture.Create<int[]>();
        var failure = Fixture.Create<string>();
        var ifIsOfType = Validators<string>.Fail<string>( failure );
        var sut = Validators<string>.IfIsOfType<object, string>( ifIsOfType );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenCreatedWithIfIsNotOfType_ShouldReturnIfIsNotOfTypeResult_WhenObjectIsNotOfProvidedType()
    {
        var value = Fixture.Create<int[]>();
        var failure = Fixture.Create<string>();
        var ifIsNotOfType = Validators<string>.Fail<object>( failure );
        var sut = Validators<string>.IfIsNotOfType<object, string>( ifIsNotOfType );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }

    [Fact]
    public void Validate_WhenCreatedWithIfIsNotOfType_ShouldReturnEmptyChain_WhenObjectIsOfProvidedType()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<string>();
        var ifIsNotOfType = Validators<string>.Fail<object>( failure );
        var sut = Validators<string>.IfIsNotOfType<object, string>( ifIsNotOfType );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }
}
