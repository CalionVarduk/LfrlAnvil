using Xunit.Sdk;

namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class CallAssertion : Assertion
{
    internal CallAssertion(string context, Action subject, Func<Exception?, Assertion> completionAssertion)
        : base( context )
    {
        Subject = subject;
        CompletionAssertion = completionAssertion;
    }

    internal Action Subject { get; }
    internal Func<Exception?, Assertion> CompletionAssertion { get; }

    public override void Go()
    {
        Exception? exception = null;
        try
        {
            Subject();
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        try
        {
            CompletionAssertion( exception ).Go();
        }
        catch ( XunitException exc )
        {
            Throw(
                $"""
                 [{Context}] exception assertion failed:
                   {exc.Message.Indent()}
                 """ );
        }
    }
}
