using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class ExactElementCountValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanZero()
    {
        var action = Lambda.Of( () => new IsElementCountExactValidator<int, string>( count: -1, failureResult: Fixture.Create<string>() ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1, 1 )]
    [InlineData( 3, 3 )]
    [InlineData( 10, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenCollectionContainsExactAmountOfElements(int count, int actualCount)
    {
        var value = Fixture.CreateMany<int>( actualCount ).ToList();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.ExactElementCount<int>( count, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 1, 0 )]
    [InlineData( 1, 2 )]
    [InlineData( 3, 1 )]
    [InlineData( 3, 4 )]
    [InlineData( 10, 9 )]
    [InlineData( 10, 11 )]
    public void Validate_ShouldReturnChainWithFailure_WhenCollectionDoesNotContainExactAmountOfElements(int count, int actualCount)
    {
        var value = Fixture.CreateMany<int>( actualCount ).ToList();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.ExactElementCount<int>( count, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, count ) );
    }
}
