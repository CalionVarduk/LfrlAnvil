using NSubstitute.Exceptions;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class ReceivedCallsAssertion<T> : Assertion
    where T : class
{
    internal ReceivedCallsAssertion(string context, string subContext, T subject, Action<T> assertion, int? count)
        : base( context )
    {
        SubContext = subContext;
        Subject = subject;
        Assertion = assertion;
        Count = count;
    }

    internal string SubContext { get; }
    internal T Subject { get; }
    internal Action<T> Assertion { get; }
    internal int? Count { get; }

    public override void Go()
    {
        if ( Count is null )
        {
            try
            {
                Assertion( Subject.Received() );
            }
            catch ( ReceivedCallsException )
            {
                Throw( $"[{Context}] => [{SubContext}] did not receive any calls." );
            }
        }
        else if ( Count.Value <= 0 )
        {
            try
            {
                Assertion( Subject.DidNotReceive() );
            }
            catch ( ReceivedCallsException )
            {
                Throw( $"[{Context}] => [{SubContext}] received at least one call." );
            }
        }
        else
        {
            try
            {
                Assertion( Subject.Received( Count.Value ) );
            }
            catch ( ReceivedCallsException )
            {
                Throw( $"[{Context}] => [{SubContext}] did not receive expected {Count.Value} call(s)." );
            }
        }
    }
}
