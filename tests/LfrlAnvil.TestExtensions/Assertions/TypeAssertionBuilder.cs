using System.Diagnostics.Contracts;

namespace LfrlAnvil.TestExtensions.Assertions;

public sealed class TypeAssertionBuilder<T>
{
    internal TypeAssertionBuilder(string context, T subject)
    {
        Context = context;
        Subject = subject;
    }

    public string Context { get; }
    public T Subject { get; }

    [Pure]
    public Assertion Exact(Type expected)
    {
        return new TypeAssertion<T>( Context, Subject, expected, exact: true );
    }

    [Pure]
    public Assertion AssignableTo(Type expected)
    {
        return new TypeAssertion<T>( Context, Subject, expected, exact: false );
    }

    [Pure]
    public Assertion Exact<TExpected>()
    {
        return Exact( typeof( TExpected ) );
    }

    [Pure]
    public Assertion AssignableTo<TExpected>()
    {
        return AssignableTo( typeof( TExpected ) );
    }
}
