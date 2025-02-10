using System.Diagnostics.Contracts;

namespace LfrlAnvil.TestExtensions.Assertions;

public sealed class RefTypeAssertionFilter<T>
    where T : class
{
    internal RefTypeAssertionFilter(T? subject)
    {
        Subject = subject;
    }

    public T? Subject { get; }

    [Pure]
    public Assertion OfType<TTarget>(Func<TTarget, Assertion> assertion)
    {
        return Subject is TTarget t ? assertion( t ) : Assertion.All();
    }

    [Pure]
    public Assertion NotNull(Func<T, Assertion> assertion)
    {
        return Subject is not null ? assertion( Subject ) : Assertion.All();
    }
}
