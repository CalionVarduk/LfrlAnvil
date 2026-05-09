// Copyright 2024-2026 Łukasz Furlepa
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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Dependencies.Internal;

internal static class Helpers
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void DisposeGracefully(this ReaderWriterLockSlim @lock)
    {
        var spinWait = new SpinWait();
        while ( true )
        {
            if ( @lock.IsWriteLockHeld || @lock.IsUpgradeableReadLockHeld || @lock.IsReadLockHeld )
                @lock.Dispose();

            try
            {
                @lock.Dispose();
                break;
            }
            catch ( SynchronizationLockException )
            {
                spinWait.SpinOnce();
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Func<DependencyScope, object> CreateResolverFactory(
        this Expression<Func<DependencyScope, object>> expression,
        IResolverFactorySource source)
    {
        Func<DependencyScope, object>? compiled = null;
        return scope =>
        {
            using ( ExclusiveLock.Enter( expression ) )
            {
                if ( compiled is not null )
                {
                    Assume.Equals( compiled, source.Factory );
                    return compiled( scope );
                }

                compiled = expression.Compile();
                source.Factory = compiled;
            }

            Assume.Equals( compiled, source.Factory );
            return compiled( scope );
        };
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static object CreateScopedInstance(
        this DependencyResolver resolver,
        Func<DependencyScope, object> factory,
        DependencyScope scope,
        Type dependencyType)
    {
        Assume.True( scope.Lock.IsWriteLockHeld );

        var scopedInstancesByResolverId = scope.GetScopedInstancesByResolverId();
        ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( scopedInstancesByResolverId, resolver.Id, out var exists )!;
        if ( exists )
            return result;

        try
        {
            result = resolver.InvokeFactory( factory, scope, dependencyType );
        }
        catch
        {
            scopedInstancesByResolverId.Remove( resolver.Id );
            throw;
        }

        var disposer = resolver.DisposalStrategy.TryCreateDisposer( result );
        if ( disposer is not null )
            scope.AddDisposer( disposer.Value );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void TryRegisterTransientDisposer(this DependencyResolver resolver, object result, DependencyScope scope)
    {
        var disposer = resolver.DisposalStrategy.TryCreateDisposer( result );
        if ( disposer is null )
            return;

        using ( WriteLockSlim.TryEnter( scope.Lock, out var entered ) )
        {
            if ( ! entered || scope.IsDisposedInternal )
            {
                if ( disposer.Value.IsAsync )
                    disposer.Value.TryDisposeAsync().AsTask().GetAwaiter().GetResult();
                else
                    disposer.Value.TryDispose();

                ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( scope ) ) );
            }

            scope.AddDisposer( disposer.Value );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static object? TryFindAncestorScopedSingletonInstance(this DependencyResolver resolver, DependencyScope? scope)
    {
        while ( scope is not null )
        {
            using ( ReadLockSlim.TryEnter( scope.Lock, out var entered ) )
            {
                if ( ! entered || scope.IsDisposedInternal )
                    ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( scope ) ) );

                if ( scope.ScopedInstancesByResolverId is not null
                    && scope.ScopedInstancesByResolverId.TryGetValue( resolver.Id, out var result ) )
                    return result;
            }

            scope = scope.InternalParentScope;
        }

        return null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MethodInfo FindResolverCreateMethod(Type resolverType)
    {
        var result = resolverType.GetMethod( nameof( DependencyResolver.Create ), BindingFlags.Instance | BindingFlags.NonPublic );
        Assume.IsNotNull( result );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static List<MemberInfo> FindInjectableMembers(this Type type, Type injectablePropertyType)
    {
        var result = new List<MemberInfo>();
        var next = type;
        do
        {
            var members = next.FindMembers(
                MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                static (member, criteria) =>
                {
                    var injectablePropertyType = ReinterpretCast.To<Type>( criteria );

                    if ( member is FieldInfo field )
                        return field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == injectablePropertyType;

                    return member is PropertyInfo property
                        && property.PropertyType.IsGenericType
                        && property.PropertyType.GetGenericTypeDefinition() == injectablePropertyType
                        && property.GetSetMethod( nonPublic: true ) is not null
                        && ! property.IsIndexer()
                        && property.GetBackingField() is null;
                },
                injectablePropertyType );

            result.AddRange( members );
            next = next.BaseType;
        }
        while ( next is not null );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Type GetInjectableMemberType(this MemberInfo member)
    {
        return member.MemberType == MemberTypes.Field
            ? ReinterpretCast.To<FieldInfo>( member ).FieldType
            : ReinterpretCast.To<PropertyInfo>( member ).PropertyType;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MemberInfo GetActualMember(this MemberInfo member)
    {
        return member.MemberType == MemberTypes.Field
            ? ReinterpretCast.To<FieldInfo>( member ).GetBackedProperty() ?? member
            : member;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsInjectableMemberOptional(
        this MemberInfo member,
        Type memberType,
        IDependencyContainerConfigurationBuilder configuration)
    {
        member = member.GetActualMember();
        Assume.Equals( memberType, member.GetInjectableMemberType().GetGenericArguments()[0] );
        return member.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: true )
            || (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof( IEnumerable<> ));
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsInjectableParameterOptional(this ParameterInfo parameter, IDependencyContainerConfigurationBuilder configuration)
    {
        return parameter.HasDefaultValue
            || parameter.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: false )
            || (parameter.ParameterType.IsGenericType && parameter.ParameterType.GetGenericTypeDefinition() == typeof( IEnumerable<> ));
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ConstructorInfo FindInjectableMemberCtor(this Type memberType, Type instanceType)
    {
        Assume.True(
            memberType.IsGenericType
            && memberType.GetGenericArguments().Length == 1
            && memberType.GetGenericArguments()[0] == instanceType );

        var ctor = memberType.GetConstructor( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, [ instanceType ] );
        Assume.IsNotNull( ctor );
        return ctor;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static object? FindCorrespondingOpenTypeMemberResolution(
        this MemberInfo closedTypeMember,
        ReadOnlyArray<KeyValuePair<MemberInfo, object?>> openMemberResolutions)
    {
        Assume.True( closedTypeMember.MemberType is MemberTypes.Field or MemberTypes.Property );

        var closedType = closedTypeMember.DeclaringType;
        Assume.IsNotNull( closedType );
        var closedTypeDefinition = closedType.IsGenericType ? closedType.GetGenericTypeDefinition() : closedType;

        foreach ( var (openTypeMember, resolution) in openMemberResolutions )
        {
            if ( openTypeMember.Name != closedTypeMember.Name )
                continue;

            var openType = openTypeMember.DeclaringType;
            Assume.IsNotNull( openType );
            var openTypeDefinition = openType.IsGenericType ? openType.GetGenericTypeDefinition() : openType;
            if ( openTypeDefinition == closedTypeDefinition )
                return resolution;
        }

        return null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Type? GetOpenGenericDependencyType(this Type type)
    {
        if ( ! type.IsGenericType )
            return null;

        var openType = type.GetGenericTypeDefinition();
        if ( openType != typeof( IEnumerable<> ) )
            return openType;

        var elementType = type.GetGenericArguments()[0];
        if ( ! elementType.IsGenericType )
            return null;

        var openElementType = elementType.GetGenericTypeDefinition();
        return openElementType == typeof( IEnumerable<> ) ? null : typeof( IEnumerable<> ).MakeGenericType( openElementType );
    }

    [Pure]
    internal static bool IsOpenGenericAssignableTo(this Type type, Type targetType)
    {
        Assume.True( targetType.IsGenericTypeDefinition );
        if ( ! type.ContainsGenericParameters )
            return false;

        var openType = type.GetGenericTypeDefinition();
        if ( openType == targetType )
            return true;

        IEnumerable<Type> candidates;
        if ( targetType.IsInterface )
            candidates = type.GetOpenGenericImplementations( targetType );
        else
        {
            var baseType = type.GetOpenGenericExtension( targetType );
            candidates = baseType is not null ? [ baseType ] : [ ];
        }

        var args = type.GetGenericArguments();
        var openArgs = openType == type ? args : openType.GetGenericArguments();
        var targetArgs = targetType.GetGenericArguments();

        HashSet<int>? usedImplementorIndices = null;
        foreach ( var candidate in candidates )
        {
            var candidateArgs = candidate.GetGenericArguments();
            if ( candidateArgs.Length != targetArgs.Length )
                continue;

            var isValid = true;
            for ( var i = 0; i < candidateArgs.Length; ++i )
            {
                var arg = candidateArgs[i];
                if ( ! arg.IsGenericParameter || arg.DeclaringMethod is not null || arg.DeclaringType != openType )
                {
                    isValid = false;
                    break;
                }

                var index = Array.IndexOf( openArgs, arg );
                if ( index < 0 )
                {
                    isValid = false;
                    break;
                }

                usedImplementorIndices ??= new HashSet<int>( candidateArgs.Length );
                var typeArg = args[index];
                if ( ! typeArg.IsGenericParameter
                    || ! usedImplementorIndices.Add( index )
                    || ! AreConstraintsCompatible( targetArgs[i], typeArg ) )
                {
                    isValid = false;
                    break;
                }
            }

            if ( isValid )
            {
                Assume.IsNotNull( usedImplementorIndices );
                for ( var i = 0; i < args.Length; ++i )
                {
                    if ( ! usedImplementorIndices.Contains( i ) && args[i].IsGenericParameter )
                    {
                        isValid = false;
                        break;
                    }
                }

                if ( isValid )
                    return true;
            }

            usedImplementorIndices?.Clear();
        }

        return false;
    }

    [Pure]
    internal static Type CloseImplementorType(this Type implementorType, Type closedType)
    {
        Assume.True( ! closedType.ContainsGenericParameters && closedType.IsGenericType );

        var openType = closedType.GetGenericTypeDefinition();
        Assume.True( implementorType.ContainsGenericParameters );
        Assume.True( implementorType.IsOpenGenericAssignableTo( openType ) );

        var matchedOpenType = openType;
        var openImplementorType = implementorType.GetGenericTypeDefinition();
        if ( openImplementorType != openType )
        {
            matchedOpenType = openType.IsInterface
                ? implementorType.GetOpenGenericImplementations( openType ).FirstOrDefault()
                : implementorType.GetOpenGenericExtension( openType );

            Assume.IsNotNull( matchedOpenType );
        }

        var openImplementorArgs = openImplementorType.GetGenericArguments();
        var matchedOpenArgs = matchedOpenType.GetGenericArguments();
        var closedTypeArgs = closedType.GetGenericArguments();

        var concreteImplementorArgs = implementorType.GetGenericArguments();
        for ( var i = 0; i < matchedOpenArgs.Length; ++i )
        {
            var implementorArg = matchedOpenArgs[i];
            var implementorIndex = Array.IndexOf( openImplementorArgs, implementorArg );
            Assume.IsGreaterThanOrEqualTo( implementorIndex, 0 );
            concreteImplementorArgs[implementorIndex] = closedTypeArgs[i];
        }

        return openImplementorType.MakeGenericType( concreteImplementorArgs );
    }

    [Pure]
    internal static ConstructorInfo? TryCloseGenericCtor(this ConstructorInfo openCtor, Type openType, Type closedType)
    {
        Assume.True( openType.IsGenericTypeDefinition );
        Assume.True( closedType.IsGenericType );
        Assume.Equals( closedType.GetGenericTypeDefinition(), openType );

        var implementorType = openCtor.DeclaringType;
        Assume.True( implementorType is not null && implementorType.ContainsGenericParameters );
        Assume.True( implementorType.IsOpenGenericAssignableTo( openType ) );

        var matchedOpenType = openType;
        var openImplementorType = implementorType.GetGenericTypeDefinition();
        if ( openImplementorType != openType )
        {
            matchedOpenType = openType.IsInterface
                ? implementorType.GetOpenGenericImplementations( openType ).FirstOrDefault()
                : implementorType.GetOpenGenericExtension( openType );

            if ( matchedOpenType is null )
                return null;
        }

        var openImplementorArgs = openImplementorType.GetGenericArguments();
        var matchedOpenArgs = matchedOpenType.GetGenericArguments();
        var closedTypeArgs = closedType.GetGenericArguments();

        var concreteImplementorArgs = implementorType.GetGenericArguments();
        for ( var i = 0; i < matchedOpenArgs.Length; ++i )
        {
            var implementorArg = matchedOpenArgs[i];
            var implementorIndex = Array.IndexOf( openImplementorArgs, implementorArg );
            if ( implementorIndex < 0 )
                return null;

            concreteImplementorArgs[implementorIndex] = closedTypeArgs[i];
        }

        var substitutionMap = new Dictionary<Type, Type>( openImplementorArgs.Length );
        for ( var i = 0; i < openImplementorArgs.Length; ++i )
            substitutionMap[openImplementorArgs[i]] = concreteImplementorArgs[i];

        var openCtorParameters = openCtor.GetParameters();
        var expectedCtorParameterTypes = new Type[openCtorParameters.Length];
        for ( var i = 0; i < openCtorParameters.Length; ++i )
            expectedCtorParameterTypes[i] = Substitute( openCtorParameters[i].ParameterType, substitutionMap );

        var closedImplementorType = openImplementorType.MakeGenericType( concreteImplementorArgs );
        foreach ( var ctor in closedImplementorType.GetConstructors() )
        {
            var ctorParameters = ctor.GetParameters();
            if ( ctorParameters.Length != expectedCtorParameterTypes.Length )
                continue;

            var match = true;
            for ( var i = 0; i < expectedCtorParameterTypes.Length; ++i )
            {
                if ( ctorParameters[i].ParameterType != expectedCtorParameterTypes[i] )
                {
                    match = false;
                    break;
                }
            }

            if ( match )
                return ctor;
        }

        return null;

        static Type Substitute(Type type, Dictionary<Type, Type> substitutionMap)
        {
            if ( ! type.ContainsGenericParameters )
                return type;

            if ( type.IsGenericParameter )
                return substitutionMap.TryGetValue( type, out var concrete ) ? concrete : type;

            var args = type.GetGenericArguments();
            var newArgs = new Type[args.Length];
            for ( var i = 0; i < args.Length; ++i )
                newArgs[i] = Substitute( args[i], substitutionMap );

            return type.GetGenericTypeDefinition().MakeGenericType( newArgs );
        }
    }

    [Pure]
    internal static bool IsAnyResolvableBy(this Type type, Type resolverType)
    {
        if ( type == resolverType )
            return true;

        var isOpen = type.ContainsGenericParameters;
        var isResolverOpen = resolverType.ContainsGenericParameters;
        if ( ! isOpen && ! isResolverOpen )
            return resolverType.IsAssignableTo( type );

        if ( isOpen != isResolverOpen || ! type.IsGenericType || ! resolverType.IsGenericTypeDefinition )
            return false;

        return resolverType.IsOpenGenericAssignableTo( type.GetGenericTypeDefinition() );
    }

    [Pure]
    private static bool AreConstraintsCompatible(Type arg, Type implementorArg)
    {
        var attributes = arg.GenericParameterAttributes;
        var implementorAttributes = implementorArg.GenericParameterAttributes;

        if ( (implementorAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0
            && (attributes & GenericParameterAttributes.ReferenceTypeConstraint) == 0 )
            return false;

        if ( (implementorAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0
            && (attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0 )
            return false;

        if ( (implementorAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0
            && (attributes & GenericParameterAttributes.DefaultConstructorConstraint) == 0 )
            return false;

        var constraints = arg.GetGenericParameterConstraints();
        var implementorConstraints = implementorArg.GetGenericParameterConstraints();

        foreach ( var implementorConstraint in implementorConstraints )
        {
            var matched = false;
            foreach ( var constraint in constraints )
            {
                if ( constraint == implementorConstraint )
                {
                    matched = true;
                    break;
                }
            }

            if ( ! matched )
                return false;
        }

        return true;
    }
}
