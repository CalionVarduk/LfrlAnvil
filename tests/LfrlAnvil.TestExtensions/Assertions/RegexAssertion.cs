using System.Text.RegularExpressions;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class RegexAssertion : SubjectAssertion<ReadOnlyMemory<char>>
{
    internal RegexAssertion(string context, ReadOnlyMemory<char> subject, Regex regex, bool match)
        : base( context, subject )
    {
        Regex = regex;
        Match = match;
    }

    internal Regex Regex { get; }
    internal bool Match { get; }

    public override void Go()
    {
        if ( Match )
        {
            if ( ! Regex.IsMatch( Subject.Span ) )
                Throw( $"[{Context}] should match regex {Regex.Stringify()} but found {Subject.Stringify()}." );
        }
        else if ( Regex.IsMatch( Subject.Span ) )
            Throw( $"[{Context}] should not match regex {Regex.Stringify()}." );
    }
}
