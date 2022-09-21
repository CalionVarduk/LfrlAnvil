using System.Linq;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class NotEmptyCollectionValidatorTests : ValidatorTestsBase
{
    [Theory]
    [InlineData( 1 )]
    [InlineData( 3 )]
    [InlineData( 10 )]
    public void Validate_ShouldReturnEmptyChain_WhenCollectionIsNotEmpty(int count)
    {
        var value = Fixture.CreateMany<int>( count ).ToList();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotEmpty<int>( resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenCollectionIsEmpty()
    {
        var value = Array.Empty<int>();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotEmpty<int>( resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource ) );
    }
}
