namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class NullAssertion<T> : Assertion
{
    internal NullAssertion(string context, T subject, bool expected)
        : base( context )
    {
        Subject = subject;
        Expected = expected;
    }

    internal T Subject { get; }
    internal bool Expected { get; }

    public override void Go()
    {
        if ( Expected )
        {
            if ( Subject is not null )
                Throw( $"[{Context}] should be null but found '{Subject}'." );
        }
        else if ( Subject is null )
            Throw( $"[{Context}] is null." );
    }
}
