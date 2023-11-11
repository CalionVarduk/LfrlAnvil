using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Extensions;

public static class TypeExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsConstructable(this Type type)
    {
        return ! type.IsAbstract && ! type.IsGenericTypeDefinition && ! type.ContainsGenericParameters;
    }

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
            .Where( static i => i.IsGenericType )
            .Select( static i => i.GetGenericTypeDefinition() )
            .Distinct();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Type? GetExtension(this Type type, Type baseType)
    {
        return type
            .Visit( static t => t.BaseType )
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
            .Visit( static t => t.BaseType )
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
            .Visit( static t => t.BaseType )
            .Where( static t => t.IsGenericType )
            .Select( static t => t.GetGenericTypeDefinition() )
            .Distinct();
    }

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
        Assume.Equals( type.IsGenericParameter, true );
        AppendDebugString( builder, type );

        if ( (type.GenericParameterAttributes & GenericParameterAttributes.Contravariant) != GenericParameterAttributes.None )
            builder.Append( " [in]" );
        else if ( (type.GenericParameterAttributes & GenericParameterAttributes.Covariant) != GenericParameterAttributes.None )
            builder.Append( " [out]" );

        return builder;
    }
}
