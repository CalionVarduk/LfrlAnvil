namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class EmptyStringalidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenStringIsEmpty()
    {
        var value = string.Empty;
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.Empty( resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "a" )]
    [InlineData( "abc" )]
    [InlineData( "abcdefghij" )]
    public void Validate_ShouldReturnChainWithFailure_WhenStringIsNotEmpty(string value)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.Empty( resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource ) );
    }
}
