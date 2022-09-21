using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class ElementCountInRangeValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMinCountIsLessThanZero()
    {
        var action = Lambda.Of(
            () => new IsElementCountInRangeValidator<int, string>( minCount: -1, maxCount: 0, failureResult: Fixture.Create<string>() ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMinCountIsGreaterThanMaxCount()
    {
        var action = Lambda.Of(
            () => new IsElementCountInRangeValidator<int, string>( minCount: 2, maxCount: 1, failureResult: Fixture.Create<string>() ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 0, 0 )]
    [InlineData( 0, 1, 0 )]
    [InlineData( 0, 1, 1 )]
    [InlineData( 1, 3, 1 )]
    [InlineData( 1, 3, 2 )]
    [InlineData( 1, 3, 3 )]
    [InlineData( 5, 10, 5 )]
    [InlineData( 5, 10, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenCollectionContainsCorrectAmountOfElements(int minCount, int maxCount, int actualCount)
    {
        var value = Fixture.CreateMany<int>( actualCount ).ToList();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.ElementCountInRange<int>( minCount, maxCount, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( 0, 0, 1 )]
    [InlineData( 0, 1, 2 )]
    [InlineData( 1, 3, 0 )]
    [InlineData( 1, 3, 4 )]
    [InlineData( 1, 3, 5 )]
    [InlineData( 5, 10, 0 )]
    [InlineData( 5, 10, 4 )]
    [InlineData( 5, 10, 11 )]
    [InlineData( 5, 10, 12 )]
    public void Validate_ShouldReturnChainWithFailure_WhenCollectionDoesNotContainCorrectAmountOfElements(
        int minCount,
        int maxCount,
        int actualCount)
    {
        var value = Fixture.CreateMany<int>( actualCount ).ToList();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.ElementCountInRange<int>( minCount, maxCount, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, minCount, maxCount ) );
    }
}
