using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Functional.Exceptions;

internal static class Resources
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingFirstEitherValue<T1, T2>()
    {
        return $"{GetEitherName<T1, T2>()} instance doesn't have the {nameof( Either<T1, T2>.First )} value.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingSecondEitherValue<T1, T2>()
    {
        return $"{GetEitherName<T1, T2>()} instance doesn't have the {nameof( Either<T1, T2>.Second )} value.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingUnsafeValue<T>()
    {
        return $"{GetUnsafeName<T>()} instance doesn't contain a {nameof( Unsafe<T>.Value )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingUnsafeError<T>()
    {
        return $"{GetUnsafeName<T>()} instance doesn't contain an {nameof( Unsafe<T>.Error )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingMaybeValue<T>()
        where T : notnull
    {
        return $"{GetMaybeName<T>()} instance doesn't contain a {nameof( Maybe<T>.Value )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingTypeCastResult<TSource, TDestination>()
    {
        return
            $"{GetTypeCastName<TSource, TDestination>()} instance doesn't contain a valid {nameof( TypeCast<TSource, TDestination>.Result )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetEitherName<T1, T2>()
    {
        return $"{nameof( Either )}<{typeof( T1 ).GetDebugString()}, {typeof( T2 ).GetDebugString()}>";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetUnsafeName<T>()
    {
        return $"{nameof( Unsafe )}<{typeof( T ).GetDebugString()}>";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetMaybeName<T>()
    {
        return $"{nameof( Maybe )}<{typeof( T ).GetDebugString()}>";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetTypeCastName<TSource, TDestination>()
    {
        return $"{nameof( TypeCast )}<{typeof( TSource ).GetDebugString()}, {typeof( TDestination ).GetDebugString()}>";
    }
}
