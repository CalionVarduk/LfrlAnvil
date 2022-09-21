namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class GreaterThanOrEqualToValidatorTests : ValidatorTestsBase
{
    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1, 0 )]
    [InlineData( 1, 1 )]
    [InlineData( 2, 1 )]
    [InlineData( 10, 10 )]
    [InlineData( 20, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsGreaterThanOrEqualToDeterminant(int value, int determinant)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.GreaterThanOrEqualTo( determinant, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, 1 )]
    [InlineData( 0, 10 )]
    [InlineData( 5, 10 )]
    [InlineData( 9, 10 )]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsLessThanDeterminant(int value, int determinant)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.GreaterThanOrEqualTo( determinant, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, determinant ) );
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1, 0 )]
    [InlineData( 1, 1 )]
    [InlineData( 2, 1 )]
    [InlineData( 10, 10 )]
    [InlineData( 20, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsGreaterThanOrEqualToDeterminant_WithCustomMessage(int value, int determinant)
    {
        var message = ValidationMessage.Create( Fixture.Create<string>() );
        var sut = FormattableValidators<string>.GreaterThanOrEqualTo( determinant, message );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 0, 1 )]
    [InlineData( 0, 10 )]
    [InlineData( 5, 10 )]
    [InlineData( 9, 10 )]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsLessThanDeterminant_WithCustomMessage(
        int value,
        int determinant)
    {
        var message = ValidationMessage.Create( Fixture.Create<string>() );
        var sut = FormattableValidators<string>.GreaterThanOrEqualTo( determinant, message );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( message.Resource ) );
    }
}
