using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation.Extensions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class ForNullableValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNull_ForRefType()
    {
        var sut = Validators<string>.Fail<string>( Fixture.Create<string>() ).ForNullable();
        var result = sut.Validate( null );
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNotNullAndValidatorReturnsEmptyChain_ForRefType()
    {
        var value = Fixture.Create<string>();
        var sut = Validators<string>.Pass<string>().ForNullable();

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotNullAndValidatorReturnsChainWithFailure_ForRefType()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.Fail<string>( failure ).ForNullable();

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNull_ForNullableStructType()
    {
        var sut = Validators<string>.Fail<int>( Fixture.Create<string>() ).ForNullable();
        var result = sut.Validate( null );
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenObjectIsNotNullAndValidatorReturnsEmptyChain_ForNullableStructType()
    {
        var value = Fixture.Create<int>();
        var sut = Validators<string>.Pass<int>().ForNullable();

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenObjectIsNotNullAndValidatorReturnsChainWithFailure_ForNullableStructType()
    {
        var value = Fixture.Create<int>();
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.Fail<int>( failure ).ForNullable();

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }
}
