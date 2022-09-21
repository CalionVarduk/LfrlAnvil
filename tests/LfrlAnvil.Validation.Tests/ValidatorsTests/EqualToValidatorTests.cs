namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class EqualToValidatorTests : ValidatorTestsBase
{
    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1, 1 )]
    [InlineData( 10, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsEqualToDeterminant(int value, int determinant)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.EqualTo( determinant, resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 1, 0 )]
    [InlineData( 0, 1 )]
    [InlineData( 2, 1 )]
    [InlineData( 5, 10 )]
    [InlineData( 20, 10 )]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotEqualToDeterminant(int value, int determinant)
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.EqualTo( determinant, resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource, determinant ) );
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 1, 1 )]
    [InlineData( 10, 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsEqualToDeterminant_WithCustomMessage(int value, int determinant)
    {
        var message = ValidationMessage.Create( Fixture.Create<string>() );
        var sut = FormattableValidators<string>.EqualTo( determinant, message );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 1, 0 )]
    [InlineData( 0, 1 )]
    [InlineData( 2, 1 )]
    [InlineData( 5, 10 )]
    [InlineData( 20, 10 )]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotEqualToDeterminant_WithCustomMessage(int value, int determinant)
    {
        var message = ValidationMessage.Create( Fixture.Create<string>() );
        var sut = FormattableValidators<string>.EqualTo( determinant, message );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( message.Resource ) );
    }
}
