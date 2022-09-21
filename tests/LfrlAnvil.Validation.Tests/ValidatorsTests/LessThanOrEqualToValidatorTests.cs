﻿namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class LessThanOrEqualToValidatorTests : ValidatorTestsBase
{
    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, 0 )]
    [InlineData( 0, 1 )]
    [InlineData( 1, 1 )]
    [InlineData( 0, 10 )]
    [InlineData( 5, 10 )]
    [InlineData( 10, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsLessThanOrEqualToDeterminant(int value, int determinant)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.LessThanOrEqualTo( determinant, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( 1, 0 )]
    [InlineData( 2, 1 )]
    [InlineData( 20, 10 )]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsGreaterThanDeterminant(int value, int determinant)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.LessThanOrEqualTo( determinant, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, determinant ) );
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, 0 )]
    [InlineData( 0, 1 )]
    [InlineData( 1, 1 )]
    [InlineData( 0, 10 )]
    [InlineData( 5, 10 )]
    [InlineData( 10, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsLessThanOrEqualToDeterminant_WithCustomMessage(int value, int determinant)
    {
        var message = ValidationMessage.Create( Fixture.Create<string>() );
        var sut = FormattableValidators<string>.LessThanOrEqualTo( determinant, message );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( 1, 0 )]
    [InlineData( 2, 1 )]
    [InlineData( 20, 10 )]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsGreaterThanDeterminant_WithCustomMessage(
        int value,
        int determinant)
    {
        var message = ValidationMessage.Create( Fixture.Create<string>() );
        var sut = FormattableValidators<string>.LessThanOrEqualTo( determinant, message );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( message.Resource ) );
    }
}
