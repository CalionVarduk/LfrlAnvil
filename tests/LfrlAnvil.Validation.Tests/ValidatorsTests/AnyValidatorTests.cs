using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class AnyValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenValidatorCollectionIsEmpty()
    {
        var value = Fixture.Create<int>();
        var sut = Validators<string>.Any<int>();

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 1 );
            result.FirstOrDefault().Result.Should().BeEmpty();
        }
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenAllValidatorsReturnEmptyChain()
    {
        var value = Fixture.Create<int>();
        var validator1 = Validators<string>.Pass<int>();
        var validator2 = Validators<string>.Pass<int>();
        var validator3 = Validators<string>.Pass<int>();
        var sut = Validators<string>.Any( validator1, validator2, validator3 );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenNotAllValidatorsReturnChainWithFailure()
    {
        var value = Fixture.Create<int>();
        var (failure1, failure2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var validator1 = Validators<string>.Fail<int>( failure1 );
        var validator2 = Validators<string>.Fail<int>( failure2 );
        var validator3 = Validators<string>.Pass<int>();
        var sut = Validators<string>.Any( validator1, validator2, validator3 );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenAllValidatorsReturnChainWithFailure()
    {
        var value = Fixture.Create<int>();
        var (failure1, failure2, failure3, failure4) = Fixture.CreateDistinctCollection<string>( count: 4 );
        var validator1 = Validators<string>.Fail<int>( failure1 );
        var validator2 = Validators<string>.All( Validators<string>.Fail<int>( failure2 ), Validators<string>.Fail<int>( failure3 ) );
        var validator3 = Validators<string>.Fail<int>( failure4 );
        var sut = Validators<string>.Any( validator1, validator2, validator3 );

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 1 );
            result.FirstOrDefault().Result.Should().BeSequentiallyEqualTo( failure1, failure2, failure3, failure4 );
        }
    }
}
