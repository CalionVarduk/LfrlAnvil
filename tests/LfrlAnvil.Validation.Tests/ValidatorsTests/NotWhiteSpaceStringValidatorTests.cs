namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class NotWhiteSpaceStringValidatorTests : ValidatorTestsBase
{
    [Theory]
    [InlineData( "  a" )]
    [InlineData( "a  " )]
    [InlineData( "  a  " )]
    public void Validate_ShouldReturnEmptyChain_WhenStringIsNotWhiteSpace(string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotWhiteSpace( resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "   " )]
    [InlineData( "  \t  " )]
    public void Validate_ShouldReturnChainWithFailure_WhenStringIsWhiteSpace(string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotWhiteSpace( resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource ) );
    }
}
