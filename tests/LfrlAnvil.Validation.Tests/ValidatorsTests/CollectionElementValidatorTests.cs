using System.Linq;
using LfrlAnvil.TestExtensions.FluentAssertions;
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

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenMapperReturnsChainWithFailureForSomeElements()
    {
        var value = new[] { 0, 1, 2, -1 };
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.GreaterThanOrEqualTo( 0, failure ).ForCollectionElement();

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 1 );
            result.FirstOrDefault().Element.Should().Be( value[^1] );
            result.FirstOrDefault().Result.Should().BeSequentiallyEqualTo( failure );
        }
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailures_WhenMapperReturnsChainWithFailureForAllElements()
    {
        var value = Fixture.CreateMany<int>().ToList();
        var failure = Fixture.Create<string>();
        var sut = Validators<string>.Fail<int>( failure ).ForCollectionElement();

        var result = sut.Validate( value );

        using ( new AssertionScope() )
        {
            result.Should().HaveCount( 3 );
            result.FirstOrDefault().Element.Should().Be( value[0] );
            result.FirstOrDefault().Result.Should().BeSequentiallyEqualTo( failure );
            result.ElementAtOrDefault( 1 ).Element.Should().Be( value[1] );
            result.ElementAtOrDefault( 1 ).Result.Should().BeSequentiallyEqualTo( failure );
            result.ElementAtOrDefault( 2 ).Element.Should().Be( value[2] );
            result.ElementAtOrDefault( 2 ).Result.Should().BeSequentiallyEqualTo( failure );
        }
    }
}
