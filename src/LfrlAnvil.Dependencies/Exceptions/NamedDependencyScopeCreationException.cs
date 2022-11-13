using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class NamedDependencyScopeCreationException : InvalidOperationException
{
    public NamedDependencyScopeCreationException(IDependencyScope parentScope, string name)
        : base( Resources.NamedScopeAlreadyExists( parentScope, name ) )
    {
        ParentScope = parentScope;
        Name = name;
    }

    public IDependencyScope ParentScope { get; }
    public string Name { get; }
}
