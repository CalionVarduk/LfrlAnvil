using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class MinElementCountValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMinCountIsLessThanZero()
    {
        var action = Lambda.Of( () => new MinElementCountValidator<int, string>( minCount: -1, failureResult: Fixture.Create<string>() ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 0, 1 )]
    [InlineData( 3, 3 )]
    [InlineData( 3, 4 )]
    [InlineData( 10, 10 )]
    [InlineData( 10, 11 )]
    public void Validate_ShouldReturnEmptyChain_WhenCollectionContainsEnoughElements(int minCount, int actualCount)
    {
        var value = Fixture.CreateMany<int>( actualCount ).ToList();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.MinElementCount<int>( minCount, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( 1, 0 )]
    [InlineData( 3, 1 )]
    [InlineData( 3, 2 )]
    [InlineData( 10, 0 )]
    [InlineData( 10, 9 )]
    public void Validate_ShouldReturnChainWithFailure_WhenCollectionDoesNotContainEnoughElements(int minCount, int actualCount)
    {
        var value = Fixture.CreateMany<int>( actualCount ).ToList();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.MinElementCount<int>( minCount, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, minCount ) );
    }
}
