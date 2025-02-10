using System.Collections.Generic;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class ComparisonAssertion<T> : Assertion
{
    internal ComparisonAssertion(string context, T subject, T value, ComparisonType type)
        : base( context )
    {
        Subject = subject;
        Value = value;
        Type = type;
    }

    internal T Subject { get; }
    internal T Value { get; }
    internal ComparisonType Type { get; }

    public override void Go()
    {
        var result = Comparer<T>.Default.Compare( Subject, Value );
        switch ( Type )
        {
            case ComparisonType.GreaterThan:
                if ( result <= 0 )
                    Throw( $"[{Context}] should be greater than '{Value}' but found '{Subject}'." );

                break;
            case ComparisonType.GreaterThanOrEqualTo:
                if ( result < 0 )
                    Throw( $"[{Context}] should be greater than or equal to '{Value}' but found '{Subject}'." );

                break;
            case ComparisonType.LessThan:
                if ( result >= 0 )
                    Throw( $"[{Context}] should be less than '{Value}' but found '{Subject}'." );

                break;
            case ComparisonType.LessThanOrEqualTo:
                if ( result > 0 )
                    Throw( $"[{Context}] should be less than or equal to '{Value}' but found '{Subject}'." );

                break;
        }
    }
}
