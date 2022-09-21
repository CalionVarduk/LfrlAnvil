namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class PassingValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain()
    {
        var value = Fixture.Create<DateTime>();
        var sut = FormattableValidators<string>.Pass<DateTime>();

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }
}
