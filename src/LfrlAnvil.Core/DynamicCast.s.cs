using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

public static class DynamicCast
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? TryTo<T>(object? value)
        where T : class
    {
        return value as T;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    [return: NotNullIfNotNull( "value" )]
    public static T? To<T>(object? value)
        where T : class
    {
        return ( T? )value;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? TryUnbox<T>(object? value)
        where T : struct
    {
        return value is T t ? t : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T Unbox<T>(object? value)
        where T : struct
    {
        return ( T )value!;
    }
}
