using System;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Exceptions;

public static class ExceptionThrower
{
    [DoesNotReturn]
    public static void Throw(Exception exception)
    {
        throw exception;
    }
}
