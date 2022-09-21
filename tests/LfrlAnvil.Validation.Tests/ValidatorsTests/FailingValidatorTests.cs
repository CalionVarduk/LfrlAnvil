namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class FailingValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnChainWithFailure()
    {
        var value = Fixture.Create<DateTime>();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.Fail<DateTime>( resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource ) );
    }
}
