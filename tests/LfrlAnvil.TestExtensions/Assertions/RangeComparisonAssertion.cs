using System.Collections.Generic;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class RangeComparisonAssertion<T> : Assertion
{
    internal RangeComparisonAssertion(string context, T subject, T min, T max, bool expected)
        : base( context )
    {
        Subject = subject;
        Min = min;
        Max = max;
        Expected = expected;
    }

    internal T Subject { get; }
    internal T Min { get; }
    internal T Max { get; }
    internal bool Expected { get; }

    public override void Go()
    {
        if ( Expected )
        {
            if ( Comparer<T>.Default.Compare( Subject, Min ) < 0 || Comparer<T>.Default.Compare( Subject, Max ) > 0 )
                Throw( $"[{Context}] should be in ['{Min}', '{Max}'] range but found '{Subject}'." );
        }
        else if ( Comparer<T>.Default.Compare( Subject, Min ) >= 0 && Comparer<T>.Default.Compare( Subject, Max ) <= 0 )
            Throw( $"[{Context}] should not be in ['{Min}', '{Max}'] range but found '{Subject}'." );
    }
}
