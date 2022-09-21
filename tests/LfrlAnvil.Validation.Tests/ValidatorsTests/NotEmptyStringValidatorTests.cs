namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class NotEmptyStringValidatorTests : ValidatorTestsBase
{
    [Theory]
    [InlineData( "a" )]
    [InlineData( "abc" )]
    [InlineData( "abcdefghij" )]
    public void Validate_ShouldReturnEmptyChain_WhenStringIsNotEmpty(string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotEmpty( resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenStringIsEmpty()
    {
        var value = string.Empty;
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotEmpty( resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource ) );
    }
}
