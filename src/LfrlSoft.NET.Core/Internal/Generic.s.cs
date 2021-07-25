using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Internal
{
    public static class Generic<T>
    {
        public static readonly bool IsReferenceType = ! typeof( T ).IsValueType;
        public static readonly bool IsValueType = typeof( T ).IsValueType;
        public static readonly bool IsNullableType = Nullable.GetUnderlyingType( typeof( T ) ) is not null;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNull(T? obj)
        {
            if ( IsReferenceType )
                return ReferenceEquals( obj, null );

            return IsNullableType && obj!.Equals( default );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNotNull(T? obj)
        {
            return ! IsNull( obj );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsDefault(T? obj)
        {
            if ( IsReferenceType )
                return ReferenceEquals( obj, null );

            return EqualityComparer<T>.Default.Equals( obj!, default! );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNotDefault(T? obj)
        {
            return ! IsDefault( obj );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool AreEqual(T? a, T? b)
        {
            return EqualityComparer<T>.Default.Equals( a!, b! );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool AreNotEqual(T? a, T? b)
        {
            return ! AreEqual( a, b );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static string ToString(T? obj)
        {
            return IsNull( obj ) ? string.Empty : obj!.ToString();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int CreateHashCode(T? obj)
        {
            return IsNull( obj ) ? 0 : obj!.GetHashCode();
        }
    }
}
