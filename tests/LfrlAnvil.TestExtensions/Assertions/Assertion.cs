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
        return new AssertionGroup( context, assertions );
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

    public abstract void Go();

    protected static void Throw(string message)
    {
        throw new XunitException( message );
    }
}
