using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit.Sdk;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class SequenceAssertion<T> : SubjectAssertion<IReadOnlyList<T>>
{
    internal SequenceAssertion(
        string context,
        IReadOnlyList<T> subject,
        IEnumerable<Func<T, int, Assertion>> elementAssertions,
        SequenceComparisonType type)
        : base( context, subject )
    {
        ElementAssertions = elementAssertions as IReadOnlyList<Func<T, int, Assertion>> ?? elementAssertions.ToArray();
        Type = type;
    }

    internal IReadOnlyList<Func<T, int, Assertion>> ElementAssertions { get; }
    internal SequenceComparisonType Type { get; }

    public override void Go()
    {
        var elements = Subject.Select( (e, i) => (Element: e, Index: i) ).ToList();

        switch ( Type )
        {
            case SequenceComparisonType.Equal:
            {
                var errors = new List<string>();
                foreach ( var ((e, i), assertion) in elements.Zip( ElementAssertions ) )
                {
                    try
                    {
                        assertion( e, i ).Go();
                    }
                    catch ( XunitException exc )
                    {
                        errors.Add( $"[@{i}] {exc.Message.Indent()}" );
                    }
                }

                if ( elements.Count != ElementAssertions.Count )
                    errors.Add( $"Expected exactly {ElementAssertions.Count} element(s) but found {elements.Count}." );

                if ( errors.Count > 0 )
                    Throw(
                        $"""
                         [{Context}] elements should sequentially satisfy provided assertions but found {errors.Count} error(s):
                         {string.Join( Environment.NewLine, errors.Select( (e, i) => $"{i + 1}. {e}" ) )}

                         Collection of {elements.Count} element(s):
                         {string.Join( Environment.NewLine, elements.Select( x => $"[@{x.Index}] {x.Element.Stringify().Indent()}" ) )}
                         """ );

                break;
            }
            case SequenceComparisonType.Contains:
            {
                if ( ElementAssertions.Count == 0 )
                    return;

                var found = 0;
                var remaining = CollectionsMarshal.AsSpan( elements );
                while ( remaining.Length > 0 && remaining.Length >= ElementAssertions.Count - found )
                {
                    var assertion = ElementAssertions[found];
                    var (e, i) = remaining[0];
                    try
                    {
                        assertion( e, i ).Go();
                        if ( ++found == ElementAssertions.Count )
                            break;
                    }
                    catch ( XunitException )
                    {
                        // NOTE: do nothing
                    }
                    finally
                    {
                        remaining = remaining.Slice( 1 );
                    }
                }

                if ( found == ElementAssertions.Count )
                    return;

                Throw(
                    $"""
                     [{Context}] should contain elements that satisfy {ElementAssertions.Count} provided assertions but found only {found} matching element(s).

                     Collection of {elements.Count} element(s):
                     {string.Join( Environment.NewLine, elements.Select( x => $"[@{x.Index}] {x.Element.Stringify().Indent()}" ) )}
                     """ );

                break;
            }
            case SequenceComparisonType.ContainsContiguous:
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
                     [{Context}] should contain elements that contiguously satisfy {ElementAssertions.Count} provided assertion(s).

                     Collection of {elements.Count} element(s):
                     {string.Join( Environment.NewLine, elements.Select( x => $"[@{x.Index}] {x.Element.Stringify().Indent()}" ) )}
                     """ );

                break;
            }
        }
    }
}
