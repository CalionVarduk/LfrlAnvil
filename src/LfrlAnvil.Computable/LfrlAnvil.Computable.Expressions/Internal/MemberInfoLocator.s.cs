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

        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo FindStringConcatMethod()
    {
        var result = typeof( string ).GetMethod(
            name: nameof( string.Concat ),
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            types: new[] { typeof( string ), typeof( string ) } );

        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo FindToStringMethod()
    {
        var result = typeof( object ).GetMethod(
            name: nameof( ToString ),
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            types: Type.EmptyTypes );

        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo FindToStringWithFormatProviderMethod(Type type)
    {
        var result = type.GetMethod(
            name: nameof( ToString ),
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            types: new[] { typeof( IFormatProvider ) } );

        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static ConstructorInfo FindInvocationExceptionCtor()
    {
        var result = typeof( ParsedExpressionInvocationException )
            .GetConstructor( new[] { typeof( string ), typeof( object?[] ) } );

        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo FindArrayEmptyMethod(Type elementType)
    {
        var genericMethod = typeof( Array ).GetMethod(
            name: nameof( Array.Empty ),
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            types: Type.EmptyTypes );

        Assume.IsNotNull( genericMethod );
        var result = genericMethod.MakeGenericMethod( elementType );
        return result;
    }

    [Pure]
    internal static ConstructorInfo FindArrayCtor(Type arrayType)
    {
        var result = arrayType.GetConstructor( new[] { typeof( int ) } );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    internal static MethodInfo FindArraySetMethod(Type arrayType)
    {
        var result = arrayType.GetMethod( "Set" );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MemberInfo[] FindFieldsAndProperties(Type type, BindingFlags bindingFlags, MemberFilter filter)
    {
        var result = type.FindMembers(
            MemberTypes.Field | MemberTypes.Property,
            bindingFlags,
            filter,
            filterCriteria: null );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MemberInfo? TryFindIndexer(Type type, Type[] parameterTypes, BindingFlags bindingFlags)
    {
        if ( type.IsArray )
        {
            var getMethod = type.GetMethod( "Get" );
            Assume.IsNotNull( getMethod );
            var parameters = getMethod.GetParameters();

            if ( parameters.Length == parameterTypes.Length && AreParametersMatching( parameters, parameterTypes ) )
                return getMethod;

            return null;
        }

        var nonPublic = (bindingFlags & BindingFlags.NonPublic) != BindingFlags.Default;
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
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ConstructorInfo? TryFindCtor(Type type, Type[] parameterTypes, BindingFlags bindingFlags)
    {
        var constructors = type.GetConstructors( bindingFlags );
        foreach ( var ctor in constructors )
        {
            var parameters = ctor.GetParameters();
            if ( parameters.Length == parameterTypes.Length && AreParametersMatching( parameters, parameterTypes ) )
                return ctor;
        }

        return null;
    }

    [Pure]
    internal static MethodInfo[] FindMethods(Type type, Type[] parameterTypes, BindingFlags bindingFlags, MemberFilter filter)
    {
        MethodInfo?[] methods = type.GetMethods( bindingFlags );
        var nonGenericParameterCount = new int[methods.Length];
        var maxNonGenericParameterCount = 0;

        for ( var i = 0; i < methods.Length; ++i )
        {
            var method = methods[i];
            Assume.IsNotNull( method );
            nonGenericParameterCount[i] = -1;

            if ( ! filter( method, null ) )
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
        Assume.Equals( parameters.Length, expectedTypes.Length );

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
        Assume.True( method.IsGenericMethodDefinition );

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
