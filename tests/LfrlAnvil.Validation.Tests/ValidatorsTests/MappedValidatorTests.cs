using System.Linq;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.Validation.Extensions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class MappedValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenMapperReturnsEmptyChain()
    {
        var value = Fixture.Create<string>();
        var sut = Validators<int>.Pass<string>().Map( r => Chain.Create( r.Select( v => v.ToString() ) ) );

        var result = sut.Validate( value );

        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenMapperReturnsChainWithFailure()
    {
        var value = Fixture.Create<string>();
        var failure = Fixture.Create<int>();
        var sut = Validators<int>.Fail<string>( failure ).Map( r => Chain.Create( r.Select( v => v.ToString() ) ) );

        var result = sut.Validate( value );

        result.Should().BeSequentiallyEqualTo( failure.ToString() );
    }
}
