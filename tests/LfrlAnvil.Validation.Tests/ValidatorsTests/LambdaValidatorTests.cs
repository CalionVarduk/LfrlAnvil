using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class LambdaValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenDelegateReturnsEmptyChain()
    {
        var sut = LambdaValidator<string>.Create( (int _) => Chain<string>.Empty );
        var result = sut.Validate( Fixture.Create<int>() );
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenDelegateReturnsChainWithFailure()
    {
        var failure = Fixture.Create<string>();
        var sut = LambdaValidator<string>.Create( (int _) => Chain.Create( failure ) );

        var result = sut.Validate( Fixture.Create<int>() );

        result.Should().BeSequentiallyEqualTo( failure );
    }
}
