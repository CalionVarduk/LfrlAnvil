using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Requests.Exceptions;

internal static class Resources
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingRequestHandler(Type requestType)
    {
        var requestText = requestType.FullName;
        return $"Handler is missing for a request of {requestText} type.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidRequestType(Type requestType, Type expectedType)
    {
        var requestText = requestType.FullName;
        var expectedText = expectedType.FullName;
        return $"{requestText} is not a valid request type because it implements a request interface with {expectedText} type.";
    }
}
