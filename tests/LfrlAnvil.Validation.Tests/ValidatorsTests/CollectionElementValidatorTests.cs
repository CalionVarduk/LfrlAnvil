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
                result.Count.TestEquals( 1 ),
                result.FirstOrDefault().Element.TestEquals( value[^1] ),
                result.FirstOrDefault().Result.TestSequence( [ failure ] ) )
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
                result.Count.TestEquals( 3 ),
                result.FirstOrDefault().Element.TestEquals( value[0] ),
                result.FirstOrDefault().Result.TestSequence( [ failure ] ),
                result.ElementAtOrDefault( 1 ).Element.TestEquals( value[1] ),
                result.ElementAtOrDefault( 1 ).Result.TestSequence( [ failure ] ),
                result.ElementAtOrDefault( 2 ).Element.TestEquals( value[2] ),
                result.ElementAtOrDefault( 2 ).Result.TestSequence( [ failure ] ) )
            .Go();
    }
}
