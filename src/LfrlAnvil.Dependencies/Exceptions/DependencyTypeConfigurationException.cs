﻿using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class DependencyTypeConfigurationException : InvalidOperationException
{
    public DependencyTypeConfigurationException(Type dependencyType, IDependencyImplementorKey implementorKey, string message)
        : base( message )
    {
        DependencyType = dependencyType;
        ImplementorKey = implementorKey;
    }

    public Type DependencyType { get; }
    public IDependencyImplementorKey ImplementorKey { get; }
}