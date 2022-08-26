using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
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

    [Pure]
    internal static MemberInfo[] FindMembers(Type type, StringSlice symbol, ParsedExpressionFactoryInternalConfiguration configuration)
    {
        var result = type.FindMembers(
            MemberTypes.Field | MemberTypes.Property | MemberTypes.Method,
            configuration.MemberBindingFlags,
            configuration.GetAccessibleMemberFilter( symbol ),
            null );

        return result;
    }

    [Pure]
    internal static MemberInfo? TryFindIndexer(Type type, Type[] parameterTypes, BindingFlags bindingFlags)
    {
        if ( type.IsArray )
        {
            var getMethod = type.GetMethod( "Get" )!;
            var parameters = getMethod.GetParameters();

            if ( parameters.Length == parameterTypes.Length && AreParametersMatching( parameters, parameterTypes ) )
                return getMethod;

            return null;
        }

        var nonPublic = (bindingFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic;
        var properties = type.GetProperties( bindingFlags );

        foreach ( var property in properties )
        {
            var getter = property.GetGetMethod( nonPublic );
            if ( getter is null )
                continue;

            var parameters = getter.GetParameters();
            if ( parameters.Length == parameterTypes.Length && AreParametersMatching( parameters, parameterTypes ) )
                return property;
        }

        return null;
    }

    [Pure]
    internal static MethodInfo[] TryFindMethods(
        Type type,
        StringSlice symbol,
        Type[] parameterTypes,
        ParsedExpressionFactoryInternalConfiguration configuration)
    {
        var baseFilter = configuration.GetAccessibleMemberFilter( symbol );
        MethodInfo?[] methods = type.GetMethods( configuration.MemberBindingFlags );
        var nonGenericParameterCount = new int[methods.Length];
        var maxNonGenericParameterCount = 0;

        for ( var i = 0; i < methods.Length; ++i )
        {
            var method = methods[i]!;
            nonGenericParameterCount[i] = -1;

            if ( ! baseFilter( method, null ) )
            {
                methods[i] = null;
                continue;
            }

            var parameters = method.GetParameters();
            if ( parameters.Length != parameterTypes.Length )
            {
                methods[i] = null;
                continue;
            }

            if ( ! method.IsGenericMethodDefinition )
            {
                if ( AreParametersMatching( parameters, parameterTypes ) )
                {
                    nonGenericParameterCount[i] = parameters.Length;
                    maxNonGenericParameterCount = parameters.Length;
                }
                else
                    methods[i] = null;

                continue;
            }

            if ( parameters.Length == 0 )
            {
                methods[i] = null;
                continue;
            }

            var genericResult = CreateClosedGenericMethod( method, parameters, parameterTypes );
            if ( genericResult.Method is null )
            {
                methods[i] = null;
                continue;
            }

            methods[i] = genericResult.Method;
            nonGenericParameterCount[i] = genericResult.NonGenericParameterCount;
            maxNonGenericParameterCount = Math.Max( maxNonGenericParameterCount, genericResult.NonGenericParameterCount );
        }

        var count = 0;
        for ( var i = 0; i < methods.Length; ++i )
        {
            if ( nonGenericParameterCount[i] == maxNonGenericParameterCount )
                ++count;
        }

        if ( count == 0 )
            return Array.Empty<MethodInfo>();

        var result = new MethodInfo[count];
        for ( int i = 0, j = 0; i < methods.Length; ++i )
        {
            if ( nonGenericParameterCount[i] != maxNonGenericParameterCount )
                continue;

            result[j++] = methods[i]!;
        }

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool AreParametersMatching(ParameterInfo[] parameters, Type[] expectedTypes)
    {
        Assume.Equals( parameters.Length, expectedTypes.Length, nameof( parameters.Length ) );

        for ( var i = 0; i < parameters.Length; ++i )
        {
            if ( parameters[i].ParameterType != expectedTypes[i] )
                return false;
        }

        return true;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static (MethodInfo? Method, int NonGenericParameterCount) CreateClosedGenericMethod(
        MethodInfo method,
        ParameterInfo[] parameters,
        Type[] parameterTypes)
    {
        Assume.Equals( method.IsGenericMethodDefinition, true, nameof( method.IsGenericMethodDefinition ) );

        const int nonGenericParameterIndex = int.MinValue;
        var genericArgs = method.GetGenericArguments();
        var genericParameterTypeIndexes = new int[parameters.Length];

        for ( var i = 0; i < parameters.Length; ++i )
        {
            var parameter = parameters[i];
            var expectedType = parameterTypes[i];

            if ( parameter.ParameterType == expectedType )
            {
                genericParameterTypeIndexes[i] = nonGenericParameterIndex;
                continue;
            }

            genericParameterTypeIndexes[i] = Array.IndexOf( genericArgs, parameter.ParameterType );
            if ( genericParameterTypeIndexes[i] < 0 )
                return (null, -1);
        }

        var genericTypes = new Type?[genericArgs.Length];
        var nonGenericParameterCount = 0;

        for ( var i = 0; i < parameters.Length; ++i )
        {
            var genericArgIndex = genericParameterTypeIndexes[i];
            if ( genericArgIndex == nonGenericParameterIndex )
            {
                ++nonGenericParameterCount;
                continue;
            }

            var expectedType = parameterTypes[i];
            if ( genericTypes[genericArgIndex] is null )
            {
                genericTypes[genericArgIndex] = expectedType;
                continue;
            }

            if ( genericTypes[genericArgIndex] == expectedType )
                continue;

            genericTypes[genericArgIndex] = null;
            break;
        }

        if ( Array.IndexOf( genericTypes, null ) != -1 )
            return (null, -1);

        try
        {
            return (method.MakeGenericMethod( genericTypes! ), nonGenericParameterCount);
        }
        catch
        {
            return (null, -1);
        }
    }
}
