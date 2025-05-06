namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class LikeAssertion : SubjectAssertion<ReadOnlyMemory<char>>
{
    internal enum ComparisonType : byte
    {
        StartsWith,
        EndsWith,
        Contains
    }

    internal LikeAssertion(
        string context,
        ReadOnlyMemory<char> subject,
        ReadOnlyMemory<char> value,
        StringComparison comparison,
        ComparisonType type)
        : base( context, subject )
    {
        Value = value;
        Comparison = comparison;
        Type = type;
    }

    internal ReadOnlyMemory<char> Value { get; }
    internal StringComparison Comparison { get; }
    internal ComparisonType Type { get; }

    public override void Go()
    {
        switch ( Type )
        {
            case ComparisonType.StartsWith:
                if ( ! Subject.Span.StartsWith( Value.Span, Comparison ) )
                    Throw( $"[{Context}] should start with {Value.Stringify()} but found {Subject.Stringify()}." );

                break;

            case ComparisonType.EndsWith:
                if ( ! Subject.Span.EndsWith( Value.Span, Comparison ) )
                    Throw( $"[{Context}] should end with {Value.Stringify()} but found {Subject.Stringify()}." );

                break;

            case ComparisonType.Contains:
                if ( ! Subject.Span.Contains( Value.Span, Comparison ) )
                    Throw( $"[{Context}] should contain {Value.Stringify()} but found {Subject.Stringify()}." );

                break;
        }
    }
}
