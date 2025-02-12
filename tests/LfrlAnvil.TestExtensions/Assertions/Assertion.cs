using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Xunit.Sdk;

namespace LfrlAnvil.TestExtensions.Assertions;

public abstract class Assertion
{
    protected Assertion(string context)
    {
        Context = context;
    }

    public string Context { get; }

    [Pure]
    public static Assertion All(params Assertion[] assertions)
    {
        return All( string.Empty, assertions );
    }

    [Pure]
    public static Assertion All(string context, params Assertion[] assertions)
    {
        return new ConjunctionAssertion( context, assertions );
    }

    [Pure]
    public static Assertion All(IEnumerable<Assertion> assertions)
    {
        return All( string.Empty, assertions );
    }

    [Pure]
    public static Assertion All(string context, IEnumerable<Assertion> assertions)
    {
        return All( context, assertions.ToArray() );
    }

    [Pure]
    public static Assertion Any(params Assertion[] assertions)
    {
        return Any( string.Empty, assertions );
    }

    [Pure]
    public static Assertion Any(string context, params Assertion[] assertions)
    {
        return new AlternativeAssertion( context, assertions );
    }

    [Pure]
    public static Assertion Any(IEnumerable<Assertion> assertions)
    {
        return Any( string.Empty, assertions );
    }

    [Pure]
    public static Assertion Any(string context, IEnumerable<Assertion> assertions)
    {
        return Any( context, assertions.ToArray() );
    }

    [Pure]
    public static Assertion CallOrder(Action order)
    {
        return CallOrder( string.Empty, order );
    }

    [Pure]
    public static Assertion CallOrder(string context, Action order)
    {
        return new CallOrderAssertion( context, order );
    }

    public abstract void Go();

    protected static void Throw(string message)
    {
        throw new XunitException( message );
    }
}
