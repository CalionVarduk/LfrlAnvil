using NSubstitute.Exceptions;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class CallOrderAssertion : Assertion
{
    internal CallOrderAssertion(string context, Action order)
        : base( context )
    {
        Order = order;
    }

    internal Action Order { get; }

    public override void Go()
    {
        try
        {
            Received.InOrder( Order );
        }
        catch ( ReceivedCallsException )
        {
            Throw( $"{(Context.Length > 0 ? $"[{Context}]" : "Call order")} was not satisfied." );
        }
    }
}
