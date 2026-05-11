// Copyright 2026 Łukasz Furlepa
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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Resolvers.Factories;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class OpenGenericDependencyResolver : DependencyResolver
{
    private readonly ConstructorInfo _ctor;
    private readonly object?[]? _parameterResolvers;
    private readonly KeyValuePair<MemberInfo, object?>[]? _memberResolvers;
    private readonly Action<Type, IDependencyScope>? _onResolvingCallback;
    private readonly Action<object, Type, IDependencyScope>? _onCreatedCallback;
    private readonly Type _injectablePropertyType;
    private readonly IInternalDependencyKey _implementorKey;
    private readonly bool _isShared;
    private readonly bool _isRangeElement;

    internal OpenGenericDependencyResolver(
        ulong id,
        Type implementorType,
        ResolvedInstanceDisposalStrategy disposalStrategy,
        ConstructorInfo ctor,
        object?[]? parameterResolvers,
        KeyValuePair<MemberInfo, object?>[]? memberResolvers,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Action<object, Type, IDependencyScope>? onCreatedCallback,
        Type injectablePropertyType,
        IInternalDependencyKey implementorKey,
        bool isShared,
        bool isRangeElement,
        DependencyLifetime lifetime)
        : base( id, implementorType, disposalStrategy )
    {
        Assume.ContainsExactly( parameterResolvers ?? [ ], ctor.GetParameters().Length );
        _ctor = ctor;
        _parameterResolvers = parameterResolvers;
        _memberResolvers = memberResolvers;
        _onResolvingCallback = onResolvingCallback;
        _onCreatedCallback = onCreatedCallback;
        _injectablePropertyType = injectablePropertyType;
        _implementorKey = implementorKey;
        _isShared = isShared;
        _isRangeElement = isRangeElement;
        Lifetime = lifetime;
    }

    internal override DependencyLifetime Lifetime { get; }

    internal DependencyResolver Close(DependencyLocator dependencyLocator, Type dependencyType, Value<ConstructorInfo?>? customCtor = null)
    {
        Assume.True( dependencyType.IsGenericType );
        Assume.True(
            customCtor?.Item?.DeclaringType is null
            || (customCtor.Value.Item.DeclaringType.IsGenericType
                && customCtor.Value.Item.DeclaringType.GetGenericTypeDefinition() == ImplementorType.GetGenericTypeDefinition()) );

        if ( customCtor is not null && customCtor.Value.Item?.DeclaringType is null )
            ExceptionThrower.Throw(
                new OpenGenericDependencyException(
                    dependencyType,
                    Resources.FailedToFindValidCtorForClosedGenericType( dependencyType.GetGenericTypeDefinition(), dependencyType ) ) );

        var baseImplementorType = customCtor is not null ? customCtor.Value.Item.DeclaringType! : ImplementorType;
        var implementorType = baseImplementorType.CloseImplementorType( dependencyType );
        var locator = _implementorKey.GetLocator( dependencyLocator );

        DependencyResolver result;
        if ( ! _isShared )
        {
            result = CreateClosedResolver(
                locator,
                dependencyType,
                implementorType,
                CustomOpenGenericResolutionSource.GetCtor( customCtor, _ctor ) );

            if ( ! _isRangeElement )
            {
                using ( AcquireActiveWriteLock( locator ) )
                    result = locator.Resolvers.GetOrAddResolver( dependencyType, result );
            }
        }
        else
        {
            var container = locator.InternalAttachedScope.InternalContainer;

            DependencyResolver? sharedResolver;
            using ( AcquireActiveSharedContainersReadLock( container ) )
                sharedResolver = locator.Resolvers.TryGetSharedGenericResolver( _implementorKey.Type, implementorType, Lifetime );

            if ( sharedResolver is not null )
            {
                result = sharedResolver;
                if ( ! _isRangeElement )
                {
                    using ( AcquireActiveWriteLock( locator ) )
                        locator.Resolvers.SetResolver( dependencyType, result );
                }
            }
            else
            {
                result = CreateClosedResolver(
                    locator,
                    dependencyType,
                    implementorType,
                    CustomOpenGenericResolutionSource.GetCtor( customCtor, _ctor ) );

                if ( ! _isRangeElement )
                {
                    using ( AcquireActiveSharedContainersWriteLock( container ) )
                    using ( AcquireActiveWriteLock( locator ) )
                    {
                        result = locator.Resolvers.GetOrAddSharedGenericResolver(
                            _implementorKey.Type,
                            implementorType,
                            Lifetime,
                            result );

                        locator.Resolvers.SetResolver( dependencyType, result );
                    }
                }
                else
                {
                    using ( AcquireActiveSharedContainersWriteLock( container ) )
                        result = locator.Resolvers.GetOrAddSharedGenericResolver(
                            _implementorKey.Type,
                            implementorType,
                            Lifetime,
                            result );
                }
            }
        }

        return result;
    }

    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        throw new OpenGenericDependencyException( dependencyType, Resources.OpenGenericCannotBeResolved( dependencyType ) );
    }

    [Pure]
    private DependencyResolver CreateClosedResolver(
        DependencyLocator locator,
        Type dependencyType,
        Type implementorType,
        ConstructorInfo ctor)
    {
        var openDependencyType = dependencyType.GetGenericTypeDefinition();
        var closedCtor = ctor.TryCloseGenericCtor( openDependencyType, dependencyType );
        if ( closedCtor is null )
            ExceptionThrower.Throw(
                new OpenGenericDependencyException(
                    dependencyType,
                    Resources.FailedToFindValidCtorForClosedGenericType( openDependencyType, dependencyType ) ) );

        Expression<Func<DependencyScope, object>> expression;
        using ( DependencyCycleTracker.Create( this, implementorType ) )
        {
            try
            {
                expression = BuildExpression( locator, closedCtor, implementorType );
            }
            catch ( CircularDependencyReferenceException exc )
            {
                ExceptionThrower.Throw( new CircularDependencyReferenceException( dependencyType, implementorType, exc ) );
                return default!;
            }
        }

        var id = locator.InternalAttachedScope.InternalContainer.GenerateResolverId();
        if ( _onResolvingCallback is not null || _onCreatedCallback is not null )
        {
            return Lifetime switch
            {
                DependencyLifetime.Scoped => new CycleTrackingScopedDependencyResolver(
                    id,
                    implementorType,
                    DisposalStrategy,
                    _onResolvingCallback,
                    expression ),
                DependencyLifetime.ScopedSingleton => new CycleTrackingScopedSingletonDependencyResolver(
                    id,
                    implementorType,
                    DisposalStrategy,
                    _onResolvingCallback,
                    expression ),
                DependencyLifetime.Singleton => new CycleTrackingSingletonDependencyResolver(
                    id,
                    implementorType,
                    DisposalStrategy,
                    _onResolvingCallback,
                    expression ),
                _ => new CycleTrackingTransientDependencyResolver( id, implementorType, DisposalStrategy, _onResolvingCallback, expression )
            };
        }

        return Lifetime switch
        {
            DependencyLifetime.Scoped => new ScopedDependencyResolver( id, implementorType, DisposalStrategy, expression ),
            DependencyLifetime.ScopedSingleton => new ScopedSingletonDependencyResolver(
                id,
                implementorType,
                DisposalStrategy,
                expression ),
            DependencyLifetime.Singleton => new SingletonDependencyResolver( id, implementorType, DisposalStrategy, expression ),
            _ => new TransientDependencyResolver( id, implementorType, DisposalStrategy, expression )
        };
    }

    [Pure]
    private Expression<Func<DependencyScope, object>> BuildExpression(
        DependencyLocator locator,
        ConstructorInfo closedCtor,
        Type implementorType)
    {
        var memberCount = _memberResolvers?.Length ?? 0;
        var injectableMembers = memberCount > 0 ? closedCtor.DeclaringType?.FindInjectableMembers( _injectablePropertyType ) ?? [ ] : null;
        var (builder, parameters) = CreateExpressionBuilder( locator, closedCtor, implementorType, injectableMembers );

        for ( var i = 0; i < parameters.Length; ++i )
        {
            Assume.IsNotNull( _parameterResolvers );
            var resolution = _parameterResolvers[i];
            var parameter = parameters[i];
            var (instanceType, name) = (parameter.ParameterType, $"p{i}");

            if ( resolution is null )
                AddDefaultResolution( builder, locator, instanceType, name, parameter.HasDefaultValue, parameter.DefaultValue );
            else if ( resolution is Expression<Func<IDependencyScope, ParameterInfo, object>> factory )
                builder.AddExpressionResolution( instanceType, name, parameter, factory );
            else
            {
                var (resolver, customSource) = CustomOpenGenericResolution.Extract( resolution );
                Assume.True( customSource is null || resolver is OpenGenericDependencyResolver );

                if ( resolver is OpenGenericDependencyResolver openGenericResolver )
                    AddOpenGenericResolution( builder, locator, openGenericResolver, instanceType, name, customSource );
                else if ( resolver is OpenGenericRangeDependencyResolver openGenericRangeResolver )
                    AddOpenGenericRangeResolution( builder, locator, openGenericRangeResolver, instanceType, name );
                else
                    builder.AddDependencyResolverResolution( instanceType, name, resolver );
            }
        }

        MemberBinding[]? memberBindings = null;
        if ( memberCount > 0 )
        {
            Assume.IsNotNull( _memberResolvers );
            Assume.IsNotNull( injectableMembers );
            Assume.ContainsExactly( injectableMembers, memberCount );
            memberBindings = new MemberBinding[memberCount];

            for ( var i = 0; i < memberCount; ++i )
            {
                var member = injectableMembers[i];
                var memberType = member.GetInjectableMemberType();
                var resolution = member.FindCorrespondingOpenTypeMemberResolution( _memberResolvers );
                var (instanceType, name) = (memberType.GetGenericArguments()[0], $"m{i}");

                if ( resolution is null )
                    AddDefaultResolution( builder, locator, instanceType, name );
                else if ( resolution is Expression<Func<IDependencyScope, MemberInfo, object>> factory )
                    builder.AddExpressionResolution( instanceType, name, member.GetActualMember(), factory );
                else
                {
                    var (resolver, customSource) = CustomOpenGenericResolution.Extract( resolution );
                    Assume.True( customSource is null || resolver is OpenGenericDependencyResolver );

                    if ( resolver is OpenGenericDependencyResolver openGenericResolver )
                        AddOpenGenericResolution( builder, locator, openGenericResolver, instanceType, name, customSource );
                    else if ( resolver is OpenGenericRangeDependencyResolver openGenericRangeResolver )
                        AddOpenGenericRangeResolution( builder, locator, openGenericRangeResolver, instanceType, name );
                    else
                        builder.AddDependencyResolverResolution( instanceType, name, resolver );
                }

                var memberCtor = memberType.FindInjectableMemberCtor( instanceType );
                memberBindings[i] = builder.CreateMemberBindingForLastVariable( member, memberCtor );
            }
        }

        var ctorParameters = builder.GetVariableRange( parameters.Length );
        var ctorCall = parameters.Length > 0 ? Expression.New( closedCtor, ctorParameters ) : Expression.New( closedCtor );
        Expression instance = memberBindings is not null ? Expression.MemberInit( ctorCall, memberBindings ) : ctorCall;
        return builder.Build( instance );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void AddDefaultResolution(
        ExpressionBuilder builder,
        DependencyLocator locator,
        Type instanceType,
        string name,
        bool hasDefaultValue = false,
        object? defaultValue = null)
    {
        DependencyResolver? resolver;
        using ( AcquireActiveReadLock( locator ) )
            resolver = locator.Resolvers.TryGetResolver( instanceType );

        if ( resolver is null )
            builder.AddDefaultResolution( instanceType, name, hasDefaultValue, defaultValue );
        else
            builder.AddDependencyResolverResolution( instanceType, name, resolver );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void AddOpenGenericResolution(
        ExpressionBuilder builder,
        DependencyLocator locator,
        OpenGenericDependencyResolver openGenericResolver,
        Type instanceType,
        string name,
        CustomOpenGenericResolutionSource? customSource)
    {
        var dependencyType = instanceType;
        var resolutionLocator = locator;
        if ( customSource is not null )
        {
            dependencyType = customSource.Value.Type.CloseImplementorType( instanceType );
            resolutionLocator = openGenericResolver._implementorKey.GetLocator( resolutionLocator );
        }

        DependencyResolver? resolver;
        using ( AcquireActiveReadLock( resolutionLocator ) )
            resolver = resolutionLocator.Resolvers.TryGetResolver( dependencyType );

        if ( resolver is not null )
        {
            builder.AddDependencyResolverResolution( instanceType, name, resolver );
            return;
        }

        resolver = openGenericResolver.Close(
            resolutionLocator,
            dependencyType,
            CustomOpenGenericResolutionSource.TryGetCtor( customSource ) );

        builder.AddDependencyResolverResolution( instanceType, name, resolver );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void AddOpenGenericRangeResolution(
        ExpressionBuilder builder,
        DependencyLocator locator,
        OpenGenericRangeDependencyResolver openGenericRangeResolver,
        Type instanceType,
        string name)
    {
        DependencyResolver? resolver;
        using ( AcquireActiveReadLock( locator ) )
            resolver = locator.Resolvers.TryGetResolver( instanceType );

        if ( resolver is not null )
        {
            builder.AddDependencyResolverResolution( instanceType, name, resolver );
            return;
        }

        resolver = openGenericRangeResolver.Close( locator, instanceType );
        builder.AddDependencyResolverResolution( instanceType, name, resolver );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (ExpressionBuilder Builder, ParameterInfo[] Parameters) CreateExpressionBuilder(
        DependencyLocator locator,
        ConstructorInfo closedCtor,
        Type implementorType,
        List<MemberInfo>? injectableMembers)
    {
        var parameters = closedCtor.GetParameters();
        var parameterCount = _parameterResolvers?.Length ?? 0;
        Assume.ContainsExactly( parameters, parameterCount );
        var defaultResolutionCount = 0;
        var hasRequiredValueTypeDependency = false;

        for ( var i = 0; i < parameterCount; ++i )
        {
            Assume.IsNotNull( _parameterResolvers );
            var resolution = _parameterResolvers[i];
            var parameter = parameters[i];

            if ( resolution is null )
            {
                using ( AcquireActiveReadLock( locator ) )
                    resolution = locator.Resolvers.TryGetResolver( parameter.ParameterType );

                if ( resolution is null )
                {
                    ++defaultResolutionCount;
                    continue;
                }
            }

            if ( ! hasRequiredValueTypeDependency
                && parameter.ParameterType.IsValueType
                && Nullable.GetUnderlyingType( parameter.ParameterType ) is null )
                hasRequiredValueTypeDependency = true;
        }

        var memberCount = 0;
        if ( injectableMembers is not null )
        {
            Assume.IsNotNull( _memberResolvers );
            Assume.ContainsExactly( injectableMembers, _memberResolvers.Length );
            var injectableMembersSpan = CollectionsMarshal.AsSpan( injectableMembers );
            memberCount = injectableMembersSpan.Length;

            for ( var i = 0; i < memberCount; ++i )
            {
                var closedMember = injectableMembers[i];
                var resolution = closedMember.FindCorrespondingOpenTypeMemberResolution( _memberResolvers );
                Type? closedMemberType = null;

                if ( resolution is null )
                {
                    closedMemberType = closedMember.GetInjectableMemberType().GetGenericArguments()[0];
                    using ( AcquireActiveReadLock( locator ) )
                        resolution = locator.Resolvers.TryGetResolver( closedMemberType );

                    if ( resolution is null )
                    {
                        ++defaultResolutionCount;
                        continue;
                    }
                }

                if ( hasRequiredValueTypeDependency )
                    continue;

                closedMemberType ??= closedMember.GetInjectableMemberType().GetGenericArguments()[0];
                if ( closedMemberType.IsValueType && Nullable.GetUnderlyingType( closedMemberType ) is null )
                    hasRequiredValueTypeDependency = true;
            }
        }

        var builder = new ExpressionBuilder(
            parameterCount + memberCount,
            defaultResolutionCount,
            hasRequiredValueTypeDependency,
            implementorType,
            _onCreatedCallback );

        return (builder, parameters);
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ReadLockSlim AcquireActiveReadLock(DependencyLocator locator)
    {
        var @lock = ReadLockSlim.TryEnter( locator.Resolvers.Lock, out var entered );
        if ( ! entered )
            ExceptionThrower.Throw(
                new ObjectDisposedException(
                    null,
                    Resources.ScopeIsDisposed( locator.InternalAttachedScope.InternalContainer.InternalRootScope ) ) );

        return @lock;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static WriteLockSlim AcquireActiveWriteLock(DependencyLocator locator)
    {
        var @lock = WriteLockSlim.TryEnter( locator.Resolvers.Lock, out var entered );
        if ( ! entered )
            ExceptionThrower.Throw(
                new ObjectDisposedException(
                    null,
                    Resources.ScopeIsDisposed( locator.InternalAttachedScope.InternalContainer.InternalRootScope ) ) );

        return @lock;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ReadLockSlim AcquireActiveSharedContainersReadLock(DependencyContainer container)
    {
        var @lock = ReadLockSlim.TryEnter( container.SharedGenericImplementorsLock, out var entered );
        if ( ! entered )
            ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( container.InternalRootScope ) ) );

        return @lock;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static WriteLockSlim AcquireActiveSharedContainersWriteLock(DependencyContainer container)
    {
        var @lock = WriteLockSlim.TryEnter( container.SharedGenericImplementorsLock, out var entered );
        if ( ! entered )
            ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( container.InternalRootScope ) ) );

        return @lock;
    }
}
