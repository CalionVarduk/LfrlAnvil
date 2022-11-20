using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Internal;

internal class DependencyScope : IDependencyScope, IDisposable
{
    protected DependencyScope(DependencyContainer container, DependencyScope? parentScope, int? threadId, string? name)
    {
        ThreadId = threadId;
        Name = name;
        InternalContainer = container;
        InternalParentScope = parentScope;
        IsDisposed = false;
        InternalDisposers = new List<DependencyDisposer>();
        ScopedInstancesByResolverId = new Dictionary<ulong, object>();
        InternalLocatorStore = DependencyLocatorStore.Create( container.KeyedResolversStore, container.GlobalResolvers, this );
    }

    public string? Name { get; }
    public int? ThreadId { get; }
    public bool IsDisposed { get; internal set; }
    public IDependencyContainer Container => InternalContainer;
    public IDependencyScope? ParentScope => InternalParentScope;
    public IDependencyLocator Locator => InternalLocatorStore.Global;
    public bool IsActive => ReferenceEquals( InternalContainer.ActiveScope, this );
    public bool IsRoot => InternalParentScope is null;
    public int Level => InternalParentScope is null ? 0 : InternalParentScope.Level + 1;
    internal DependencyContainer InternalContainer { get; }
    internal DependencyScope? InternalParentScope { get; }
    internal DependencyLocatorStore InternalLocatorStore { get; }
    internal List<DependencyDisposer> InternalDisposers { get; }
    internal Dictionary<ulong, object> ScopedInstancesByResolverId { get; }

    public void Dispose()
    {
        InternalContainer.DisposeScope( this );
    }

    internal DependencyLocator<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull
    {
        return InternalContainer.GetOrCreateKeyedLocator( this, key );
    }

    internal ChildDependencyScope BeginScope(string? name = null)
    {
        return InternalContainer.CreateChildScope( this, name );
    }

    [Pure]
    internal DependencyScope? UseScope(string name)
    {
        return InternalContainer.GetScope( name );
    }

    public bool EndScope(string name)
    {
        return InternalContainer.EndScope( name );
    }

    internal Chain<OwnedDependencyDisposalException> DisposeInstances()
    {
        var exceptions = Chain<OwnedDependencyDisposalException>.Empty;

        foreach ( var disposer in InternalDisposers )
        {
            var exception = disposer.TryDispose();
            if ( exception is not null )
                exceptions = exceptions.Extend( new OwnedDependencyDisposalException( this, exception ) );
        }

        InternalDisposers.Clear();
        ScopedInstancesByResolverId.Clear();
        InternalLocatorStore.Clear();
        return exceptions;
    }

    IDependencyLocator<TKey> IDependencyScope.GetKeyedLocator<TKey>(TKey key)
    {
        return GetKeyedLocator( key );
    }

    IChildDependencyScope IDependencyScope.BeginScope(string? name)
    {
        return BeginScope( name );
    }

    [Pure]
    IDependencyScope? IDependencyScope.UseScope(string name)
    {
        return UseScope( name );
    }
}
