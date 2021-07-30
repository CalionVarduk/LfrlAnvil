using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class NullableExtensions
    {
        [Pure]
        public static T? ToNullable<T>(this T source)
            where T : struct
        {
            return source;
        }
    }
}
