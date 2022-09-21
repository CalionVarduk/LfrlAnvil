namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class NotMultilineStringValidatorTests : ValidatorTestsBase
{
    [Theory]
    [InlineData( "" )]
    [InlineData( "a" )]
    [InlineData( "abc" )]
    [InlineData( "abcde" )]
    public void Validate_ShouldReturnEmptyChain_WhenStringDoesNotContainNewLineCharacter(string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotMultiline( resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "\n" )]
    [InlineData( "a\nb" )]
    [InlineData( "a\nb\r\nc" )]
    public void Validate_ShouldReturnChainWithFailure_WhenStringContainsAtLeastOneNewLineCharacter(string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotMultiline( resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource ) );
    }
}
