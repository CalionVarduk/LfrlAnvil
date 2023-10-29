using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies;

public sealed class DependencyContainer : IDisposableDependencyContainer
{
    private readonly object _sync = new object();
    private readonly Dictionary<int, ChildDependencyScope> _activeScopesPerThread;
    private readonly Dictionary<string, ChildDependencyScope> _namedScopes;
    private readonly UlongSequenceGenerator _idGenerator;

    internal DependencyContainer(
        UlongSequenceGenerator idGenerator,
        Dictionary<Type, DependencyResolver> globalResolvers,
        KeyedDependencyResolversStore keyedResolversStore)
    {
        GlobalResolvers = globalResolvers;
        KeyedResolversStore = keyedResolversStore;
        _idGenerator = idGenerator;
        _activeScopesPerThread = new Dictionary<int, ChildDependencyScope>();
        _namedScopes = new Dictionary<string, ChildDependencyScope>();
        InternalRootScope = new RootDependencyScope( this );
    }

    public IDependencyScope RootScope => InternalRootScope;

    public IDependencyScope ActiveScope
    {
        get
        {
            lock ( _sync )
            {
                return GetActiveScope( Environment.CurrentManagedThreadId );
            }
        }
    }

    internal RootDependencyScope InternalRootScope { get; }
    internal Dictionary<Type, DependencyResolver> GlobalResolvers { get; }
    internal KeyedDependencyResolversStore KeyedResolversStore { get; }

    public void Dispose()
    {
        lock ( _sync )
        {
            if ( InternalRootScope.IsDisposed )
                return;

            DisposeRootScope();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal object? TryResolveDependency(DependencyLocator locator, Type dependencyType)
    {
        lock ( _sync )
        {
            if ( locator.InternalAttachedScope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( locator.InternalAttachedScope ) ) );

            return locator.Resolvers.TryGetValue( dependencyType, out var resolver )
                ? resolver.Create( locator.InternalAttachedScope, dependencyType )
                : TryResolveDynamicDependency( locator.Resolvers, dependencyType );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyLocator<TKey> GetOrCreateKeyedLocator<TKey>(DependencyScope scope, TKey key)
        where TKey : notnull
    {
        lock ( _sync )
        {
            if ( scope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( scope ) ) );

            return scope.InternalLocatorStore.GetOrCreate( key );
        }
    }

    internal ChildDependencyScope CreateChildScope(DependencyScope parentScope, string? name)
    {
        lock ( _sync )
        {
            if ( parentScope.IsDisposed )
                throw new ObjectDisposedException( Resources.ScopeIsDisposed( parentScope ) );

            var threadId = Environment.CurrentManagedThreadId;
            var activeScope = GetActiveScope( threadId );

            if ( ! ReferenceEquals( activeScope, parentScope ) )
                throw new DependencyScopeCreationException( parentScope, activeScope, threadId );

            if ( name is not null && _namedScopes.ContainsKey( name ) )
                throw new NamedDependencyScopeCreationException( parentScope, name );

            var result = new ChildDependencyScope( this, parentScope, threadId, name );
            LinkChildScope( parentScope, result );

            if ( name is not null )
                _namedScopes.Add( name, result );

            return result;
        }
    }

    [Pure]
    internal DependencyScope? GetScope(string name)
    {
        lock ( _sync )
        {
            return _namedScopes.TryGetValue( name, out var scope ) ? scope : null;
        }
    }

    internal bool EndScope(string name)
    {
        lock ( _sync )
        {
            if ( ! _namedScopes.TryGetValue( name, out var scope ) )
                return false;

            DisposeScopeInternal( scope );
            return true;
        }
    }

    internal void DisposeScope(DependencyScope scope)
    {
        lock ( _sync )
        {
            DisposeScopeInternal( scope );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Type[] GetResolvableTypes(Dictionary<Type, DependencyResolver> resolvers)
    {
        lock ( _sync )
        {
            Assume.ContainsAtLeast( resolvers, 1 );
            var result = new Type[resolvers.Count];
            resolvers.Keys.CopyTo( result, 0 );
            return result;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyLifetime? TryGetLifetime(Dictionary<Type, DependencyResolver> resolvers, Type dependencyType)
    {
        lock ( _sync )
        {
            return resolvers.TryGetValue( dependencyType, out var resolver ) ? resolver.Lifetime : null;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void LinkChildScope(DependencyScope parentScope, ChildDependencyScope childScope)
    {
        Assume.IsNotNull( childScope.ThreadId );

        _activeScopesPerThread[childScope.ThreadId.Value] = childScope;

        if ( ReferenceEquals( parentScope, InternalRootScope ) )
        {
            InternalRootScope.ChildrenByThreadId.Add( childScope.ThreadId.Value, childScope );
            return;
        }

        var parentChildScope = ReinterpretCast.To<ChildDependencyScope>( parentScope );

        Assume.IsNull( parentChildScope.Child );
        Assume.IsNotNull( parentChildScope.ThreadId );
        Assume.Equals( childScope.ThreadId.Value, parentChildScope.ThreadId.Value );
        parentChildScope.Child = childScope;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void UnlinkChildScope(ChildDependencyScope childScope)
    {
        Assume.IsNotNull( childScope.InternalParentScope );
        Assume.IsNotNull( childScope.ThreadId );

        if ( ReferenceEquals( childScope.InternalParentScope, InternalRootScope ) )
        {
            InternalRootScope.ChildrenByThreadId.Remove( childScope.ThreadId.Value );
            _activeScopesPerThread.Remove( childScope.ThreadId.Value );
            return;
        }

        var parentChildScope = ReinterpretCast.To<ChildDependencyScope>( childScope.InternalParentScope );
        parentChildScope.Child = null;

        Assume.IsNotNull( parentChildScope.ThreadId );
        Assume.Equals( childScope.ThreadId.Value, parentChildScope.ThreadId.Value );
        _activeScopesPerThread[childScope.ThreadId.Value] = parentChildScope;
    }

    private void DisposeScopeInternal(DependencyScope scope)
    {
        if ( scope.IsDisposed )
            return;

        if ( ReferenceEquals( scope, InternalRootScope ) )
            DisposeRootScope();
        else
            DisposeChildScope( ReinterpretCast.To<ChildDependencyScope>( scope ) );
    }

    private void DisposeChildScope(ChildDependencyScope scope)
    {
        Assume.IsNotNull( scope.ThreadId );

        var threadId = Environment.CurrentManagedThreadId;
        if ( threadId != scope.ThreadId.Value )
            throw new DependencyScopeDisposalException( scope, threadId );

        var exceptions = DisposeChildScopeInternal( scope, Chain<OwnedDependencyDisposalException>.Empty );
        UnlinkChildScope( scope );

        if ( exceptions.Count > 0 )
            throw new OwnedDependenciesDisposalAggregateException( scope, exceptions );
    }

    private void DisposeRootScope()
    {
        var exceptions = Chain<OwnedDependencyDisposalException>.Empty;

        foreach ( var childScope in InternalRootScope.ChildrenByThreadId.Values )
        {
            var childExceptions = DisposeChildScopeInternal( childScope, Chain<OwnedDependencyDisposalException>.Empty );
            if ( childExceptions.Count > 0 )
                exceptions = exceptions.Extend( childExceptions );
        }

        var rootExceptions = InternalRootScope.DisposeInstances();
        exceptions = exceptions.Extend( rootExceptions );
        InternalRootScope.ChildrenByThreadId.Clear();
        InternalRootScope.IsDisposed = true;

        _activeScopesPerThread.Clear();

        if ( exceptions.Count > 0 )
            throw new OwnedDependenciesDisposalAggregateException( InternalRootScope, exceptions );
    }

    private Chain<OwnedDependencyDisposalException> DisposeChildScopeInternal(
        ChildDependencyScope scope,
        Chain<OwnedDependencyDisposalException> exceptions)
    {
        if ( scope.Child is not null )
        {
            exceptions = DisposeChildScopeInternal( scope.Child, exceptions );
            scope.Child = null;
        }

        var scopeExceptions = scope.DisposeInstances();
        exceptions = exceptions.Extend( scopeExceptions );
        scope.IsDisposed = true;

        if ( scope.Name is not null )
            _namedScopes.Remove( scope.Name );

        return exceptions;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private DependencyScope GetActiveScope(int threadId)
    {
        return _activeScopesPerThread.TryGetValue( threadId, out var scope ) ? scope : InternalRootScope;
    }

    private object? TryResolveDynamicDependency(Dictionary<Type, DependencyResolver> resolvers, Type dependencyType)
    {
        if ( dependencyType.IsGenericType && dependencyType.GetGenericTypeDefinition() == typeof( IEnumerable<> ) )
        {
            var underlyingType = dependencyType.GetGenericArguments()[0];
            var emptyArrayMethod = ExpressionBuilder.GetClosedArrayEmptyMethod( underlyingType );
            var emptyArray = emptyArrayMethod.Invoke( null, null );
            Assume.IsNotNull( emptyArray );

            var resolver = new TransientDependencyResolver(
                id: _idGenerator.Generate(),
                implementorType: dependencyType,
                disposalStrategy: DependencyImplementorDisposalStrategy.RenounceOwnership(),
                onResolvingCallback: null,
                factory: _ => emptyArray );

            resolvers.Add( dependencyType, resolver );
            return emptyArray;
        }

        return null;
    }
}
