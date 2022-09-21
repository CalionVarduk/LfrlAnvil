namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class NotNullValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNotNull_ForRefType()
    {
        var value = Fixture.Create<string>();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotNull<string?>( resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNotNull_ForNullableStructType()
    {
        var value = Fixture.CreateNullable<int>();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotNull<int?>( resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNotNull_ForStructType()
    {
        var value = Fixture.Create<int>();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotNull<int>( resource );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNull_ForRefType()
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotNull<string?>( resource );

        var result = sut.Validate( null );

        AssertValidationResult( result, ValidationMessage.Create( resource ) );
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNull_ForNullableStructType()
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.NotNull<int?>( resource );

        var result = sut.Validate( null );

        AssertValidationResult( result, ValidationMessage.Create( resource ) );
    }
}
