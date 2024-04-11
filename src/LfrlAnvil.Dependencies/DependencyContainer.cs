using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies;

public sealed class DependencyContainer : IDisposableDependencyContainer
{
    private readonly object _sync = new object();
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
        _namedScopes = new Dictionary<string, ChildDependencyScope>();
        InternalRootScope = new RootDependencyScope( this );
    }

    public IDependencyScope RootScope => InternalRootScope;
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

    [Pure]
    public IDependencyScope? TryGetScope(string name)
    {
        lock ( _sync )
        {
            return _namedScopes.TryGetValue( name, out var scope ) ? scope : null;
        }
    }

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

            ChildDependencyScope result;
            if ( name is null )
                result = new ChildDependencyScope( this, parentScope, name );
            else
            {
                ref var namedScope = ref CollectionsMarshal.GetValueRefOrAddDefault( _namedScopes, name, out var exists );
                if ( exists )
                    throw new NamedDependencyScopeCreationException( parentScope, name );

                result = new ChildDependencyScope( this, parentScope, name );
                namedScope = result;
            }

            return result;
        }
    }

    internal void DisposeScope(DependencyScope scope)
    {
        lock ( _sync )
        {
            if ( scope.IsDisposed )
                return;

            if ( ReferenceEquals( scope, InternalRootScope ) )
                DisposeRootScope();
            else
                DisposeChildScope( ReinterpretCast.To<ChildDependencyScope>( scope ) );
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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal IDependencyScope[] GetScopeChildren(DependencyScope scope)
    {
        lock ( _sync )
        {
            var node = scope.FirstChild;
            if ( node is null )
                return Array.Empty<IDependencyScope>();

            var result = new List<IDependencyScope>();
            do
            {
                result.Add( node );
                node = node.NextSibling;
            }
            while ( node is not null );

            return result.ToArray();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void DisposeChildScope(ChildDependencyScope scope)
    {
        var exceptions = DisposeChildScopeCore( scope, Chain<OwnedDependencyDisposalException>.Empty );

        Assume.IsNotNull( scope.InternalParentScope );
        scope.InternalParentScope.RemoveChild( scope );

        if ( exceptions.Count > 0 )
            ExceptionThrower.Throw( new OwnedDependenciesDisposalAggregateException( scope, exceptions ) );
    }

    private void DisposeRootScope()
    {
        var exceptions = DisposeChildScopeRange( InternalRootScope, Chain<OwnedDependencyDisposalException>.Empty );
        var rootExceptions = InternalRootScope.DisposeInstances();
        exceptions = exceptions.Extend( rootExceptions );
        InternalRootScope.MarkAsDisposed();

        if ( exceptions.Count > 0 )
            ExceptionThrower.Throw( new OwnedDependenciesDisposalAggregateException( InternalRootScope, exceptions ) );
    }

    private Chain<OwnedDependencyDisposalException> DisposeChildScopeCore(
        ChildDependencyScope scope,
        Chain<OwnedDependencyDisposalException> exceptions)
    {
        exceptions = DisposeChildScopeRange( scope, exceptions );
        var scopeExceptions = scope.DisposeInstances();
        exceptions = exceptions.Extend( scopeExceptions );
        scope.MarkAsDisposed();

        if ( scope.Name is not null )
            _namedScopes.Remove( scope.Name );

        return exceptions;
    }

    private Chain<OwnedDependencyDisposalException> DisposeChildScopeRange(
        DependencyScope parent,
        Chain<OwnedDependencyDisposalException> exceptions)
    {
        var child = parent.FirstChild;
        while ( child is not null )
        {
            exceptions = DisposeChildScopeCore( child, exceptions );
            var next = child.NextSibling;
            child.PrevSibling = null;
            child.NextSibling = null;
            child = next;
        }

        return exceptions;
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
