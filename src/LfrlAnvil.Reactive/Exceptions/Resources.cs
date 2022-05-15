using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Exceptions
{
    internal static class Resources
    {
        internal const string DisposedEventSource = "Event source is disposed.";
        internal const string SubscriberIsAlreadyInitialized = "Subscriber is already initialized.";

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static string InvalidArgumentType(object? argument, Type expectedType)
        {
            var argumentTypeName = argument is null ? "<null>" : argument.GetType().FullName;
            return $"Expected argument of type {expectedType.FullName} but found {argumentTypeName}.";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static string InvalidCapacity(int capacity)
        {
            return $"Expected capacity greater than 0 but found {capacity}.";
        }
    }
}
