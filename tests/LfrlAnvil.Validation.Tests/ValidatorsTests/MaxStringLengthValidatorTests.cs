using LfrlAnvil.Functional;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class MaxStringLengthValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMaxLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => new MaxLengthValidator<string>( maxLength: -1, failureResult: Fixture.Create<string>() ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, "" )]
    [InlineData( 1, "" )]
    [InlineData( 3, "ab" )]
    [InlineData( 3, "abc" )]
    [InlineData( 10, "abcdefghi" )]
    [InlineData( 10, "abcdefghij" )]
    public void Validate_ShouldReturnEmptyChain_WhenStringDoesNotContainTooManyCharacters(int maxLength, string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.MaxLength( maxLength, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( 0, "a" )]
    [InlineData( 3, "abcd" )]
    [InlineData( 3, "abcde" )]
    [InlineData( 10, "abcdefghijk" )]
    [InlineData( 10, "abcdefghijkl" )]
    public void Validate_ShouldReturnChainWithFailure_WhenStringContainsTooManyCharacters(int maxLength, string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.MaxLength( maxLength, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, maxLength ) );
    }
}
