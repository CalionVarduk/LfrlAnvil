using System;

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred during an invalid <see cref="IDependencyContainerConfigurationBuilder"/> change.
/// </summary>
public class DependencyContainerBuilderConfigurationException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="DependencyContainerBuilderConfigurationException"/> instance.
    /// </summary>
    /// <param name="message">Exception's message.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public DependencyContainerBuilderConfigurationException(string message, string paramName)
        : base( message, paramName ) { }
}
