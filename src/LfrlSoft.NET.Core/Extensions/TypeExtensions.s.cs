using System;
using System.Collections.Generic;
using System.Linq;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class TypeExtensions
    {
        public static Type? GetImplementation(this Type type, Type interfaceType)
        {
            return type
                .GetInterfaces()
                .FirstOrDefault( i => i == interfaceType );
        }

        public static Type? GetImplementation<T>(this Type type)
            where T : class
        {
            return type.GetImplementation( typeof( T ) );
        }

        public static bool Implements(this Type type, Type interfaceType)
        {
            return type.GetImplementation( interfaceType ) is not null;
        }

        public static bool Implements<T>(this Type type)
            where T : class
        {
            return type.Implements( typeof( T ) );
        }

        public static IEnumerable<Type> GetOpenGenericImplementations(this Type type, Type openGenericInterfaceDefinition)
        {
            return type.GetInterfaces()
                .Where( i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterfaceDefinition );
        }

        public static bool ImplementsOpenGeneric(this Type type, Type openGenericInterfaceDefinition)
        {
            return type.GetOpenGenericImplementations( openGenericInterfaceDefinition ).Any();
        }

        public static IEnumerable<Type> GetAllImplementedGenericDefinitions(this Type type)
        {
            return type.GetInterfaces()
                .Where( i => i.IsGenericType )
                .Select( i => i.GetGenericTypeDefinition() )
                .Distinct();
        }

        public static Type? GetExtension(this Type type, Type baseType)
        {
            return type
                .Visit( t => t.BaseType )
                .FirstOrDefault( t => t == baseType );
        }

        public static Type? GetExtension<T>(this Type type)
        {
            return type.GetExtension( typeof( T ) );
        }

        public static bool Extends(this Type type, Type baseType)
        {
            return type.GetExtension( baseType ) is not null;
        }

        public static bool Extends<T>(this Type type)
        {
            return type.Extends( typeof( T ) );
        }

        public static Type? GetOpenGenericExtension(this Type type, Type openGenericBaseTypeDefinition)
        {
            return type
                .Visit( t => t.BaseType )
                .FirstOrDefault( t => t.IsGenericType && t.GetGenericTypeDefinition() == openGenericBaseTypeDefinition );
        }

        public static bool ExtendsOpenGeneric(this Type type, Type openGenericBaseTypeDefinition)
        {
            return type.GetOpenGenericExtension( openGenericBaseTypeDefinition ) is not null;
        }

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
