using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class DependencyTypeConfigurationException : InvalidOperationException
{
    public DependencyTypeConfigurationException(Type dependencyType, ISharedDependencyImplementorKey implementorKey, string message)
        : base( message )
    {
        DependencyType = dependencyType;
        ImplementorKey = implementorKey;
    }

    public Type DependencyType { get; }
    public ISharedDependencyImplementorKey ImplementorKey { get; }
}
