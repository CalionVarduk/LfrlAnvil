using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class DependencyTypeConfigurationException : InvalidOperationException
{
    public DependencyTypeConfigurationException(Type dependencyType, Type implementorType, string message)
        : base( message )
    {
        DependencyType = dependencyType;
        ImplementorType = implementorType;
    }

    public Type DependencyType { get; }
    public Type ImplementorType { get; }
}
