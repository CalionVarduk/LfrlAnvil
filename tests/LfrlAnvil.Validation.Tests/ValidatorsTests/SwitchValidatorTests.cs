using System.Collections.Generic;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class SwitchValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenValidatorForSwitchValueExistsAndReturnsEmptyChain()
    {
        var value = Fixture.Create<string>();
        var validator1 = Validators<string>.Pass<string>();
        var validator2 = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var defaultValidator = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var sut = Validators<string>.Switch(
            v => v.Length,
            new Dictionary<int, IValidator<string, string>>
            {
                { value.Length, validator1 },
                { value.Length + 1, validator2 }
            },
            defaultValidator );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenValidatorForSwitchValueExistsAndReturnsChainWithFailure()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<string>();
        var validator1 = Validators<string>.Fail<string>( failure );
        var validator2 = Validators<string>.Pass<string>();
        var defaultValidator = Validators<string>.Pass<string>();
        var sut = Validators<string>.Switch(
            v => v.Length,
            new Dictionary<int, IValidator<string, string>>
            {
                { value.Length, validator1 },
                { value.Length + 1, validator2 }
            },
            defaultValidator );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenValidatorForSwitchValueDoesNotExistAndDefaultReturnsEmptyChain()
    {
        var value = Fixture.Create<string>();
        var validator1 = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var validator2 = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var defaultValidator = Validators<string>.Pass<string>();
        var sut = Validators<string>.Switch(
            v => v.Length,
            new Dictionary<int, IValidator<string, string>>
            {
                { value.Length + 1, validator1 },
                { value.Length + 2, validator2 }
            },
            defaultValidator );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenValidatorForSwitchValueDoesNotExistAndDefaultReturnsChainWithFailure()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<string>();
        var validator1 = Validators<string>.Pass<string>();
        var validator2 = Validators<string>.Pass<string>();
        var defaultValidator = Validators<string>.Fail<string>( failure );
        var sut = Validators<string>.Switch(
            v => v.Length,
            new Dictionary<int, IValidator<string, string>>
            {
                { value.Length + 1, validator1 },
                { value.Length + 2, validator2 }
            },
            defaultValidator );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }

    [Fact]
    public void Validate_WhenCreatedWithoutExplicitDefault_ShouldReturnValidatorResult_WhenValidatorForSwitchValueExists()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<string>();
        var validator1 = Validators<string>.Fail<string>( failure );
        var validator2 = Validators<string>.Pass<string>();
        var sut = Validators<string>.Switch(
            v => v.Length,
            new Dictionary<int, IValidator<string, string>>
            {
                { value.Length, validator1 },
                { value.Length + 1, validator2 }
            } );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }

    [Fact]
    public void Validate_WhenCreatedWithoutExplicitDefault_ShouldReturnEmptyChain_WhenValidatorForSwitchValueDoesNotExist()
    {
        var value = Fixture.Create<string>();
        var validator1 = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var validator2 = Validators<string>.Fail<string>( Fixture.Create<string>() );
        var sut = Validators<string>.Switch(
            v => v.Length,
            new Dictionary<int, IValidator<string, string>>
            {
                { value.Length + 1, validator1 },
                { value.Length + 2, validator2 }
            } );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }
}
