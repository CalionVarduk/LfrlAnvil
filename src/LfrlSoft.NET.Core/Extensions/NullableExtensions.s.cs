namespace LfrlSoft.NET.Core.Extensions
{
    public static class NullableExtensions
    {
        public static T? ToNullable<T>(this T source)
            where T : struct
        {
            return source;
        }
    }
}
