namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class NullValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNull_ForRefType()
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.Null<string?>( resource );

        var result = sut.Validate( null );

        result.TestEmpty().Go();
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNull_ForNullableStructType()
    {
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.Null<int?>( resource );

        var result = sut.Validate( null );

        result.TestEmpty().Go();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotNull_ForRefType()
    {
        var value = Fixture.Create<string>();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.Null<string?>( resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource ) ).Go();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotNull_ForNullableStructType()
    {
        var value = Fixture.Create<int?>();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.Null<int?>( resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource ) ).Go();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotNull_ForStructType()
    {
        var value = Fixture.Create<int>();
        var resource = Fixture.Create<string>();
        var sut = FormattableValidators<string>.Null<int>( resource );

        var result = sut.Validate( value );

        AssertValidationResult( result, ValidationMessage.Create( resource ) ).Go();
    }
}
