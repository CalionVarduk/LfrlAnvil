using System;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Common.Internal
{
    public static class Generic<T>
    {
        public static readonly bool IsReferenceType = !typeof( T ).IsValueType;
        public static readonly bool IsValueType = typeof( T ).IsValueType;
        public static readonly bool IsNullableType = !(Nullable.GetUnderlyingType( typeof( T ) ) is null);
        public static readonly bool IsEnumType = typeof( T ).IsEnum;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNull(T obj)
        {
            return (IsReferenceType && ReferenceEquals( obj, null )) || (IsNullableType && obj.Equals( default( T ) ));
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNotNull(T obj)
        {
            return !IsNull( obj );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsDefault(T obj)
        {
            return (IsReferenceType && ReferenceEquals( obj, null )) || (IsValueType && obj.Equals( default( T ) ));
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNotDefault(T obj)
        {
            return !IsDefault( obj );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool AreEqual(T a, T b)
        {
            if ( IsNull( a ) )
                return IsNull( b );

            return !IsNull( b ) && a.Equals( b );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool AreNotEqual(T a, T b)
        {
            return !AreEqual( a, b );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static string ToString(T obj)
        {
            return IsNull( obj ) ? string.Empty : obj.ToString();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int CreateHashCode(T obj)
        {
            return IsNull( obj ) ? 0 : obj.GetHashCode();
        }
    }
}
