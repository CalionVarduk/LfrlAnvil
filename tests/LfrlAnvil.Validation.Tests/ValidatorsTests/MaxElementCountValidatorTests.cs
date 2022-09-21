using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class MaxElementCountValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMaxCountIsLessThanZero()
    {
        var action = Lambda.Of( () => new MaxElementCountValidator<int, string>( maxCount: -1, failureResult: Fixture.Create<string>() ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1, 0 )]
    [InlineData( 3, 2 )]
    [InlineData( 3, 3 )]
    [InlineData( 10, 9 )]
    [InlineData( 10, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenCollectionDoesNotContainTooManyElements(int maxCount, int actualCount)
    {
        var value = Fixture.CreateMany<int>( actualCount ).ToList();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.MaxElementCount<int>( maxCount, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( 0, 1 )]
    [InlineData( 3, 4 )]
    [InlineData( 3, 5 )]
    [InlineData( 10, 11 )]
    [InlineData( 10, 12 )]
    public void Validate_ShouldReturnChainWithFailure_WhenCollectionContainsTooManyElements(int maxCount, int actualCount)
    {
        var value = Fixture.CreateMany<int>( actualCount ).ToList();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.MaxElementCount<int>( maxCount, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, maxCount ) );
    }
}
