using LfrlAnvil.Functional;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class ExactStringLengthValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => new IsLengthExactValidator<string>( length: -1, failureResult: Fixture.Create<string>() ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, "" )]
    [InlineData( 1, "a" )]
    [InlineData( 3, "abc" )]
    [InlineData( 10, "abcdefghij" )]
    public void Validate_ShouldReturnEmptyChain_WhenStringContainsExactAmountOfCharacters(int length, string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.ExactLength( length, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( 0, "a" )]
    [InlineData( 1, "" )]
    [InlineData( 1, "ab" )]
    [InlineData( 3, "a" )]
    [InlineData( 3, "abcd" )]
    [InlineData( 10, "abcdefghi" )]
    [InlineData( 10, "abcdefghijk" )]
    public void Validate_ShouldReturnChainWithFailure_WhenStringDoesNotContainExactAmountOfCharacters(int length, string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.ExactLength( length, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, length ) );
    }
}
