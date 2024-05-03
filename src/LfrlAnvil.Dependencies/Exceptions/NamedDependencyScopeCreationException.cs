using System;

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred due to duplicated scope name.
/// </summary>
public class NamedDependencyScopeCreationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="NamedDependencyScopeCreationException"/> instance.
    /// </summary>
    /// <param name="parentScope">Parent scope.</param>
    /// <param name="name">Duplicated name.</param>
    public NamedDependencyScopeCreationException(IDependencyScope parentScope, string name)
        : base( Resources.NamedScopeAlreadyExists( parentScope, name ) )
    {
        ParentScope = parentScope;
        Name = name;
    }

    /// <summary>
    /// Parent scope.
    /// </summary>
    public IDependencyScope ParentScope { get; }

    /// <summary>
    /// Duplicated name.
    /// </summary>
    public string Name { get; }
}
