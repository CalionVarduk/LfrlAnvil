using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Exceptions;

public static class ExceptionThrower
{
    [DoesNotReturn]
    [StackTraceHidden]
    public static void Throw(Exception exception)
    {
        throw exception;
    }
}
