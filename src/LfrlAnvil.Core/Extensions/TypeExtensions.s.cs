// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Type"/> extension methods.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Checks whether or not the given <paramref name="type"/> is constructable.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns><b>true</b> when <paramref name="type"/> is constructable, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// <see cref="Type"/> is considered to be constructable when it is not abstract and it is not a generic type definition
    /// and it does not contain any generic parameters.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsConstructable(this Type type)
    {
        return ! type.IsAbstract && ! type.IsGenericTypeDefinition && ! type.ContainsGenericParameters;
    }

    /// <summary>
    /// Attempts to find an implementation of the specified interface in the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <param name="interfaceType">Type of an interface to get an implementation for.</param>
    /// <returns>Interface's implementation type when the given <paramref name="type"/> implements it, otherwise null.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Type? GetImplementation(this Type type, Type interfaceType)
    {
        return type
            .GetInterfaces()
            .FirstOrDefault( i => i == interfaceType );
    }

    /// <summary>
    /// Attempts to find an implementation of the specified interface in the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <typeparam name="T">Type of an interface to get an implementation for.</typeparam>
    /// <returns>Interface's implementation type when the given <paramref name="type"/> implements it, otherwise null.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Type? GetImplementation<T>(this Type type)
        where T : class
    {
        return type.GetImplementation( typeof( T ) );
    }

    /// <summary>
    /// Checks whether or not the specified interface is implemented by the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <param name="interfaceType">Type of an interface to check.</param>
    /// <returns><b>true</b> when the given <paramref name="type"/> implements the interface, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Implements(this Type type, Type interfaceType)
    {
        return type.GetImplementation( interfaceType ) is not null;
    }

    /// <summary>
    /// Checks whether or not the specified interface is implemented by the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <typeparam name="T">Type of an interface to check.</typeparam>
    /// <returns><b>true</b> when the given <paramref name="type"/> implements the interface, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Implements<T>(this Type type)
        where T : class
    {
        return type.Implements( typeof( T ) );
    }

    /// <summary>
    /// Finds all implementations of the specified open generic interface definition in the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <param name="openGenericInterfaceDefinition">Type of an open generic interface definition to get implementations for.</param>
    /// <returns>
    /// Collection of all implementations of the specified open generic interface definition in the given <paramref name="type"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<Type> GetOpenGenericImplementations(this Type type, Type openGenericInterfaceDefinition)
    {
        return type.GetInterfaces()
            .Where( i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterfaceDefinition );
    }

    /// <summary>
    /// Checks whether or not the specified open generic interface definition is implemented by the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <param name="openGenericInterfaceDefinition">Type of an open generic interface definition to check.</param>
    /// <returns><b>true</b> when the given <paramref name="type"/> implements the interface, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ImplementsOpenGeneric(this Type type, Type openGenericInterfaceDefinition)
    {
        return type.GetOpenGenericImplementations( openGenericInterfaceDefinition ).Any();
    }

    /// <summary>
    /// Finds all implementations of all open generic interface definitions in the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <returns>Collection of all implementations of all open generic interface definitions in the given <paramref name="type"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<Type> GetAllImplementedGenericDefinitions(this Type type)
    {
        return type.GetInterfaces()
            .Where( static i => i.IsGenericType )
            .Select( static i => i.GetGenericTypeDefinition() )
            .Distinct();
    }

    /// <summary>
    /// Attempts to find an extension of the specified base type in the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <param name="baseType">Base type to get an extension for.</param>
    /// <returns>Base type's extension type when the given <paramref name="type"/> extends it, otherwise null.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Type? GetExtension(this Type type, Type baseType)
    {
        return type
            .Visit( static t => t.BaseType )
            .FirstOrDefault( t => t == baseType );
    }

    /// <summary>
    /// Attempts to find an extension of the specified base type in the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <typeparam name="T">Base type to get an extension for.</typeparam>
    /// <returns>Base type's extension type when the given <paramref name="type"/> extends it, otherwise null.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Type? GetExtension<T>(this Type type)
    {
        return type.GetExtension( typeof( T ) );
    }

    /// <summary>
    /// Checks whether or not the specified base type is extended by the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <param name="baseType">Base type to check.</param>
    /// <returns><b>true</b> when the given <paramref name="type"/> extends the base type, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Extends(this Type type, Type baseType)
    {
        return type.GetExtension( baseType ) is not null;
    }

    /// <summary>
    /// Checks whether or not the specified base type is extended by the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <typeparam name="T">Base type to check.</typeparam>
    /// <returns><b>true</b> when the given <paramref name="type"/> extends the base type, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Extends<T>(this Type type)
    {
        return type.Extends( typeof( T ) );
    }

    /// <summary>
    /// Attempts to find an extension of the specified open generic base type definition in the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <param name="openGenericBaseTypeDefinition">Open generic base type definition to get an extension for.</param>
    /// <returns>
    /// Open generic base type definition's extension type when the given <paramref name="type"/> extends it, otherwise null.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Type? GetOpenGenericExtension(this Type type, Type openGenericBaseTypeDefinition)
    {
        return type
            .Visit( static t => t.BaseType )
            .FirstOrDefault( t => t.IsGenericType && t.GetGenericTypeDefinition() == openGenericBaseTypeDefinition );
    }

    /// <summary>
    /// Checks whether or not the specified open generic base type definition is extended by the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <param name="openGenericBaseTypeDefinition">Type of an open generic base type definition to check.</param>
    /// <returns><b>true</b> when the given <paramref name="type"/> extends the base type, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ExtendsOpenGeneric(this Type type, Type openGenericBaseTypeDefinition)
    {
        return type.GetOpenGenericExtension( openGenericBaseTypeDefinition ) is not null;
    }

    /// <summary>
    /// Finds all extensions of all open generic base type definitions in the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <returns>Collection of all extensions of all open generic base type definitions in the given <paramref name="type"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<Type> GetAllExtendedGenericDefinitions(this Type type)
    {
        return type
            .Visit( static t => t.BaseType )
            .Where( static t => t.IsGenericType )
            .Select( static t => t.GetGenericTypeDefinition() )
            .Distinct();
    }

    /// <summary>
    /// Attempts to find the first non-null member returned by the specified <paramref name="memberSelector"/>
    /// in the given <paramref name="type"/> or in all of its ancestors.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <param name="memberSelector">
    /// Member selector invoked for the given <paramref name="type"/> or for all of its ancestors, until it returns a non-null result.
    /// </param>
    /// <typeparam name="T">Member type.</typeparam>
    /// <returns>Found member, if it exists, otherwise null.</returns>
    [Pure]
    public static T? FindMember<T>(this Type type, Func<Type, T?> memberSelector)
        where T : class
    {
        var t = type;
        do
        {
            var member = memberSelector( t );
            if ( member is not null )
                return member;

            t = t.BaseType;
        }
        while ( t is not null );

        var interfaces = type.GetInterfaces();
        foreach ( var i in interfaces )
        {
            var member = memberSelector( i );
            if ( member is not null )
                return member;
        }

        return null;
    }

    /// <summary>
    /// Creates a string representation of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Source type.</param>
    /// <returns>String representation of the provided <paramref name="type"/>.</returns>
    [Pure]
    public static string GetDebugString(this Type type)
    {
        return AppendDebugString( new StringBuilder(), type ).ToString();
    }

    internal static StringBuilder AppendDebugString(StringBuilder builder, Type type)
    {
        if ( ! type.IsNested )
            builder.Append( type.Namespace ).Append( '.' );
        else if ( ! type.IsGenericParameter && type.DeclaringType is not null )
            AppendDebugString( builder, type.DeclaringType ).Append( '+' );

        builder.Append( type.Name );

        if ( ! type.IsGenericType )
            return builder;

        Type[] openGenericArgs;
        var closedGenericArgs = type.GetGenericArguments();

        if ( type.IsGenericTypeDefinition )
        {
            openGenericArgs = closedGenericArgs;
            closedGenericArgs = Type.EmptyTypes;
        }
        else
            openGenericArgs = type.GetGenericTypeDefinition().GetGenericArguments();

        return AppendGenericArgumentsString( builder, openGenericArgs, closedGenericArgs );
    }

    internal static StringBuilder AppendGenericArgumentsString(StringBuilder builder, Type[] openGenericArgs, Type[] closedGenericArgs)
    {
        Assume.IsNotEmpty( openGenericArgs );

        builder.Append( '[' );
        if ( closedGenericArgs.Length == 0 )
        {
            foreach ( var arg in openGenericArgs )
                AppendOpenGenericArgumentString( builder, arg ).Append( ", " );
        }
        else
        {
            Assume.ContainsExactly( closedGenericArgs, openGenericArgs.Length );

            for ( var i = 0; i < openGenericArgs.Length; ++i )
            {
                AppendOpenGenericArgumentString( builder, openGenericArgs[i] ).Append( " is " );
                AppendDebugString( builder, closedGenericArgs[i] ).Append( ", " );
            }
        }

        builder.ShrinkBy( 2 ).Append( ']' );
        return builder;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StringBuilder AppendOpenGenericArgumentString(StringBuilder builder, Type type)
    {
        Assume.True( type.IsGenericParameter );
        AppendDebugString( builder, type );

        if ( (type.GenericParameterAttributes & GenericParameterAttributes.Contravariant) != GenericParameterAttributes.None )
            builder.Append( " [in]" );
        else if ( (type.GenericParameterAttributes & GenericParameterAttributes.Covariant) != GenericParameterAttributes.None )
            builder.Append( " [out]" );

        return builder;
    }
}
