using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class ConjunctionAssertion : Assertion
{
    internal ConjunctionAssertion(string context, IReadOnlyList<Assertion> assertions)
        : base( context )
    {
        Assertions = assertions;
    }

    internal IReadOnlyList<Assertion> Assertions { get; }

    public override void Go()
    {
        var errors = new List<string>();
        foreach ( var assertion in Assertions )
        {
            try
            {
                assertion.Go();
            }
            catch ( XunitException exc )
            {
                errors.Add( exc.Message );
            }
        }

        if ( errors.Count > 0 )
            Throw(
                $"""
                 {(Context.Length > 0 ? $"[{Context}]" : "ALL assertion")} failed with {errors.Count} error(s):
                 {string.Join( Environment.NewLine, errors.Select( (e, i) => $"{i + 1}. {e.Indent()}" ) )}
                 """ );
    }
}
