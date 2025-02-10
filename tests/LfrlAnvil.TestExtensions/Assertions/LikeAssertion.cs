namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class LikeAssertion : Assertion
{
    internal enum ComparisonType : byte
    {
        StartsWith,
        Contains
    }

    internal LikeAssertion(
        string context,
        ReadOnlyMemory<char> subject,
        ReadOnlyMemory<char> value,
        StringComparison comparison,
        ComparisonType type)
        : base( context )
    {
        Subject = subject;
        Value = value;
        Comparison = comparison;
        Type = type;
    }

    internal ReadOnlyMemory<char> Subject { get; }
    internal ReadOnlyMemory<char> Value { get; }
    internal StringComparison Comparison { get; }
    internal ComparisonType Type { get; }

    public override void Go()
    {
        switch ( Type )
        {
            case ComparisonType.StartsWith:
                if ( ! Subject.Span.StartsWith( Value.Span, Comparison ) )
                    Throw( $"[{Context}] should start with '{Value}' but found '{Subject}'." );

                break;

            case ComparisonType.Contains:
                if ( ! Subject.Span.Contains( Value.Span, Comparison ) )
                    Throw( $"[{Context}] should contain '{Value}' but found '{Subject}'." );

                break;
        }
    }
}
