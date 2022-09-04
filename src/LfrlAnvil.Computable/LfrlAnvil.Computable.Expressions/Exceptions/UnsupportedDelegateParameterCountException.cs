using System;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class UnsupportedDelegateParameterCountException : InvalidOperationException
{
    public UnsupportedDelegateParameterCountException(int parameterCount)
        : base( Resources.UnsupportedDelegateParameterCount( parameterCount ) )
    {
        ParameterCount = parameterCount;
    }

    public int ParameterCount { get; }
}
