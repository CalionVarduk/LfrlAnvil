namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class RefEqualsAssertion<T> : Assertion
{
    internal RefEqualsAssertion(string context, T subject, object? value, bool expected)
        : base( context )
    {
        Subject = subject;
        Value = value;
        Expected = expected;
    }

    internal T Subject { get; }
    internal object? Value { get; }
    internal bool Expected { get; }

    public override void Go()
    {
        if ( Expected )
        {
            if ( ! ReferenceEquals( Subject, Value ) )
                Throw( $"[{Context}] should be ref-equal to '{Value}' but found '{Subject}'." );
        }
        else if ( ReferenceEquals( Subject, Value ) )
            Throw( $"[{Context}] should not be ref-equal to '{Value}'." );
    }
}
