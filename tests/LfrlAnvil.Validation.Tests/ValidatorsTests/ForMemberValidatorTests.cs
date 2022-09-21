using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation.Extensions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class ForMemberValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenMemberValidatorReturnsEmptyChain()
    {
        var value = Fixture.Create<string>();
        var sut = Validators<string>.Pass<int>().ForMember( (string v) => v.Length );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenMemberValidatorReturnsChainWithFailure()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.Fail<int>( failure ).ForMember( (string v) => v.Length );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }
}
