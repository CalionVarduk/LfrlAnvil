namespace LfrlAnvil.TestExtensions.Assertions;

internal sealed class ContinuationAssertion<T> : SubjectAssertion<T>
{
    internal ContinuationAssertion(SubjectAssertion<T> @base, Func<T, Assertion> continuation)
        : base( $"[Continuation]: {@base.Context}", @base.Subject )
    {
        Base = @base;
        Continuation = continuation;
    }

    internal SubjectAssertion<T> Base { get; }
    internal Func<T, Assertion> Continuation { get; }

    public override void Go()
    {
        Base.Go();
        Continuation( Subject ).Go();
    }
}
