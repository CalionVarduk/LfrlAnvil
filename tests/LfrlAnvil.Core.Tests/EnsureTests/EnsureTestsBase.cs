namespace LfrlAnvil.Tests.EnsureTests;

public abstract class EnsureTestsBase : TestsBase
{
    protected static void ShouldPass(Action action)
    {
        action.Test( exc => exc.TestNull() ).Go();
    }

    protected static void ShouldThrowArgumentException(Action action)
    {
        ShouldThrowExactly<ArgumentException>( action );
    }

    protected static void ShouldThrowExactly<TException>(Action action)
        where TException : Exception
    {
        action.Test( exc => exc.TestType().Exact<TException>() ).Go();
    }
}
