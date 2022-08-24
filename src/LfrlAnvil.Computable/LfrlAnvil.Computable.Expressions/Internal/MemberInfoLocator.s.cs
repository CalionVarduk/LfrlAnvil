using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal static class MemberInfoLocator
{
    [Pure]
    internal static MethodInfo FindCompareToMethod(Type targetType, Type parameterType, Type constructType)
    {
        var result = targetType.GetMethod(
            name: nameof( IComparable.CompareTo ),
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            types: new[] { parameterType } );

        if ( result is null || result.GetParameters()[0].ParameterType != parameterType || result.ReturnType != typeof( int ) )
        {
            throw new ParsedExpressionConstructException(
                Resources.ConstructFailedToFindCompareToMethod( targetType, parameterType, constructType ),
                constructType );
        }

        return result;
    }

    [Pure]
    internal static MethodInfo FindStringCompareMethod()
    {
        var result = typeof( string ).GetMethod(
            name: nameof( string.Compare ),
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            types: new[] { typeof( string ), typeof( string ), typeof( StringComparison ) } );

        return result!;
    }

    [Pure]
    internal static MethodInfo FindStringConcatMethod()
    {
        var result = typeof( string ).GetMethod(
            name: nameof( string.Concat ),
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            types: new[] { typeof( string ), typeof( string ) } );

        return result!;
    }

    [Pure]
    internal static MethodInfo FindToStringMethod()
    {
        var result = typeof( object ).GetMethod(
            name: nameof( ToString ),
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            types: Array.Empty<Type>() );

        return result!;
    }

    [Pure]
    internal static MethodInfo FindToStringWithFormatProviderMethod(Type type)
    {
        var result = type.GetMethod(
            name: nameof( ToString ),
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            types: new[] { typeof( IFormatProvider ) } );

        return result!;
    }

    [Pure]
    internal static ConstructorInfo FindInvocationExceptionCtor()
    {
        var result = typeof( ParsedExpressionInvocationException )
            .GetConstructor( new[] { typeof( string ), typeof( object?[] ) } )!;

        return result;
    }

    [Pure]
    internal static MethodInfo FindArrayEmptyMethod(Type elementType)
    {
        var genericMethod = typeof( Array ).GetMethod(
            nameof( Array.Empty ),
            BindingFlags.Public | BindingFlags.Static,
            Array.Empty<Type>() )!;

        var result = genericMethod.MakeGenericMethod( elementType );
        return result;
    }

    [Pure]
    internal static ConstructorInfo FindArrayCtor(Type arrayType)
    {
        var result = arrayType.GetConstructor( new[] { typeof( int ) } )!;
        return result;
    }

    [Pure]
    internal static MethodInfo FindArraySetMethod(Type arrayType)
    {
        var result = arrayType.GetMethod( "Set" )!;
        return result;
    }
}
