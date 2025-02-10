using System.Text.RegularExpressions;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class RegexAssertion : Assertion
{
    internal RegexAssertion(string context, ReadOnlyMemory<char> subject, Regex regex, bool match)
        : base( context )
    {
        Subject = subject;
        Regex = regex;
        Match = match;
    }

    internal ReadOnlyMemory<char> Subject { get; }
    internal Regex Regex { get; }
    internal bool Match { get; }

    public override void Go()
    {
        if ( Match )
        {
            if ( ! Regex.IsMatch( Subject.Span ) )
                Throw( $"[{Context}] should match regex '{Regex}' but found '{Subject}'." );
        }
        else if ( Regex.IsMatch( Subject.Span ) )
            Throw( $"[{Context}] should not match regex '{Regex}'." );
    }
}
