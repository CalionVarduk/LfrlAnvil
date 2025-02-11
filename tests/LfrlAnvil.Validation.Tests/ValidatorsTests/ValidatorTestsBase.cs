using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Validation.Tests.ValidatorsTests;

public abstract class ValidatorTestsBase : TestsBase
{
    [Pure]
    protected static Assertion AssertValidationResult<TResource>(
        Chain<ValidationMessage<TResource>> result,
        params ValidationMessage<TResource>[] expected)
    {
        return result.TestSequence(
            expected.Select(
                e => ( Func<ValidationMessage<TResource>, int, Assertion> )((r, _) => Assertion.All(
                    r.Resource.TestEquals( e.Resource ),
                    (r.Parameters ?? Array.Empty<object?>()).TestSequence( e.Parameters ?? Array.Empty<object?>() ) )) ) );
    }
}
