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
    internal static bool IsInjectableMemberOptional(this MemberInfo member, IDependencyContainerConfigurationBuilder configuration)
    {
        member = member.GetActualMember();
        return member.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: true );
    }

    [Pure]
    internal static bool IsOpenGenericAssignableTo(this Type type, Type targetType)
    {
        Assume.True( targetType.IsGenericTypeDefinition );
        if ( ! type.IsGenericTypeDefinition )
            return false;

        if ( type == targetType )
            return true;

        var args = type.GetGenericArguments();
        var targetArgs = targetType.GetGenericArguments();
        if ( targetArgs.Length != args.Length )
            return false;

        IEnumerable<Type> candidates;
        if ( targetType.IsInterface )
            candidates = type.GetOpenGenericImplementations( targetType );
        else
        {
            var baseType = type.GetOpenGenericExtension( targetType );
            candidates = baseType is not null ? [ baseType ] : [ ];
        }

        HashSet<int>? usedImplementorIndices = null;
        foreach ( var candidate in candidates )
        {
            var candidateArgs = candidate.GetGenericArguments();
            if ( candidateArgs.Length != args.Length )
                continue;

            var isValid = true;
            for ( var i = 0; i < candidateArgs.Length; ++i )
            {
                var arg = candidateArgs[i];
                if ( ! arg.IsGenericParameter || arg.DeclaringMethod is not null || arg.DeclaringType != type )
                {
                    isValid = false;
                    break;
                }

                usedImplementorIndices ??= new HashSet<int>( candidateArgs.Length );
                var index = Array.IndexOf( args, arg );
                if ( index < 0
                    || ! usedImplementorIndices.Add( index )
                    || ! AreConstraintsCompatible( targetArgs[i], args[index] ) )
                {
                    isValid = false;
                    break;
                }
            }

            if ( isValid )
                return true;

            usedImplementorIndices?.Clear();
        }

        return false;
    }

    [Pure]
    internal static Type CloseImplementorType(this Type openImplementorType, Type closedType)
    {
        Assume.True( ! closedType.ContainsGenericParameters && closedType.IsGenericType );

        var openType = closedType.GetGenericTypeDefinition();
        Assume.True( openType.IsOpenGenericAssignableTo( closedType.GetGenericTypeDefinition() ) );
        Assume.True( openImplementorType.IsGenericTypeDefinition );
        Assume.True( openImplementorType.IsOpenGenericAssignableTo( openType ) );

        var matchedOpenType = openType;
        if ( openImplementorType != openType )
        {
            matchedOpenType = openType.IsInterface
                ? openImplementorType.GetOpenGenericImplementations( openType ).FirstOrDefault()
                : openImplementorType.GetOpenGenericExtension( openType );

            Assume.IsNotNull( matchedOpenType );
        }

        var openImplementorArgs = openImplementorType.GetGenericArguments();
        var matchedOpenArgs = matchedOpenType.GetGenericArguments();
        var closedTypeArgs = closedType.GetGenericArguments();

        var concreteImplementorArgs = new Type[openImplementorArgs.Length];
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
        Assume.True( ! closedType.ContainsGenericParameters && closedType.IsGenericType );
        Assume.True( openType.IsOpenGenericAssignableTo( closedType.GetGenericTypeDefinition() ) );

        var openImplementorType = openCtor.DeclaringType;
        Assume.True( openImplementorType is not null && openImplementorType.IsGenericTypeDefinition );
        Assume.True( openImplementorType.IsOpenGenericAssignableTo( openType ) );

        var matchedOpenType = openType;
        if ( openImplementorType != openType )
        {
            matchedOpenType = openType.IsInterface
                ? openImplementorType.GetOpenGenericImplementations( openType ).FirstOrDefault()
                : openImplementorType.GetOpenGenericExtension( openType );

            if ( matchedOpenType is null )
                return null;
        }

        var openImplementorArgs = openImplementorType.GetGenericArguments();
        var matchedOpenArgs = matchedOpenType.GetGenericArguments();
        var closedTypeArgs = closedType.GetGenericArguments();

        var concreteImplementorArgs = new Type[openImplementorArgs.Length];
        for ( var i = 0; i < matchedOpenArgs.Length; ++i )
        {
            var implementorArg = matchedOpenArgs[i];
            var implementorIndex = Array.IndexOf( openImplementorArgs, implementorArg );
            if ( implementorIndex < 0 )
                return null;

            concreteImplementorArgs[implementorIndex] = closedTypeArgs[i];
        }

        var closedImplementorType = openImplementorType.MakeGenericType( concreteImplementorArgs );

        var openCtorParameters = openCtor.GetParameters();
        var expectedCtorParameterTypes = new Type[openCtorParameters.Length];
        for ( var i = 0; i < openCtorParameters.Length; ++i )
            expectedCtorParameterTypes[i] = Substitute( openCtorParameters[i].ParameterType, openImplementorArgs, concreteImplementorArgs );

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

        static Type Substitute(Type type, Type[] openImplementorArgs, Type[] concreteImplementorArgs)
        {
            if ( ! type.ContainsGenericParameters )
                return type;

            if ( type.IsGenericParameter )
            {
                var index = Array.IndexOf( openImplementorArgs, type );
                return index >= 0 ? concreteImplementorArgs[index] : type;
            }

            var args = type.GetGenericArguments();
            var newArgs = new Type[args.Length];
            for ( var i = 0; i < args.Length; ++i )
                newArgs[i] = Substitute( args[i], openImplementorArgs, concreteImplementorArgs );

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

        if ( isOpen != isResolverOpen || ! type.IsGenericType )
            return false;

        var resolverDefinition = resolverType.IsGenericType ? resolverType.GetGenericTypeDefinition() : resolverType;
        return resolverDefinition.IsOpenGenericAssignableTo( type.GetGenericTypeDefinition() );
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
