using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace LfrlAnvil.TestExtensions.Sql.Assertions;

public static class AssertionsExtensions
{
    private static readonly Regex NewLineRegex = new Regex( "\\r{0,1}\\n(\\[[ ]\\])*", RegexOptions.Compiled );

    // TODO: rename to TestSatisfySql
    [Pure]
    public static Assertion SatisfySql(this string? subject, params string[] expected)
    {
        var regexes = new Regex[expected.Length];
        for ( var i = 0; i < expected.Length; ++i )
        {
            var pattern = NewLineRegex.Replace(
                expected[i]
                    .Replace( " ", "[ ]" )
                    .Replace( "{GUID}", "[0-9a-fA-F]{32}" )
                    .Replace( "\\", "\\\\" )
                    .Replace( "(", "\\(" )
                    .Replace( ")", "\\)" )
                    .Replace( ".", "\\." )
                    .Replace( "*", "\\*" )
                    .Replace( "+", "\\+" ),
                "\\r{0,1}\\n[ ]*" );

            regexes[i] = new Regex( '^' + pattern + '$', RegexOptions.Singleline );
        }

        var statements = subject?.Split( ';' ).Select( s => s.Trim() + ';' ).Where( s => s.Length > 1 ).ToArray() ?? Array.Empty<string>();
        return statements.TestSequence(
            regexes.Select( r => ( Func<string, int, Assertion> )((statement, _) => statement.TestMatch( r )) ) );
    }
}
