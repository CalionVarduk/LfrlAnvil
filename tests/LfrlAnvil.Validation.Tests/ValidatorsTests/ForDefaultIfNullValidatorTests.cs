using LfrlAnvil.Validation.Extensions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class ForDefaultIfNullValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNullAndValidatorReturnsEmptyChainForDefaultValue_ForRefType()
    {
        var sut = Validators<string>.Empty( Fixture.Create<string>() ).ForDefaultIfNull( string.Empty );
        var result = sut.Validate( null );
        result.TestEmpty().Go();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNullAndValidatorReturnsChainWithFailureForDefaultValue_ForRefType()
    {
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.NotEmpty( failure ).ForDefaultIfNull( string.Empty );

        var result = sut.Validate( null );

        result.TestSequence( [ failure ] ).Go();
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNotNullAndValidatorReturnsEmptyChain_ForRefType()
    {
        var value = Fixture.Create<string>();
        var sut = Validators<string>.NotEmpty( Fixture.Create<string>() ).ForDefaultIfNull( string.Empty );

        var result = sut.Validate( value );

        result.TestEmpty().Go();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotNullAndValidatorReturnsChainWithFailure_ForRefType()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.Empty( failure ).ForDefaultIfNull( string.Empty );

        var result = sut.Validate( value );

        result.TestSequence( [ failure ] ).Go();
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNullAndValidatorReturnsEmptyChainForDefaultValue_ForNullableStructType()
    {
        var sut = Validators<string>.EqualTo( 0, Fixture.Create<string>() ).ForDefaultIfNull( 0 );
        var result = sut.Validate( null );
        result.TestEmpty().Go();
    }

    [Fact]
    public void
        Validate_ShouldReturnChainWithFailure_WhenObjectIsNullAndValidatorReturnsChainWithFailureForDefaultValue_ForNullableStructType()
    {
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.NotEqualTo( 0, failure ).ForDefaultIfNull( 0 );

        var result = sut.Validate( null );

        result.TestSequence( [ failure ] ).Go();
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNotNullAndValidatorReturnsEmptyChain_ForNullableStructType()
    {
        var value = Fixture.CreateNotDefault<int>();
        var sut = Validators<string>.NotEqualTo( 0, Fixture.Create<string>() ).ForDefaultIfNull( 0 );

        var result = sut.Validate( value );

        result.TestEmpty().Go();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotNullAndValidatorReturnsChainWithFailure_ForNullableStructType()
    {
        var value = Fixture.CreateNotDefault<int>();
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.EqualTo( 0, failure ).ForDefaultIfNull( 0 );

        var result = sut.Validate( value );

        result.TestSequence( [ failure ] ).Go();
    }
}
