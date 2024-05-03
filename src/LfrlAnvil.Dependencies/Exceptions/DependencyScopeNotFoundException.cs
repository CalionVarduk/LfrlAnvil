using System.Collections.Generic;

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred due to missing named scope.
/// </summary>
public class DependencyScopeNotFoundException : KeyNotFoundException
{
    /// <summary>
    /// Creates a new <see cref="DependencyScopeNotFoundException"/> instance.
    /// </summary>
    /// <param name="scopeName">Missing scope name.</param>
    public DependencyScopeNotFoundException(string scopeName)
        : base( Resources.MissingDependencyScope( scopeName ) )
    {
        ScopeName = scopeName;
    }

    /// <summary>
    /// Missing scope name.
    /// </summary>
    public string ScopeName { get; }
}
