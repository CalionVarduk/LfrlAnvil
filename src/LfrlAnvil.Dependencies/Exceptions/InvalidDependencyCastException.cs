﻿using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class InvalidDependencyCastException : InvalidCastException
{
    public InvalidDependencyCastException(Type dependencyType, Type resultType)
        : base( Resources.InvalidDependencyType( dependencyType, resultType ) )
    {
        DependencyType = dependencyType;
        ResultType = resultType;
    }

    public Type DependencyType { get; }
    public Type ResultType { get; }
}