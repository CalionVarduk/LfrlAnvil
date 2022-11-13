using System;
using System.Diagnostics.Contracts;

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
        InternalLocator = new DependencyLocator( this );
    }

    public string? Name { get; }
    public int? ThreadId { get; }
    public bool IsDisposed { get; internal set; }
    public IDependencyContainer Container => InternalContainer;
    public IDependencyScope? ParentScope => InternalParentScope;
    public IDependencyLocator Locator => InternalLocator;
    public bool IsActive => ReferenceEquals( InternalContainer.ActiveScope, this );
    public bool IsRoot => InternalParentScope is null;
    public int Level => InternalParentScope is null ? 0 : InternalParentScope.Level + 1;
    internal DependencyContainer InternalContainer { get; }
    internal DependencyScope? InternalParentScope { get; }
    internal DependencyLocator InternalLocator { get; }

    public void Dispose()
    {
        InternalContainer.DisposeScope( this );
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
