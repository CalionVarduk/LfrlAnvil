using System.Text.RegularExpressions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class NotMatchStringValidatorTests : ValidatorTestsBase
{
    [Theory]
    [InlineData( "a.*", "b" )]
    [InlineData( ".b.*", "acd" )]
    [InlineData( ".+cd.*", "abced" )]
    public void Validate_ShouldReturnEmptyChain_WhenStringDoesNotMatchTheRegex(string pattern, string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotMatch( new Regex( pattern ), resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "a.*", "a" )]
    [InlineData( ".b.*", "abc" )]
    [InlineData( ".+cd.*", "abcde" )]
    public void Validate_ShouldReturnChainWithFailure_WhenStringMatchesTheRegex(string pattern, string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotMatch( new Regex( pattern ), resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource ) );
    }
}
