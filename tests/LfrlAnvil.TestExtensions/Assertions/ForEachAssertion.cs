using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class ForEachAssertion<T> : Assertion
{
    internal ForEachAssertion(string context, IEnumerable<T> subject, Func<T, int, Assertion> elementAssertion, bool expectAll)
        : base( context )
    {
        Subject = subject;
        ElementAssertion = elementAssertion;
        ExpectAll = expectAll;
    }

    internal IEnumerable<T> Subject { get; }
    internal Func<T, int, Assertion> ElementAssertion { get; }
    internal bool ExpectAll { get; }

    public override void Go()
    {
        var elements = Subject.Select( (e, i) => (Element: e, Index: i) ).ToList();
        if ( ExpectAll )
        {
            var errors = new List<string>();
            foreach ( var (e, i) in elements )
            {
                try
                {
                    ElementAssertion( e, i ).Go();
                }
                catch ( XunitException exc )
                {
                    errors.Add( $"@[{i}] {exc.Message.Indent()}" );
                }
            }

            if ( errors.Count > 0 )
                Throw(
                    $"""
                     [{Context}] elements should all satisfy the assertion but found {errors.Count} error(s):
                     {string.Join( Environment.NewLine, errors.Select( (e, i) => $"{i + 1}. {e}" ) )}

                     Collection of {elements.Count} element(s):
                     {string.Join( Environment.NewLine, elements.Select( x => $"[@{x.Index}] {$"'{x.Element}'".Indent()}" ) )}
                     """ );
        }
        else
        {
            foreach ( var (e, i) in elements )
            {
                try
                {
                    ElementAssertion( e, i ).Go();
                    return;
                }
                catch ( XunitException )
                {
                    // NOTE: do nothing
                }
            }

            Throw(
                $"""
                 [{Context}] should contain at least one element that satisfies the assertion.

                 Collection of {elements.Count} element(s):
                 {string.Join( Environment.NewLine, elements.Select( x => $"[@{x.Index}] {$"'{x.Element}'".Indent()}" ) )}
                 """ );
        }
    }
}
