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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies;

/// <inheritdoc cref="IDisposableDependencyContainer" />
public sealed class DependencyContainer : IDisposableDependencyContainer
{
    private readonly NamedDependencyScopeStore _namedScopes;
    private readonly UlongSequenceGenerator _idGenerator;

    internal DependencyContainer(
        UlongSequenceGenerator idGenerator,
        Dictionary<Type, DependencyResolver> globalResolvers,
        KeyedDependencyResolversStore keyedResolversStore)
    {
        GlobalResolvers = DependencyResolversStore.Create( globalResolvers );
        KeyedResolversStore = keyedResolversStore;
        _idGenerator = idGenerator;
        _namedScopes = NamedDependencyScopeStore.Create();
        InternalRootScope = new RootDependencyScope( this );
    }

    /// <inheritdoc />
    public IDependencyScope RootScope => InternalRootScope;

    internal RootDependencyScope InternalRootScope { get; }
    internal DependencyResolversStore GlobalResolvers { get; }
    internal KeyedDependencyResolversStore KeyedResolversStore { get; }

    /// <inheritdoc />
    /// <remarks>Disposes the <see cref="RootScope"/>.</remarks>
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    /// <remarks>Disposes the <see cref="RootScope"/>.</remarks>
    public ValueTask DisposeAsync()
    {
        return DisposeRootScopeAsync();
    }

    /// <inheritdoc />
    [Pure]
    public IDependencyScope? TryGetScope(string name)
    {
        return _namedScopes.TryGetScope( name );
    }

    /// <inheritdoc />
    [Pure]
    public IDependencyScope GetScope(string name)
    {
        var result = TryGetScope( name );
        if ( result is null )
            throw new DependencyScopeNotFoundException( name );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal object? TryResolveDependency(DependencyLocator locator, Type dependencyType)
    {
        DependencyResolver? resolver;
        using ( ReadLockSlim.TryEnter( locator.Resolvers.Lock, out var entered ) )
        {
            if ( ! entered || locator.InternalAttachedScope.IsDisposedInternal )
                ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( locator.InternalAttachedScope ) ) );

            resolver = locator.Resolvers.TryGetResolver( dependencyType );
        }

        return resolver is not null
            ? resolver.Create( locator.InternalAttachedScope, dependencyType )
            : TryResolveDynamicDependency( locator, dependencyType );
    }

    internal ChildDependencyScope CreateChildScope(DependencyScope parentScope, string? name)
    {
        using ( WriteLockSlim.TryEnter( parentScope.Lock, out var entered ) )
        {
            if ( ! entered || parentScope.IsDisposedInternal )
                ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( parentScope ) ) );

            var result = name is null ? new ChildDependencyScope( this, parentScope ) : _namedScopes.CreateScope( parentScope, name );
            parentScope.AddChildCore( result );
            return result;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ValueTask DisposeScopeAsync(DependencyScope scope)
    {
        return ReferenceEquals( scope, InternalRootScope )
            ? DisposeRootScopeAsync()
            : DisposeChildScopeAsync( ReinterpretCast.To<ChildDependencyScope>( scope ) );
    }

    private async ValueTask DisposeRootScopeAsync()
    {
        var children = ListSlim<ChildDependencyScope>.Create();
        using ( WriteLockSlim.TryEnter( InternalRootScope.Lock, out var entered ) )
        {
            if ( ! entered || InternalRootScope.IsDisposedInternal )
                return;

            MarkChildScopeRangeAsDisposed( ref children, InternalRootScope );
            InternalRootScope.MarkAsDisposed();
        }

        var exceptions = Chain<OwnedDependencyDisposalException>.Empty;
        for ( var i = 0; i < children.Count; ++i )
        {
            var child = children[i];
            exceptions = exceptions.Extend( await child.FinalizeDisposalAsync().ConfigureAwait( false ) );

            if ( child.Name is not null )
                _namedScopes.RemoveScope( child.Name );

            child.Lock.DisposeGracefully();
        }

        exceptions = exceptions.Extend( await InternalRootScope.FinalizeDisposalAsync().ConfigureAwait( false ) );

        using ( WriteLockSlim.Enter( InternalRootScope.Lock ) )
        {
            GlobalResolvers.Dispose();
            KeyedResolversStore.Dispose();
            _namedScopes.Dispose();
        }

        InternalRootScope.Lock.DisposeGracefully();

        if ( exceptions.Count > 0 )
            ExceptionThrower.Throw( new OwnedDependenciesDisposalAggregateException( InternalRootScope, exceptions ) );
    }

    private async ValueTask DisposeChildScopeAsync(ChildDependencyScope scope)
    {
        Assume.IsNotNull( scope.InternalParentScope );

        var children = ListSlim<ChildDependencyScope>.Create();
        WriteLockSlim parentLock = default;
        try
        {
            parentLock = WriteLockSlim.TryEnter( scope.InternalParentScope.Lock, out var entered );
            if ( ! entered || scope.InternalParentScope.IsDisposedInternal )
                return;

            using ( WriteLockSlim.TryEnter( scope.Lock, out entered ) )
            {
                if ( ! entered || scope.IsDisposedInternal )
                    return;

                scope.InternalParentScope.RemoveChild( scope );
                parentLock.Dispose();
                parentLock = default;

                MarkChildScopeRangeAsDisposed( ref children, scope );
                scope.MarkAsDisposed();
            }
        }
        finally
        {
            parentLock.Dispose();
        }

        var exceptions = Chain<OwnedDependencyDisposalException>.Empty;
        for ( var i = 0; i < children.Count; ++i )
        {
            var child = children[i];
            exceptions = exceptions.Extend( await child.FinalizeDisposalAsync().ConfigureAwait( false ) );

            if ( child.Name is not null )
                _namedScopes.RemoveScope( child.Name );

            child.Lock.DisposeGracefully();
        }

        exceptions = exceptions.Extend( await scope.FinalizeDisposalAsync().ConfigureAwait( false ) );

        if ( scope.Name is not null )
            _namedScopes.RemoveScope( scope.Name );

        scope.Lock.DisposeGracefully();

        if ( exceptions.Count > 0 )
            ExceptionThrower.Throw( new OwnedDependenciesDisposalAggregateException( scope, exceptions ) );
    }

    private static void MarkChildScopeRangeAsDisposed(ref ListSlim<ChildDependencyScope> descendants, DependencyScope parent)
    {
        Assume.True( parent.Lock.IsWriteLockHeld );

        var child = parent.FirstChild;
        while ( child is not null )
        {
            ChildDependencyScope? next;
            using ( WriteLockSlim.Enter( child.Lock ) )
            {
                Assume.False( child.IsDisposedInternal );
                MarkChildScopeRangeAsDisposed( ref descendants, child );
                child.MarkAsDisposed();
                next = child.NextSibling;
                child.PrevSibling = null;
                child.NextSibling = null;
            }

            descendants.Add( child );
            child = next;
        }
    }

    private object? TryResolveDynamicDependency(DependencyLocator locator, Type dependencyType)
    {
        if ( ! dependencyType.IsGenericType || dependencyType.GetGenericTypeDefinition() != typeof( IEnumerable<> ) )
            return null;

        using var @lock = UpgradeableReadLockSlim.TryEnter( locator.Resolvers.Lock, out var entered );
        if ( ! entered || locator.InternalAttachedScope.IsDisposedInternal )
            ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( locator.InternalAttachedScope ) ) );

        var resolver = locator.Resolvers.TryGetResolver( dependencyType );
        if ( resolver is not null )
            return resolver;

        var underlyingType = dependencyType.GetGenericArguments()[0];
        var emptyArrayMethod = ExpressionBuilder.GetClosedArrayEmptyMethod( underlyingType );
        var emptyArray = emptyArrayMethod.Invoke( null, null );
        Assume.IsNotNull( emptyArray );

        var id = GenerateResolverId();
        resolver = new EmptyRangeResolver( id, dependencyType, emptyArray );
        using ( @lock.Upgrade() )
            locator.Resolvers.AddResolver( dependencyType, resolver );

        return emptyArray;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ulong GenerateResolverId()
    {
        using ( ExclusiveLock.Enter( _idGenerator ) )
            return _idGenerator.Generate();
    }
}
