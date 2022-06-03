using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Exceptions
{
    internal static class Resources
    {
        internal const string DisposedEventSource = "Event source is disposed.";

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static string InvalidArgumentType(object? argument, Type expectedType)
        {
            var argumentTypeName = argument is null ? "<null>" : argument.GetType().FullName;
            return $"Expected argument of type {expectedType.FullName} but found {argumentTypeName}.";
        }
    }
}
