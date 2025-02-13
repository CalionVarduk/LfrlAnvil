using System.Linq;
using LfrlAnvil.Validation.Extensions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class CollectionElementValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenElementValidatorReturnsEmptyChainForEachElement()
    {
        var value = Fixture.CreateMany<int>().ToList();
        var sut = Validators<string>.Pass<int>().ForCollectionElement();

        var result = sut.Validate( value );

        result.TestEmpty().Go();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenMapperReturnsChainWithFailureForSomeElements()
    {
        var value = new[] { 0, 1, 2, -1 };
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.GreaterThanOrEqualTo( 0, failure ).ForCollectionElement();

        var result = sut.Validate( value );

        Assertion.All(
                result.Select( r => r.Result ).TestAll( (r, _) => r.TestSequence( [ failure ] ) ),
                result.Select( r => r.Element ).TestSequence( [ value[^1] ] ) )
            .Go();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailures_WhenMapperReturnsChainWithFailureForAllElements()
    {
        var value = Fixture.CreateMany<int>().ToList();
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.Fail<int>( failure ).ForCollectionElement();

        var result = sut.Validate( value );

        Assertion.All(
                result.Select( r => r.Result ).TestAll( (r, _) => r.TestSequence( [ failure ] ) ),
                result.Select( r => r.Element ).TestSequence( value ) )
            .Go();
    }
}
