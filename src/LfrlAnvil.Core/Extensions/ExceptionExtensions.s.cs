using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace LfrlAnvil.Extensions;

public static class ExceptionExtensions
{
    [DoesNotReturn]
    [StackTraceHidden]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Rethrow(this Exception exception)
    {
        ExceptionDispatchInfo.Throw( exception );
    }
}
