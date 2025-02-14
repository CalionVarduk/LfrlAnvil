using Xunit.Sdk;

namespace LfrlAnvil.TestExtensions.Assertions;

public sealed class CallAssertion : SubjectAssertion<Action>
{
    internal CallAssertion(string context, Action subject, Func<Exception?, Assertion> completionAssertion)
        : base( context, subject )
    {
        CompletionAssertion = completionAssertion;
    }

    internal Func<Exception?, Assertion> CompletionAssertion { get; }

    public Assertion Invoke()
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

        return new Invocation( Context, CompletionAssertion( exception ) );
    }

    public override void Go()
    {
        var assertion = Invoke();
        assertion.Go();
    }

    private sealed class Invocation : Assertion
    {
        internal Invocation(string context, Assertion @base)
            : base( context )
        {
            Base = @base;
        }

        internal Assertion Base { get; }

        public override void Go()
        {
            try
            {
                Base.Go();
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
}
