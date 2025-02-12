using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class AlternativeAssertion : Assertion
{
    internal AlternativeAssertion(string context, IReadOnlyList<Assertion> assertions)
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
                return;
            }
            catch ( XunitException exc )
            {
                errors.Add( exc.Message );
            }
        }

        if ( errors.Count == 0 )
            Throw( $"{(Context.Length > 0 ? $"[{Context}]" : "Assertion group")} failed to satisfy at least one assertion." );

        Throw(
            $"""
             {(Context.Length > 0 ? $"[{Context}]" : "Assertion group")} failed to satisfy at least one assertion, with following {errors.Count} error(s):
             {string.Join( Environment.NewLine, errors.Select( (e, i) => $"{i + 1}. {AssertionExtensions.Indent( e )}" ) )}
             """ );
    }
}
