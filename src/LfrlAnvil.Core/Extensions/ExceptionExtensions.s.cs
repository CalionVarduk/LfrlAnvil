using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Exception"/> extension methods.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Rethrows the provided <paramref name="exception"/>.
    /// </summary>
    /// <param name="exception">Exception to rethrow.</param>
    /// <returns>This method does not return.</returns>
    /// <remarks>See <see cref="ExceptionDispatchInfo.Throw(Exception)"/> for more information.</remarks>
    [DoesNotReturn]
    [StackTraceHidden]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Exception Rethrow(this Exception exception)
    {
        ExceptionDispatchInfo.Throw( exception );
        return exception;
    }
}
