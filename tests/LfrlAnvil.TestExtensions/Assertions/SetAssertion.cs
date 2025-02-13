using System.Collections.Generic;
using System.Linq;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class SetAssertion<T> : SubjectAssertion<IReadOnlyList<T>>
{
    internal SetAssertion(string context, IReadOnlyList<T> subject, IEnumerable<T> values, bool exact)
        : base( context, subject )
    {
        Values = values as IReadOnlyList<T> ?? values.ToArray();
        Exact = exact;
    }

    internal IReadOnlyList<T> Values { get; }
    internal bool Exact { get; }

    public override void Go()
    {
        var elements = Subject.Select( (e, i) => (Element: e, Index: i) ).ToList();
        var missing = Values.Except( elements.Select( x => x.Element ) ).ToList();

        if ( Exact )
        {
            var excess = elements.Select( x => x.Element ).Except( Values ).ToList();
            if ( missing.Count == 0 && excess.Count == 0 )
                return;

            var errors = new List<string>();
            if ( missing.Count > 0 )
                errors.Add(
                    $"""
                     Found {missing.Count} missing element(s):
                     {string.Join( Environment.NewLine, missing.Select( (e, i) => $"{i + 1}. {$"'{e}'".Indent()}" ) )}
                     """ );

            if ( excess.Count > 0 )
                errors.Add(
                    $"""
                     Found {excess.Count} excess element(s):
                     {string.Join( Environment.NewLine, excess.Select( (e, i) => $"{i + 1}. {$"'{e}'".Indent()}" ) )}
                     """ );

            Throw(
                $"""
                 [{Context}] should be set equal to the provided collection.
                 {string.Join( $"{Environment.NewLine}{Environment.NewLine}", errors )}

                 Collection of {elements.Count} element(s):
                 {string.Join( Environment.NewLine, elements.Select( x => $"[@{x.Index}] {$"'{x.Element}'".Indent()}" ) )}
                 """ );
        }
        else if ( missing.Count > 0 )
            Throw(
                $"""
                 [{Context}] should contain all provided elements but found {missing.Count} missing element(s):
                 {string.Join( Environment.NewLine, missing.Select( (e, i) => $"{i + 1}. {$"'{e}'".Indent()}" ) )}

                 Collection of {elements.Count} element(s):
                 {string.Join( Environment.NewLine, elements.Select( x => $"[@{x.Index}] {$"'{x.Element}'".Indent()}" ) )}
                 """ );
    }
}
