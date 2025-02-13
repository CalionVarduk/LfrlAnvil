namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class ApproximationAssertion<T> : SubjectAssertion<T>
{
    internal ApproximationAssertion(string context, T subject, T value, T epsilon)
        : base( context, subject )
    {
        Value = value;
        Epsilon = epsilon;
    }

    internal T Value { get; }
    internal T Epsilon { get; }

    public override void Go()
    {
        var subject = ( dynamic? )Subject;
        var value = ( dynamic? )Value;
        var epsilon = ( dynamic? )Epsilon;

        var delta = subject - value;
        if ( delta < 0 )
            delta = -delta;

        if ( delta > epsilon )
            Throw( $"[{Context}] should be approximately equal to '{Value}' but found '{Subject}', using '{Epsilon}' epsilon." );
    }
}
