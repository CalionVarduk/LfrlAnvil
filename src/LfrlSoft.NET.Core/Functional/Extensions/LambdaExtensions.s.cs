using System;

namespace LfrlSoft.NET.Core.Functional.Extensions
{
    public static class LambdaExtensions
    {
        public static Func<Nil> ToFunc(this Action action)
        {
            return () =>
            {
                action();
                return Nil.Instance;
            };
        }

        public static Func<T1, Nil> ToFunc<T1>(this Action<T1> action)
        {
            return a1 =>
            {
                action( a1 );
                return Nil.Instance;
            };
        }

        public static Func<T1, T2, Nil> ToFunc<T1, T2>(this Action<T1, T2> action)
        {
            return (a1, a2) =>
            {
                action( a1, a2 );
                return Nil.Instance;
            };
        }

        public static Func<T1, T2, T3, Nil> ToFunc<T1, T2, T3>(this Action<T1, T2, T3> action)
        {
            return (a1, a2, a3) =>
            {
                action( a1, a2, a3 );
                return Nil.Instance;
            };
        }

        public static Func<T1, T2, T3, T4, Nil> ToFunc<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action)
        {
            return (a1, a2, a3, a4) =>
            {
                action( a1, a2, a3, a4 );
                return Nil.Instance;
            };
        }

        public static Func<T1, T2, T3, T4, T5, Nil> ToFunc<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action)
        {
            return (a1, a2, a3, a4, a5) =>
            {
                action( a1, a2, a3, a4, a5 );
                return Nil.Instance;
            };
        }

        public static Func<T1, T2, T3, T4, T5, T6, Nil> ToFunc<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> action)
        {
            return (a1, a2, a3, a4, a5, a6) =>
            {
                action( a1, a2, a3, a4, a5, a6 );
                return Nil.Instance;
            };
        }

        public static Func<T1, T2, T3, T4, T5, T6, T7, Nil> ToFunc<T1, T2, T3, T4, T5, T6, T7>(
            this Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            return (a1, a2, a3, a4, a5, a6, a7) =>
            {
                action( a1, a2, a3, a4, a5, a6, a7 );
                return Nil.Instance;
            };
        }

        public static Action ToAction(this Func<Nil> func)
        {
            return () => func();
        }

        public static Action<T1> ToAction<T1>(this Func<T1, Nil> func)
        {
            return a1 => func( a1 );
        }

        public static Action<T1, T2> ToAction<T1, T2>(this Func<T1, T2, Nil> func)
        {
            return (a1, a2) => func( a1, a2 );
        }

        public static Action<T1, T2, T3> ToAction<T1, T2, T3>(this Func<T1, T2, T3, Nil> func)
        {
            return (a1, a2, a3) => func( a1, a2, a3 );
        }

        public static Action<T1, T2, T3, T4> ToAction<T1, T2, T3, T4>(this Func<T1, T2, T3, T4, Nil> func)
        {
            return (a1, a2, a3, a4) => func( a1, a2, a3, a4 );
        }

        public static Action<T1, T2, T3, T4, T5> ToAction<T1, T2, T3, T4, T5>(this Func<T1, T2, T3, T4, T5, Nil> func)
        {
            return (a1, a2, a3, a4, a5) => func( a1, a2, a3, a4, a5 );
        }

        public static Action<T1, T2, T3, T4, T5, T6> ToAction<T1, T2, T3, T4, T5, T6>(this Func<T1, T2, T3, T4, T5, T6, Nil> func)
        {
            return (a1, a2, a3, a4, a5, a6) => func( a1, a2, a3, a4, a5, a6 );
        }

        public static Action<T1, T2, T3, T4, T5, T6, T7> ToAction<T1, T2, T3, T4, T5, T6, T7>(
            this Func<T1, T2, T3, T4, T5, T6, T7, Nil> func)
        {
            return (a1, a2, a3, a4, a5, a6, a7) => func( a1, a2, a3, a4, a5, a6, a7 );
        }
    }
}
