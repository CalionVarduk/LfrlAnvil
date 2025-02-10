using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit.Sdk;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class SequenceAssertion<T> : Assertion
{
    internal SequenceAssertion(string context, IEnumerable<T> subject, IReadOnlyList<Func<T, int, Assertion>> elementAssertions, bool exact)
        : base( context )
    {
        Subject = subject;
        ElementAssertions = elementAssertions;
        Exact = exact;
    }

    internal IEnumerable<T> Subject { get; }
    internal IReadOnlyList<Func<T, int, Assertion>> ElementAssertions { get; }
    internal bool Exact { get; }

    public override void Go()
    {
        var elements = Subject.Select( (e, i) => (Element: e, Index: i) ).ToList();
        var errors = new List<string>();

        if ( Exact )
        {
            foreach ( var ((e, i), assertion) in elements.Zip( ElementAssertions ) )
            {
                try
                {
                    assertion( e, i ).Go();
                }
                catch ( XunitException exc )
                {
                    errors.Add( $"@[{i}] {exc.Message.Indent()}" );
                }
            }

            if ( elements.Count != ElementAssertions.Count )
                errors.Add( $"Expected exactly {ElementAssertions.Count} elements but found {elements.Count}." );

            if ( errors.Count > 0 )
                Throw(
                    $"""
                     [{Context}] elements should sequentially satisfy provided assertions but found {errors.Count} error(s):
                     {string.Join( Environment.NewLine, errors.Select( (e, i) => $"{i + 1}. {e}" ) )}

                     Collection of {elements.Count} element(s):
                     {string.Join( Environment.NewLine, elements.Select( x => $"[@{x.Index}] {$"'{x.Element}'".Indent()}" ) )}
                     """ );
        }
        else
        {
            if ( ElementAssertions.Count == 0 )
                return;

            var remaining = CollectionsMarshal.AsSpan( elements );
            while ( remaining.Length >= ElementAssertions.Count )
            {
                var j = 0;
                bool contains = true;
                foreach ( var assertion in ElementAssertions )
                {
                    var (e, i) = remaining[j];
                    try
                    {
                        assertion( e, i ).Go();
                    }
                    catch ( XunitException )
                    {
                        contains = false;
                        break;
                    }

                    ++j;
                }

                if ( contains )
                    return;

                remaining = remaining.Slice( 1 );
            }

            Throw(
                $"""
                 [{Context}] should contain elements that contiguously satisfy provided assertions.

                 Collection of {elements.Count} element(s):
                 {string.Join( Environment.NewLine, elements.Select( x => $"[@{x.Index}] {$"'{x.Element}'".Indent()}" ) )}
                 """ );
        }
    }
}
