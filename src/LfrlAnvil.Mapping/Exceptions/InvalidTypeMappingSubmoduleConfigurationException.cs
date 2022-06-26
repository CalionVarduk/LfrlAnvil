using System;

namespace LfrlAnvil.Mapping.Exceptions;

public class InvalidTypeMappingSubmoduleConfigurationException : ArgumentException
{
    public InvalidTypeMappingSubmoduleConfigurationException(string message, string paramName)
        : base( message, paramName ) { }
}
