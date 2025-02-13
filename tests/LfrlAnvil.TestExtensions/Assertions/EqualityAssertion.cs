using System.Collections.Generic;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class EqualityAssertion<T> : SubjectAssertion<T>
{
    internal EqualityAssertion(string context, T subject, T value, bool expected)
        : base( context, subject )
    {
        Value = value;
        Expected = expected;
    }

    internal T Value { get; }
    internal bool Expected { get; }

    public override void Go()
    {
        var areEqual = EqualityComparer<T>.Default.Equals( Subject, Value );
        if ( Expected )
        {
            if ( ! areEqual )
                Throw( $"[{Context}] should be equal to '{Value}' but found '{Subject}'." );
        }
        else if ( areEqual )
            Throw( $"[{Context}] is equal to '{Value}'." );
    }
}
