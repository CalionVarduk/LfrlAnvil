using System;

namespace LfrlAnvil.Mapping.Exceptions;

/// <summary>
/// Represents an error that occurred during registration of <see cref="TypeMappingConfigurationModule"/> as a sub-module.
/// </summary>
public class InvalidTypeMappingSubmoduleConfigurationException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidTypeMappingSubmoduleConfigurationException"/> instance.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="paramName">Error parameter name.</param>
    public InvalidTypeMappingSubmoduleConfigurationException(string message, string paramName)
        : base( message, paramName ) { }
}
