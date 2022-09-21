using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class AllValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenValidatorCollectionIsEmpty()
    {
        var value = Fixture.Create<int>();
        var sut = Validators<string>.All<int>();

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenAllValidatorsReturnEmptyChain()
    {
        var value = Fixture.Create<int>();
        var validator1 = Validators<string>.Pass<int>();
        var validator2 = Validators<string>.Pass<int>();
        var validator3 = Validators<string>.Pass<int>();
        var sut = Validators<string>.All( validator1, validator2, validator3 );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenSomeValidatorsReturnChainWithFailure()
    {
        var value = Fixture.Create<int>();
        var failure = Fixture.Create<string>();
        var validator1 = Validators<string>.Pass<int>();
        var validator2 = Validators<string>.Pass<int>();
        var validator3 = Validators<string>.Fail<int>( failure );
        var sut = Validators<string>.All( validator1, validator2, validator3 );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure );
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailures_WhenAllValidatorsReturnChainWithFailure()
    {
        var value = Fixture.Create<int>();
        var (failure1, failure2, failure3, failure4) = Fixture.CreateDistinctCollection<string>( count: 4 );
        var validator1 = Validators<string>.Fail<int>( failure1 );
        var validator2 = Validators<string>.All( Validators<string>.Fail<int>( failure2 ), Validators<string>.Fail<int>( failure3 ) );
        var validator3 = Validators<string>.Fail<int>( failure4 );
        var sut = Validators<string>.All( validator1, validator2, validator3 );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure1, failure2, failure3, failure4 );
    }
}
