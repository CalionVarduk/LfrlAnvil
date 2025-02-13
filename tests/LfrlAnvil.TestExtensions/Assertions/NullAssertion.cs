namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class NullAssertion<T> : SubjectAssertion<T>
{
    internal NullAssertion(string context, T subject, bool expected)
        : base( context, subject )
    {
        Expected = expected;
    }

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
