using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class MissingDependencyException : InvalidOperationException
{
    public MissingDependencyException(Type dependencyType)
        : base( Resources.MissingDependency( dependencyType ) )
    {
        DependencyType = dependencyType;
    }

    public Type DependencyType { get; }
}
