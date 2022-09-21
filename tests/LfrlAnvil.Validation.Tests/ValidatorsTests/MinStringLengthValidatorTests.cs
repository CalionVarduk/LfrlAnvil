using LfrlAnvil.Functional;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class MinStringLengthValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMinLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => new MinLengthValidator<string>( minLength: -1, failureResult: Fixture.Create<string>() ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, "" )]
    [InlineData( 0, "a" )]
    [InlineData( 3, "abc" )]
    [InlineData( 3, "abcd" )]
    [InlineData( 10, "abcdefghij" )]
    [InlineData( 10, "abcdefghijk" )]
    public void Validate_ShouldReturnEmptyChain_WhenStringContainsEnoughCharacters(int minLength, string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.MinLength( minLength, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( 1, "" )]
    [InlineData( 3, "a" )]
    [InlineData( 3, "ab" )]
    [InlineData( 10, "" )]
    [InlineData( 10, "abcdefghi" )]
    public void Validate_ShouldReturnChainWithFailure_WhenStringDoesNotContainEnoughCharacters(int minLength, string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.MinLength( minLength, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, minLength ) );
    }
}
