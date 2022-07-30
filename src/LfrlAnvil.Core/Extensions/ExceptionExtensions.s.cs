using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace LfrlAnvil.Extensions;

public static class ExceptionExtensions
{
    [DoesNotReturn]
    [StackTraceHidden]
    public static void Rethrow(this Exception exception)
    {
        ExceptionDispatchInfo.Throw( exception );
    }
}
