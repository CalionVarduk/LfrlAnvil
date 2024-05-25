using System.Linq;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public abstract class ValidatorTestsBase : TestsBase
{
    protected static void AssertValidationResult<TResource>(
        Chain<ValidationMessage<TResource>> result,
        params ValidationMessage<TResource>[] expected)
    {
        using ( new AssertionScope() )
        {
            result.Count.Should().Be( expected.Length );
            var actual = result.ToArray();
            var count = Math.Min( actual.Length, expected.Length );
            for ( var i = 0; i < count; ++i )
            {
                actual[i].Resource.Should().BeEquivalentTo( expected[i].Resource );
                actual[i].Parameters.Should().BeSequentiallyEqualTo( expected[i].Parameters ?? Array.Empty<object?>() );
            }
        }
    }
}
