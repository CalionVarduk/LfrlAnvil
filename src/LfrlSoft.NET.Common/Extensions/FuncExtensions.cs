using System;

namespace LfrlSoft.NET.Common.Extensions
{
    public static class FuncExtensions
    {
        public static Lazy<T> ToLazy<T>(this Func<T> source)
        {
            return new Lazy<T>( source );
        }
    }
}
