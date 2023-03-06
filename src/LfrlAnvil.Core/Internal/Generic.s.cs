using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Internal;

public static class Generic<T>
{
    public static readonly bool IsReferenceType = ! typeof( T ).IsValueType;
    public static readonly bool IsValueType = typeof( T ).IsValueType;
    public static readonly bool IsNullableType = Nullable.GetUnderlyingType( typeof( T ) ) is not null;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNull([NotNullWhen( false )] T? obj)
    {
        if ( IsReferenceType )
            return ReferenceEquals( obj, null );

        return IsNullableType && EqualityComparer<T>.Default.Equals( obj, default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNotNull([NotNullWhen( true )] T? obj)
    {
        return ! IsNull( obj );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsDefault([NotNullWhen( false )] T? obj)
    {
        if ( IsReferenceType )
            return ReferenceEquals( obj, null );

        return EqualityComparer<T>.Default.Equals( obj, default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNotDefault([NotNullWhen( true )] T? obj)
    {
        return ! IsDefault( obj );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool AreEqual(T? a, T? b)
    {
        return EqualityComparer<T>.Default.Equals( a, b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool AreNotEqual(T? a, T? b)
    {
        return ! AreEqual( a, b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ToString(T? obj)
    {
        return IsNull( obj ) ? string.Empty : obj.ToString()!;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int CreateHashCode(T? obj)
    {
        return IsNull( obj ) ? 0 : EqualityComparer<T>.Default.GetHashCode( obj );
    }
}
