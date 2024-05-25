using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation.Extensions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class SelectorValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenSelectorValidatorReturnsEmptyChain()
    {
        var value = Fixture.Create<string>();
        var sut = Validators<string>.Pass<int>().For( (string v) => v.Length );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenSelectorValidatorReturnsChainWithFailure()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.Fail<int>( failure ).For( (string v) => v.Length );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }
}
