using System.Diagnostics.Contracts;

namespace LfrlAnvil.TestExtensions.Assertions;

public sealed class TypeAssertionBuilder<T>
    where T : class
{
    internal TypeAssertionBuilder(string context, T? subject)
    {
        Context = context;
        Subject = subject;
    }

    public string Context { get; }
    public T? Subject { get; }

    [Pure]
    public SubjectAssertion<T?> Exact(Type expected)
    {
        return new TypeAssertion<T?>( Context, Subject, expected, exact: true );
    }

    [Pure]
    public SubjectAssertion<T?> AssignableTo(Type expected)
    {
        return new TypeAssertion<T?>( Context, Subject, expected, exact: false );
    }

    [Pure]
    public SubjectAssertion<T?> Exact<TExpected>()
    {
        return Exact( typeof( TExpected ) );
    }

    [Pure]
    public SubjectAssertion<T?> Exact<TExpected>(Func<TExpected, Assertion> continuation)
    {
        return Exact( typeof( TExpected ) ).Then( x => continuation( ( TExpected )( object )x! ) );
    }

    [Pure]
    public SubjectAssertion<T?> AssignableTo<TExpected>()
    {
        return AssignableTo( typeof( TExpected ) );
    }

    [Pure]
    public SubjectAssertion<T?> AssignableTo<TExpected>(Func<TExpected, Assertion> continuation)
    {
        return AssignableTo( typeof( TExpected ) ).Then( x => continuation( ( TExpected )( object )x! ) );
    }
}
