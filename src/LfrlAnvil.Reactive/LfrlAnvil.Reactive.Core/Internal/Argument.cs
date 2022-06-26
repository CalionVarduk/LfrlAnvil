using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Internal;

internal static class Argument
{
    [Pure]
    internal static T CastTo<T>(object? argument, string name)
    {
        if ( argument is T castArgument )
            return castArgument;

        throw new InvalidArgumentTypeException( argument, typeof( T ), name );
    }
}
