using System.Collections.Generic;

namespace LfrlAnvil.Dependencies.Exceptions;

public class DependencyScopeNotFoundException : KeyNotFoundException
{
    public DependencyScopeNotFoundException(string scopeName)
        : base( Resources.MissingDependencyScope( scopeName ) )
    {
        ScopeName = scopeName;
    }

    public string ScopeName { get; }
}
