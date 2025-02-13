using System.Diagnostics.Contracts;

namespace LfrlAnvil.TestExtensions.Assertions;

public abstract class SubjectAssertion<T> : Assertion
{
    protected SubjectAssertion(string context, T subject)
        : base( context )
    {
        Subject = subject;
    }

    public T Subject { get; }

    [Pure]
    public SubjectAssertion<T> Then(Func<T, Assertion> continuation)
    {
        return new ContinuationAssertion<T>( this, continuation );
    }
}
