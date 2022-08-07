using FluentAssertions.Execution;

namespace LfrlAnvil.TestExtensions;

public sealed class TestActedOn<T>
{
    public readonly ArrangedTest<T> Test;
    public readonly Action<T> Act;

    internal TestActedOn(ArrangedTest<T> test, Action<T> act)
    {
        Test = test;
        Act = act;
    }

    public void Assert(Action<T> asserter)
    {
        var data = Test.DataProvider();
        Act( data );

        using ( new AssertionScope() )
        {
            asserter( data );
        }
    }
}

public sealed class TestActedOn<T, TResult>
{
    public readonly ArrangedTest<T> Test;
    public readonly Func<T, TResult> Act;

    internal TestActedOn(ArrangedTest<T> test, Func<T, TResult> act)
    {
        Test = test;
        Act = act;
    }

    public void Assert(Action<(T Data, TResult Result)> asserter)
    {
        var data = Test.DataProvider();
        var result = Act( data );

        using ( new AssertionScope() )
        {
            asserter( (data, result) );
        }
    }
}