using System;

namespace LfrlAnvil.Dependencies.Exceptions;

public class InvalidInjectablePropertyTypeException : ArgumentException
{
    public InvalidInjectablePropertyTypeException(Type type, string paramName)
        : base( Resources.InvalidInjectablePropertyType( type ), paramName ) { }
}
