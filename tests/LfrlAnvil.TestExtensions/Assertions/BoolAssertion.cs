namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class BoolAssertion : SubjectAssertion<bool>
{
    internal BoolAssertion(string context, bool subject, bool expected)
        : base( context, subject )
    {
        Expected = expected;
    }

    internal bool Expected { get; }

    public override void Go()
    {
        if ( Subject != Expected )
            Throw( $"[{Context}] is {Subject}." );
    }
}
