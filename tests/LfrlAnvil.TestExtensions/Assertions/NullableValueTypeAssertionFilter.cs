using System.Diagnostics.Contracts;

namespace LfrlAnvil.TestExtensions.Assertions;

public sealed class NullableValueTypeAssertionFilter<T>
    where T : struct
{
    internal NullableValueTypeAssertionFilter(T? subject)
    {
        Subject = subject;
    }

    public T? Subject { get; }

    [Pure]
    public Assertion NotNull(Func<T, Assertion> assertion)
    {
        return Subject is not null ? assertion( Subject.Value ) : Assertion.All();
    }
}
