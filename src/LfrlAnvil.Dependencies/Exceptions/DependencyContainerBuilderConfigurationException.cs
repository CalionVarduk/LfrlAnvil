using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class DependencyContainerBuilderConfigurationException : ArgumentException
{
    public DependencyContainerBuilderConfigurationException(string message, string paramName)
        : base( message, paramName ) { }
}
