using LfrlAnvil.Functional;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class StringLengthInRangeValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMinLengthIsLessThanZero()
    {
        var action = Lambda.Of(
            () => new IsLengthInRangeValidator<string>( minLength: -1, maxLength: 0, failureResult: Fixture.Create<string>() ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMinLengthIsGreaterThanMaxLength()
    {
        var action = Lambda.Of(
            () => new IsLengthInRangeValidator<string>( minLength: 2, maxLength: 1, failureResult: Fixture.Create<string>() ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 0, "" )]
    [InlineData( 0, 1, "" )]
    [InlineData( 0, 1, "a" )]
    [InlineData( 1, 3, "a" )]
    [InlineData( 1, 3, "ab" )]
    [InlineData( 1, 3, "abc" )]
    [InlineData( 5, 10, "abcde" )]
    [InlineData( 5, 10, "abcdefghij" )]
    public void Validate_ShouldReturnEmptyChain_WhenStringContainsCorrectAmountOfCharacters(int minLength, int maxLength, string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.LengthInRange( minLength, maxLength, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( 0, 0, "a" )]
    [InlineData( 0, 1, "ab" )]
    [InlineData( 1, 3, "" )]
    [InlineData( 1, 3, "abcd" )]
    [InlineData( 1, 3, "abcde" )]
    [InlineData( 5, 10, "" )]
    [InlineData( 5, 10, "abcd" )]
    [InlineData( 5, 10, "abcdefghijk" )]
    [InlineData( 5, 10, "abcdefghijkl" )]
    public void Validate_ShouldReturnChainWithFailure_WhenStringDoesNotContainCorrectAmountOfCharacters(
        int minLength,
        int maxLength,
        string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.LengthInRange( minLength, maxLength, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, minLength, maxLength ) );
    }
}
