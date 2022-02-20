using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions
{
    public static class TypeExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type? GetImplementation(this Type type, Type interfaceType)
        {
            return type
                .GetInterfaces()
                .FirstOrDefault( i => i == interfaceType );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type? GetImplementation<T>(this Type type)
            where T : class
        {
            return type.GetImplementation( typeof( T ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool Implements(this Type type, Type interfaceType)
        {
            return type.GetImplementation( interfaceType ) is not null;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool Implements<T>(this Type type)
            where T : class
        {
            return type.Implements( typeof( T ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<Type> GetOpenGenericImplementations(this Type type, Type openGenericInterfaceDefinition)
        {
            return type.GetInterfaces()
                .Where( i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterfaceDefinition );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool ImplementsOpenGeneric(this Type type, Type openGenericInterfaceDefinition)
        {
            return type.GetOpenGenericImplementations( openGenericInterfaceDefinition ).Any();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<Type> GetAllImplementedGenericDefinitions(this Type type)
        {
            return type.GetInterfaces()
                .Where( i => i.IsGenericType )
                .Select( i => i.GetGenericTypeDefinition() )
                .Distinct();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type? GetExtension(this Type type, Type baseType)
        {
            return type
                .Visit( t => t.BaseType )
                .FirstOrDefault( t => t == baseType );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type? GetExtension<T>(this Type type)
        {
            return type.GetExtension( typeof( T ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool Extends(this Type type, Type baseType)
        {
            return type.GetExtension( baseType ) is not null;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool Extends<T>(this Type type)
        {
            return type.Extends( typeof( T ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type? GetOpenGenericExtension(this Type type, Type openGenericBaseTypeDefinition)
        {
            return type
                .Visit( t => t.BaseType )
                .FirstOrDefault( t => t.IsGenericType && t.GetGenericTypeDefinition() == openGenericBaseTypeDefinition );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool ExtendsOpenGeneric(this Type type, Type openGenericBaseTypeDefinition)
        {
            return type.GetOpenGenericExtension( openGenericBaseTypeDefinition ) is not null;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEnumerable<Type> GetAllExtendedGenericDefinitions(this Type type)
        {
            return type
                .Visit( t => t.BaseType )
                .Where( t => t.IsGenericType )
                .Select( t => t.GetGenericTypeDefinition() )
                .Distinct();
        }
    }
}
