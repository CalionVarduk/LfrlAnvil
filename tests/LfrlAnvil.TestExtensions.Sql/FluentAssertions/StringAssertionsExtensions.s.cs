using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions.Primitives;

namespace LfrlAnvil.TestExtensions.Sql.FluentAssertions;

public static class StringAssertionsExtensions
{
    private static readonly Regex NewLineRegex = new Regex( "\\r{0,1}\\n(\\[[ ]\\])*", RegexOptions.Compiled );

    public static AndConstraint<StringAssertions> SatisfySql(this StringAssertions assertions, params string[] expected)
    {
        var regexes = new Regex[expected.Length];
        for ( var i = 0; i < expected.Length; ++i )
        {
            var pattern = NewLineRegex.Replace(
                expected[i]
                    .Replace( " ", "[ ]" )
                    .Replace( "{GUID}", "[0-9a-fA-F]{32}" )
                    .Replace( "(", "\\(" )
                    .Replace( ")", "\\)" )
                    .Replace( ".", "\\." )
                    .Replace( "*", "\\*" )
                    .Replace( "+", "\\+" ),
                "\\r{0,1}\\n[ ]*" );

            regexes[i] = new Regex( '^' + pattern + '$', RegexOptions.Singleline );
        }

        var text = assertions.Subject;
        var statements = text?.Split( ';' ).Select( s => s.Trim() + ';' ).Where( s => s.Length > 1 ).ToArray() ?? Array.Empty<string>();

        statements.Should().HaveSameCount( regexes );

        statements.Zip( regexes )
            .Should()
            .OnlyContain( x => x.Second.IsMatch( x.First ) );

        return new AndConstraint<StringAssertions>( assertions );
    }
}
