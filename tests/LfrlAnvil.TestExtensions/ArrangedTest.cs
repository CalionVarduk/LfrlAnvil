using System;

namespace LfrlAnvil.TestExtensions
{
    public sealed class ArrangedTest<T>
    {
        public readonly Func<T> DataProvider;

        internal ArrangedTest(Func<T> dataProvider)
        {
            DataProvider = dataProvider;
        }

        public TestActedOn<T> Act(Action<T> act)
        {
            return new TestActedOn<T>( this, act );
        }

        public TestActedOn<T, TResult> Act<TResult>(Func<T, TResult> act)
        {
            return new TestActedOn<T, TResult>( this, act );
        }
    }
}
