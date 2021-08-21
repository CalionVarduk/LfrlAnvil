using System;

namespace LfrlSoft.NET.Core.Functional
{
    public static class Lambda<T>
    {
        public static readonly Func<T, T> Identity = x => x;
    }

    public static class Lambda
    {
        public static Func<TReturn> Of<TReturn>(Func<TReturn> func)
        {
            return func;
        }

        public static Func<T1, TReturn> Of<T1, TReturn>(Func<T1, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, TReturn> Of<T1, T2, TReturn>(Func<T1, T2, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, TReturn> Of<T1, T2, T3, TReturn>(Func<T1, T2, T3, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, TReturn> Of<T1, T2, T3, T4, TReturn>(Func<T1, T2, T3, T4, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, TReturn> Of<T1, T2, T3, T4, T5, TReturn>(Func<T1, T2, T3, T4, T5, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, TReturn> Of<T1, T2, T3, T4, T5, T6, TReturn>(Func<T1, T2, T3, T4, T5, T6, TReturn> func)
        {
            return func;
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, TReturn> Of<T1, T2, T3, T4, T5, T6, T7, TReturn>(
            Func<T1, T2, T3, T4, T5, T6, T7, TReturn> func)
        {
            return func;
        }

        public static Action Of(Action action)
        {
            return action;
        }

        public static Action<T1> Of<T1>(Action<T1> action)
        {
            return action;
        }

        public static Action<T1, T2> Of<T1, T2>(Action<T1, T2> action)
        {
            return action;
        }

        public static Action<T1, T2, T3> Of<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            return action;
        }

        public static Action<T1, T2, T3, T4> Of<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            return action;
        }

        public static Action<T1, T2, T3, T4, T5> Of<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            return action;
        }

        public static Action<T1, T2, T3, T4, T5, T6> Of<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
        {
            return action;
        }

        public static Action<T1, T2, T3, T4, T5, T6, T7> Of<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            return action;
        }
    }
}
