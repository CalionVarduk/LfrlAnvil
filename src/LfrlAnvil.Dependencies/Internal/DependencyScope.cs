using System;

namespace LfrlAnvil.Dependencies.Internal;

internal class DependencyScope : IDependencyScope, IDisposable
{
    protected DependencyScope(DependencyContainer container, DependencyScope? parentScope, int? threadId)
    {
        ThreadId = threadId;
        InternalContainer = container;
        InternalParentScope = parentScope;
        IsDisposed = false;
        InternalLocator = new DependencyLocator( this );
    }

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

    public ChildDependencyScope BeginScope()
    {
        return InternalContainer.CreateChildScope( this );
    }

    IChildDependencyScope IDependencyScope.BeginScope()
    {
        return BeginScope();
    }
}
