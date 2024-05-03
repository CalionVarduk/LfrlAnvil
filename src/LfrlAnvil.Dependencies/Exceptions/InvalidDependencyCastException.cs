using System;

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid type cast of a resolved dependency.
/// </summary>
public class InvalidDependencyCastException : InvalidCastException
{
    /// <summary>
    /// Creates a new <see cref="InvalidDependencyCastException"/> instance.
    /// </summary>
    /// <param name="dependencyType">Expected type.</param>
    /// <param name="resultType">Resolved dependency type.</param>
    public InvalidDependencyCastException(Type dependencyType, Type resultType)
        : base( Resources.InvalidDependencyType( dependencyType, resultType ) )
    {
        DependencyType = dependencyType;
        ResultType = resultType;
    }

    /// <summary>
    /// Creates a new <see cref="InvalidDependencyCastException"/> instance.
    /// </summary>
    /// <param name="dependencyType">Expected type.</param>
    public InvalidDependencyCastException(Type dependencyType)
        : base( Resources.InvalidDependencyType( dependencyType, null ) )
    {
        DependencyType = dependencyType;
        ResultType = null;
    }

    /// <summary>
    /// Expected type.
    /// </summary>
    public Type DependencyType { get; }

    /// <summary>
    /// Resolved dependency type.
    /// </summary>
    public Type? ResultType { get; }
}
