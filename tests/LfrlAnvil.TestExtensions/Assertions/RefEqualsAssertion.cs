namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class RefEqualsAssertion<T> : SubjectAssertion<T>
{
    internal RefEqualsAssertion(string context, T subject, object? value, bool expected)
        : base( context, subject )
    {
        Value = value;
        Expected = expected;
    }

    internal object? Value { get; }
    internal bool Expected { get; }

    public override void Go()
    {
        if ( Expected )
        {
            if ( ! ReferenceEquals( Subject, Value ) )
                Throw( $"[{Context}] should be ref-equal to {Value.Stringify()} but found {Subject.Stringify()}." );
        }
        else if ( ReferenceEquals( Subject, Value ) )
            Throw( $"[{Context}] should not be ref-equal to {Value.Stringify()}." );
    }
}
