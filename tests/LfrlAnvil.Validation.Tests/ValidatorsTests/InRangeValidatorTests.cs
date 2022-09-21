using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class InRangeValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMinIsGreaterThanMax()
    {
        var action = Lambda.Of(
            () => new IsInRangeValidator<int, string>( min: 2, max: 1, Comparer<int>.Default, failureResult: Fixture.Create<string>() ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 0, 0, 0 )]
    [InlineData( 0, 0, 1 )]
    [InlineData( 1, 0, 1 )]
    [InlineData( 3, 3, 10 )]
    [InlineData( 4, 3, 10 )]
    [InlineData( 7, 3, 10 )]
    [InlineData( 9, 3, 10 )]
    [InlineData( 10, 3, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsInRangeDeterminedByMinAndMax(int value, int min, int max)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.InRange( min, max, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( -1, 0, 0 )]
    [InlineData( 1, 0, 0 )]
    [InlineData( 2, 0, 1 )]
    [InlineData( 2, 3, 10 )]
    [InlineData( 0, 3, 10 )]
    [InlineData( 11, 3, 10 )]
    [InlineData( 20, 3, 10 )]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotInRangeDeterminedByMinAndMax(int value, int min, int max)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.InRange( min, max, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, min, max ) );
    }

    [Theory]
    [InlineData( 0, 0, 0 )]
    [InlineData( 0, 0, 1 )]
    [InlineData( 1, 0, 1 )]
    [InlineData( 3, 3, 10 )]
    [InlineData( 4, 3, 10 )]
    [InlineData( 7, 3, 10 )]
    [InlineData( 9, 3, 10 )]
    [InlineData( 10, 3, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsInRangeDeterminedByMinAndMax_WithCustomMessage(int value, int min, int max)
    {
        var message = ValidationMessage.Create( Fixture.Create<string>() );
        var sut = FormattableValidators<string>.InRange( min, max, message );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( -1, 0, 0 )]
    [InlineData( 1, 0, 0 )]
    [InlineData( 2, 0, 1 )]
    [InlineData( 2, 3, 10 )]
    [InlineData( 0, 3, 10 )]
    [InlineData( 11, 3, 10 )]
    [InlineData( 20, 3, 10 )]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotInRangeDeterminedByMinAndMax_WithCustomMessage(
        int value,
        int min,
        int max)
    {
        var message = ValidationMessage.Create( Fixture.Create<string>() );
        var sut = FormattableValidators<string>.InRange( min, max, message );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( message.Resource ) );
    }
}
