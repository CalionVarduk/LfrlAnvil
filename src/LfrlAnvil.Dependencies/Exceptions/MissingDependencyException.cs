using System;

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred due to missing dependency resolution registration.
/// </summary>
public class MissingDependencyException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MissingDependencyException"/> instance.
    /// </summary>
    /// <param name="dependencyType">Missing dependency type.</param>
    public MissingDependencyException(Type dependencyType)
        : base( Resources.MissingDependency( dependencyType ) )
    {
        DependencyType = dependencyType;
    }

    /// <summary>
    /// Missing dependency type.
    /// </summary>
    public Type DependencyType { get; }
}
