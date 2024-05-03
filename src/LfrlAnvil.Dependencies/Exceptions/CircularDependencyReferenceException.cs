using System;

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred due to circular dependency reference detection during dependency resolution.
/// </summary>
public class CircularDependencyReferenceException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="CircularDependencyReferenceException"/> instance.
    /// </summary>
    /// <param name="dependencyType">Dependency type.</param>
    /// <param name="implementorType">Implementor type.</param>
    /// <param name="innerException">Optional inner exception.</param>
    public CircularDependencyReferenceException(
        Type dependencyType,
        Type implementorType,
        CircularDependencyReferenceException? innerException = null)
        : base( Resources.CircularDependencyReference( dependencyType, implementorType ), innerException )
    {
        DependencyType = dependencyType;
        ImplementorType = implementorType;
    }

    /// <summary>
    /// Dependency type.
    /// </summary>
    public Type DependencyType { get; }

    /// <summary>
    /// Implementor type.
    /// </summary>
    public Type ImplementorType { get; }
}
