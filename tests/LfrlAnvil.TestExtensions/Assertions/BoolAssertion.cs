namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class BoolAssertion : Assertion
{
    internal BoolAssertion(string context, bool subject, bool expected)
        : base( context )
    {
        Subject = subject;
        Expected = expected;
    }

    internal bool Subject { get; }
    internal bool Expected { get; }

    public override void Go()
    {
        if ( Subject != Expected )
            Throw( $"[{Context}] is {Subject}." );
    }
}
