using System.Linq;
using System.Text;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.Validation.Extensions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public class FormattedValidatorTests : ValidatorTestsBase
{
    [Fact]
    public void Validate_ShouldReturnEmptyChain_WhenFormatterReturnsNullStringBuilder()
    {
        var formatter = Substitute.For<IValidationMessageFormatter<string>>();
        formatter.Format( Arg.Any<StringBuilder?>(), Arg.Any<Chain<ValidationMessage<string>>>(), Arg.Any<IFormatProvider?>() )
            .Returns( _ => null );

        var format = Substitute.For<IFormatProvider>();
        var formatProvider = Substitute.For<Func<IFormatProvider?>>().WithAnyArgs( _ => format );
        var sut = FormattableValidators<string>.Pass<int>().Format( formatter, formatProvider );

        var result = sut.Validate( Fixture.Create<int>() );

        Assertion.All(
                result.TestEmpty(),
                formatProvider.CallCount().TestEquals( 1 ),
                formatter.TestReceivedCalls( f => f.Format( null, Chain<ValidationMessage<string>>.Empty, format ), count: 1 ) )
            .Go();
    }

    [Fact]
    public void Validate_ShouldReturnChainWithFailure_WhenFormatterReturnsStringBuilderForReceivedChainWithFailureFromValidator()
    {
        var failure = Fixture.Create<string>();
        var formatterResult = new StringBuilder( Fixture.Create<string>() );

        var formatter = Substitute.For<IValidationMessageFormatter<string>>();
        formatter.Format( Arg.Any<StringBuilder?>(), Arg.Any<Chain<ValidationMessage<string>>>(), Arg.Any<IFormatProvider?>() )
            .Returns( _ => formatterResult );

        var sut = FormattableValidators<string>.Fail<int>( failure ).Format( formatter );

        var result = sut.Validate( Fixture.Create<int>() );

        Assertion.All(
                result.Count.TestEquals( 1 ),
                result.FirstOrDefault().Result.TestEquals( formatterResult.ToString() ),
                AssertValidationResult( result.FirstOrDefault().Messages, ValidationMessage.Create( failure ) ),
                formatter.TestReceivedCalls( f => f.Format( null, Arg.Any<Chain<ValidationMessage<string>>>() ), count: 1 ) )
            .Go();
    }
}
