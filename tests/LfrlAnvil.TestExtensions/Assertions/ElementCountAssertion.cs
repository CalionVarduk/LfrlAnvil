using System.Collections.Generic;
using Xunit.Sdk;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class ElementCountAssertion<T> : SubjectAssertion<IReadOnlyList<T>>
{
    internal ElementCountAssertion(string context, IReadOnlyList<T> subject, Func<int, Assertion> assertion)
        : base( context, subject )
    {
        Assertion = assertion;
    }

    internal Func<int, Assertion> Assertion { get; }

    public override void Go()
    {
        try
        {
            Assertion( Subject.Count ).Go();
        }
        catch ( XunitException exc )
        {
            Throw(
                $"""
                 [{Context}] element count assertion failed:
                   {exc.Message.Indent()}
                 """ );
        }
    }
}
