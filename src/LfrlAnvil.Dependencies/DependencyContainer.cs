using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies;

public sealed class DependencyContainer : IDisposableDependencyContainer
{
    private readonly object _sync = new object();
    private readonly IReadOnlyDictionary<Type, DependencyResolver> _resolvers;
    private readonly Dictionary<int, ChildDependencyScope> _activeScopesPerThread;

    internal DependencyContainer(IReadOnlyDictionary<Type, DependencyResolver> resolvers)
    {
        _resolvers = resolvers;
        _activeScopesPerThread = new Dictionary<int, ChildDependencyScope>();
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

            return _resolvers.TryGetValue( dependencyType, out var resolver )
                ? resolver.Create( locator.InternalAttachedScope, dependencyType )
                : null;
        }
    }

    internal ChildDependencyScope CreateChildScope(DependencyScope parentScope)
    {
        lock ( _sync )
        {
            if ( parentScope.IsDisposed )
                throw new ObjectDisposedException( Resources.ScopeIsDisposed( parentScope ) );

            var threadId = Environment.CurrentManagedThreadId;
            var activeScope = GetActiveScope( threadId );

            if ( ! ReferenceEquals( activeScope, parentScope ) )
                throw new DependencyScopeCreationException( parentScope, activeScope, threadId );

            var result = new ChildDependencyScope( this, parentScope, threadId );
            LinkChildScope( parentScope, result );
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void LinkChildScope(DependencyScope parentScope, ChildDependencyScope childScope)
    {
        Assume.IsNotNull( childScope.ThreadId, nameof( childScope.ThreadId ) );

        _activeScopesPerThread[childScope.ThreadId.Value] = childScope;

        if ( ReferenceEquals( parentScope, InternalRootScope ) )
        {
            InternalRootScope.ChildrenByThreadId.Add( childScope.ThreadId.Value, childScope );
            return;
        }

        var parentChildScope = ReinterpretCast.To<ChildDependencyScope>( parentScope );

        Assume.IsNull( parentChildScope.Child, nameof( parentChildScope.Child ) );
        Assume.IsNotNull( parentChildScope.ThreadId, nameof( parentChildScope.ThreadId ) );
        Assume.Equals( childScope.ThreadId.Value, parentChildScope.ThreadId.Value, nameof( childScope.ThreadId ) );
        parentChildScope.Child = childScope;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void UnlinkChildScope(ChildDependencyScope childScope)
    {
        Assume.IsNotNull( childScope.InternalParentScope, nameof( childScope.InternalParentScope ) );
        Assume.IsNotNull( childScope.ThreadId, nameof( childScope.ThreadId ) );

        if ( ReferenceEquals( childScope.InternalParentScope, InternalRootScope ) )
        {
            InternalRootScope.ChildrenByThreadId.Remove( childScope.ThreadId.Value );
            _activeScopesPerThread.Remove( childScope.ThreadId.Value );
            return;
        }

        var parentChildScope = ReinterpretCast.To<ChildDependencyScope>( childScope.InternalParentScope );
        parentChildScope.Child = null;

        Assume.IsNotNull( parentChildScope.ThreadId, nameof( parentChildScope.ThreadId ) );
        Assume.Equals( childScope.ThreadId.Value, parentChildScope.ThreadId.Value, nameof( childScope.ThreadId ) );
        _activeScopesPerThread[childScope.ThreadId.Value] = parentChildScope;
    }

    private void DisposeChildScope(ChildDependencyScope scope)
    {
        Assume.IsNotNull( scope.ThreadId, nameof( scope.ThreadId ) );

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

        var rootExceptions = InternalRootScope.InternalLocator.DisposeInstances();
        exceptions = exceptions.Extend( rootExceptions );
        InternalRootScope.ChildrenByThreadId.Clear();
        InternalRootScope.IsDisposed = true;

        _activeScopesPerThread.Clear();

        if ( exceptions.Count > 0 )
            throw new OwnedDependenciesDisposalAggregateException( InternalRootScope, exceptions );
    }

    private static Chain<OwnedDependencyDisposalException> DisposeChildScopeInternal(
        ChildDependencyScope scope,
        Chain<OwnedDependencyDisposalException> exceptions)
    {
        if ( scope.Child is not null )
        {
            exceptions = DisposeChildScopeInternal( scope.Child, exceptions );
            scope.Child = null;
        }

        var scopeExceptions = scope.InternalLocator.DisposeInstances();
        exceptions = exceptions.Extend( scopeExceptions );
        scope.IsDisposed = true;
        return exceptions;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private DependencyScope GetActiveScope(int threadId)
    {
        return _activeScopesPerThread.TryGetValue( threadId, out var scope ) ? scope : InternalRootScope;
    }
}
