using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class CircularDependencyReferenceException : InvalidOperationException
{
    public CircularDependencyReferenceException(
        Type dependencyType,
        Type implementorType,
        CircularDependencyReferenceException? innerException = null)
        : base( Resources.CircularDependencyReference( dependencyType, implementorType ), innerException )
    {
        DependencyType = dependencyType;
        ImplementorType = implementorType;
    }

    public Type DependencyType { get; }
    public Type ImplementorType { get; }
}
