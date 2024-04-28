using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Exceptions;

/// <summary>
/// Helper class for throwing exceptions.
/// </summary>
public static class ExceptionThrower
{
    /// <summary>
    /// Throws the provided <paramref name="exception"/>.
    /// </summary>
    /// <param name="exception">Exception to throw.</param>
    [DoesNotReturn]
    [StackTraceHidden]
    public static void Throw(Exception exception)
    {
        throw exception;
    }
}
